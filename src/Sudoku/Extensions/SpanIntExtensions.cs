namespace Sudoku.Extensions;

public static class SpanIntExtensions
{
    public static bool IsValidSudoku(this Span<int> puzzle)
    {
        var uniqueRow = new HashSet<int>();

        var uniqueColumn = new HashSet<int>();

        for (var y = 0; y < 9; y++)
        {
            uniqueRow.Clear();

            uniqueColumn.Clear();

            var countRow = 0;

            var countColumn = 0;

            for (var x = 0; x < 9; x++)
            {
                if (puzzle[x + y * 9] != 0)
                {
                    uniqueRow.Add(puzzle[x + y * 9]);

                    countRow++;
                }

                if (puzzle[y + x * 9] == 0)
                {
                    continue;
                }
                
                uniqueColumn.Add(puzzle[y + x * 9]);

                countColumn++;
            }

            if (uniqueRow.Count < countRow || uniqueColumn.Count < countColumn)
            {
                return false;
            }
        }

        var uniqueBox = new HashSet<int>();

        for (var yO = 0; yO < 9; yO += 3)
        {
            for (var xO = 0; xO < 9; xO += 3)
            {
                uniqueBox.Clear();

                var countBox = 0;

                for (var x = 0; x < 3; x++)
                {
                    for (var y = 0; y < 3; y++)
                    {
                        if (puzzle[(yO + y) * 9 + xO + x] == 0)
                        {
                            continue;
                        }
                        
                        uniqueBox.Add(puzzle[(yO + y) * 9 + xO + x]);

                        countBox++;
                    }
                }

                if (uniqueBox.Count < countBox)
                {
                    return false;
                }
            }
        }

        return true;
    }
}