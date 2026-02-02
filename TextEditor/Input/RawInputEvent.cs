namespace TextEditor.Input;

/// <summary>
/// Base type for all raw input coming from the window system.
/// This represents "something happened" without editor meaning.
/// </summary>
public abstract record RawInputEvent;