namespace TextEditor.Rendering;

/// <summary>
/// Represents the normalized texture coordinates (UVs) for a single character glyph within a font atlas.
/// </summary>
/// <remarks>
/// These coordinates are used by the fragment shader to sample the correct portion of 
/// the font texture. Values typically range from 0.0 to 1.0.
/// </remarks>
public readonly struct Glyph
{
    public readonly float U0;
    public readonly float V0;
    public readonly float U1;
    public readonly float V1;
    
    /// <summary>
    /// Initializes a new instance of the <see cref="Glyph"/> struct with specific UV boundaries.
    /// </summary>
    /// <param name="u0">The left UV coordinate.</param>
    /// <param name="v0">The top UV coordinate.</param>
    /// <param name="u1">The right UV coordinate.</param>
    /// <param name="v1">The bottom UV coordinate.</param>
    public Glyph(float u0, float v0, float u1, float v1)
    {
        U0 = u0;
        V0 = v0;
        U1 = u1;
        V1 = v1;
    }
}