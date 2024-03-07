# Sudoku

A fast Sudoku solver written in C#. Can also act as a Sudoku generator.

## Console Application

Clone this repository as usual. From a terminal in the root of the repository `./run.sh` for macOS or `.\run.bat` for Windows. Then just follow the on screen prompts.

What to expect: https://youtu.be/pvAZ--dLYH4

If there is only 1 puzzle to solve, it will display every evaluated step. Otherwise, it just shows each puzzle and its solution.

![Screenshot](screenshot.png)

## Usage in Code

```csharp
var puzzle = new int[81];

/*
  Fill the array with the puzzle here. Use 0 to represent empty cells.
  
  Just flatten the puzzle into one long sequence,
  so puzzle[0..8] is the first row, puzzle[9..17] is second and so on.
*/

var solver = new Solver();

var result = solver.Solve(puzzle);

for (var y = 0; y < 9; y++)
{
    for (var (x = 0; x < 9; x++)
    {
        Console.Write($"{result.Solution[y * 9 + x]} ");
    }
    
    Console.WriteLine();
}

// The result has additional informational properties.

result.Steps; // How many steps were explored to find the answer.

result.Microseconds; // How long it took to solve.

/*
  There is an option to get the order in which the answer was found.
  This isn't generated by default for performance reasons.
  If you would like the history, add true as a parameter to the Solve call...
*/

solver.Solve(puzzle, true);

foreach (var move in result.History)
{
    Console.WriteLine($"Row: {move.Y}    Column: {move.X}    Value: {move.Value}");        
}
```