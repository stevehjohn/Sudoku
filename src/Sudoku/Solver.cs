using System.Diagnostics;
using System.Numerics;
using System.Runtime.CompilerServices;
using Sudoku.Extensions;

namespace Sudoku;

public class Solver
{
    private readonly int[] _cellCandidates = new int[81];

    private readonly int[] _solution = new int[81];

    private readonly int[] _workingCopy = new int[81];

    private readonly HistoryType _historyType;

    private readonly SolveMethod _solveMethod;

    private List<Move> _history;

    private int _steps;

    private int _solutionCount;

    private int _score;

    private MoveType _moveType;

    private bool _verifyOnly;

    public Solver(HistoryType historyType, SolveMethod solveMethod)
    {
        _historyType = historyType;

        _solveMethod = solveMethod;
    }

    public SudokuResult Solve(int[] puzzle, bool verifyOnly = false)
    {
        _verifyOnly = verifyOnly;

        _solutionCount = 0;

        _steps = 0;

        _score = 81;

        var stopwatch = Stopwatch.StartNew();

        for (var i = 0; i < 81; i++)
        {
            if (puzzle[i] != 0)
            {
                _score--;

                _workingCopy[i] = puzzle[i];
            }
            else
            {
                _workingCopy[i] = 0;
            }
        }

        var span = new Span<int>(_workingCopy);

        switch (_score)
        {
            case 0:
                stopwatch.Stop();

                return span.IsValidSudoku()
                    ? new SudokuResult(_workingCopy, true, _steps, stopwatch.Elapsed.TotalMicroseconds, null, null, 0, "Full valid board")
                    : new SudokuResult(_workingCopy, false, _steps, stopwatch.Elapsed.TotalMicroseconds, null, null, 0, "Full invalid board");

            case > 64:
                stopwatch.Stop();

                return new SudokuResult(_workingCopy, false, _steps, stopwatch.Elapsed.TotalMicroseconds, null, null, 0, $"Insufficient number of clues: {81 - _score}");
        }

        _history = _historyType != HistoryType.None ? [] : null;

        var candidates = GetSectionCandidates(span);

        List<int>[] initialCandidates = null;

        if (_historyType == HistoryType.AllSteps)
        {
            initialCandidates = new List<int>[81];

            GetCellCandidates(puzzle, candidates);

            for (var i = 0; i < 81; i++)
            {
                if (_cellCandidates[i] <= 0)
                {
                    continue;
                }

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

        GetCellCandidates(puzzle, candidates);

        var solved = SolveStep(span, candidates);

        stopwatch.Stop();

        if (! solved && _solutionCount == 0)
        {
            return new SudokuResult(_workingCopy, false, _steps, stopwatch.Elapsed.TotalMicroseconds, _history, initialCandidates, 0, "Unsolvable");
        }

        return _solutionCount > 1
            ? new SudokuResult(_workingCopy, false, _steps, stopwatch.Elapsed.TotalMicroseconds, _history, initialCandidates, _solutionCount, $"Multiple solutions: {_solutionCount}")
            : new SudokuResult(_solution, true, _steps, stopwatch.Elapsed.TotalMicroseconds, _history, initialCandidates, 1, "Solved");
    }

    private bool SolveStep(Span<int> puzzle, (Candidates Row, Candidates Column, Candidates Box) candidates)
    {
        if (! GetCellCandidates(puzzle, candidates))
        {
            return false;
        }

        while (true)
        {
            var single = FindHiddenSingle();

            if (single != -1)
            {
                return CreateNextSteps(puzzle, (Position: (X: single % 9, Y: single / 9), Values: _cellCandidates[single], ValueCount: 1), candidates);
            }

            var move = FindNakedSingle(puzzle);

            if (move.ValueCount == 1)
            {
                return CreateNextSteps(puzzle, move, candidates);
            }

            if (_score < 55 && move.ValueCount < 4)
            {
                var box = move.Position.Y / 3 * 3 + move.Position.X / 3;
                
                var changed = FindNakedPairs(UnitTables.Row(move.Position.Y), move.Position.Y, MoveType.NakedPairRow);

                changed |= FindNakedPairs(UnitTables.Column(move.Position.X), move.Position.X, MoveType.NakedPairColumn);

                changed |= FindNakedPairs(UnitTables.Box(box), box, MoveType.NakedPairBox);

                if (changed)
                {
                    continue;
                }
            }

            if (move.ValueCount == 0)
            {
                return false;
            }

            return CreateNextSteps(puzzle, move, candidates);
        }
    }

    private static (Candidates Row, Candidates Column, Candidates Box) GetSectionCandidates(Span<int> puzzle)
    {
        var rowCandidates = new Candidates();

        var columnCandidates = new Candidates();

        var boxCandidates = new Candidates();

        for (var i = 0; i < 81; i++)
        {
            if (puzzle[i] == 0)
            {
                continue;
            }

            var x = i % 9;

            var y = i / 9;

            var boxY = y / 3 * 3;

            rowCandidates.Remove(y, puzzle[i]);

            columnCandidates.Remove(x, puzzle[i]);

            boxCandidates.Remove(boxY + x / 3, puzzle[i]);
        }

        return (rowCandidates, columnCandidates, boxCandidates);
    }

    private bool GetCellCandidates(Span<int> puzzle, (Candidates Row, Candidates Column, Candidates Box) candidates)
    {
        for (var i = 0; i < 81; i++)
        {
            if (puzzle[i] == 0)
            {
                var x = i % 9;

                var y = i / 9;

                var boxY = y / 3 * 3;

                _cellCandidates[i] = candidates.Column[x] & candidates.Row[y] & candidates.Box[boxY + x / 3];

                if (_cellCandidates[i] != 0)
                {
                    continue;
                }

                if (_historyType == HistoryType.AllSteps)
                {
                    _history.Add(new Move(x, y, 0, MoveType.NoCandidates));
                }

                return false;
            }

            _cellCandidates[i] = 0;
        }

        return true;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private int FindHiddenSingle()
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

                if (BitOperations.PopCount((uint) once) != 1)
                {
                    continue;
                }

                for (var y = 0; y < 3; y++)
                {
                    for (var x = 0; x < 3; x++)
                    {
                        if ((_cellCandidates[start + (y << 3) + y + x] & once) <= 0)
                        {
                            continue;
                        }

                        _cellCandidates[start + (y << 3) + y + x] = once;

                        _moveType = MoveType.HiddenSingle;

                        return start + (y << 3) + y + x;
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
            {
                for (var x = 0; x < 9; x++)
                {
                    if ((_cellCandidates[y9 + x] & onceRow) <= 0)
                    {
                        continue;
                    }

                    _cellCandidates[y9 + x] = onceRow;

                    _moveType = MoveType.HiddenSingle;

                    return y9 + x;
                }
            }

            if (BitOperations.PopCount((uint) onceColumn) != 1)
            {
                continue;
            }

            for (var x = 0; x < 9; x++)
            {
                if ((_cellCandidates[(x << 3) + x + y] & onceColumn) <= 0)
                {
                    continue;
                }

                _cellCandidates[(x << 3) + x + y] = onceColumn;

                _moveType = MoveType.HiddenSingle;

                return (x << 3) + x + y;
            }
        }

        return -1;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private ((int X, int Y) Position, int Values, int ValueCount) FindNakedSingle(Span<int> puzzle)
    {
        var position = (X: -1, Y: -1);

        var values = 0;

        var valueCount = 0b11_1111_1111;

        for (var i = 0; i < 81; i++)
        {
            if (puzzle[i] != 0)
            {
                continue;
            }

            var candidates = _cellCandidates[i];

            var count = BitOperations.PopCount((uint) candidates);

            if (count >= valueCount)
            {
                continue;
            }

            position = (i % 9, i / 9);

            values = candidates;

            valueCount = count;

            if (count != 1)
            {
                continue;
            }

            _moveType = MoveType.NakedSingle;

            return (position, values, valueCount);
        }

        return (position, values, valueCount);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private bool FindNakedPairs(ReadOnlySpan<int> unit, int unitIndex, MoveType moveType)
    {
        var mask = 0;

        var count = 1;

        for (var i = 0; i < 9; i++)
        {
            var cell = _cellCandidates[unit[i]];

            if (BitOperations.PopCount((uint) cell) == 2)
            {
                if (mask == 0)
                {
                    mask = cell;
                }
                else if (cell == mask)
                {
                    count++;
                }
            }
        }

        if (count == 2)
        {
            var metadata = new NakedPairMetadata(unitIndex);
            
            for (var i = 0; i < 9; i++)
            {
                var index = unit[i];

                var cell = _cellCandidates[index];

                if (cell == mask)
                {
                    metadata.Pair.Add(index);

                    continue;
                }

                var overlap = cell & mask;

                if (overlap == 0)
                {
                    continue;
                }

                var remaining = cell & ~overlap;
                
                if (remaining != cell)
                {
                    metadata.Affected.Add(index);

                    _cellCandidates[index] = remaining;
                }
            }
        
            if (_historyType != HistoryType.None && metadata.Affected.Count > 0)
            {
                var move = new Move(0, 0, mask, moveType);
            
                move.AddMetadata(metadata);
            
                _history.Add(move);
            }

            return metadata.Affected.Count > 0;
        }

        return false;
    }

    private bool CreateNextSteps(Span<int> puzzle, ((int X, int Y) Position, int Values, int ValueCount) move, (Candidates Row, Candidates Column, Candidates Box) candidates)
    {
        var cell = move.Position.X + (move.Position.Y << 3) + move.Position.Y;

        var values = move.Values;

        while (values > 0)
        {
            var i = BitOperations.TrailingZeroCount(values) + 1;

            values &= values - 1;

            puzzle[cell] = i;

            var box = move.Position.Y / 3 * 3 + move.Position.X / 3;

            var oldRowCandidates = candidates.Row[move.Position.Y];

            var oldColumnCandidates = candidates.Column[move.Position.X];

            var oldBoxCandidates = candidates.Box[box];

            candidates.Row.Remove(move.Position.Y, i);

            candidates.Column.Remove(move.Position.X, i);

            candidates.Box.Remove(box, i);

            _score--;

            if (move.ValueCount > 1)
            {
                _moveType = MoveType.Guess;
            }

            if (_historyType != HistoryType.None)
            {
                var historyMove = new Move(move.Position.X, move.Position.Y, i, _moveType);

                var historyCandidates = new List<int>();

                for (var j = 1; j < 10; j++)
                {
                    if ((move.Values & 1 << (j - 1)) != 0)
                    {
                        historyCandidates.Add(j);
                    }
                }

                historyMove.Candidates = historyCandidates.ToArray();

                _history?.Add(historyMove);
            }

            if (_score == 0)
            {
                if (_solutionCount == 0 && ! _verifyOnly)
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

            candidates.Row[move.Position.Y] = oldRowCandidates;

            candidates.Column[move.Position.X] = oldColumnCandidates;

            candidates.Box[box] = oldBoxCandidates;

            if (_historyType != HistoryType.None)
            {
                if (_historyType == HistoryType.SolutionOnly)
                {
                    _history?.RemoveAt(_history.Count - 1);
                }
                else
                {
                    _history?.Add(new Move(move.Position.X, move.Position.Y, i, MoveType.Backtrack));
                }
            }

            _moveType = MoveType.Guess;

            _score++;
        }

        return false;
    }
}