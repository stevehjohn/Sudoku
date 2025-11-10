using System.Numerics;
using Sudoku.Extensions;

namespace Sudoku;

public struct Candidates
{
    private ulong _high = ulong.MaxValue;

    private ulong _low = ulong.MaxValue;

    public Candidates()
    {
    }

    public void Clear()
    {
        _high = 0;

        _low = 0;
    }

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
            _low &= ~(1ul << ((index - 5).MultiplyByNine() + value - 1));
        }
    }

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
            _low |= 1ul << ((index - 5).MultiplyByNine() + value - 1);
        }
    }

    public int Count(int index)
    {
        return BitOperations.PopCount((uint) this[index]);
    }

    public int this[int index]
    {
        get
        {
            if (index < 5)
            {
                return (int) (_high >> index.MultiplyByNine()) & 0b1_1111_1111;
            }

            return (int) (_low >> (index - 5).MultiplyByNine()) & 0b1_1111_1111;
        }
        set
        {
            if (index < 5)
            {
                index = index.MultiplyByNine();

                var mask = ~(0b1_1111_1111ul << index);

                _high = (_high & mask) | (((ulong) value & 0b1_1111_1111) << index);

                return;
            }

            index = (index - 5).MultiplyByNine();

            var maskLow = ~(0b1_1111_1111ul << index);

            _low = (_low & maskLow) | (((ulong) value & 0b1_1111_1111) << index);
        }
    }
}