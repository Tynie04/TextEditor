using System.Diagnostics;
using OpenTK.Graphics.OpenGL;
using OpenTK.Mathematics;
using OpenTK.Windowing.Common;
using OpenTK.Windowing.Desktop;
using TextEditor.Editor;
using TextEditor.Input;

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
    private Texture _testTexture;
    private readonly Queue<RawInputEvent> _inputQueue = new();
    private EditorController _editor;
    private Texture _planeTexture;
    private BitmapFont _bitmapFont;
    private TextRenderer _renderer;
    
    // TEMPORARY + DUMB
    private const float TextStartX = 20f;
    private const float TextStartY = 20f;
    private const float TextScale = 1.0f;


    /// <summary>
    /// Initializes a new instance of the <see cref="EditorWindow"/> class.
    /// </summary>
    /// <param name="gameSettings">General game engine settings.</param>
    /// <param name="nativeSettings">Native window settings such as resolution and title.</param>
    public EditorWindow(GameWindowSettings gameSettings, NativeWindowSettings nativeSettings)
        : base(gameSettings, nativeSettings)
    {
    }

    /// <summary>
    /// Called once when the window is first loaded. Handles OpenGL state setup, 
    /// shader compilation, and asset loading.
    /// </summary>
    protected override void OnLoad()
    {
        base.OnLoad();
        
        // Setup Alpha Blending for transparent PNG textures/fonts
        GL.Enable(EnableCap.Blend);
        GL.BlendFunc(BlendingFactor.SrcAlpha, BlendingFactor.OneMinusSrcAlpha);

        _editor = new EditorController();

        _testTexture = Texture.LoadFromFile("Assets/test.png");
        _planeTexture = Texture.LoadFromFile("Assets/transparent-plane.png");
        _bitmapFont = BitmapFont.Load(
            "Assets/Fonts/unifont.bmp",
            16,  // Cell width is 16 pixels
            16); // Cell height is 16 pixels

        Console.WriteLine($"Font loaded: {_bitmapFont.Columns}x{_bitmapFont.Rows}");
        
        Glyph gA = _bitmapFont.GetGlyph('A');
        Glyph g0 = _bitmapFont.GetGlyph('0');
        Glyph gQ = _bitmapFont.GetGlyph('?');

        Console.WriteLine($"A (65): U0={gA.U0:F4}, V0={gA.V0:F4} -> U1={gA.U1:F4}, V1={gA.V1:F4}");
        Console.WriteLine($"0 (48): U0={g0.U0:F4}, V0={g0.V0:F4} -> U1={g0.U1:F4}, V1={g0.V1:F4}");
        Console.WriteLine($"? (63): U0={gQ.U0:F4}, V0={gQ.V0:F4} -> U1={gQ.U1:F4}, V1={gQ.V1:F4}");


        
        GL.ClearColor(1.0f, 1.0f, 1.0f, 1.0f);
        
        // Vertex Shader: Projects 2D pixel coordinates to Normalized Device Coordinates (NDC)
        string vertexShaderSource = @"
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
        
        // Fragment Shader: Samples textures using UV coordinates
        string fragmentShaderSource = @"
            #version 330 core
            in vec2 vTexCoord;
            out vec4 FragColor;

            uniform sampler2D tex;
            uniform int renderMode;
            uniform vec3 color;

            void main()
            {
                if (renderMode == 0)
                {
                    // Normal textured image
                    FragColor = texture(tex, vTexCoord);
                }
                else if (renderMode == 1)
                {
                    // Bitmap font: black glyphs on white background
                    vec4 c = texture(tex, vTexCoord);
                    float alpha = 1.0 - c.r; // black = visible, white = transparent
                    FragColor = vec4(0.0, 0.0, 0.0, alpha);
                }
                else
                {
                    // Solid color rect (cursor)
                    FragColor = vec4(color, 1.0);
                }
            }

        ";

        int vertexShader = GL.CreateShader(ShaderType.VertexShader);
        GL.ShaderSource(vertexShader, vertexShaderSource);
        GL.CompileShader(vertexShader);

        int fragmentShader = GL.CreateShader(ShaderType.FragmentShader);
        GL.ShaderSource(fragmentShader, fragmentShaderSource);
        GL.CompileShader(fragmentShader);

        _shaderProgram = GL.CreateProgram();
        GL.AttachShader(_shaderProgram, vertexShader);
        GL.AttachShader(_shaderProgram, fragmentShader);
        GL.LinkProgram(_shaderProgram);

        GL.DeleteShader(vertexShader);
        GL.DeleteShader(fragmentShader);

        // Set up projection matrix for pixel coordinates (Top-Left is 0,0)
        GL.UseProgram(_shaderProgram);
        var projection = Matrix4.CreateOrthographicOffCenter(0, Size.X, Size.Y, 0, -1, 1);
        int projLoc = GL.GetUniformLocation(_shaderProgram, "projection");
        GL.UniformMatrix4(projLoc, false, ref projection);

        // Create Vertex Array Object and Vertex Buffer Object
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
       
        // Stride is 4 floats: 2 for position (X, Y) and 2 for TexCoords (U, V)
        int stride = 4 * sizeof(float);

        // Attribute 0: Position (x, y)
        GL.VertexAttribPointer(0, 2, VertexAttribPointerType.Float, false, stride, 0);
        GL.EnableVertexAttribArray(0);

        // Attribute 1: Texture coordinates (u, v)
        GL.VertexAttribPointer(1, 2, VertexAttribPointerType.Float, false, stride, 2 * sizeof(float));
        GL.EnableVertexAttribArray(1);
        
        Resize += OnWindowResize;
    }

    /// <summary>
    /// Responds to window resizing by updating the OpenGL viewport and the projection matrix.
    /// </summary>
    /// <param name="e">Arguments containing the new window dimensions.</param>
    private void OnWindowResize(ResizeEventArgs e)
    {
        GL.Viewport(0, 0, Size.X, Size.Y);
        
        // Update projection for new window size to maintain 1:1 pixel ratio
        GL.UseProgram(_shaderProgram);
        var projection = Matrix4.CreateOrthographicOffCenter(0, Size.X, Size.Y, 0, -1, 1);
        int projLoc = GL.GetUniformLocation(_shaderProgram, "projection");
        GL.UniformMatrix4(projLoc, false, ref projection);
    }
    
    /// <summary>
    /// Executes every frame. Handles the clearing of the buffer and calls to drawing routines.
    /// </summary>
    /// <param name="args">Timing information for the frame.</param>
    protected override void OnRenderFrame(FrameEventArgs args)
    {
        base.OnRenderFrame(args);
        
        GL.Clear(ClearBufferMask.ColorBufferBit);
        
        GL.UseProgram(_shaderProgram);
        GL.BindVertexArray(_vao);

        // _renderer.DrawString(10, 10, "hello world");
        RenderEditor();
        
        
        SwapBuffers();
    }
    
    private void RenderEditor()
    {
        float glyphWidth  = _bitmapFont.CellWidth * TextScale;
        float glyphHeight = _bitmapFont.CellHeight * TextScale;

        for (int row = 0; row < _editor.Buffer.GetLineCount(); row++)
        {
            string line = _editor.Buffer.GetLine(row);

            float x = TextStartX;
            float y = TextStartY + row * glyphHeight;

            _renderer.DrawString(x, y, line, TextScale);
        }

        RenderCursor(glyphWidth, glyphHeight);
    }

    private void RenderCursor(float glyphWidth, float glyphHeight)
    {
        Cursor cursor = _editor.Cursor;

        float cursorX = TextStartX + cursor.Col * glyphWidth;
        float cursorY = TextStartY + cursor.Row * glyphHeight;

        // Simple vertical caret
        _renderer.DrawRect(
            cursorX,
            cursorY,
            2f,
            glyphHeight,
            new Vector3(0f, 0f, 0f)
        );
    }

    
    protected override void OnUpdateFrame(FrameEventArgs args)
    {
        base.OnUpdateFrame(args);

        while (_inputQueue.Count > 0)
        {
            var input = _inputQueue.Dequeue();
            _editor.HandleRawInput(input);
        }
    }


    protected override void OnKeyDown(KeyboardKeyEventArgs e)
    {
        base.OnKeyDown(e);
        
        // Console.WriteLine(
        //     $"[KeyDown] Key={e.Key}, Modifiers={e.Modifiers}, Repeat={e.IsRepeat}"
        // );
        //
        // We forward the key info
        _inputQueue.Enqueue(new RawKeyEvent(
            Key: e.Key,
            Modifiers: e.Modifiers,
            IsRepeat: e.IsRepeat
        ));
    }

    protected override void OnTextInput(TextInputEventArgs  e)
    {
        base.OnTextInput(e);
        
        // Console.WriteLine(
        //     $"[TextInput] Char='{e.Unicode}' (U+{(int)e.Unicode:X4})"
        // );
        
        _inputQueue.Enqueue(new RawTextEvent((char)e.Unicode));
    }

    /// <summary>
    /// Cleans up GPU resources when the window is closed.
    /// </summary>
    protected override void OnUnload()
    {
        GL.DeleteBuffer(_vbo);
        GL.DeleteVertexArray(_vao);
        GL.DeleteProgram(_shaderProgram);
        base.OnUnload();
    }
}