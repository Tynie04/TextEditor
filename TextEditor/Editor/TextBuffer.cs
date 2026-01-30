using System.IO;

namespace TextEditor.Editor;

public class TextBuffer
{
    private List<string> _lines = new();

    public TextBuffer()
    {
        _lines.Add(string.Empty);
    }
    
    // Essential mutations
    public void InsertChar(int row, int col, char character)
    {
        string line = _lines[row];
        
        string left = line.Substring(0, col);
        string right = line.Substring(col);
        _lines[row] = left + character + right;
    }
    
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
    
    public void InsertNewLine(int row, int col)
    {
        string line = _lines[row];
        
        string left = line.Substring(0, col);
        string right = line.Substring(col);
        
        _lines[row] = left;
        _lines.Insert(row + 1, right);
    }
    
    // Essential Queries
    public string GetLine(int row)
    {
        return _lines[row];
    }
    
    public int GetLineCount()
    {
        return _lines.Count;
    }

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

    public void SaveToFile(string path)
    {
        string content = string.Join("\n", _lines);

        File.WriteAllText(path, content);
    }
}