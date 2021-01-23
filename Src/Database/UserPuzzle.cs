using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace KyudosudokuWebsite.Database
{
    public sealed class UserPuzzle
    {
        [Key, Column(Order = 1)]
        public int UserID { get; set; }
        [Key, Column(Order = 2)]
        public int PuzzleID { get; set; }

        public string Progess { get; set; }
        public bool Solved { get; set; }
        public int Time { get; set; }
    }
}
