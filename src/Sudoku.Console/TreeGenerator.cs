using System.Text;
using Sudoku.Console.Supporting_Files;

namespace Sudoku.Console;

public class TreeGenerator
{
    private const string NodeTemplate = "<li><a class='{class}' href='#'><pre>{puzzle}</pre></a>{children}</li>"; 
        
    public void Generate(int[] puzzle, string filename)
    {
        var solver = new Solver(HistoryType.AllSteps, true);

        var result = solver.Solve(puzzle);
        
        var root = GenerateNodes(puzzle, result);
        
        var visualisation = File.ReadAllText("src/Sudoku.Console/Supporting Files/Template.html");

        visualisation = visualisation.Replace("{css}", File.ReadAllText("src/Sudoku.Console/Supporting Files/Styles.css"));

        visualisation = visualisation.Replace("{nodes}", $"<ul>{ProcessNode(root)}</ul>");

        File.WriteAllText($"{filename}.html", visualisation);
    }

    private static string ProcessNode(Node node)
    {
        var content = NodeTemplate;

        var puzzle = new StringBuilder();

        for (var i = 0; i < 81; i++)
        {
            if (i % 9 == 0)
            {
                puzzle.Append("<br/>");
            }
            else
            {
                puzzle.Append(' ');
            }

            if (node[i] == 0)
            {
                puzzle.Append(' ');
            }
            else
            {
                puzzle.Append(node[i]);
            }
        }

        content = content.Replace("{puzzle}", puzzle.ToString());

        content = content.Replace("{class}", node.OnSolvedPath ? "solvePath" : string.Empty);

        if (node.Children.Count == 0)
        {
            content = content.Replace("{children}", string.Empty);
        }
        else
        {
            var children = new StringBuilder();

            foreach (var child in node.Children)
            {
                children.Append(ProcessNode(child));
            }
            
            content = content.Replace("{children}", $"<ul>{children}</ul>");
        }

        return content;
    }

    private static Node GenerateNodes(int[] puzzle, SudokuResult result)
    {
        var node = new Node(puzzle);

        var root = node;

        Node solved = node;
        
        foreach (var move in result.History)
        {
            if (move.Type == MoveType.Backtrack)
            {
                node = node.Parent;
                
                continue;
            }
            
            node = node.AddChild(move);

            if (node.Solved)
            {
                solved = node;
            }
        }

        while (solved.Parent != null)
        {
            solved.OnSolvedPath = true;

            solved = solved.Parent;
        }

        solved.OnSolvedPath = true;

        return root;
    }
}