using System;
using System.Diagnostics;
using Sudoku.Extensions;

namespace Sudoku;

public class Generator
{
    private readonly List<int>[] _candidates = new List<int>[81];

    private readonly Solver _solver = new(HistoryType.None, SolveMethod.FindUnique);

    private readonly List<int> _filledCells = [];

    private readonly Random _random;

    private readonly List<int>[] _unavoidableSetLookup = new List<int>[81];

    private List<int[]> _unavoidableSets = [];

    private int[] _unavoidableSetCounts = Array.Empty<int>();

    public Generator()
    {
        _random = new Random();

        InitialiseLookup();
    }

    public Generator(int seed)
    {
        _random = new Random(seed);

        InitialiseLookup();
    }

    public int[] Generate(int cluesToLeave = 30, bool useBudget = true)
    {
        var puzzle = new int[81];

        InitialiseCandidates();

        CreateSolvedPuzzle(puzzle);

        InitialiseUnavoidableSets(puzzle);

        var budgetSeconds = 0;

        var budgetMax = 3;
        
        if (useBudget)
        {
            budgetSeconds = 2;

            budgetMax = cluesToLeave switch
            {
                < 20 => 60,
                < 21 => 20,
                22 => 8,
                21 => 8,
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

                InitialiseUnavoidableSets(puzzle);

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

    private void InitialiseLookup()
    {
        for (var i = 0; i < 81; i++)
        {
            _unavoidableSetLookup[i] = [];
        }
    }

    private void InitialiseUnavoidableSets(ReadOnlySpan<int> puzzle)
    {
        _unavoidableSets = UnavoidableSetFinder.Find(puzzle);

        _unavoidableSetCounts = new int[_unavoidableSets.Count];

        for (var i = 0; i < 81; i++)
        {
            _unavoidableSetLookup[i].Clear();
        }

        for (var i = 0; i < _unavoidableSets.Count; i++)
        {
            var unavoidableSet = _unavoidableSets[i];

            _unavoidableSetCounts[i] = unavoidableSet.Length;

            foreach (var cell in unavoidableSet)
            {
                _unavoidableSetLookup[cell].Add(i);
            }
        }
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

        for (var i = start; i < _filledCells.Count; i++)
        {
            var cellIndex = _filledCells[i];

            var cellValue = puzzle[cellIndex];

            if (! TryRemoveCell(cellIndex))
            {
                continue;
            }

            puzzle[cellIndex] = 0;

            var result = _solver.Solve(puzzle, true);

            var unique = result.Solved && result.SolutionCount == 1;

            if (unique && RemoveCell(puzzle, cellsToRemove - 1, stopwatch, budgetTicks, i + 1))
            {
                return true;
            }

            puzzle[cellIndex] = cellValue;

            RestoreCell(cellIndex);
        }

        return false;
    }

    private bool TryRemoveCell(int cellIndex)
    {
        if (_unavoidableSetCounts.Length == 0)
        {
            return true;
        }

        var unavoidableSets = _unavoidableSetLookup[cellIndex];

        foreach (var unavoidableSetIndex in unavoidableSets)
        {
            if (_unavoidableSetCounts[unavoidableSetIndex] <= 1)
            {
                return false;
            }
        }

        foreach (var unavoidableSetIndex in unavoidableSets)
        {
            _unavoidableSetCounts[unavoidableSetIndex]--;
        }

        return true;
    }

    private void RestoreCell(int cellIndex)
    {
        if (_unavoidableSetCounts.Length == 0)
        {
            return;
        }

        foreach (var unavoidableSetIndex in _unavoidableSetLookup[cellIndex])
        {
            _unavoidableSetCounts[unavoidableSetIndex]++;
        }
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