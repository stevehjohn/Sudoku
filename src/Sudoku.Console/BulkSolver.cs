using System.Diagnostics;
using System.Text;
using Sudoku.Solver;

namespace Sudoku.Console;

public class BulkSolver
{
    private readonly int[][] _puzzles;

    private readonly int _puzzleCount;
    
    private (double Total, double Minimum, double Maximum) _elapsed = (0, double.MaxValue, 0);

    private (long Total, int Minimum, int Maximum) _steps = (0, int.MaxValue, 0);

    private int _maxStepsPuzzleNumber;

    private int _maxTimePuzzleNumber;

    private Stopwatch _stopwatch;

    private readonly StringBuilder _output = new(10_000);

    private readonly object _statsLock = new();

    private readonly object _consoleLock = new();
    
    public BulkSolver(int[][] puzzles)
    {
        _puzzles = puzzles;

        _puzzleCount = puzzles.Length;
    }

    public void Solve()
    {
        var solved = 0;
        
        System.Console.Clear();

        System.Console.CursorVisible = false;
        
        _stopwatch = Stopwatch.StartNew();

        var solver = new Solver.Solver();

        var record = _puzzles.Length == 1;
        
        Parallel.For(
            0, 
            _puzzles.Length,
            new ParallelOptions
            {
                MaxDegreeOfParallelism = Environment.ProcessorCount - 1
            },
            i => 
            {
                var solution = solver.Solve(_puzzles[i], record);

                lock (_statsLock)
                {
                    var totalMicroseconds = solution.Microseconds;
                    
                    _elapsed.Total += totalMicroseconds;

                    _elapsed.Minimum = Math.Min(_elapsed.Minimum, totalMicroseconds);

                    if (totalMicroseconds > _elapsed.Maximum)
                    {
                        _maxTimePuzzleNumber = i;

                        _elapsed.Maximum = totalMicroseconds;
                    }

                    _steps.Total += solution.Steps;

                    _steps.Minimum = Math.Min(_steps.Minimum, solution.Steps);

                    if (solution.Steps > _steps.Maximum)
                    {
                        _maxStepsPuzzleNumber = i;
                        
                        _steps.Maximum = solution.Steps;
                    }

                    solved++;
                }

                Dump(_puzzles[i], solution.Solution, solved);

                if (record)
                {
                    DumpHistory(_puzzles[i], solution.History);
                }
            });

        _stopwatch.Stop();

        System.Console.WriteLine($"\n All puzzles solved in: {_stopwatch.Elapsed.Minutes:N0}:{_stopwatch.Elapsed.Seconds:D2}.");
        
        System.Console.CursorVisible = true;
    }

    private void DumpHistory(int[] puzzle, List<Move> solution)
    {
        for (var y = 0; y < 9; y++)
        {
            System.Console.CursorTop = y + 1;

            System.Console.CursorLeft = 23;
            
            for (var x = 0; x < 9; x++)
            {
                if (puzzle[x + y * 9] == 0)
                {
                    System.Console.Write("  ");
                }
                else
                {
                    System.Console.Write($"{puzzle[x + y * 9]} ");
                }
            }
        }

        var color = System.Console.ForegroundColor;

        var sleep = true;
        
        foreach (var move in solution)
        {
            if (System.Console.KeyAvailable)
            {
                sleep = false;

                System.Console.ReadKey();
            }

            for (var i = 0; i < 5; i++)
            {
                System.Console.ForegroundColor = ConsoleColor.Magenta;

                System.Console.CursorTop = 1 + move.Y;

                System.Console.CursorLeft = 23 + move.X * 2;

                if (i % 2 == 0)
                {
                    System.Console.Write(move.Value);
                }
                else
                {
                    System.Console.Write(" ");
                }

                if (sleep)
                {
                    Thread.Sleep(100);
                }
            }

            if (sleep)
            {
                Thread.Sleep(500);
            }

            System.Console.ForegroundColor = color;

            System.Console.CursorTop = 1 + move.Y;

            System.Console.CursorLeft = 23 + move.X * 2;
            
            System.Console.Write(move.Value);
        }

        System.Console.CursorTop = 28;
    }

    private void Dump(int[] left, int[] right, int solved)
    {
        lock (_consoleLock)
        {
            _output.Clear();
        
            for (var y = 0; y < 9; y++)
            {
                for (var x = 0; x < 9; x++)
                {
                    if (left[x + y * 9] == 0)
                    {
                        _output.Append("  ");
                    }
                    else
                    {
                        _output.Append($" {left[x + y * 9]}");
                    }
                }

                _output.Append("    ");
                
                for (var x = 0; x < 9; x++)
                {
                    if (right[x + y * 9] == 0)
                    {
                        _output.Append("  ");
                    }
                    else
                    {
                        _output.Append($" {right[x + y * 9]}");
                    }
                }
                
                _output.AppendLine();
            }
            
            if (solved > 0)
            {
                _output.AppendLine($"\n Solved: {solved:N0}/{_puzzleCount:N0} puzzles ({solved / _stopwatch.Elapsed.TotalSeconds:N0} puzzles/sec).       \n");

                var mean = _elapsed.Total / solved;

                _output.AppendLine($" Timings...\n  Minimum: {_elapsed.Minimum:N0}μs          \n  Mean:    {mean:N0}μs          \n  Maximum: {_elapsed.Maximum:N0}μs (Puzzle #{_maxTimePuzzleNumber:N0})         \n");
                
                _output.AppendLine($" Combinations...\n  Minimum: {_steps.Minimum:N0}          \n  Mean:    {_steps.Total / solved:N0}          \n  Maximum: {_steps.Maximum:N0} (Puzzle #{_maxStepsPuzzleNumber:N0})           \n");

                var meanTime = _stopwatch.Elapsed.TotalSeconds / solved;
                
                var eta = TimeSpan.FromSeconds((_puzzles.Length - solved) * meanTime);
                
                _output.AppendLine($" Elapsed time: {_stopwatch.Elapsed.Minutes:N0}:{_stopwatch.Elapsed.Seconds:D2}    Estimated remaining: {eta.Hours:N0}:{eta.Minutes:D2}:{eta.Seconds:D2}          \n");
                
                var percent = 100 - (_puzzleCount - solved) * 100d / _puzzleCount;

                _output.AppendLine($" Solved: {Math.Floor(percent):N0}%\n");

                var line = (int) Math.Floor(percent / 2);

                if (Math.Floor(percent) > 0 && (int) Math.Floor(percent) % 2 == 1)
                {
                    _output.AppendLine($" {new string('\u2588', line)}\u258c{new string('-', 49 - line)}");
                }
                else
                {
                    _output.AppendLine($" {new string('\u2588', line)}{new string('-', 50 - line)}");
                }
            }

            System.Console.CursorTop = 1;
        
            System.Console.Write(_output.ToString());
        }
    }
}