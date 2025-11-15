namespace Sudoku;

public static class Canoniser
{
    public static int[] CanonisePuzzle(int[] puzzle)
    {
        var workingCopy = new int[81];
        
        Array.Copy(puzzle, workingCopy, 81);

        NormaliseDigits(puzzle);
        
        return workingCopy;
    }

    private static void NormaliseDigits(Span<int> puzzle)
    {
        var mappings = new int[10];

        var digit = 1;
        
        for (var i = 0; i < 81; i++)
        {
            var cellValue = puzzle[i];

            if (mappings[cellValue] == 0)
            {
                mappings[cellValue] = digit++;
            }
        }

        for (var i = 0; i < 81; i++)
        {
            var cellValue = puzzle[i];

            if (cellValue != 0)
            {
                puzzle[i] = mappings[cellValue];
            }
        }
    }

    private static void PermuteBand(Span<int> puzzle, int band)
    {
        for (var i = 0; i < 3; i++)
        {
        }
    }

    private static int Compare(ReadOnlySpan<int> left, ReadOnlySpan<int> right)
    {
        for (var i = 0; i < 9; i++)
        {
            if (left[i] < right[i])
            {
                return -1;
            }

            if (right[i] < left[i])
            {
                return 1;
            }
        }

        return 0;
    }

    private static void SwapRows(Span<int> puzzle, int firstRow, int secondRow)
    {
        firstRow *= 9;

        secondRow *= 9;
        
        for (var i = 0; i < 9; i++)
        {
            (puzzle[firstRow + i], puzzle[secondRow + i]) = (puzzle[secondRow + i], puzzle[firstRow + i]);
        }
    }
}