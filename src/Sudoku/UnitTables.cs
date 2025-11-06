namespace Sudoku;

public static class UnitTables
{
    private static readonly byte[] Units;

    static UnitTables()
    {
        Units = new byte[27 * 9 * 2];

        var i = 0;

        for (var y = 0; y < 9; y++)
        {
            for (var x = 0; x < 9; x++)
            {
                Units[i] = (byte) (y * 9 + x);

                Units[243 + i++] = (byte) y;
            }
        }

        for (var x = 0; x < 9; x++)
        {
            for (var y = 0; y < 9; y++)
            {
                Units[i] = (byte) (y * 9 + x);

                Units[243 + i++] = (byte) x;
            }
        }

        for (var box = 0; box < 9; box++)
        {
            var r0 = 3 * (box / 3);
            
            var c0 = box % 3 * 3;
            
            for (var dy = 0; dy < 3; dy++)
            {
                for (var dx = 0; dx < 3; dx++)
                {
                    Units[i] = (byte) ((r0 + dy) * 9 + c0 + dx);

                    Units[243 + i++] = (byte) box;
                }
            }
        }
    }

    public static ReadOnlySpan<byte> RowCells(int index) => Units.AsSpan(index * 9, 9);

    public static ReadOnlySpan<byte> ColumnCells(int index) => Units.AsSpan(81 + index * 9, 9);

    public static ReadOnlySpan<byte> BoxCells(int index) => Units.AsSpan(162 + index * 9, 9);

    public static byte CellRow(int index) => Units[243 + index];

    public static byte CellColumn(int index) => Units[324 + index];

    public static byte CellBox(int index) => Units[405 + index];
}