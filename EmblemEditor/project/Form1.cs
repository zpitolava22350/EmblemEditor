using static System.Windows.Forms.VisualStyles.VisualStyleElement;
using System.Diagnostics;
using System.Security.Policy;
using System.Windows.Forms;
using StbImageSharp;
using OpenTK.Graphics.OpenGL4;
using OpenTK.GLControl;

namespace EmblemEditor {
    public partial class Form1: Form {

        Image LoadedImage;
        string LoadedImageFilepath;

        Random rnd;

        List<Candidate> candidates;

        int Generate = 1000;
        int Take = 6;
        int AdjustEach = 140;

        int TotalItems = 40;

        public Form1() {
            InitializeComponent();
            StbImage.stbi_set_flip_vertically_on_load(1);

            Candidate.SetControl(glControl1);

            rnd = new Random();

            glControl1.Paint += GlControl1_Paint;
            glControl1.Load += GlControl1_Load;
            glControl1.Resize += GlControl1_Resize;
        }

        public void GlControl1_Load(object? sender, EventArgs e) {
            if (!glControl1.Context.IsCurrent)
                glControl1.MakeCurrent();

            Candidate.Load();
        }

        private void button1_Click(object sender, EventArgs e) {

            if (!glControl1.Context.IsCurrent)
                glControl1.MakeCurrent();

            if (LoadedImage == null)
                return;

            Stopwatch sw = Stopwatch.StartNew();

            candidates = new List<Candidate>();

            for (int i = 0; i < Generate; i++) {
                candidates.Add(new Candidate());
                candidates[candidates.Count - 1].CalculateScore(false);
            }

            //candidates.Sort((a, b) => Math.Abs(a.Score).CompareTo(Math.Abs(b.Score)));
            candidates = candidates.OrderBy(c => c.Score).Take(Take).ToList();

            for (int i = 0; i < AdjustEach; i++) {
                for (int j = 0; j < Take; j++) {
                    candidates.Add(Candidate.Adjust(candidates[j]));
                    candidates[candidates.Count - 1].CalculateScore(true);
                }
            }

            candidates = candidates.OrderBy(c => c.Score).Take(1).ToList();

            candidates[0].Show();


            for (int items = 1; items < TotalItems; items++) {

                for (int i = 0; i < Generate; i++) {
                    candidates.Add(Candidate.New(candidates[0]));
                    candidates[candidates.Count - 1].CalculateScore(false);
                }

                //candidates.Sort((a, b) => Math.Abs(a.Score).CompareTo(Math.Abs(b.Score)));
                candidates = candidates.OrderBy(c => c.Score).Take(Take).ToList();

                for (int i = 0; i < AdjustEach; i++) {
                    for (int j = 0; j < Take; j++) {
                        candidates.Add(Candidate.Adjust(candidates[j]));
                        candidates[candidates.Count - 1].CalculateScore(true);
                    }
                }

                candidates = candidates.OrderBy(c => c.Score).Take(1).ToList();

                candidates[0].Show();

            }


            sw.Stop();
            Debug.WriteLine($"Took: {sw.ElapsedMilliseconds}ms");

        }

        private void GlControl1_Paint(object? sender, PaintEventArgs e) {

            if (!glControl1.Context.IsCurrent)
                glControl1.MakeCurrent();

            GL.Clear(ClearBufferMask.ColorBufferBit);

            glControl1.SwapBuffers();

        }
        
        private void GlControl1_Resize(object? sender, EventArgs e) {
            if (!glControl1.Context.IsCurrent)
                glControl1.MakeCurrent();

            GL.Viewport(0, 0, glControl1.Width, glControl1.Height);
        }

        private void Form1_DragDrop(object sender, DragEventArgs e) {
            string[] files = (string[])e.Data.GetData(DataFormats.FileDrop);
            if (files.Length == 1) {
                string filePath = files[0];
                if (filePath.ToLower().EndsWith(".png") || filePath.ToLower().EndsWith(".jpg")) {
                    LoadedImage = Image.FromFile(filePath);
                    LoadedImageFilepath = filePath;
                    pictureBox1.Image = LoadedImage;
                    //ImageResult image = ImageResult.FromStream(File.OpenRead(LoadedImageFilepath), ColorComponents.RedGreenBlueAlpha);

                    Candidate.Reference = Texture.LoadFromFile(LoadedImageFilepath);
                    //texture.Use(TextureUnit.Texture0);
                }
            }
        }

        private void Form1_DragEnter(object sender, DragEventArgs e) {
            if (e.Data.GetDataPresent(DataFormats.FileDrop)) {
                // Get the dragged file(s)
                string[] files = (string[])e.Data.GetData(DataFormats.FileDrop);
                // Check if the file has the ".nbs" extension
                if (files.Length == 1 && (Path.GetExtension(files[0]).Equals(".png", StringComparison.OrdinalIgnoreCase) || Path.GetExtension(files[0]).Equals(".jpg", StringComparison.OrdinalIgnoreCase))) {
                    e.Effect = DragDropEffects.Copy; // Allow drop
                } else {
                    e.Effect = DragDropEffects.None; // Disallow drop
                }
            } else {
                e.Effect = DragDropEffects.None; // Disallow drop
            }
        }
    }
}

