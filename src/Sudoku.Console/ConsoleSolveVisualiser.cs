using System.Text;
using Out = System.Console;

namespace Sudoku.Console;

public class ConsoleSolveVisualiser
{
    private readonly int[] _puzzle;

    private List<Move> _history;

    public ConsoleSolveVisualiser(int[] puzzle, List<Move> history)
    {
        _puzzle = puzzle;
        
        _history = history;
    }

    public void Visualise()
    {
        Out.CursorVisible = false;
        
        Out.Clear();
        
        DrawBox(1, 2);

        Out.CursorVisible = true;
    }

    private void DrawBox(int left, int top)
    {
        Out.CursorTop = top++;

        Out.CursorLeft = left;
        
        Out.Write("┏━━━━━┯━━━━━┯━━━━━┳━━━━━┯━━━━━┯━━━━━┳━━━━━┯━━━━━┯━━━━━┓");

        for (var i = 0; i < 3; i++)
        {
            for (var j = 0; j < 3; j++)
            {
                for (var k = 0; k < 3; k++)
                {
                    Out.CursorTop = top++;

                    Out.CursorLeft = 1;

                    Out.Write("┃     │     │     ┃     │     │     ┃     │     │     ┃");
                }

                if (j < 2)
                {
                    Out.CursorTop = top++;

                    Out.CursorLeft = 1;

                    Out.Write("┠─────┼─────┼─────╂─────┼─────┼─────╂─────┼─────┼─────┨");
                }
            }

            if (i < 2)
            {
                Out.CursorTop = top++;

                Out.CursorLeft = 1;

                Out.Write("┣━━━━━┿━━━━━┿━━━━━╋━━━━━┿━━━━━┿━━━━━╋━━━━━┿━━━━━┿━━━━━┫");
            }
        }

        Out.CursorTop = top;

        Out.CursorLeft = 1;
        
        Out.Write("┗━━━━━┷━━━━━┷━━━━━┻━━━━━┷━━━━━┷━━━━━┻━━━━━┷━━━━━┷━━━━━┛ ");
    }
}