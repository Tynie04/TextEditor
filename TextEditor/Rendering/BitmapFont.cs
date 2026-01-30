namespace TextEditor.Rendering;

public class BitmapFont
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
        int rows
    )
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
        int cellHeight
    ) 
    {
        Texture texture = Texture.LoadFromFile(imagePath);

        int columns = texture.Width / cellWidth;
        int rows = texture.Height / cellHeight;

        return new BitmapFont(
            texture,
            cellWidth,
            cellHeight,
            columns,
            rows
        );
    }
}