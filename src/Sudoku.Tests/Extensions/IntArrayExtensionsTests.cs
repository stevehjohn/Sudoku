using Sudoku.Extensions;
using Xunit;

namespace Sudoku.Tests.Extensions;

public class IntArrayExtensionsTests
{
    // [Fact]
    public void CanShowPuzzleAndSolutionSideBySide()
    {
        var generator = new Generator();

        var puzzle = generator.Generate(60);

        var y = Console.CursorTop;

        var solver = new Solver();

        var result = solver.Solve(puzzle);
        
        puzzle.DumpToConsole(1, y);
        
        result.Solution.DumpToConsole(30, y);
        
        // This test is asserted by your eyes.
    }
}