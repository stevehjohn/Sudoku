using System.Diagnostics;
using Sudoku.Extensions;

namespace Sudoku;

public class Generator
{
    private readonly int[][] _candidates = new int[81][];

    private readonly int[] _candidateCounts = new int[81];
    
    private readonly Solver _solver = new();

    private readonly List<int> _filledCells = [];

    private readonly Random _random;
    
    private readonly bool[] _failed = new bool[81];

    public Action<int> AttemptHook { get; set; }

    public Generator()
    {
        _random = new Random();
    }

    public Generator(int seed)
    {
        _random = new Random(seed);
    }
    
    public (bool Succeeded, int[] Puzzle) Generate(int cluesToLeave, CancellationToken cancellationToken, bool useBudget = true)
    {
        var puzzle = new int[81];
        
        InitialiseCandidates();

        if (! CreateSolvedPuzzle(puzzle, cancellationToken))
        {
            return (false, puzzle);
        }

        var budgetSeconds = 0;

        var budgetMax = 3;
        
        if (useBudget && ! Debugger.IsAttached)
        {
            budgetSeconds = 2;

            budgetMax = cluesToLeave switch
            {
                < 19 => 60,
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
            RemoveCells(puzzle, cluesToLeave, 81 - cluesToLeave, 0, cancellationToken);
        }
        else
        {
            var attempts = 1;
            
            while (! RemoveCells(puzzle, cluesToLeave, 81 - cluesToLeave, budgetSeconds, cancellationToken))
            {
                if (cancellationToken.IsCancellationRequested)
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
    
    private bool RemoveCells(int[] puzzle, int targetClues, int cellsToRemove, int budgetSeconds, CancellationToken cancellationToken)
    {
        _filledCells.Clear();
        
        for (var i = 0; i < 81; i++)
        {
            _filledCells.Add(i);
        }

        ShuffleFilledCells();

        var stopWatch = Stopwatch.StartNew();
        
        Array.Fill(_failed, false);
        
        return RemoveCell(puzzle, targetClues, cellsToRemove, stopWatch, budgetSeconds * Stopwatch.Frequency, 0, cancellationToken);
    }

    private bool RemoveCell(int[] puzzle, int targetClues, int cellsToRemove, Stopwatch stopwatch, long budgetTicks, int start, CancellationToken cancellationToken)
    {
        if (cellsToRemove == 0)
        {
            return true;
        }

        if ((budgetTicks > 0 && stopwatch.ElapsedTicks > budgetTicks) || cancellationToken.IsCancellationRequested)
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

            if (_failed[cellIndex])
            {
                continue;
            }

            var cellValue = puzzle[cellIndex];

            var row = cellIndex / 9;

            if (CountValueInUnit(puzzle, UnitTables.Row(row), cellValue) == 9)
            {
                continue;
            }

            var column = cellIndex % 9;

            if (CountValueInUnit(puzzle, UnitTables.Column(column), cellValue) == 9)
            {
                continue;
            }

            var box = row / 3 * 3 + column / 3;

            if (CountValueInUnit(puzzle, UnitTables.Box(box), cellValue) == 9)
            {
                continue;
            }

            puzzle[cellIndex] = 0;

            if (cancellationToken.IsCancellationRequested)
            {
                return false;
            }
            
            var result = _solver.Solve(puzzle, true);

            var unique = result.Solved && result.SolutionCount == 1;

            if (unique && RemoveCell(puzzle, targetClues, cellsToRemove - 1, stopwatch, budgetTicks, i + 1, cancellationToken))
            {
                return true;
            }

            puzzle[cellIndex] = cellValue;

            _failed[cellIndex] = true;

            backtracks++;

            if ((targetClues < 24 && backtracks > 3) || cancellationToken.IsCancellationRequested)
            {
                return false;
            }
        }

        return false;
    }
    
    private int CountValueInUnit(int[] puzzle, ReadOnlySpan<int> unit, int value)
    {
        var count = 0;
    
        for (var i = 0; i < 9; i++)
        {
            if (puzzle[unit[i]] == value)
            {
                count++;
            }
        }

        return count;
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

    private bool CreateSolvedPuzzle(Span<int> puzzle, CancellationToken cancellationToken, int cell = 0)
    {
        if (cancellationToken.IsCancellationRequested)
        {
            return false;
        }

        while (_candidateCounts[cell] > 0)
        {
            var candidateIndex = _random.Next(_candidateCounts[cell]);

            var candidate = _candidates[cell][candidateIndex];

            _candidates[cell][candidateIndex] = _candidates[cell][_candidateCounts[cell] - 1];
            
            _candidateCounts[cell]--;
            
            puzzle[cell] = candidate;

            if (puzzle.IsValidSudoku())
            {
                return cell == 80 || CreateSolvedPuzzle(puzzle, cancellationToken, cell + 1);
            }
        }
        
        puzzle[cell] = 0;

        _candidates[cell] = [1, 2, 3, 4, 5, 6, 7, 8, 9];

        _candidateCounts[cell] = 9;

        return CreateSolvedPuzzle(puzzle, cancellationToken, cell - 1);
    }

    private void InitialiseCandidates()
    {
        for (var i = 0; i < 81; i++)
        {
            _candidates[i] = [1, 2, 3, 4, 5, 6, 7, 8, 9];

            _candidateCounts[i] = 9;
        }
    }
}