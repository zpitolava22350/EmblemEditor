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

        public static void Load() {
            if (!Control.Context.IsCurrent)
                Control.MakeCurrent();
            GL.Viewport(0, 0, Control.Width, Control.Height);
            GL.ClearColor(0.2f, 0.3f, 0.3f, 1f);
            DrawSquares = new Shader("Shaders/drawsquares/vertex.vert", "Shaders/drawsquares/fragment.frag");
            DrawSquares.Use();
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
                index: 0, // (location = 1)
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
            GL.DrawArrays(PrimitiveType.Triangles, 0, VerticesLength());

            // 6. Show on screen
            Control.SwapBuffers();

            // 7. Dispose of the VAO
            GL.BindVertexArray(0);
            GL.DeleteVertexArray(VAO);



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

    }
}
