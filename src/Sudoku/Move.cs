namespace Sudoku;

public struct Move
{
    public int X { get; }
    
    public int Y { get; }
    
    public int Value { get; }
    
    public bool Remove { get; }
    
    public int[] Candidates { get; set; }

    public Move(int x, int y, int value, bool remove)
    {
        X = x;
        
        Y = y;
        
        Value = value;

        Remove = remove;
    }
}