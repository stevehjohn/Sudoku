using System.Diagnostics;
using Sudoku.Extensions;

namespace Sudoku;

public class Generator
{
    private const int RotationThreshold = 50;

    private readonly List<int>[] _candidates = new List<int>[81];

    private readonly Solver _solver = new(HistoryType.None, SolveMethod.FindUnique);

    private readonly List<int> _filledCells = [];

    private readonly Random _random;

    public Generator()
    {
        _random = new Random();
    }

    public Generator(int seed)
    {
        _random = new Random(seed);
    }

    public int[] Generate(int cluesToLeave = 30, bool useBudget = true)
    {
        var puzzle = new int[81];

        InitialiseCandidates();

        CreateSolvedPuzzle(puzzle);

        var budgetSeconds = 0;

        var budgetMax = 10;

        if (useBudget)
        {
            budgetSeconds = 2;

            budgetMax = cluesToLeave switch
            {
                < 20 => 120,
                < 21 => 20,
                _ => budgetMax
            };
        }

        if (budgetSeconds == 0)
        {
            RemoveCells(puzzle, 81 - cluesToLeave, budgetSeconds);
        }
        else
        {
            var attempts = 0;

            while (! RemoveCells(puzzle, 81 - cluesToLeave, budgetSeconds))
            {
                InitialiseCandidates();

                CreateSolvedPuzzle(puzzle);

                attempts++;

                if (cluesToLeave < 20 && attempts > 10 && budgetSeconds < budgetMax)
                {
                    budgetSeconds++;

                    attempts = 0;
                }
            }
        }

        return puzzle;
    }

    private bool RemoveCells(int[] puzzle, int cellsToRemove, int budgetSeconds)
    {
        CreateFilledCells();

        var stopWatch = Stopwatch.StartNew();

        return RemoveCell(puzzle, cellsToRemove, stopWatch, budgetSeconds * Stopwatch.Frequency);
    }

    private bool RemoveCell(int[] puzzle, int cellsToRemove, Stopwatch stopwatch, long budgetTicks, int start = 0)
    {
        if (cellsToRemove == 0)
        {
            return true;
        }

        if (budgetTicks > 0 && stopwatch.ElapsedTicks > budgetTicks)
        {
            return false;
        }

        if (_filledCells.Count - start < cellsToRemove)
        {
            return false;
        }

        if (cellsToRemove > 1 && start < RotationThreshold)
        {
            for (var i = start; i < RotationThreshold; i += 2)
            {
                var cellIndex1 = _filledCells[i];

                var cellValue1 = puzzle[cellIndex1];

                puzzle[cellIndex1] = 0;

                var cellIndex2 = 0;

                var cellValue2 = 0;

                if (cellIndex1 != 40)
                {
                    cellIndex2 = _filledCells[i + 1];

                    cellValue2 = puzzle[cellIndex2];

                    puzzle[cellIndex2] = 0;
                }

                var result = _solver.Solve(puzzle, true);

                var unique = result.Solved && result.SolutionCount == 1;

                var delta = cellIndex1 != 40 ? 2 : 1;

                if (unique && RemoveCell(puzzle, cellsToRemove - delta, stopwatch, budgetTicks, i + delta))
                {
                    return true;
                }

                puzzle[cellIndex1] = cellValue1;

                if (cellIndex1 != 40)
                {
                    puzzle[cellIndex2] = cellValue2;
                }
            }
        }

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
        }

        return false;
    }

    private void CreateFilledCells()
    {
        _filledCells.Clear();

        var filledCells = new List<int>();

        for (var i = 0; i < 81; i++)
        {
            filledCells.Add(i);
        }

        var count = filledCells.Count;

        for (var left = 0; left < count - 1; left++)
        {
            var right = left + _random.Next(count - left);

            (filledCells[left], filledCells[right]) = (filledCells[right], filledCells[left]);
        }

        count = 0;
        
        while (filledCells.Count > 0)
        {
            var cell = filledCells[count];
            
            _filledCells.Add(cell);
            
            filledCells.RemoveAt(0);

            if (count < RotationThreshold && cell != 40)
            {
                cell = filledCells[count];
            
                _filledCells.Add(cell);
            
                filledCells.RemoveAt(0);
                
                count++;
            }

            count++;
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