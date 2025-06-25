using OpenTK.Mathematics;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EmblemEditor {
    internal class Shape {

        static Random rnd = new Random();
        static float MaxSize = 1f;

        public enum ShapeType {
            Square = 0
        }

        public Vector2 Position { get; private set; }
        public Vector2 Size { get; private set; }
        public float Rotation { get; private set; }
        public Vector4 Color { get; private set; }
        public ShapeType Type { get; private set; }

        public Shape() {
            Position = new Vector2((rnd.NextSingle() * 2f) - 1f, (rnd.NextSingle() * 2f) - 1f);
            Size = new Vector2(rnd.NextSingle() * MaxSize, rnd.NextSingle() * MaxSize);
            //Rotation = rnd.NextSingle() * MathHelper.Pi * 2f;
            Color = new Vector4(rnd.NextSingle(), rnd.NextSingle(), rnd.NextSingle(), rnd.NextSingle());
            Type = ShapeType.Square;
        }

        public List<float> BoundingBox() {

            return new List<float>{
                Position.X + (Size.X / 2f), Position.Y + (Size.Y / 2f), 0f, Color.X, Color.Y, Color.Z, Color.W, // Top Right
                Position.X + (Size.X / 2f), Position.Y - (Size.Y / 2f), 0f, Color.X, Color.Y, Color.Z, Color.W, // Bottom Right
                Position.X - (Size.X / 2f), Position.Y + (Size.Y / 2f), 0f, Color.X, Color.Y, Color.Z, Color.W, // Top Left

                Position.X + (Size.X / 2f), Position.Y - (Size.Y / 2f), 0f, Color.X, Color.Y, Color.Z, Color.W, // Bottom Right
                Position.X - (Size.X / 2f), Position.Y - (Size.Y / 2f), 0f, Color.X, Color.Y, Color.Z, Color.W, // Bottom Left
                Position.X - (Size.X / 2f), Position.Y + (Size.Y / 2f), 0f, Color.X, Color.Y, Color.Z, Color.W  // Top Left
            };
        }

        public override string ToString() {
            return $"Pos: ({Position.X}, {Position.Y}), Size: ({Size.X}, {Size.Y}), Rot: {Rotation}, Color: ({Color.X}, {Color.Y}, {Color.Z}, {Color.W})";
        }

    }
}
