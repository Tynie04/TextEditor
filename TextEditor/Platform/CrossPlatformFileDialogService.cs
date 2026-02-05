using NativeFileDialogSharp;

namespace TextEditor.Platform;

public sealed class CrossPlatformFileDialogService : IFileDialogService
{
    private const string Filters =
        "txt;" +
        "md;" +
        "html;" +
        "css;" +
        "js;" +
        "ts;" +
        "csv;" +
        "json;" +
        "xml;" +
        "yaml,yml;" +
        "glsl;" +
        "*";

    public string? ShowOpenFileDialog()
    {
        var result = Dialog.FileOpen(Filters);
        return result.IsOk ? result.Path : null;
    }

    public string? ShowSaveFileDialog()
    {
        var result = Dialog.FileSave(
            defaultPath: "untitled.txt",
            filterList: Filters
        );
        
        return result.IsOk ? result.Path : null;
    }
}