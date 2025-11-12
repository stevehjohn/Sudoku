// ReSharper disable AccessToDisposedClosure

using Sudoku.Extensions;

namespace Sudoku;

public static class BulkGenerator
{
    public static void Generate(int quantity, int cluesToLeave, int isomorphCount, Action<int[]> callback)
    {
        var workers = Math.Max(Environment.ProcessorCount / 2, 1);

        var count = 0;

        using var cancellationTokenSource = new CancellationTokenSource();

        var cancellationToken = cancellationTokenSource.Token;

        var tasks = new Task[workers];

        var callbackLock = new Lock();

        var solvedPuzzleReuseCount = cluesToLeave switch
        {
            < 19 => 1_000_000,
            20 or 21 => int.MaxValue,
            < 25 => 1_000,
            _ => 1
        };

        var totalQuantity = quantity * Math.Max(isomorphCount, 1);

        for (var i = 0; i < workers; i++)
        {
            var generator = new Generator();

            tasks[i] = Task.Run(() =>
            {
                var puzzleUsages = -1;

                var puzzle = new int[81];

                while (! cancellationToken.IsCancellationRequested)
                {
                    if (Volatile.Read(ref count) >= totalQuantity)
                    {
                        cancellationTokenSource.Cancel();

                        break;
                    }

                    if (puzzleUsages < 0 || puzzleUsages > solvedPuzzleReuseCount)
                    {
                        var solved = false;

                        while (! solved && ! cancellationToken.IsCancellationRequested)
                        {
                            solved = generator.CreateSolvedPuzzle(puzzle, cancellationToken);
                        }

                        puzzleUsages = 0;
                    }

                    var result = generator.Generate(puzzle, cluesToLeave, cancellationToken);

                    puzzleUsages++;

                    if (! result.Succeeded || cancellationToken.IsCancellationRequested)
                    {
                        continue;
                    }

                    if (Interlocked.Increment(ref count) > totalQuantity)
                    {
                        cancellationTokenSource.Cancel();

                        break;
                    }

                    lock (callbackLock)
                    {
                        callback(result.Puzzle);
                    }

                    if (isomorphCount > 0)
                    {
                        var isomorphs = IsomorphGenerator.CreateIsomorphs(result.Puzzle, isomorphCount, cancellationToken);

                        for (var isomorph = 0; isomorph < isomorphCount; isomorph++)
                        {
                            if (Interlocked.Increment(ref count) > totalQuantity)
                            {
                                cancellationTokenSource.Cancel();

                                break;
                            }
                            
                            lock (callbackLock)
                            {
                                callback(isomorphs[isomorph]);
                            }
                        }
                    }
                }
            }, CancellationToken.None);
        }

        Task.WaitAll(tasks, CancellationToken.None);
    }
}