namespace TextEditor.Rendering;

/// <summary>
/// Represents a bitmap-based font loaded from a texture atlas grid.
/// </summary>
public sealed class BitmapFont
{
    public Texture Texture { get; }
    
    public int CellWidth { get; }
    public int CellHeight { get; }
    
    public int Columns { get; }
    
    public int Rows { get; }

    private BitmapFont(
        Texture texture,
        int cellWidth,
        int cellHeight,
        int columns,
        int rows)
    {
        Texture = texture;
        CellWidth = cellWidth;
        CellHeight = cellHeight;
        Columns = columns;
        Rows = rows;
    }
    
    /// <summary>
    /// Loads a bitmap font atlas from a file and calculates grid dimensions.
    /// </summary>
    /// <param name="imagePath">The file path to the font texture (e.g., a PNG atlas).</param>
    /// <param name="cellWidth">The width of a single character cell in pixels.</param>
    /// <param name="cellHeight">The height of a single character cell in pixels.</param>
    /// <returns>A new <see cref="BitmapFont"/> instance.</returns>
    public static BitmapFont Load(
        string imagePath,
        int cellWidth,
        int cellHeight)
    {
        Texture texture = Texture.LoadFromFile(imagePath);

        int columns = texture.Width / cellWidth;
        int rows = texture.Height / cellHeight;

        return new BitmapFont(
            texture,
            cellWidth,
            cellHeight,
            columns,
            rows);
    }
    
    /// <summary>
    /// Calculates the UV coordinates for a specific character based on the GNU Unifont 
    /// 256x256 grid layout (High-byte = Row, Low-byte = Column).
    /// </summary>
    /// <param name="c">The character to retrieve.</param>
    /// <returns>A <see cref="Glyph"/> containing normalized UV coordinates.</returns>
    public Glyph GetGlyph(char c)
    {
        if (c < 32 || c > 126)
        {
            c = '?';
        }

        int codepoint = c;

        int column = codepoint & 0xFF;
        int row = codepoint >> 8;

        int pixelX = column * CellWidth;
        int pixelY = row * CellHeight;

        float u0 = (float)pixelX / Texture.Width;
        float v0 = (float)pixelY / Texture.Height;
        float u1 = (float)(pixelX + CellWidth) / Texture.Width;
        float v1 = (float)(pixelY + CellHeight) / Texture.Height;
        
        return new Glyph(u0, v0, u1, v1);
    }
}
