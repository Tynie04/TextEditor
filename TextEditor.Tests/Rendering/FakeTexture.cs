using TextEditor.Rendering;

namespace TextEditor.Tests.Rendering;

internal sealed class FakeTexture : ITexture
{
    public int Width { get; }
    public int Height { get; }

    public FakeTexture(int width, int height)
    {
        Width = width;
        Height = height;
    }
}
