namespace TextEditor.Editor;

/// <summary>
/// Represents a pointer to a specific character position within a <see cref="TextBuffer"/>.
/// </summary>
/// <remarks>
/// The cursor uses zero-based indexing for both rows and columns. 
/// Future implementations may include 'Preferred Column' logic to maintain horizontal 
/// positioning when navigating across lines of varying lengths.
/// </remarks>
public struct Cursor
{
    /// <summary>
    /// Initializes a new instance of the <see cref="Cursor"/> struct with specified coordinates.
    /// </summary>
    /// <param name="row">The initial row (line index).</param>
    /// <param name="col">The initial column (character index).</param>
    public Cursor(int row, int col)
    {
        Row = row;
        Col = col;
        PreferredCol = col;
    }

    public int Row { get; set; }
    public int Col { get; set; }
    public int PreferredCol { get; private set; }


    /// <summary>
    /// Moves the cursor one position to the left,
    /// and updates preferred column accordingly.
    /// </summary>
    /// <param name="buffer">The <see cref="TextBuffer"/> used to calculate line wrapping.</param>
    /// <remarks>
    /// If the cursor is at the start of a line (column 0), it will wrap to the end of the previous line.
    /// </remarks>
    public void MoveLeft(TextBuffer buffer)
    {
        if (Col > 0)
        {
            Col--;
        }
        else if (Row > 0)
        {
            Row--;
            Col = buffer.GetLine(Row).Length;
        }
        else
        {
            return;
        }

        PreferredCol = Col;
    }


    /// <summary>
    /// Moves the cursor one position to the right,
    /// and updates the preferred column accordingly.
    /// </summary>
    /// <param name="buffer">The <see cref="TextBuffer"/> used to calculate line wrapping.</param>
    /// <remarks>
    /// If the cursor is at the end of a line, it will wrap to the start of the next line.
    /// </remarks>
    public void MoveRight(TextBuffer buffer)
    {
        if (Col < buffer.GetLine(Row).Length)
        {
            Col++;
        }
        else if (Row < buffer.GetLineCount() - 1)
        {
            Row++;
            Col = 0;
        }
        else
        {
            return;
        }

        PreferredCol = Col;
    }

    /// <summary>
    /// Moves the cursor up to the previous line, while trying to preserve the preferred column.
    /// </summary>
    /// <param name="buffer">The <see cref="TextBuffer"/> used to check line lengths.</param>
    /// <remarks>
    /// If the current column exceeds the length of the line above, the cursor snaps to the end of that line.
    /// </remarks>
    public void MoveUp(TextBuffer buffer)
    {
        if (Row == 0)
        {
            return;
        }
        
        Row--;
        
        int lineLength = buffer.GetLine(Row).Length;
        Col = Math.Min(PreferredCol, lineLength);
    }

    /// <summary>
    /// Moves the cursor down to the next line, while trying to preserve the preferred column.
    /// </summary>
    /// <param name="buffer">The <see cref="TextBuffer"/> used to check line lengths.</param>
    /// <remarks>
    /// If the current column exceeds the length of the line below, the cursor snaps to the end of that line.
    /// </remarks>
    public void MoveDown(TextBuffer buffer)
    {
        if (Row >= buffer.GetLineCount() - 1)
        {
            return;
        }
        
        Row++;
        
        int lineLength = buffer.GetLine(Row).Length;
        Col = Math.Min(PreferredCol, lineLength);
    }
    
    /// <summary>
    /// Updates the cursor position explicitly and resets the preferred column.
    /// This should be called after text insertion or deletion.
    /// </summary>
    /// <param name="row">The new row index.</param>
    /// <param name="col">The new column index.</param>
    public void SetPosition(int row, int col)
    {
        Row = row;
        Col = col;
        PreferredCol = col;
    }
    
    /// <summary>
    /// Updates the preferred column to match the current column.
    /// This should be called after horizontal cursor movement.
    /// </summary>
    public void SyncPreferredColumn()
    {
        PreferredCol = Col;
    }

}