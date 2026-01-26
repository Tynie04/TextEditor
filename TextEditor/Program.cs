using TextEditor.Editor;

namespace TextEditor;

class Program
{
    static void Main(string[] args)
    {
        var buffer = new TextBuffer();
        
        // INSERT CHARACTERS IN EMPTY BUFFER
        buffer.InsertChar(0, 0, 'H');
        buffer.InsertChar(0, 1, 'i');

        PrintBuffer(buffer, "Insert into empty line");

        AssertEqual(buffer.GetLine(0), "Hi", "InsertChar basic");

        // INSERT NEW LINE IN MIDDLE OF LINE
        buffer.InsertNewLine(0, 1);
        PrintBuffer(buffer, "Insert new line in middle of line");
        
        AssertEqual(buffer.GetLine(0), "H", "Line before newline");
        AssertEqual(buffer.GetLine(1), "i", "Line after newline");
        
        // DELETE INSIDE A LINE
        buffer.DeleteChar(1, 1); // delete 'i'

        PrintBuffer(buffer, "DeleteChar inside line");

        AssertEqual(buffer.GetLine(1), "", "Delete inside line");

        // DELETE AT START OF LINE (MERGE LINES)
        buffer.DeleteChar(1, 0);

        PrintBuffer(buffer, "DeleteChar merge lines");

        AssertEqual(buffer.GetLineCount(), 1, "Line merge count");
        AssertEqual(buffer.GetLine(0), "H", "Line merge content");
    }
    
    static void PrintBuffer(TextBuffer buffer, string title)
    {
        Console.WriteLine($"--- {title} ---");
        for (int i = 0; i < buffer.GetLineCount(); i++)
        {
            Console.WriteLine($"{i}: \"{buffer.GetLine(i)}\"");
        }
        Console.WriteLine();
    }

    static void AssertEqual(string actual, string expected, string message)
    {
        if (actual != expected)
            throw new Exception($"ASSERT FAILED: {message}\nExpected: \"{expected}\"\nActual:   \"{actual}\"");
    }
    
    static void AssertEqual(int actual, int expected, string message)
    {
        if (actual != expected)
            throw new Exception(
                $"ASSERT FAILED: {message}\nExpected: {expected}\nActual:   {actual}"
            );
    }

}