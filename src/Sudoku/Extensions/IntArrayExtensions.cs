namespace Sudoku.Extensions;

public static class IntArrayExtensions
{
    public static void DumpToConsole(this int[] array, (int X, int Y) position)
    {
        DumpToConsoleInternal(array, position);
    }

    public static void DumpToConsole(this int[] array)
    {
        DumpToConsoleInternal(array, null);
    }

    private static void DumpToConsoleInternal(int[] array, (int X, int Y)? position)
    {
        SetPosition(position, 0);
        
        Console.Write("\u250c\u2500\u2500\u2500\u2500\u2500\u2500\u2500\u252c\u2500\u2500\u2500\u2500\u2500\u2500\u2500\u252c\u2500\u2500\u2500\u2500\u2500\u2500\u2500\u2510");

        var line = 1;
        
        for (var y = 0; y < 9; y++)
        {
            SetPosition(position, line++);
            
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

            Console.Write(" \u2502");
            
            if (y is 2 or 5)
            {
                SetPosition(position, line++);
            
                Console.Write("\u251c\u2500\u2500\u2500\u2500\u2500\u2500\u2500\u253c\u2500\u2500\u2500\u2500\u2500\u2500\u2500\u253c\u2500\u2500\u2500\u2500\u2500\u2500\u2500\u2524");
            }
        }
        
        SetPosition(position, line);

        Console.Write("\u2514\u2500\u2500\u2500\u2500\u2500\u2500\u2500\u2534\u2500\u2500\u2500\u2500\u2500\u2500\u2500\u2534\u2500\u2500\u2500\u2500\u2500\u2500\u2500\u2518");
    }

    private static void SetPosition((int X, int Y)? position, int y)
    {
        if (position == null)
        {
            return;
        }

        Console.CursorTop = position.Value.Y + y;

        Console.CursorLeft = position.Value.X;
    }
}