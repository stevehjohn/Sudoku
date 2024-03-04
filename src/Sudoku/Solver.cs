using System.Diagnostics;
using System.Numerics;

namespace Sudoku.Solver;

public class Solver
{
    private readonly int[] _rowCandidates = new int[9];
    
    private readonly int[] _columnCandidates = new int[9];
    
    private readonly int[] _boxCandidates = new int[9];

    private readonly int[] _cellCandidates = new int[81];

    private readonly int[] _frequencies = new int[10];

    public (int[] Solution, int Steps, int MaxStackSize, double Microseconds, List<Move> History) Solve(int[] puzzle, bool record = false)
    {
        var steps = 0;

        var maxStackSize = 0;

        var stopwatch = Stopwatch.StartNew();

        var score = 81;
        
        for (var i = 0; i < 81; i++)
        {
            if (puzzle[i] != 0)
            {
                score--;
            }
        }

        var history = record ? new List<Move>() : null;

        var span = new Span<int>(puzzle);
        
        SolveStep(span, score, history);
        
        stopwatch.Stop();
        
        return (puzzle, steps, maxStackSize, stopwatch.Elapsed.TotalMicroseconds, history);
    }
    
    private void SolveStep(Span<int> puzzle, int score, List<Move> history)
    {
        GetCellCandidates(puzzle);

        FindHiddenSingles();

        var move = FindLowestMove(puzzle);

        CreateNextSteps(puzzle, move, score, history);
    }

    private void GetCellCandidates(Span<int> puzzle)
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
            var oneMaskRow = 0;
        
            var twoMaskRow = 0;
        
            var oneMaskColumn = 0;
        
            var twoMaskColumn = 0;
        
            for (var x = 0; x < 9; x++)
            {
                twoMaskRow |= oneMaskRow & _cellCandidates[(y << 3) + y + x];
        
                oneMaskRow |= _cellCandidates[(y << 3) + y + x];

                twoMaskColumn |= oneMaskColumn & _cellCandidates[(x << 3) + x + y];
        
                oneMaskColumn |= _cellCandidates[(x << 3) + x + y];
            }
        
            var onceRow = oneMaskRow & ~twoMaskRow;
        
            var onceColumn = oneMaskColumn & ~twoMaskColumn;
        
            if (BitOperations.PopCount((uint) onceRow) == 1)
            {
                for (var x = 0; x < 9; x++)
                {
                    if ((_cellCandidates[(y << 3) + y + x] & onceRow) > 0)
                    {
                        _cellCandidates[(y << 3) + y + x] = onceRow;

                        return;
                    }
                }
            }
        
            if (BitOperations.PopCount((uint) onceColumn) == 1)
            {
                for (var x = 0; x < 9; x++)
                {
                    if ((_cellCandidates[(x << 3) + x + y] & onceColumn) > 0)
                    {
                        _cellCandidates[(x << 3) + x + y] = onceColumn;

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

    private ((int X, int Y) Position, int Values, int ValueCount) FindLowestMove(Span<int> puzzle)
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

    private void CreateNextSteps(Span<int> puzzle, ((int X, int Y) Position, int Values, int ValueCount) move, int score, List<Move> history)
    {
        for (var i = 1; i < 10; i++)
        {
            if ((move.Values & (1 << i)) == 0)
            {
                continue;
            }

            if (puzzle[move.Position.X + (move.Position.Y << 3) + move.Position.Y] != 0)
            {
                return;
            }

            puzzle[move.Position.X + (move.Position.Y << 3) + move.Position.Y] = i;

            score--;
            
            if (history != null)
            {
                history.Add(new Move(move.Position.X, move.Position.Y, i));
            }

            if (score == 0)
            {
                return;
            }
            
            SolveStep(puzzle, score, history);

            puzzle[move.Position.X + (move.Position.Y << 3) + move.Position.Y] = 0;

            if (history != null)
            {
                history.RemoveAt(history.Count - 1);
            }

            score++;
        }
    }
}