namespace TextEditor.Commands;

/// <summary>
/// Editing commands that are NOT text insertion.
/// </summary>
public sealed record DeleteBackward : EditorCommand;
public sealed record DeleteForward  : EditorCommand;
public sealed record InsertNewLine  : EditorCommand;