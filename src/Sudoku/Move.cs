namespace Sudoku;

public struct Move
{
    public int X { get; }
    
    public int Y { get; }
    
    public int Value { get; }
    
    public MoveType Type { get; }

    public int[] Candidates { get; set; } = [];

    private object _metadata;

    public Move(int x, int y, int value, MoveType moveType)
    {
        X = x;
        
        Y = y;
        
        Value = value;

        Type = moveType;
    }
    
    public void AddMetadata<T>(T metadata)
    {
        _metadata = metadata;
    }

    public T GetMetadata<T>()
    {
        return (T) _metadata;
    }
}