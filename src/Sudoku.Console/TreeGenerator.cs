using Sudoku.Console.Supporting_Files;

namespace Sudoku.Console;

public class TreeGenerator
{
    private const string NodeTemplate = "<li><a href='#'>{move}<pre>{puzzle}</pre></a></li>"; 
        
    public void Generate(int[] puzzle, string filename)
    {
        var solver = new Solver(HistoryType.AllSteps, true);

        var result = solver.Solve(puzzle);
        
        var root = GenerateNodes(puzzle, result);
        
        var visualisation = File.ReadAllText("Supporting Files\\Template.html");

        visualisation = visualisation.Replace("{css}", File.ReadAllText("Supporting Files\\Styles.css"));

        visualisation = visualisation.Replace("{nodes}", $"<ul>{ProcessNode(root)}</ul>");

        File.WriteAllText($"{filename}.html", visualisation);
    }

    private static string ProcessNode(Node node)
    {
        var type = node.Move.Type switch
        {
            MoveType.HiddenSingle => "Hidden Single",
            MoveType.LastPossibleNumber => "Last Possible",
            _ => "Guess"
        };
        
        return NodeTemplate.Replace("{move}", type);
    }

    private Node GenerateNodes(int[] puzzle, SudokuResult result)
    {
        var node = new Node(puzzle);

        var root = node;
        
        foreach (var move in result.History)
        {
            if (move.Type == MoveType.Backtrack)
            {
                node = node.Parent;
                
                continue;
            }
            
            node = node.AddChild(move);
        }

        return root;
    }
}