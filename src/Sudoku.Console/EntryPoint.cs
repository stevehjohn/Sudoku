namespace Sudoku.Console;

public static class EntryPoint
{
    public static void Main()
    {
        var application = new ConsoleApplication();

        System.Console.CancelKeyPress += (_, _) => System.Console.CursorVisible = true; 

        application.Run();
    }
}