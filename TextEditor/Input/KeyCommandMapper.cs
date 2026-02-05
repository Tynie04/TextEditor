using OpenTK.Windowing.GraphicsLibraryFramework;
using TextEditor.Commands;

namespace TextEditor.Input;

/// <summary>
/// Maps raw key input to editor commands.
/// Text input is explicitly excluded.
/// </summary>
public sealed class KeyCommandMapper
{
    public bool TryMap(
        RawInputEvent input,
        out EditorCommand? command)
    {
        command = null;

        if (input is not RawKeyEvent keyEvent)
            return false;

        switch (keyEvent.Key)
        {
            case Keys.Left:
                command = new MoveCursorLeft();
                return true;
            
            case Keys.Right:
                command = new MoveCursorRight();
                return true;
            
            case Keys.Up:
                command = new MoveCursorUp();
                return true;
            
            case Keys.Down:
                command = new MoveCursorDown();
                return true;
            
            case Keys.Backspace:
                command = new DeleteBackward();
                return true;
            
            case Keys.Delete:
                command = new DeleteForward();
                return true;
            
            case Keys.Enter:
                command = new InsertNewLine();
                return true;
            
            case Keys.S when keyEvent.Modifiers.HasFlag(KeyModifiers.Control):
                command = new SaveCommand();
                return true;
            
            default:
                return false;
        }
    }
}