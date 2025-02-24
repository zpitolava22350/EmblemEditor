using System.Diagnostics;

namespace EmblemEditorCPU {
    public partial class Form1: Form {

        Bitmap referenceBitmap;

        List<Candidate> candidates;

        int objects = 1000;
        int tries = 20000;
        int takeFrom = 30;
        int modifications = 700;

        public Form1() {

            InitializeComponent();

            //referenceBitmap = new Bitmap("images/blehpfp320.png");
            referenceBitmap = new Bitmap("images/IMG_80062.png");

            Candidate.setReference(referenceBitmap);

            pictureBox1.Image = referenceBitmap;

            UpdateBackground(Color.FromArgb(255, 191, 166, 157));

        }

        private void run() {

            candidates = new List<Candidate>();
            List<Candidate> tempList;

            // Run first iteration
            for (int i = 0; i < tries; i++) {
                candidates.Add(new Candidate());
                candidates[i].CalculateScore();
            }

            candidates.Sort();
            Candidate best = (Candidate)candidates[0].Clone();
            best.RenderCandidate();
            pictureBox2.Image = Candidate.finalBitmap;
            pictureBox2.Invalidate();
            Application.DoEvents();

            Debug.WriteLine($"Object #1 Generated {best.score}");

            // Make better
            candidates = candidates.Take(takeFrom).ToList();
            tempList = candidates.ToList();
            Debug.WriteLine(candidates.Count());
            foreach (Candidate c in tempList) {
                for(int i = 0; i < modifications; i++) {
                    candidates.Add(new Candidate(c, 10f));
                    candidates[candidates.Count()-1].CalculateScore();
                }
            }

            candidates.Sort();
            best = (Candidate)candidates[0].Clone();
            best.RenderCandidate();
            pictureBox2.Image = Candidate.finalBitmap;
            pictureBox2.Invalidate();
            Application.DoEvents();

            Debug.WriteLine($"Object #1 Improved {best.score}");

            // Run other iterations
            for (int t = 0; t < objects-1; t++) {
                candidates = new List<Candidate>();
                for (int i = 0; i < tries; i++) {
                    candidates.Add(new Candidate(best));
                    candidates[i].CalculateScore();
                }

                candidates.Sort();
                best = (Candidate)candidates[0].Clone();
                best.RenderCandidate();
                pictureBox2.Image = Candidate.finalBitmap;
                pictureBox2.Invalidate();
                Application.DoEvents();

                Debug.WriteLine($"Object #{t + 2} Generated {best.score}");

                // Make better
                candidates = candidates.Take(takeFrom).ToList();
                tempList = candidates.ToList();
                foreach (Candidate c in tempList) {
                    for (int i = 0; i < modifications; i++) {
                        candidates.Add(new Candidate(c, 10f));
                        candidates[candidates.Count() - 1].CalculateScore();
                    }
                }

                candidates.Sort();
                best = (Candidate)candidates[0].Clone();
                best.RenderCandidate();
                pictureBox2.Image = Candidate.finalBitmap;
                pictureBox2.Invalidate();
                Application.DoEvents();

                Debug.WriteLine($"Object #{t + 2} Improved {best.score}");

            }

        }

        private void clrdisplay_Click(object sender, EventArgs e) {
            if (colorDialog1.ShowDialog() == DialogResult.OK) {
                UpdateBackground(colorDialog1.Color);
            }
        }

        private void btn_Generate_Click(object sender, EventArgs e) {
            run();
        }

        private void pictureBox1_MouseClick(object sender, MouseEventArgs e) {
            if (pictureBox1.Image != null) {
                Bitmap bmp = new Bitmap(pictureBox1.Image);
                UpdateBackground(bmp.GetPixel(e.X, e.Y));
            }
        }

        private void UpdateBackground(Color clr) {
            clrdisplay.BackColor = clr;
            Candidate.background = clr;
        }
    }
}
