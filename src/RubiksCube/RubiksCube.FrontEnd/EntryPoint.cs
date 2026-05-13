namespace RubiksCube.FrontEnd;

public static class EntryPoint
{
    public static void Main()
    {
        using var cube = new Display.RubiksCube();
        
        cube.Run();
    }
}