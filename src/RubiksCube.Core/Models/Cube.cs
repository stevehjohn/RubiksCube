namespace RubiksCube.Core.Models;

public class Cube
{
    private readonly Colour[][,] _faces = new Colour[6][,];

    public Cube()
    {
        foreach (var face in Enum.GetValues<Face>())
        {
            _faces[(int) face] = new Colour[6, 6];
        }
    }

    public Colour this[Face face, int x, int y]
    {
        get => _faces[(int) face][x, y];
        set => _faces[(int) face][x, y] = value;
    }

    public void ApplyMove(Face face, Direction direction)
    {
        var matrix = _faces[(int) face];
    }
}