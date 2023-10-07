using System;
using System.Data.Entity;
using System.Diagnostics;
using System.Linq;
using KyudosudokuWebsite.Database;
using RT.Json;
using RT.Servers;
using RT.TagSoup;
using RT.Util;
using RT.Util.ExtensionMethods;
using SvgPuzzleConstraints;

namespace KyudosudokuWebsite
{
    partial class KyudosudokuPropellerModule
    {
        private HttpResponse profilePage(HttpRequest req) => withSession(req, (session, db) =>
        {
            var linkUserIdStr = req.Url.Path.Length == 0 ? "" : req.Url.Path.Substring(1);
            if (!int.TryParse(linkUserIdStr, out int linkUserId) || linkUserId < 0)
                return page404(req);

            if (!db.Users.Any(user => user.UserID == linkUserId))
                return page404(req);

            var recentPuzzles = db.UserPuzzles.Where(puzzle => puzzle.UserID == linkUserId && puzzle.Solved).OrderByDescending(n => n.SolveTime).Take(10)
                .Select(up => new
                {
                    UserPuzzle = up,
                    Puzzle = db.Puzzles.FirstOrDefault(p => p.PuzzleID == up.PuzzleID),
                    SolveCount = db.UserPuzzles.Count(up2 => up2.PuzzleID == up.PuzzleID && up2.Solved)
                })
                .AsEnumerable()
                .Select(inf => new PuzzleResultInfo(inf.Puzzle, inf.UserPuzzle, inf.SolveCount))
                .ToArray();

            return RenderPage(null, session.User, new PageOptions { AddFooter = true, Db = db, Resources = { Resource.FindCss } },
                new DIV { class_ = "main" }._(
                    new H1($"Profile of {db.Users.First(user => user.UserID == linkUserId).Username}"),
                    new H2($"Puzzles solved: {db.UserPuzzles.Count(puzzle => puzzle.UserID == linkUserId && puzzle.Solved)}"),
                    new UL(
                        new LI($"Puzzles above average: {db.UserPuzzles.Count(puzzle => puzzle.UserID == linkUserId && puzzle.Solved && puzzle.Time > db.Puzzles.FirstOrDefault(p => p.PuzzleID == puzzle.PuzzleID).AverageTime)}"),
                        new LI($"Puzzles below average: {db.UserPuzzles.Count(puzzle => puzzle.UserID == linkUserId && puzzle.Solved && puzzle.Time < db.Puzzles.FirstOrDefault(p => p.PuzzleID == puzzle.PuzzleID).AverageTime)}"),
                        new LI($"Puzzles equal the average: {db.UserPuzzles.Count(puzzle => puzzle.UserID == linkUserId && puzzle.Solved && puzzle.Time == db.Puzzles.FirstOrDefault(p => p.PuzzleID == puzzle.PuzzleID).AverageTime)}")),
                    new H2($"Latest puzzles:"),
                    new DIV { id = "results" }._(
                        GeneratePuzzleTable(recentPuzzles, recentPuzzles.Length, PuzzleTableType.Solved))));
        });
    }
}
