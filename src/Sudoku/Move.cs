namespace Sudoku.Solver;

public class Move
{
    public int X { get; }
    
    public int Y { get; }
    
    public int Value { get; }

    public Move(int x, int y, int value)
    {
        X = x;
        
        Y = y;
        
        Value = value;
    }
}