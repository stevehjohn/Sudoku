using System.Runtime.CompilerServices;

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

            var row = UnitTables.CellRow(i);

            var column = UnitTables.CellColumn(i);

            var box = UnitTables.CellBox(i);

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
    
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static bool IsValidSudoku(this Span<int> puzzle, int updatedCell)
    {
        var value = puzzle[updatedCell];

        if (value == 0)
        {
            return true;
        }

        var peers = UnitTables.Peers(updatedCell);

        for (var i = 0; i < peers.Length; i++)
        {
            var peerIndex = peers[i];

            if (puzzle[peerIndex] == value)
            {
                return false;
            }
        }

        return true;
    }

}