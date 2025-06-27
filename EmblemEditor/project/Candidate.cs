using OpenTK.GLControl;
using OpenTK.Graphics.OpenGL4;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static System.Windows.Forms.VisualStyles.VisualStyleElement;
using System.Diagnostics;
using System.Reflection.Metadata;
using System.Reflection;
using OpenTK.Mathematics;
using System.DirectoryServices.ActiveDirectory;

namespace EmblemEditor {
    internal class Candidate {

        static GLControl Control;
        public static Texture Reference;

        public static void SetControl(GLControl control) { Control = control; }

        static Shader DrawSquares;
        static Shader DiffPass;

        static Shader AverageColorMask;
        static Shader AverageColorSample;

        public static Vector4 BackgroundColor;

        public static void Load() {
            if (!Control.Context.IsCurrent)
                Control.MakeCurrent();
            GL.Viewport(0, 0, Control.Width, Control.Height);
            BackgroundColor = new Vector4(0.746f, 0.6484f, 0.6133f, 1f);
            //BackgroundColor = new Vector4(0f, 0f, 0f, 1f);
            GL.ClearColor(BackgroundColor.X, BackgroundColor.Y, BackgroundColor.Z, BackgroundColor.W);
            //GL.ClearColor(0f, 0f, 0f, 1f);
            DrawSquares = new Shader("Shaders/drawsquares/vertex.vert", "Shaders/drawsquares/fragment.frag");
            DrawSquares.Use();
            DiffPass = new Shader("Shaders/diffpass/vertex.vert", "Shaders/diffpass/fragment.frag");
            DiffPass.Use();
            AverageColorMask = new Shader("Shaders/averagecolor/mask/vertex.vert", "Shaders/averagecolor/mask/fragment.frag");
            AverageColorMask.Use();
            AverageColorSample = new Shader("Shaders/averagecolor/sample/vertex.vert", "Shaders/averagecolor/sample/fragment.frag");
            AverageColorSample.Use();
        }

        public static Candidate Adjust(Candidate c) {
            return new Candidate(c, true);
        }

        public static Candidate New(Candidate c) {
            return new Candidate(c, false);
        }

        protected List<Shape> Shapes;
        protected Shape CurrentShape;

        public float Score;

        public Candidate() {
            Shapes = new List<Shape>();
            CurrentShape = new Shape();
            SetCurrentShapeColor();
        }

        private Candidate(Candidate c, bool adjust) {
            if (adjust) {
                Shapes = c.Shapes.ToList();
                CurrentShape = new Shape(c.CurrentShape);
            } else {
                Shapes = c.Shapes.ToList();
                Shapes.Add(c.CurrentShape);
                CurrentShape = new Shape();
            }
            SetCurrentShapeColor();
        }

        private void SetCurrentShapeColor() {
            if (!Control.Context.IsCurrent)
                Control.MakeCurrent();
            int maskTex = GL.GenTexture();
            GL.BindTexture(TextureTarget.Texture2D, maskTex);
            GL.TexImage2D(TextureTarget.Texture2D, 0, PixelInternalFormat.R32f, 512, 512, 0, PixelFormat.Red, PixelType.Float, IntPtr.Zero);
            GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMinFilter, (int)TextureMinFilter.Linear);
            GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMagFilter, (int)TextureMagFilter.Linear);

            int maskFbo = GL.GenFramebuffer();
            GL.BindFramebuffer(FramebufferTarget.Framebuffer, maskFbo);
            GL.FramebufferTexture2D(FramebufferTarget.Framebuffer, FramebufferAttachment.ColorAttachment0, TextureTarget.Texture2D, maskTex, 0);

            // Tell opengl which color attachments to draw to
            GL.DrawBuffers(1, new[] { DrawBuffersEnum.ColorAttachment0 });

            // Create VAO and bind
            int VAO = GL.GenVertexArray();
            GL.BindVertexArray(VAO);

            // Create VBO, bind, and upload data
            int VBO = GL.GenBuffer();
            GL.BindBuffer(BufferTarget.ArrayBuffer, VBO);
            GL.BufferData(BufferTarget.ArrayBuffer, 168, CurrentShape.BoundingBox().ToArray(), BufferUsageHint.StaticDraw); // VerticesLength() * 4 because sizeof(float) is 4

            // Tell VAO how to interpret the VBO
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

            // Unbind VBO
            GL.BindBuffer(BufferTarget.ArrayBuffer, 0);

            // Render
            GL.ClearColor(0f, 0f, 0f, 1f);
            GL.Clear(ClearBufferMask.ColorBufferBit);
            AverageColorMask.Use();
            GL.BindVertexArray(VAO);
            GL.DrawArrays(PrimitiveType.Triangles, 0, 42);
            GL.BindVertexArray(0);
            GL.ClearColor(BackgroundColor.X, BackgroundColor.Y, BackgroundColor.Z, BackgroundColor.W);

            // Generate mipmaps
            GL.BindTexture(TextureTarget.Texture2D, maskTex);
            GL.GenerateMipmap(GenerateMipmapTarget.Texture2D);

            // Blit and show
            //ShowFramebuffer(maskFbo); // blits onto back buffer
            //GL.BindFramebuffer(FramebufferTarget.Framebuffer, 0);
            //Control.SwapBuffers();

            int levels = (int)Math.Floor(Math.Log(Math.Max(512, 512), 2)) + 1;
            int topLevel = levels - 1;

            float[] result = new float[1];
            GL.GetTexImage(
                TextureTarget.Texture2D,
                topLevel,            // mipmap level to read
                PixelFormat.Red,
                PixelType.Float,
                result
            );

            float maskFillRatio = result[0];

            //Debug.WriteLine($"maskFillRatio: {maskFillRatio}");

            //#########################################

            int refTex = GL.GenTexture();
            GL.BindTexture(TextureTarget.Texture2D, refTex);
            GL.TexImage2D(TextureTarget.Texture2D, 0, PixelInternalFormat.Rgba32f, 512, 512, 0, PixelFormat.Rgba, PixelType.Float, IntPtr.Zero);
            GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMinFilter, (int)TextureMinFilter.Linear);
            GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMagFilter, (int)TextureMagFilter.Linear);

            int refFbo = GL.GenFramebuffer();
            GL.BindFramebuffer(FramebufferTarget.Framebuffer, refFbo);
            GL.FramebufferTexture2D(FramebufferTarget.Framebuffer, FramebufferAttachment.ColorAttachment0, TextureTarget.Texture2D, refTex, 0);

            // Tell opengl which color attachments to draw to
            GL.DrawBuffers(2, new[] { DrawBuffersEnum.ColorAttachment0, DrawBuffersEnum.ColorAttachment1 });

            GL.ClearColor(0f, 0f, 0f, 1f);
            GL.Clear(ClearBufferMask.ColorBufferBit);

            AverageColorSample.Use();
            GL.ActiveTexture(TextureUnit.Texture0);
            GL.BindTexture(TextureTarget.Texture2D, maskTex);
            AverageColorSample.SetInt("uMask", 0);

            GL.ActiveTexture(TextureUnit.Texture1);
            GL.BindTexture(TextureTarget.Texture2D, Reference.Handle);
            AverageColorSample.SetInt("uRef", 1);

            // 5. Make a fullscreen quad
            int VAO2 = GL.GenVertexArray();
            GL.BindVertexArray(VAO2);
            int VBO2 = GL.GenBuffer();
            GL.BindBuffer(BufferTarget.ArrayBuffer, VBO2);
            GL.BufferData(
                BufferTarget.ArrayBuffer,
                120, // (3 + 2) * 6 * sizeof(float)
                new float[] {
                    //Position        Texture coordinates
                    1f,  1f, 0.0f,    1.0f, 1.0f, // top right
                    1f, -1f, 0.0f,    1.0f, 0.0f, // bottom right
                    -1f,  1f, 0.0f,   0.0f, 1.0f,  // top left
                    
                    1f, -1f, 0.0f,    1.0f, 0.0f, // bottom right
                    -1f, -1f, 0.0f,   0.0f, 0.0f, // bottom left
                    -1f,  1f, 0.0f,   0.0f, 1.0f  // top left
                },
                BufferUsageHint.StaticDraw
            );

            // 6. Attrib pointers
            GL.EnableVertexAttribArray(0);
            GL.VertexAttribPointer(0, 3, VertexAttribPointerType.Float, false, 5 * sizeof(float), 0);
            GL.EnableVertexAttribArray(1);
            GL.VertexAttribPointer(1, 2, VertexAttribPointerType.Float, false, 5 * sizeof(float), 3 * sizeof(float));

            // 7. Render
            AverageColorSample.Use();
            GL.BindVertexArray(VAO2);
            GL.DrawArrays(PrimitiveType.Triangles, 0, 6);
            GL.BindVertexArray(0);
            GL.BindBuffer(BufferTarget.ArrayBuffer, 0);
            GL.ClearColor(BackgroundColor.X, BackgroundColor.Y, BackgroundColor.Z, BackgroundColor.W);

            // Generate mipmaps
            GL.BindTexture(TextureTarget.Texture2D, refTex);
            GL.GenerateMipmap(GenerateMipmapTarget.Texture2D);

            // Blit and show
            //ShowFramebuffer(refFbo); // blits onto back buffer
            //GL.BindFramebuffer(FramebufferTarget.Framebuffer, 0);
            //Control.SwapBuffers();

            result = new float[4];
            GL.GetTexImage(
                TextureTarget.Texture2D,
                topLevel,            // mipmap level to read
                PixelFormat.Rgba,
                PixelType.Float,
                result
            );

            Vector4 maskColorAverage = new Vector4(result[0], result[1], result[2], result[3]);

            //Debug.WriteLine($"maskColorAverage: {maskColorAverage}");

            CurrentShape.Color = new Vector4(
                maskColorAverage.X / maskFillRatio,
                maskColorAverage.Y / maskFillRatio,
                maskColorAverage.Z / maskFillRatio,
                maskColorAverage.W / maskFillRatio
            );

            // DISPOSE OF EVERYTHING
            GL.BindVertexArray(0);
            GL.BindBuffer(BufferTarget.ArrayBuffer, 0);
            GL.BindTexture(TextureTarget.Texture2D, 0);
            GL.BindFramebuffer(FramebufferTarget.Framebuffer, 0);
            GL.DeleteVertexArray(VAO);
            GL.DeleteVertexArray(VAO2);
            GL.DeleteBuffer(VBO);
            GL.DeleteBuffer(VBO2);
            GL.DeleteTexture(maskTex);
            GL.DeleteTexture(refTex);
            GL.DeleteFramebuffer(maskFbo);
            GL.DeleteFramebuffer(refFbo);

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

            if (render) {
                // 9.5. Put onto back buffer
                ShowFramebuffer(fbo); // blits onto back buffer
                GL.BindFramebuffer(FramebufferTarget.Framebuffer, 0);

                // 10. Show on screen
                Control.SwapBuffers();
            }

            //##############################

            // 1. Create Difference FBO
            int diffFbo = GL.GenFramebuffer();
            GL.BindFramebuffer(FramebufferTarget.Framebuffer, diffFbo);

            // 2. Create Difference texture
            int diffTex = GL.GenTexture();
            GL.BindTexture(TextureTarget.Texture2D, diffTex);
            GL.TexImage2D(TextureTarget.Texture2D, 0, PixelInternalFormat.R32f, 512, 512, 0, PixelFormat.Red, PixelType.Float, IntPtr.Zero);
            GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMinFilter, (int)TextureMinFilter.LinearMipmapLinear);
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
            int fsVAO = GL.GenVertexArray();
            GL.BindVertexArray(fsVAO);
            int fsVBO = GL.GenBuffer();
            GL.BindBuffer(BufferTarget.ArrayBuffer, fsVBO);
            GL.BufferData(
                BufferTarget.ArrayBuffer,
                120, // (3 + 2) * 6 * sizeof(float)
                new float[] {
                    //Position        Texture coordinates
                    1f,  1f, 0.0f,    1.0f, 1.0f, // top right
                    1f, -1f, 0.0f,    1.0f, 0.0f, // bottom right
                    -1f,  1f, 0.0f,   0.0f, 1.0f,  // top left
                    
                    1f, -1f, 0.0f,    1.0f, 0.0f, // bottom right
                    -1f, -1f, 0.0f,   0.0f, 0.0f, // bottom left
                    -1f,  1f, 0.0f,   0.0f, 1.0f  // top left
                },
                BufferUsageHint.StaticDraw
            );

            // 6. Attrib pointers
            GL.EnableVertexAttribArray(0);
            GL.VertexAttribPointer(0, 3, VertexAttribPointerType.Float, false, 5 * sizeof(float), 0);
            GL.EnableVertexAttribArray(1);
            GL.VertexAttribPointer(1, 2, VertexAttribPointerType.Float, false, 5 * sizeof(float), 3 * sizeof(float));

            // 7. Render
            DiffPass.Use();
            GL.BindVertexArray(fsVAO);
            GL.DrawArrays(PrimitiveType.Triangles, 0, 6);
            GL.BindVertexArray(0);
            GL.BindBuffer(BufferTarget.ArrayBuffer, 0);

            // 8. Generate mipmaps
            GL.BindTexture(TextureTarget.Texture2D, diffTex);
            GL.GenerateMipmap(GenerateMipmapTarget.Texture2D);

            // 9. Show on back buffer
            //ShowFramebuffer(diffFbo); // blits onto back buffer
            //GL.BindFramebuffer(FramebufferTarget.Framebuffer, 0);
            //Control.SwapBuffers();

            int levels = (int)Math.Floor(Math.Log(Math.Max(512, 512), 2)) + 1;
            int topLevel = levels - 1;

            float[] result = new float[1];
            GL.GetTexImage(
                TextureTarget.Texture2D,
                topLevel,            // mipmap level to read
                PixelFormat.Red,
                PixelType.Float,
                result
            );

            Score = result[0];

            // DISPOSE OF EVERYTHING
            GL.BindVertexArray(0);
            GL.BindBuffer(BufferTarget.ArrayBuffer, 0);
            GL.BindTexture(TextureTarget.Texture2D, 0);
            GL.BindFramebuffer(FramebufferTarget.Framebuffer, 0);
            GL.DeleteVertexArray(VAO);
            GL.DeleteVertexArray(fsVAO);
            GL.DeleteBuffer(VBO);
            GL.DeleteBuffer(fsVBO);
            GL.DeleteTexture(colorTex);
            GL.DeleteTexture(diffTex);
            GL.DeleteFramebuffer(fbo);
            GL.DeleteFramebuffer(diffFbo);

            //Debug.WriteLine("Final result (1x1 mipmap value): " + Score);

        }

        public void Show() {
            if (!Control.Context.IsCurrent)
                Control.MakeCurrent();

            // 1. Create VAO and bind
            int VAO = GL.GenVertexArray();
            GL.BindVertexArray(VAO);

            // 2. Create VBO, bind, and upload data
            int VBO = GL.GenBuffer();
            GL.BindBuffer(BufferTarget.ArrayBuffer, VBO);
            GL.BufferData(BufferTarget.ArrayBuffer, VerticesLength() * 4, Vertices(), BufferUsageHint.StaticDraw); // VerticesLength() * 4 because sizeof(float) is 4

            // 3. Tell VAO how to interpret the VBO
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

            // 4. Unbind VBO
            GL.BindBuffer(BufferTarget.ArrayBuffer, 0);

            // 5. Render
            GL.Clear(ClearBufferMask.ColorBufferBit);
            DrawSquares.Use();
            GL.BindVertexArray(VAO);
            GL.DrawArrays(PrimitiveType.Triangles, 0, VerticesLength());
            GL.BindVertexArray(0);

            // 10. Show on screen
            Control.SwapBuffers();

            // DISPOSE OF EVERYTHING
            GL.BindVertexArray(0);
            GL.BindBuffer(BufferTarget.ArrayBuffer, 0);
            GL.BindTexture(TextureTarget.Texture2D, 0);
            GL.BindFramebuffer(FramebufferTarget.Framebuffer, 0);
            GL.DeleteVertexArray(VAO);
            GL.DeleteBuffer(VBO);
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
