# Sudoku

A fast Sudoku solver written in C#. Can also act as a Sudoku generator.

## Visualisations of Solving Process

Use + and - to zoom in and out. [Screenshot](screenshot.png).

- ["World's Hardest"](https://html-preview.github.io/?url=https://github.com/stevehjohn/Sudoku/blob/master/Visualisations/World%20Hardest.html)
- [Random 1](https://html-preview.github.io/?url=https://github.com/stevehjohn/Sudoku/blob/master/Visualisations/vis-1.html)
- [Random 2](https://html-preview.github.io/?url=https://github.com/stevehjohn/Sudoku/blob/master/Visualisations/vis-2.html)
- [Random 3](https://html-preview.github.io/?url=https://github.com/stevehjohn/Sudoku/blob/master/Visualisations/vis-3.html)

## Usage in Code

### Solving Sudokus

```csharp
var puzzle = new int[81];

/*
  Fill the array with the puzzle here. Use 0 to represent empty cells.
  
  Just flatten the puzzle into one long sequence,
  so puzzle[0..8] is the first row, puzzle[9..17] is second and so on.
*/

/*
  This is most performant.
  If you want:
   - the solution steps, select HistoryType.SolutionOnly.
   - all the steps attempted, select HistoryType.AllSteps.
   - to check the puzzle has only 1 solution, specify SolveMethod.FindUnique.
   - a count of the number of solutions, specify SolveMethod.FindAll.
*/

var solver = new Solver(HistoryType.None, SolveMethod.FindFirst);

var result = solver.Solve(puzzle);
        
/*
  Output to console with a left offset of 1.
  Store cursor y position to draw puzzle and solution side by side.
*/

var y = Console.CursorTop;

puzzle.DumpToConsole(1, y);
        
result.Solution.DumpToConsole(30, y);

// The result has additional informational properties.

result.Steps; // How many steps were explored to find the answer.

result.Microseconds; // How long it took to solve.

result.LogToConsole(); // Dump the steps taken to solve the puzzle.

foreach (var move in result.History)
{
    Console.WriteLine($"Row: {move.Y}    Column: {move.X}    Value: {move.Value}");        
}

Console.WriteLine(result.Message);
```

For an example of log output, see [here](Example%20Log.md).

Output:

```
 ┌───────┬───────┬───────┐    ┌───────┬───────┬───────┐
 │       │   2   │   9   │    │ 4 5 7 │ 8 2 6 │ 1 9 3 │
 │   6   │ 1     │       │    │ 9 6 3 │ 1 4 5 │ 2 8 7 │
 │     2 │ 7 3   │ 6     │    │ 8 1 2 │ 7 3 9 │ 6 5 4 │
 ├───────┼───────┼───────┤    ├───────┼───────┼───────┤
 │   3   │ 4     │ 8     │    │ 2 3 1 │ 4 5 7 │ 8 6 9 │
 │ 7 8   │ 6 9   │       │    │ 7 8 4 │ 6 9 1 │ 3 2 5 │
 │       │       │   4   │    │ 6 9 5 │ 2 8 3 │ 7 4 1 │
 ├───────┼───────┼───────┤    ├───────┼───────┼───────┤
 │     8 │       │       │    │ 1 7 8 │ 9 6 4 │ 5 3 2 │
 │     9 │       │   1   │    │ 5 2 9 │ 3 7 8 │ 4 1 6 │
 │       │ 5     │     8 │    │ 3 4 6 │ 5 1 2 │ 9 7 8 │
 └───────┴───────┴───────┘    └───────┴───────┴───────┘
 
 Solved
```

### Generating Sudokus

```csharp
var generator = new Generator();

var puzzle = generator.Generate(30); // Will generate a puzzle with 30 clues. Can get quite slow below 22.

puzzle.DumpToConsole(1);
```

### Timings

On my machine:

| Clues | puzzles/s | s/puzzle |
|-------|-----------|----------|
| 19    |           | 18,000   |
| 20    |           | 22       |
| 21    |           | 1.5      |
| 22    | 2         |          |
| 23    | 100       |          |
| 24    | 230       |          |
| 25    | 1,600     |          |
| 26    | 3,800     |          |
| 27    | 4,300     |          |
| 28    | 4,500     |          |
| 29    | 4,500     |          |
| 30    | 4,500     |          |
| 31    | 4,600     |          |
| 32    | 4,700     |          |
| 33    | 4,700     |          |
| 34    | 4,700     |          |
| 35    | 4,700     |          |
| 36    | 4,800     |          |
| 37    | 4,900     |          |
| 38    | 4,900     |          |
| 39    | 4,900     |          |
| 40    | 5,000     |          |

Output:

```
 ┌───────┬───────┬───────┐
 │   6   │     4 │   2 8 │
 │       │   8 9 │     6 │
 │ 9 5   │       │       │
 ├───────┼───────┼───────┤
 │     5 │       │       │
 │ 6   4 │       │       │
 │ 8     │ 4 2   │   7 3 │
 ├───────┼───────┼───────┤
 │     6 │       │     7 │
 │       │       │ 4 8   │
 │       │       │       │
 └───────┴───────┴───────┘
```