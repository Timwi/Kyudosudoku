using System;
using System.Collections.Generic;
using System.Linq;
using KyudosudokuWebsite.Database;
using PuzzleSolvers;
using RT.CommandLine;
using RT.Json;
using RT.PostBuild;
using RT.PropellerApi;
using RT.Serialization;
using RT.Util;
using RT.Util.Consoles;
using RT.Util.ExtensionMethods;

namespace KyudosudokuWebsite
{
    class Program
    {
        [STAThread]
        static int Main(string[] args)
        {
            try
            {
                var result = CommandLineParser.Parse<CommandLineBase>(args);
                switch (result)
                {
                    case PostBuild pb:
                        return PostBuildChecker.RunPostBuildChecks(pb.SourcePath, System.Reflection.Assembly.GetExecutingAssembly());

                    // generate 30 "Server=CORNFLOWER;Database=Kyudosudoku;Trusted_Connection=True;"
                    case GeneratePuzzles gp:
                        Db.ConnectionString = gp.DbConnectionString;
                        return GenerateNewPuzzles(gp.MaxNumber);

                    case Run r:
                        PropellerUtil.RunStandalone(@"D:\Daten\Config\Kyudosudoku.config.json", new KyudosudokuPropellerModule(),
#if DEBUG
                            true
#else
                            false
#endif
                        );
                        return 0;
                }
            }
            catch (CommandLineParseException ex)
            {
                ex.WriteUsageInfoToConsole();
            }
            return 1;
        }

        private static int GenerateNewPuzzles(int maxNumber)
        {
            var newPuzzleId = Rnd.Next(0, 1000);
            using (var db = new Db())
            {
                // How many puzzles are in the DB that nobody has solved yet?
                var numUnsolvedPuzzles = db.Puzzles.Where(p => !db.UserPuzzles.Any(up => up.PuzzleID == p.PuzzleID && up.Solved)).Count();
                Console.WriteLine($"There are currently {numUnsolvedPuzzles} unsolved puzzles in the database.");
                if (numUnsolvedPuzzles >= maxNumber)
                    return 0;

                // Choose a random number for a new puzzle
                while (db.Puzzles.Any(p => p.PuzzleID == newPuzzleId))
                    newPuzzleId += Rnd.Next(0, 1000);
            }
            Console.WriteLine($"Generating puzzle #{newPuzzleId}");
            var puzzle = Kyudosudoku.Generate(newPuzzleId);
            using (var db = new Db())
            {
                db.Puzzles.Add(new Database.Puzzle
                {
                    PuzzleID = newPuzzleId,
                    KyudokuGrids = puzzle.Grids.SelectMany(grid => grid.Select(i => (char) (i + '0'))).JoinString(),
                    Constraints = ClassifyJson.Serialize(puzzle.Constraints).ToString(),
                    ConstraintNames = puzzle.Constraints.Select(c => $"<{c.GetType().Name}>").Distinct().Order().JoinString()
                });
                db.SaveChanges();
            }
            return 0;
        }
    }
}
