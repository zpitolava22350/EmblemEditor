using System.Diagnostics;

namespace EmblemEditorCPU {
    public partial class Form1: Form {

        Bitmap referenceBitmap;

        List<Candidate> candidates;

        public Form1() {

            InitializeComponent();

            referenceBitmap = new Bitmap("images/blehpfp320.png");

            Candidate.setReference(referenceBitmap);

            pictureBox1.Image = referenceBitmap;

        }

        private void pictureBox1_Click(object sender, EventArgs e) {
            run();
        }

        private void run() {

            candidates = new List<Candidate>();

            for (int i = 0; i < 100000; i++) {
                candidates.Add(new Candidate());
                candidates[i].CalculateScore();
            }

            candidates.Sort();
            Candidate best = (Candidate)candidates[0].Clone();

            best.RenderCandidate();

            pictureBox2.Image = Candidate.finalBitmap;

            Debug.WriteLine($"Object #1 done {best.score}");

            for (int t = 0; t < 30; t++) {
                candidates = new List<Candidate>();
                for (int i = 0; i < 10000; i++) {
                    candidates.Add(new Candidate(best));
                    candidates[i].CalculateScore();
                }

                candidates.Sort();

                best = (Candidate)candidates[0].Clone();

                best.RenderCandidate();

                pictureBox2.Image = Candidate.finalBitmap;

                Debug.WriteLine($"Object #{t + 2} done {best.score}");
            }

        }

    }
}
