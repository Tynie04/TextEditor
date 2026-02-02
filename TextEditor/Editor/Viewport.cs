namespace TextEditor.Editor;

/// <summary>
/// Represents the vertical viewport state of the editor,
/// defining which portion of the text buffer is currently visible.
/// </summary>
public struct Viewport
{
    public int ScrollRow;
    public int VisibleRows;
}