namespace Sudoku;

public class SudokuResult
{
    private readonly int[] _solution;

    private readonly List<int>[] _initialCandidates;

    public bool Solved { get; }

    public int Steps { get; }

    public double ElapsedMicroseconds { get; }

    public IReadOnlyList<Move> History { get; }

    public string Message { get; }

    public int this[int index] => _solution[index];

    public SudokuResult(int[] solution, bool solved, int steps, double elapsedMilliseconds, List<Move> history, List<int>[] initialCandidates, string message)
    {
        _solution = solution;

        Solved = solved;

        Steps = steps;

        ElapsedMicroseconds = elapsedMilliseconds;

        History = history;

        _initialCandidates = initialCandidates;

        Message = message;
    }

    public IReadOnlyList<int> GetInitialCandidates(int index)
    {
        return _initialCandidates[index];
    }

    public void LogToConsole()
    {
        var backtrackCount = 0;

        foreach (var move in History)
        {
            if (move.Type == MoveType.Backtrack)
            {
                backtrackCount++;

                continue;
            }

            if (backtrackCount > 0)
            {
                Console.WriteLine($" - Backtracking {backtrackCount} guess{(backtrackCount > 1 ? "es" : string.Empty)}");

                backtrackCount = 0;
            }

            switch (move.Type)
            {
                case MoveType.NakedSingle:
                    Console.WriteLine($" - Last possible number {move.Value} at ({move.X}, {move.Y})");
                    break;

                case MoveType.HiddenSingle:
                    Console.WriteLine($" - Hidden single {move.Value} at ({move.X}, {move.Y})");
                    break;
                
                case MoveType.NoCandidates:
                    Console.WriteLine($" - No candidates at ({move.X}, {move.Y})");
                    break;

                default:
                    Console.WriteLine($" - Guess of {move.Value} at ({move.X}, {move.Y})");
                    break;
            }
        }
    }

    public void DumpToConsole(int left = -1, int top = -1)
    {
        SetPosition(left, top, 0);

        Console.WriteLine(
            "\u250c\u2500\u2500\u2500\u2500\u2500\u2500\u2500\u252c\u2500\u2500\u2500\u2500\u2500\u2500\u2500\u252c\u2500\u2500\u2500\u2500\u2500\u2500\u2500\u2510");

        var line = 1;

        for (var y = 0; y < 9; y++)
        {
            SetPosition(left, top, line++);

            Console.Write("\u2502");

            for (var x = 0; x < 9; x++)
            {
                if (_solution[x + y * 9] == 0)
                {
                    Console.Write("  ");
                }
                else
                {
                    Console.Write($" {_solution[x + y * 9]}");
                }

                if (x is 2 or 5)
                {
                    Console.Write(" \u2502");
                }
            }

            Console.WriteLine(" \u2502");

            if (y is 2 or 5)
            {
                SetPosition(left, top, line++);

                Console.WriteLine(
                    "\u251c\u2500\u2500\u2500\u2500\u2500\u2500\u2500\u253c\u2500\u2500\u2500\u2500\u2500\u2500\u2500\u253c\u2500\u2500\u2500\u2500\u2500\u2500\u2500\u2524");
            }
        }

        SetPosition(left, top, line);

        Console.WriteLine(
            "\u2514\u2500\u2500\u2500\u2500\u2500\u2500\u2500\u2534\u2500\u2500\u2500\u2500\u2500\u2500\u2500\u2534\u2500\u2500\u2500\u2500\u2500\u2500\u2500\u2518");
    }

    private static void SetPosition(int left, int top, int y)
    {
        if (top > -1)
        {
            Console.CursorTop = top + y;
        }

        if (left > -1)
        {
            Console.CursorLeft = left;
        }
    }
}