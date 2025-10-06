using Sudoku.Extensions;
using Xunit;

namespace Sudoku.Tests.Extensions;

[Collection("Non-parallel")]
public class IntArrayExtensionsTests
{
    [Fact]
    public void CanShowPuzzleAndSolutionSideBySide()
    {
        var generator = new Generator();

        var puzzle = generator.Generate(30, CancellationToken.None).Puzzle;

        var y = Console.CursorTop;

        var solver = new Solver(HistoryType.None, SolveMethod.FindFirst);

        var result = solver.Solve(puzzle);
        
        puzzle.DumpToConsole(1, y);
        
        result.DumpToConsole(30, y);
        
        // This test is asserted by your eyes.
    }
}