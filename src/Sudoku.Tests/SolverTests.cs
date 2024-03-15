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
    public void VerifyLogToConsole()
    {
        var puzzles = File.ReadAllLines("Test Data/Puzzles With Answers.txt");

        var puzzle = new int[81];

        for (var i = 0; i < 81; i++)
        {
            puzzle[i] = puzzles[9_999][i] - '0';
        }

        var solver = new Solver(HistoryType.AllSteps, SolveMethod.FindFirst);

        var result = solver.Solve(puzzle);

        result.LogToConsole();
        
        _testOutputHelper.WriteLine($" Solved in {result.ElapsedMicroseconds:N0}μs, {result.Steps} steps taken.");
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
            
            // ReSharper disable once Xunit.XunitTestWithConsoleOutput
            Console.WriteLine($"\n {solution.Message}");
            
            solution.DumpToConsole(1);
        }

        stopwatch.Stop();

        _testOutputHelper.WriteLine($"{puzzles.Length:N0} puzzles verified in {stopwatch.Elapsed.TotalMilliseconds:N3}ms.");
    }

    [Fact]
    public void VerifySolverProducesCorrectResults()
    {
        VerifyAgainstFile("Test Data/Puzzles With Answers.txt", false);
    }

    [Fact]
    public void VerifyAgainstSudopediaTests()
    {
        VerifyAgainstFile("Test Data/Sudopedia Tests.txt", true);
    }

    private void VerifyAgainstFile(string filename, bool dump)
    {
        var puzzles = File.ReadAllLines(filename);

        var solver = new Solver(HistoryType.SolutionOnly, SolveMethod.FindUnique);

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
                Assert.False(result[i] != parts[1][i] - '0');
            }

            if (dump)
            {
                result.DumpToConsole(1);
            }
        }

        stopwatch.Stop();

        _testOutputHelper.WriteLine($"{puzzles.Length:N0} puzzles verified in {stopwatch.Elapsed.TotalMilliseconds:N3}ms.");
    }
}