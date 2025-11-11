using System.Numerics;
using System.Runtime.CompilerServices;

namespace Sudoku.Extensions;

public static class IntExtensions
{
    extension(int bits)
    {
        public List<int> BitsToCandidates()
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
        public int MultiplyByNine()
        {
            return (bits << 3) + bits;
        }
    }
}