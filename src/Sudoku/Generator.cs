using Sudoku.Extensions;

namespace Sudoku;

public class Generator
{
    private readonly List<int> _positions = new();

    private readonly List<int>[] _candidates = new List<int>[81];

    private int[] _puzzle;

    private readonly Random _rng = Random.Shared;

    private readonly Solver _solver = new(HistoryType.None, SolveMethod.FindUnique);
    
    public int[] Generate(int cluesToLeave = 30)
    {
        _puzzle = new int[81];
        
        GeneratePositions(cluesToLeave);
        
        FillPositions(cluesToLeave);
        
        return _puzzle;
    }

    private void GeneratePositions(int cluesToLeave)
    {
        _positions.Clear();
        
        for (var i = 0; i < cluesToLeave; i++)
        {
            var position = _rng.Next(81);

            while (_puzzle[position] != 0)
            {
                position = _rng.Next(81);
            }

            _puzzle[position] = -1;
            
            _positions.Add(position);
        }
    }

    private bool FillPositions(int cluesToLeave, int position = 0)
    {
        InitialiseCandidates();

        var cell = _positions[position];

        while (_candidates[cell].Count > 0)
        {
            var index = _rng.Next(_candidates[cell].Count);

            _puzzle[cell] = _candidates[cell][index];
            
            _candidates[cell].RemoveAt(index);

            if (_puzzle.IsValidSudoku())
            {
                if (position == cluesToLeave - 1)
                {
                    return _solver.Solve(_puzzle).Solved;
                }
                
                return FillPositions(cluesToLeave, position + 1);
            }
        }
        
        if (position == 0)
        {
            return false;
        }

        _puzzle[cell] = -1;

        _candidates[cell] = [1, 2, 3, 4, 5, 6, 7, 8, 9];

        return FillPositions(cluesToLeave, position - 1);
    }

    private void InitialiseCandidates()
    {
        for (var i = 0; i < 81; i++)
        {
            _candidates[i] = [1, 2, 3, 4, 5, 6, 7, 8, 9];
        }
    }
}