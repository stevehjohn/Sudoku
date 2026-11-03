using System.Runtime.InteropServices;
using Sudoku.Extensions;

namespace Sudoku;

public class HittingSetGenerator : IGenerator
{
    private readonly int[] _candidates = new int[729];

    private readonly int[] _candidateCounts = new int[81];

    private const long NodeLimit = 3_000_000;

    private const int WindowCount = 400;

    private readonly HashSet<Int128> _setLookup = [];

    private readonly List<Int128>[] _levels = [.. Enumerable.Range(0, 90).Select(_ => new List<Int128>())];

    private readonly int[] _levelSeen = new int[90];

    private readonly int[] _window = new int[81];

    private static readonly Int128[] CellBits = [.. Enumerable.Range(0, 81).Select(i => Int128.One << i)];

    private static readonly Int128[] HouseMasks = BuildHouseMasks();

    private readonly Solver _solver = new();

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

    public (bool Succeeded, int[] Puzzle) Generate(int[] solvedPuzzle, int cluesToLeave, CancellationToken cancellationToken)
    {
        _solution = solvedPuzzle;

        _clues = cluesToLeave;

        _nodes = 0;

        _nodeLimit = NodeLimit;

        _stopped = false;

        _cancellationToken = cancellationToken;

        _sets.Clear();

        _setLookup.Clear();

        SeedTwoDigitSets();

        SeedWindowSets();

        _sets.Sort((a, b) => Int128.PopCount(a).CompareTo(Int128.PopCount(b)));

        _levels[0].Clear();

        _levelSeen[0] = 0;

        var puzzle = new int[81];

        if (Search(0, 0, 0))
        {
            BuildPuzzle(puzzle);

            var solver = new Solver();

            var result = solver.Solve(puzzle);

            if (! result.Solved)
            {
                return (false, puzzle);
            }

            if (cluesToLeave < 21)
            {
                File.AppendAllLines(Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.UserProfile), "Sudoku.log"), [$"{DateTime.Now:ddd d MMM HH:mm:ss}, {cluesToLeave}er: {puzzle.FlattenPuzzle()} - Nodes: {_nodes}"]);
            }

            return (true, puzzle);
        }
        
        return (false, puzzle);
    }

    private bool Search(int depth, Int128 chosen, Int128 excluded)
    {
        if (++_nodes > _nodeLimit || ((_nodes & 1023) == 0 && _cancellationToken.IsCancellationRequested))
        {
            _stopped = true;

            return false;
        }

        var unhit = _levels[depth];

        Span<int> order = stackalloc int[81];

        Span<int> score = stackalloc int[81];

        while (true)
        {
            while (_levelSeen[depth] < _sets.Count)
            {
                var learned = _sets[_levelSeen[depth]++];

                if ((learned & chosen) == 0)
                {
                    var learnedSize = Int128.PopCount(learned);

                    var position = unhit.Count;

                    while (position > 0 && Int128.PopCount(unhit[position - 1]) > learnedSize)
                    {
                        position--;
                    }

                    unhit.Insert(position, learned);
                }
            }

            Int128 best = 0;

            var bestSize = int.MaxValue;

            Int128 packed = 0;

            var packing = 0;

            var sets = CollectionsMarshal.AsSpan(unhit);

            for (var i = 0; i < sets.Length; i++)
            {
                var available = sets[i] & ~excluded;

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

            if (depth + packing > _clues)
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

                AddSet(Minimise(differences));

                continue;
            }

            var orderCount = 0;

            var remainingCells = best;

            while (remainingCells != 0)
            {
                order[orderCount++] = (int) Int128.TrailingZeroCount(remainingCells);

                remainingCells &= remainingCells - 1;
            }

            for (var i = orderCount - 1; i > 0; i--)
            {
                var j = _random.Next(i + 1);

                (order[i], order[j]) = (order[j], order[i]);
            }

            for (var o = 0; o < orderCount; o++)
            {
                var cellBit = CellBits[order[o]];

                var covered = 0;

                for (var i = 0; i < unhit.Count; i++)
                {
                    if ((unhit[i] & cellBit) != 0)
                    {
                        covered++;
                    }
                }

                score[o] = covered;
            }

            for (var i = 1; i < orderCount; i++)
            {
                var cellValue = order[i];

                var scoreValue = score[i];

                var j = i - 1;

                while (j >= 0 && score[j] < scoreValue)
                {
                    order[j + 1] = order[j];

                    score[j + 1] = score[j];

                    j--;
                }

                order[j + 1] = cellValue;

                score[j + 1] = scoreValue;
            }

            var excludedHere = excluded;

            var child = _levels[depth + 1];

            for (var o = 0; o < orderCount; o++)
            {
                var bit = CellBits[order[o]];

                child.Clear();

                for (var i = 0; i < unhit.Count; i++)
                {
                    if ((unhit[i] & bit) == 0)
                    {
                        child.Add(unhit[i]);
                    }
                }

                _levelSeen[depth + 1] = _levelSeen[depth];

                if (Search(depth + 1, chosen | bit, excludedHere))
                {
                    return true;
                }

                if (_stopped)
                {
                    return false;
                }

                excludedHere |= bit;
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

    private void AddSet(Int128 set)
    {
        if (_setLookup.Add(set))
        {
            _sets.Add(set);
        }
    }

    private static Int128[] BuildHouseMasks()
    {
        var masks = new Int128[27];

        for (var cell = 0; cell < 81; cell++)
        {
            masks[UnitTables.CellRow(cell)] |= CellBits[cell];

            masks[9 + UnitTables.CellColumn(cell)] |= CellBits[cell];

            masks[18 + UnitTables.CellBox(cell)] |= CellBits[cell];
        }

        return masks;
    }

    private Int128 CreateWindow(int index)
    {
        Int128 window = 0;

        if (index < 6)
        {
            var first = index < 3 ? index * 3 : 9 + (index - 3) * 3;

            for (var i = 0; i < 3; i++)
            {
                window |= HouseMasks[first + i];
            }

            return window;
        }

        var houses = 2 + _random.Next(3);

        for (var i = 0; i < houses; i++)
        {
            window |= HouseMasks[_random.Next(27)];
        }

        return window;
    }

    private void SeedWindowSets()
    {
        for (var w = 0; w < WindowCount; w++)
        {
            var window = CreateWindow(w);

            Int128 fixedCells = 0;

            while (true)
            {
                for (var i = 0; i < 81; i++)
                {
                    _window[i] = (window & CellBits[i]) != 0 && (fixedCells & CellBits[i]) == 0 ? 0 : _solution[i];
                }

                if (! _solver.TryFindAlternative(_window, _solution, out var differences))
                {
                    break;
                }

                var set = Minimise(differences);

                if (Int128.PopCount(set) <= 12)
                {
                    AddSet(set);
                }

                var skip = _random.Next((int) Int128.PopCount(set));

                var bits = set;

                for (var i = 0; i < skip; i++)
                {
                    bits &= bits - 1;
                }

                fixedCells |= CellBits[(int) Int128.TrailingZeroCount(bits)];
            }
        }
    }

    public bool CreateSolvedPuzzle(Span<int> puzzle, CancellationToken cancellationToken)
    {
        // var grid = "829463751561897423734215689495726318672381594318549276957638142146952837283174965"
        //     .Select(c => c - '0')
        //     .ToArray();
        //
        // for (var i = 0; i < 81; i++)
        // {
        //     puzzle[i] = grid[i];
        // }
        //
        // return true;

        for (var i = 0; i < 81; i++)
        {
            puzzle[i] = 0;
        }
        
        InitialiseCandidates();

        return CreateSolvedPuzzle(puzzle, 0, cancellationToken);
    }

    private bool CreateSolvedPuzzle(Span<int> puzzle, int cell, CancellationToken cancellationToken)
    {
        if (cancellationToken.IsCancellationRequested)
        {
            return false;
        }

        if (cell == 81)
        {
            return true;
        }

        var offset = cell * 9;

        var count = _candidateCounts[cell];

        while (count > 0)
        {
            var candidateIndex = _random.Next(count);

            var candidate = _candidates[offset + candidateIndex];

            _candidates[offset + candidateIndex] = _candidates[offset + count - 1];

            count--;

            puzzle[cell] = candidate;

            if (puzzle.IsValidSudoku(cell) && CreateSolvedPuzzle(puzzle, cell + 1, cancellationToken))
            {
                return true;
            }
        }

        puzzle[cell] = 0;

        for (var i = 0; i < 9; i++)
        {
            _candidates[offset + i] = i + 1;
        }

        _candidateCounts[cell] = 9;

        return false;
    }

    private void InitialiseCandidates()
    {
        for (var i = 0; i < 81; i++)
        {
            for (var j = 0; j < 9; j++)
            {
                _candidates[i * 9 + j] = j + 1;
            }

            _candidateCounts[i] = 9;
        }
    }
}