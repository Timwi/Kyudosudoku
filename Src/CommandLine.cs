using RT.CommandLine;
using RT.PostBuild;

namespace KyudosudokuWebsite
{
    [CommandLine]
    abstract class CommandLineBase
    {
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
    }

    [CommandName("postbuild"), Undocumented]
    sealed class PostBuild : CommandLineBase
    {
        [IsPositional, IsMandatory, Undocumented]
        public string SourcePath = null;
    }

    [CommandName("run"), Documentation("Runs a standalone Kyudosudoku server.")]
    sealed class Run : CommandLineBase
    {
    }
}
