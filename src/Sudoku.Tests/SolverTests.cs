﻿using System.Diagnostics;
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

    //[Fact]
    public void VerifySolverDetectsInvalidPuzzles()
    {
        var puzzles = File.ReadAllLines("Test Data/Invalid Puzzles.txt");

        var solver = new Solver();

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

            var solution = solver.Solve(input, HistoryType.None, true).Solution;

            if (solution != null)
            {
                // ReSharper disable once Xunit.XunitTestWithConsoleOutput
                Console.Write($"Puzzle {count} was solved.");
                
                solution.DumpToConsole(1);
            }
        }
        
        stopwatch.Stop();
        
        _testOutputHelper.WriteLine($"{puzzles.Length:N0} puzzles verified in {stopwatch.Elapsed.TotalMilliseconds:N3}ms.");
    }

    [Fact]
    public void VerifySolverProducesCorrectResults()
    {
        var puzzles = File.ReadAllLines("Test Data/Puzzles With Answers.txt");

        var solver = new Solver();

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
            
            for (var i = 0; i < 81; i++)
            {
                if (result.Solution[i] != parts[1][i] - '0')
                {
                    Assert.Fail();
                }
            }
        }
        
        stopwatch.Stop();
        
        _testOutputHelper.WriteLine($"{puzzles.Length:N0} puzzles verified in {stopwatch.Elapsed.TotalMilliseconds:N3}ms.");
    }
}