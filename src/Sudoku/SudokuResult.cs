namespace Sudoku;

public class SudokuResult
{
    public int[] Solution { get; }
    
    public bool Solved { get; }
    
    public int Steps { get; }

    public double ElapsedMilliseconds { get; }
    
    public List<Move> History { get; }
    
    public List<int>[] InitialCandidates { get; }
    
    public string Message { get; }
}