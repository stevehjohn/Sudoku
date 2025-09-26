using Sudoku.Extensions;

namespace Sudoku;

public class Generator
{
    private readonly List<int>[] _candidates = new List<int>[81];

    private readonly Random _rng = Random.Shared;

    private readonly Solver _solver = new(HistoryType.None, SolveMethod.CountAll);
    
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
        var filledCells = new List<int>();

        for (var i = 0; i < 81; i++)
        {
            filledCells.Add(i);
        }

        for (var i = 0; i < cellsToRemove; i++)
        {
            var cellIndex = filledCells[_rng.Next(filledCells.Count)];

            filledCells.Remove(cellIndex);
            
            var cellValue = puzzle[cellIndex];
            
            puzzle[cellIndex] = 0;

            var result = _solver.Solve(puzzle);
            
            if (! result.Solved || result.SolutionCount > 1)
            {
                puzzle[cellIndex] = cellValue;

                i++;
            }
        }
    }

    private bool CreateSolvedPuzzle(Span<int> puzzle, int cell = 0)
    {
        while (_candidates[cell].Count > 0)
        {
            var candidateIndex = _rng.Next(_candidates[cell].Count);

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