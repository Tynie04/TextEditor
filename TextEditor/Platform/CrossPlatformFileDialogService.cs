using NativeFileDialogSharp;

namespace TextEditor.Platform;

public sealed class CrossPlatformFileDialogService : IFileDialogService
{
    private const string Filters =
        "Text Files (*.txt):txt;" +
        "Markdown (*.md):md;" +
        "HTML (*.html):html;" +
        "CSS (*.css):css;" +
        "JavaScript (*.js):js;" +
        "TypeScript (*.ts):ts;" +
        "CSV (*.csv):csv;" +
        "JSON (*.json):json;" +
        "XML (*.xml):xml;" +
        "YAML (*.yaml):yaml,yml;" +
        "GLSL (*.glsl):glsl;" +
        "All Files (*):*";

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