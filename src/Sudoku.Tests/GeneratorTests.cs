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

        var puzzle = generator.Generate(25, CancellationToken.None).Puzzle;

        puzzle.DumpToConsole(1);

        var solver = new Solver(HistoryType.None, SolveMethod.CountAll);

        var result = solver.Solve(puzzle);

        Assert.Equal(1, result.SolutionCount);

        Assert.True(result.Solved);
    }

    [Fact]
    public void GeneratesSamePuzzleWithSameSeed()
    {
        var generator = new Generator(1234);

        var puzzle1 = generator.Generate(25).Puzzle;

        puzzle1.DumpToConsole(1);

        generator = new Generator(1234);

        var puzzle2 = generator.Generate(25).Puzzle;

        puzzle2.DumpToConsole(1);

        Assert.Equal(puzzle1, puzzle2);
    }
}