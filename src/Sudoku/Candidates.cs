using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using Sudoku.Extensions;

namespace Sudoku;

[StructLayout(LayoutKind.Sequential, Size = 16)]
public struct Candidates
{
    private ulong _high = ulong.MaxValue;

    private ulong _low = ulong.MaxValue;

    public Candidates()
    {
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void Remove(int index, int value)
    {
        if (value == 0)
        {
            return;
        }

        if (index < 5)
        {
            _high &= ~(1ul << (index.MultiplyByNine() + value - 1));
        }
        else
        {
            index -= 5;
            
            _low &= ~(1ul << (index.MultiplyByNine() + value - 1));
        }
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void Add(int index, int value)
    {
        if (value == 0)
        {
            return;
        }

        if (index < 5)
        {
            _high |= 1ul << (index.MultiplyByNine() + value - 1);
        }
        else
        {
            index -= 5;
            
            _low |= 1ul << (index.MultiplyByNine() + value - 1);
        }
    }

    public int this[int index]
    {
        get
        {
            if (index < 5)
            {
                return (int) (_high >> index.MultiplyByNine()) & 0b1_1111_1111;
            }

            index -= 5;
            
            return (int) (_low >> (index.MultiplyByNine())) & 0b1_1111_1111;
        }
    }
}