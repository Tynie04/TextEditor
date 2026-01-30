using OpenTK.Graphics.OpenGL;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;


namespace TextEditor.Rendering;

public sealed class Texture
{
    public int Handle { get; }
    public int Width { get; }
    public int Height { get; }

    private Texture(int handle, int width, int height)
    {
        Handle = handle;
        Width = width;
        Height = height;
    }

    // Loads an image file and uploads it to OpenGL
    public static Texture LoadFromFile(string path)
    {
        using Image<Rgba32> image = Image.Load<Rgba32>(path);

        int width = image.Width;
        int height = image.Height;

        Console.WriteLine($"Loaded image {path} ({width}x{height})");

        byte[] pixels = new byte[width * height * 4];
        image.CopyPixelDataTo(pixels);
        
        Console.WriteLine($"Extracted {pixels.Length} bytes of pixel data");

        int handle = GL.GenTexture();
        GL.BindTexture(TextureTarget.Texture2D, handle);
        
        GL.TexImage2D(
            TextureTarget.Texture2D,
            level: 0,
            internalformat: PixelInternalFormat.Rgba,
            width,
            height,
            border: 0,
            format: PixelFormat.Rgba,
            type: PixelType.UnsignedByte,
            pixels
        );
        
        GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureWrapS, (int)TextureWrapMode.ClampToEdge);
        GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureWrapT, (int)TextureWrapMode.ClampToEdge);
        GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMinFilter, (int)TextureMinFilter.Nearest);
        GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMagFilter, (int)TextureMagFilter.Nearest);

        GL.BindTexture(TextureTarget.Texture2D, 0);
        
        return new Texture(handle, width, height);
    }

    public void Bind(TextureUnit unit = TextureUnit.Texture0)
    {
        GL.ActiveTexture(unit);
        GL.BindTexture(TextureTarget.Texture2D, Handle);
    }

}