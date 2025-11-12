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
}