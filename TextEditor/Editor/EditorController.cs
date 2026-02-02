using TextEditor.Commands;
using TextEditor.Input;

namespace TextEditor.Editor;

/// <summary>
/// Central coordinator that owns editor state and applies editor commands
/// derived from raw input events.
/// </summary>
public sealed class EditorController
{
    private readonly TextBuffer _buffer;
    private Cursor _cursor;
    private readonly KeyCommandMapper _keyMapper;
    private Viewport _viewport;

    public Viewport Viewport => _viewport;
    public Cursor Cursor => _cursor;
    public TextBuffer Buffer => _buffer;

    /// <summary>
    /// Initializes a new editor controller with an empty buffer,
    /// a default cursor position, and an initial viewport.
    /// </summary>
    public EditorController()
    {
        _buffer = new TextBuffer();
        _cursor = new Cursor(0, 0);
        _keyMapper = new KeyCommandMapper();
        _viewport = new Viewport
        {
            ScrollRow = 0,
            VisibleRows = 0
        };
    }

    /// <summary>
    /// Updates the number of visible text rows in the viewport.
    /// This value is clamped to ensure it remains valid.
    /// </summary>
    /// <param name="visibleRows">
    /// The number of rows that can be displayed at once.
    /// </param>
    public void SetVisibleRows(int visibleRows)
    {
        _viewport.VisibleRows = Math.Max(1, visibleRows);
        ClampScroll();
    }

    /// <summary>
    /// Clamps the current scroll position so it remains within
    /// the valid range of the text buffer.
    /// </summary>
    private void ClampScroll()
    {
        int maxScroll =
            Math.Max(0, _buffer.GetLineCount() - _viewport.VisibleRows);

        _viewport.ScrollRow =
            Math.Clamp(_viewport.ScrollRow, 0, maxScroll);
    }

    /// <summary>
    /// Adjusts the viewport scroll position to ensure that the
    /// cursor remains visible on screen.
    /// </summary>
    private void EnsureCursorVisible()
    {
        if (_cursor.Row < _viewport.ScrollRow)
        {
            _viewport.ScrollRow = _cursor.Row;
        }
        else if (_cursor.Row >= _viewport.ScrollRow + _viewport.VisibleRows)
        {
            _viewport.ScrollRow =
                _cursor.Row - _viewport.VisibleRows + 1;
        }

        ClampScroll();
    }

    /// <summary>
    /// Handles a raw input event coming from the window and
    /// translates it into editor commands when applicable.
    /// </summary>
    /// <param name="input">
    /// The raw input event to process.
    /// </param>
    public void HandleRawInput(RawInputEvent input)
    {
        if (input is RawTextEvent text)
        {
            Execute(new InsertChar(text.Character));
            return;
        }

        if (_keyMapper.TryMap(input, out var command))
        {
            if (command != null)
            {
                Execute(command);
            }
        }
    }

    /// <summary>
    /// Executes a single editor command and updates editor state
    /// accordingly.
    /// </summary>
    /// <param name="command">
    /// The editor command to execute.
    /// </param>
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

        EnsureCursorVisible();
    }
}
