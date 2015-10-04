using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace shaderstuff {
    public struct VertexPositionColorNormal {
        public Vector3 Position;
        public Vector3 Normal;
        public Color Color;

        public VertexPositionColorNormal(Vector3 position, Vector3 normal, Color color) {
            this.Position = position;
            this.Normal = normal;
            this.Color = color;
        }

        public static VertexDeclaration VertexDeclaration = new VertexDeclaration(
            new VertexElement(0, VertexElementFormat.Vector3, VertexElementUsage.Position, 0),
            new VertexElement(12, VertexElementFormat.Vector3, VertexElementUsage.Normal, 0),
            new VertexElement(24, VertexElementFormat.Color, VertexElementUsage.Color, 0));
    }
    public class Node {
        public World world;
        public Node[] Children;
        public Node Parent;
        public VertexPositionColorNormal[] Verticies;
        public VertexPositionColorNormal[] VertexBuffer;
        public int[] Indicies;
        public bool IsSplit = false;
        public int Level = 0;
        public BoundingBox bbox;

        public Node(VertexPositionColorNormal v0, VertexPositionColorNormal v1, VertexPositionColorNormal v2, VertexPositionColorNormal v3, World wld, Node parent = null) {
            Children = new Node[4];
            Verticies = new VertexPositionColorNormal[4] { v0, v1, v2, v3 };

            Vector3 n1 = Vector3.Cross(v3.Position - v1.Position, v0.Position - v1.Position);
            Vector3 n2 = Vector3.Cross(v0.Position - v2.Position, v3.Position - v2.Position);
            VertexBuffer = new VertexPositionColorNormal[] { v0, v1, v3, v0, v3, v2};
            VertexBuffer[0].Normal = n1;
            VertexBuffer[1].Normal = n1;
            VertexBuffer[2].Normal = n1;
            VertexBuffer[3].Normal = n2;
            VertexBuffer[4].Normal = n2;
            VertexBuffer[5].Normal = n2;

            world = wld;
            Parent = parent;

            bbox = new BoundingBox(v0.Position, v3.Position);
        }

        public void splitIfIntersect(BoundingSphere sphere, int deepestLevel) {
            if (bbox.Intersects(sphere)) {
                if (IsSplit) {
                    for (int i = 0; i < Children.Length; i++)
                        if (Children[i] != null && Children[i].Level < deepestLevel)
                            Children[i].splitIfIntersect(sphere, deepestLevel);
                } else
                    Split();
            } else {
                Unsplit();
            }
        }

        public void Unsplit() {
            Children = new Node[4];
            IsSplit = false;
        }

        public void UnsplitLowest() {
            if (IsSplit) {
                if (!Children[0].IsSplit && !Children[1].IsSplit && !Children[2].IsSplit && !Children[3].IsSplit) {
                    Children = new Node[4];
                    IsSplit = false;
                }
                else {
                    Children[0].UnsplitLowest();
                    Children[1].UnsplitLowest();
                    Children[2].UnsplitLowest();
                    Children[3].UnsplitLowest();
                }
            }
        }

        public void Split() {
            if (IsSplit) {
                Children[0].Split();
                Children[1].Split();
                Children[2].Split();
                Children[3].Split();
            }
            else {
                VertexPositionColorNormal center = new VertexPositionColorNormal(
                    (Verticies[0].Position + Verticies[1].Position + Verticies[2].Position + Verticies[3].Position) / 4f,
                    Vector3.Up,
                    Verticies[0].Color);
                center.Position.Y = world.getHeight(center.Position.X, center.Position.Z);

                VertexPositionColorNormal top = new VertexPositionColorNormal(
                    (Verticies[0].Position + Verticies[1].Position) / 2f,
                    Vector3.Up,
                    Verticies[0].Color);
                top.Position.Y = world.getHeight(top.Position.X, top.Position.Z);

                VertexPositionColorNormal bottom = new VertexPositionColorNormal(
                    (Verticies[2].Position + Verticies[3].Position) / 2f,
                    Vector3.Up,
                    Verticies[0].Color);
                bottom.Position.Y = world.getHeight(bottom.Position.X, bottom.Position.Z);

                VertexPositionColorNormal left = new VertexPositionColorNormal(
                     (Verticies[0].Position + Verticies[2].Position) / 2f,
                    Vector3.Up,
                    Verticies[0].Color);
                left.Position.Y = world.getHeight(left.Position.X, left.Position.Z);

                VertexPositionColorNormal right = new VertexPositionColorNormal(
                    (Verticies[1].Position + Verticies[3].Position) / 2f,
                    Vector3.Up,
                    Verticies[0].Color);
                right.Position.Y = world.getHeight(right.Position.X, right.Position.Z);

                Vector3 norm0 = Vector3.Cross(left.Position - Verticies[0].Position, top.Position - Verticies[0].Position);
                Vector3 norm1 = Vector3.Cross(right.Position - Verticies[1].Position, top.Position - Verticies[1].Position);
                Vector3 norm2 = Vector3.Cross(left.Position - Verticies[2].Position, bottom.Position - Verticies[2].Position);
                Vector3 norm3 = Vector3.Cross(right.Position - Verticies[3].Position, bottom.Position - Verticies[3].Position);

                Children[0] = new Node(Verticies[0], top, left, center, world, this);
                Children[0].Verticies[0].Normal = norm0;
                Children[0].Level = Level + 1;

                Children[1] = new Node(top, Verticies[1], center, right, world, this);
                Children[1].Verticies[1].Normal = norm1;
                Children[1].Level = Level + 1;

                Children[2] = new Node(left, center, Verticies[2], bottom, world, this);
                Children[2].Verticies[2].Normal = norm2;
                Children[2].Level = Level + 1;

                Children[3] = new Node(center, right, bottom, Verticies[3], world, this);
                Children[3].Verticies[3].Normal = norm3;
                Children[3].Level = Level + 1;

                IsSplit = true;

            }
        }

        public void Render(GraphicsDevice device, Effect effect) {
            if (IsSplit) {
                Children[0].Render(device, effect);
                Children[1].Render(device, effect);
                Children[2].Render(device, effect);
                Children[3].Render(device, effect);
            }
            else {
                foreach (EffectPass p in effect.CurrentTechnique.Passes) {
                    p.Apply();
                    //device.DrawUserIndexedPrimitives<VertexPositionColorNormal>(PrimitiveType.TriangleStrip, Verticies, 0, 4, Indicies, 0, 2, VertexPositionColorNormal.VertexDeclaration);
                    device.DrawUserPrimitives<VertexPositionColorNormal>(PrimitiveType.TriangleList, VertexBuffer, 0, 2, VertexPositionColorNormal.VertexDeclaration);
                }
            }
        }
    }
}
