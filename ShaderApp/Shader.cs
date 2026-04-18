using OpenTK.Graphics.OpenGL4;
using OpenTK.Mathematics;
using System.Windows;

namespace ShaderApp
{
    // A simple class meant to help create shaders.
    public class Shader
    {
        public readonly int ProgramHandle;
        private readonly Dictionary<string, int> uniformLocations;

        public Shader(string vertexCode, string fragmentCode)
        {
            // Create the Vertex shader 
            var vertexShader = GL.CreateShader(ShaderType.VertexShader);
            GL.ShaderSource(vertexShader, vertexCode);
            CompileShader(vertexShader);
            //  Create the Fragment shader
            var fragmentShader = GL.CreateShader(ShaderType.FragmentShader);
            GL.ShaderSource(fragmentShader, fragmentCode);
            CompileShader(fragmentShader);
            // Attach both shaders to a ShaderProgram...
            ProgramHandle = GL.CreateProgram();
            GL.AttachShader(ProgramHandle, vertexShader);
            GL.AttachShader(ProgramHandle, fragmentShader);
            LinkProgram(ProgramHandle);
            // When the shader program is linked, it no longer needs the individual shaders attached to it;
            // Detach them, and then delete them.
            GL.DetachShader(ProgramHandle, vertexShader);
            GL.DetachShader(ProgramHandle, fragmentShader);
            GL.DeleteShader(fragmentShader);
            GL.DeleteShader(vertexShader);
            // Get all the shader uniform locations for later use.
            GL.GetProgram(ProgramHandle, GetProgramParameterName.ActiveUniforms, out var numberOfUniforms);
            uniformLocations = new Dictionary<string, int>();
            for (var i = 0; i < numberOfUniforms; i++)
            {
                var key = GL.GetActiveUniform(ProgramHandle, i, out _, out _);
                var location = GL.GetUniformLocation(ProgramHandle, key);
                uniformLocations.Add(key, location);
            }
        }

        private static void CompileShader(int shader)
        {
            GL.CompileShader(shader);
            // Check for compilation errors
            GL.GetShader(shader, ShaderParameter.CompileStatus, out var code);
            if (code != (int)All.True)
            {
                var infoLog = GL.GetShaderInfoLog(shader);
                MessageBox.Show($"Error occurred whilst compiling Shader({shader}).\n\n{infoLog}");
            }
        }

        private static void LinkProgram(int program)
        {
            GL.LinkProgram(program);
            // Check for linking errors
            GL.GetProgram(program, GetProgramParameterName.LinkStatus, out var code);
            if (code != (int)All.True)
            {
                MessageBox.Show($"Error occurred whilst linking Program({program})");
            }
        }

        // A wrapper function that enables the shader program.
        public void Use()
        {
            GL.UseProgram(ProgramHandle);
        }

        public int GetAttribLocation(string attribName)
        {
            return GL.GetAttribLocation(ProgramHandle, attribName);
        }

        /// <summary>
        /// Set a uniform int on this shader.
        /// </summary>
        /// <param name="name">The name of the uniform</param>
        /// <param name="data">The data to set</param>
        public void SetInt(string name, int data)
        {
            if (uniformLocations.ContainsKey(name))
            {
                GL.UseProgram(ProgramHandle);
                GL.Uniform1(uniformLocations[name], data);
            }
        }

        /// <summary>
        /// Set a uniform float on this shader.
        /// </summary>
        /// <param name="name">The name of the uniform</param>
        /// <param name="data">The data to set</param>
        public void SetFloat(string name, float data)
        {
            if (uniformLocations.ContainsKey(name))
            {
                GL.UseProgram(ProgramHandle);
                GL.Uniform1(uniformLocations[name], data);
            }
        }

        /// <summary>
        /// Set a uniform Vector3 on this shader.
        /// </summary>
        /// <param name="name">The name of the uniform</param>
        /// <param name="data">The data to set</param>
        public void SetVector3(string name, Vector3 data)
        {
            if (uniformLocations.ContainsKey(name))
            {
                GL.UseProgram(ProgramHandle);
                GL.Uniform3(uniformLocations[name], data);
            }
        }

        /// <summary>
        /// Set a uniform Matrix3 on this shader
        /// </summary>
        /// <param name="name">The name of the uniform</param>
        /// <param name="data">The data to set</param>
        /// <param name="transpose">The matrix is transposed before being sent to the shader.</param>
        public void SetMatrix3(string name, bool transpose, Matrix3 data)
        {
            if (uniformLocations.ContainsKey(name))
            {
                GL.UseProgram(ProgramHandle);
                GL.UniformMatrix3(uniformLocations[name], transpose, ref data);
            }
        }

        /// <summary>
        /// Set a uniform Matrix4 on this shader
        /// </summary>
        /// <param name="name">The name of the uniform</param>
        /// <param name="data">The data to set</param>
        /// <param name="transpose">The matrix is transposed before being sent to the shader.</param>
        public void SetMatrix4(string name, bool transpose, Matrix4 data)
        {
            if (uniformLocations.ContainsKey(name))
            {
                GL.UseProgram(ProgramHandle);
                GL.UniformMatrix4(uniformLocations[name], transpose, ref data);
            }
        }
    }
}
