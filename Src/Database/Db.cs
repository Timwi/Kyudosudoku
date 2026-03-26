using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;

namespace KyudosudokuWebsite.Database
{
    public sealed class Db : DbContext
    {
        public static string ConnectionString { get; set; }

        protected override void OnConfiguring(DbContextOptionsBuilder options)
            => options.UseSqlServer(ConnectionString);

        public DbSet<Puzzle> Puzzles { get; set; }
        public DbSet<User> Users { get; set; }
        public DbSet<UserPuzzle> UserPuzzles { get; set; }
        public DbSet<Session> Sessions { get; set; }

        public double? CalculateAveragePuzzleTime(int puzzleId)
        {
            var q = Database.SqlQueryRaw<int>("""
                DECLARE @c BIGINT = (SELECT COUNT(*) FROM UserPuzzles WHERE PuzzleID=@puzzleId AND Solved=1);
                SELECT Time FROM UserPuzzles
                	WHERE PuzzleID=@puzzleId AND Solved=1
                    ORDER BY Time
                    OFFSET (@c - 1) / 2 ROWS
                    FETCH NEXT 1 + (1 - @c % 2) ROWS ONLY
                """, new SqlParameter("@puzzleId", puzzleId)).ToArray();
            if (q.Length > 0)
                return q.Average();
            return null;
        }
    }
}
