namespace Sudoku.Extensions;

public static class SpanIntExtensions
{
    public static bool IsValidSudoku(this Span<int> puzzle)
    {
        Span<ushort> rowMask = stackalloc ushort[9];

        Span<ushort> columnMask = stackalloc ushort[9];

        Span<ushort> boxMask = stackalloc ushort[9];

        for (var i = 0; i < 81; i++)
        {
            var value = puzzle[i];

            if (value == 0)
            {
                continue;
            }

            var bit = (ushort) (1 << value);

            var row = i / 9;

            var column = i % 9;

            var box = row / 3 * 3 + column / 3;

            if (((rowMask[row] | columnMask[column] | boxMask[box]) & bit) != 0)
            {
                return false;
            }

            rowMask[row] |= bit;

            columnMask[column] |= bit;

            boxMask[box] |= bit;
        }

        return true;
    }
}