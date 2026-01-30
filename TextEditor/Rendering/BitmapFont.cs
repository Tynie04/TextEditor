namespace TextEditor.Rendering;

public class BitmapFont
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
}
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
