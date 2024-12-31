using OpenTK.Graphics.OpenGL4;
using OpenTK.Windowing.Common;
using OpenTK.Windowing.Desktop;
using static System.Windows.Forms.VisualStyles.VisualStyleElement;

namespace EmblemEditor {
    public partial class Form1: Form {
        GameWindow gameWindow = new GameWindow(GameWindowSettings.Default, NativeWindowSettings.Default);
        public Form1() {
            InitializeComponent();
            gameWindow.Load += OnLoad;
            gameWindow.RenderFrame += OnRenderFrame;
            //gameWindow.Run();
        }

        static int referenceTexture, renderTexture, diffBuffer, computeProgram;
        static Shader renderShader, computeShader;

        private static void OnLoad() {
            // Initialize shaders
            renderShader = new Shader("vertex_shader.glsl", "fragment_shader.glsl");
            computeShader = new Shader("compute_shader.glsl");

            // Load reference image into a texture
            referenceTexture = LoadTexture("reference_image.png");
            renderTexture = CreateEmptyTexture(512, 512); // Example size

            // Create buffer for differences
            diffBuffer = GL.GenBuffer();
            GL.BindBuffer(BufferTarget.ShaderStorageBuffer, diffBuffer);
            GL.BufferData(BufferTarget.ShaderStorageBuffer, 512 * 512 * sizeof(float), IntPtr.Zero, BufferUsageHint.DynamicCopy);
            GL.BindBuffer(BufferTarget.ShaderStorageBuffer, 0);

            // Setup OpenGL state
            GL.ClearColor(0.1f, 0.1f, 0.1f, 1.0f);
        }

        private static void OnRenderFrame(FrameEventArgs args) {
            // Bind the render texture
            GL.BindFramebuffer(FramebufferTarget.Framebuffer, renderTexture);
            GL.Clear(ClearBufferMask.ColorBufferBit);

            // Render squares
            renderShader.Use();
            GL.DrawArrays(PrimitiveType.Triangles, 0, 6); // Example: a single square

            // Reset framebuffer
            GL.BindFramebuffer(FramebufferTarget.Framebuffer, 0);

            // Dispatch compute shader
            computeShader.Use();
            GL.BindImageTexture(0, referenceTexture, 0, false, 0, TextureAccess.ReadOnly, SizedInternalFormat.Rgba8);
            GL.BindImageTexture(1, renderTexture, 0, false, 0, TextureAccess.ReadOnly, SizedInternalFormat.Rgba8);
            GL.BindBufferBase(BufferRangeTarget.ShaderStorageBuffer, 2, diffBuffer);

            GL.DispatchCompute(512 / 16, 512 / 16, 1); // Dispatch compute shader
            GL.MemoryBarrier(MemoryBarrierFlags.ShaderStorageBarrierBit);

            // Read back results (if needed)
            GL.BindBuffer(BufferTarget.ShaderStorageBuffer, diffBuffer);
            float[] differences = new float[512 * 512];
            GL.GetBufferSubData(BufferTarget.ShaderStorageBuffer, IntPtr.Zero, differences.Length * sizeof(float), differences);

            // Swap buffers
            //gameWindow.SwapBuffers();
        }


        static int LoadTexture(string filePath) {
            // Load an image file into an OpenGL texture
            int texture = GL.GenTexture();
            GL.BindTexture(TextureTarget.Texture2D, texture);
            // Add texture loading logic here
            return texture;
        }

        static int CreateEmptyTexture(int width, int height) {
            int texture = GL.GenTexture();
            GL.BindTexture(TextureTarget.Texture2D, texture);
            GL.TexImage2D(TextureTarget.Texture2D, 0, PixelInternalFormat.Rgba8, width, height, 0, PixelFormat.Rgba, PixelType.UnsignedByte, IntPtr.Zero);
            GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMinFilter, (int)TextureMinFilter.Linear);
            GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMagFilter, (int)TextureMagFilter.Linear);
            return texture;
        }

    }
}
