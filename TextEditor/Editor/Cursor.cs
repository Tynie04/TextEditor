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
    }

    public int Row { get; set; }
    public int Col { get; set; }

    /// <summary>
    /// Moves the cursor one position to the left.
    /// </summary>
    /// <param name="buffer">The <see cref="TextBuffer"/> used to calculate line wrapping.</param>
    /// <remarks>
    /// If the cursor is at the start of a line (column 0), it will wrap to the end of the previous line.
    /// </remarks>
    public void MoveLeft(TextBuffer buffer)
    {
        // Case 1: Move left in the same line
        if (Col > 0)
        {
            Col--;
            return;
        }
        
        // Case 2: At start of line, move to end of previous line
        if (Row > 0)
        {
            Row--;
            Col = buffer.GetLine(Row).Length;
        }
    }

    /// <summary>
    /// Moves the cursor one position to the right.
    /// </summary>
    /// <param name="buffer">The <see cref="TextBuffer"/> used to calculate line wrapping.</param>
    /// <remarks>
    /// If the cursor is at the end of a line, it will wrap to the start of the next line.
    /// </remarks>
    public void MoveRight(TextBuffer buffer)
    {
        // Case 1: Move right in the same line
        if (Col < buffer.GetLine(Row).Length)
        {
            Col++;
            return;
        }
        
        // Case 2: At end of line, move to begin of next line
        if (Row < buffer.GetLineCount() - 1)
        {
            Row++;
            Col = 0;
        }
    }

    /// <summary>
    /// Moves the cursor up to the previous line.
    /// </summary>
    /// <param name="buffer">The <see cref="TextBuffer"/> used to check line lengths.</param>
    /// <remarks>
    /// If the current column exceeds the length of the line above, the cursor snaps to the end of that line.
    /// </remarks>
    public void MoveUp(TextBuffer buffer)
    {
        if (Row > 0)
        {
            Row--;
            Col = Math.Min(Col, buffer.GetLine(Row).Length);
        }
    }

    /// <summary>
    /// Moves the cursor down to the next line.
    /// </summary>
    /// <param name="buffer">The <see cref="TextBuffer"/> used to check line lengths.</param>
    /// <remarks>
    /// If the current column exceeds the length of the line below, the cursor snaps to the end of that line.
    /// </remarks>
    public void MoveDown(TextBuffer buffer)
    {
        if (Row < buffer.GetLineCount() - 1)
        {
            Row++;
            Col = Math.Min(Col, buffer.GetLine(Row).Length);
        }
    }
}