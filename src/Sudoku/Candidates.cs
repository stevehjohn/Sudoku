namespace Sudoku.Solver;

public class Candidates
{
    private ulong _high;

    private ulong _low;

    public void Remove(int index, int value)
    {
        if (value == 0)
        {
            return;
        }

        if (index < 5)
        {
            _high &= ~(1ul << (index * 9 + value - 1));
        }
        else
        {
            _low &= ~(1ul << ((index - 5) * 9 + value - 1));
        }
    }

    public void Reset()
    {
        _low = ulong.MaxValue;

        _high = ulong.MaxValue;
    }

    public int this[int index]
    {
        get
        {
            if (index < 5)
            {
                return (int) (_high >> (index * 9)) & 0b1_1111_1111;
            }

            return (int) (_low >> ((index - 5) * 9)) & 0b1_1111_1111;
        }
        // set
        // {
        //     if (index < 5)
        //     {
        //         _high &= ~(0b1_1111_1111ul << (index * 9));
        //
        //         _high |= (ulong) value << (index * 9);
        //     }
        //     else
        //     {
        //         _low &= ~(0b1_1111_1111ul << ((index - 5) * 9));
        //
        //         _low |= (ulong) value << ((index - 5) * 9);
        //     }
        // }
    }
}