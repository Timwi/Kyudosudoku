﻿using System.ComponentModel.DataAnnotations;

namespace KyudosudokuWebsite.Database
{
    public sealed class User
    {
        [Key]
        public int UserID { get; set; }
        public string Username { get; set; }
        public string PasswordHash { get; set; }
        public string EmailAddress { get; set; }

        // Game options
        public bool ShowErrors { get; set; }
        public bool SemitransparentXs { get; set; }
        public bool ShowSolveTime { get; set; }
        public bool PlayInvalidSound { get; set; }
    }
}
