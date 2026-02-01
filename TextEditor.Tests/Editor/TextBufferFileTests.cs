using System;
using System.IO;
using TextEditor.Editor;
using Xunit;

namespace TextEditor.Tests;

public class TextBufferFileTests
{
    [Fact]
    public void SaveAndLoad_PreservesAllLines()
    {
        var buffer = new TextBuffer();

        buffer.InsertChar(0, 0, 'A');
        buffer.InsertChar(0, 1, 'B');
        buffer.InsertNewLine(0, 2);
        buffer.InsertChar(1, 0, 'C');

        string path = Path.Combine(Path.GetTempPath(), Guid.NewGuid() + ".txt");

        try
        {
            buffer.SaveToFile(path);

            var loaded = new TextBuffer();
            loaded.LoadFromFile(path);

            Assert.Equal(buffer.GetLineCount(), loaded.GetLineCount());

            for (int i = 0; i < buffer.GetLineCount(); i++)
            {
                Assert.Equal(buffer.GetLine(i), loaded.GetLine(i));
            }
        }
        finally
        {
            if (File.Exists(path))
                File.Delete(path);
        }
    }
}