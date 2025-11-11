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

    private readonly int[] _originalPuzzle = new int[81];

    private (Candidates Row, Candidates Column, Candidates Box) _unitCandidates = (new Candidates(), new Candidates(), new Candidates());

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

    public (bool Succeeded, int[] Puzzle) Generate(int[] solvedPuzzle, int cluesToLeave, CancellationToken cancellationToken, bool useBudget = true)
    {
        var puzzle = new int[81];

        Array.Copy(solvedPuzzle, puzzle, 81);

        Array.Copy(solvedPuzzle, _originalPuzzle, 81);

        _unitCandidates.Row.Clear();

        _unitCandidates.Column.Clear();

        _unitCandidates.Box.Clear();

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
                if (RemoveCells(puzzle, 81 - cluesToLeave, 0, cancellationToken))
                {
                    return (true, puzzle);
                }

                Array.Copy(_originalPuzzle, puzzle, 81);
            }
        }
        else
        {
            var attempts = 1;

            while (! RemoveCells(puzzle, 81 - cluesToLeave, budgetSeconds, cancellationToken))
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

    private bool RemoveCells(int[] puzzle, int cellsToRemove, int budgetSeconds, CancellationToken cancellationToken)
    {
        CreateAndShuffleFilledCells();

        var stopWatch = Stopwatch.StartNew();

        _failedStamp++;

        return RemoveCell(puzzle, cellsToRemove, stopWatch, budgetSeconds * Stopwatch.Frequency, 0, cancellationToken);
    }

    private bool RemoveCell(int[] puzzle, int cellsToRemove, Stopwatch stopwatch, long budgetTicks, int start, CancellationToken cancellationToken)
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

        var filledCount = _filledCells.Count;

        for (var i = start; i < filledCount; i++)
        {
            if (cancellationToken.IsCancellationRequested)
            {
                return false;
            }

            var cellIndex = _filledCells[i];

            if (_failed[cellIndex] == _failedStamp)
            {
                continue;
            }

            if ((puzzle[80 - cellIndex] == 0 || _failed[80 - cellIndex] == _failedStamp) && i < 80)
            {
                (_filledCells[i], _filledCells[filledCount - 1]) = (_filledCells[filledCount - 1], _filledCells[i]);

                cellIndex = _filledCells[i];

                if (_failed[cellIndex] == _failedStamp)
                {
                    continue;
                }
            }

            var cellValue = puzzle[cellIndex];

            var row = UnitTables.CellRow(cellIndex);

            var column = UnitTables.CellColumn(cellIndex);

            var box = UnitTables.CellBox(cellIndex);

            var bit = 1 << (cellValue - 1);
            
            if ((_unitCandidates.Row[row] | bit) == 0x1FF ||
                (_unitCandidates.Column[column] | bit) == 0x1FF ||
                (_unitCandidates.Box[box] | bit) == 0x1FF)
            {
                continue;
            }

            puzzle[cellIndex] = 0;

            _unitCandidates.Row.Add(row, cellValue);

            _unitCandidates.Column.Add(column, cellValue);

            _unitCandidates.Box.Add(box, cellValue);

            if (cancellationToken.IsCancellationRequested)
            {
                return false;
            }

            var unique = i < 9 || _solver.HasUniqueSolution(puzzle, _originalPuzzle);

            if (unique && RemoveCell(puzzle, cellsToRemove - 1, stopwatch, budgetTicks, i + 1, cancellationToken))
            {
                return true;
            }

            puzzle[cellIndex] = cellValue;

            _unitCandidates.Row.Remove(row, cellValue);

            _unitCandidates.Column.Remove(column, cellValue);

            _unitCandidates.Box.Remove(box, cellValue);

            _failed[cellIndex] = _failedStamp;
        }

        return false;
    }
    
    private void CreateAndShuffleFilledCells()
    {
        _filledCells.Clear();

        for (var i = 0; i < 81; i++)
        {
            _filledCells.Add(i);
        }

        for (var left = 0; left < 80; left++)
        {
            var right = left + _random.Next(81 - left);

            (_filledCells[left], _filledCells[right]) = (_filledCells[right], _filledCells[left]);
        }
    }
}