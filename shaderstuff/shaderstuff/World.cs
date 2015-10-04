using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace shaderstuff {
    public class World {
        public Node Node;
        public Vector3 Position;
        float Size;

        public World(float size = 128) {
            float texScale = 20;
            Size = size;
            Node = new Node(
                new VertexPositionNormalTexture(new Vector3(0, getHeight(0, 0), 0), Vector3.Zero, Vector2.Zero),
                new VertexPositionNormalTexture(new Vector3(Size, getHeight(Size, 0), 0), Vector3.Zero, Vector2.UnitX * texScale),
                new VertexPositionNormalTexture(new Vector3(0, getHeight(0, Size), Size), Vector3.Zero, Vector2.UnitY * texScale),
                new VertexPositionNormalTexture(new Vector3(Size, getHeight(Size, Size), Size), Vector3.Zero, Vector2.One * texScale), this);
            Node.Index = -1;
            Position = new Vector3(-Size, 0, -Size) / 2f;
        }

        public Vector3 getHeight(Vector3 pos) {
            return Vector3.Up * getHeight(pos.X, pos.Z);
        }
        public float getHeight(Vector2 pos) {
            return getHeight(pos.X, pos.Y);
        }
        public float getHeight(float x, float z) {
            return SimplexNoise.Noise.Generate(x / 20f, z / 20f) * 3f;
        }

        public void Render(GraphicsDevice device, Effect effect) {
                if (Node != null)
                    Node.Render(device, effect);
        }
    }
}
