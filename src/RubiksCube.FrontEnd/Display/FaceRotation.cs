namespace RubiksCube.FrontEnd.Display;

public sealed class FaceRotation(Face face, bool clockwise)
{
    public Face Face { get; } = face;

    public bool Clockwise { get; } = clockwise;

    public float Elapsed { get; set; }
}