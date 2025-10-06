using System.Diagnostics;
using Sudoku.Extensions;

namespace Sudoku;

public class Generator
{
    private readonly List<int>[] _candidates = new List<int>[81];

    private readonly Solver _solver = new(HistoryType.None, SolveMethod.FindUnique);

    private readonly List<int> _filledCells = [];

    private readonly Random _random;
    
    public Action<int> AttemptHook { get; set; }

    public Generator()
    {
        _random = new Random();
    }

    public Generator(int seed)
    {
        _random = new Random(seed);
    }
    
    public (bool Succeeded, int[] Puzzle) Generate(int cluesToLeave = 30, CancellationToken? cancellationToken = null, bool useBudget = true)
    {
        var puzzle = new int[81];
        
        InitialiseCandidates();

        CreateSolvedPuzzle(puzzle);

        var budgetSeconds = 0;

        var budgetMax = 3;
        
        if (useBudget)
        {
            budgetSeconds = 2;

            budgetMax = cluesToLeave switch
            {
                < 20 => 30,
                < 21 => 20,
                22 => 10,
                23 => 8,
                _ => budgetMax
            };
        }

        var succeeded = true;
        
        if (budgetSeconds == 0)
        {
            RemoveCells(puzzle, 81 - cluesToLeave, budgetSeconds, cancellationToken);
        }
        else
        {
            var attempts = 1;
            
            while (! RemoveCells(puzzle, 81 - cluesToLeave, budgetSeconds, cancellationToken))
            {
                if (cancellationToken is { IsCancellationRequested: true })
                {
                    succeeded = false;
                    
                    break;
                }

                attempts++;

                AttemptHook?.Invoke(attempts);

                if (cluesToLeave < 20 && attempts % 10 == 0 && budgetSeconds < budgetMax)
                {
                    budgetSeconds++;
                }
            }
        }

        return (succeeded, puzzle);
    }

    private bool RemoveCells(int[] puzzle, int cellsToRemove, int budgetSeconds, CancellationToken? cancellationToken)
    {
        _filledCells.Clear();
        
        for (var i = 0; i < 81; i++)
        {
            _filledCells.Add(i);
        }

        ShuffleFilledCells();

        var stopWatch = Stopwatch.StartNew();
        
        return RemoveCell(puzzle, cellsToRemove, stopWatch, budgetSeconds * Stopwatch.Frequency, 0, cancellationToken);
    }

    private bool RemoveCell(int[] puzzle, int cellsToRemove, Stopwatch stopwatch, long budgetTicks, int start, CancellationToken? cancellationToken = null)
    {
        if (cellsToRemove == 0)
        {
            return true;
        }

        if ((budgetTicks > 0 && stopwatch.ElapsedTicks > budgetTicks) || cancellationToken is { IsCancellationRequested: true })
        {
            return false;
        }

        if (_filledCells.Count - start < cellsToRemove)
        {
            return false;
        }

        var backtracks = 0;

        for (var i = start; i < _filledCells.Count; i++)
        {
            var cellIndex = _filledCells[i];

            var cellValue = puzzle[cellIndex];

            puzzle[cellIndex] = 0;

            var result = _solver.Solve(puzzle, true);

            var unique = result.Solved && result.SolutionCount == 1;

            if (unique && RemoveCell(puzzle, cellsToRemove - 1, stopwatch, budgetTicks, i + 1))
            {
                return true;
            }

            puzzle[cellIndex] = cellValue;

            backtracks++;

            if (backtracks > 3 || cancellationToken is { IsCancellationRequested: true })
            {
                return false;
            }
        }

        return false;
    }

    private void ShuffleFilledCells()
    {
        var count = _filledCells.Count;
        
        for (var left = 0; left < count - 1; left++)
        {
            var right = left + _random.Next(count - left);
            
            (_filledCells[left], _filledCells[right]) = (_filledCells[right], _filledCells[left]);
        }
    }

    private bool CreateSolvedPuzzle(Span<int> puzzle, int cell = 0)
    {
        while (_candidates[cell].Count > 0)
        {
            var candidateIndex = _random.Next(_candidates[cell].Count);

            var candidate = _candidates[cell][candidateIndex];

            _candidates[cell].RemoveAt(candidateIndex);

            puzzle[cell] = candidate;

            if (puzzle.IsValidSudoku())
            {
                return cell == 80 || CreateSolvedPuzzle(puzzle, cell + 1);
            }
        }
        
        puzzle[cell] = 0;

        _candidates[cell] = [1, 2, 3, 4, 5, 6, 7, 8, 9];

        return CreateSolvedPuzzle(puzzle, cell - 1);
    }

    private void InitialiseCandidates()
    {
        for (var i = 0; i < 81; i++)
        {
            _candidates[i] = [1, 2, 3, 4, 5, 6, 7, 8, 9];
        }
    }
}