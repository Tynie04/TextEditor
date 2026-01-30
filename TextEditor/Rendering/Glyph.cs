namespace TextEditor.Rendering;

public readonly struct Glyph
{
    public readonly float U0;
    public readonly float V0;
    public readonly float U1;
    public readonly float V1;
    
    public Glyph(float u0, float v0, float u1, float v1)
    {
        U0 = u0;
        V0 = v0;
        U1 = u1;
        V1 = v1;
    }
}