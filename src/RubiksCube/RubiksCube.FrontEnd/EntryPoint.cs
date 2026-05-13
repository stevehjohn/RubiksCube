namespace RubiksCube.FrontEnd;

public static class EntryPoint
{
    public static void Main()
    {
        using var cube = new RubiksCube();
        
        cube.Run();
    }
}