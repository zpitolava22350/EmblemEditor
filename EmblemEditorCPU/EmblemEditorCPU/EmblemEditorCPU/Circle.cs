using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;
using System.Diagnostics;

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

        public Circle(Circle copy, float threshold) {
            x = copy.x + ThresholdRandom(threshold);
            y = copy.y + ThresholdRandom(threshold);
            width = copy.width + ThresholdRandom(threshold);
            height = copy.height + ThresholdRandom(threshold);
            int r = (int)Math.Floor(Math.Clamp(copy.color.R + ThresholdRandom(threshold), 0, 255));
            int g = (int)Math.Floor(Math.Clamp(copy.color.G + ThresholdRandom(threshold), 0, 255));
            int b = (int)Math.Floor(Math.Clamp(copy.color.B + ThresholdRandom(threshold), 0, 255));
            if (r > 255 || g > 255 || b > 255)
                Debug.WriteLine($"{r}, {g}, {b}");
            color = Color.FromArgb(255, r, g, b);
        }

        public float ThresholdRandom(float threshold) {
            return (rnd.NextSingle() * (threshold * 2)) - threshold;
        }

    }
}
