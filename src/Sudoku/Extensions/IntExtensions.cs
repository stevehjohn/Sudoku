using System.Numerics;
using System.Runtime.CompilerServices;

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

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static int MultiplyByNine(this int value)
    {
        return (value << 3) + value;
    }
}