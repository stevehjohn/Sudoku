using System.Buffers;
using System.Diagnostics;
using System.Numerics;

namespace Sudoku.Solver;

public class Solver
{
    private readonly ArrayPool<int> _pool = ArrayPool<int>.Shared;
    
    public (int[] Solution, int Steps, double Microseconds, List<Move> History) Solve(int[] sudoku, bool record = false)
    {
        var stack = new Stack<(int[] Puzzle, List<Move> History)>();

        stack.Push((sudoku, record ? [] : null));

        var steps = 0;

        var stopwatch = Stopwatch.StartNew();
        
        while (stack.TryPop(out var item))
        {
            steps++;

            var solutions = SolveStep(item.Puzzle, item.History);

            if (steps > 1)
            {
                _pool.Return(item.Puzzle);
            }

            foreach (var solution in solutions)
            {
                if (solution.Solved)
                {
                    stopwatch.Stop();
                    
                    while (stack.TryPop(out item))
                    {
                        _pool.Return(item.Puzzle);
                    }

                    return (solution.Sudoku, steps, stopwatch.Elapsed.TotalMicroseconds, solution.History);
                }

                stack.Push((solution.Sudoku, solution.History));
            }
        }

        stopwatch.Stop();
        
        return (null, steps, stopwatch.Elapsed.TotalMicroseconds, null);
    }

    private List<(int[] Sudoku, bool Solved, List<Move> History)> SolveStep(int[] sudoku, List<Move> history)
    {
        var rowCandidates = _pool.Rent(9);

        var columnCandidates = _pool.Rent(9);

        for (var y = 0; y < 9; y++)
        {
            rowCandidates[y] = 0b11_1111_1111;

            columnCandidates[y] = 0b11_1111_1111;

            var y9 = y * 9;

            for (var x = 0; x < 9; x++)
            {
                rowCandidates[y] &= ~(1 << sudoku[x + y9]);

                columnCandidates[y] &= ~(1 << sudoku[y + x * 9]);
            }
        }

        var boxCandidates = _pool.Rent(9);

        for (var y = 0; y < 9; y += 3)
        {
            for (var x = 0; x < 3; x++)
            {
                boxCandidates[y + x] = 0b11_1111_1111;

                var x3 = x * 3;

                for (var y1 = 0; y1 < 3; y1++)
                {
                    var yy1 = y + y1;

                    for (var x1 = 0; x1 < 3; x1++)
                    {
                        boxCandidates[y + x] &= ~(1 << sudoku[x3 + x1 + yy1 * 9]);
                    }
                }
            }
        }

        var position = (X: -1, Y: -1);

        var values = 0;

        var valueCount = 0b11_1111_1111;

        for (var y = 0; y < 9; y++)
        {
            var row = rowCandidates[y];

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

                var column = columnCandidates[x];

                var box = boxCandidates[y / 3 * 3 + x / 3];

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

        var solutions = new List<(int[] Sudoku, bool Solved, List<Move> History)>();

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

            if (score == 0)
            {
                solutions.Clear();

                solutions.Add((copy, true, newHistory));

                break;
            }

            solutions.Add((copy, false, newHistory));
        }
        
        _pool.Return(rowCandidates);
        
        _pool.Return(columnCandidates);
        
        _pool.Return(boxCandidates);

        return solutions;
    }
}