using System;
using System.ComponentModel.DataAnnotations;

namespace KyudosudokuWebsite.Database
{
    public sealed class Session
    {
        [Key]
        public string SessionID { get; set; }
        public int? UserID { get; set; }
        public DateTime LastLogin { get; set; }
    }
}
