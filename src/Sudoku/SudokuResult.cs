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

    public SudokuResult(int[] solution, bool solved, int steps, double elapsedMilliseconds, List<Move> history, List<int>[] initialCandidates, string message)
    {
        Solution = solution;
        
        Solved = solved;
        
        Steps = steps;
        
        ElapsedMilliseconds = elapsedMilliseconds;
        
        History = history;
        
        InitialCandidates = initialCandidates;
        
        Message = message;
    }
}