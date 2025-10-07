using System.Numerics;

namespace Sudoku.Extensions;

public static class IntExtensions
{
    public static List<int> BitsToCandidates(this int bits)
    {
        var result = new List<int>();

        while (bits > 0)
        {
            var value = BitOperations.TrailingZeroCount(bits) + 1;

            bits &= bits - 1;
            
            result.Add(value);
        }

        return result;
    }
}