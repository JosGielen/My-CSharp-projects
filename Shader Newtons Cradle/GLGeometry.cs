using System;
using System.Windows.Media.Media3D;
using System.Collections.Generic;
using SharpGL;
using SharpGL.Shaders;
using SharpGL.VertexBuffers;
using SharpGL.Enumerations;
using GlmNet;

namespace Shader_Newtons_Cradle
{
    public abstract class GLGeometry
    {
        protected OpenGL my_OpenGL;
        protected int my_VertexCount;
        protected Vector3D[] my_Vertices;
        protected int[] my_Indices;
        protected VertexBufferArray my_VertexBufferArray;
        private VertexBuffer vertexBuffer;
        private IndexBuffer indexBuffer;
        protected mat4 my_ModelMatrix = mat4.identity();
        protected mat4 my_ViewMatrix = mat4.identity();
        protected mat3 my_NormalMatrix = mat3.identity();
        private vec3 my_Position = new vec3(0, 0, 0);
        //Drawing Mode
        private DrawMode my_DrawMode = DrawMode.Fill;
        protected BeginMode GLBeginMode = BeginMode.Triangles;

        #region "Properties"

        public Vector3D Position
        {
            get { return new Vector3D(my_Position.x, my_Position.y, my_Position.z); }
            set { my_Position = new vec3((float)value.X, (float)value.Y, (float)value.Z); }
        }

        public Vector3D[] Vertices
        {
            get { return my_Vertices; }
        }

        public int[] Indices
        {
            get { return my_Indices; }
        }

        public DrawMode DrawMode
        {
            get { return my_DrawMode; }
            set { my_DrawMode = value; }
        }

        #endregion 


        public void GenerateGeometry(GLScene scene)
        {
            my_OpenGL = scene.GL;
            my_VertexBufferArray = new VertexBufferArray();
            my_VertexBufferArray.Create(my_OpenGL);
            my_VertexBufferArray.Bind(my_OpenGL);
            CreateVertexBuffer(GLScene.positionAttribute);
            CreateIndexBuffer();
            my_VertexBufferArray.Unbind(my_OpenGL);
        }

        public void Draw(ShaderProgram shader)
        {
            shader.SetUniformMatrix4(my_OpenGL, "Model", GetModelMatrix().to_array());
            shader.SetUniformMatrix3(my_OpenGL, "NormalMatrix", GetNormalMatrix().to_array());
            my_VertexBufferArray.Bind(my_OpenGL);
            switch (my_DrawMode)
            {
                case DrawMode.Fill:
                    my_OpenGL.PolygonMode(FaceMode.FrontAndBack, PolygonMode.Filled);
                    break;
                case DrawMode.Lines:
                    my_OpenGL.PolygonMode(FaceMode.FrontAndBack, PolygonMode.Lines);
                    break;
                case DrawMode.Points:
                    my_OpenGL.PolygonMode(FaceMode.FrontAndBack, PolygonMode.Points);
                    break;
            }
            switch (GLBeginMode)
            {
                case BeginMode.Triangles:
                    my_OpenGL.DrawElements(OpenGL.GL_TRIANGLES, my_Indices.Length, OpenGL.GL_UNSIGNED_INT, IntPtr.Zero);
                    break;
                case BeginMode.Lines:
                    my_OpenGL.DrawElements(OpenGL.GL_LINES, my_Indices.Length, OpenGL.GL_UNSIGNED_INT, IntPtr.Zero);
                    break;
                case BeginMode.Points:
                    my_OpenGL.DrawElements(OpenGL.GL_POINTS, my_Indices.Length, OpenGL.GL_UNSIGNED_INT, IntPtr.Zero);
                    break;
            }
            my_VertexBufferArray.Unbind(my_OpenGL);
        }

        private void CreateVertexBuffer(uint vertexAttributeLocation)
        {
            CreateVertices();
            vertexBuffer = new VertexBuffer();
            vertexBuffer.Create(my_OpenGL);
            vertexBuffer.Bind(my_OpenGL);
            List<float> vData = new List<float>();
            foreach (Vector3D v in my_Vertices)
            {
                vData.Add((float)v.X);
                vData.Add((float)v.Y);
                vData.Add((float)v.Z);
            }
            vertexBuffer.SetData(my_OpenGL, vertexAttributeLocation, vData.ToArray(), false, 3);
        }

        private void CreateIndexBuffer()
        {
            CreateIndices();
            indexBuffer = new IndexBuffer();
            indexBuffer.Create(my_OpenGL);
            indexBuffer.Bind(my_OpenGL);
            uint[] indis = new uint[my_Indices.Length];
            for (int I = 0; I < my_Indices.Length; I++)
            {
                indis[I] = (uint)my_Indices[I];
            }
            indexBuffer.SetData(my_OpenGL, indis);
        }

        protected virtual mat4 GetModelMatrix()
        {
            mat4 translation = glm.translate(mat4.identity(), my_Position);
            my_ModelMatrix = translation;
            return my_ModelMatrix;
        }

        protected virtual mat3 GetNormalMatrix()
        {
            return GetModelMatrix().to_mat3();
        }

        public abstract void CreateVertices();

        public abstract void CreateIndices();
    }
}

public enum DrawMode
{
    Points = 0,
    Lines = 1,
    Fill = 2
}

public enum GLDrawMode
{
    Points = 0,
    Lines = 1,
    Triangles = 2
}
