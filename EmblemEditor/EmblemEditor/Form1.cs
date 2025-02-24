using OpenTK.GLControl;
using OpenTK.Graphics.OpenGL4;
using OpenTK.Windowing.Common;
using OpenTK.Windowing.Desktop;
using System.Drawing.Imaging;
using System.Runtime.InteropServices;
using System.Windows.Forms;
using static System.Windows.Forms.VisualStyles.VisualStyleElement;
using PixelFormat = OpenTK.Graphics.OpenGL4.PixelFormat;

namespace EmblemEditor {
    public partial class Form1: Form {

        int reference;
        Bitmap referenceBitmap;
        int refWidth;
        int refHeight;

        int framebufferTexture;
        int framebuffer;

        int diffFramebuffer;
        int diffTexture;

        int differenceShaderProgram;

        public Form1() {
            InitializeComponent();

            referenceBitmap = new Bitmap("textures/blehpfp512.png");

            pictureBox1.Image = referenceBitmap;

            refWidth = referenceBitmap.Width;
            refHeight = referenceBitmap.Height;

            // Register event handlers for OpenGL control
            glControl1.Load += GlControl1_Load;
            glControl1.Paint += GlControl1_Paint;
        }

        private void GlControl1_Load(object sender, EventArgs e) {
            // This ensures the OpenGL context is initialized
            string vertexShaderSource = File.ReadAllText("shaders/vertex_shader.glsl");
            string fragmentShaderSource = File.ReadAllText("shaders/fragment_shader.glsl");

            // Create shader program
            differenceShaderProgram = CreateShaderProgram(vertexShaderSource, fragmentShaderSource);

            // Setup reference texture
            reference = GL.GenTexture();
            GL.BindTexture(TextureTarget.Texture2D, reference);

            // Reference image to bytes
            BitmapData referenceData = referenceBitmap.LockBits(
                new Rectangle(0, 0, refWidth, refHeight),
                ImageLockMode.ReadOnly,
                System.Drawing.Imaging.PixelFormat.Format32bppArgb
                //referenceBitmap.PixelFormat
            );
            byte[] referencePixels = new byte[refWidth * refHeight * 4];
            Marshal.Copy(referenceData.Scan0, referencePixels, 0, referencePixels.Length);
            referenceBitmap.UnlockBits(referenceData);

            // Upload bytes to GL texture
            GL.TexImage2D(TextureTarget.Texture2D, 0, PixelInternalFormat.Rgba, refWidth, refHeight, 0, PixelFormat.Rgba, PixelType.UnsignedByte, referencePixels);

            // Set texture parameters
            GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMinFilter, (int)TextureMinFilter.Linear);
            GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMagFilter, (int)TextureMagFilter.Linear);
        }


        private void RenderDifferenceTextureToScreen() {
            // Use the texture that contains the color difference (diffTexture)
            GL.BindFramebuffer(FramebufferTarget.Framebuffer, 0); // Bind the default framebuffer (screen)

            // Clear the screen
            GL.Clear(ClearBufferMask.ColorBufferBit);

            // Bind the difference texture (diffTexture) to texture unit 0
            GL.ActiveTexture(TextureUnit.Texture0);
            GL.BindTexture(TextureTarget.Texture2D, diffTexture);

            // Use a simple shader to render the difference texture
            GL.UseProgram(differenceShaderProgram);

            // Set the texture uniform
            GL.Uniform1(GL.GetUniformLocation(differenceShaderProgram, "framebufferTexture"), 0); // Texture unit 0

            // Draw a full-screen quad to display the difference texture on the screen
            DrawFullScreenQuad();

            // Swap buffers after rendering
            glControl1.SwapBuffers();  // Assuming you're using the GLControl's SwapBuffers for double buffering
        }

        private int CompileShader(string shaderSource, ShaderType type) {
            int shader = GL.CreateShader(type);
            GL.ShaderSource(shader, shaderSource);
            GL.CompileShader(shader);

            string infoLog = GL.GetShaderInfoLog(shader);
            if (!string.IsNullOrEmpty(infoLog)) {
                throw new Exception($"Shader compile error: {infoLog}");
            }

            return shader;
        }

        private int CreateShaderProgram(string vertexShaderSource, string fragmentShaderSource) {
            // Compile shaders
            int vertexShader = CompileShader(vertexShaderSource, ShaderType.VertexShader);
            int fragmentShader = CompileShader(fragmentShaderSource, ShaderType.FragmentShader);

            // Create shader program and link shaders
            int program = GL.CreateProgram();
            GL.AttachShader(program, vertexShader);
            GL.AttachShader(program, fragmentShader);
            GL.LinkProgram(program);

            // Check for linking errors
            string programLog = GL.GetProgramInfoLog(program);
            if (!string.IsNullOrEmpty(programLog)) {
                throw new Exception($"Program link error: {programLog}");
            }

            // Clean up shaders as they are no longer needed after linking
            GL.DeleteShader(vertexShader);
            GL.DeleteShader(fragmentShader);

            return program;
        }

        /*
        public Form1() {
            InitializeComponent();

            // Load the shaders
            string vertexShaderSource = File.ReadAllText("shaders/vertex_shader.glsl");
            string fragmentShaderSource = File.ReadAllText("shaders/fragment_shader.glsl");

            // Create the shader program
            differenceShaderProgram = CreateShaderProgram(vertexShaderSource, fragmentShaderSource);

            referenceBitmap = new Bitmap("textures/blehpfp512.png");

            refWidth = referenceBitmap.Width;
            refHeight = referenceBitmap.Height;

            //pictureBox1.Image = referenceBitmap;

            glControl1.Load += GlControl1_Load;
            glControl1.Paint += GlControl1_Paint;

            reference = GL.GenTexture();
            GL.BindTexture(TextureTarget.Texture2D, reference);

            // Reference image to bytes
            BitmapData referenceData = referenceBitmap.LockBits(
                new Rectangle(0, 0, refWidth, refHeight),
                ImageLockMode.ReadOnly,
                System.Drawing.Imaging.PixelFormat.Format32bppArgb
            );
            byte[] referencePixels = new byte[refWidth * refHeight * 4];
            Marshal.Copy(referenceData.Scan0, referencePixels, 0, referencePixels.Length);
            referenceBitmap.UnlockBits(referenceData);

            // Upload bytes to GL texture
            GL.TexImage2D(TextureTarget.Texture2D, 0, PixelInternalFormat.Rgba, refWidth, refHeight, 0, PixelFormat.Rgba, PixelType.UnsignedByte, referencePixels);

            // Set texture parameters
            GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMinFilter, (int)TextureMinFilter.Linear);
            GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMagFilter, (int)TextureMagFilter.Linear);

            CreateFramebuffer();
            //CreateDiffFramebuffer();

        }
        */

        private void CreateFramebuffer() {
            // Create texture to store the framebuffer output
            framebufferTexture = GL.GenTexture();
            GL.BindTexture(TextureTarget.Texture2D, framebufferTexture);
            GL.TexImage2D(TextureTarget.Texture2D, 0, PixelInternalFormat.Rgba, 512, 512, 0, PixelFormat.Rgba, PixelType.UnsignedByte, IntPtr.Zero);

            // Set texture parameters (optional)
            GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMinFilter, (int)TextureMinFilter.Linear);
            GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMagFilter, (int)TextureMagFilter.Linear);

            // Create framebuffer and attach the texture to it
            framebuffer = GL.GenFramebuffer();
            GL.BindFramebuffer(FramebufferTarget.Framebuffer, framebuffer);
            GL.FramebufferTexture(FramebufferTarget.Framebuffer, FramebufferAttachment.ColorAttachment0, framebufferTexture, 0);

            // Check framebuffer status
            if (GL.CheckFramebufferStatus(FramebufferTarget.Framebuffer) != FramebufferErrorCode.FramebufferComplete) {
                throw new Exception("Framebuffer is not complete!");
            }

            // Unbind the framebuffer
            GL.BindFramebuffer(FramebufferTarget.Framebuffer, 0);
        }

        private void CreateDiffFramebuffer() {

            // Create a texture to store the result
            diffTexture = GL.GenTexture();
            GL.BindTexture(TextureTarget.Texture2D, diffTexture);
            GL.TexImage2D(TextureTarget.Texture2D, 0, PixelInternalFormat.Rgba32f, 512, 512, 0, PixelFormat.Rgba, PixelType.Float, IntPtr.Zero);

            // Create the framebuffer and attach the diff texture to it
            diffFramebuffer = GL.GenFramebuffer();
            GL.BindFramebuffer(FramebufferTarget.Framebuffer, diffFramebuffer);
            GL.FramebufferTexture(FramebufferTarget.Framebuffer, FramebufferAttachment.ColorAttachment0, diffTexture, 0);

            // Check framebuffer status
            if (GL.CheckFramebufferStatus(FramebufferTarget.Framebuffer) != FramebufferErrorCode.FramebufferComplete) {
                throw new Exception("Framebuffer is not complete!");
            }

            // Unbind framebuffer
            GL.BindFramebuffer(FramebufferTarget.Framebuffer, 0);
        }

        private void RenderColorDifference() {
            GL.UseProgram(differenceShaderProgram);

            // Bind the framebuffer texture (or the texture you're comparing to)
            GL.ActiveTexture(TextureUnit.Texture0);
            GL.BindTexture(TextureTarget.Texture2D, framebufferTexture);

            GL.ActiveTexture(TextureUnit.Texture1);
            GL.BindTexture(TextureTarget.Texture2D, reference);

            // Set shader uniforms
            GL.Uniform1(GL.GetUniformLocation(differenceShaderProgram, "framebufferTexture"), 0);
            GL.Uniform1(GL.GetUniformLocation(differenceShaderProgram, "referenceTexture"), 1);

            // Draw the full-screen quad
            DrawFullScreenQuad();
        }



        private void DrawFullScreenQuad() {
            // Full-screen quad vertices (position + texture coordinates)
            float[] vertices = {
                -1.0f, -1.0f, 0.0f, 0.0f, 0.0f,  // Bottom-left
                1.0f, -1.0f, 0.0f, 1.0f, 0.0f,   // Bottom-right
                1.0f,  1.0f, 0.0f, 1.0f, 1.0f,   // Top-right
                -1.0f,  1.0f, 0.0f, 0.0f, 1.0f   // Top-left
            };

            uint[] indices = { 0, 1, 2, 2, 3, 0 };

            // Create and bind VAO, VBO, and EBO
            int vao = GL.GenVertexArray();
            int vbo = GL.GenBuffer();
            int ebo = GL.GenBuffer();

            GL.BindVertexArray(vao);

            GL.BindBuffer(BufferTarget.ArrayBuffer, vbo);
            GL.BufferData(BufferTarget.ArrayBuffer, vertices.Length * sizeof(float), vertices, BufferUsageHint.StaticDraw);

            GL.BindBuffer(BufferTarget.ElementArrayBuffer, ebo);
            GL.BufferData(BufferTarget.ElementArrayBuffer, indices.Length * sizeof(uint), indices, BufferUsageHint.StaticDraw);

            // Set up vertex attributes (position and texture coordinates)
            GL.VertexAttribPointer(0, 3, VertexAttribPointerType.Float, false, 5 * sizeof(float), 0);
            GL.EnableVertexAttribArray(0);
            GL.VertexAttribPointer(1, 2, VertexAttribPointerType.Float, false, 5 * sizeof(float), 3 * sizeof(float));
            GL.EnableVertexAttribArray(1);

            // Draw the quad
            GL.DrawElements(PrimitiveType.Triangles, indices.Length, DrawElementsType.UnsignedInt, 0);

            // Clean up
            GL.DeleteBuffer(vbo);
            GL.DeleteBuffer(ebo);
            GL.DeleteVertexArray(vao);
        }


        /*
        private int CompileShader(string shaderSource, ShaderType type) {
            int shader = GL.CreateShader(type);
            GL.ShaderSource(shader, shaderSource);
            GL.CompileShader(shader);

            string infoLog = GL.GetShaderInfoLog(shader);
            if (!string.IsNullOrEmpty(infoLog)) {
                throw new Exception($"Shader compile error: {infoLog}");
            }

            return shader;
        }
        */

        /*
        private int CreateShaderProgram(string vertexShaderSource, string fragmentShaderSource) {
            // Compile shaders
            int vertexShader = CompileShader(vertexShaderSource, ShaderType.VertexShader);
            int fragmentShader = CompileShader(fragmentShaderSource, ShaderType.FragmentShader);

            // Create shader program and link shaders
            int program = GL.CreateProgram();
            GL.AttachShader(program, vertexShader);
            GL.AttachShader(program, fragmentShader);
            GL.LinkProgram(program);

            // Check for linking errors
            string programLog = GL.GetProgramInfoLog(program);
            if (!string.IsNullOrEmpty(programLog)) {
                throw new Exception($"Program link error: {programLog}");
            }

            // Clean up shaders as they are no longer needed after linking
            GL.DeleteShader(vertexShader);
            GL.DeleteShader(fragmentShader);

            return program;
        }
        */

        /*
        private void GlControl1_Load(object? sender, EventArgs e) {
            // Ensure OpenGL bindings are loaded
            GL.LoadBindings(new OpenTK.Graphics.ES30.GraphicsBinding());

            // Load shaders
            string vertexShaderSource = File.ReadAllText("shaders/vertex_shader.glsl");
            string fragmentShaderSource = File.ReadAllText("shaders/fragment_shader.glsl");

            // Create shader program after OpenGL is initialized
            differenceShaderProgram = CreateShaderProgram(vertexShaderSource, fragmentShaderSource);

            // Any other OpenGL initialization can go here
        }
        */


        private void GlControl1_Paint(object sender, PaintEventArgs e) {
            // This ensures the GLControl is rendered on each paint event
            GL.Clear(ClearBufferMask.ColorBufferBit);

            // Render the color difference here instead
            RenderColorDifference();

            glControl1.SwapBuffers(); // Swap buffers after rendering
        }



    }
}
