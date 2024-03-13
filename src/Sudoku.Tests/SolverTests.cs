using System.Diagnostics;
using Sudoku.Extensions;
using Xunit;
using Xunit.Abstractions;

namespace Sudoku.Tests;

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

        var solver = new Solver(HistoryType.None, true);

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

            Assert.False(solution.Solved);
            
            if (solution.Solved)
            {
                solution.DumpToConsole(1);
                
                Assert.Fail($"Puzzle {count} was solved.");
            }

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
        var puzzles = File.ReadAllLines("Test Data/Puzzles With Answers.txt");

        var solver = new Solver(HistoryType.None, true);

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