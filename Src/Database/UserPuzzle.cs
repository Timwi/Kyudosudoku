using Microsoft.EntityFrameworkCore;

namespace KyudosudokuWebsite.Database
{
    [PrimaryKey("UserID", "PuzzleID")]
    public sealed class UserPuzzle
    {
        public int UserID { get; set; }
        public int PuzzleID { get; set; }
        public string Progess { get; set; }
        public bool Solved { get; set; }
        public int Time { get; set; }
        public DateTime SolveTime { get; set; }
    }
}
