using System.Diagnostics;
using OpenTK.Graphics.OpenGL;
using OpenTK.Mathematics;

namespace TextEditor.Rendering;

/// <summary>
/// Responsible for all 2D drawing operations such as glyphs,
/// strings, rectangles, and textured quads.
/// </summary>
public sealed class TextRenderer
{
    private readonly int _shaderProgram;
    private readonly int _vao;
    private readonly int _vbo;
    private readonly BitmapFont _bitmapFont;
    
    public TextRenderer(
        int shaderProgram,
        int vao,
        int vbo,
        BitmapFont bitmapFont)
    {
        _shaderProgram = shaderProgram;
        _vao = vao;
        _vbo = vbo;
        _bitmapFont = bitmapFont;
    }
    
    /// <summary>
    /// Renders a solid color rectangle using two triangles.
    /// </summary>
    /// <param name="x">The X screen coordinate.</param>
    /// <param name="y">The Y screen coordinate.</param>
    /// <param name="width">Width in pixels.</param>
    /// <param name="height">Height in pixels.</param>
    /// <param name="color">The RGB color of the rectangle.</param>
    public void DrawRect(float x, float y, float width, float height, Vector3 color)
    {
        float[] vertices = {
            x, y,
            x + width, y,
            x + width, y + height,
            x, y,
            x + width, y + height,
            x, y + height
        };

        GL.BindBuffer(BufferTarget.ArrayBuffer, _vbo);
        GL.BufferData(BufferTarget.ArrayBuffer, vertices.Length * sizeof(float), vertices, BufferUsageHint.DynamicDraw);

        int colorLoc = GL.GetUniformLocation(_shaderProgram, "color");
        GL.Uniform3(colorLoc, color);

        GL.DrawArrays(PrimitiveType.Triangles, 0, 6);
    }
    
    /// <summary>
    /// Renders a textured rectangle (sprite) to the screen.
    /// </summary>
    /// <param name="x">The X screen coordinate.</param>
    /// <param name="y">The Y screen coordinate.</param>
    /// <param name="width">Width in pixels.</param>
    /// <param name="height">Height in pixels.</param>
    /// <param name="texture">The <see cref="Texture"/> to apply.</param>
    public void DrawTexturedRect(float x, float y, float width, float height, Texture texture)
    {
        float[] vertices =
        {
            x, y, 0f, 0f,
            x + width, y, 1f, 0f,
            x + width, y + height, 1f, 1f,

            x, y, 0f, 0f,
            x + width, y + height, 1f, 1f,
            x, y + height, 0f, 1f
        };

        GL.BindBuffer(BufferTarget.ArrayBuffer, _vbo);
        GL.BufferData(BufferTarget.ArrayBuffer, vertices.Length * sizeof(float), vertices, BufferUsageHint.DynamicDraw);

        texture.Bind(TextureUnit.Texture0);
        GL.Uniform1(GL.GetUniformLocation(_shaderProgram, "tex"), 0);
        GL.Uniform1(GL.GetUniformLocation(_shaderProgram, "renderMode"), 0);

        GL.DrawArrays(PrimitiveType.Triangles, 0, 6);
    }

    /// <summary>
    /// Draws a single character from the bitmap font at the specified screen position.
    /// </summary>
    /// <param name="x">The X screen coordinate.</param>
    /// <param name="y">The Y screen coordinate.</param>
    /// <param name="c">The character to draw.</param>
    /// <param name="scale">Optional scale factor (default 1.0 for pixel-perfect rendering).</param>
    public void DrawGlyph(float x, float y, char c, float scale = 1.0f)
    {
        Glyph glyph = _bitmapFont.GetGlyph(c);

        float width = _bitmapFont.CellWidth * scale;
        float height = _bitmapFont.CellHeight * scale;

        float[] vertices =
        {
            x, y, glyph.U0, glyph.V0,
            x + width, y, glyph.U1, glyph.V0,
            x + width, y + height, glyph.U1, glyph.V1,

            x, y, glyph.U0, glyph.V0,
            x + width, y + height, glyph.U1, glyph.V1,
            x, y + height, glyph.U0, glyph.V1
        };

        GL.BindBuffer(BufferTarget.ArrayBuffer, _vbo);
        GL.BufferData(BufferTarget.ArrayBuffer, vertices.Length * sizeof(float), vertices, BufferUsageHint.DynamicDraw);

        _bitmapFont.Texture.Bind(TextureUnit.Texture0);
        GL.Uniform1(GL.GetUniformLocation(_shaderProgram, "tex"), 0);
        GL.Uniform1(GL.GetUniformLocation(_shaderProgram, "renderMode"), 1);

        GL.DrawArrays(PrimitiveType.Triangles, 0, 6);
    }

    /// <summary>
    /// Draws a string of text at the specified screen position using the bitmap font.
    /// </summary>
    /// <param name="x">
    /// The X screen coordinate (in pixels) where the text begins.
    /// This corresponds to the left edge of the first character.
    /// </param>
    /// <param name="y">
    /// The Y screen coordinate (in pixels) where the text begins.
    /// This corresponds to the top edge of the text baseline.
    /// </param>
    /// <param name="text">
    /// The string to render.
    /// </param>
    /// <param name="scale">
    /// Optional scale factor applied uniformly to all glyphs.
    /// A value of 1.0 renders pixel-perfect text.
    /// </param>
    /// <remarks>
    /// This method is responsible only for positioning and iterating
    /// over glyphs. Actual glyph rendering should be delegated to
    /// <see cref="DrawGlyph"/> to keep responsibilities separated.
    /// </remarks>
    public void DrawString(float x, float y, string text, float scale = 1.0f, float lineSpacing = 1.0f)
    {
        float penX = x;
        float penY = y;

        scale = MathF.Round(scale);
        scale = Math.Max(scale, 1.0f);

        Debug.Assert(scale % 1.0f == 0.0f,
            "Bitmap fonts only support integer scaling.");

        float glyphWidth = _bitmapFont.CellWidth * scale;
        float glyphHeight = _bitmapFont.CellHeight * scale;
        float lineAdvance = glyphHeight * lineSpacing;

        foreach (char c in text)
        {
            if (c == '\n')
            {
                penX = x;
                penY += lineAdvance;
                continue;
            }

            float snappedX = MathF.Round(penX);
            float snappedY = MathF.Round(penY);

            DrawGlyph(snappedX, snappedY, c, scale);
            penX += glyphWidth;
        }
    }
}