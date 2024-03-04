using System.Diagnostics;
using System.Numerics;

namespace Sudoku.Solver;

public class Solver
{
    private readonly int[] _rowCandidates = new int[9];
    
    private readonly int[] _columnCandidates = new int[9];
    
    private readonly int[] _boxCandidates = new int[9];

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
        
        GetCellCandidates(span);

        SolveStep(span, score, ref steps, history);
        
        stopwatch.Stop();
        
        return (workingCopy, steps, stopwatch.Elapsed.TotalMicroseconds, history);
    }
    
    private bool SolveStep(Span<int> puzzle, int score, ref int steps, List<Move> history)
    {
        var move = FindLowestMove(puzzle);

        return CreateNextSteps(puzzle, move, score, ref steps, history);
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

    private bool CreateNextSteps(Span<int> puzzle, ((int X, int Y) Position, int Values, int ValueCount) move, int score, ref int steps, List<Move> history)
    {
        for (var i = 1; i < 10; i++)
        {
            var bit = 1 << i;
            
            if ((move.Values & bit) == 0)
            {
                continue;
            }

            puzzle[move.Position.X + (move.Position.Y << 3) + move.Position.Y] = i;

            var copy = new int[81];
            
            Array.Copy(_cellCandidates, copy, 81);

            for (var j = 0; j < 9; j++)
            {
                _cellCandidates[j + move.Position.Y * 9] &= ~bit;
                
                _cellCandidates[move.Position.Y + j * 9] &= ~bit;

                _cellCandidates[move.Position.Y / 3 * 3 + move.Position.X / 3] &= ~bit;
            }

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

            Array.Copy(copy, _cellCandidates, 81);

            history?.RemoveAt(history.Count - 1);

            score++;
        }

        return false;
    }
}