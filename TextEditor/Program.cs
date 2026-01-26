using OpenTK.Mathematics;
using OpenTK.Windowing.Common;
using OpenTK.Windowing.Desktop;
using TextEditor.Editor;
using TextEditor.Rendering;

namespace TextEditor;

class Program
{
    static void Main(string[] args)
    {
        var buffer = new TextBuffer();

        RunBufferTests(buffer);
        RunCursorTests(buffer);
        RunFileTests(buffer);

        var gameSettings = GameWindowSettings.Default;

        var nativeSettings = new NativeWindowSettings
        {
            ClientSize = new Vector2i(800, 600),
            Title = "TextEditor",
            
            APIVersion = new Version(3, 3),
            Profile = ContextProfile.Compatability
        };

        using var window = new EditorWindow(gameSettings, nativeSettings);
        window.Run();

    }

    static void RunBufferTests(TextBuffer buffer)
    {
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

    private static void RunCursorTests(TextBuffer buffer)
    {
        var cursor = new Cursor(0, 0);

        // Rebuild buffer content for cursor tests
        buffer = new TextBuffer();
        buffer.InsertChar(0, 0, 'H');
        buffer.InsertChar(0, 1, 'e');
        buffer.InsertChar(0, 2, 'l');
        buffer.InsertChar(0, 3, 'l');
        buffer.InsertChar(0, 4, 'o');

        buffer.InsertNewLine(0, 5);

        buffer.InsertChar(1, 0, 'W');
        buffer.InsertChar(1, 1, 'o');
        buffer.InsertChar(1, 2, 'r');
        buffer.InsertChar(1, 3, 'l');
        buffer.InsertChar(1, 4, 'd');

        PrintBuffer(buffer, "Buffer for cursor tests");

        // Cursor starts at (0,0)
        AssertCursor(cursor, 0, 0, "Initial cursor position");

        // Move right 5 times, end of first line
        for (int i = 0; i < 5; i++)
            cursor.MoveRight(buffer);

        AssertCursor(cursor, 0, 5, "MoveRight to end of line");

        // Move right once, next line, col 0
        cursor.MoveRight(buffer);
        AssertCursor(cursor, 1, 0, "MoveRight wraps to next line");

        // Move right 3 times
        cursor.MoveRight(buffer);
        cursor.MoveRight(buffer);
        cursor.MoveRight(buffer);
        AssertCursor(cursor, 1, 3, "MoveRight inside second line");

        // Move left once
        cursor.MoveLeft(buffer);
        AssertCursor(cursor, 1, 2, "MoveLeft inside line");

        // Move left to start of line
        cursor.MoveLeft(buffer);
        cursor.MoveLeft(buffer);
        AssertCursor(cursor, 1, 0, "MoveLeft to start of line");

        // Move left once more, previous line end
        cursor.MoveLeft(buffer);
        AssertCursor(cursor, 0, 5, "MoveLeft wraps to previous line");

        // Move down, should clamp to line length
        cursor.MoveDown(buffer);
        AssertCursor(cursor, 1, 5, "MoveDown clamps to end of shorter line");

        // Move up, should clamp again
        cursor.MoveUp(buffer);
        AssertCursor(cursor, 0, 5, "MoveUp clamps correctly");

        Console.WriteLine("ALL CURSOR TESTS PASSED");
    }

    private static void RunFileTests(TextBuffer buffer)
    {
        string tempPath = Path.Combine(Path.GetTempPath(), "textbuffer_test.txt");

        // Build a buffer with multiple lines (including empty line)
        buffer = new TextBuffer();
        buffer.InsertChar(0, 0, 'A');
        buffer.InsertChar(0, 1, 'B');
        buffer.InsertChar(0, 2, 'C');

        buffer.InsertNewLine(0, 3);

        buffer.InsertChar(1, 0, 'D');
        buffer.InsertChar(1, 1, 'E');

        buffer.InsertNewLine(1, 2); // create empty third line

        PrintBuffer(buffer, "Original buffer before save");

        // Save to file
        buffer.SaveToFile(tempPath);

        // Load into a new buffer
        var loadedBuffer = new TextBuffer();
        loadedBuffer.LoadFromFile(tempPath);

        PrintBuffer(loadedBuffer, "Loaded buffer after save/load");

        // Assertions
        AssertEqual(loadedBuffer.GetLineCount(), buffer.GetLineCount(), "Line count after load");

        for (int i = 0; i < buffer.GetLineCount(); i++)
        {
            AssertEqual(
                loadedBuffer.GetLine(i),
                buffer.GetLine(i),
                $"Line content match at row {i}"
            );
        }

        // Clean up temp file
        File.Delete(tempPath);

        Console.WriteLine("FILE SAVE / LOAD TESTS PASSED");

    }
    
    private static void PrintBuffer(TextBuffer buffer, string title)
    {
        Console.WriteLine($"--- {title} ---");
        for (int i = 0; i < buffer.GetLineCount(); i++)
        {
            Console.WriteLine($"{i}: \"{buffer.GetLine(i)}\"");
        }
        Console.WriteLine();
    }

    private static void AssertEqual(string actual, string expected, string message)
    {
        if (actual != expected)
            throw new Exception($"ASSERT FAILED: {message}\nExpected: \"{expected}\"\nActual:   \"{actual}\"");
    }
    
    private static void AssertEqual(int actual, int expected, string message)
    {
        if (actual != expected)
            throw new Exception(
                $"ASSERT FAILED: {message}\nExpected: {expected}\nActual:   {actual}"
            );
    }
    
    private static void AssertCursor(Cursor cursor, int expectedRow, int expectedCol, string message)
    {
        if (cursor.Row != expectedRow || cursor.Col != expectedCol)
        {
            throw new Exception(
                $"ASSERT FAILED: {message}\n" +
                $"Expected: (Row={expectedRow}, Col={expectedCol})\n" +
                $"Actual:   (Row={cursor.Row}, Col={cursor.Col})"
            );
        }
    }
}