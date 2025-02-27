using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Numerics;
using System.Drawing;
using System.Runtime.ConstrainedExecution;
using System.Drawing.Imaging;
using System.Runtime.Intrinsics;

namespace EmblemEditorCPU {
    internal class Candidate: IComparable, ICloneable {

        public static int size = 20;

        public static Color background = Color.Black;

        private static byte[] reference = new byte[size * size * 3];

        public static Bitmap finalBitmap;

        private List<Circle> prevCircles;

        private Circle current;
        private byte[] image;

        private bool isNew = true;

        public long score { get; private set; }

        static Candidate() {

        }

        /// <summary>
        /// Generates one object
        /// </summary>
        public Candidate() {
            score = long.MaxValue;
            current = new Circle();
            if (isNew) {
                prevCircles = new List<Circle>();
            } else {
                ComputeImage();
            }
            isNew = false;
        }

        /// <summary>
        /// Generate a new circle off of a previous candidate
        /// </summary>
        /// <param name="other">Previous candidate</param>
        public Candidate(Candidate other) : this() {
            image = (byte[])other.image.Clone();
            prevCircles = new List<Circle>(other.prevCircles);
            prevCircles.Add(other.current);
            isNew = false;
        }

        /// <summary>
        /// Exact copy
        /// </summary>
        /// <param name="other">Candidate to copy</param>
        /// <param name="bruh">Just here to indicate copy</param>
        public Candidate(Candidate other, bool bruh) {
            score = other.score;
            current = other.current;
            image = (byte[])other.image.Clone();
            prevCircles = new List<Circle>(other.prevCircles);
        }

        public Candidate(Candidate other, float threshold) {
            current = new Circle(other.current, threshold);
            image = (byte[])other.image.Clone();
            prevCircles = new List<Circle>(other.prevCircles);
        }

        public static void setReference(Bitmap bmp) {
            int i = 0;
            for (int x = 0; x < size; x++) {
                for (int y = 0; y < size; y++) {

                    Color clr = bmp.GetPixel(x*16, y*16);

                    reference[i++] = clr.R;
                    reference[i++] = clr.G;
                    reference[i++] = clr.B;

                }
            }
        }

        public void CalculateScore() {

            ComputeImage();

            int totalPixels = size * size * 3;

            long tempScore = 0;

            for(int x = 0; x < totalPixels; x++) {
                tempScore += Math.Abs(image[x] - reference[x]);
            }

            score = tempScore;
        }

        private void ComputeImage() {

            image = new byte[size * size * 3];
            SetBackground();

            int cx, cy;
            int rx, ry;
            int rx2, ry2;
            byte r, g, b;

            foreach (Circle c in prevCircles) {

                cx = (int)Math.Round(c.x);
                cy = (int)Math.Round(c.y);
                rx = (int)Math.Round(c.width / 2);
                ry = (int)Math.Round(c.height / 2);
                rx2 = rx * rx;
                ry2 = ry * ry;
                r = c.color.R;
                g = c.color.G;
                b = c.color.B;

                for (int y = Math.Max(0, cy - ry); y < Math.Min(size, cy + ry); y++) {
                    for (int x = Math.Max(0, cx - rx); x < Math.Min(size, cx + rx); x++) {
                        int dx = x - cx, dy = y - cy;
                        if ((dx * dx) * ry2 + (dy * dy) * rx2 <= rx2 * ry2) { // Ellipse equation
                            int index = (y * size + x) * 3;
                            image[index] = r;
                            image[index + 1] = g;
                            image[index + 2] = b;
                        }
                    }
                }
            }

            cx = (int)Math.Round(current.x);
            cy = (int)Math.Round(current.y);
            rx = (int)Math.Round(current.width / 2);
            ry = (int)Math.Round(current.height / 2);
            rx2 = rx * rx;
            ry2 = ry * ry;
            r = current.color.R;
            g = current.color.G;
            b = current.color.B;

            for (int y = Math.Max(0, cy - ry); y < Math.Min(size, cy + ry); y++) {
                for (int x = Math.Max(0, cx - rx); x < Math.Min(size, cx + rx); x++) {
                    int dx = x - cx, dy = y - cy;
                    if ((dx * dx) * ry2 + (dy * dy) * rx2 <= rx2 * ry2) { // Ellipse equation
                        int index = (y * size + x) * 3;
                        image[index] = r;
                        image[index + 1] = g;
                        image[index + 2] = b;
                        //Debug.WriteLine($"{r}, {g}, {b}");
                    }
                }
            }
        }


        private void SetBackground() {
            int totalBytes = size * size * 3;
            for(int i = 0; i < totalBytes; i += 3) {
                image[i] = background.R;
                image[i + 1] = background.G;
                image[i + 2] = background.B;
            }
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

        public void RenderCandidate() {

            finalBitmap = new Bitmap(size, size);

            for(int i = 0; i < size*size; i++) {

                finalBitmap.SetPixel(i / size, i % size, Color.FromArgb(image[i * 3], image[(i * 3) + 1], image[(i * 3) + 2]));

            }

            /*
            if(finalBitmap != null)
                finalBitmap.Dispose();

            finalBitmap = new Bitmap(size, size);
            BitmapData bmpData = finalBitmap.LockBits(new Rectangle(0, 0, size, size),
                                                      ImageLockMode.WriteOnly, PixelFormat.Format24bppRgb);
            IntPtr ptr = bmpData.Scan0;
            System.Runtime.InteropServices.Marshal.Copy(image, 0, ptr, image.Length);
            finalBitmap.UnlockBits(bmpData);
            */

        }


    }
}
