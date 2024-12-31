using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using OpenTK.Graphics.OpenGL4;

namespace EmblemEditor {
    internal class Shader {
        public int Handle;

        public Shader(string vertexPath, string fragmentPath) {
            // Load and compile vertex and fragment shaders
        }

        public Shader(string computePath) {
            // Load and compile compute shader
        }

        public void Use() {
            GL.UseProgram(Handle);
        }
    }

}
