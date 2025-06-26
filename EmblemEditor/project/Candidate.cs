using OpenTK.GLControl;
using OpenTK.Graphics.OpenGL4;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static System.Windows.Forms.VisualStyles.VisualStyleElement;
using System.Diagnostics;

namespace EmblemEditor {
    internal class Candidate {

        static GLControl Control;
        public static Texture Reference;

        public static void SetControl(GLControl control) { Control = control; }

        static Shader DrawSquares;
        static Shader DiffPass;

        public static void Load() {
            if (!Control.Context.IsCurrent)
                Control.MakeCurrent();
            GL.Viewport(0, 0, Control.Width, Control.Height);
            GL.ClearColor(0.2f, 0.3f, 0.3f, 1f);
            DrawSquares = new Shader("Shaders/drawsquares/vertex.vert", "Shaders/drawsquares/fragment.frag");
            DrawSquares.Use();
            DiffPass = new Shader("Shaders/diffpass/vertex.vert", "Shaders/diffpass/fragment.frag");
            DiffPass.Use();
        }

        List<Shape> Shapes;
        Shape CurrentShape;

        int Score;

        public Candidate() {
            Shapes = new List<Shape>();
            CurrentShape = new Shape();
        }

        public void CalculateScore(bool render) {
            if (!Control.Context.IsCurrent)
                Control.MakeCurrent();

            // 1. Create a framebuffer
            int fbo = GL.GenFramebuffer();
            GL.BindFramebuffer(FramebufferTarget.Framebuffer, fbo);

            // 2. Create color texture, allocate storage, set filtering
            int colorTex = GL.GenTexture();
            GL.BindTexture(TextureTarget.Texture2D, colorTex);

            GL.TexImage2D(
                target: TextureTarget.Texture2D,
                level: 0,
                internalformat: PixelInternalFormat.Rgba8,
                width: 512,
                height: 512,
                border: 0,
                format: PixelFormat.Rgba,
                type: PixelType.UnsignedByte,
                pixels: IntPtr.Zero
            );

            GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMinFilter, (int)TextureMinFilter.Linear);
            GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMagFilter, (int)TextureMagFilter.Linear);

            // 3. Attach to framebuffer
            GL.FramebufferTexture2D(FramebufferTarget.Framebuffer, FramebufferAttachment.ColorAttachment0, TextureTarget.Texture2D, colorTex, 0);

            var status = GL.CheckFramebufferStatus(FramebufferTarget.Framebuffer);
            if (status != FramebufferErrorCode.FramebufferComplete) {
                throw new Exception("Framebuffer is not complete: " + status);
            }

            // 4. Tell opengl which color attachments to draw to
            GL.DrawBuffers(1, new[] { DrawBuffersEnum.ColorAttachment0 });

            // 5. Create VAO and bind
            int VAO = GL.GenVertexArray();
            GL.BindVertexArray(VAO);

            // 6. Create VBO, bind, and upload data
            int VBO = GL.GenBuffer();
            GL.BindBuffer(BufferTarget.ArrayBuffer, VBO);
            GL.BufferData(BufferTarget.ArrayBuffer, VerticesLength() * 4, Vertices(), BufferUsageHint.StaticDraw); // VerticesLength() * 4 because sizeof(float) is 4

            // 7. Tell VAO how to interpret the VBO
            GL.EnableVertexAttribArray(0); // Position
            GL.VertexAttribPointer(
                index: 0, // (location = 0)
                size: 3,  // (x, y, z)
                type: VertexAttribPointerType.Float,
                normalized: false,
                stride: 28, // 7 * sizeof(float)
                offset: 0);

            GL.EnableVertexAttribArray(1); // Color
            GL.VertexAttribPointer(
                index: 1, // (location = 1)
                size: 4,  // (r, g, b, a)
                type: VertexAttribPointerType.Float,
                normalized: false,
                stride: 28, // 7 * sizeof(float)
                offset: 12);// 3 * sizeof(float)

            // 8. Unbind VBO
            GL.BindBuffer(BufferTarget.ArrayBuffer, 0);

            // 9. Render
            GL.Clear(ClearBufferMask.ColorBufferBit);
            DrawSquares.Use();
            GL.BindVertexArray(VAO);
            GL.DrawArrays(PrimitiveType.Triangles, 0, VerticesLength());
            GL.BindVertexArray(0);

            // 9.5. Put onto back buffer
            ShowFramebuffer(fbo);
            GL.BindFramebuffer(FramebufferTarget.Framebuffer, 0);

            // 10. Show on screen
            Control.SwapBuffers();

            // 11. Dispose of the VAO
            GL.BindVertexArray(0);
            GL.DeleteVertexArray(VAO);

            //##############################

            // 1. Create Difference FBO
            int diffFbo = GL.GenFramebuffer();
            GL.BindFramebuffer(FramebufferTarget.Framebuffer, diffFbo);

            // 2. Create Difference texture
            int diffTex = GL.GenTexture();
            GL.BindTexture(TextureTarget.Texture2D, diffTex);
            GL.TexImage2D(TextureTarget.Texture2D, 0, PixelInternalFormat.R32f, 512, 512, 0, PixelFormat.Red, PixelType.Float, IntPtr.Zero);
            GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMinFilter, (int)TextureMinFilter.Linear);
            GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMagFilter, (int)TextureMagFilter.Linear);

            // 3. Tell FBO how to interpret
            GL.FramebufferTexture2D(FramebufferTarget.Framebuffer, FramebufferAttachment.ColorAttachment0, TextureTarget.Texture2D, diffTex, 0);
            GL.DrawBuffer(DrawBufferMode.ColorAttachment0);

            // 4. Give shader the textures
            GL.Clear(ClearBufferMask.ColorBufferBit);

            DiffPass.Use();
            GL.ActiveTexture(TextureUnit.Texture0);
            GL.BindTexture(TextureTarget.Texture2D, colorTex);
            DiffPass.SetInt("uFb", 0);

            GL.ActiveTexture(TextureUnit.Texture1);
            GL.BindTexture(TextureTarget.Texture2D, Reference.Handle);
            DiffPass.SetInt("uRef", 1);

            // 5. Make a fullscreen quad
            GL.BindVertexArray(screenQuadVAO);

            // 6. Render
            GL.DrawArrays(PrimitiveType.Triangles, 0, 6);



            /*
            // 1. Create a framebuffer
            int fbo = GL.GenFramebuffer();
            GL.BindFramebuffer(FramebufferTarget.Framebuffer, fbo);

            // 2. Create color texture, allocate storage, set filtering
            int colorTex = GL.GenTexture();
            GL.BindTexture(TextureTarget.Texture2D, colorTex);

            GL.TexImage2D(
                target: TextureTarget.Texture2D,
                level: 0,
                internalformat: PixelInternalFormat.Rgba8,
                width: 512,
                height: 512,
                border: 0,
                format: PixelFormat.Rgba,
                type: PixelType.UnsignedByte,
                pixels: IntPtr.Zero
            );

            GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMinFilter, (int)TextureMinFilter.Linear);
            GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMagFilter, (int)TextureMagFilter.Linear);

            // 3. Attach to framebuffer
            GL.FramebufferTexture2D(FramebufferTarget.Framebuffer, FramebufferAttachment.ColorAttachment0, TextureTarget.Texture2D, colorTex, 0);

            // 4. Tell opengl which color attachments to draw to
            GL.DrawBuffers(1, new[] { DrawBuffersEnum.ColorAttachment0 });
            */

        }

        /// <summary>
        /// Get vertex data for all the shapes as a single array of floats
        /// </summary>
        /// <returns>An array of floats with the format [x, y, z, r, g, b, a, x, y, z, r, g, b, a, etc..]</returns>
        private float[] Vertices() {
            
            List<float> ListVertices = new List<float>();

            for(int i = Shapes.Count - 1; i >= 0; i--) {
                ListVertices.AddRange(Shapes[i].BoundingBox());
            }

            ListVertices.AddRange(CurrentShape.BoundingBox());

            return ListVertices.ToArray();

        }

        private int VerticesLength() {
            return 42 * (Shapes.Count + 1);
        }

        private void ShowFramebuffer(int fb) {
            GL.BindFramebuffer(FramebufferTarget.ReadFramebuffer, fb);
            GL.BindFramebuffer(FramebufferTarget.DrawFramebuffer, 0);
            GL.BlitFramebuffer(
                0, 0, 512, 512,
                0, 0, 512, 512,
                ClearBufferMask.ColorBufferBit,
                BlitFramebufferFilter.Linear
            );
            GL.BindFramebuffer(FramebufferTarget.Framebuffer, fb);
        }

    }
}
