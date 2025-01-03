using System.Diagnostics;
using System.Runtime;
using System.Text;

namespace Sudoku.Console;

public class BulkSolver
{
    private readonly (int[] Puzzle, int Clues)[] _puzzles;

    private readonly int _puzzleCount;

    private (double Total, double Minimum, double Maximum) _elapsed = (0, double.MaxValue, 0);

    private (long Total, int Minimum, int Maximum) _steps = (0, int.MaxValue, 0);

    private readonly Dictionary<int, (int Count, double Elapsed)> _timings = new();

    private int _maxStepsPuzzleNumber;

    private int _mostStepsClues;

    private int _maxTimePuzzleNumber;

    private Stopwatch _stopwatch;

    private readonly StringBuilder _output = new(10_000);

    private readonly object _statsLock = new();

    private readonly object _consoleLock = new();

    public BulkSolver((int[] Puzzle, int Clues)[] puzzles)
    {
        _puzzles = puzzles;

        _puzzleCount = puzzles.Length;
    }

    public void Solve(bool quiet = false, bool noSummary = false)
    {
        var solved = 0;

        if (! quiet)
        {
            System.Console.Clear();
        }

        System.Console.CursorVisible = false;

        _stopwatch = Stopwatch.StartNew();

        var historyType = _puzzles.Length == 1 ? HistoryType.AllSteps : HistoryType.None;

        var oldMode = GCSettings.LatencyMode;

        GCSettings.LatencyMode = GCLatencyMode.SustainedLowLatency;

        Parallel.For(
            0,
            _puzzles.Length,
            new ParallelOptions
            {
                MaxDegreeOfParallelism = Environment.ProcessorCount - 1
            },
            () => new Solver(historyType, SolveMethod.FindFirst),
            (i, _, solver) =>
            {
                var solution = solver.Solve(_puzzles[i].Puzzle);

                if (! quiet)
                {
                    lock (_statsLock)
                    {
                        var totalMicroseconds = solution.ElapsedMicroseconds;

                        var clues = _puzzles[i].Clues;

                        if (! _timings.TryGetValue(clues, out var value))
                        {
                            value = (0, 0);

                            _timings[clues] = value;
                        }

                        _timings[clues] = (value.Count + 1, value.Elapsed + solution.ElapsedMicroseconds);

                        _elapsed.Total += totalMicroseconds;

                        _elapsed.Minimum = Math.Min(_elapsed.Minimum, totalMicroseconds);

                        if (totalMicroseconds > _elapsed.Maximum)
                        {
                            _maxTimePuzzleNumber = i;

                            _elapsed.Maximum = totalMicroseconds;
                        }

                        _steps.Total += solution.Steps;

                        if (solution.Steps < _steps.Minimum)
                        {
                            _steps.Minimum = solution.Steps;
                            
                            File.WriteAllText($"{FileHelper.GetPuzzlesPath()}/Least Steps.txt", string.Join(string.Empty, _puzzles[i].Puzzle).Replace('0', '.'));
                        }

                        if (solution.Steps > _steps.Maximum)
                        {
                            _maxStepsPuzzleNumber = i;

                            _steps.Maximum = solution.Steps;

                            _mostStepsClues = clues;

                            File.WriteAllText($"{FileHelper.GetPuzzlesPath()}/Most Steps.txt", string.Join(string.Empty, _puzzles[i].Puzzle).Replace('0', '.'));
                        }
                    }
                }

                if (! quiet)
                {
                    lock (_statsLock)
                    {
                        var totalMicroseconds = solution.ElapsedMicroseconds;

                        var clues = _puzzles[i].Clues;

                        if (! _timings.TryGetValue(clues, out var value))
                        {
                            value = (0, 0);

                            _timings[clues] = value;
                        }

                        _timings[clues] = (value.Count + 1, value.Elapsed + solution.ElapsedMicroseconds);

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
                    }
                }

                lock (_statsLock)
                {
                    solved++;
                }

                if (quiet)
                {
                    ShowProgress(solved, noSummary);
                }
                else
                {
                    Dump(_puzzles[i].Puzzle, solution, solved);
                }

                if (historyType != HistoryType.None)
                {
                    DumpHistory(_puzzles[i].Puzzle, solution.History, historyType);
                }

                return solver;
            },
            _ => { });

        _stopwatch.Stop();

        GCSettings.LatencyMode = oldMode;

        if (! quiet)
        {
            if (solved == 1)
            {
                System.Console.WriteLine("\n");
            }

            System.Console.WriteLine($"\n All puzzles solved in: {_stopwatch.Elapsed.Minutes:N0}:{_stopwatch.Elapsed.Seconds:D2}.{_stopwatch.Elapsed.Milliseconds:N0}.\n");

            System.Console.WriteLine(" Clues...");

            var i = 0;

            foreach (var timing in _timings.OrderBy(t => t.Key))
            {
                System.Console.Write($"  {timing.Key}: {$"{timing.Value.Elapsed / timing.Value.Count:N0}",5}μs  ");

                i++;

                if (i != 4)
                {
                    continue;
                }
                
                System.Console.WriteLine();

                i = 0;
            }

            if (i > 0)
            {
                System.Console.WriteLine();
            }
        }
        else
        {
            if (! noSummary)
            {
                var perSecond = solved / _stopwatch.Elapsed.TotalSeconds;

                System.Console.WriteLine(_stopwatch.Elapsed.TotalSeconds < 1
                    ? $" puzzles solved in {_stopwatch.Elapsed.TotalMicroseconds:N0}μs, {perSecond:N0}/sec."
                    : $" puzzles solved in {_stopwatch.Elapsed.Minutes:N0}:{_stopwatch.Elapsed.Seconds:D2}.{_stopwatch.Elapsed.Milliseconds:N0}, {perSecond:N0}/sec.");
            }
        }

        System.Console.CursorVisible = true;
    }

    private void ShowProgress(int solved, bool noSummary)
    {
        if (solved != _puzzleCount)
        {
            if (! Monitor.TryEnter(_consoleLock))
            {
                return;
            }
        }
        else
        {
            Monitor.Enter(_consoleLock);
        }

        System.Console.CursorLeft = 17;

        var percent = 100 - (_puzzleCount - solved) * 100d / _puzzleCount;

        var line = (int) Math.Floor(percent / 4);

        System.Console.Write($" {new string('\u2588', line)}{new string('-', 25 - line)}   ");

        if (! noSummary)
        {
            System.Console.Write($"{$"{solved:N0}",9}");
        }

        Monitor.Exit(_consoleLock);
    }

    private static void DumpHistory(int[] puzzle, IReadOnlyCollection<Move> solution, HistoryType historyType)
    {
        int yIncrement;

        for (var y = 0; y < 9; y++)
        {
            yIncrement = 0;

            if (y > 2)
            {
                yIncrement++;
            }

            if (y > 5)
            {
                yIncrement++;
            }

            System.Console.CursorTop = y + 2 + yIncrement;

            for (var x = 0; x < 9; x++)
            {
                System.Console.CursorLeft = 30 + x * 2;

                if (x > 2)
                {
                    System.Console.CursorLeft += 2;
                }

                if (x > 5)
                {
                    System.Console.CursorLeft += 2;
                }

                System.Console.Write(puzzle[x + y * 9] == 0 ? "  " : $"{puzzle[x + y * 9]} ");
            }
        }

        var color = System.Console.ForegroundColor;

        var sleep = true;

        var step = 0;

        var previousMove = new Move(-1, -1, -1, MoveType.Guess);

        foreach (var move in solution)
        {
            if (System.Console.KeyAvailable)
            {
                sleep = false;

                System.Console.ReadKey();
            }

            if (historyType == HistoryType.SolutionOnly)
            {
                for (var i = 0; i < 5; i++)
                {
                    System.Console.ForegroundColor = ConsoleColor.Magenta;

                    yIncrement = 0;

                    if (move.Y > 2)
                    {
                        yIncrement++;
                    }

                    if (move.Y > 5)
                    {
                        yIncrement++;
                    }

                    System.Console.CursorTop = move.Y + 2 + yIncrement;

                    System.Console.CursorLeft = 30 + move.X * 2;

                    if (move.X > 2)
                    {
                        System.Console.CursorLeft += 2;
                    }

                    if (move.X > 5)
                    {
                        System.Console.CursorLeft += 2;
                    }

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

                yIncrement = 0;

                if (move.Y > 2)
                {
                    yIncrement++;
                }

                if (move.Y > 5)
                {
                    yIncrement++;
                }

                System.Console.CursorTop = move.Y + 2 + yIncrement;

                System.Console.CursorLeft = 30 + move.X * 2;

                if (move.X > 2)
                {
                    System.Console.CursorLeft += 2;
                }

                if (move.X > 5)
                {
                    System.Console.CursorLeft += 2;
                }

                System.Console.Write(move.Value);
            }
            else
            {
                yIncrement = 0;

                if (move.Y > 2)
                {
                    yIncrement++;
                }

                if (move.Y > 5)
                {
                    yIncrement++;
                }

                System.Console.CursorTop = move.Y + 2 + yIncrement;

                System.Console.CursorLeft = 30 + move.X * 2;

                if (move.X > 2)
                {
                    System.Console.CursorLeft += 2;
                }

                if (move.X > 5)
                {
                    System.Console.CursorLeft += 2;
                }

                System.Console.ForegroundColor = ConsoleColor.Magenta;

                System.Console.Write(move.Type == MoveType.Backtrack ? " " : move.Value.ToString());

                System.Console.ForegroundColor = color;

                if (previousMove.X > -1)
                {
                    yIncrement = 0;

                    if (previousMove.Y > 2)
                    {
                        yIncrement++;
                    }

                    if (previousMove.Y > 5)
                    {
                        yIncrement++;
                    }

                    System.Console.CursorTop = previousMove.Y + 2 + yIncrement;

                    System.Console.CursorLeft = 30 + previousMove.X * 2;

                    if (previousMove.X > 2)
                    {
                        System.Console.CursorLeft += 2;
                    }

                    if (previousMove.X > 5)
                    {
                        System.Console.CursorLeft += 2;
                    }

                    System.Console.Write(previousMove.Type == MoveType.Backtrack ? " " : previousMove.Value.ToString());
                }

                previousMove = move;

                System.Console.CursorTop = 0;

                step++;

                var stepText = $"Step: {step:N0}/{solution.Count:N0}";

                System.Console.CursorLeft = 40 - stepText.Length / 2;

                System.Console.Write(stepText);

                var tried = false;

                for (var i = 1; i < 10; i++)
                {
                    System.Console.CursorLeft = 54;

                    System.Console.CursorTop = i + 2;

                    if (move.Type == MoveType.Backtrack)
                    {
                        System.Console.Write(' ');
                        
                        continue;
                    }

                    if (move.Candidates.Contains(i))
                    {
                        if (move.Value == i)
                        {
                            System.Console.ForegroundColor = ConsoleColor.Magenta;

                            tried = true;
                        }
                        else
                        {
                            if (! tried)
                            {
                                System.Console.ForegroundColor = ConsoleColor.Blue;
                            }
                        }

                        System.Console.Write(i);

                        System.Console.ForegroundColor = color;
                    }
                    else
                    {
                        System.Console.Write(' ');
                    }
                }

                Thread.Sleep(10);
            }
        }

        System.Console.CursorTop = 30;
    }

    private void Dump(int[] left, SudokuResult right, int solved)
    {
        if (solved != _puzzleCount)
        {
            if (! Monitor.TryEnter(_consoleLock))
            {
                return;
            }
        }
        else
        {
            Monitor.Enter(_consoleLock);
        }

        _output.Clear();

        _output.AppendLine(
            " ┌───────┬───────┬───────┐  ┌───────┬───────┬───────┐");

        for (var y = 0; y < 9; y++)
        {
            _output.Append(" │");

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

                if (x is 2 or 5)
                {
                    _output.Append(" │");
                }
            }

            _output.Append(" │  │");

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

                if (x is 2 or 5)
                {
                    _output.Append(" │");
                }
            }

            _output.Append(" │");

            _output.AppendLine();

            if (y is 2 or 5)
            {
                _output.AppendLine(
                    " ├───────┼───────┼───────┤  ├───────┼───────┼───────┤");
            }
        }

        _output.AppendLine(
            " └───────┴───────┴───────┘  └───────┴───────┴───────┘");

        if (solved > 0)
        {
            _output.AppendLine($"\n Solved: {solved:N0}/{_puzzleCount:N0} puzzles ({solved / _stopwatch.Elapsed.TotalSeconds:N0} puzzles/sec).       \n");

            var mean = _elapsed.Total / solved;

            _output.AppendLine(
                $" Timings...\n  Minimum: {_elapsed.Minimum:N0}μs          \n  Mean:    {mean:N0}μs          \n  Maximum: {_elapsed.Maximum:N0}μs (Puzzle #{_maxTimePuzzleNumber:N0})         \n");

            _output.AppendLine(
                $" Combinations...\n  Minimum: {_steps.Minimum:N0}          \n  Mean:    {_steps.Total / (solved + 1):N0}          \n  Maximum: {_steps.Maximum:N0} (Puzzle #{_maxStepsPuzzleNumber:N0}, Clues: {_mostStepsClues})           \n");

            var meanTime = _stopwatch.Elapsed.TotalSeconds / solved;

            var eta = TimeSpan.FromSeconds((_puzzles.Length - solved) * meanTime);

            _output.AppendLine(
                $" Elapsed time: {_stopwatch.Elapsed.Minutes:N0}:{_stopwatch.Elapsed.Seconds:D2}    Estimated remaining: {eta.Hours:N0}:{eta.Minutes:D2}:{eta.Seconds:D2}          \n");

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

        Monitor.Exit(_consoleLock);
    }
}