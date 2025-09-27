namespace Sudoku;

public static class UnitTables
{
    private static readonly int[] Units;

    static UnitTables()
    {
        Units = new int[27 * 9];

        var i = 0;

        for (var y = 0; y < 9; y++)
        {
            for (var x = 0; x < 9; x++)
            {
                Units[i++] = y * 9 + x;
            }
        }

        for (var x = 0; x < 9; x++)
        {
            for (var y = 0; y < 9; y++)
            {
                Units[i++] = y * 9 + x;
            }
        }
    }

    public static ReadOnlySpan<int> Row(int index) => Units.AsSpan(index * 9, 9);

    public static ReadOnlySpan<int> Column(int index) => Units.AsSpan(81 + index * 9, 9);

    public static ReadOnlySpan<int> Box(int index) => Units.AsSpan(162 + index * 9, 9);
}