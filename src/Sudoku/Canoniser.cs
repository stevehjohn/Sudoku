namespace Sudoku;

public static class Canoniser
{
    public static Span<int> CanonisePuzzle(Span<int> puzzle)
    {
        var workingCopy = new Span<int>(new int[81]);
        
        puzzle.CopyTo(workingCopy);

        NormaliseDigits(workingCopy);

        var transposed = new Span<int>(new int[81]);

        for (var pass = 0; pass < 2; pass++)
        {
            for (var i = 0; i < 3; i++)
            {
                PermuteBand(workingCopy, i);
            }

            PermuteBands(workingCopy);

            if (pass == 0)
            {
                Transpose(workingCopy);
                
                workingCopy.CopyTo(transposed);
            }
        }

        if (Compare(workingCopy, transposed) < 0)
        {
            return workingCopy;
        }

        return transposed;
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

        var firstRow = band * 3;
        
        for (var pass = 0; pass < 2; pass++)
        {
            for (var i = 0; i < 2 - pass; i++)
            {
                var firstRowStart = bandStart + i * 9;

                var secondRowStart = bandStart + (i + 1) * 9;
                
                if (Compare(puzzle.Slice(secondRowStart, 9), puzzle.Slice(firstRowStart, 9)) < 0)
                {
                    SwapRows(puzzle, firstRow + i, firstRow + i + 1);
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
                var firstBandStart = i * 27;

                var secondBandStart = (i + 1) * 27;
                
                if (Compare(puzzle.Slice(secondBandStart, 27), puzzle.Slice(firstBandStart, 27)) < 0)
                {
                    SwapBands(puzzle, i, i + 1);
                }
            }
        }
    }

    private static void Transpose(Span<int> puzzle)
    {
        for (var y = 0; y < 9; y++)
        {
            for (var x = y + 1; x < 9; x++)
            {
                var left = y * 9 + x;

                var right = x * 9 + y;

                (puzzle[left], puzzle[right]) = (puzzle[right], puzzle[left]);
            }
        }
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