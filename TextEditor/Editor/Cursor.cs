namespace TextEditor.Editor;

public struct Cursor
{
    public Cursor(int row, int col)
    {
        Row = row;
        Col = col;
    }

    public int Row { get; set; }
    public int Col { get; set; }

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

    public void MoveUp(TextBuffer buffer)
    {
        if (Row > 0)
        {
            Row--;
            Col = Math.Min(Col, buffer.GetLine(Row).Length);
        }
    }

    public void MoveDown(TextBuffer buffer)
    {
        if (Row < buffer.GetLineCount() - 1)
        {
            Row++;
            Col = Math.Min(Col, buffer.GetLine(Row).Length);
        }
    }
}