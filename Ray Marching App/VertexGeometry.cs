using System;
using System.Collections.Generic;
using System.IO;
using System.Windows;
using System.Windows.Media.Media3D;

namespace JG_GL
{
    public class VertexGeometry : GLGeometry
    {
        private List<int> my_XYIndices = new List<int>();
        private List<int> my_XZIndices = new List<int>();
        private List<int> my_YZIndices = new List<int>();
        private double my_Scale;
        private double my_XScale;
        private double my_YScale;
        private double my_ZScale;
        private string my_VertexFile;
        private List<JG_Vertex> my_JG_Verteces;

        public VertexGeometry()
        {
            my_Scale = 1.0;
            my_XScale = 1.0;
            my_YScale = 1.0;
            my_ZScale = 1.0;
            my_VertexFile = "";
            my_JG_Verteces = new List<JG_Vertex>();
            my_VertexCount = 0;
        }

        public VertexGeometry(double scale)
        {
            my_Scale = scale;
            my_XScale = scale;
            my_YScale = scale;
            my_ZScale = scale;
            my_VertexFile = "";
            my_JG_Verteces = new List<JG_Vertex>();
            my_VertexCount = 0;
        }

        public VertexGeometry(double scale, string Vertexfile)
        {
            my_Scale = scale;
            my_XScale = scale;
            my_YScale = scale;
            my_ZScale = scale;
            my_VertexFile = VertexFile;
            my_JG_Verteces = GetVerteces(Vertexfile);
            my_VertexCount = my_JG_Verteces.Count;
        }

        public List<int> XYIndices
        {
            get { return my_XYIndices; }
        }

        public List<int> XZIndices
        {
            get { return my_XZIndices; }
        }

        public List<int> YZIndices
        {
            get { return my_YZIndices; }
        }

        public double Scale
        {
            get { return my_Scale; }
            set
            {
                my_Scale = value;
                my_XScale = value;
                my_YScale = value;
                my_ZScale = value;
            }
        }

        public double XScale
        {
            get { return my_XScale; }
            set { my_XScale = value; }
        }

        public double YScale
        {
            get { return my_YScale; }
            set { my_YScale = value; }
        }

        public double ZScale
        {
            get { return my_ZScale; }
            set { my_ZScale = value; }
        }


        public string VertexFile
        {
            get { return my_VertexFile; }
            set
            {
                my_VertexFile = value;
                my_JG_Verteces = GetVerteces(my_VertexFile);
                my_VertexCount = my_JG_Verteces.Count;
            }
        }

        public override void CreateVertices()
        {
            my_Vertices = new Vector3D[my_JG_Verteces.Count];
            //Calculate the Verteces from each JG_Vertex, corrected for scale
            for (int I = 0; I < my_JG_Verteces.Count; I++)
            {
                my_Vertices[I] = new Vector3D(my_XScale * my_JG_Verteces[I].Vert.X, my_YScale * my_JG_Verteces[I].Vert.Y, my_ZScale * my_JG_Verteces[I].Vert.Z);
            }
        }

        public override void CreateIndices()
        {
            my_Indices = new int[my_VertexCount];
            for (int I = 0; I < my_Vertices.Length; I++)
            {
                my_Indices[I] = I;
            }
        }

        private List<JG_Vertex> GetVerteces(string file)
        {
            //File Format:
            //JG_Vertex File : Description
            //  Vertex Data
            //      Normal: xxxx,yyyy,zzzz
            //      Vertex: xxxx,yyyy,zzzz
            //      TexCoord: xxxx,yyyy
            //  End Vertex Data
            //  ....
            //End File
            List<JG_Vertex > result = new List<JG_Vertex>();
            StreamReader sr;
            string S1;
            string S2;
            string S3;
            string my_line;
            sr = new StreamReader(file);
            while (!sr.EndOfStream)
            {
                my_line = sr.ReadLine();
                if (my_line.Contains("Vertex Data"))
                {

                    S1 = sr.ReadLine().TrimStart(); //Normal Vector
                    S1 = S1.Substring(8, S1.Length - 8);
                    S2 = sr.ReadLine().TrimStart(); //Vertex;
                    S2 = S2.Substring(8, S2.Length - 8);
                    S3 = sr.ReadLine().TrimStart(); //TextureCoordinate
                    S3 = S3.Substring(10, S3.Length - 10);
                    sr.ReadLine();
                    result.Add(new JG_Vertex(S1, S2, S3, ' '));
                }
            }
            return result;
        }
    }

    public struct JG_Vertex
    {
        public Vector3D Norm;
        public Vector3D Vert;
        public Vector Tex;

        public JG_Vertex(Vector3D n, Vector3D v, Vector t)
        {
            Norm = n;
            Vert = v;
            Tex = t;
        }

        public JG_Vertex(string n, string v, string t, char delimit)
        {
            string[] parts1 = n.Split(delimit);
            string[] parts2 = v.Split(delimit);
            string[] parts3 = t.Split(delimit);
            double X, Y, Z;
            if (parts1.Length != 3 | parts2.Length != 3 | parts3.Length != 2)
            {
                throw new Exception("Invalid JG_Vertex string Format");
            }
            try
            {
                X = double.Parse(parts1[0]);
                Y = double.Parse(parts1[1]);
                Z = double.Parse(parts1[2]);
                Norm = new Vector3D(X, Y, Z);
                X = double.Parse(parts2[0]);
                Y = double.Parse(parts2[1]);
                Z = double.Parse(parts2[2]);
                Vert = new Vector3D(X, Y, Z);
                X = double.Parse(parts3[0]);
                Y = double.Parse(parts3[1]);
                Tex = new Vector(X, Y);
            }
            catch
            {
                throw new Exception("Invalid JG_Vertex string Format");
            }
        }
    }

}
