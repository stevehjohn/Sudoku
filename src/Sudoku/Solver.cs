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

    private readonly int[] _cellCandidates = new int[81];

    private readonly int[] _frequencies = new int[10];

    private readonly PriorityQueue<(int[] Puzzle, bool Solved, List<Move> History), int> _stepSolutions = new();

    private readonly Stack<(int[] Puzzle, List<Move> History)> _stack = [];
    
    public (int[] Solution, int Steps, int MaxStackSize, double Microseconds, List<Move> History) Solve(int[] puzzle, bool record = false)
    {
        _stepSolutions.Clear();
        
        _stack.Clear();
        
        _stack.Push((puzzle, record ? [] : null));

        var steps = 0;

        var maxStackSize = 0;

        var stopwatch = Stopwatch.StartNew();
        
        while (_stack.TryPop(out var item))
        {
            maxStackSize = Math.Max(maxStackSize, _stack.Count);
            
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

                    return (solution.Puzzle, steps, maxStackSize, stopwatch.Elapsed.TotalMicroseconds, solution.History);
                }

                _stack.Push((solution.Puzzle, solution.History));
            }
        }

        stopwatch.Stop();
        
        return (null, steps, maxStackSize, stopwatch.Elapsed.TotalMicroseconds, null);
    }
    
    private void SolveStep(int[] puzzle, List<Move> history)
    {
        GetCellCandidates(puzzle);

        FindHiddenSingles();

        var move = FindLowestMove(puzzle);

        CreateNextSteps(puzzle, move, history);
    }

    private void GetCellCandidates(int[] puzzle)
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

                _frequencies[puzzle[x + y9]]++;
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
                if (puzzle[x + (y << 3) + y] == 0)
                {
                    _cellCandidates[x + (y << 3) + y] = _columnCandidates[x] & _rowCandidates[y] & _boxCandidates[y / 3 * 3 + x / 3];
                }
                else
                {
                    _cellCandidates[x + (y << 3) + y] = 0;
                }
            }
        }
    }

    private void FindHiddenSingles()
    {
        for (var y = 0; y < 9; y++)
        {
            var oneMask = 0;
        
            var twoMask = 0;
        
            for (var x = 0; x < 9; x++)
            {
                twoMask |= oneMask & _cellCandidates[(y << 3) + y + x];
        
                oneMask |= _cellCandidates[(y << 3) + y + x];
            }
        
            var once = oneMask & ~twoMask;
        
            if (BitOperations.PopCount((uint) once) == 1)
            {
                for (var x = 0; x < 9; x++)
                {
                    if ((_cellCandidates[(y << 3) + y + x] & once) > 0)
                    {
                        _cellCandidates[(y << 3) + y + x] = once;

                        return;
                    }
                }
            }
        }

        for (var x = 0; x < 9; x++)
        {
            var oneMask = 0;
    
            var twoMask = 0;
    
            for (var y = 0; y < 9; y++)
            {
                twoMask |= oneMask & _cellCandidates[(y << 3) + y + x];
    
                oneMask |= _cellCandidates[(y << 3) + y + x];
            }
    
            var once = oneMask & ~twoMask;
    
            if (BitOperations.PopCount((uint) once) == 1)
            {
                for (var y = 0; y < 9; y++)
                {
                    if ((_cellCandidates[(y << 3) + y + x] & once) > 0)
                    {
                        _cellCandidates[(y << 3) + y + x] = once;
    
                        return;
                    }
                }
            }
        }

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
                        twoMask |= oneMask & _cellCandidates[start + (y << 3) + y + x];

                        oneMask |= _cellCandidates[start + (y << 3) + y + x];
                    }
                }

                var once = oneMask & ~twoMask;

                if (BitOperations.PopCount((uint) once) == 1)
                {
                    for (var y = 0; y < 3; y++)
                    {
                        for (var x = 0; x < 3; x++)
                        {
                            if ((_cellCandidates[start + (y << 3) + y + x] & once) > 0)
                            {
                                _cellCandidates[start + (y << 3) + y + x] = once;
                            }
                        }
                    }
                }
            }
        }
    }

    private ((int X, int Y) Position, int Values, int ValueCount) FindLowestMove(int[] puzzle)
    {
        var position = (X: -1, Y: -1);

        var values = 0;

        var valueCount = 0b11_1111_1111;

        for (var y = 0; y < 9; y++)
        {
            for (var x = 0; x < 9; x++)
            {
                if (puzzle[x + (y << 3) + y] != 0)
                {
                    continue;
                }

                var candidates = _cellCandidates[x + (y << 3) + y];
                
                var count = BitOperations.PopCount((uint) candidates);

                if (count < valueCount)
                {
                    position = (x, y);

                    values = candidates;

                    valueCount = count;
                }
            }
        }

        return (position, values, valueCount);
    }

    private void CreateNextSteps(int[] puzzle, ((int X, int Y) Position, int Values, int ValueCount) move, List<Move> history)
    {
        _stepSolutions.Clear();
        
        for (var i = 1; i < 10; i++)
        {
            if ((move.Values & (1 << i)) == 0)
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

            copy[move.Position.X + (move.Position.Y << 3) + move.Position.Y] = i;

            List<Move> newHistory = null;

            if (history != null)
            {
                newHistory = [..history, new Move(move.Position.X, move.Position.Y, i)];
            }

            if (score == 0)
            {
                _stepSolutions.Enqueue((copy, true, newHistory), 0);
                
                return;
            }

            _stepSolutions.Enqueue((copy, false, newHistory), score * 100 + _frequencies[i] + 1);
        }
    }
}