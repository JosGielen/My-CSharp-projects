using OpenTK.Graphics.OpenGL4;
using StbImageSharp;
using System.IO;
using System.Windows;

namespace OpenTK_WPF
{
    internal class Cubemap
    {
        public readonly int Handle;

        // Create Cubemap from file paths.
        public static Cubemap LoadFromFiles(List<string> imagePaths)
        {
            // Generate handle
            int handle = GL.GenTexture();
            GL.ActiveTexture(TextureUnit.Texture0);
            GL.BindTexture(TextureTarget.TextureCubeMap, handle);
            //TextureTargets for the 6 individual images that make up a Cubemap
            TextureTarget[] targets =
            {
                TextureTarget.TextureCubeMapPositiveX, //Left
                TextureTarget.TextureCubeMapNegativeX, //Right
                TextureTarget.TextureCubeMapPositiveY, //Top
                TextureTarget.TextureCubeMapNegativeY, //Bottom
                TextureTarget.TextureCubeMapPositiveZ, //Back
                TextureTarget.TextureCubeMapNegativeZ  //Front
            };
            //Load the 6 images from the files
            for (int i = 0; i < imagePaths.Count; i++)
            {
                try
                {
                    using (Stream stream = File.OpenRead(imagePaths[i]))
                    {
                        ImageResult image = ImageResult.FromStream(stream, ColorComponents.RedGreenBlueAlpha);
                        GL.TexImage2D(targets[i], 0, PixelInternalFormat.Rgba, image.Width, image.Height, 0, PixelFormat.Rgba, PixelType.UnsignedByte, image.Data);
                    }
                }
                catch
                {
                    MessageBox.Show("Cubemap files not found.");
                }
            }
            // Settings to affect how the image appears on rendering.
            GL.TexParameter(TextureTarget.TextureCubeMap, TextureParameterName.TextureMinFilter, (int)TextureMinFilter.Linear);
            GL.TexParameter(TextureTarget.TextureCubeMap, TextureParameterName.TextureMagFilter, (int)TextureMagFilter.Linear);
            GL.TexParameter(TextureTarget.TextureCubeMap, TextureParameterName.TextureWrapS, (int)TextureWrapMode.ClampToEdge);
            GL.TexParameter(TextureTarget.TextureCubeMap, TextureParameterName.TextureWrapT, (int)TextureWrapMode.ClampToEdge);
            GL.TexParameter(TextureTarget.TextureCubeMap, TextureParameterName.TextureWrapR, (int)TextureWrapMode.ClampToEdge);
            return new Cubemap(handle);
        }

        public Cubemap(int glHandle)
        {
            Handle = glHandle;
        }

        public void Use(TextureUnit unit)
        {
            GL.ActiveTexture(unit);
            GL.BindTexture(TextureTarget.TextureCubeMap, Handle);
        }
    }
}
