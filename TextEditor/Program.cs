using OpenTK.Mathematics;
using OpenTK.Windowing.Common;
using OpenTK.Windowing.Desktop;
using TextEditor.Editor;
using TextEditor.Rendering;

namespace TextEditor;

class Program
{
    static void Main(string[] args)
    {
        var buffer = new TextBuffer();
        
        var gameSettings = GameWindowSettings.Default;

        var nativeSettings = new NativeWindowSettings
        {
            ClientSize = new Vector2i(800, 600),
            Title = "TextEditor",
            
            APIVersion = new Version(3, 3),
            Profile = ContextProfile.Compatability
        };

        using var window = new EditorWindow(gameSettings, nativeSettings);
        window.Run();

    }
    
    private static void PrintBuffer(TextBuffer buffer, string title)
    {
        Console.WriteLine($"--- {title} ---");
        for (int i = 0; i < buffer.GetLineCount(); i++)
        {
            Console.WriteLine($"{i}: \"{buffer.GetLine(i)}\"");
        }
        Console.WriteLine();
    }
}