using System.Text;

namespace Sudoku.Extensions;

public static class IntArrayExtensions
{
    public static void DumpToConsole(this int[] array, int left = -1, int top = -1)
    {
        SetPosition(left, top, 0);
        
        Console.WriteLine("┌───────┬───────┬───────┐");

        var line = 1;
        
        for (var y = 0; y < 9; y++)
        {
            SetPosition(left, top, line++);
            
            Console.Write("│");

            for (var x = 0; x < 9; x++)
            {
                Console.Write(array[x + y * 9] == 0 ? "  " : $" {array[x + y * 9]}");

                if (x is 2 or 5)
                {
                    Console.Write(" │");
                }
            }

            Console.WriteLine(" │");

            if (y is not (2 or 5))
            {
                continue;
            }
            
            SetPosition(left, top, line++);
            
            Console.WriteLine("├───────┼───────┼───────┤");
        }
        
        SetPosition(left, top, line);

        Console.WriteLine("└───────┴───────┴───────┘");
    }

    public static string FlattenPuzzle(this int[] puzzle)
    {
        var builder = new StringBuilder();

        for (var i = 0; i < puzzle.Length; i++)
        {
            builder.Append(puzzle[i] == 0 ? '.' : (char) ('0' + puzzle[i]));
        }

        return builder.ToString();
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