namespace Rubiks.FrontEnd;

public static class EntryPoint
{
    public static void Main()
    {
        using var game = new RubiksCube();

        game.Run();
    }
}