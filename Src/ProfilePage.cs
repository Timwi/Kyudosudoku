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

            var solvedPuzzles = db.UserPuzzles.Where(puzzle => puzzle.UserID == linkUserId && puzzle.Solved);

            var recentPuzzles = db.UserPuzzles.Where(puzzle => puzzle.UserID == linkUserId && puzzle.Solved).OrderByDescending(n => n.SolveTime).Take(10)
                .Select(up => db.Puzzles.FirstOrDefault(p => p.PuzzleID == up.PuzzleID)).ToArray();

            object renderRecentPuzzles(Puzzle pz) => new TR(
                new TD(pz.PuzzleID),
                new TD(pz.AverageTime),
                new TD(solvedPuzzles.First(puzzle => puzzle.PuzzleID == pz.PuzzleID).Time),
                new TD(pz.ConstraintNames == null || pz.ConstraintNames.Length == 0 ? "(none)" : $"{pz.NumConstraints}: {pz.ConstraintNames.Substring(1, pz.ConstraintNames.Length - 2).Split("><").Select(cn => SvgConstraint.Constraints.FirstOrDefault(tup => tup.type.Name == cn).name).JoinString(", ")}"));

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
                        new TABLE { class_ = "big" }._(
                            new TR { class_ = "headers" }._(
                                new TH { class_ = "nowrap" }._("Puzzle"),
                                new TH { class_ = "nowrap" }._("Average time"),
                                new TH { class_ = "nowrap" }._("Time"),
                                new TH { class_ = "nowrap" }._("Constraints")),
                            recentPuzzles.Select(renderRecentPuzzles)))));
        });
    }
}
