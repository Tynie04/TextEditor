namespace TextEditor.Rendering;

/// <summary>
/// Represents a bitmap-based font loaded from a texture atlas grid.
/// </summary>
public sealed class BitmapFont
{
    private const int ColumnLabelOffset = 32;
    private const int RowLabelOffset = 64;
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
    /// Calculates the UV coordinates for a specific character from the GNU Unifont bitmap. 
    /// </summary>
    /// <param name="c">The character to retrieve.</param>
    /// <example>
    /// 'A' (0x0041) = row 0x00, column 0x41 (decimal 65) = pixel (1072, 62)
    /// </example>
    /// <returns>A <see cref="Glyph"/> containing normalized UV coordinates.</returns>
    public Glyph GetGlyph(char c)
    {
        // Replace unprintable characters with '?'
        if (c < 32 || c > 126)
        {
            c = '?';
        }

        int codepoint = c;

        // Unifont grid indexing:
        // Row = high byte (for ASCII 0x00-0xFF, this is always 0x00)
        // Column = low byte (the full character code 0x00-0xFF)
        int row = (codepoint >> 8) & 0xFF;    // High byte
        int column = codepoint & 0xFF;         // Low byte

        // Calculate pixel position with offsets for white border + labels
        int pixelX = ColumnLabelOffset + (column * CellWidth);
        int pixelY = RowLabelOffset + (row * CellHeight);

        // Convert to normalized UV coordinates (0.0 to 1.0)
        float u0 = (float)pixelX / Texture.Width;
        float v0 = (float)pixelY / Texture.Height;
        float u1 = (float)(pixelX + CellWidth) / Texture.Width;
        float v1 = (float)(pixelY + CellHeight) / Texture.Height;
        
        return new Glyph(u0, v0, u1, v1);
    }
}