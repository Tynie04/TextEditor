using TextEditor.Editor;
using Xunit;

namespace TextEditor.Tests;

public class CursorTests
{
    private static TextBuffer CreateHelloWorldBuffer()
    {
        var buffer = new TextBuffer();

        foreach (char c in "Hello")
            buffer.InsertChar(0, buffer.GetLine(0).Length, c);

        buffer.InsertNewLine(0, 5);

        foreach (char c in "World")
            buffer.InsertChar(1, buffer.GetLine(1).Length, c);

        return buffer;
    }
    
    [Fact]
    public void Cursor_StartsAtOrigin()
    {
        var cursor = new Cursor(0, 0);

        Assert.Equal(0, cursor.Row);
        Assert.Equal(0, cursor.Col);
    }

    [Fact]
    public void MoveRight_WrapsToNextLine()
    {
        var buffer = CreateHelloWorldBuffer();
        var cursor = new Cursor(0, 5);

        cursor.MoveRight(buffer);

        Assert.Equal(1, cursor.Row);
        Assert.Equal(0, cursor.Col);
    }

    [Fact]
    public void MoveLeft_WrapsToPreviousLine()
    {
        var buffer = CreateHelloWorldBuffer();
        var cursor = new Cursor(1, 0);

        cursor.MoveLeft(buffer);

        Assert.Equal(0, cursor.Row);
        Assert.Equal(5, cursor.Col);
    }

    [Fact]
    public void MoveDown_ClampsToLineLength()
    {
        var buffer = CreateHelloWorldBuffer();
        var cursor = new Cursor(0, 10);

        cursor.MoveDown(buffer);

        Assert.Equal(1, cursor.Row);
        Assert.Equal(5, cursor.Col);
    }
}