namespace Sudoku.Console;

public class ConsoleApplication
{
    private List<string> _files;
    
    public void Run()
    {
        Clear();

        EnumeratePuzzleFiles();

        Out("Please select the puzzle set to solve:");

        Out();

        var count = 1;
        
        foreach (var file in _files)
        {
            Out($"  {count,2}: {Path.GetFileNameWithoutExtension(file)}");

            count++;
        }
        
        Out();

        var id = In();
    }

    private static void Out(string text = null)
    {
        if (text == null)
        {
            System.Console.WriteLine();
        }
        else
        {
            System.Console.WriteLine($" {text}");
        }
    }

    private static void Clear()
    {
        System.Console.Clear();
        
        System.Console.WriteLine();
    }

    private static string In()
    {
        System.Console.Write(" ");
        
        return System.Console.ReadLine();
    }

    private void EnumeratePuzzleFiles()
    {
        _files = Directory.EnumerateFiles("Puzzles", "*.sudoku").ToList();
    }
}