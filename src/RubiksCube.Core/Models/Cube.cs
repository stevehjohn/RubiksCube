namespace RubiksCube.Core.Models;

public class Cube
{
    private readonly Colour[][,] _faces = new Colour[6][,];

    private readonly Face[][] _effectedEdges =
    [
        [Face.Front, Face.Left, Face.Back, Face.Right], // Up
        [Face.Right, Face.Back, Face.Left, Face.Front], // Down
        [Face.Up, Face.Right, Face.Down, Face.Left],    // Front,
        [Face.Left, Face.Down, Face.Right, Face.Up],    // Back,
        [Face.Up, Face.Front, Face.Down, Face.Back],    // Left,
        [Face.Back, Face.Down, Face.Front, Face.Up]     // Right,
    ];

    public Cube()
    {
        foreach (var face in Enum.GetValues<Face>())
        {
            _faces[(int) face] = new Colour[3, 3];
        }
    }

    public Colour this[Face face, int x, int y]
    {
        get => _faces[(int) face][x, y];
        set => _faces[(int) face][x, y] = value;
    }

    public void ApplyMove(Face face, Direction direction)
    {
        RotateFace(face, direction);

        RotateEdges(face, direction);
    }

    private void RotateFace(Face face, Direction direction)
    {
        var matrix = _faces[(int) face];

        if (direction == Direction.Clockwise)
        {
            (matrix[0, 0], matrix[2, 0], matrix[2, 2], matrix[0, 2]) = (matrix[2, 0], matrix[2, 2], matrix[0, 2], matrix[0, 0]);

            (matrix[1, 0], matrix[2, 1], matrix[1, 2], matrix[0, 1]) = (matrix[2, 1], matrix[1, 2], matrix[0, 1], matrix[1, 0]);
        }
        else
        {
            (matrix[0, 0], matrix[2, 0], matrix[2, 2], matrix[0, 2]) = (matrix[0, 2], matrix[0, 0], matrix[2, 0], matrix[2, 2]);

            (matrix[1, 0], matrix[2, 1], matrix[1, 2], matrix[0, 1]) = (matrix[0, 1], matrix[1, 0], matrix[2, 1], matrix[1, 2]);
        }
    }

    private void RotateEdges(Face face, Direction direction)
    {
    }
}