# Sudoku

A fast Sudoku library written in C#.

Features:

- Solver
- Generator
- Isomorph generator
- Canoniser

## Visualisations of Solving Process

Use + and - to zoom in and out. [Screenshot](screenshot.png).

- ["World's Hardest"](https://html-preview.github.io/?url=https://github.com/stevehjohn/Sudoku/blob/master/Visualisations/World%20Hardest.html)
- [Random 1](https://html-preview.github.io/?url=https://github.com/stevehjohn/Sudoku/blob/master/Visualisations/vis-1.html)
- [Random 2](https://html-preview.github.io/?url=https://github.com/stevehjohn/Sudoku/blob/master/Visualisations/vis-2.html)
- [Random 3](https://html-preview.github.io/?url=https://github.com/stevehjohn/Sudoku/blob/master/Visualisations/vis-3.html)

## Quick Usage

There are scripts in the root of the repo that will run the console app for you.

Windows: `run.bat` macOS/Linux `./run.sh`.

The console app looks like this:

```
 Please select the puzzle set to solve or an option below:

    1: 19 Clues
    2: 2 Million
    3: 20 Clues
    4: 21 Clues
    5: 22 Clues
    6: Benchmarks
    7: Diabolical
    8: Easy
    9: Euler
   10: Generated 2m x 25
   11: Generated 2m x 30
   12: Generated 2m x 37
   13: Generated 2m x 40
   14: Generated 2m x 50
   15: Generated
   16: Hard
   17: Least Steps
   18: Medium
   19: Minimum Clues
   20: Most Steps
   21: Top 50K Toughest
   22: World Hardest

    B: Generate tree branch diagram for last most steps
    V: Visualise last most steps
    L: See log of last most steps
    E: Enter manually
    T: Test suite
    G: Generate puzzles
    Q: Exit application
```

## Usage in Code

### Solving Puzzles

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

### Solver Timings

| Clues | puzzles/s |
|-------|-----------|
| 17    | 100,000   |
| 25    | 450,000   |
| 30    | 600,000   |
| 37    | 850,000   |
| 40    | 900,000   |

### Category Timings

| Clues             | puzzles/s |
|-------------------|-----------|
| Benchmarks        | 11,000    |
| Easy              | 550,000   |
| Medium            | 600,000   |
| Hard              | 400,000   |
| Diabolical        | 300,000   |
| Top 50k Hardest   | 100,000   |
| Minimum Clues     | 60,000    |
| Sample 2 million  | 15,000    |

World's hardest solved in 1,278μs.

### Generating Puzzles

```csharp
var generator = new Generator();

var puzzle = generator.Generate(30); // Will generate a puzzle with 30 clues. Can get quite slow below 20.

puzzle.DumpToConsole(1);
```
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

### Generator Timings

On my machine:

| Clues | puzzles/s | time/puzzle |
|-------|-----------|-------------|
| 18    |           | ~2 days     |
| 19    | -         | ~2 mins     |
| 20    | 1         |             |
| 21    | 30        |             |
| 22    | 500       |             |
| 23    | 2,800     |             |
| 24    | 9,000     |             |
| 25    | 14,500    |             |
| 26    | 20,000    |             |
| 27    | 24,500    |             |
| 28    | 27,000    |             |
| 29    | 29,500    |             |
| 30    | 32,000    |             |
| 31    | 34,500    |             |
| 32    | 36,500    |             |
| 33    | 37,500    |             |
| 34    | 39,000    |             |
| 35    | 41,500    |             |
| 36    | 42,500    |             |
| 37    | 44.500    |             |
| 38    | 45,000    |             |
| 39    | 48,000    |             |
| 40    | 50,000    |             |

Note these can be massively increased by generating isomorphs.