namespace TextEditor.Input;

/// <summary>
/// Represents text input that already passed through keyboard layout / IME.
/// This is used for character insertion.
/// </summary>
public sealed record RawTextEvent(
    char Character
) : RawInputEvent;