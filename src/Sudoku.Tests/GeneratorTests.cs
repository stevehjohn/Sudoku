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

        var puzzle = generator.Generate();

        puzzle.DumpToConsole(1);

        // This test is asserted by your eyes.
    }
    
    [Fact]
    public void GeneratesManyValidSudokuPuzzles()
    {
        for (var i = 0; i < 1000; i++)
        {
            var generator = new Generator();

            generator.Generate();
        }
    }
}