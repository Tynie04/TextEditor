using TextEditor.Editor;
using Xunit;

namespace TextEditor.Tests;

public class TextBufferTests
{
    [Fact]
    public void InsertChar_IntoEmptyBuffer_CreateText()
    {
        var buffer = new TextBuffer();
        
        buffer.InsertChar(0, 0, 'H');
        buffer.InsertChar(0, 1, 'i');
        
        Assert.Equal("Hi", buffer.GetLine(0));
        Assert.Equal(1, buffer.GetLineCount());
    }

    [Fact]
    public void InsertNewLine_SplitsLineCorrectly()
    {
        var buffer = new TextBuffer();
        
        buffer.InsertChar(0, 0, 'H');
        buffer.InsertChar(0, 1, 'i');
        
        buffer.InsertNewLine(0,1);
        
        Assert.Equal("H", buffer.GetLine(0));
        Assert.Equal("i", buffer.GetLine(1));
        Assert.Equal(2, buffer.GetLineCount());

    }

    [Fact]
    public void DeleteChar_InsideLine_RemovesCharacter()
    {
        var buffer = new TextBuffer();

        buffer.InsertChar(0, 0, 'A');
        buffer.InsertChar(0, 1, 'B');

        buffer.DeleteChar(0, 2);

        Assert.Equal("A", buffer.GetLine(0));
    }

    [Fact]
    public void DeleteChar_AtStartOfLine_MergesLines()
    {
        var buffer = new TextBuffer();

        buffer.InsertChar(0, 0, 'A');
        buffer.InsertNewLine(0, 1);
        buffer.InsertChar(1, 0, 'B');

        buffer.DeleteChar(1, 0);

        Assert.Equal(1, buffer.GetLineCount());
        Assert.Equal("AB", buffer.GetLine(0));
    }
}