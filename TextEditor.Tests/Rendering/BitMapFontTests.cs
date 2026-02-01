using TextEditor.Rendering;

namespace TextEditor.Tests.Rendering;

public class BitmapFontTests
{
    [Fact]
    public void Constructor_ComputesRowsAndColumnsCorrectly()
    {
        var texture = new FakeTexture(width: 256, height: 128);
        int cellWidth = 16;
        int cellHeight = 16;

        var font = BitmapFont.CreateForTest(
            texture,
            cellWidth,
            cellHeight);

        Assert.Equal(16, font.Columns);
        Assert.Equal(8, font.Rows);
    }
    
    [Fact]
    public void GetGlyph_ReturnsCorrectUVs_ForAsciiCharacter()
    {
        var texture = new FakeTexture(512, 512);
        var font = BitmapFont.CreateForTest(texture, 16, 16);

        Glyph g = font.GetGlyph('A'); // ASCII 65

        int expectedPixelX = 32 + (65 * 16);
        int expectedPixelY = 64 + (0 * 16);

        Assert.Equal((float)expectedPixelX / 512, g.U0);
        Assert.Equal((float)expectedPixelY / 512, g.V0);
        Assert.Equal((float)(expectedPixelX + 16) / 512, g.U1);
        Assert.Equal((float)(expectedPixelY + 16) / 512, g.V1);
    }

    
    [Theory]
    [InlineData('\0')]
    [InlineData('\n')]
    [InlineData((char)200)]
    public void GetGlyph_MapsInvalidCharactersToQuestionMark(char input)
    {
        var texture = new FakeTexture(512, 512);
        var font = BitmapFont.CreateForTest(texture, 16, 16);

        Glyph actual = font.GetGlyph(input);
        Glyph expected = font.GetGlyph('?');

        Assert.Equal(expected.U0, actual.U0);
        Assert.Equal(expected.V0, actual.V0);
    }

}
