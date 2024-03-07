using Sudoku.Extensions;
using Xunit;

namespace Sudoku.Tests;

public class GeneratorTests
{
    [Fact]
    public void GeneratesAValidSudokuPuzzle()
    {
        var generator = new Generator();

        var puzzle = generator.Generate(60);

        puzzle.DumpToConsole(1);

        // This test is asserted by your eyes.
    }
}