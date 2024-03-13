namespace Sudoku.Console;

public class TreeGenerator
{
    public void Generate(int[] puzzle, string filename)
    {
        var solver = new Solver(HistoryType.AllSteps, true);

        var result = solver.Solve(puzzle);
        
        GenerateNodes(result);
    }

    private void GenerateNodes(SudokuResult result)
    {
    }
}