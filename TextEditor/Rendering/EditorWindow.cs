using System.Diagnostics;
using OpenTK.Graphics.OpenGL;
using OpenTK.Mathematics;
using OpenTK.Windowing.Common;
using OpenTK.Windowing.Desktop;
using TextEditor.Editor;
using TextEditor.Input;
using TextEditor.Platform;

namespace TextEditor.Rendering;

/// <summary>
/// The main rendering window for the text editor, responsible for OpenGL initialization,
/// shader management, and the primary render loop.
/// </summary>
public class EditorWindow : GameWindow
{
    private int _shaderProgram;
    private int _vao;
    private int _vbo;
    private readonly Queue<RawInputEvent> _inputQueue = new();
    private EditorController _editor;
    private BitmapFont _bitmapFont;
    private TextRenderer _renderer;

    private const float TextScale = 1.0f;

    public EditorWindow(GameWindowSettings gameSettings, NativeWindowSettings nativeSettings)
        : base(gameSettings, nativeSettings)
    {
    }

    protected override void OnLoad()
    {
        base.OnLoad();

        GL.Enable(EnableCap.Blend);
        GL.BlendFunc(BlendingFactor.SrcAlpha, BlendingFactor.OneMinusSrcAlpha);

        _editor = new EditorController(new CrossPlatformFileDialogService());

        _bitmapFont = BitmapFont.Load(
            "Assets/Fonts/unifont.bmp",
            16,
            16
        );

        GL.ClearColor(1f, 1f, 1f, 1f);

        int vertexShader = CompileShader(ShaderType.VertexShader, VertexShaderSource);
        int fragmentShader = CompileShader(ShaderType.FragmentShader, FragmentShaderSource);

        _shaderProgram = GL.CreateProgram();
        GL.AttachShader(_shaderProgram, vertexShader);
        GL.AttachShader(_shaderProgram, fragmentShader);
        GL.LinkProgram(_shaderProgram);

        GL.DeleteShader(vertexShader);
        GL.DeleteShader(fragmentShader);

        GL.UseProgram(_shaderProgram);
        UpdateProjection();

        _vao = GL.GenVertexArray();
        _vbo = GL.GenBuffer();

        GL.BindVertexArray(_vao);
        GL.BindBuffer(BufferTarget.ArrayBuffer, _vbo);

        _renderer = new TextRenderer(
            _shaderProgram,
            _vao,
            _vbo,
            _bitmapFont
        );

        int stride = 4 * sizeof(float);

        GL.VertexAttribPointer(0, 2, VertexAttribPointerType.Float, false, stride, 0);
        GL.EnableVertexAttribArray(0);

        GL.VertexAttribPointer(1, 2, VertexAttribPointerType.Float, false, stride, 2 * sizeof(float));
        GL.EnableVertexAttribArray(1);

        Resize += OnWindowResize;

        UpdateViewport();
    }

    protected override void OnRenderFrame(FrameEventArgs args)
    {
        base.OnRenderFrame(args);

        GL.Clear(ClearBufferMask.ColorBufferBit);
        GL.UseProgram(_shaderProgram);
        GL.BindVertexArray(_vao);

        RenderEditor();

        SwapBuffers();
    }

    protected override void OnUpdateFrame(FrameEventArgs args)
    {
        base.OnUpdateFrame(args);

        while (_inputQueue.Count > 0)
        {
            _editor.HandleRawInput(_inputQueue.Dequeue());
        }
    }

    protected override void OnKeyDown(KeyboardKeyEventArgs e)
    {
        base.OnKeyDown(e);

        _inputQueue.Enqueue(new RawKeyEvent(
            e.Key,
            e.Modifiers,
            e.IsRepeat
        ));
    }

    protected override void OnTextInput(TextInputEventArgs e)
    {
        base.OnTextInput(e);
        _inputQueue.Enqueue(new RawTextEvent((char)e.Unicode));
    }

    protected override void OnUnload()
    {
        GL.DeleteBuffer(_vbo);
        GL.DeleteVertexArray(_vao);
        GL.DeleteProgram(_shaderProgram);
        base.OnUnload();
    }

    private void OnWindowResize(ResizeEventArgs e)
    {
        GL.Viewport(0, 0, ClientSize.X, ClientSize.Y);
        UpdateProjection();
        UpdateViewport();
    }

    /// <summary>
    /// Updates the orthographic projection matrix so that pixel coordinates map
    /// directly to screen space with a top-left origin.
    /// </summary>
    private void UpdateProjection()
    {
        var projection = Matrix4.CreateOrthographicOffCenter(
            0, ClientSize.X,
            ClientSize.Y, 0,
            -1, 1
        );

        int location = GL.GetUniformLocation(_shaderProgram, "projection");
        GL.UniformMatrix4(location, false, ref projection);
    }

    /// <summary>
    /// Computes the rectangle used for rendering editor text.
    /// This rectangle represents the editor viewport and excludes
    /// future UI elements such as menu bars or status bars.
    /// </summary>
    /// <returns>
    /// A <see cref="Box2"/> defining the text viewport in screen coordinates.
    /// </returns>
    private Box2 GetTextViewport()
    {
        float top = 24f;
        float left = 24f;
        float bottom = 24f;

        Vector2 min = new(left, top);
        Vector2 max = new(
            ClientSize.X,
            ClientSize.Y - bottom
        );

        return new Box2(min, max);
    }

    /// <summary>
    /// Updates the editor viewport state based on the current text viewport size.
    /// </summary>
    private void UpdateViewport()
    {
        Box2 viewport = GetTextViewport();

        float glyphHeight = _bitmapFont.CellHeight * TextScale;
        int visibleRows = (int)(viewport.Size.Y / glyphHeight);

        _editor.SetVisibleRows(Math.Max(1, visibleRows));
    }

    /// <summary>
    /// Renders all visible lines of text and the cursor using the current viewport.
    /// </summary>
    private void RenderEditor()
    {
        Box2 viewport = GetTextViewport();
        float glyphHeight = _bitmapFont.CellHeight * TextScale;

        var editorViewport = _editor.Viewport;

        int startRow = editorViewport.ScrollRow;
        int endRow = Math.Min(
            startRow + editorViewport.VisibleRows,
            _editor.Buffer.GetLineCount()
        );

        for (int row = startRow; row < endRow; row++)
        {
            string line = _editor.Buffer.GetLine(row);
            int screenRow = row - startRow;

            float x = viewport.Min.X;
            float y = viewport.Min.Y + screenRow * glyphHeight;

            _renderer.DrawString(x, y, line, TextScale);
        }

        RenderCursor(viewport, glyphHeight);
    }

    /// <summary>
    /// Renders the cursor if it is currently within the visible viewport.
    /// </summary>
    /// <param name="viewport">
    /// The rectangle defining the text viewport.
    /// </param>
    /// <param name="glyphHeight">
    /// The height of a single glyph in screen pixels.
    /// </param>
    private void RenderCursor(Box2 viewport, float glyphHeight)
    {
        Cursor cursor = _editor.Cursor;
        var editorViewport = _editor.Viewport;

        if (cursor.Row < editorViewport.ScrollRow ||
            cursor.Row >= editorViewport.ScrollRow + editorViewport.VisibleRows)
        {
            return;
        }

        int screenRow = cursor.Row - editorViewport.ScrollRow;

        float x =
            viewport.Min.X +
            cursor.Col * _bitmapFont.CellWidth * TextScale;

        float y =
            viewport.Min.Y +
            screenRow * glyphHeight;

        _renderer.DrawRect(
            x,
            y,
            2f,
            glyphHeight,
            new Vector3(0f, 0f, 0f)
        );
    }

    /// <summary>
    /// Compiles an OpenGL shader from GLSL source code.
    /// </summary>
    /// <param name="type">The shader type.</param>
    /// <param name="source">The GLSL source code.</param>
    /// <returns>The OpenGL handle of the compiled shader.</returns>
    private static int CompileShader(ShaderType type, string source)
    {
        int shader = GL.CreateShader(type);
        GL.ShaderSource(shader, source);
        GL.CompileShader(shader);
        return shader;
    }

    private const string VertexShaderSource = @"
        #version 330 core
        layout (location = 0) in vec2 aPosition;
        layout (location = 1) in vec2 aTexCoord;

        uniform mat4 projection;
        out vec2 vTexCoord;

        void main()
        {
            vTexCoord = aTexCoord;
            gl_Position = projection * vec4(aPosition, 0.0, 1.0);
        }
    ";

    private const string FragmentShaderSource = @"
        #version 330 core
        in vec2 vTexCoord;
        out vec4 FragColor;

        uniform sampler2D tex;
        uniform int renderMode;
        uniform vec3 color;

        void main()
        {
            if (renderMode == 1)
            {
                vec4 c = texture(tex, vTexCoord);
                float alpha = 1.0 - c.r;
                FragColor = vec4(0.0, 0.0, 0.0, alpha);
            }
            else
            {
                FragColor = vec4(color, 1.0);
            }
        }
    ";
}
