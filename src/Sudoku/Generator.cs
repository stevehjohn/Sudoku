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

    private int _failedStamp;

    private readonly int[] _originalPuzzle = new int[81];

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
        
        Array.Copy(puzzle, _originalPuzzle, 81);

        var budgetSeconds = 0;

        var budgetMin = 3;

        if (useBudget && ! Debugger.IsAttached)
        {
            budgetSeconds = 2;

            budgetMin = cluesToLeave switch
            {
                < 19 => 60,
                < 20 => 30,
                < 21 => 25,
                21 => 20,
                22 => 10,
                23 => 8,
                _ => budgetMin
            };
        }

        var succeeded = true;

        if (budgetSeconds == 0)
        {
            while (! cancellationToken.IsCancellationRequested)
            {
                if (RemoveCells(puzzle, cluesToLeave, 81 - cluesToLeave, 0, cancellationToken))
                {
                    return (true, puzzle);
                }
                
                Array.Copy(_originalPuzzle, puzzle, 81);
            }
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

                if (cluesToLeave < 20 && attempts % 10 == 0 && budgetSeconds < budgetMin)
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

        _failedStamp++;

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

            if (_failed[cellIndex] == _failedStamp)
            {
                continue;
            }

            var cellValue = puzzle[cellIndex];

            var row = UnitTables.CellRow(cellIndex);

            var column = UnitTables.CellColumn(cellIndex);

            var box = UnitTables.CellBox(cellIndex);

            if (WouldEmptyUnit(puzzle, UnitTables.RowCells(row), cellIndex)
                || WouldEmptyUnit(puzzle, UnitTables.ColumnCells(column), cellIndex)
                || WouldEmptyUnit(puzzle, UnitTables.BoxCells(box), cellIndex))
            {
                continue;
            }

            puzzle[cellIndex] = 0;

            if (cancellationToken.IsCancellationRequested)
            {
                return false;
            }

            var unique = _solver.HasUniqueSolution(puzzle, _originalPuzzle);

            if (unique && RemoveCell(puzzle, targetClues, cellsToRemove - 1, stopwatch, budgetTicks, i + 1, cancellationToken))
            {
                return true;
            }

            puzzle[cellIndex] = cellValue;

            _failed[cellIndex] = _failedStamp;

            backtracks++;

            if ((targetClues < 24 && backtracks > 5) || cancellationToken.IsCancellationRequested)
            {
                return false;
            }
        }

        return false;
    }

    private static bool WouldEmptyUnit(int[] puzzle, ReadOnlySpan<byte> unit, int removedCell)
    {
        for (var i = 0; i < 9; i++)
        {
            var cell = unit[i];

            if (cell != removedCell && puzzle[cell] > 0)
            {
                return false;
            }
        }

        return true;
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

            if (puzzle.IsValidSudoku(cell))
            {
                return cell == 80 || CreateSolvedPuzzle(puzzle, cancellationToken, cell + 1);
            }
        }

        puzzle[cell] = 0;

        for (var i = 0; i < 9; i++)
        {
            _candidates[cell][i] = i + 1;
        }

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