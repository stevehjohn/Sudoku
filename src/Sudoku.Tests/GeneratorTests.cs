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

        var puzzle = generator.Generate();

        for (var y = 0; y < 9; y++)
        {
            _testOutputHelper.WriteLine(string.Join(' ', puzzle[(y * 9)..((y * 9) + 9)]));
        }
    }
}