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

        Shader shader;

        Shader diffProgram;
        Shader squareProgram;

        Texture texture;

        float[] vertices = {
            //Position        Texture coordinates
            1f,  1f, 0.0f,    1.0f, 1.0f, // top right
            1f, -1f, 0.0f,    1.0f, 0.0f, // bottom right
            -1f, -1f, 0.0f,   0.0f, 0.0f, // bottom left
            -1f,  1f, 0.0f,   0.0f, 1.0f  // top left
        };

        uint[] indices = {  // note that we start from 0!
            0, 1, 3,   // first triangle
            1, 2, 3    // second triangle
        };

        int vbo;
        int vao;

        int sceneFBO;
        int sceneTex;

        public Form1() {
            InitializeComponent();
            StbImage.stbi_set_flip_vertically_on_load(1);

            rnd = new Random();

            glControl1.Paint += GlControl1_Paint;
            glControl1.Load += GlControl1_Load;
            glControl1.Resize += GlControl1_Resize;

            FormClosing += (object? sender, FormClosingEventArgs e) => { shader.Dispose(); };
        }

        public void GlControl1_Load(object? sender, EventArgs e) {
            if (!glControl1.Context.IsCurrent)
                glControl1.MakeCurrent();

            GL.Viewport(0, 0, glControl1.Width, glControl1.Height);
            GL.ClearColor(0.2f, 0.3f, 0.3f, 1f);

            
            shader = new Shader("Shaders/old/vertex.vert", "Shaders/old/fragment.frag");
            shader.Use();

            vbo = GL.GenBuffer();

            GL.BindBuffer(BufferTarget.ArrayBuffer, vbo);
            GL.BufferData(BufferTarget.ArrayBuffer, vertices.Length * sizeof(float), vertices, BufferUsageHint.StaticDraw);

            vao = GL.GenVertexArray();
            GL.BindVertexArray(vao);

            int vertexLocation = GL.GetAttribLocation(shader.Handle, "aPosition");
            GL.EnableVertexAttribArray(vertexLocation);
            GL.VertexAttribPointer(vertexLocation, 3, VertexAttribPointerType.Float, false, 5 * sizeof(float), 0);

            int texCoordLocation = GL.GetAttribLocation(shader.Handle, "aTexCoord");
            GL.EnableVertexAttribArray(texCoordLocation);
            GL.VertexAttribPointer(texCoordLocation, 2, VertexAttribPointerType.Float, false, 5 * sizeof(float), 3 * sizeof(float));

            int ebo = GL.GenBuffer();
            GL.BindBuffer(BufferTarget.ElementArrayBuffer, ebo);
            GL.BufferData(BufferTarget.ElementArrayBuffer, indices.Length * sizeof(int), indices, BufferUsageHint.StaticDraw);
            

            // SQUARE SLOP

            squareProgram = new Shader("Shaders/drawsquares/vertex.vert", "Shaders/drawsquares/fragment.frag");
            squareProgram.Use();


            // FBO SLOP

            diffProgram = new Shader("Shaders/diffpass/vertex.vert", "Shaders/diffpass/fragment.frag");
            diffProgram.Use();

            // make tex
            GL.GenTextures(1, out sceneTex);
            GL.BindTexture(TextureTarget.Texture2D, sceneTex);
            GL.TexImage2D(TextureTarget.Texture2D,
                          0,
                          PixelInternalFormat.Rgba8,
                          512, 512,
                          0,
                          PixelFormat.Rgba,
                          PixelType.UnsignedByte,
                          IntPtr.Zero);
            GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMinFilter, (int)TextureMinFilter.Nearest);
            GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMagFilter, (int)TextureMagFilter.Nearest);
            GL.BindTexture(TextureTarget.Texture2D, 0);

            // make fbo
            GL.GenFramebuffers(1, out sceneFBO);
            GL.BindFramebuffer(FramebufferTarget.Framebuffer, sceneFBO);
            GL.FramebufferTexture2D(FramebufferTarget.Framebuffer,
                                   FramebufferAttachment.ColorAttachment0,
                                   TextureTarget.Texture2D,
                                   sceneTex, 0);

            GL.Uniform1(GL.GetUniformLocation(diffProgram.Handle, "uSceneTex"), 0);
            GL.Uniform1(GL.GetUniformLocation(diffProgram.Handle, "uReferenceTex"), 1);

            GL.BindFramebuffer(FramebufferTarget.Framebuffer, 0);
        }

        private void GlControl1_Resize(object? sender, EventArgs e) {
            if (!glControl1.Context.IsCurrent)
                glControl1.MakeCurrent();

            GL.Viewport(0, 0, glControl1.Width, glControl1.Height);
        }

        private void GlControl1_Paint(object? sender, PaintEventArgs e) {

            if (!glControl1.Context.IsCurrent)
                glControl1.MakeCurrent();

            GL.Clear(ClearBufferMask.ColorBufferBit);

            glControl1.SwapBuffers();

        }

        private void button1_Click(object sender, EventArgs e) {

            if (!glControl1.Context.IsCurrent)
                glControl1.MakeCurrent();

            if (LoadedImage == null)
                return;

            GL.Clear(ClearBufferMask.ColorBufferBit);

            GL.BindVertexArray(vao);

            texture.Use(TextureUnit.Texture0);
            shader.Use();

            //GL.DrawArrays(PrimitiveType.Triangles, 0, 3);
            GL.DrawElements(BeginMode.Triangles, indices.Length, DrawElementsType.UnsignedInt, 0);

            glControl1.SwapBuffers();

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

                    texture = Texture.LoadFromFile(LoadedImageFilepath);
                    texture.Use(TextureUnit.Texture0);
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

