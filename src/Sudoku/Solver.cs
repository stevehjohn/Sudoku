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

    private int _candidateCount;

    private int[] _knownSolution;

    private readonly ushort[] _rowMask = new ushort[9];
    
    private readonly ushort[] _columnMask = new ushort[9];
    
    private readonly ushort[] _boxMask = new ushort[9];

    public Solver(HistoryType historyType = HistoryType.None, SolveMethod solveMethod = SolveMethod.FindUnique)
    {
        _historyType = historyType;

        _solveMethod = solveMethod;
    }

    public SudokuResult Solve(int[] puzzle, bool verifyOnly = false)
    {
        var stopwatch = Stopwatch.StartNew();

        _knownSolution = null;

        _verifyOnly = verifyOnly;

        Initialise(new Span<int>(puzzle));

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

        List<int>[] initialCandidates = null;

        GetCellCandidates();

        if (_historyType == HistoryType.AllSteps)
        {
            initialCandidates = new List<int>[81];

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

        var solved = SolveStep();

        stopwatch.Stop();

        if (! solved && _solutionCount == 0)
        {
            return new SudokuResult(_workingCopy, false, _steps, stopwatch.Elapsed.TotalMicroseconds, _history, initialCandidates, 0, "Unsolvable");
        }

        return _solutionCount > 1
            ? new SudokuResult(_workingCopy, false, _steps, stopwatch.Elapsed.TotalMicroseconds, _history, initialCandidates, _solutionCount, $"Multiple solutions: {_solutionCount}")
            : new SudokuResult(_solution, true, _steps, stopwatch.Elapsed.TotalMicroseconds, _history, initialCandidates, 1, "Solved");
    }

    public bool HasUniqueSolution(int[] puzzle, int[] knownSolution)
    {
        _verifyOnly = true;

        Initialise(new Span<int>(puzzle));

        _knownSolution = knownSolution;

        if (_candidateCount == 0)
        {
            return false;
        }

        _solutionCount = 0;

        SolveStep();

        _knownSolution = null;

        return _solutionCount == 1;
    }

    private void Initialise(Span<int> puzzle)
    {
        _solutionCount = 0;

        _steps = 0;

        _score = 81;

        Array.Clear(_rowMask, 0, _rowMask.Length);

        Array.Clear(_columnMask, 0, _columnMask.Length);

        Array.Clear(_boxMask, 0, _boxMask.Length);

        for (var i = 0; i < 81; i++)
        {
            var value = puzzle[i];

            if (puzzle[i] != 0)
            {
                _score--;
            }

            _workingCopy[i] = puzzle[i];

            if (value == 0)
            {
                continue;
            }

            var bit = (ushort) (1 << (value - 1));

            var cellUnits = UnitTables.CellUnits(i);

            var x = cellUnits[0];

            var y = cellUnits[1];

            var box = cellUnits[2];

            _rowMask[y] |= bit;

            _columnMask[x] |= bit;

            _boxMask[box] |= bit;
        }

        GetCellCandidates();
    }

    private bool SolveStep()
    {
        if (_candidateCount == 0)
        {
            return false;
        }

        while (true)
        {
            var single = FindHiddenSingle();

            if (single != -1)
            {
                return CreateNextSteps((Position: (X: UnitTables.CellColumn(single), Y: UnitTables.CellRow(single)), Values: _cellCandidates[single], ValueCount: 1));
            }

            var move = FindLeastRemainingCandidates();

            if (move.ValueCount == 1)
            {
                return CreateNextSteps(move);
            }

            if (_score < 55 && move.ValueCount < 4)
            {
                var box = UnitTables.CellBox(move.Position.Y * 9 + move.Position.X);

                var changed = FindNakedPairs(UnitTables.RowCells(move.Position.Y), move.Position.Y, MoveType.NakedPairRow);

                changed |= FindNakedPairs(UnitTables.ColumnCells(move.Position.X), move.Position.X, MoveType.NakedPairColumn);

                changed |= FindNakedPairs(UnitTables.BoxCells(box), box, MoveType.NakedPairBox);

                if (changed)
                {
                    continue;
                }
            }

            if (move.ValueCount == 0)
            {
                return false;
            }

            return CreateNextSteps(move);
        }
    }

    private void GetCellCandidates()
    {
        _candidateCount = 0;

        for (var i = 0; i < 81; i++)
        {
            if (_workingCopy[i] == 0)
            {
                var cellUnits = UnitTables.CellUnits(i);

                var x = cellUnits[0];

                var y = cellUnits[1];

                var box = cellUnits[2];

                _cellCandidates[i] = ~(_rowMask[y] | _columnMask[x] | _boxMask[box]) & 0x1FF;
                
                if (_cellCandidates[i] != 0)
                {
                    _candidateCount++;

                    continue;
                }

                if (_historyType == HistoryType.AllSteps)
                {
                    _history.Add(new Move(x, y, 0, MoveType.NoCandidates));
                }
            }

            _cellCandidates[i] = 0;
        }
    }

    private void UpdateCellAndPeerCandidates(int updatedCell, ReadOnlySpan<byte> peers)
    {
        UpdateCellCandidates(updatedCell);

        for (var i = 0; i < peers.Length; i++)
        {
            var cell = peers[i];

            UpdateCellCandidates(cell);
        }
    }

    private void UpdateCellCandidates(int cell)
    {
        if (_workingCopy[cell] > 0)
        {
            _cellCandidates[cell] = 0;

            return;
        }

        var oldValue = _cellCandidates[cell];

        var cellUnits = UnitTables.CellUnits(cell);

        var x = cellUnits[0];

        var y = cellUnits[1];

        var box = cellUnits[2];
        
        _cellCandidates[cell] = ~(_rowMask[y] | _columnMask[x] | _boxMask[box]) & 0x1FF;

        if (oldValue > 0)
        {
            if (_cellCandidates[cell] == 0)
            {
                _candidateCount--;
            }
        }
        else
        {
            if (_cellCandidates[cell] > 0)
            {
                _candidateCount++;
            }
        }

        if (_historyType == HistoryType.AllSteps && _cellCandidates[cell] == 0)
        {
            _history.Add(new Move(x, y, 0, MoveType.NoCandidates));
        }
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private int FindHiddenSingle()
    {
        for (var i = 0; i < 9; i++)
        {
            var oneMask = 0;

            var twoMask = 0;

            var cells = UnitTables.BoxCells(i);

            for (var cellIndex = 0; cellIndex < 9; cellIndex++)
            {
                var cell = cells[cellIndex];
                
                twoMask |= oneMask & _cellCandidates[cell];

                oneMask |= _cellCandidates[cell];
            }

            var once = oneMask & ~twoMask;

            if (BitOperations.PopCount((uint) once) != 1)
            {
                continue;
            }

            for (var cellIndex = 0; cellIndex < 9; cellIndex++)
            {
                var cell = cells[cellIndex];
                
                if ((_cellCandidates[cell] & once) <= 0)
                {
                    continue;
                }

                _cellCandidates[cell] = once;

                _moveType = MoveType.HiddenSingle;

                return cell;
            }
        }

        for (var y = 0; y < 9; y++)
        {
            var oneMaskRow = 0;

            var twoMaskRow = 0;

            var oneMaskColumn = 0;

            var twoMaskColumn = 0;

            var y9 = y.MultiplyByNine();

            for (var x = 0; x < 9; x++)
            {
                twoMaskRow |= oneMaskRow & _cellCandidates[y9 + x];

                oneMaskRow |= _cellCandidates[y9 + x];

                twoMaskColumn |= oneMaskColumn & _cellCandidates[x.MultiplyByNine() + y];

                oneMaskColumn |= _cellCandidates[x.MultiplyByNine() + y];
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
                if ((_cellCandidates[x.MultiplyByNine() + y] & onceColumn) <= 0)
                {
                    continue;
                }

                _cellCandidates[x.MultiplyByNine() + y] = onceColumn;

                _moveType = MoveType.HiddenSingle;

                return x.MultiplyByNine() + y;
            }
        }

        return -1;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private ((int X, int Y) Position, int Values, int ValueCount) FindLeastRemainingCandidates()
    {
        var position = (X: -1, Y: -1);

        var values = 0;

        var valueCount = 10;

        for (var i = 0; i < 81; i++)
        {
            if (_workingCopy[i] != 0)
            {
                continue;
            }

            var candidates = _cellCandidates[i];

            var count = BitOperations.PopCount((uint) candidates);

            if (count >= valueCount)
            {
                continue;
            }

            position = (UnitTables.CellColumn(i), UnitTables.CellRow(i));

            values = candidates;

            valueCount = count;

            if (count == 1)
            {
                _moveType = MoveType.NakedSingle;

                return (position, values, valueCount);
            }
        }

        return (position, values, valueCount);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private bool FindNakedPairs(ReadOnlySpan<byte> unit, int unitIndex, MoveType moveType)
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

                    if (count > 2)
                    {
                        return false;
                    }
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

    private bool CreateNextSteps(((int X, int Y) Position, int Values, int ValueCount) move)
    {
        var cell = move.Position.X + move.Position.Y.MultiplyByNine();

        var box = UnitTables.CellBox(cell);

        var values = move.Values;

        var knownValue = 0;

        while (values > 0 || knownValue > 0)
        {
            int value;

            if (values == 0)
            {
                value = knownValue;

                knownValue = 0;
            }
            else
            {
                value = BitOperations.TrailingZeroCount(values) + 1;

                values &= values - 1;
            }

            if (_knownSolution != null && _knownSolution[cell] == value && BitOperations.PopCount((uint) values) > 0)
            {
                knownValue = value;

                continue;
            }

            _workingCopy[cell] = value;

            var bit = (ushort) (1 << (value - 1));

            _rowMask[move.Position.Y] |= bit;

            _columnMask[move.Position.X] |= bit;

            _boxMask[box] |= bit;

            var peers = UnitTables.Peers(cell);

            UpdateCellAndPeerCandidates(cell, peers);

            _score--;

            if (move.ValueCount > 1)
            {
                _moveType = MoveType.Guess;
            }

            if (_historyType != HistoryType.None)
            {
                var historyMove = new Move(move.Position.X, move.Position.Y, value, _moveType);

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
                if (_knownSolution != null && _verifyOnly)
                {
                    for (var i = 0; i < 81; i++)
                    {
                        if (_knownSolution[i] != _workingCopy[i])
                        {
                            _solutionCount = 2;

                            return true;
                        }
                    }

                    _solutionCount = 1;

                    return true;
                }

                if (_solutionCount == 0 && ! _verifyOnly)
                {
                    for (var j = 0; j < 81; j++)
                    {
                        _solution[j] = _workingCopy[j];
                    }
                }

                if (_solveMethod == SolveMethod.FindFirst)
                {
                    return true;
                }

                _solutionCount++;

                if ((_solveMethod == SolveMethod.FindUnique || _verifyOnly) && _solutionCount > 1)
                {
                    return true;
                }
            }

            _steps++;

            if (SolveStep())
            {
                return true;
            }

            _workingCopy[cell] = 0;

            _rowMask[move.Position.Y] &= (ushort) ~bit;

            _columnMask[move.Position.X] &= (ushort) ~bit;

            _boxMask[box] &= (ushort) ~bit;

            UpdateCellAndPeerCandidates(cell, peers);

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