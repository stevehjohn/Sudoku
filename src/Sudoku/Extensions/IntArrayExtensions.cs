namespace Sudoku.Extensions;

public static class IntArrayExtensions
{
    public static void DumpToConsole(this int[] array, int left = -1, int top = -1)
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
                if (array[x + y * 9] == 0)
                {
                    Console.Write("  ");
                }
                else
                {
                    Console.Write($" {array[x + y * 9]}");
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