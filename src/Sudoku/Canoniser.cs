namespace Sudoku;

public static class Canoniser
{
    public static int[] CanonisePuzzle(int[] puzzle)
    {
        var workingCopy = new int[81];
        
        Array.Copy(puzzle, workingCopy, 81);

        NormaliseDigits(workingCopy);

        for (var pass = 0; pass < 2; pass++)
        {
            for (var i = 0; i < 3; i++)
            {
                PermuteBand(workingCopy, i);
            }

            PermuteBands(workingCopy);

            Transpose(workingCopy);
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
                var firstBandStart = bandStart + i * 9;

                var secondBandStart = bandStart + (i + 1) * 9;
                
                if (Compare(puzzle.Slice(secondBandStart, 9), puzzle.Slice(firstBandStart, 9)) < 0)
                {
                    SwapRows(puzzle, firstBandStart, secondBandStart);
                }
            }
        }
    }

    private static void PermuteBands(Span<int> puzzle)
    {
        for (var pass = 0; pass < 2; pass++)
        {
            for (var i = 0; i < 2 - pass; i++)
            {
                var firstRowStart = i * 27;

                var secondRowStart = (i + 1) * 27;
                
                if (Compare(puzzle.Slice(secondRowStart, 27), puzzle.Slice(firstRowStart, 27)) < 0)
                {
                    SwapBands(puzzle, firstRowStart, secondRowStart);
                }
            }
        }
    }

    private static void Transpose(Span<int> puzzle)
    {
    }

    private static int Compare(ReadOnlySpan<int> left, ReadOnlySpan<int> right)
    {
        for (var i = 0; i < left.Length; i++)
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

    private static void SwapBands(Span<int> puzzle, int firstBand, int secondBand)
    {
        firstBand *= 27;

        secondBand *= 27;
        
        for (var i = 0; i < 27; i++)
        {
            (puzzle[firstBand + i], puzzle[secondBand + i]) = (puzzle[secondBand + i], puzzle[firstBand + i]);
        }
    }
}