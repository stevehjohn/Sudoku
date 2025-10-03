namespace Sudoku;

public struct Candidates
{
    private ulong _high = ulong.MaxValue;

    private ulong _low = ulong.MaxValue;

    public Candidates()
    {
    }

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
        set
        {
            if (index < 5)
            {
                var mask = ~(0b1_1111_1111ul << (index * 9));
                
                _high = (_high & mask) | (((ulong) value & 0b1_1111_1111) << (index * 9));
                
                return;
            }

            var maskLow = ~(0b1_1111_1111ul << ((index - 5) * 9));
            
            _low = (_low & maskLow) | (((ulong) value & 0b1_1111_1111) << ((index - 5) * 9));
        }
    }
}