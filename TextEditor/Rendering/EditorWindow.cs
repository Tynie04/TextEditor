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
    private BitmapFont _bitmapFont;

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
        _bitmapFont = BitmapFont.Load(
            "Assets/Fonts/unifont.bmp",
            16,
            16);

        Console.WriteLine($"Font loaded: {_bitmapFont.Columns}x{_bitmapFont.Rows}");
        
        Glyph gA = _bitmapFont.GetGlyph('A');
        Glyph g0 = _bitmapFont.GetGlyph('0');
        Glyph gQ = _bitmapFont.GetGlyph('?');

        Console.WriteLine($"A  : {gA.U0}, {gA.V0} -> {gA.U1}, {gA.V1}");
        Console.WriteLine($"0  : {g0.U0}, {g0.V0} -> {g0.U1}, {g0.V1}");
        Console.WriteLine($"?  : {gQ.U0}, {gQ.V0} -> {gQ.U1}, {gQ.V1}");


        
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

            void main()
            {
                FragColor = texture(tex, vTexCoord);
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
    /// Renders a solid color rectangle using two triangles.
    /// </summary>
    /// <param name="x">The X screen coordinate.</param>
    /// <param name="y">The Y screen coordinate.</param>
    /// <param name="width">Width in pixels.</param>
    /// <param name="height">Height in pixels.</param>
    /// <param name="color">The RGB color of the rectangle.</param>
    private void DrawRect(float x, float y, float width, float height, Vector3 color)
    {
        float[] vertices = {
            x, y,
            x + width, y,
            x + width, y + height,
            x, y,
            x + width, y + height,
            x, y + height
        };

        GL.BindBuffer(BufferTarget.ArrayBuffer, _vbo);
        GL.BufferData(BufferTarget.ArrayBuffer, vertices.Length * sizeof(float), vertices, BufferUsageHint.DynamicDraw);

        int colorLoc = GL.GetUniformLocation(_shaderProgram, "color");
        GL.Uniform3(colorLoc, color);

        GL.DrawArrays(PrimitiveType.Triangles, 0, 6);
    }
    
    /// <summary>
    /// Renders a textured rectangle (sprite) to the screen.
    /// </summary>
    /// <param name="x">The X screen coordinate.</param>
    /// <param name="y">The Y screen coordinate.</param>
    /// <param name="width">Width in pixels.</param>
    /// <param name="height">Height in pixels.</param>
    /// <param name="texture">The <see cref="Texture"/> to apply.</param>
    private void DrawTexturedRect(float x, float y, float width, float height, Texture texture)
    {
        float[] vertices =
        {
            // x, y, u, v
            x, y, 0f, 0f,
            x + width, y, 1f, 0f,
            x + width, y + height, 1f, 1f,

            x, y, 0f, 0f,
            x + width, y + height, 1f, 1f,
            x, y + height, 0f, 1f
        };

        GL.BindBuffer(BufferTarget.ArrayBuffer, _vbo);
        GL.BufferData(BufferTarget.ArrayBuffer, vertices.Length * sizeof(float), vertices, BufferUsageHint.DynamicDraw);

        texture.Bind(TextureUnit.Texture0);
        GL.Uniform1(GL.GetUniformLocation(_shaderProgram, "tex"), 0);

        GL.DrawArrays(PrimitiveType.Triangles, 0, 6);
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

        //DrawRect(10, 10, 100, 50, new Vector3(1.0f, 0.0f, 0.0f)); // Red rectangle
        //DrawRect(150, 100, 200, 100, new Vector3(0.0f, 1.0f, 0.0f)); // Green rectangle
        
        DrawTexturedRect(50, 50, 64, 64, _testTexture);

        
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