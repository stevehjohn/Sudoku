namespace Sudoku;

public class Generator
{
    private readonly List<int>[] _candidates = new List<int>[81];

    private readonly Random _rng = Random.Shared;
    
    public int[] Generate(int cellsToRemove = 51)
    {
        var puzzle = new int[81];
        
        InitialiseCandidates();

        CreatePuzzle(puzzle);
        
        return puzzle;
    }

    private void CreatePuzzle(Span<int> puzzle)
    {
    }

    private void InitialiseCandidates()
    {
        for (var i = 0; i < 81; i++)
        {
            for (var c = 1; c < 10; c++)
            {
                _candidates[i].Clear();
                
                _candidates[i].Add(c);
            }
        }
    }
}