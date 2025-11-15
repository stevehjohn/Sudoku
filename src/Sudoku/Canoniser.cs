namespace Sudoku;

public static class Canoniser
{
    public static int[] CanonisePuzzle(int[] puzzle)
    {
        var workingCopy = new int[81];
        
        Array.Copy(puzzle, workingCopy, 81);

        NormaliseDigits(puzzle);
        
        return workingCopy;
    }

    private static void NormaliseDigits(Span<int> puzzle)
    {
        var mappings = new int[10];

        var digit = 1;
        
        for (var i = 0; i < 81; i++)
        {
            var cellValue = puzzle[i];

            if (mappings[cellValue] == 0)
            {
                mappings[cellValue] = digit++;
            }
        }

        for (var i = 0; i < 81; i++)
        {
            var cellValue = puzzle[i];

            if (cellValue != 0)
            {
                puzzle[i] = mappings[cellValue];
            }
        }
    }
}