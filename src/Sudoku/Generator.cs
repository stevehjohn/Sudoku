using Sudoku.Extensions;

namespace Sudoku;

public class Generator
{
    private readonly List<int>[] _candidates = new List<int>[81];

    private readonly Random _random = Random.Shared;

    private readonly Solver _solver = new(HistoryType.None, SolveMethod.FindUnique);

    private readonly List<int> _filledCells = [];
    
    public int[] Generate(int cluesToLeave = 30)
    {
        var puzzle = new int[81];
        
        InitialiseCandidates();

        CreateSolvedPuzzle(puzzle);

        RemoveCells(puzzle, 81 - cluesToLeave);
        
        return puzzle;
    }

    private void RemoveCells(int[] puzzle, int cellsToRemove)
    {
        _filledCells.Clear();
        
        for (var i = 0; i < 81; i++)
        {
            _filledCells.Add(i);
        }

        ShuffleFilledCells();
        
        RemoveCell(puzzle, cellsToRemove);
    }

    private bool RemoveCell(int[] puzzle, int cellsToRemove, int start = 0)
    {
        if (cellsToRemove == 0)
        {
            return true;
        }

        for (var i = start; i < _filledCells.Count; i++)
        {
            (_filledCells[i], _filledCells[start]) = (_filledCells[start], _filledCells[i]);
            
            var cellIndex = _filledCells[start];

            var cellValue = puzzle[cellIndex];

            puzzle[cellIndex] = 0;

            var result = _solver.Solve(puzzle);

            var unique = result.Solved && result.SolutionCount == 1;

            if (unique && RemoveCell(puzzle, cellsToRemove - 1, start + 1))
            {
                return true;
            }

            puzzle[cellIndex] = cellValue;
         
            (_filledCells[i], _filledCells[start]) = (_filledCells[start], _filledCells[i]);
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