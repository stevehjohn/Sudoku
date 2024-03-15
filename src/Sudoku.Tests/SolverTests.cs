using System.Diagnostics;
using Xunit;
using Xunit.Abstractions;

namespace Sudoku.Tests;

[Collection("Non-parallel")]
public class SolverTests
{
    private readonly ITestOutputHelper _testOutputHelper;

    public SolverTests(ITestOutputHelper testOutputHelper)
    {
        _testOutputHelper = testOutputHelper;
    }

    [Fact]
    public void VerifySolverDetectsInvalidPuzzles()
    {
        var puzzles = File.ReadAllLines("Test Data/Invalid Puzzles.txt");

        var solver = new Solver(HistoryType.None, SolveMethod.CountAll);

        var stopwatch = Stopwatch.StartNew();

        var count = 0;

        foreach (var puzzle in puzzles)
        {
            count++;

            var parts = puzzle.Split(',');

            var input = new int[81];

            for (var i = 0; i < 81; i++)
            {
                input[i] = parts[0][i] - '0';
            }

            var solution = solver.Solve(input);

            _testOutputHelper.WriteLine($"Puzzle {count}: {solution.Message}");

            Assert.Contains(parts[2], solution.Message.ToLower());

            if (parts[1] != "0")
            {
                Assert.Contains(parts[1], solution.Message.ToLower());
            }
        }

        stopwatch.Stop();

        _testOutputHelper.WriteLine($"{puzzles.Length:N0} puzzles verified in {stopwatch.Elapsed.TotalMilliseconds:N3}ms.");
    }

    [Fact]
    public void VerifySolverProducesCorrectResults()
    {
        VerifyAgainstFile("Test Data/Puzzles With Answers.txt");
    }

    [Fact]
    public void VerifyAgainstSudopediaTests()
    {
        VerifyAgainstFile("Test Data/Sudopedia Tests.txt");
    }

    private void VerifyAgainstFile(string filename)
    {
        var puzzles = File.ReadAllLines(filename);

        var solver = new Solver(HistoryType.AllSteps, SolveMethod.FindUnique);

        var stopwatch = Stopwatch.StartNew();

        foreach (var puzzle in puzzles)
        {
            var parts = puzzle.Split(',');

            var input = new int[81];

            for (var i = 0; i < 81; i++)
            {
                input[i] = parts[0][i] - '0';
            }

            var result = solver.Solve(input);

            Assert.True(result.Solved);

            for (var i = 0; i < 81; i++)
            {
                if (result[i] != parts[1][i] - '0')
                {
                    Assert.Fail();
                }
            }
        }

        stopwatch.Stop();

        _testOutputHelper.WriteLine($"{puzzles.Length:N0} puzzles verified in {stopwatch.Elapsed.TotalMilliseconds:N3}ms.");
    }
}