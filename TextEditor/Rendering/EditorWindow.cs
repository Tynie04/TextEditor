using System.Diagnostics;
using OpenTK.Graphics.OpenGL;
using OpenTK.Mathematics;
using OpenTK.Windowing.Common;
using OpenTK.Windowing.Desktop;

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
    private Texture _planeTexture;
    private BitmapFont _bitmapFont;
    private TextRenderer _renderer;


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


        
        GL.ClearColor(0.2f, 0.3f, 0.3f, 1.0f);
        
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

            void main()
            {
                vec4 c = texture(tex, vTexCoord);

                if (renderMode == 0)
                {
                    // Normal image: use RGBA as-is
                    FragColor = c;
                }
                else
                {
                    // Bitmap font:
                    // White background → transparent
                    float alpha = 1.0 - c.r;   // assumes white background, black glyphs
                    FragColor = vec4(0.0, 0.0, 0.0, alpha);
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

        // Test drawing individual characters
        _renderer.DrawGlyph(50, 50, 'H', 2.0f);
        _renderer.DrawGlyph(82, 50, 'e', 2.0f);
        _renderer.DrawGlyph(114, 50, 'l', 2.0f);
        _renderer.DrawGlyph(146, 50, 'l', 2.0f);
        _renderer.DrawGlyph(178, 50, 'o', 2.0f);
        
        _renderer.DrawGlyph(50, 100, 'W', 2.0f);
        _renderer.DrawGlyph(82, 100, 'o', 2.0f);
        _renderer.DrawGlyph(114, 100, 'r', 2.0f);
        _renderer.DrawGlyph(146, 100, 'l', 2.0f);
        _renderer.DrawGlyph(178, 100, 'd', 2.0f);
        _renderer.DrawGlyph(210, 100, '!', 2.0f);

        // Test different characters at normal scale
        _renderer.DrawGlyph(50, 150, 'A');
        _renderer.DrawGlyph(70, 150, 'B');
        _renderer.DrawGlyph(90, 150, 'C');
        _renderer.DrawGlyph(110, 150, '0');
        _renderer.DrawGlyph(130, 150, '1');
        _renderer.DrawGlyph(150, 150, '2');
        _renderer.DrawGlyph(170, 150, '?');
        
        
        _renderer.DrawString(300, 50, "Hello");
        _renderer.DrawString(300, 80, "Hello\nWorld!");
        _renderer.DrawString(300, 140, "Scaled Text", 1.5f);
        _renderer.DrawString(300, 190, "ABC 012 ?!");
                  
        //_renderer.DrawTexturedRect(200, 200, 64, 64, _testTexture);
        _renderer.DrawTexturedRect(300, 240, 200, 205, _planeTexture);
        
        SwapBuffers();
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