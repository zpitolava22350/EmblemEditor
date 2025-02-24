using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;

namespace EmblemEditorCPU {
    internal class Circle {

        private static Random rnd = new Random();

        public float x { get; private set; }
        public float y { get; private set; }
        public float width { get; private set; }
        public float height { get; private set; }
        public Color color { get; private set; }

        public Circle() : this(rnd.NextSingle()*Candidate.size, rnd.NextSingle() * Candidate.size, rnd.NextSingle() * Candidate.size, rnd.NextSingle() * Candidate.size, Color.FromArgb(255, rnd.Next(255), rnd.Next(255), rnd.Next(255))) {

        }

        public Circle(float xPos, float yPos, float w, float h, Color c) {
            x = xPos;
            y = yPos;
            width = w;
            height = h;
            color = c;
        }

        public Circle(Circle copy) : this(copy.x, copy.y, copy.width, copy.height, copy.color) {

        }

        public Color? GetColor(float checkX, float checkY) {
            float normX = (checkX - x) / (width / 2);
            float normY = (checkY - y) / (height / 2);

            return (normX * normX + normY * normY) <= 1 ? color : null;
        }


    }
}
