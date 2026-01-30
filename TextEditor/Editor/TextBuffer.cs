using System.IO;

namespace TextEditor.Editor;

/// <summary>
/// Manages the underlying text data of the editor, handling line storage, 
/// insertions, deletions, and file I/O operations.
/// </summary>
public class TextBuffer
{
    private List<string> _lines = new();

    /// <summary>
    /// Initializes a new, empty <see cref="TextBuffer"/> with a single empty line.
    /// </summary>
    public TextBuffer()
    {
        _lines.Add(string.Empty);
    }
    
    /// <summary>
    /// Inserts a character at the specified row and column.
    /// </summary>
    /// <param name="row">The zero-based index of the line.</param>
    /// <param name="col">The zero-based character index within the line.</param>
    /// <param name="character">The character to insert.</param>
    public void InsertChar(int row, int col, char character)
    {
        string line = _lines[row];
        
        string left = line.Substring(0, col);
        string right = line.Substring(col);
        _lines[row] = left + character + right;
    }
    
    /// <summary>
    /// Deletes a character at the specified position.
    /// </summary>
    /// <param name="row">The zero-based index of the line.</param>
    /// <param name="col">The zero-based character index. If 0, the current line will merge with the one above.</param>
    /// <remarks>
    /// If <paramref name="col"/> is greater than 0, the character immediately preceding the cursor is removed.
    /// If <paramref name="col"/> is 0 and <paramref name="row"/> is greater than 0, the current line is appended 
    /// to the end of the previous line and then removed.
    /// </remarks>
    public void DeleteChar(int row, int col)
    {
        // Delete character before the cursor (within the same line)
        if (col > 0)
        {
            string line = _lines[row];
            _lines[row] = line.Substring(0, col - 1) + line.Substring(col);
        }
        
        // Merge with previous line if at the start of a line
        else if (row > 0)
        {
            _lines[row - 1] += _lines[row];
            _lines.RemoveAt(row);
        }
    }
    
    /// <summary>
    /// Splits the line at the specified column and moves the trailing text to a new line below.
    /// </summary>
    /// <param name="row">The zero-based index of the line to split.</param>
    /// <param name="col">The zero-based index where the split occurs.</param>
    public void InsertNewLine(int row, int col)
    {
        string line = _lines[row];
        
        string left = line.Substring(0, col);
        string right = line.Substring(col);
        
        _lines[row] = left;
        _lines.Insert(row + 1, right);
    }
    
    /// <summary>
    /// Retrieves the text content of a specific line.
    /// </summary>
    /// <param name="row">The zero-based index of the line.</param>
    /// <returns>The string content of the requested line.</returns>
    public string GetLine(int row)
    {
        return _lines[row];
    }
    
    /// <summary>
    /// Gets the total number of lines currently in the buffer.
    /// </summary>
    /// <returns>The line count.</returns>
    public int GetLineCount()
    {
        return _lines.Count;
    }

    /// <summary>
    /// Reads the content of a file from disk and populates the buffer.
    /// </summary>
    /// <param name="path">The full path to the file.</param>
    /// <remarks>
    /// If the file does not exist, the operation is aborted. The method handles empty 
    /// files by ensuring at least one empty string exists in the buffer.
    /// </remarks>
    public void LoadFromFile(string path)
    {
        if (!File.Exists(path))
        {
            // TODO: Throw an appropriate error
            return;
        }

        string content = File.ReadAllText(path);
        string[] splitContent = content.Split(new[] { "\r\n", "\r", "\n" }, StringSplitOptions.None);
        
        _lines.Clear();
        foreach (string line in splitContent)
        {
            _lines.Add(line);
        }

        if (_lines.Count == 0)
        {
            _lines.Add(String.Empty);
        }
    }

    /// <summary>
    /// Saves the current buffer content to a file on disk.
    /// </summary>
    /// <param name="path">The full path where the file should be saved.</param>
    /// <remarks>
    /// Lines are joined using a newline character (\n) before being written to the file.
    /// </remarks>
    public void SaveToFile(string path)
    {
        string content = string.Join("\n", _lines);

        File.WriteAllText(path, content);
    }
}