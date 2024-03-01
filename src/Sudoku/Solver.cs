﻿using System.Buffers;
using System.Diagnostics;
using System.Numerics;

namespace Sudoku.Solver;

public class Solver
{
    private readonly ArrayPool<int> _pool = ArrayPool<int>.Shared;
    
    private readonly int[] _rowCandidates = new int[9];
    
    private readonly int[] _columnCandidates = new int[9];
    
    private readonly int[] _boxCandidates = new int[9];

    private readonly PriorityQueue<(int[] Sudoku, bool Solved, List<Move> History), int> _stepSolutions = new();

    private readonly Stack<(int[] Puzzle, List<Move> History)> _stack = [];

    private static readonly (int Row, int Column, int Box)[] Lookup = 
    [
        (0, 0, 0),
        (0, 1, 0),
        (0, 2, 0),
        (0, 3, 1),
        (0, 4, 1),
        (0, 5, 1),
        (0, 6, 2),
        (0, 7, 2),
        (0, 8, 2),

        (1, 0, 0),
        (1, 1, 0),
        (1, 2, 0),
        (1, 3, 1),
        (1, 4, 1),
        (1, 5, 1),
        (1, 6, 2),
        (1, 7, 2),
        (1, 8, 2),

        (2, 0, 0),
        (2, 1, 0),
        (2, 2, 0),
        (2, 3, 1),
        (2, 4, 1),
        (2, 5, 1),
        (2, 6, 2),
        (2, 7, 2),
        (2, 8, 2),

        (3, 0, 3),
        (3, 1, 3),
        (3, 2, 3),
        (3, 3, 4),
        (3, 4, 4),
        (3, 5, 4),
        (3, 6, 5),
        (3, 7, 5),
        (3, 8, 5),

        (4, 0, 3),
        (4, 1, 3),
        (4, 2, 3),
        (4, 3, 4),
        (4, 4, 4),
        (4, 5, 4),
        (4, 6, 5),
        (4, 7, 5),
        (4, 8, 5),

        (5, 0, 3),
        (5, 1, 3),
        (5, 2, 3),
        (5, 3, 4),
        (5, 4, 4),
        (5, 5, 4),
        (5, 6, 5),
        (5, 7, 5),
        (5, 8, 5),

        (6, 0, 6),
        (6, 1, 6),
        (6, 2, 6),
        (6, 3, 7),
        (6, 4, 7),
        (6, 5, 7),
        (6, 6, 8),
        (6, 7, 8),
        (6, 8, 8),

        (7, 0, 6),
        (7, 1, 6),
        (7, 2, 6),
        (7, 3, 7),
        (7, 4, 7),
        (7, 5, 7),
        (7, 6, 8),
        (7, 7, 8),
        (7, 8, 8),

        (8, 0, 6),
        (8, 1, 6),
        (8, 2, 6),
        (8, 3, 7),
        (8, 4, 7),
        (8, 5, 7),
        (8, 6, 8),
        (8, 7, 8),
        (8, 8, 8)
    ];
    
    public (int[] Solution, int Steps, double Microseconds, List<Move> History) Solve(int[] sudoku, bool record = false)
    {
        _stepSolutions.Clear();
        
        _stack.Clear();
        
        _stack.Push((sudoku, record ? [] : null));

        var steps = 0;

        var stopwatch = Stopwatch.StartNew();
        
        while (_stack.TryPop(out var item))
        {
            steps++;

            SolveStep(item.Puzzle, item.History);

            if (steps > 1)
            {
                _pool.Return(item.Puzzle);
            }

            while (_stepSolutions.TryDequeue(out var solution, out _))
            {
                if (solution.Solved)
                {
                    stopwatch.Stop();

                    while (_stack.TryPop(out item))
                    {
                        _pool.Return(item.Puzzle);
                    }

                    return (solution.Sudoku, steps, stopwatch.Elapsed.TotalMicroseconds, solution.History);
                }

                _stack.Push((solution.Sudoku, solution.History));
            }
        }

        stopwatch.Stop();
        
        return (null, steps, stopwatch.Elapsed.TotalMicroseconds, null);
    }
    
    private void SolveStep(int[] sudoku, List<Move> history)
    {
        for (var i = 0; i < 9; i++)
        {
            _rowCandidates[i] = 0b11_1111_1111;

            _columnCandidates[i] = 0b11_1111_1111;
            
            _boxCandidates[i] = 0b11_1111_1111;
        }

        for (var i = 0; i < 81; i++)
        {
            var lookup = Lookup[i];

            var value = ~(1 << sudoku[i]);
            
            _rowCandidates[lookup.Row] &= value;
            
            _columnCandidates[lookup.Column] &= value;
            
            _boxCandidates[lookup.Box] &= value;
        }

        var position = (X: -1, Y: -1);

        var values = 0;

        var valueCount = 0b11_1111_1111;

        for (var y = 0; y < 9; y++)
        {
            var row = _rowCandidates[y];

            if (row == 1)
            {
                continue;
            }

            var y9 = y * 9;

            for (var x = 0; x < 9; x++)
            {
                if (sudoku[x + y9] != 0)
                {
                    continue;
                }

                var column = _columnCandidates[x];

                var box = _boxCandidates[y / 3 * 3 + x / 3];

                var common = row & column & box;

                if (common == 1)
                {
                    continue;
                }

                var count = BitOperations.PopCount((uint) common);

                if (count < valueCount)
                {
                    position = (x, y);

                    values = common;

                    valueCount = count;
                }
            }
        }

        for (var i = 1; i < 10; i++)
        {
            if ((values & (1 << i)) == 0)
            {
                continue;
            }

            sudoku[position.X + position.Y * 9] = i;

            var copy = _pool.Rent(81);

            var score = 81;

            for (var j = 0; j < 81; j++)
            {
                var value = sudoku[j];

                copy[j] = value;

                if (value != 0)
                {
                    score--;
                }
            }

            List<Move> newHistory = null;

            if (history != null)
            {
                newHistory = [..history, new Move(position.X, position.Y, i)];
            }

            _stepSolutions.Enqueue((copy, score == 0, newHistory), score);
        }
    }
}