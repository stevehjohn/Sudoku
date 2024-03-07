using Sudoku.Extensions;
using Xunit;
using Xunit.Abstractions;

namespace Sudoku.Tests;

public class GeneratorTests
{
    private readonly ITestOutputHelper _testOutputHelper;

    public GeneratorTests(ITestOutputHelper testOutputHelper)
    {
        _testOutputHelper = testOutputHelper;
    }

    [Fact]
    public void GeneratesAValidSudokuPuzzle()
    {
        var generator = new Generator();

        var puzzle = generator.Generate(60);

        puzzle.DumpToConsole(1);

        // This test is asserted by your eyes.
    }
}