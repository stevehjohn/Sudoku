namespace Sudoku;

public static class Canoniser
{
    public static int[] CanonisePuzzle(Span<int> puzzle)
    {
        var canon = new int[81];

        var workingCopy = new int[81];
        
        for (var i = 0; i < 8; i++)
        {
            puzzle.CopyTo(workingCopy);
            
            ApplySymmetry(workingCopy, i);
            
            Canonise(workingCopy);

            if (i == 0 || Compare(workingCopy, canon) < 0)
            {
                workingCopy.CopyTo(canon);
            }
        }
        
        NormaliseDigits(canon);

        return canon;
    }
    
    private static void ApplySymmetry(Span<int> puzzle, int symmetry)
    {
        if ((symmetry & 4) != 0)
        {
            Transpose(puzzle);
        }

        if ((symmetry & 2) != 0)
        {
            FlipHorizontally(puzzle);
        }

        if ((symmetry & 1) != 0)
        {
            FlipVertically(puzzle);
        }
    }
    
    private static void Canonise(Span<int> puzzle)
    {
        bool swapped;

        do
        {
            swapped = false;

            for (var pass = 0; pass < 2; pass++)
            {
                for (var i = 0; i < 3; i++)
                {
                    NormaliseDigits(puzzle);

                    swapped |= PermuteBand(puzzle, i);
                }

                NormaliseDigits(puzzle);

                swapped |= PermuteBands(puzzle);

                Transpose(puzzle);
            }
            
        } while (swapped);
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

    private static void FlipVertically(Span<int> puzzle)
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

    private static void FlipHorizontally(Span<int> puzzle)
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

    private static bool PermuteBand(Span<int> puzzle, int band)
    {
        var bandStart = band * 27;

        var firstRow = band * 3;

        var swapped = false;
        
        for (var pass = 0; pass < 2; pass++)
        {
            for (var i = 0; i < 2 - pass; i++)
            {
                var firstRowStart = bandStart + i * 9;

                var secondRowStart = bandStart + (i + 1) * 9;
                
                if (Compare(puzzle.Slice(secondRowStart, 9), puzzle.Slice(firstRowStart, 9)) < 0)
                {
                    SwapRows(puzzle, firstRow + i, firstRow + i + 1);

                    swapped = true;
                }
            }
        }

        return swapped;
    }

    private static bool PermuteBands(Span<int> puzzle)
    {
        var swapped = false;
        
        for (var pass = 0; pass < 2; pass++)
        {
            for (var i = 0; i < 2 - pass; i++)
            {
                var firstBandStart = i * 27;

                var secondBandStart = (i + 1) * 27;
                
                if (Compare(puzzle.Slice(secondBandStart, 27), puzzle.Slice(firstBandStart, 27)) < 0)
                {
                    SwapBands(puzzle, i, i + 1);

                    swapped = true;
                }
            }
        }

        return swapped;
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