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
        var source = Random.Shared.Next(3) * 27;

        var target = (Random.Shared.Next(2) + source + 1) % 3 * 27;

        for (var row = 0; row < 27; row += 9)
        {
            for (var x = 0; x < 9; x++)
            {
                var offset = row + x;

                (puzzle[source + offset], puzzle[target + offset]) = (puzzle[target + offset], puzzle[source + offset]);
            }
        }
    }
}