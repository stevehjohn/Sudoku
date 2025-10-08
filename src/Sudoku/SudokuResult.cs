using Sudoku.Extensions;

namespace Sudoku;

public class SudokuResult
{
    private readonly int[] _solution;

    private readonly List<int>[] _initialCandidates;

    public bool Solved { get; }

    public int Steps { get; }

    public double ElapsedMicroseconds { get; }

    public IReadOnlyList<Move> History { get; }
    
    public int SolutionCount { get; }

    public string Message { get; }

    public int this[int index] => _solution[index];

    public SudokuResult(int[] solution, bool solved, int steps, double elapsedMilliseconds, List<Move> history, List<int>[] initialCandidates, int solutionCount, string message)
    {
        _solution = solution;

        Solved = solved;

        Steps = steps;

        ElapsedMicroseconds = elapsedMilliseconds;

        History = history;

        _initialCandidates = initialCandidates;

        SolutionCount = solutionCount;
        
        Message = message;
    }

    public IReadOnlyList<int> GetInitialCandidates(int index)
    {
        return _initialCandidates[index];
    }

    public void LogToConsole()
    {
        var backtrackCount = 0;

        if (_initialCandidates != null)
        {
            var sum = 0;

            for (var i = 0; i < 81; i++)
            {
                sum += GetInitialCandidates(i)?.Sum() ?? 0;
            }

            Console.WriteLine($" - Candidate count: {sum}");
        }

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

                case MoveType.NakedPairRow:
                case MoveType.NakedPairColumn:
                case MoveType.NakedPairBox:
                    Console.WriteLine($" - Naked pair ({string.Join(", ", ((move.Value & 0x7FFF0000) >> 16).BitsToCandidates())}), left other candidates ({string.Join(", ", (move.Value & 0xFFFF).BitsToCandidates())}) at ({move.X}, {move.Y})");
                    break;
                
                case MoveType.None:
                case MoveType.Guess:
                case MoveType.Backtrack:
                default:
                    Console.WriteLine($" - Guess of {move.Value} at ({move.X}, {move.Y}). Candidates: {string.Join(", ", move.Candidates)}");
                    break;
            }
        }
    }

    public void DumpToConsole(int left = -1, int top = -1)
    {
        _solution.DumpToConsole(left, top);
    }
}