using System.Diagnostics;
using System.Numerics;

namespace Sudoku.Solver;

public class Solver
{
    private readonly int[] _cellCandidates = new int[81];

    public (int[] Solution, int Steps, double Microseconds, List<Move> History) Solve(int[] puzzle, bool record = false)
    {
        var steps = 0;

        var stopwatch = Stopwatch.StartNew();

        var workingCopy = new int[81];

        var score = 81;

        for (var i = 0; i < 81; i++)
        {
            if (puzzle[i] != 0)
            {
                score--;

                workingCopy[i] = puzzle[i];
            }
        }

        var history = record ? new List<Move>() : null;

        var span = new Span<int>(workingCopy);

        SolveStep(span, score, ref steps, history);

        stopwatch.Stop();

        return (workingCopy, steps, stopwatch.Elapsed.TotalMicroseconds, history);
    }

    private bool SolveStep(Span<int> puzzle, int score, ref int steps, List<Move> history)
    {
        var candidates = GetSectionCandidates(puzzle);

        GetCellCandidates(puzzle, candidates);
        
        //if (! FindHiddenSingles())
        {
            //FindNakedPairs();
        }

        var move = FindLowestMove(puzzle);

        return CreateNextSteps(puzzle, move, score, ref steps, history);
    }

    private static (Candidates Row, Candidates Column, Candidates Box) GetSectionCandidates(Span<int> puzzle)
    {
        var rowCandidates = new Candidates();
        
        var columnCandidates = new Candidates();
        
        var boxCandidates = new Candidates();
        
        for (var y = 0; y < 9; y++)
        {
            var y9 = (y << 3) + y;

            for (var x = 0; x < 9; x++)
            {
                rowCandidates.Remove(y, puzzle[x + y9]);

                columnCandidates.Remove(y, puzzle[y + (x << 3) + x]);
            }
        }

        var boxIndex = 0;

        for (var yO = 0; yO < 81; yO += 27)
        {
            for (var xO = 0; xO < 9; xO += 3)
            {
                var start = xO + yO;

                for (var y = 0; y < 3; y++)
                {
                    var row = start + (y << 3) + y;

                    for (var x = 0; x < 3; x++)
                    {
                        boxCandidates.Remove(boxIndex, puzzle[row + x]);
                    }
                }

                boxIndex++;
            }
        }

        return (rowCandidates, columnCandidates, boxCandidates);
    }

    private void GetCellCandidates(Span<int> puzzle, (Candidates Row, Candidates Column, Candidates Box) candidates)
    {
        for (var y = 0; y < 9; y++)
        {
            for (var x = 0; x < 9; x++)
            {
                if (puzzle[x + (y << 3) + y] == 0)
                {
                    _cellCandidates[x + (y << 3) + y] = candidates.Row[x] & candidates.Column[y] & candidates.Box[y / 3 * 3 + x / 3];
                }
                else
                {
                    _cellCandidates[x + (y << 3) + y] = 0;
                }
            }
        }
    }

    // private bool FindHiddenSingles()
    // {
    //     for (var y = 0; y < 9; y++)
    //     {
    //         var oneMaskRow = 0;
    //
    //         var twoMaskRow = 0;
    //
    //         var oneMaskColumn = 0;
    //
    //         var twoMaskColumn = 0;
    //
    //         for (var x = 0; x < 9; x++)
    //         {
    //             twoMaskRow |= oneMaskRow & _cellCandidates[(y << 3) + y + x];
    //
    //             oneMaskRow |= _cellCandidates[(y << 3) + y + x];
    //
    //             twoMaskColumn |= oneMaskColumn & _cellCandidates[(x << 3) + x + y];
    //
    //             oneMaskColumn |= _cellCandidates[(x << 3) + x + y];
    //         }
    //
    //         var onceRow = oneMaskRow & ~twoMaskRow;
    //
    //         var onceColumn = oneMaskColumn & ~twoMaskColumn;
    //
    //         if (BitOperations.PopCount((uint) onceRow) == 1)
    //         {
    //             for (var x = 0; x < 9; x++)
    //             {
    //                 if ((_cellCandidates[(y << 3) + y + x] & onceRow) > 0)
    //                 {
    //                     _cellCandidates[(y << 3) + y + x] = onceRow;
    //                 }
    //             }
    //
    //             return true;
    //         }
    //
    //         if (BitOperations.PopCount((uint) onceColumn) == 1)
    //         {
    //             for (var x = 0; x < 9; x++)
    //             {
    //                 if ((_cellCandidates[(x << 3) + x + y] & onceColumn) > 0)
    //                 {
    //                     _cellCandidates[(x << 3) + x + y] = onceColumn;
    //                 }
    //             }
    //
    //             return true;
    //         }
    //     }
    //
    //     for (var yO = 0; yO < 81; yO += 27)
    //     {
    //         for (var xO = 0; xO < 9; xO += 3)
    //         {
    //             var oneMask = 0;
    //
    //             var twoMask = 0;
    //
    //             var start = yO + xO;
    //
    //             for (var y = 0; y < 3; y++)
    //             {
    //                 for (var x = 0; x < 3; x++)
    //                 {
    //                     twoMask |= oneMask & _cellCandidates[start + (y << 3) + y + x];
    //
    //                     oneMask |= _cellCandidates[start + (y << 3) + y + x];
    //                 }
    //             }
    //
    //             var once = oneMask & ~twoMask;
    //
    //             if (BitOperations.PopCount((uint) once) == 1)
    //             {
    //                 for (var y = 0; y < 3; y++)
    //                 {
    //                     for (var x = 0; x < 3; x++)
    //                     {
    //                         if ((_cellCandidates[start + (y << 3) + y + x] & once) > 0)
    //                         {
    //                             _cellCandidates[start + (y << 3) + y + x] = once;
    //                         }
    //                     }
    //                 }
    //
    //                 return true;
    //             }
    //         }
    //     }
    //
    //     return false;
    // }
    //
    // private void FindNakedPairs()
    // {
    //     var first = 0;
    //
    //     var second = 0;
    //
    //     var third = 0;
    //
    //     var count = 0;
    //
    //     for (var i = 0; i < 9; i++)
    //     {
    //         var candidate = _rowCandidates[i];
    //         
    //         if (BitOperations.PopCount((uint) candidate) == 2)
    //         {
    //             if (first > 0)
    //             {
    //                 if (second > 0)
    //                 {
    //                     third = candidate;
    //                 }
    //                 else
    //                 {
    //                     second = candidate;
    //                 }
    //             }
    //             else
    //             {
    //                 first = candidate;
    //             }
    //         }
    //
    //         if (candidate > 0)
    //         {
    //             count++;
    //         }
    //     }
    //
    //     if (first == second && third == 0 && count > 2)
    //     {
    //         first = ~first;
    //
    //         for (var i = 0; i < 9; i++)
    //         {
    //             if (_rowCandidates[i] != first)
    //             {
    //                 _rowCandidates[i] &= first;
    //             }
    //         }
    //
    //         return;
    //     }
    //
    //     first = 0;
    //
    //     second = 0;
    //
    //     third = 0;
    //
    //     for (var i = 0; i < 9; i++)
    //     {
    //         var candidate = _columnCandidates[i];
    //         
    //         if (BitOperations.PopCount((uint) candidate) == 2)
    //         {
    //             if (first > 0)
    //             {
    //                 if (second > 0)
    //                 {
    //                     third = candidate;
    //                 }
    //                 else
    //                 {
    //                     second = candidate;
    //                 }
    //             }
    //             else
    //             {
    //                 first = candidate;
    //             }
    //         }
    //
    //         if (candidate > 0)
    //         {
    //             count++;
    //         }
    //
    //     }
    //
    //     if (first == second && third == 0 && count > 2)
    //     {
    //         first = ~first;
    //
    //         for (var i = 0; i < 9; i++)
    //         {
    //             if (_columnCandidates[i] != first)
    //             {
    //                 _columnCandidates[i] &= first;
    //             }
    //         }
    //
    //         return;
    //     }
    //
    //     first = 0;
    //
    //     second = 0;
    //
    //     third = 0;
    //
    //     for (var i = 0; i < 9; i++)
    //     {
    //         var candidate = _boxCandidates[i];
    //         
    //         if (BitOperations.PopCount((uint) candidate) == 2)
    //         {
    //             if (first > 0)
    //             {
    //                 if (second > 0)
    //                 {
    //                     third = candidate;
    //                 }
    //                 else
    //                 {
    //                     second = candidate;
    //                 }
    //             }
    //             else
    //             {
    //                 first = candidate;
    //             }
    //         }
    //
    //         if (candidate > 0)
    //         {
    //             count++;
    //         }
    //     }
    //
    //     if (first == second && third == 0 && count > 2)
    //     {
    //         first = ~first;
    //
    //         for (var i = 0; i < 9; i++)
    //         {
    //             if (_boxCandidates[i] != first)
    //             {
    //                 _boxCandidates[i] &= first;
    //             }
    //         }
    //     }
    // }

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

                    if (count == 1)
                    {
                        return (position, values, valueCount);
                    }
                }
            }
        }

        return (position, values, valueCount);
    }

    private bool CreateNextSteps(Span<int> puzzle, ((int X, int Y) Position, int Values, int ValueCount) move, int score, ref int steps, List<Move> history)
    {
        for (var i = 1; i < 10; i++)
        {
            var bit = 1 << (i - 1);

            if ((move.Values & bit) == 0)
            {
                continue;
            }

            puzzle[move.Position.X + (move.Position.Y << 3) + move.Position.Y] = i;

            score--;

            history?.Add(new Move(move.Position.X, move.Position.Y, i));

            if (score == 0)
            {
                return true;
            }

            steps++;

            if (SolveStep(puzzle, score, ref steps, history))
            {
                return true;
            }

            puzzle[move.Position.X + (move.Position.Y << 3) + move.Position.Y] = 0;

            history?.RemoveAt(history.Count - 1);

            score++;
        }

        return false;
    }
}