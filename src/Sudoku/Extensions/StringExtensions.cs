namespace Sudoku.Extensions;

public static class StringExtensions
{
    extension(string puzzleString)
    {
        public int[] ParsePuzzle()
        {
            var puzzle = new int[81];
            
            for (var i = 0; i < 81; i++)
            {
                var character = puzzleString[i];
                
                if (character is '0' or '.')
                {
                    continue;
                }

                puzzle[i] = character - '0';

                if (puzzle[i] < 1 || puzzle[i] > 9)
                {
                    Console.WriteLine($"Invalid character: {character}");
                }
            }

            return puzzle;
        }
    }
}