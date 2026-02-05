namespace TextEditor.Platform;

public interface IFileDialogService
{
    string? ShowOpenFileDialog();
    string? ShowSaveFileDialog();
}