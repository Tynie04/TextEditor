using TextEditor.Commands;
using TextEditor.Input;

namespace TextEditor.Editor;

/// <summary>
/// Central coordinator that owns editor state and applies commands.
/// </summary>
public sealed class EditorController
{
    private readonly TextBuffer _buffer;
    private Cursor _cursor;
    private readonly KeyCommandMapper _keyMapper;
    
    public Cursor Cursor => _cursor;
    public TextBuffer Buffer => _buffer;


    public EditorController()
    {
        _buffer = new TextBuffer();
        _cursor = new Cursor(0, 0);
        _keyMapper = new KeyCommandMapper();
    }
    /// <summary>
    /// Entry point for all raw input coming from the window.
    /// </summary>
    public void HandleRawInput(RawInputEvent input)
    {
        if (input is RawTextEvent text)
        {
            Execute(new InsertChar(text.Character));
            return;
        }
        
        if (_keyMapper.TryMap(input, out var command))
        {
            if (command != null) Execute(command);
        }
    }

    private void Execute(EditorCommand command)
    {
        switch (command)
        {
            case InsertChar insert:
            {
                _buffer.InsertChar(_cursor.Row, _cursor.Col, insert.Character);
                _cursor.Col++;
                break;
            }
            case MoveCursorLeft:
            {
                _cursor.MoveLeft(_buffer);
                break;
            }
            case MoveCursorRight:
            {
                _cursor.MoveRight(_buffer);
                break;
            }
            case MoveCursorUp:
            {
                _cursor.MoveUp(_buffer);
                break;
            }
            case MoveCursorDown:
            {
                _cursor.MoveDown(_buffer);
                break;
            }
            
            case DeleteBackward:
            {
                if (_cursor.Col > 0)
                {
                    _buffer.DeleteChar(_cursor.Row, _cursor.Col);
                    _cursor.Col--;
                }
                else if (_cursor.Row > 0)
                {
                    int newRow = _cursor.Row - 1;
                    int newCol = _buffer.GetLine(newRow).Length;

                    _buffer.DeleteChar(_cursor.Row, _cursor.Col);

                    _cursor.Row = newRow;
                    _cursor.Col = newCol;
                }

                break;
            }

            case DeleteForward:
            {
                string line = _buffer.GetLine(_cursor.Row);

                if (_cursor.Col < line.Length)
                {
                    _buffer.DeleteChar(_cursor.Row, _cursor.Col + 1);
                }
                else if (_cursor.Row < _buffer.GetLineCount() - 1)
                {
                    _buffer.DeleteChar(_cursor.Row + 1, 0);
                }

                break;
            }

            case InsertNewLine:
            {
                _buffer.InsertNewLine(_cursor.Row, _cursor.Col);
                _cursor.Row++;
                _cursor.Col = 0;
                break;
            }
        }
        Console.WriteLine(
            $"Command: {command.GetType().Name} | Cursor=({_cursor.Row},{_cursor.Col})"
        );

    }
}