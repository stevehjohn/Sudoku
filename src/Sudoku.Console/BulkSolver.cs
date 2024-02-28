using System.Diagnostics;
using System.Text;

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

    private int _maxFilled;
    
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

        if (_puzzles.Length == 1)
        {
            solver.StepHook = (state, step, stack) => Dump(_puzzles[0], state, 0, step, stack);
        }

        Parallel.For(
            0, 
            _puzzles.Length,
            new ParallelOptions
            {
                MaxDegreeOfParallelism = Environment.ProcessorCount - 1
            },
            i => 
            {
                var solution = solver.Solve(_puzzles[i]);

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

                if (_puzzles.Length > 1)
                {
                    Dump(_puzzles[i], solution.Solution, solved);
                }
            });

        _stopwatch.Stop();

        System.Console.WriteLine($" All puzzles solved in: {_stopwatch.Elapsed.Minutes:N0}:{_stopwatch.Elapsed.Seconds:D2}.");
        
        System.Console.CursorVisible = true;
    }

    private void Dump(int[] left, int[] right, int solved, int step = -1, int stack = -1)
    {
        var filled = 0;
        
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

                        filled++;
                    }
                }
                
                _output.AppendLine();
            }
            
            if (solved > 0 && _puzzles.Length > 1)
            {
                _output.AppendLine($"\n Solved: {solved:N0}/{_puzzleCount:N0} puzzles ({solved / _stopwatch.Elapsed.TotalSeconds:N0} puzzles/sec).       \n");

                var mean = _elapsed.Total / solved;

                _output.AppendLine($" Timings...\n  Minimum: {_elapsed.Minimum:N0}μs          \n  Mean:    {mean:N0}μs          \n  Maximum: {_elapsed.Maximum:N0}μs (Puzzle #{_maxTimePuzzleNumber:N0})         \n");
                
                _output.AppendLine($" Combinations...\n  Minimum: {_steps.Minimum:N0}          \n  Mean:    {_steps.Total / solved:N0}          \n  Maximum: {_steps.Maximum:N0} (Puzzle #{_maxStepsPuzzleNumber:N0})           \n");

                var meanTime = _stopwatch.Elapsed.TotalSeconds / solved;
                
                var eta = TimeSpan.FromSeconds((_puzzles.Length - solved) * meanTime);
                
                _output.AppendLine($" Elapsed time: {_stopwatch.Elapsed.Minutes:N0}:{_stopwatch.Elapsed.Seconds:D2}    Estimated remaining: {eta.Hours:N0}:{eta.Minutes:N0}:{eta.Seconds:D2}          \n");
                
                var percent = 100 - (_puzzleCount - solved) * 100d / _puzzleCount;

                _output.AppendLine($" Solved: {Math.Floor(percent):N0}%\n");

                var line = (int) Math.Floor(percent / 2);

                if (Math.Floor(percent) > 0 && (int) Math.Floor(percent) % 2 == 1)
                {
                    _output.AppendLine($" {new string('\u2588', line)}\u258c{new string('-', 49 - line)}\n");
                }
                else
                {
                    _output.AppendLine($" {new string('\u2588', line)}{new string('-', 50 - line)}\n");
                }
            }

            if (step > -1)
            {
                _output.AppendLine($"\n Steps: {step:N0}    Stack size: {stack}    ");

                _maxFilled = Math.Max(_maxFilled, filled);
                
                _output.AppendLine($"\n Most filled: {_maxFilled}    Filled: {filled}    \n");
            }

            System.Console.CursorTop = 1;
        
            System.Console.Write(_output.ToString());
        }
    }
}