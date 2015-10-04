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
        public VertexPositionNormalTexture[] Verticies;
        public VertexPositionColor[] Normals;
        public bool IsSplit = false;
        public int Level = 0;
        public int Index;
        public BoundingSphere bsphere;

        public Node(VertexPositionNormalTexture v0, VertexPositionNormalTexture v1, VertexPositionNormalTexture v2, VertexPositionNormalTexture v3, World wld, Node parent = null) {
            Children = new Node[4];

            world = wld;
            Parent = parent;

            /*
            Vector3 n1 = -Vector3.Cross(v3.Position - v1.Position, v0.Position - v1.Position);
            Vector3 n2 = -Vector3.Cross(v0.Position - v2.Position, v3.Position - v2.Position);
            n1.Normalize();
            n2.Normalize();

            v0.Normal = (n1 + n2) / 2f;
            v1.Normal = n1;
            v2.Normal = n2;
            v3.Normal = (n1 + n2) / 2f;*/
            Verticies = new VertexPositionNormalTexture[4] { v0, v1, v2, v3 };
            
            float d = v1.Position.X - v0.Position.X;
            Vector3[] offsets1 = new Vector3[] { new Vector3(0, 0, -1), new Vector3(-1, 0, -1), new Vector3(-1, 0, 0), new Vector3(1, 0, 0), new Vector3(1, 0, 1), new Vector3(0, 0, 1) };
            Vector3[] offsets2 = new Vector3[] { new Vector3(-1, 0, -1), new Vector3(-1, 0, 0), new Vector3(0, 0, 1), new Vector3(0, 0, -1), new Vector3(1, 0, 0), new Vector3(1, 0, 1) };
            for (int i = 0; i < Verticies.Length; i++) {
                Verticies[i].Normal = Vector3.Zero;
                 Vector3 p = Verticies[i].Position;
                for (int j = 0; j < offsets1.Length; j++) {
                    Vector3 n = Vector3.Cross(
                        offsets1[j] * d + world.getHeight(p + offsets1[j] * d),
                        offsets2[j] * d + world.getHeight(p + offsets2[j] * d));
                    n.Normalize();
                    Verticies[i].Normal += n;
                }
                Verticies[i].Normal.Normalize();
            }

            Normals = new VertexPositionColor[] {
                new VertexPositionColor(v0.Position, Color.Red),
                new VertexPositionColor(v0.Position + v0.Normal, Color.Blue),
                new VertexPositionColor(v1.Position, Color.Red),
                new VertexPositionColor(v1.Position + v1.Normal, Color.Blue),
                new VertexPositionColor(v2.Position, Color.Red),
                new VertexPositionColor(v2.Position + v2.Normal, Color.Blue),
                new VertexPositionColor(v3.Position, Color.Red),
                new VertexPositionColor(v3.Position + v3.Normal, Color.Blue)
            };
            
            bsphere = new BoundingSphere((v0.Position + v1.Position + v2.Position + v3.Position) / 4f, 1f);
            bsphere.Radius = Vector3.DistanceSquared(bsphere.Center, v0.Position) * 10;
        }

        public void splitIfIntersect(Vector3 point, int deepestLevel) {
            if (bsphere.Contains(point) == ContainmentType.Contains) {
                Split(false);
                for (int i = 0; i < Children.Length; i++)
                    if (Children[i] != null && Children[i].Level < deepestLevel)
                        Children[i].splitIfIntersect(point, deepestLevel);
            }
            else {
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

        public void Split(bool splitchildren = true) {
            if (IsSplit) {
                if (splitchildren) {
                    Children[0].Split();
                    Children[1].Split();
                    Children[2].Split();
                    Children[3].Split();
                }
            }
            else {
                VertexPositionNormalTexture center = new VertexPositionNormalTexture((Verticies[0].Position + Verticies[1].Position + Verticies[2].Position + Verticies[3].Position) / 4f, Vector3.Zero, (Verticies[0].TextureCoordinate + Verticies[3].TextureCoordinate) / 2f);
                center.Position.Y = world.getHeight(center.Position.X, center.Position.Z);

                VertexPositionNormalTexture top = new VertexPositionNormalTexture((Verticies[0].Position + Verticies[1].Position) / 2f, Vector3.Zero, (Verticies[0].TextureCoordinate + Verticies[1].TextureCoordinate) / 2f);
                VertexPositionNormalTexture bottom = new VertexPositionNormalTexture((Verticies[2].Position + Verticies[3].Position) / 2f, Vector3.Zero, (Verticies[2].TextureCoordinate + Verticies[3].TextureCoordinate) / 2f);
                VertexPositionNormalTexture left = new VertexPositionNormalTexture((Verticies[0].Position + Verticies[2].Position) / 2f, Vector3.Zero, (Verticies[0].TextureCoordinate + Verticies[2].TextureCoordinate) / 2f);
                VertexPositionNormalTexture right = new VertexPositionNormalTexture((Verticies[1].Position + Verticies[3].Position) / 2f, Vector3.Zero, (Verticies[1].TextureCoordinate + Verticies[3].TextureCoordinate) / 2f);

                byte displace = 0;
                displace |= 1; displace |= 2; displace |= 4; displace |= 8;
                if (true){//(displace & 1) == 1){ // displace top
                    float y = world.getHeight(top.Position.X, top.Position.Z);
                    top.Position.Y = y;
                }
                if (true){//((displace & 2) == 1) { // displace bottom
                    float y = world.getHeight(bottom.Position.X, bottom.Position.Z);
                    bottom.Position.Y = y;
                }
                if (true) {//((displace & 4) == 1) { // displace left
                    float y = world.getHeight(left.Position.X, left.Position.Z);
                    left.Position.Y = y;
                }
                if (true) {//((displace & 8) == 1) { // displace right
                    float y = world.getHeight(right.Position.X, right.Position.Z);
                    right.Position.Y = y;
                }

                Children[0] = new Node(Verticies[0], top, left, center, world, this);
                Children[0].Level = Level + 1;
                Children[0].Index = 0;

                Children[1] = new Node(top, Verticies[1], center, right, world, this);
                Children[1].Level = Level + 1;
                Children[1].Index = 1;

                Children[2] = new Node(left, center, Verticies[2], bottom, world, this);
                Children[2].Level = Level + 1;
                Children[2].Index = 2;

                Children[3] = new Node(center, right, bottom, Verticies[3], world, this);
                Children[3].Level = Level + 1;
                Children[3].Index = 3;

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
                    device.DrawUserPrimitives<VertexPositionNormalTexture>(PrimitiveType.TriangleStrip, Verticies, 0, 2, VertexPositionNormalTexture.VertexDeclaration);
                }
                /*
                if (effect.CurrentTechnique.Name == "Lambert") {
                    effect.CurrentTechnique = effect.Techniques["Light"];
                    foreach (EffectPass p in effect.CurrentTechnique.Passes) {
                        p.Apply();
                        device.DrawUserPrimitives<VertexPositionColor>(PrimitiveType.LineList, Normals, 0, 4);
                    }
                    effect.CurrentTechnique = effect.Techniques["Lambert"];
                }*/
            }
        }
    }
}
