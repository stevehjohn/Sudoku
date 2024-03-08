namespace Sudoku;

public class Generator
{
    private readonly List<int>[] _candidates = new List<int>[81];

    private readonly Random _rng = Random.Shared;

    private readonly Solver _solver = new();
    
    public int[] Generate(int cellsToRemove = 51)
    {
        var puzzle = new int[81];
        
        InitialiseCandidates();

        CreateSolvedPuzzle(puzzle);

        RemoveCells(puzzle, cellsToRemove);

        return puzzle;
    }

    private void RemoveCells(int[] puzzle, int cellsToRemove)
    {
        var copy = new int[81];

        for (var i = 0; i < 81; i++)
        {
            copy[i] = puzzle[i];
        }

        var filledCells = new List<int>();

        for (var i = 0; i < 81; i++)
        {
            filledCells.Add(i);
        }

        while (true)
        {
            for (var i = 0; i < cellsToRemove; i++)
            {
                var cell = filledCells[_rng.Next(filledCells.Count)];

                filledCells.Remove(cell);
                
                puzzle[cell] = 0;
            }

            if (_solver.Solve(puzzle, false, true).Solution != null)
            {
                return;
            }

            for (var i = 0; i < 81; i++)
            {
                if (puzzle[i] == 0)
                {
                    filledCells.Add(i);

                    puzzle[i] = copy[i];
                }
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

            if (IsValid(puzzle))
            {
                if (cell == 80)
                {
                    return true;
                }
                
                return CreateSolvedPuzzle(puzzle, cell + 1);
            }
        }
        
        puzzle[cell] = 0;

        _candidates[cell] = [1, 2, 3, 4, 5, 6, 7, 8, 9];

        return CreateSolvedPuzzle(puzzle, cell - 1);
    }

    private static bool IsValid(Span<int> puzzle)
    {
        var uniqueRow = new HashSet<int>();

        var uniqueColumn = new HashSet<int>();

        for (var y = 0; y < 9; y++)
        {
            uniqueRow.Clear();
            
            uniqueColumn.Clear();

            var countRow = 0;

            var countColumn = 0;

            for (var x = 0; x < 9; x++)
            {
                if (puzzle[x + y * 9] != 0)
                {
                    uniqueRow.Add(puzzle[x + y * 9]);

                    countRow++;
                }

                if (puzzle[y + x * 9] != 0)
                {
                    uniqueColumn.Add(puzzle[y + x * 9]);

                    countColumn++;
                }
            }

            if (uniqueRow.Count < countRow || uniqueColumn.Count < countColumn)
            {
                return false;
            }
        }

        var uniqueBox = new HashSet<int>();

        for (var yO = 0; yO < 9; yO += 3)
        {
            for (var xO = 0; xO < 9; xO += 3)
            {
                uniqueBox.Clear();
                
                var countBox = 0;

                for (var x = 0; x < 3; x++)
                {
                    for (var y = 0; y < 3; y++)
                    {
                        if (puzzle[(yO + y) * 9 + xO + x] != 0)
                        {
                            uniqueBox.Add(puzzle[(yO + y) * 9 + xO + x]);

                            countBox++;
                        }
                    }
                }

                if (uniqueBox.Count < countBox)
                {
                    return false;
                }
            }
        }

        
        return true;
    }

    private void InitialiseCandidates()
    {
        for (var i = 0; i < 81; i++)
        {
            _candidates[i] = [1, 2, 3, 4, 5, 6, 7, 8, 9];
        }
    }
}