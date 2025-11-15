namespace Sudoku;

public static class Canoniser
{
    public static int[] CanonisePuzzle(int[] puzzle)
    {
        var workingCopy = new int[81];
        
        Array.Copy(puzzle, workingCopy, 81);

        NormaliseDigits(workingCopy);

        for (var i = 0; i < 3; i++)
        {
            PermuteBand(workingCopy, i);
        }

        return workingCopy;
    }

    private static void NormaliseDigits(Span<int> puzzle)
    {
        var mappings = new int[10];

        var digit = 1;
        
        for (var i = 0; i < 81; i++)
        {
            var cellValue = puzzle[i];

            if (cellValue == 0)
            {
                continue;
            }

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
        var bandStart = band * 27;
        
        for (var pass = 0; pass < 2; pass++)
        {
            for (var i = 0; i < 2 - pass; i++)
            {
                var firstRowStart = bandStart + i * 9;

                var secondRowStart = bandStart + (i + 1) * 9;
                
                if (Compare(puzzle.Slice(secondRowStart, 9), puzzle.Slice(firstRowStart, 9)) < 0)
                {
                    SwapRows(puzzle, band * 3 + i, band * 3 + i + 1);
                }
            }
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