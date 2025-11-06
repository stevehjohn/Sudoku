namespace Sudoku;

public static class UnitTables
{
    private static readonly byte[] Units;

    static UnitTables()
    {
        Units = new byte[27 * 9 * 2 + 9 + 81 * 20];

        var i = 0;

        for (var y = 0; y < 9; y++)
        {
            for (var x = 0; x < 9; x++)
            {
                Units[i++] = (byte) (y * 9 + x);
            }
        }

        for (var x = 0; x < 9; x++)
        {
            for (var y = 0; y < 9; y++)
            {
                Units[i++] = (byte) (y * 9 + x);
            }
        }

        for (var box = 0; box < 9; box++)
        {
            var row = 3 * (box / 3);
            
            var column = box % 3 * 3;
            
            for (var dy = 0; dy < 3; dy++)
            {
                for (var dx = 0; dx < 3; dx++)
                {
                    Units[i++] = (byte) ((row + dy) * 9 + column + dx);
                }
            }
        }

        for (i = 0; i < 81; i++)
        {
            var y = i / 9;
            
            Units[243 + i] = (byte) y;

            var x = i % 9;
            
            Units[324 + i] = (byte) x;

            Units[405 + i] = (byte) (y / 3 * 3 + x / 3);
        }

        for (i = 0; i < 9; i++)
        {
            Units[486 + i] = (byte) (3 * (i / 3) * 9 + 3 * (i % 3));
        }
    }

    public static ReadOnlySpan<byte> RowCells(int index) => Units.AsSpan(index * 9, 9);

    public static ReadOnlySpan<byte> ColumnCells(int index) => Units.AsSpan(81 + index * 9, 9);

    public static ReadOnlySpan<byte> BoxCells(int index) => Units.AsSpan(162 + index * 9, 9);

    public static byte CellRow(int index) => Units[243 + index];

    public static byte CellColumn(int index) => Units[324 + index];

    public static byte CellBox(int index) => Units[405 + index];

    public static ReadOnlySpan<byte> BoxStartIndices => Units.AsSpan(486, 9);

    public static ReadOnlySpan<byte> Peers(int index) => Units.AsSpan(495 + index * 20, 20);
}