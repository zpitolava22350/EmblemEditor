﻿using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EmblemEditorCPU {
    internal class Candidate : IComparable, ICloneable {

        public static int sizeDiv = 8;

        public static int sectionSize = 5;

        public static int size = 320/sizeDiv;

        public static Color background = Color.Black;

        private static Color[,] reference = new Color[size, size];

        private static Color[,] finalImage = new Color[320, 320];

        public static Bitmap finalBitmap;

        private List<Circle> previousCircles;
        private Circle current;

        public long score { get; private set; }

        static Candidate() {
            
        }

        /// <summary>
        /// Generates one object
        /// </summary>
        public Candidate() {
            score = long.MaxValue;
            previousCircles = new List<Circle>();
            current = new Circle();
        }

        /// <summary>
        /// Generate a new circle off of a previous candidate
        /// </summary>
        /// <param name="other">Previous candidate</param>
        public Candidate(Candidate other) : this() {
            previousCircles = new List<Circle>(other.previousCircles);
            previousCircles.Add(other.current);
        }

        /// <summary>
        /// Exact copy
        /// </summary>
        /// <param name="other">Candidate to copy</param>
        /// <param name="bruh">Just here to indicate copy</param>
        public Candidate(Candidate other, bool bruh) {
            previousCircles = new List<Circle>(other.previousCircles);
            score = other.score;
            current = other.current;
        }

        public Candidate(Candidate other, float threshold) {
            previousCircles = new List<Circle>(other.previousCircles);
            current = new Circle(other.current, threshold/sizeDiv);
        }

        public static void setReference(Bitmap bmp) {
            for (int x = 0; x < size; x++) {
                for (int y = 0; y < size; y++) {
                    
                    reference[x, y] = bmp.GetPixel(x * sizeDiv, y * sizeDiv);

                }
            }
        }

        public void CalculateScore() {
            long tempScore = 0;

            for (int x = 0; x < size; x++) {
                for (int y = 0; y < size; y++) {

                    Color pixelColor = background;

                    if (current.GetColor(x, y) != null) {
                        pixelColor = current.color;
                    } else {
                        foreach (Circle circle in previousCircles.AsEnumerable().Reverse()) {
                            if (circle.GetColor(x, y) != null) {
                                pixelColor = circle.color;
                                break;
                            }
                        }
                    }

                    int temp = ColorDifference(pixelColor, reference[x, y]);
                    tempScore += temp;

                }
            }

            score = tempScore;
        }

        public int CompareTo(object obj) {

            int returnValue = 0;

            if (obj is Candidate cand) {
                // Sort by score
                returnValue = this.score.CompareTo(cand.score);
            }
            return returnValue;
        }

        public object Clone() {
            return new Candidate(this, true);
        }

        private static int ColorDifference(Color color1, Color color2) {
            return Math.Abs(color1.R - color2.R) + Math.Abs(color1.G - color2.G) + Math.Abs(color1.B - color2.B);
        }

        public void RenderCandidate() {
            for (int x = 0; x < 320; x++) {
                for (int y = 0; y < 320; y++) {

                    Color pixelColor = background;

                    Circle tempCircle = new Circle(current.x * sizeDiv, current.y * sizeDiv, current.width * sizeDiv, current.height * sizeDiv, current.color);

                    if (tempCircle.GetColor(x, y) != null) {
                        pixelColor = tempCircle.color;
                    } else {
                        foreach (Circle circle in previousCircles.AsEnumerable().Reverse()) {
                            tempCircle = new Circle(circle.x * sizeDiv, circle.y * sizeDiv, circle.width * sizeDiv, circle.height * sizeDiv, circle.color);
                            if (tempCircle.GetColor(x, y) != null) {
                                pixelColor = tempCircle.color;
                                break;
                            }
                        }
                    }

                    finalImage[x, y] = pixelColor;

                }
            }

            finalBitmap = new Bitmap(320, 320);

            for (int x = 0; x < 320; x++) {
                for (int y = 0; y < 320; y++) {

                    finalBitmap.SetPixel(x, y, finalImage[x, y]);

                }
            }

        }

    }
}
