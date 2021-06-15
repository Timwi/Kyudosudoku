using System;
using System.Collections.Generic;
using System.Linq;
using KyudosudokuWebsite.Database;
using PuzzleSolvers;
using RT.CommandLine;
using RT.Json;
using RT.Serialization;
using RT.Util;
using RT.Util.Consoles;
using RT.Util.ExtensionMethods;
using SvgPuzzleConstraints;

namespace KyudosudokuWebsite
{
    class Program
    {
        [STAThread]
        static int Main(string[] args)
        {
            try
            {
                return CommandLineParser.Parse<CommandLineBase>(args).Execute();
            }
            catch (CommandLineParseException ex)
            {
                ex.WriteUsageInfoToConsole();
                return 1;
            }
        }

        private static void Temp()
        {
            Db.ConnectionString = @"Server=CORNFLOWER;Database=Kyudosudoku;Trusted_Connection=True;"; // local
            //Db.ConnectionString = @"Server=SAPPHIRE\SQLEXPRESS;Database=Kyudosudoku;Trusted_Connection=True;"; // live site
            using var db = new Db();
            var puzzleIds = new HashSet<int>();
            var count = 0;
            foreach (var up in db.UserPuzzles.Where(u => u.Solved))
            {
                using var db2 = new Db();
                var gridsStr = db2.Puzzles.First(p => p.PuzzleID == up.PuzzleID).KyudokuGrids;
                var grids = Enumerable.Range(0, 4).Select(corner => gridsStr.Substring(36 * corner, 36).Select(ch => ch - '0').ToArray()).ToArray();
                var state = JsonDict.Parse(up.Progess);
                var circled = state["circledDigits"].GetList().Select(gr => gr.GetList().Select(v => v?.GetBool() ?? false).ToArray()).ToArray();
                for (var cell = 0; cell < 81; cell++)
                {
                    var kyCells = Enumerable.Range(0, 4)
                        .Where(c => cell % 9 >= 3 * (c % 2) && cell % 9 < 6 + 3 * (c % 2) && cell / 9 >= 3 * (c / 2) && cell / 9 < 6 + 3 * (c / 2))
                        .Select(c => (corner: c, kyCell: cell % 9 - 3 * (c % 2) + 6 * ((cell / 9) - 3 * (c / 2))))
                        .ToArray();

                    foreach (var (t1, t2) in kyCells.UniquePairs())
                    {
                        if ((grids[t1.corner][t1.kyCell] == grids[t2.corner][t2.kyCell]) &&
                            (circled[t1.corner][t1.kyCell] != circled[t2.corner][t2.kyCell]))
                            Console.WriteLine($"Puzzle {up.PuzzleID}, Cell {cell}, ky {t1.corner}/{t1.kyCell} and {t2.corner}/{t2.kyCell}, digits {grids[t1.corner][t1.kyCell]}/{grids[t2.corner][t2.kyCell]}");
                        puzzleIds.Add(up.PuzzleID);
                        count++;
                    }
                }
            }
            Console.WriteLine($"count = {count}, puzzles = {puzzleIds.Count}");
        }

        private static void RunStatistics()
        {
            var lockObj = new object();
            var seedCounter = 1000;
            var stats = new Dictionary<string, int>();
            var arrowLengthStats = new Dictionary<int, int>();
            var inclusionNumStats = new Dictionary<int, int>();
            var killerCageSizeStats = new Dictionary<int, int>();
            var killerCageSumStats = new Dictionary<int, int>();
            var renbanCageSizeStats = new Dictionary<int, int>();
            var palindromeSizeStats = new Dictionary<int, int>();
            var thermometerSizeStats = new Dictionary<int, int>();
            var cappedLineSizeStats = new Dictionary<int, int>();

            //var json = JsonValue.Parse(File.ReadAllText(@"D:\temp\kyudo-stats.json"));
            //var strings = @"AntiBishop,AntiKing,AntiKnight,NoConsecutive,OddEven,,,Arrow,KillerCage,Palindrome,RenbanCage,Snowball,Thermometer,,,Battlefield,Binairo,Sandwich,Skyscraper,ToroidalSandwich,XSum,,,Battenburg,Clockface,ConsecutiveNeighbors,DoubleNeighbors,Inclusion,LittleKiller".Split(',');
            //Clipboard.SetText(strings.Select(s => string.IsNullOrWhiteSpace(s) ? "" : json["Stats"][s].GetInt().ToString()).JoinString("\n"));
            //return;

            Enumerable.Range(0, Environment.ProcessorCount).ParallelForEach(proc =>
            {
                var seed = 0;
                while (true)
                {
                    lock (lockObj)
                    {
                        seed = seedCounter++;
                        Console.WriteLine($"Generating {seed}");
                    }
                    var puzzle = Kyudosudoku.Generate(seed);
                    lock (lockObj)
                    {
                        foreach (var constr in puzzle.Constraints)
                        {
                            stats.IncSafe(constr.GetType().Name);
                            if (constr is Arrow a) arrowLengthStats.IncSafe(a.Cells.Length - 1);
                            if (constr is Inclusion i) inclusionNumStats.IncSafe(i.Digits.Length);
                            if (constr is KillerCage kc) { killerCageSizeStats.IncSafe(kc.Cells.Length); killerCageSumStats.IncSafe(kc.Sum ?? -1); }
                            if (constr is RenbanCage rc) renbanCageSizeStats.IncSafe(rc.Cells.Length);
                            if (constr is Palindrome p) palindromeSizeStats.IncSafe(p.Cells.Length);
                            if (constr is Thermometer t) thermometerSizeStats.IncSafe(t.Cells.Length);
                            if (constr is CappedLine cl) cappedLineSizeStats.IncSafe(cl.Cells.Length);
                        }
                        ClassifyJson.SerializeToFile(new
                        {
                            Stats = stats,
                            ArrowLengths = arrowLengthStats,
                            InclusionNums = inclusionNumStats,
                            KillerCageSizes = killerCageSizeStats,
                            KillerCageSums = killerCageSumStats,
                            RenbanCageSizes = renbanCageSizeStats,
                            PalindromeSizes = palindromeSizeStats,
                            ThermometerSizes = thermometerSizeStats,
                            CappedLineSizeStats = cappedLineSizeStats
                        }, @"D:\temp\kyudo-stats.json");
                    }
                }
            });
        }

        private static int FindPuzzleWithConstraint()
        {
            var lockObj = new object();

            Db.ConnectionString = @"Server=CORNFLOWER;Database=Kyudosudoku;Trusted_Connection=True;";
            //var notFound = new HashSet<string> { "SkyscraperSum" };
            var stats = new Dictionary<string, int>();
            Enumerable.Range(2000, 3000).ParallelForEach(Environment.ProcessorCount, (seed, ix) =>
            {
                try
                {
                    using (var db = new Db())
                        if (db.Puzzles.Any(p => p.PuzzleID == seed))
                            return;
                    lock (lockObj)
                    {
                        //if (notFound.Count == 0)
                        //    return;
                        Console.CursorTop = 0;
                        Console.CursorLeft = 6 * ix;
                        Console.Write(seed);
                    }
                    var start = DateTime.UtcNow;
                    var puz = Kyudosudoku.Generate(seed);
                    var took = (DateTime.UtcNow - start).TotalSeconds;
                    lock (lockObj)
                    {
                        foreach (var lk in puz.Constraints)
                        {
                            var str = lk.GetType().Name;
                            stats.IncSafe(str);
                            //if (notFound.Contains(str))
                            //{
                            //    puz.SaveToDb(seed, null);
                            //    ConsoleUtil.WriteLine($" — {seed} has {str}".Color(ConsoleColor.White, ConsoleColor.DarkGreen));
                            //    notFound.Remove(str);
                            //}
                        }
                        Console.CursorTop = 2;
                        Console.CursorLeft = 0;
                        foreach (var kvp in stats.OrderByDescending(kvp => kvp.Value))
                            Console.WriteLine($"{kvp.Key} = {kvp.Value}{new string(' ', 100)}");
                        //if (notFound.Count == 0)
                        //    return;
                    }
                }
                catch (Exception e)
                {
                    lock (lockObj)
                        Console.WriteLine($"{seed}: {e.Message} ({e.GetType().FullName})".Color(ConsoleColor.Red));
                    throw;
                }
            });

            Console.WriteLine("Done.");
            Console.ReadLine();
            return 0;
        }

        private static void GenerateConstraintFromExampleSudoku()
        {
            var grid = @"687294531314586279295371684723615948146829753859437126462953817531768492978142365".Select(ch => ch - '0').ToArray();
            var total = 0;
            ConsoleUtil.WriteLine(new Sudoku().SolutionToConsole(grid));
            Console.WriteLine();

            foreach (CappedLine clue in CappedLine.Generate(grid))
            {
                //Console.WriteLine(ClassifyJson.Serialize(clue).ToString());
                //if (clue.Cells.Length == 15)
                //{
                //    var sudoku = new Sudoku()
                //        .AddConstraint(new LessThanConstraint(new[] { clue.Cells[0] }), ConsoleColor.White, ConsoleColor.DarkBlue)
                //        .AddConstraint(new LessThanConstraint(clue.Cells.Skip(1).SkipLast(1)), ConsoleColor.White, ConsoleColor.DarkGreen)
                //        .AddConstraint(new LessThanConstraint(new[] { clue.Cells.Last() }), ConsoleColor.White, ConsoleColor.DarkCyan);
                //    ConsoleUtil.WriteLine(sudoku.SolutionToConsole(grid));
                //    Console.WriteLine();
                //}
                total++;
            }
            Console.WriteLine($"Total: {total}");
            Console.ReadLine();
            return;
        }

        private static void FindBuggyConstraint()
        {
            var grid = @"687294531314586279295371684723615948146829753859437126462953817531768492978142365".Select(ch => ch - '0').ToArray();
            var total = 0;
            ConsoleUtil.WriteLine(new Sudoku().SolutionToConsole(grid));
            Console.WriteLine();

            foreach (CappedLine clue in CappedLine.Generate(grid))
            {
                var sudoku = new Sudoku()
                    .AddGivens(grid.Select((given, ix) => clue.Cells.Contains(ix) ? null : given.Nullable()).ToArray())
                    .AddConstraints(clue.GetConstraints());
                var solutions = sudoku.Solve().Take(2).ToArray();
                if (solutions.Length < 1)
                {
                    System.Diagnostics.Debugger.Break();
                }
                total++;
            }
            Console.WriteLine($"Total: {total}");
            Console.ReadLine();
            return;
        }
    }
}
