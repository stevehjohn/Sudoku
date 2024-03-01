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

    private static readonly (byte Row, byte Column, byte Box)[] LookupTable = 
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

    public unsafe (int[] Solution, int Steps, double Microseconds, List<(int Position, int Value)> History) Solve(
        int[] sudoku, bool record = false)
    {
        _stepSolutions.Clear();

        _stack.Clear();

        _stack.Push((sudoku, record ? [] : null));

        var steps = 0;

        var stopwatch = Stopwatch.StartNew();

        fixed ((byte, byte, byte)* lookup = &LookupTable[0])
        {
            while (_stack.TryPop(out var item))
            {
                steps++;

                SolveStep(item.Puzzle, item.History, lookup);

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
    }

    private unsafe void SolveStep(int[] sudoku, List<(int Position, int Value)> history, (byte Row, byte Column, byte Box)* lookup)
    {
        for (var i = 0; i < 9; i++)
        {
            _rowCandidates[i] = 0b11_1111_1111;

            _columnCandidates[i] = 0b11_1111_1111;
            
            _boxCandidates[i] = 0b11_1111_1111;
        }

        for (var i = 0; i < 81; i++)
        {
            var pointer = lookup[i];

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

            var pointer = lookup[i];

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