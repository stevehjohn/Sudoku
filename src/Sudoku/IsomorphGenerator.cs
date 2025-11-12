namespace Sudoku;

public static class IsomorphGenerator
{
    public static List<int[]> CreateIsomorphs(int count, int[] puzzle)
    {
        var isomorphs = new List<int[]>();
        
        while (count > 0)
        {
            var isomorph = new int[81];
            
            isomorphs.Add(isomorph);
            
            Array.Copy(puzzle, isomorph, 81);

            // TODO: Morphin'
            
            RelabelDigits(puzzle);
            
            count--;
        }

        return isomorphs;
    }

    private static void RelabelDigits(Span<int> puzzle)
    {
    }
}