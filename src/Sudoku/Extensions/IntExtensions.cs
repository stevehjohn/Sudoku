using System.Runtime.CompilerServices;

namespace Sudoku.Extensions;

public static class IntExtensions
{
    extension(int bits)
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public int MultiplyByNine()
        {
            return (bits << 3) + bits;
        }
    }
}