using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using KyudosudokuWebsite.Database;
using RT.CommandLine;
using RT.PostBuild;
using RT.PropellerApi;
using RT.Serialization;
using RT.Util;
using RT.Util.Consoles;
using RT.Util.ExtensionMethods;
using SvgPuzzleConstraints;

namespace KyudosudokuWebsite
{
    [CommandLine]
    abstract class CommandLineBase
    {
        public abstract int Execute();

        public static void PostBuildCheck(IPostBuildReporter rep)
        {
            CommandLineParser.PostBuildStep<CommandLineBase>(rep, null);
        }
    }

    [CommandName("reeval"), DocumentationLiteral("Re-evaluates puzzles to check for redundant constraints.")]
    sealed class Reeval : CommandLineBase, ICommandLineValidatable
    {
        [IsPositional, IsMandatory, Documentation("Database connection string.")]
        public string DbConnectionString = null;

        [Option("-d"), Documentation("Specify a date. Only puzzles generated since that date are re-evaluated.")]
        public string SinceDate = null;

        public override int Execute()
        {
            Db.ConnectionString = DbConnectionString;
            var db = new Db();

            var since = SinceDate.NullOr(DateTime.Parse);
            var examine = since == null
                ? db.Puzzles.Where(p => !p.Invalid).ToArray()
                : db.Puzzles.Where(p => !p.Invalid && p.Generated != null && p.Generated.Value >= since).ToArray();
            Console.WriteLine($"Examining {examine.Length} puzzles:");
            string escape(string str) => str.Select(c => c == '\'' ? "''" : c.ToString()).JoinString();
            var sql = new List<string>();
            for (var exIx = 0; exIx < examine.Length; exIx++)
            {
                var dbPuzzle = examine[exIx];
                Console.Write($"Puzzle {dbPuzzle.PuzzleID} ({exIx}/{examine.Length})\r");

                var puzzle = new Kyudosudoku(dbPuzzle.KyudokuGrids.Split(36).Select(subgrid => subgrid.Select(ch => ch - '0').ToArray()).ToArray(),
                    dbPuzzle.Constraints == null ? new SvgConstraint[0] : ClassifyJson.Deserialize<SvgConstraint[]>(dbPuzzle.Constraints));

                try
                {
                    var result = puzzle.Reevaluate();

                    switch (result)
                    {
                        case Kyudosudoku.CanReduce cr:
                            ConsoleUtil.WriteLine(new ConsoleColoredString($@"{$"Puzzle {dbPuzzle.PuzzleID}:".Color(ConsoleColor.Yellow)} {puzzle.Constraints.Union(cr.NewConstraints).Select(c => $@"{(puzzle.Constraints.Contains(c) && cr.NewConstraints.Contains(c) ? "✓" : puzzle.Constraints.Contains(c) ? "✗" : "+")} {c.Name}".Color(puzzle.Constraints.Contains(c) && cr.NewConstraints.Contains(c) ? ConsoleColor.Green : puzzle.Constraints.Contains(c) ? ConsoleColor.Red : ConsoleColor.Yellow)).JoinColoredString(" | ".Color(ConsoleColor.DarkGray))}"));
                            sql.Add($"UPDATE Puzzles SET Constraints='{escape(ClassifyJson.Serialize(cr.NewConstraints).ToString())}', NumConstraints={cr.NewConstraints.Length}, ConstraintNames='{cr.NewConstraints.Select(c => $"<{c.GetType().Name}>").Distinct().Order().JoinString()}' WHERE PuzzleID={dbPuzzle.PuzzleID}");
                            break;

                        case Kyudosudoku.ReevaluateError re:
                            ConsoleUtil.WriteLine($"Puzzle {dbPuzzle.PuzzleID} error: {re.Error}".Color(ConsoleColor.Red));
                            break;
                    }
                }
                catch (Exception exception)
                {
                    ConsoleUtil.WriteLine($"Puzzle {dbPuzzle.PuzzleID} error:".Color(ConsoleColor.Magenta));
                    var e = exception;
                    var indent = 0;
                    while (e != null)
                    {
                        indent += 4;
                        ConsoleUtil.WriteLine($"{new string(' ', indent)}{e.Message} ({e.GetType().FullName})".Color(ConsoleColor.Red));
                        ConsoleUtil.WriteLine(e.StackTrace.Indent(indent).Color(ConsoleColor.DarkRed));
                        e = e.InnerException;
                    }
                }
            }
            Console.WriteLine();
            foreach (var s in sql)
                Console.WriteLine(s);
            return 0;
        }

        ConsoleColoredString ICommandLineValidatable.Validate() => SinceDate != null && !DateTime.TryParse(SinceDate, out _)
                ? new ConsoleColoredString($"{SinceDate.Color(ConsoleColor.White)} is not a valid date/time stamp.")
                : null;
    }

    [CommandName("generate"), Documentation("Can be used by a scheduled task to generate new puzzles in the background. Each invocation will only generate at most one puzzle. Through repeated invocation, the number of unsolved puzzles can be kept at a desired number.")]
    sealed class GeneratePuzzles : CommandLineBase
    {
        [IsPositional, IsMandatory, Documentation("If -s is not specified, maximum number of unsolved puzzles to keep in the database. The process will not generate any new puzzles if this number of unsolved puzzles is already in the database.")]
        public int MaxNumber = 100;

        [IsPositional, IsMandatory, Documentation("Database connection string.")]
        public string DbConnectionString = null;

        [Option("--specific", "-s"), Documentation("Specifies that the number is instead a specific puzzle ID to be generated.")]
        public bool Specific = false;

        // generate 30 "Server=CORNFLOWER;Database=Kyudosudoku;Trusted_Connection=True;"
        public override int Execute()
        {
            Db.ConnectionString = DbConnectionString;
            int newPuzzleId;
            if (Specific)
            {
                newPuzzleId = MaxNumber;
                using var db = new Db();
                if (db.Puzzles.Any(p => p.PuzzleID == newPuzzleId))
                {
                    Console.WriteLine($"Puzzle #{newPuzzleId} is already in the DB.");
                    return 1;
                }
            }
            else
            {
                newPuzzleId = Rnd.Next(0, 1000);
                using var db = new Db();
                // How many puzzles are in the DB that nobody has solved yet?
                var numUnsolvedPuzzles = db.Puzzles.Where(p => !db.UserPuzzles.Any(up => up.PuzzleID == p.PuzzleID && up.Solved)).Count();
                Console.WriteLine($"There are currently {numUnsolvedPuzzles} unsolved puzzles in the database.");
                if (numUnsolvedPuzzles >= MaxNumber)
                    return 0;

                // Choose a random number for a new puzzle
                while (db.Puzzles.Any(p => p.PuzzleID == newPuzzleId))
                    newPuzzleId += Rnd.Next(0, 1000);
            }
            Console.WriteLine($"Generating puzzle #{newPuzzleId}");
            var start = DateTime.UtcNow;
            var puzzle = Kyudosudoku.Generate(newPuzzleId);
            var took = (int) (DateTime.UtcNow - start).TotalSeconds;
            puzzle.SaveToDb(newPuzzleId, took);
            Console.WriteLine($"Took {took} seconds.");
            return 0;
        }
    }

    [CommandName("postbuild"), Undocumented]
    sealed class PostBuild : CommandLineBase
    {
        [IsPositional, IsMandatory, Undocumented]
        public string SourcePath = null;

        public override int Execute() => PostBuildChecker.RunPostBuildChecks(SourcePath, Assembly.GetExecutingAssembly());
    }

    [CommandName("run"), Documentation("Runs a standalone Kyudosudoku server.")]
    sealed class Run : CommandLineBase
    {
        [IsPositional]
        public string ConfigFile = null;

        public override int Execute()
        {
            PropellerUtil.RunStandalone(ConfigFile ?? @"D:\Daten\Config\Kyudosudoku.config.json", new KyudosudokuPropellerModule(),
#if DEBUG
                true
#else
                false
#endif
            );
            return 0;
        }
    }

    [CommandName("avg"), Documentation("Recalculates all average times for all puzzles.")]
    sealed class RecalculateAverages : CommandLineBase
    {
        [IsPositional, IsMandatory, Documentation("Database connection string.")]
        public string DbConnectionString = null;

        public override int Execute()
        {
            Console.WriteLine("Recalculating average times");
            Db.ConnectionString = DbConnectionString;
            using var db = new Db();
            foreach (var puzzle in db.Puzzles.ToArray())
                puzzle.AverageTime = db.CalculateAveragePuzzleTime(puzzle.PuzzleID);
            db.SaveChanges();
            return 0;
        }
    }
}
