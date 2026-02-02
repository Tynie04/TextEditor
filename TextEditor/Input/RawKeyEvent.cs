using OpenTK.Windowing.GraphicsLibraryFramework;

namespace TextEditor.Input;

/// <summary>
/// Represents a physical key press (arrows, backspace, modifiers, etc.).
/// This does NOT represent text input.
/// </summary>
public sealed record RawKeyEvent(
        Keys Key,
        KeyModifiers Modifiers,
        bool IsRepeat
    ) : RawInputEvent;