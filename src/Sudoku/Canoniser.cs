namespace Sudoku;

public static class Canoniser
{
    private static readonly byte[][] Permutations = new byte[1_296][];

    static Canoniser()
    {
        var permutations = new[]
        {
            new[] { 0, 1, 2 }, new[] { 0, 2, 1 }, new[] { 1, 0, 2 },
            new[] { 1, 2, 0 }, new[] { 2, 0, 1 }, new[] { 2, 1, 0 }
        };

        var index = 0;

        foreach (var band in permutations)
        {
            foreach (var row1 in permutations)
            {
                foreach (var row2 in permutations)
                {
                    foreach (var row3 in permutations)
                    {
                        var permutation = new byte[9];

                        for (var i = 0; i < 3; i++)
                        {
                            permutation[i] = (byte) (band[0] * 3 + row1[i]);
                        }

                        for (var i = 0; i < 3; i++)
                        {
                            permutation[i + 3] = (byte) (band[1] * 3 + row2[i]);
                        }

                        for (var i = 0; i < 3; i++)
                        {
                            permutation[i + 6] = (byte) (band[2] * 3 + row3[i]);
                        }

                        Permutations[index++] = permutation;
                    }
                }
            }
        }
    }

    public static int[] CanonisePuzzle(ReadOnlySpan<int> puzzle)
    {
        var bestCanon = new int[81];

        Array.Fill(bestCanon, int.MaxValue);

        CheckAllPermutations(puzzle, bestCanon, false);

        CheckAllPermutations(puzzle, bestCanon, true);

        return bestCanon;
    }

    private static void CheckAllPermutations(ReadOnlySpan<int> puzzle, Span<int> bestCanon, bool transpose)
    {
        Span<int> digitMap = stackalloc int[10];

        for (var rowIndex = 0; rowIndex < 1296; rowIndex++)
        {
            var rowMap = Permutations[rowIndex];

            for (var columnIndex = 0; columnIndex < 1296; columnIndex++)
            {
                var columnMap = Permutations[columnIndex];

                digitMap.Clear();
                
                var nextDigit = 1;

                for (var cellIndex = 0; cellIndex < 81; cellIndex++)
                {
                    var y = cellIndex / 9;
                    
                    var x = cellIndex % 9;

                    var sourceY = rowMap[y];
                    
                    var sourceX = columnMap[x];

                    var cellValue = transpose ? puzzle[sourceX * 9 + sourceY] : puzzle[sourceY * 9 + sourceX];

                    var canonicalValue = 0;
                    
                    if (cellValue != 0)
                    {
                        var mappedValue = digitMap[cellValue];
                        
                        if (mappedValue == 0)
                        {
                            mappedValue = nextDigit++;
                            
                            digitMap[cellValue] = mappedValue;
                        }

                        canonicalValue = mappedValue;
                    }

                    var bestVal = bestCanon[cellIndex];

                    if (canonicalValue < bestVal)
                    {
                        bestCanon[cellIndex] = canonicalValue;
                        
                        WriteRemainingCells(puzzle, bestCanon, cellIndex + 1, rowMap, columnMap, transpose, digitMap, ref nextDigit);
                        
                        break;
                    }
                    
                    if (canonicalValue > bestVal)
                    {
                        break;
                    }
                }
            }
        }
    }

    private static void WriteRemainingCells(ReadOnlySpan<int> puzzle, Span<int> bestCanon, int startIndex, byte[] rowMap, byte[] colMap, bool transpose, Span<int> digitMap, ref int nextDigit)
    {
        for (var cell = startIndex; cell < 81; cell++)
        {
            var y = cell / 9;
            
            var x = cell % 9;
            
            var sourceY = rowMap[y];
            
            var sourceX = colMap[x];

            var cellValue = transpose
                ? puzzle[sourceX * 9 + sourceY]
                : puzzle[sourceY * 9 + sourceX];

            var canonicalValue = 0;
            
            if (cellValue != 0)
            {
                var mapped = digitMap[cellValue];
                
                if (mapped == 0)
                {
                    mapped = nextDigit++;
                    
                    digitMap[cellValue] = mapped;
                }

                canonicalValue = mapped;
            }

            bestCanon[cell] = canonicalValue;
        }
    }
}