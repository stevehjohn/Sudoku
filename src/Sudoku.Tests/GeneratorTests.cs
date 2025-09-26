using Sudoku.Extensions;
using Xunit;

namespace Sudoku.Tests;

[Collection("Non-parallel")]
public class GeneratorTests
{
    [Fact]
    public void GeneratesAValidSudokuPuzzle()
    {
        var generator = new Generator();

        var puzzle = generator.Generate(25);

        puzzle.DumpToConsole(1);

        var solver = new Solver(HistoryType.None, SolveMethod.CountAll);

        var result = solver.Solve(puzzle);
        
        Assert.Equal(1, result.SolutionCount);
        
        Assert.True(result.Solved);
    }
}