using System.Runtime.CompilerServices;

namespace Sudoku.Extensions;

public static class SpanIntExtensions
{
    extension(Span<int> array)
    {
        public bool IsValidSudoku()
        {
            Span<ushort> rowMask = stackalloc ushort[9];

            Span<ushort> columnMask = stackalloc ushort[9];

            Span<ushort> boxMask = stackalloc ushort[9];

            for (var i = 0; i < 81; i++)
            {
                var value = array[i];

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
        public bool IsValidSudoku(int updatedCell)
        {
            var value = array[updatedCell];

            if (value == 0)
            {
                return true;
            }

            var peers = UnitTables.Peers(updatedCell);

            for (var i = 0; i < peers.Length; i++)
            {
                var peerIndex = peers[i];

                if (array[peerIndex] == value)
                {
                    return false;
                }
            }

            return true;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Shuffle()
        {
            for (var left = 0; left < array.Length - 1; left++)
            {
                var right = left + Random.Shared.Next(array.Length - left);

                (array[left], array[right]) = (array[right], array[left]);
            }
        }
    }
}