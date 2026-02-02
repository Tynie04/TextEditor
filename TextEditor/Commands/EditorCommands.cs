namespace TextEditor.Commands;

/// <summary>
/// Cursor movement commands.
/// </summary>
public sealed record MoveCursorLeft  : EditorCommand;
public sealed record MoveCursorRight : EditorCommand;
public sealed record MoveCursorUp    : EditorCommand;
public sealed record MoveCursorDown  : EditorCommand;
public sealed record InsertChar(char Character) : EditorCommand;
