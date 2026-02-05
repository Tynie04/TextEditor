namespace TextEditor.Editor;

public sealed class Document
{
    public TextBuffer Buffer { get; } = new();
    
    public string? FilePath { get; private set; }
    public bool IsDirty { get; private set; }
    
    public void MarkDirty() => IsDirty = true;
    public void MarkClean() => IsDirty = false;

    public void SetPath(string? path)
    {
        FilePath = path;
        MarkClean();
    }
}