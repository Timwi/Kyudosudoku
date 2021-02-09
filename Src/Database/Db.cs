using System.Data.Entity;
using System.Data.Entity.Infrastructure;
using System.Data.SqlClient;
using System.Linq;

namespace KyudosudokuWebsite.Database
{
    public sealed class Db : DbContext
    {
        public static string ConnectionString;

        public Db() : base(ConnectionString)
        {
            // This is false by default, but it's very important to set this to true so we can use
            // LINQ to Entities with WHERE clauses with comparisons on variables that may be null.
            // (Without it, comparisons are translated to e.g. "<> NULL" (wrong!) instead of "IS NOT NULL".)
            ((IObjectContextAdapter) this).ObjectContext.ContextOptions.UseCSharpNullComparisonBehavior = true;
        }

        public DbSet<Puzzle> Puzzles { get; set; }
        public DbSet<User> Users { get; set; }
        public DbSet<UserPuzzle> UserPuzzles { get; set; }
        public DbSet<Session> Sessions { get; set; }

        public double? CalculateAveragePuzzleTime(int puzzleId)
        {
            var q = Database.SqlQuery<int>(@"
                DECLARE @c BIGINT = (SELECT COUNT(*) FROM UserPuzzles WHERE PuzzleID=@puzzleId AND Solved=1);
                SELECT Time FROM UserPuzzles
	                WHERE PuzzleID=@puzzleId AND Solved=1
                    ORDER BY Time
                    OFFSET (@c - 1) / 2 ROWS
                    FETCH NEXT 1 + (1 - @c % 2) ROWS ONLY
            ", new SqlParameter("@puzzleId", puzzleId)).ToArray();
            if (q.Length > 0)
                return q.Average();
            return null;
        }
    }
}
