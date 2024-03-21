﻿using System.Diagnostics;
using System.Numerics;
using System.Runtime.CompilerServices;
using Sudoku.Extensions;

namespace Sudoku;

public class Solver
{
    private readonly int[] _cellCandidates = new int[81];

    private readonly int[] _solution = new int[81];

    private readonly int[] _frequencies = new int[10];

    private readonly HistoryType _historyType;

    private readonly SolveMethod _solveMethod;

    private List<Move> _history;

    private int _steps;

    private int _solutionCount;

    private int _score;

    private MoveType _moveType;

    public Solver(HistoryType historyType, SolveMethod solveMethod)
    {
        _historyType = historyType;

        _solveMethod = solveMethod;
    }

    public SudokuResult Solve(int[] puzzle)
    {
        _solutionCount = 0;

        _steps = 0;

        _score = 81;

        var stopwatch = Stopwatch.StartNew();

        var workingCopy = new int[81];

        for (var i = 0; i < 81; i++)
        {
            if (puzzle[i] != 0)
            {
                _score--;

                workingCopy[i] = puzzle[i];
            }
        }

        var span = new Span<int>(workingCopy);

        if (_score == 0)
        {
            stopwatch.Stop();

            if (span.IsValidSudoku())
            {
                return new SudokuResult(workingCopy, true, _steps, stopwatch.Elapsed.TotalMicroseconds, null, null,
                    "Full valid board");
            }

            return new SudokuResult(workingCopy, false, _steps, stopwatch.Elapsed.TotalMicroseconds, null, null,
                "Full invalid board");
        }

        if (_score > 64)
        {
            stopwatch.Stop();

            return new SudokuResult(workingCopy, false, _steps, stopwatch.Elapsed.TotalMicroseconds, null, null,
                $"Insufficient number of clues: {81 - _score}");
        }

        _history = _historyType != HistoryType.None ? new List<Move>() : null;

        var candidates = GetSectionCandidates(span);

        List<int>[] initialCandidates = null;

        if (_historyType == HistoryType.AllSteps)
        {
            initialCandidates = new List<int>[81];

            GetCellCandidates(puzzle, candidates);

            for (var i = 0; i < 81; i++)
            {
                if (_cellCandidates[i] > 0)
                {
                    initialCandidates[i] = [];

                    for (var j = 1; j < 10; j++)
                    {
                        if ((_cellCandidates[i] & 1 << (j - 1)) > 0)
                        {
                            initialCandidates[i].Add(j);
                        }
                    }
                }
            }
        }

        var solved = SolveStep(span, candidates);

        stopwatch.Stop();

        if (! solved && _solutionCount == 0)
        {
            return new SudokuResult(workingCopy, false, _steps, stopwatch.Elapsed.TotalMicroseconds, _history,
                initialCandidates, "Unsolvable");
        }

        if (_solutionCount > 1)
        {
            return new SudokuResult(workingCopy, false, _steps, stopwatch.Elapsed.TotalMicroseconds, _history,
                initialCandidates, $"Multiple solutions: {_solutionCount}");
        }

        return new SudokuResult(_solution, true, _steps, stopwatch.Elapsed.TotalMicroseconds, _history,
            initialCandidates, "Solved");
    }

    private bool SolveStep(Span<int> puzzle, (Candidates Row, Candidates Column, Candidates Box) candidates)
    {
        if (! GetCellCandidates(puzzle, candidates))
        {
            return false;
        }

        _moveType = MoveType.Guess;

        FindHiddenSingles();

        var move = FindLowestMove(puzzle);

        return CreateNextSteps(puzzle, move, candidates);
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

    private bool GetCellCandidates(Span<int> puzzle, (Candidates Row, Candidates Column, Candidates Box) candidates)
    {
        for (var i = 1; i < 10; i++)
        {
            _frequencies[i] = 0;
        }

        for (var y = 0; y < 9; y++)
        {
            var boxY = y / 3 * 3;

            for (var x = 0; x < 9; x++)
            {
                var cell = x + (y << 3) + y;

                if (puzzle[cell] == 0)
                {
                    _cellCandidates[cell] = candidates.Column[x] & candidates.Row[y] & candidates.Box[boxY + x / 3];

                    if (_cellCandidates[cell] == 0)
                    {
                        if (_historyType == HistoryType.AllSteps)
                        {
                            _history.Add(new Move(x, y, 0, MoveType.NoCandidates));
                        }

                        return false;
                    }
                }
                else
                {
                    _cellCandidates[cell] = 0;

                    _frequencies[puzzle[cell]]++;
                }
            }
        }

        return true;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private void FindHiddenSingles()
    {
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
                    for (var y = 0; y < 3; y++)
                    {
                        for (var x = 0; x < 3; x++)
                        {
                            if ((_cellCandidates[start + (y << 3) + y + x] & once) > 0)
                            {
                                _cellCandidates[start + (y << 3) + y + x] = once;

                                _moveType = MoveType.HiddenSingle;

                                return;
                            }
                        }
                    }
            }
        }

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
                for (var x = 0; x < 9; x++)
                {
                    if ((_cellCandidates[y9 + x] & onceRow) > 0)
                    {
                        _cellCandidates[y9 + x] = onceRow;

                        _moveType = MoveType.HiddenSingle;

                        return;
                    }
                }

            if (BitOperations.PopCount((uint) onceColumn) == 1)
                for (var x = 0; x < 9; x++)
                {
                    if ((_cellCandidates[(x << 3) + x + y] & onceColumn) > 0)
                    {
                        _cellCandidates[(x << 3) + x + y] = onceColumn;

                        _moveType = MoveType.HiddenSingle;

                        return;
                    }
                }
        }
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private ((int X, int Y) Position, (int First, int Second) Values, int ValueCount) FindLowestMove(Span<int> puzzle)
    {
        var first = 0;

        var second = 0;

        var position = (-1, -1);

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

                if (count > 0)
                {
                    position = (x, y);

                    for (var i = 1; i < 10; i++)
                    {
                        var bit = 1 << (i - 1);

                        if ((candidates & bit) == 0)
                        {
                            continue;
                        }

                        if (first == 0)
                        {
                            first = i;

                            if (count < 2)
                            {
                                break;
                            }
                        }
                        else
                        {
                            second = i;

                            break;
                        }
                    }

                    goto done;
                }
            }
        }

        done:
        if (second == 0)
        {
            return (position, (first, 0), 1);
        }

        if (_frequencies[first] < _frequencies[second])
        {
            return (position, (first, second), 2);
        }

        return (position, (second, first), 2);
    }

    private bool CreateNextSteps(Span<int> puzzle,
        ((int X, int Y) Position, (int First, int Second) Values, int ValueCount) move,
        (Candidates Row, Candidates Column, Candidates Box) candidates)
    {
        var cell = move.Position.X + (move.Position.Y << 3) + move.Position.Y;

        for (var i = 0; i < move.ValueCount; i++)
        {
            var value = i == 0 ? move.Values.First : move.Values.Second;

            puzzle[cell] = value; 

            var oldCandidates = candidates;

            candidates.Row.Remove(move.Position.Y, value);

            candidates.Column.Remove(move.Position.X, value);

            candidates.Box.Remove(move.Position.Y / 3 * 3 + move.Position.X / 3, value);

            _score--;

            if (move.ValueCount > 1)
            {
                _moveType = MoveType.Guess;
            }

            if (_historyType != HistoryType.None)
            {
                var historyMove = new Move(move.Position.X, move.Position.Y, value, _moveType);

                var historyCandidates = new List<int> { move.Values.First };

                if (move.ValueCount > 1)
                {
                    historyCandidates.Add(move.Values.Second);
                }

                historyMove.Candidates = historyCandidates.ToArray();

                _history?.Add(historyMove);
            }

            if (_score == 0)
            {
                if (_solutionCount == 0)
                {
                    for (var j = 0; j < 81; j++)
                    {
                        _solution[j] = puzzle[j];
                    }
                }

                if (_solveMethod == SolveMethod.FindFirst)
                {
                    return true;
                }

                _solutionCount++;

                if (_solveMethod == SolveMethod.FindUnique && _solutionCount > 1)
                {
                    return true;
                }
            }

            _steps++;

            if (SolveStep(puzzle, candidates))
            {
                return true;
            }

            puzzle[cell] = 0;

            candidates = oldCandidates;

            if (_historyType != HistoryType.None)
            {
                if (_historyType == HistoryType.SolutionOnly)
                {
                    _history?.RemoveAt(_history.Count - 1);
                }
                else
                {
                    _history?.Add(new Move(move.Position.X, move.Position.Y, value, MoveType.Backtrack));
                }
            }

            _moveType = MoveType.Guess;

            _score++;
        }

        return false;
    }
}