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

    private readonly int[] _cellCandidates = new int[81];

    private readonly PriorityQueue<(int[] Puzzle, bool Solved, List<Move> History), int> _stepSolutions = new();

    private readonly Stack<(int[] Puzzle, List<Move> History)> _stack = [];
    
    public (int[] Solution, int Steps, double Microseconds, List<Move> History) Solve(int[] puzzle, bool record = false)
    {
        _stepSolutions.Clear();
        
        _stack.Clear();
        
        _stack.Push((puzzle, record ? [] : null));

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

                    return (solution.Puzzle, steps, stopwatch.Elapsed.TotalMicroseconds, solution.History);
                }

                _stack.Push((solution.Puzzle, solution.History));
            }
        }

        stopwatch.Stop();
        
        return (null, steps, stopwatch.Elapsed.TotalMicroseconds, null);
    }
    
    private void SolveStep(int[] puzzle, List<Move> history)
    {
        for (var y = 0; y < 9; y++)
        {
            _rowCandidates[y] = 0b11_1111_1111;

            _columnCandidates[y] = 0b11_1111_1111;

            var y9 = (y << 3) + y;

            for (var x = 0; x < 9; x++)
            {
                _rowCandidates[y] &= ~(1 << puzzle[x + y9]);

                _columnCandidates[y] &= ~(1 << puzzle[y + (x << 3) + x]);
            }
        }

        var boxIndex = 0;
        
        for (var yO = 0; yO < 81; yO += 27)
        {
            for (var xO = 0; xO < 9; xO += 3)
            {
                var start = xO + yO;

                _boxCandidates[boxIndex] = 0b11_1111_1111;

                for (var y = 0; y < 3; y++)
                {
                    var row = start + (y << 3) + y;

                    for (var x = 0; x < 3; x++)
                    {
                        _boxCandidates[boxIndex] &= ~(1 << puzzle[row + x]);
                    }
                }

                boxIndex++;
            }
        }

        for (var y = 0; y < 9; y++)
        {
            for (var x = 0; x < 9; x++)
            {
                if (puzzle[x + y * 9] == 0)
                {
                    _cellCandidates[x + y * 9] = _columnCandidates[x] & _rowCandidates[y] & _boxCandidates[y / 3 * 3 + x / 3];
                }
            }
        }

        // for (var y = 0; y < 9; y++)
        // {
        //     var oneMask = 0;
        //
        //     var twoMask = 0;
        //
        //     for (var x = 0; x < 9; x++)
        //     {
        //         twoMask |= oneMask & _cellCandidates[y * 9 + x];
        //
        //         oneMask |= _cellCandidates[y * 9 + x];
        //     }
        //
        //     var once = oneMask & ~twoMask;
        //
        //     if (once != 0)
        //     {
        //         for (var x = 0; x < 9; x++)
        //         {
        //             if ((_cellCandidates[y * 9 + x] & once) > 0)
        //             {
        //                 _cellCandidates[y * 9 + x] = once;
        //             }
        //         }
        //     }
        // }

        for (var yO = 0; yO < 81; yO += 27)
        {
            for (var xO = 0; xO < 9; xO += 3)
            {
                var oneMask = 0;

                var twoMask = 0;

                var start = yO + xO;

                for (var y = 0; y < 3; y++)
                {
                    for (var x = 0; x < 3; x++)
                    {
                        twoMask |= oneMask & _cellCandidates[start + y * 9 + x];

                        oneMask |= _cellCandidates[start + y * 9 + x];
                    }
                }

                var once = oneMask & ~twoMask;

                if (once != 0)
                {
                    for (var y = 0; y < 3; y++)
                    {
                        for (var x = 0; x < 3; x++)
                        {
                            if ((_cellCandidates[start + y * 9 + x] & once) > 0)
                            {
                                _cellCandidates[start + y * 9 + x] = once;
                            }
                        }
                    }
                }
            }
        }

        var position = (X: -1, Y: -1);

        var values = 0;

        var valueCount = 0b11_1111_1111;

        for (var y = 0; y < 9; y++)
        {
            for (var x = 0; x < 9; x++)
            {
                if (puzzle[x + y * 9] != 0)
                {
                    continue;
                }

                var candidates = _cellCandidates[x + y * 9];
                
                var count = BitOperations.PopCount((uint) candidates);

                if (count < valueCount)
                {
                    position = (x, y);

                    values = candidates;

                    valueCount = count;
                }
            }
        }

        _stepSolutions.Clear();
        
        for (var i = 1; i < 10; i++)
        {
            if ((values & (1 << i)) == 0)
            {
                continue;
            }

            var copy = _pool.Rent(81);

            var score = 80;

            for (var j = 0; j < 81; j++)
            {
                var value = puzzle[j];

                copy[j] = value;

                if (value != 0)
                {
                    score--;
                }
            }

            copy[position.X + (position.Y << 3) + position.Y] = i;

            List<Move> newHistory = null;

            if (history != null)
            {
                newHistory = [..history, new Move(position.X, position.Y, i)];
            }

            if (score == 0)
            {
                _stepSolutions.Clear();
                
                _stepSolutions.Enqueue((copy, true, newHistory), valueCount);
                
                return;
            }

            _stepSolutions.Enqueue((copy, false, newHistory), valueCount);
        }
    }
}