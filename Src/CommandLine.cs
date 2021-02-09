using System;
using System.Linq;
using System.Reflection;
using KyudosudokuWebsite.Database;
using RT.CommandLine;
using RT.PostBuild;
using RT.PropellerApi;
using RT.Util;

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

    [CommandName("generate"), Documentation("Can be used by a scheduled task to generate new puzzles in the background. Each invocation will only generate at most one puzzle. Through repeated invocation, the number of unsolved puzzles can be kept at a desired number.")]
    sealed class GeneratePuzzles : CommandLineBase
    {
        [IsPositional, IsMandatory, Documentation("Maximum number of unsolved puzzles to keep in the database. The process will not generate any new puzzles if this number of unsolved puzzles is already in the database.")]
        public int MaxNumber = 100;

        [IsPositional, IsMandatory, Documentation("Database connection string.")]
        public string DbConnectionString = null;

        // generate 30 "Server=CORNFLOWER;Database=Kyudosudoku;Trusted_Connection=True;"
        public override int Execute()
        {
            Db.ConnectionString = DbConnectionString;
            var newPuzzleId = Rnd.Next(0, 1000);
            using (var db = new Db())
            {
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
            Kyudosudoku.Generate(newPuzzleId).SaveToDb(newPuzzleId);
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
