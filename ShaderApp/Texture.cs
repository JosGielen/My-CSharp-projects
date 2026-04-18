using OpenTK.Graphics.OpenGL4;
using StbImageSharp;
using System.IO;
using System.Windows;
using System.Windows.Media.Imaging;

namespace ShaderApp
{
    public class Texture
    {
        public readonly int Handle;

        public static Texture LoadFromFile(string path)
        {
            //Generate and bind a Texture
            int handle = GL.GenTexture();
            GL.ActiveTexture(TextureUnit.Texture0);
            GL.BindTexture(TextureTarget.Texture2D, handle);
            //Load the texture from a file
            try
            {
                //BitmapImage bmp = new BitmapImage(new Uri(path));
                //int Stride = bmp.PixelWidth * bmp.Format.BitsPerPixel / 8;
                //byte[] pixelData = new byte[Stride * bmp.PixelHeight];
                //bmp.CopyPixels(pixelData, Stride, 0);
                //GL.TexImage2D(TextureTarget.Texture2D, 0, PixelInternalFormat.Rgba, bmp.PixelWidth, bmp.PixelHeight, 0, PixelFormat.Rgba, PixelType.UnsignedByte, pixelData);
                using (Stream stream = File.OpenRead(path))
                {
                    ImageResult image = ImageResult.FromStream(stream, ColorComponents.RedGreenBlueAlpha);
                    GL.TexImage2D(TextureTarget.Texture2D, 0, PixelInternalFormat.Rgba, image.Width, image.Height, 0, PixelFormat.Rgba, PixelType.UnsignedByte, image.Data);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Texture file not found");
            }
            // Settings to affect how the image appears on rendering.
            GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMinFilter, (int)TextureMinFilter.Linear);
            GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMagFilter, (int)TextureMagFilter.Linear);
            GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureWrapS, (int)TextureWrapMode.Repeat);
            GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureWrapT, (int)TextureWrapMode.Repeat);
            //Generate mipmaps.
            GL.GenerateMipmap(GenerateMipmapTarget.Texture2D);
            return new Texture(handle);
        }

        public Texture(int glHandle)
        {
            Handle = glHandle;
        }

        public void Use(TextureUnit unit)
        {
            GL.ActiveTexture(unit);
            GL.BindTexture(TextureTarget.Texture2D, Handle);
        }
    }
}
