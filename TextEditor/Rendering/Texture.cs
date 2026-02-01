using OpenTK.Graphics.OpenGL;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;


namespace TextEditor.Rendering;

/// <summary>
/// Encapsulates an OpenGL texture object, handling image loading via SixLabors.ImageSharp 
/// and management of GPU texture resources.
/// </summary>
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

    /// <summary>
    /// Loads an image from the specified file path and uploads the pixel data to the GPU.
    /// </summary>
    /// <param name="path">The relative or absolute path to the image file.</param>
    /// <returns>A new <see cref="Texture"/> instance representing the uploaded image.</returns>
    /// <remarks>
    /// This method configures the texture with <see cref="TextureMinFilter.Nearest"/> and 
    /// <see cref="TextureMagFilter.Nearest"/> to ensure pixel-perfect rendering for bitmap fonts.
    /// </remarks>
    public static Texture LoadFromFile(string path)
    {
        using Image<Rgba32> image = Image.Load<Rgba32>(path);

        int width = image.Width;
        int height = image.Height;

        Console.WriteLine($"Loaded image {path} ({width}x{height})");
        var p = image[0, 0];
        Console.WriteLine($"BG pixel: {p.R},{p.G},{p.B},{p.A}");

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
        
        // Configuration for UI and Bitmap Fonts: 
        // Clamp edges to prevent bleeding and use Nearest filtering for sharpness.
        GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureWrapS, (int)TextureWrapMode.ClampToEdge);
        GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureWrapT, (int)TextureWrapMode.ClampToEdge);
        GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMinFilter, (int)TextureMinFilter.Nearest);
        GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMagFilter, (int)TextureMagFilter.Nearest);

        GL.BindTexture(TextureTarget.Texture2D, 0);
        
        return new Texture(handle, width, height);
    }

    /// <summary>
    /// Binds the texture to a specific OpenGL texture unit.
    /// </summary>
    /// <param name="unit">The <see cref="TextureUnit"/> to bind to. Defaults to <see cref="TextureUnit.Texture0"/>.</param>
    public void Bind(TextureUnit unit = TextureUnit.Texture0)
    {
        GL.ActiveTexture(unit);
        GL.BindTexture(TextureTarget.Texture2D, Handle);
    }

}