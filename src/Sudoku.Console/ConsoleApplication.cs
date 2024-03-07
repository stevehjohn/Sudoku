using System.Diagnostics;
using System.IO.Compression;

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
            
            Out("   T: Test Suite\n");

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

                if (response == "t")
                {
                    RunTestSuite();
                    
                    Out();

                    Out("Press any key to continue.");

                    System.Console.ReadKey();

                    break;
                }

                if (response == "e")
                {
                    SolveUserPuzzle();

                    Out();

                    Out("Press any key to continue.");

                    System.Console.ReadKey();

                    break;
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

    private static void RunTestSuite()
    {
        // Easy, medium, hard, diabolical, min clues, benchmarks, 2m.
        var files = new[] { "Easy", "Medium", "Hard", "Diabolical", "Minimum Clues", "Benchmarks", "2 Million" };

        System.Console.Clear();

        System.Console.CursorVisible = false;
        
        System.Console.WriteLine();

        var stopwatch = Stopwatch.StartNew();

        System.Console.Write(" Warming up...");

        var solver = new BulkSolver(LoadPuzzles("Puzzles/Easy.zip"));
        
        solver.Solve(true, true);
        
        System.Console.WriteLine("\n");

        foreach (var file in files)
        {
            System.Console.Write(" Loading...");
            
            solver = new BulkSolver(LoadPuzzles($"Puzzles/{file}.zip"));

            System.Console.CursorLeft = 0;

            System.Console.Write("                    ");

            System.Console.CursorLeft = 0;
            
            System.Console.Write($" {file}: ");
            
            solver.Solve(true);

            System.Console.CursorVisible = false;
            
            System.Console.WriteLine();
        }
        
        stopwatch.Stop();
        
        System.Console.WriteLine($" Tests run in {stopwatch.Elapsed.Minutes:N0}:{stopwatch.Elapsed.Seconds:D2}.{stopwatch.Elapsed.Milliseconds:N0}.");

        System.Console.CursorVisible = true;
    }

    private static void SolveUserPuzzle()
    {
        Out();
        
        Out("Please enter the puzzle flattened into one row.");
        
        Out();
        
        Out("E.g. ......6....59.....82....8....45........3........6..3.54...325..6..................\n");

        var puzzle = new int[81];

        var clues = 0;
        
        retry:
        System.Console.Write($" Puzzle: ");

        var line = System.Console.ReadLine();

        try
        {
            for (var i = 0; i < 81; i++)
            {
                // ReSharper disable once PossibleNullReferenceException
                if (line[i] == '.')
                {
                    continue;
                }

                puzzle[i] = line[i] - '0';

                clues++;
            }
        }
        catch
        {
            Out("");
            
            Out("That appears to be an invalid line. Please try again.\n");
            
            goto retry;
        }

        var puzzles = new (int[] Puzzle, int Clues)[1];

        puzzles[0].Puzzle = puzzle;

        puzzles[0].Clues = clues;

        var solver = new BulkSolver(puzzles);
        
        solver.Solve();
    }

    private void SolvePuzzles(int fileId)
    {
        Out();
        
        Out("Loading puzzles...");

        var solver = new BulkSolver(LoadPuzzles(_files[fileId]));

        solver.Solve();
    }

    private static (int[] Puzzle, int Clues)[] LoadPuzzles(string filename)
    {
        using var file = File.OpenRead(filename);

        using var zip = new ZipArchive(file, ZipArchiveMode.Read);

        var lines = new List<string>();
        
        foreach(var entry in zip.Entries)
        {
            using var stream = entry.Open();

            using var reader = new StreamReader(stream);

            while (reader.ReadLine() is { } line)
            {
                if (line[0] == '\0')
                {
                    continue;
                }

                lines.Add(line);
            }
        }

        return ParseData(lines.ToArray());
    }

    private static (int[] Puzzle, int Clues)[] ParseData(string[] data)
    {
        var puzzles = new (int[] Puzzle, int Clues)[data.Length];

        var count = 0;
        
        foreach (var line in data)
        {
            var clues = 0;
            
            puzzles[count].Puzzle = new int[81];

            for (var i = 0; i < 81; i++)
            {
                var character = line[i];

                if (character is > '0' and <= '9')
                {
                    puzzles[count].Puzzle[i] = character - '0';

                    if (character != '0')
                    {
                        clues++;
                    }
                }
            }

            puzzles[count].Clues = clues;

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
        if (Debugger.IsAttached)
        {
            _files = Directory.EnumerateFiles("/Users/steve.john/Git/Sudoku/Puzzles", "*.zip").Order().ToList();
        }
        else
        {
            _files = Directory.EnumerateFiles("Puzzles", "*.zip").Order().ToList();
        }
    }
}