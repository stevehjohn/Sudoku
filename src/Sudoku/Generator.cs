using System.Diagnostics;
using Sudoku.Extensions;

namespace Sudoku;

public class Generator
{
    private readonly List<int>[] _candidates = new List<int>[81];

    private readonly Random _random = Random.Shared;

    private readonly Solver _solver = new(HistoryType.None, SolveMethod.FindUnique);

    private readonly List<int> _filledCells = [];
    
    public int[] Generate(int cluesToLeave = 30, bool useBudget = true)
    {
        var puzzle = new int[81];
        
        InitialiseCandidates();

        CreateSolvedPuzzle(puzzle);

        var budgetSeconds = 0;
        
        if (useBudget)
        {
            budgetSeconds = 2;
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

                if (attempts > 10 && budgetSeconds < 10)
                {
                    budgetSeconds++;
                }
            }
        }

        return puzzle;
    }

    private bool RemoveCells(int[] puzzle, int cellsToRemove, int budgetSeconds)
    {
        _filledCells.Clear();
        
        for (var i = 0; i < 81; i++)
        {
            _filledCells.Add(i);
        }

        ShuffleFilledCells();

        var stopWatch = Stopwatch.StartNew();
        
        return RemoveCell(puzzle, cellsToRemove, stopWatch, budgetSeconds);
    }

    private bool RemoveCell(int[] puzzle, int cellsToRemove, Stopwatch stopwatch, int budgetSeconds, int start = 0)
    {
        if (cellsToRemove == 0)
        {
            return true;
        }

        if (budgetSeconds > 0 && stopwatch.Elapsed.TotalSeconds > budgetSeconds)
        {
            return false;
        }

        if (_filledCells.Count - start < cellsToRemove)
        {
            return false;
        }

        for (var i = start; i < _filledCells.Count; i++)
        {
            var cellIndex = _filledCells[i];

            var cellValue = puzzle[cellIndex];

            puzzle[cellIndex] = 0;

            var result = _solver.Solve(puzzle, true);

            var unique = result.Solved && result.SolutionCount == 1;

            if (unique && RemoveCell(puzzle, cellsToRemove - 1, stopwatch, budgetSeconds, i + 1))
            {
                return true;
            }

            puzzle[cellIndex] = cellValue;
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