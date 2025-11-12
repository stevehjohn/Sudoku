using Sudoku.Extensions;

namespace Sudoku;

public static class IsomorphGenerator
{
    public static List<int[]> CreateIsomorphs(int count, int[] puzzle)
    {
        var isomorphs = new List<int[]>();

        while (count > 0)
        {
            var isomorph = new int[81];

            isomorphs.Add(isomorph);

            Array.Copy(puzzle, isomorph, 81);

            RelabelDigits(puzzle);

            SwapBands(puzzle);

            SwapStacks(puzzle);

            RotateOrFlip(puzzle);

            count--;
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
                Rotate(puzzle, Random.Shared.Next(3));
                
                break;
            
            case 1:
                // TODO: Flip H or V
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

    private static void Rotate(Span<int> puzzle, int times)
    {
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
        for (var column = 0; column < 5; column++)
        {
            var sourceX = column * 9;

            var targetX = 8 - column;
            
            for (var x = 0; x < 81; x += 9)
            {
                (puzzle[sourceX + x], puzzle[targetX + x]) = (puzzle[targetX + x], puzzle[sourceX + x]);
            }
        }
    }
}