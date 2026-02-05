namespace TextEditor.Commands;

public sealed record SaveCommand : EditorCommand;

public sealed record LoadCommand : EditorCommand;

public sealed record NewDocumentCommand : EditorCommand;
