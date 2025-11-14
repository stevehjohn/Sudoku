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

    private readonly int[] _failed = new int[81];

    private readonly int[] _failedDepth = new int[81];

    private readonly int[] _originalPuzzle = new int[81];

    private readonly int[] _digitCounts = new int[10];

    private int _failedStamp;

    public Generator()
    {
        _random = new Random();
    }

    public Generator(int seed)
    {
        _random = new Random(seed);
    }

    public (bool Succeeded, int[] Puzzle) Generate(int cluesToLeave)
    {
        return Generate(cluesToLeave, CancellationToken.None);
    }

    public (bool Succeeded, int[] Puzzle) Generate(int cluesToLeave, CancellationToken cancellationToken)
    {
        var puzzle = CreateSolvedPuzzle();

        return Generate(puzzle, cluesToLeave, cancellationToken);
    }

    public (bool Succeeded, int[] Puzzle) Generate(int[] solvedPuzzle, int cluesToLeave, CancellationToken cancellationToken)
    {
        var puzzle = new int[81];

        Array.Copy(solvedPuzzle, puzzle, 81);

        Array.Copy(solvedPuzzle, _originalPuzzle, 81);

        var puzzleUseCount = 0;

        while (! cancellationToken.IsCancellationRequested)
        {
            InitialiseDigitCounts();
            
            if (RemoveCells(puzzle, 81 - cluesToLeave, 0, cancellationToken) == RemoveResult.Success)
            {
                return (true, puzzle);
            }

            puzzleUseCount++;

            if (cluesToLeave < 22 && puzzleUseCount > 100)
            {
                puzzle = CreateSolvedPuzzle();

                Array.Copy(puzzle, _originalPuzzle, 81);

                puzzleUseCount = 0;
            }
            else
            {
                Array.Copy(_originalPuzzle, puzzle, 81);
            }
        }

        return (false, puzzle);
    }

    private void InitialiseDigitCounts()
    {
        for (var i = 1; i < 10; i++)
        {
            _digitCounts[i] = 9;
        }
    }

    public int[] CreateSolvedPuzzle()
    {
        var puzzle = new int[81];

        var solved = false;

        while (! solved)
        {
            solved = CreateSolvedPuzzle(puzzle, CancellationToken.None);
        }

        return puzzle;
    }

    public bool CreateSolvedPuzzle(Span<int> puzzle, CancellationToken cancellationToken)
    {
        for (var i = 0; i < 81; i++)
        {
            puzzle[i] = 0;
        }

        InitialiseCandidates();

        return CreateSolvedPuzzle(puzzle, 0, cancellationToken);
    }

    private void InitialiseCandidates()
    {
        for (var i = 0; i < 81; i++)
        {
            _candidates[i] = [1, 2, 3, 4, 5, 6, 7, 8, 9];

            _candidateCounts[i] = 9;
        }
    }

    private RemoveResult RemoveCells(int[] puzzle, int cellsToRemove, int budgetSeconds, CancellationToken cancellationToken)
    {
        CreateAndShuffleFilledCells();

        var stopWatch = Stopwatch.StartNew();

        _failedStamp++;

        return RemoveCell(puzzle, cellsToRemove, stopWatch, budgetSeconds * Stopwatch.Frequency, 0, cancellationToken);
    }

    private RemoveResult RemoveCell(int[] puzzle, int cellsToRemove, Stopwatch stopwatch, long budgetTicks, int depth, CancellationToken cancellationToken)
    {
        if (cellsToRemove == 0)
        {
            return RemoveResult.Success;
        }

        if (budgetTicks > 0 && stopwatch.ElapsedTicks > budgetTicks)
        {
            return RemoveResult.BudgetExceeded;
        }

        if (_filledCells.Count - depth < cellsToRemove)
        {
            return RemoveResult.Failure;
        }

        var filledCount = _filledCells.Count;

        for (var i = depth; i < filledCount; i++)
        {
            if (cancellationToken.IsCancellationRequested)
            {
                return RemoveResult.Cancelled;
            }

            var cellIndex = _filledCells[i];

            if (_failed[cellIndex] == _failedStamp && _failedDepth[cellIndex] >= depth)
            {
                continue;
            }

            var cellValue = puzzle[cellIndex];

            if (_digitCounts[cellValue] == 1 && cellsToRemove > 7)
            {
                continue;
            }

            puzzle[cellIndex] = 0;

            _digitCounts[cellValue]--;

            if (cancellationToken.IsCancellationRequested)
            {
                return RemoveResult.Cancelled;
            }

            var unique = _solver.HasUniqueSolution(puzzle, _originalPuzzle);

            if (unique)
            {
                var result = RemoveCell(puzzle, cellsToRemove - 1, stopwatch, budgetTicks, i + 1, cancellationToken);

                if (result != RemoveResult.Failure)
                {
                    return result;
                }
            }

            puzzle[cellIndex] = cellValue;

            _digitCounts[cellValue]++;

            _failed[cellIndex] = _failedStamp;

            _failedDepth[cellIndex] = depth;
        }

        return RemoveResult.Failure;
    }

    private void CreateAndShuffleFilledCells()
    {
        _filledCells.Clear();

        for (var i = 0; i < 40; i++)
        {
            _filledCells.Add(i);
        }

        for (var left = 0; left < 39; left++)
        {
            var right = left + _random.Next(40 - left);

            (_filledCells[left], _filledCells[right]) = (_filledCells[right], _filledCells[left]);
        }

        for (var i = 39; i >= 0; i--)
        {
            _filledCells.Insert(i + 1, 80 - _filledCells[i]);
        }

        var centre = _random.Next(39) * 2;

        _filledCells.Insert(centre, 40);
    }

    private bool CreateSolvedPuzzle(Span<int> puzzle, int cell, CancellationToken cancellationToken)
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

            if (puzzle.IsValidSudoku(cell))
            {
                return cell == 80 || CreateSolvedPuzzle(puzzle, cell + 1, cancellationToken);
            }
        }

        puzzle[cell] = 0;

        for (var i = 0; i < 9; i++)
        {
            _candidates[cell][i] = i + 1;
        }

        _candidateCounts[cell] = 9;

        return CreateSolvedPuzzle(puzzle, cell - 1, cancellationToken);
    }
}