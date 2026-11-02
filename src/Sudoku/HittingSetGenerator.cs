using System.Runtime.InteropServices;

namespace Sudoku;

public class HittingSetGenerator
{
    private static readonly Int128[] CellBits = [.. Enumerable.Range(0, 81).Select(i => Int128.One << i)];

    private readonly Solver _solver = new();

    private readonly Generator _gridGenerator = new();

    private readonly Random _random = new();

    private readonly List<Int128> _sets = [];

    private readonly int[] _scratch = new int[81];

    private readonly int[,] _position = new int[10, 27];

    private readonly int[] _stack = new int[18];

    private int[] _solution;

    private int _clues;

    private long _nodes;

    private long _nodeLimit;

    private bool _stopped;

    private CancellationToken _cancellationToken;

    private Int128 _found;

    public (HittingSetResult Result, int[] Puzzle) Generate(int clues, long nodeLimit, CancellationToken cancellationToken)
    {
        var grid = _gridGenerator.CreateSolvedPuzzle();

        return Generate(grid, clues, nodeLimit, cancellationToken);
    }

    public (HittingSetResult Result, int[] Puzzle) Generate(int[] solvedPuzzle, int clues, long nodeLimit, CancellationToken cancellationToken)
    {
        _solution = solvedPuzzle;

        _clues = clues;

        _nodes = 0;

        _nodeLimit = nodeLimit;

        _stopped = false;

        _cancellationToken = cancellationToken;

        _sets.Clear();

        SeedTwoDigitSets();

        var puzzle = new int[81];

        if (Search(0, 0, 0))
        {
            BuildPuzzle(puzzle);

            return (HittingSetResult.Found, puzzle);
        }

        if (cancellationToken.IsCancellationRequested)
        {
            return (HittingSetResult.Cancelled, puzzle);
        }

        return (_stopped ? HittingSetResult.Abandoned : HittingSetResult.None, puzzle);
    }

    private bool Search(Int128 chosen, int count, Int128 excluded)
    {
        if (++_nodes > _nodeLimit || ((_nodes & 1023) == 0 && _cancellationToken.IsCancellationRequested))
        {
            _stopped = true;

            return false;
        }

        while (true)
        {
            Int128 best = 0;

            var bestSize = int.MaxValue;

            Int128 packed = 0;

            var packing = 0;

            var sets = CollectionsMarshal.AsSpan(_sets);

            for (var i = 0; i < sets.Length; i++)
            {
                var set = sets[i];

                if ((set & chosen) != 0)
                {
                    continue;
                }

                var available = set & ~excluded;

                var size = (int) Int128.PopCount(available);

                if (size == 0)
                {
                    return false;
                }

                if (size < bestSize)
                {
                    best = available;

                    bestSize = size;
                }

                if ((available & packed) == 0)
                {
                    packed |= available;

                    packing++;
                }
            }

            if (count + packing > _clues)
            {
                return false;
            }

            if (bestSize == int.MaxValue)
            {
                for (var i = 0; i < 81; i++)
                {
                    _scratch[i] = (chosen & CellBits[i]) != 0 ? _solution[i] : 0;
                }

                if (! _solver.TryFindAlternative(_scratch, _solution, out var differences))
                {
                    _found = chosen;

                    return true;
                }

                _sets.Add(Minimise(differences));

                continue;
            }

            var cells = best;

            var excludedHere = excluded;

            while (cells != 0)
            {
                var cell = (int) Int128.TrailingZeroCount(cells);

                cells &= cells - 1;

                if (Search(chosen | CellBits[cell], count + 1, excludedHere))
                {
                    return true;
                }

                if (_stopped)
                {
                    return false;
                }

                excludedHere |= CellBits[cell];
            }

            return false;
        }
    }

    private Int128 Minimise(Int128 set)
    {
        var improved = true;

        while (improved)
        {
            improved = false;

            for (var i = 0; i < 81; i++)
            {
                _scratch[i] = (set & CellBits[i]) == 0 ? _solution[i] : 0;
            }

            for (var i = 0; i < 81 && ! improved; i++)
            {
                if ((set & CellBits[i]) == 0)
                {
                    continue;
                }

                _scratch[i] = _solution[i];

                if (_solver.TryFindAlternative(_scratch, _solution, out var smaller))
                {
                    set = smaller;

                    improved = true;
                }
                else
                {
                    _scratch[i] = 0;
                }
            }
        }

        return set;
    }

    private void SeedTwoDigitSets()
    {
        for (var cell = 0; cell < 81; cell++)
        {
            var digit = _solution[cell];

            var row = UnitTables.CellRow(cell);

            var column = UnitTables.CellColumn(cell);

            var box = UnitTables.CellBox(cell);

            _position[digit, row] = cell;

            _position[digit, 9 + column] = cell;

            _position[digit, 18 + box] = cell;
        }

        for (var a = 1; a < 10; a++)
        {
            for (var b = a + 1; b < 10; b++)
            {
                Int128 seen = 0;

                for (var start = 0; start < 81; start++)
                {
                    if (_solution[start] != a || (seen & CellBits[start]) != 0)
                    {
                        continue;
                    }

                    var component = CellBits[start];

                    var top = 0;

                    _stack[top++] = start;

                    while (top > 0)
                    {
                        var cell = _stack[--top];

                        var other = _solution[cell] == a ? b : a;

                        for (var house = 0; house < 3; house++)
                        {
                            var unit = house switch
                            {
                                0 => UnitTables.CellRow(cell),
                                1 => 9 + UnitTables.CellColumn(cell),
                                _ => 18 + UnitTables.CellBox(cell)
                            };

                            var next = _position[other, unit];

                            if ((component & CellBits[next]) == 0)
                            {
                                component |= CellBits[next];

                                _stack[top++] = next;
                            }
                        }
                    }

                    seen |= component;

                    if (Int128.PopCount(component) <= 12)
                    {
                        _sets.Add(component);
                    }
                }
            }
        }
    }

    private void BuildPuzzle(int[] puzzle)
    {
        var count = 0;

        for (var i = 0; i < 81; i++)
        {
            if ((_found & CellBits[i]) != 0)
            {
                puzzle[i] = _solution[i];

                count++;
            }
            else
            {
                puzzle[i] = 0;
            }
        }

        while (count < _clues)
        {
            var cell = _random.Next(81);

            if (puzzle[cell] != 0)
            {
                continue;
            }

            puzzle[cell] = _solution[cell];

            count++;
        }
    }
}