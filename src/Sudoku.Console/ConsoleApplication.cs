namespace Sudoku.Console;

public class ConsoleApplication
{
    private List<string> _files;
    
    public void Run()
    {
        while (true)
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
            
            Out("   E: Enter manually\n");

            Out("   Q: Exit application\n");

            while (true)
            {
                var response = In().ToLower();

                if (response == "q")
                {
                    Out();

                    Out("Thanks for playing. Bye.\n");

                    return;
                }

                if (int.TryParse(response, out var id))
                {
                    if (id > _files.Count)
                    {
                        Out();
                        
                        Out("Invalid puzzle set number, please try again.\n");
                        
                        continue;
                    }

                    SolvePuzzles(id - 1);

                    Out();

                    Out("Press any key to continue.");

                    System.Console.ReadKey();
                    
                    break;
                }
                
                Out();
                
                Out("Unknown command, please try again.\n");
            }
        }
    }

    private void SolvePuzzles(int fileId)
    {
        Out();
        
        Out("Loading puzzles...");
        
        var data = File.ReadAllLines(_files[fileId]);

        var puzzles = ParseData(data);
        
        var solver = new BulkSolver(puzzles);
    }

    private int[][] ParseData(string[] data)
    {
        var puzzles = new int[data.Length][];

        var count = 0;
        
        foreach (var line in data)
        {
            puzzles[count] = new int[81];

            for (var i = 0; i < 81; i++)
            {
                var character = line[i];

                if (character is > '0' and <= '9')
                {
                    puzzles[count][i] = character - '0';
                }
            }

            count++;
        }

        return puzzles;
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
        System.Console.Write(" > ");
        
        return System.Console.ReadLine();
    }

    private void EnumeratePuzzleFiles()
    {
        _files = Directory.EnumerateFiles("Puzzles", "*.sudoku").ToList();
    }
}