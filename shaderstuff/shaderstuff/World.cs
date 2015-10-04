using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace shaderstuff {
    public class World {
        public Node[] Nodes;
        public Vector3 Position;

        public World() {
            Nodes = new Node[16];
            int sc = 100;
            for (int x = 0; x < sc * 4; x += sc) {
                for (int y = 0; y < sc * 4; y += sc) {
                    Nodes[x / sc + y / sc * 4] = new Node(
                        new VertexPositionColorNormal(new Vector3(x, getHeight(x, y), y), Vector3.Zero, Color.ForestGreen),
                        new VertexPositionColorNormal(new Vector3(x + sc, getHeight(x + sc, y), y), Vector3.Zero, Color.ForestGreen),
                        new VertexPositionColorNormal(new Vector3(x, getHeight(x, y + sc), y + sc), Vector3.Zero, Color.ForestGreen),
                        new VertexPositionColorNormal(new Vector3(x + sc, getHeight(x + sc, y + sc), y + sc), Vector3.Zero, Color.ForestGreen), this);
                }
            }
            Position = new Vector3(-200, 0, -200);
        }

        public float getHeight(float x, float z) {
            return SimplexNoise.Noise.Generate(x / 20f, z / 20f) * 3f;
        }

        public void Render(GraphicsDevice device, Effect effect) {
            for (int i = 0; i < Nodes.Length; i++) {
                if (Nodes[i] != null) {
                    Nodes[i].Render(device, effect);
                }
            }
        }
    }
}
