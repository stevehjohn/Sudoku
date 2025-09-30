using System;
using System.Collections.Generic;

namespace Sudoku;

internal static class UnavoidableSetFinder
{
    public static List<int[]> Find(ReadOnlySpan<int> solution)
    {
        var unavoidableSets = new List<int[]>();
        var seen = new HashSet<string>();

        var rowDigitColumn = new int[9, 10];
        var columnDigitRow = new int[9, 10];

        for (var index = 0; index < 81; index++)
        {
            var digit = solution[index];
            if (digit == 0)
            {
                throw new ArgumentException("Solution must be a completed puzzle", nameof(solution));
            }

            var row = index / 9;
            var column = index % 9;

            rowDigitColumn[row, digit] = column;
            columnDigitRow[column, digit] = row;
        }

        Span<bool> visited = stackalloc bool[81];

        for (var firstDigit = 1; firstDigit <= 8; firstDigit++)
        {
            for (var secondDigit = firstDigit + 1; secondDigit <= 9; secondDigit++)
            {
                visited.Clear();

                for (var row = 0; row < 9; row++)
                {
                    var column = rowDigitColumn[row, firstDigit];
                    var index = row * 9 + column;

                    if (visited[index])
                    {
                        continue;
                    }

                    var cycle = BuildCycle(index, firstDigit, secondDigit, solution, rowDigitColumn, columnDigitRow, visited);

                    if (cycle.Count > 0)
                    {
                        cycle.Sort();
                        var key = string.Join(',', cycle);

                        if (seen.Add(key))
                        {
                            unavoidableSets.Add(cycle.ToArray());
                        }
                    }
                }
            }
        }

        return unavoidableSets;
    }

    private static List<int> BuildCycle(
        int startIndex,
        int firstDigit,
        int secondDigit,
        ReadOnlySpan<int> solution,
        int[,] rowDigitColumn,
        int[,] columnDigitRow,
        Span<bool> visited)
    {
        var cycle = new List<int>();

        var index = startIndex;
        var stepRow = true;

        while (true)
        {
            if (visited[index])
            {
                if (index == startIndex && cycle.Count > 0)
                {
                    break;
                }

                return new List<int>();
            }

            cycle.Add(index);
            visited[index] = true;

            index = stepRow
                ? GetRowNeighbour(index, firstDigit, secondDigit, solution, rowDigitColumn)
                : GetColumnNeighbour(index, firstDigit, secondDigit, solution, columnDigitRow);

            stepRow = !stepRow;
        }

        return cycle;
    }

    private static int GetRowNeighbour(
        int index,
        int firstDigit,
        int secondDigit,
        ReadOnlySpan<int> solution,
        int[,] rowDigitColumn)
    {
        var row = index / 9;
        var digit = solution[index];
        var nextDigit = digit == firstDigit ? secondDigit : firstDigit;
        var column = rowDigitColumn[row, nextDigit];

        return row * 9 + column;
    }

    private static int GetColumnNeighbour(
        int index,
        int firstDigit,
        int secondDigit,
        ReadOnlySpan<int> solution,
        int[,] columnDigitRow)
    {
        var column = index % 9;
        var digit = solution[index];
        var nextDigit = digit == firstDigit ? secondDigit : firstDigit;
        var row = columnDigitRow[column, nextDigit];

        return row * 9 + column;
    }
}
