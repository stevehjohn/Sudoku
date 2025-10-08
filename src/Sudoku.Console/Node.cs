namespace Sudoku.Console;

public class Node
{
    private readonly List<Node> _children = [];

    private readonly int[] _puzzleState = new int[81];
    
    public Move Move { get; }

    public Node Parent { get; }
    
    public bool OnSolvedPath { get; set; }

    public bool Solved => _puzzleState.All(cell => cell != 0);

    public IReadOnlyList<Node> Children => _children;

    public Node(int[] puzzleState)
    {
        Move = new Move(0, 0, 0, MoveType.None);
        
        for (var i = 0; i < 81; i++)
        {
            _puzzleState[i] = puzzleState[i];
        }
    }
    
    private Node(Move move, Node parent)
    {
        Move = move;

        Parent = parent;

        for (var i = 0; i < 81; i++)
        {
            _puzzleState[i] = parent._puzzleState[i];
        }

        if (move.Type is not (MoveType.None or MoveType.NakedPairRow or MoveType.NakedPairColumn or MoveType.NakedPairBox))
        {
            _puzzleState[move.X + move.Y * 9] = move.Type == MoveType.Backtrack ? 0 : move.Value;
        }
    }

    public int this[int index] => _puzzleState[index];

    public Node AddChild(Move child, bool onSolvedPath = false)
    {
        var node = new Node(child, this)
        {
            OnSolvedPath = onSolvedPath
        };
        
        _children.Add(node);

        return node;
    }

    public void ReorderChildren(bool left)
    {
        var solved = _children.Single(c => c.OnSolvedPath);

        _children.Remove(solved);
        
        if (left)
        {
            _children.Insert(0, solved);
        }
        else
        {
            _children.Add(solved);
        }
    }
}