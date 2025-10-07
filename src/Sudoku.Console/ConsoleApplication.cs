using System.Diagnostics;
using System.IO.Compression;
using Sudoku.Extensions;

namespace Sudoku.Console;

public class ConsoleApplication
{
    private List<string> _files;

    private readonly object _lock = new();
    
    public void Run()
    {
        while (true)
        {
            Clear();

            EnumeratePuzzleFiles();

            Out("Please select the puzzle set to solve or an option below:");

            Out();

            var count = 1;

            foreach (var file in _files)
            {
                var name = Path.GetFileName(file);

                name = name[..name.IndexOf('.')];

                Out($"  {count,2}: {name}");

                count++;
            }

            Out();
            
            Out("   B: Generate tree branch diagram for last most steps");

            Out("   V: Visualise last most steps");

            Out("   L: See log of last most steps");

            Out("   E: Enter manually");

            Out("   T: Test suite");

            Out("   G: Generate puzzles");

            Out("   Q: Exit application\n");

            while (true)
            {
                var response = In().ToLower().Trim();

                if (response == "q")
                {
                    Out();

                    Out("Thanks for playing. Bye.\n");

                    return;
                }

                if (response == "b")
                {
                    TreeMostSteps();
                    
                    Out();

                    Out("Press any key to continue.");

                    System.Console.ReadKey();
                    
                    break;
                }

                if (response == "v")
                {
                    VisualiseMostSteps();
                    
                    Out();

                    Out("Press any key to continue.");

                    System.Console.ReadKey();
                    
                    break;
                }

                if (response == "l")
                {
                    DumpMostStepsLog();
                    
                    Out();

                    Out("Press any key to continue.");

                    System.Console.ReadKey();
                    
                    break;
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

                if (response == "g")
                {
                    GeneratePuzzles();

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

    private static void TreeMostSteps()
    {
        System.Console.Clear();
        
        var puzzle = LoadPuzzles($"{FileHelper.GetPuzzlesPath()}/Most Steps.txt")[0];

        var filename = $"Tree {DateTime.UtcNow:yy-MM-dd hh-mm-ss}";
        
        TreeGenerator.Generate(puzzle.Puzzle, filename);
        
        Out($"Tree file saved to {filename}.html");
        
        Process.Start(new ProcessStartInfo
        {
            FileName = $"{filename}.html",
            UseShellExecute = true
        });
    }

    private static void DumpMostStepsLog()
    {
        System.Console.Clear();
        
        var puzzle = LoadPuzzles($"{FileHelper.GetPuzzlesPath()}/Most Steps.txt")[0];

        puzzle.Puzzle.DumpToConsole(1);
        
        var solver = new Solver(HistoryType.AllSteps, SolveMethod.FindFirst);

        var result = solver.Solve(puzzle.Puzzle);

        Out();

        result.LogToConsole();

        Out();
        
        Out($"Solved in {result.ElapsedMicroseconds:N0}μs with {result.History.Count:N0} steps taken.\n");
        
        result.DumpToConsole(1);
    }

    private static void VisualiseMostSteps()
    {
        var puzzle = LoadPuzzles($"{FileHelper.GetPuzzlesPath()}/Most Steps.txt")[0];

        var solver = new Solver(HistoryType.AllSteps, SolveMethod.FindFirst);

        var result = solver.Solve(puzzle.Puzzle);

        var visualiser = new ConsoleSolveVisualiser(puzzle.Puzzle, result);
        
        visualiser.Visualise(1, 1);
    }

    private void GeneratePuzzles()
    {
        System.Console.Write("\n Clues to leave (17 - 72): ");

        var response = System.Console.ReadLine();

        if (! int.TryParse(response, out var clues))
        {
            Out("\n Invalid input.");

            return;
        }

        if (clues is < 17 or > 72)
        {
            Out("\n Invalid input.");

            return;
        }

        System.Console.Write("\n Number of puzzles to generate: ");

        response = System.Console.ReadLine();

        if (! int.TryParse(response, out var puzzles))
        {
            Out("\n Invalid input.");

            return;
        }

        GeneratePuzzles(clues, puzzles);
    }

    private void GeneratePuzzles(int clues, int puzzleCount)
    {
        System.Console.Clear();

        var stopwatch = Stopwatch.StartNew();

        System.Console.CursorVisible = false;

        var filename = $"{FileHelper.GetPuzzlesPath()}/Generated.txt";

        if (File.Exists(filename))
        {
            File.Delete(filename);
        }

        Out($"\n {DateTime.Now:ddd d MMM HH:mm:ss}: Generating {clues} clue puzzle{(puzzleCount > 1 ? "s" : string.Empty)}...\n");

        var recent = new List<string>();

        var recentLock = new object();

        var generated = 0;

        int[] lastPuzzle = null;

        var cancellationTokenSource = new CancellationTokenSource();

        var cancellationToken = cancellationTokenSource.Token;

        using var stream = new FileStream(filename, FileMode.Create);

        using var writer = new StreamWriter(stream);
        
        writer.AutoFlush = true;

        var top = 0;
                
        Parallel.For(0, int.MaxValue,
            new ParallelOptions { MaxDegreeOfParallelism = Environment.ProcessorCount - 1 },
            (_, state) =>
            {
                if (state.IsStopped)
                {
                    return;
                }

                lock (_lock)
                {
                    System.Console.CursorTop = 3;

                    System.Console.WriteLine($" Puzzle {Math.Min(generated, puzzleCount):N0}/{puzzleCount:N0}.               \n");
                }

                if (generated >= puzzleCount)
                {
                    return;
                }

                var generator = new Generator();

                if (puzzleCount == 1)
                {
                    generator.AttemptHook = ShowAttemptCount;
                }

                var result = generator.Generate(clues, cancellationToken);

                if (! result.Succeeded)
                {
                    return;
                }

                var puzzle = result.Puzzle;

                lastPuzzle = puzzle;

                var puzzleString = $"{string.Join(string.Empty, puzzle).Replace('0', '.')}";

                lock (recentLock)
                {
                    recent.Insert(0, puzzleString);

                    if (recent.Count > 20)
                    {
                        recent.RemoveAt(20);
                    }
                }

                lock (_lock)
                {
                    if (generated < puzzleCount)
                    {
                        System.Console.CursorTop = 3;

                        System.Console.WriteLine($" Puzzle {generated + 1:N0}/{puzzleCount:N0}.               \n");

                        lock (recentLock)
                        {
                            foreach (var item in recent)
                            {
                                System.Console.WriteLine($" {item}");
                            }
                        }
                        
                        // ReSharper disable once AccessToDisposedClosure
                        writer.WriteLine(puzzleString);

                        top = System.Console.CursorTop;
                    }
                }

                var count = Interlocked.Increment(ref generated);

                if (count >= puzzleCount)
                {
                    cancellationTokenSource.Cancel();
                    
                    stopwatch.Stop();
                    
                    state.Stop();
                }
            });

        stopwatch.Stop();

        System.Console.CursorTop = top;

        Out($"\n Puzzles have been written to {filename}.");

        var rate = puzzleCount / stopwatch.Elapsed.TotalSeconds;

        var rateText = rate < 1
            ? $"{stopwatch.Elapsed.TotalSeconds / puzzleCount:N2} second(s)/puzzle"
            : $"{puzzleCount / stopwatch.Elapsed.TotalSeconds:N2} puzzle(s)/second";
        
        Out($"\n {puzzleCount:N0} {clues} clue puzzle(s) generated in {stopwatch.Elapsed:dd\\.hh\\:mm\\:ss\\.fff}, {rateText}.");

        if (lastPuzzle != null)
        {
            System.Console.WriteLine();
            
            System.Console.WriteLine(" Last puzzle:");
            
            System.Console.WriteLine();
            
            lastPuzzle.DumpToConsole(1);
        }

        System.Console.CursorVisible = true;
    }

    private static void ShowAttemptCount(int count)
    {
        var x = System.Console.CursorLeft;
        
        System.Console.Write($" Attempt: {count}.");

        System.Console.CursorLeft = x;
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

        var solver = new BulkSolver(LoadPuzzles($"{FileHelper.GetPuzzlesPath()}/Easy.zip"));

        solver.Solve(true, true);

        System.Console.WriteLine("\n");

        foreach (var file in files)
        {
            System.Console.Write(" Loading...");

            solver = new BulkSolver(LoadPuzzles($"{FileHelper.GetPuzzlesPath()}/{file}.zip"));

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
        System.Console.Write(" Puzzle: ");

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
        if (! filename.EndsWith('1'))
        {
            return LoadPuzzlesInternal(filename);
        }

        var puzzles = new List<(int[] Puzzle, int Clues)>();

        puzzles.AddRange(LoadPuzzlesInternal(filename));

        filename = $"{filename[..^1]}2";

        puzzles.AddRange(LoadPuzzlesInternal(filename));

        return puzzles.ToArray();
    }

    private static (int[] Puzzle, int Clues)[] LoadPuzzlesInternal(string filename)
    {
        if (Path.GetExtension(filename).Equals(".txt", StringComparison.InvariantCultureIgnoreCase))
        {
            var data = File.ReadAllLines(filename);

            return ParseData(data);
        }

        using var file = File.OpenRead(filename);

        using var zip = new ZipArchive(file, ZipArchiveMode.Read);

        var lines = new List<string>();

        foreach (var entry in zip.Entries)
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

                if (character is <= '0' or > '9')
                {
                    continue;
                }
                
                puzzles[count].Puzzle[i] = character - '0';

                if (character != '0')
                {
                    clues++;
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
        _files = Directory.EnumerateFiles(FileHelper.GetPuzzlesPath(), "*").Order().ToList();

        _files.RemoveAll(f => f.Contains(".DS_"));

        _files.RemoveAll(f => f.EndsWith('2'));
    }
}