using System.Text;

namespace Sudoku.Console;

public static class TreeGenerator
{
    private const string Numbers = "➊➋➌➍➎➏➐➑➒";
    
    private const string NodeTemplate = "<li><a class='{class}' href='#'><div class='cellTitle'>{type}</div>{puzzle}</a>{children}</li>"; 
        
    public static void Generate(int[] puzzle, string filename)
    {
        var solver = new Solver(HistoryType.AllSteps, SolveMethod.FindUnique);

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

        puzzle.Append("<table cellspacing='0'><tr>");
        
        for (var i = 0; i < 81; i++)
        {
            if (i > 0)
            {
                if (i % 9 == 0)
                {
                    puzzle.Append("</tr><tr>");
                }
            }

            var x = i % 9;

            var y = i / 9;

            var box = y / 3 * 3 + x / 3;

            var isGuess = node.Move.Type != MoveType.None && i == node.Move.X + node.Move.Y * 9;

            var radius = i switch
            {
                0 or 6 or 30 or 54 or 60 => " tl",
                2 or 8 or 32 or 56 or 62 => " tr",
                18 or 24 or 48 or 72 or 78 => " bl",
                20 or 26 or 50 or 74 or 80 => " br",
                _ => string.Empty
            };
            
            if (box % 2 == 0)
            {
                if (isGuess)
                {
                    puzzle.Append($"<td class='guess{radius}' style='background-color: #e0e0e0;'>");
                }
                else
                {
                    puzzle.Append($"<td class='{radius}' style='background-color: #e0e0e0;'>");
                }
            }
            else
            {
                if (isGuess)
                {
                    puzzle.Append("<td class='guess'>");
                }
                else
                {
                    puzzle.Append("<td>");
                }
            }

            if (node[i] != 0)
            {
                if (isGuess)
                {
                    puzzle.Append($"<span class='added'>{Numbers[node[i] - 1]}</span>");
                }
                else
                {
                    puzzle.Append(node[i]);
                }
            }
            else
            {
                if (node.Move.Type == MoveType.NoCandidates && node.Move.X == x && node.Move.Y == y)
                {
                    puzzle.Append("<span class='added'>ⓧ</span>");
                }
                else
                {
                    puzzle.Append("<pre>&nbsp;</pre>");
                }
            }

            puzzle.Append("</td>");
        }

        puzzle.Append("</tr></table>");

        content = content.Replace("{puzzle}", puzzle.ToString());

        if (! node.OnSolvedPath && node.Children.Count == 0 && node.Move.Type != MoveType.NoCandidates)
        {
            content = content.Replace("{class}", "deadEnd").Replace("{type}", "Unsolvable");
        }
        else
        {
            switch (node.Move.Type)
            {
                case MoveType.None:
                    content = content.Replace("{class}", "solvePath").Replace("{type}", node.Children.Count > 0 ? "Puzzle" : "Answer");
                    break;
                    
                case MoveType.Guess:
                    content = content.Replace("{class}", "guess").Replace("{type}", "Guess");
                    break;

                case MoveType.LastPossibleNumber:
                    content = content.Replace("{class}", "lastPossible").Replace("{type}", "Last Candidate");
                    break;
                
                case MoveType.NoCandidates:
                    content = content.Replace("{class}", "deadEnd").Replace("{type}", $"No Candidate");
                    break;

                default:
                    content = content.Replace("{class}", node.OnSolvedPath ? "solvePath" : string.Empty).Replace("{type}", "Hidden Single");
                    break;
            }
        }

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
                
                solved.AddChild(new Move(0, 0, 0, MoveType.None), true);
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