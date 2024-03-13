namespace Sudoku;

public class SudokuResult
{
    private readonly int[] _solution;
    
    public bool Solved { get; }
    
    public int Steps { get; }

    public double ElapsedMicroseconds { get; }
    
    public List<Move> History { get; }
    
    public List<int>[] InitialCandidates { get; }
    
    public string Message { get; }

    public int this[int index] => _solution[index];

    public SudokuResult(int[] solution, bool solved, int steps, double elapsedMilliseconds, List<Move> history, List<int>[] initialCandidates, string message)
    {
        _solution = solution;
        
        Solved = solved;
        
        Steps = steps;
        
        ElapsedMicroseconds = elapsedMilliseconds;
        
        History = history;
        
        InitialCandidates = initialCandidates;
        
        Message = message;
    }
    
    public void DumpToConsole(int left = -1, int top = -1)
    {
        SetPosition(left, top, 0);
        
        Console.WriteLine("\u250c\u2500\u2500\u2500\u2500\u2500\u2500\u2500\u252c\u2500\u2500\u2500\u2500\u2500\u2500\u2500\u252c\u2500\u2500\u2500\u2500\u2500\u2500\u2500\u2510");

        var line = 1;
        
        for (var y = 0; y < 9; y++)
        {
            SetPosition(left, top, line++);
            
            Console.Write("\u2502");

            for (var x = 0; x < 9; x++)
            {
                if (_solution[x + y * 9] == 0)
                {
                    Console.Write("  ");
                }
                else
                {
                    Console.Write($" {_solution[x + y * 9]}");
                }

                if (x is 2 or 5)
                {
                    Console.Write(" \u2502");
                }
            }

            Console.WriteLine(" \u2502");
            
            if (y is 2 or 5)
            {
                SetPosition(left, top, line++);
            
                Console.WriteLine("\u251c\u2500\u2500\u2500\u2500\u2500\u2500\u2500\u253c\u2500\u2500\u2500\u2500\u2500\u2500\u2500\u253c\u2500\u2500\u2500\u2500\u2500\u2500\u2500\u2524");
            }
        }
        
        SetPosition(left, top, line);

        Console.WriteLine("\u2514\u2500\u2500\u2500\u2500\u2500\u2500\u2500\u2534\u2500\u2500\u2500\u2500\u2500\u2500\u2500\u2534\u2500\u2500\u2500\u2500\u2500\u2500\u2500\u2518");
    }

    private static void SetPosition(int left, int top, int y)
    {
        if (top > -1)
        {
            Console.CursorTop = top + y;
        }

        if (left > -1)
        {
            Console.CursorLeft = left;
        }
    }
}