using System.Data.Entity;
using System.Data.Entity.Infrastructure;

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
    }
}
