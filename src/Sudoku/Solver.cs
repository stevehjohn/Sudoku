using System.Buffers;
using System.Diagnostics;
using System.Numerics;

namespace Sudoku.Solver;

public class Solver
{
    private readonly ArrayPool<int> _pool = ArrayPool<int>.Shared;
    
    private readonly int[] _rowCandidates = new int[9];
    
    private readonly int[] _columnCandidates = new int[9];
    
    private readonly int[] _boxCandidates = new int[9];

    private readonly PriorityQueue<(int[] Sudoku, bool Solved, List<(int Position, int Value)> History), int> _stepSolutions = new();

    private readonly Stack<(int[] Puzzle, List<(int Position, int Value)> History)> _stack = [];

    public (int[] Solution, int Steps, double Microseconds, List<(int Position, int Value)> History) Solve(int[] sudoku, bool record = false)
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

    private (int Row, int Column, int Box) Lookup(int index)
    {
        var row = index / 9;

        var column = index % 9;

        return (row, column, row / 3 * 3 + column / 3);
    }

    private void SolveStep(int[] sudoku, List<(int Position, int Value)> history)
    {
        for (var i = 0; i < 9; i++)
        {
            _rowCandidates[i] = 0b11_1111_1111;

            _columnCandidates[i] = 0b11_1111_1111;
            
            _boxCandidates[i] = 0b11_1111_1111;
        }

        for (var i = 0; i < 81; i++)
        {
            var pointer = Lookup(i);

            var value = ~(1 << sudoku[i]);
            
            _rowCandidates[pointer.Row] &= value;
            
            _columnCandidates[pointer.Column] &= value;
            
            _boxCandidates[pointer.Box] &= value;
        }

        var values = 0;

        var valueCount = 0b11_1111_1111;

        var index = 0;
        
        for (var i = 0; i < 81; i++)
        {
            if (sudoku[i] != 0)
            {
                continue;
            }

            var pointer = Lookup(i);

            var row = _rowCandidates[pointer.Row];

            if (row == 1)
            {
                continue;
            }

            var candidates = row & _columnCandidates[pointer.Column] & _boxCandidates[pointer.Box];

            if (candidates == 1)
            {
                continue;
            }

            var count = BitOperations.PopCount((uint) candidates);

            if (count < valueCount)
            {
                values = candidates;

                valueCount = count;

                index = i;
            }
        }

        for (var i = 1; i < 10; i++)
        {
            if ((values & (1 << i)) == 0)
            {
                continue;
            }

            sudoku[index] = i;

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

            List<(int Position, int Value)> newHistory = null;

            if (history != null)
            {
                newHistory = [..history, (index, i)];
            }

            _stepSolutions.Enqueue((copy, score == 0, newHistory), score);
        }
    }
}