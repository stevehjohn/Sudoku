using System.Diagnostics;
using System.Numerics;
using System.Runtime.CompilerServices;

namespace Sudoku;

public class Solver
{
    private readonly int[] _cellCandidates = new int[81];

    public (int[] Solution, int Steps, double Microseconds, List<Move> History) Solve(int[] puzzle, HistoryType historyType = HistoryType.None, bool unique = false)
    {
        var solutionCount = unique ? 2 : 1;
        
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

        var history = historyType != HistoryType.None ? new List<Move>() : null;

        var span = new Span<int>(workingCopy);

        var candidates = GetSectionCandidates(span);

        SolveStep(span, score, candidates, ref steps, ref solutionCount, historyType, history);

        stopwatch.Stop();

        if (unique && solutionCount == 0)
        {
            history?.Clear();
            
            return (null, steps, stopwatch.Elapsed.TotalMicroseconds, history);
        }

        return (workingCopy, steps, stopwatch.Elapsed.TotalMicroseconds, history);
    }

    private bool SolveStep(Span<int> puzzle, int score, (Candidates Row, Candidates Column, Candidates Box) candidates, ref int steps, ref int solutionCount, HistoryType historyType, List<Move> history)
    {
        GetCellCandidates(puzzle, candidates);

        if (score > 27)
        {
            FindHiddenSingles();
        }

        var move = FindLowestMove(puzzle);

        return CreateNextSteps(puzzle, move, score, candidates, ref steps, ref solutionCount, historyType, history);
    }

    private static (Candidates Row, Candidates Column, Candidates Box) GetSectionCandidates(Span<int> puzzle)
    {
        var rowCandidates = new Candidates();

        var columnCandidates = new Candidates();

        var boxCandidates = new Candidates();

        for (var y = 0; y < 9; y++)
        {
            var y9 = (y << 3) + y;

            var y3 = y / 3 * 3;
            
            for (var x = 0; x < 9; x++)
            {
                rowCandidates.Remove(y, puzzle[x + y9]);

                columnCandidates.Remove(y, puzzle[y + (x << 3) + x]);
                
                boxCandidates.Remove(y3 + x / 3, puzzle[x + y9]);
            }
        }

        return (rowCandidates, columnCandidates, boxCandidates);
    }

    private void GetCellCandidates(Span<int> puzzle, (Candidates Row, Candidates Column, Candidates Box) candidates)
    {
        for (var y = 0; y < 9; y++)
        {
            var boxY = y / 3 * 3;
            
            for (var x = 0; x < 9; x++)
            {
                var cell = x + (y << 3) + y;
                
                if (puzzle[x + (y << 3) + y] == 0)
                {
                    _cellCandidates[cell] = candidates.Column[x] & candidates.Row[y] & candidates.Box[boxY + x / 3];
                }
                else
                {
                    _cellCandidates[cell] = 0;
                }
            }
        }
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private void FindHiddenSingles()
    {
        for (var y = 0; y < 9; y++)
        {
            var oneMaskRow = 0;
    
            var twoMaskRow = 0;
    
            var oneMaskColumn = 0;
    
            var twoMaskColumn = 0;
    
            var y9 = (y << 3) + y;
            
            for (var x = 0; x < 9; x++)
            {
                twoMaskRow |= oneMaskRow & _cellCandidates[y9 + x];
    
                oneMaskRow |= _cellCandidates[y9 + x];
    
                twoMaskColumn |= oneMaskColumn & _cellCandidates[(x << 3) + x + y];
    
                oneMaskColumn |= _cellCandidates[(x << 3) + x + y];
            }
    
            var onceRow = oneMaskRow & ~twoMaskRow;
    
            var onceColumn = oneMaskColumn & ~twoMaskColumn;
    
            if (BitOperations.PopCount((uint) onceRow) == 1)
            {
                for (var x = 0; x < 9; x++)
                {
                    if ((_cellCandidates[y9 + x] & onceRow) > 0)
                    {
                        _cellCandidates[y9 + x] = onceRow;
                    }
                }
    
                return;
            }
    
            if (BitOperations.PopCount((uint) onceColumn) == 1)
            {
                for (var x = 0; x < 9; x++)
                {
                    if ((_cellCandidates[(x << 3) + x + y] & onceColumn) > 0)
                    {
                        _cellCandidates[(x << 3) + x + y] = onceColumn;
                    }
                }
    
                return;
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
                    var y9 = (y << 3) + y;
                    
                    for (var x = 0; x < 3; x++)
                    {
                        twoMask |= oneMask & _cellCandidates[start + y9 + x];
    
                        oneMask |= _cellCandidates[start + y9 + x];
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
    
                    return;
                }
            }
        }
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private ((int X, int Y) Position, int Values, int ValueCount) FindLowestMove(Span<int> puzzle)
    {
        var position = (X: -1, Y: -1);

        var values = 0;

        var valueCount = 0b11_1111_1111;

        for (var y = 0; y < 9; y++)
        {
            var y9 = (y << 3) + y;
            
            for (var x = 0; x < 9; x++)
            {
                if (puzzle[x + y9] != 0)
                {
                    continue;
                }

                var candidates = _cellCandidates[x + y9];

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

    private bool CreateNextSteps(Span<int> puzzle, ((int X, int Y) Position, int Values, int ValueCount) move, int score, (Candidates Row, Candidates Column, Candidates Box) candidates, ref int steps, ref int solutionCount, HistoryType historyType, List<Move> history)
    {
        var cell = move.Position.X + (move.Position.Y << 3) + move.Position.Y;
            
        for (var i = 1; i < 10; i++)
        {
            var bit = 1 << (i - 1);

            if ((move.Values & bit) == 0)
            {
                continue;
            }

            puzzle[cell] = i;

            var oldCandidates = candidates;

            candidates.Row.Remove(move.Position.Y, i);

            candidates.Column.Remove(move.Position.X, i);

            candidates.Box.Remove(move.Position.Y / 3 * 3 + move.Position.X / 3, i);
            
            score--;

            if (historyType != HistoryType.None)
            {
                var historyMove = new Move(move.Position.X, move.Position.Y, i, false);

                var historyCandidates = new List<int>();
                
                for (var j = 1; j < 10; j++)
                {
                    if ((move.Values & 1 << (j - 1)) != 0)
                    {
                        historyCandidates.Add(j);
                    }
                }

                historyMove.Candidates = historyCandidates.ToArray();

                history?.Add(historyMove);
            }

            if (score == 0)
            {
                solutionCount--;

                return solutionCount == 0;
            }

            steps++;

            if (SolveStep(puzzle, score, candidates, ref steps, ref solutionCount, historyType, history))
            {
                return true;
            }

            puzzle[cell] = 0;

            candidates = oldCandidates;

            if (historyType != HistoryType.None)
            {
                if (historyType == HistoryType.SolutionOnly)
                {
                    history?.RemoveAt(history.Count - 1);
                }
                else
                {
                    history?.Add(new Move(move.Position.X, move.Position.Y, i, true));
                }
            }

            score++;
        }

        return false;
    }
}