using Sudoku.Extensions;

namespace Sudoku;

public static class IsomorphGenerator
{
    public static List<int[]> CreateIsomorphs(int[] puzzle, int count)
    {
        var unique = new HashSet<string>();
        
        var isomorphs = new List<int[]>();

        while (unique.Count < count)
        {
            var isomorph = new int[81];

            Array.Copy(puzzle, isomorph, 81);

            RelabelDigits(isomorph);

            SwapBands(isomorph);

            SwapStacks(isomorph);

            RotateOrFlip(isomorph);

            SwapBandRows(isomorph);
            
            SwapStackColumns(isomorph);

            if (unique.Add(isomorph.FlattenPuzzle()))
            {
                isomorphs.Add(isomorph);
            }
        }

        return isomorphs;
    }

    private static void RelabelDigits(Span<int> puzzle)
    {
        Span<int> digits = [1, 2, 3, 4, 5, 6, 7, 8, 9];

        digits.Shuffle();

        for (var i = 0; i < 81; i++)
        {
            var cell = puzzle[i];

            if (cell == 0)
            {
                continue;
            }

            puzzle[i] = digits[cell - 1];
        }
    }

    private static void SwapBands(Span<int> puzzle)
    {
        var sourceBand = Random.Shared.Next(3);

        var targetBand = (Random.Shared.Next(2) + sourceBand + 1) % 3;

        var source = sourceBand * 27;

        var target = targetBand * 27;

        for (var row = 0; row < 27; row += 9)
        {
            for (var x = 0; x < 9; x++)
            {
                var offset = row + x;

                (puzzle[source + offset], puzzle[target + offset]) = (puzzle[target + offset], puzzle[source + offset]);
            }
        }
    }

    private static void SwapStacks(Span<int> puzzle)
    {
        var sourceStack = Random.Shared.Next(3);

        var targetStack = (Random.Shared.Next(2) + sourceStack + 1) % 3;

        var source = sourceStack * 3;

        var target = targetStack * 3;

        for (var column = 0; column < 3; column++)
        {
            for (var y = 0; y < 81; y += 9)
            {
                var offset = column + y;

                (puzzle[source + offset], puzzle[target + offset]) = (puzzle[target + offset], puzzle[source + offset]);
            }
        }
    }

    private static void RotateOrFlip(Span<int> puzzle)
    {
        switch (Random.Shared.Next(2))
        {
            case 0:
                var times = Random.Shared.Next(3) + 1;

                for (var i = 0; i < times; i++)
                {
                    RotateClockwise(puzzle);
                }
                
                break;
            
            case 1:
                if (Random.Shared.Next(2) == 0)
                {
                    FlipHorizontally(puzzle);
                }
                else
                {
                    FlipVertically(puzzle);
                }

                break;
        }
    }

    private static void RotateClockwise(Span<int> puzzle)
    {
        var copy = new int[81];

        for (var x = 0; x < 9; x++)
        {
            for (var y = 0; y < 9; y++)
            {
                copy[x + y * 9] = puzzle[(8 - x) * 9 + y];
            }
        }

        for (var i = 0; i < 81; i++)
        {
            puzzle[i] = copy[i];
        }
    }

    private static void FlipHorizontally(Span<int> puzzle)
    {
        for (var row = 0; row < 5; row++)
        {
            var sourceY = row * 9;

            var targetY = (8 - row) * 9;
            
            for (var x = 0; x < 9; x++)
            {
                (puzzle[sourceY + x], puzzle[targetY + x]) = (puzzle[targetY + x], puzzle[sourceY + x]);
            }
        }
    }

    private static void FlipVertically(Span<int> puzzle)
    {
        for (var sourceX = 0; sourceX < 5; sourceX++)
        {
            var targetX = 8 - sourceX;
            
            for (var x = 0; x < 81; x += 9)
            {
                (puzzle[sourceX + x], puzzle[targetX + x]) = (puzzle[targetX + x], puzzle[sourceX + x]);
            }
        }
    }

    private static void SwapBandRows(Span<int> puzzle)
    {
        var band = Random.Shared.Next(3) * 27;
        
        var sourceRow = Random.Shared.Next(3);

        var targetRow = (Random.Shared.Next(2) + sourceRow + 1) % 3;

        var sourceOffset = band + sourceRow * 9;

        var targetOffset = band + targetRow * 9;
        
        for (var x = 0; x < 9; x++)
        {
            (puzzle[sourceOffset + x], puzzle[targetOffset + x]) = (puzzle[targetOffset + x], puzzle[sourceOffset + x]);
        }
    }

    private static void SwapStackColumns(Span<int> puzzle)
    {
        var stack = Random.Shared.Next(3) * 3;
        
        var sourceColumn = Random.Shared.Next(3);

        var targetColumn = (Random.Shared.Next(2) + sourceColumn + 1) % 3;

        var sourceOffset = stack + sourceColumn;

        var targetOffset = stack + targetColumn;
        
        for (var y = 0; y < 81; y += 9)
        {
            (puzzle[sourceOffset + y], puzzle[targetOffset + y]) = (puzzle[targetOffset + y], puzzle[sourceOffset + y]);
        }
    }
}