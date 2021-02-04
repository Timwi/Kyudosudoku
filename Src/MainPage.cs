using System;
using System.Linq;
using KyudosudokuWebsite.Database;
using RT.Servers;
using RT.TagSoup;
using RT.Util;
using RT.Util.ExtensionMethods;

namespace KyudosudokuWebsite
{
    partial class KyudosudokuPropellerModule
    {
        private HttpResponse mainPage(HttpRequest req) => withSession(req, (session, db) =>
        {
            var unfinishedPuzzles = session.User == null ? new Puzzle[0] : db.Puzzles.Where(p => db.UserPuzzles.Any(up => up.UserID == session.User.UserID && up.PuzzleID == p.PuzzleID && !up.Solved)).ToArray();
            var unsolvedPuzzles = (session.User == null ? db.Puzzles : db.Puzzles.Where(p => !db.UserPuzzles.Any(up => up.UserID == session.User.UserID && up.PuzzleID == p.PuzzleID))).ToArray();

            static object puzzleBox(Puzzle pz) => new A { href = $"/puzzle/{pz.PuzzleID}" }._(
                new DIV { class_ = "puzzle-id" }._("Puzzle", new BR(), $"#{pz.PuzzleID}"),
                new DIV { class_ = "average-time" }._(new SPAN { class_ = "label" }._("Average time"),
                    pz.AverageTime == null ? "unkn." :
                    pz.AverageTime.Value >= 60 ? $"{(int) Math.Round(pz.AverageTime.Value / 3600)} h {((int) Math.Round(pz.AverageTime.Value / 60)) % 60} min" :
                    $"{(int) Math.Round(pz.AverageTime.Value / 60)} min"));

            return RenderPageTagSoup(null, session.User, null,
                session.User != null ? null : new DIV { class_ = "warning" }._(new STRONG("You are not logged in."), " Your puzzle progress is only saved to your local browser. If you log in with an account, the website can restore your puzzle progress across multiple devices and keep track of which puzzles you’ve already solved."),
                new H1("Try these puzzles:"),
                new DIV { class_ = "choice" }._(unsolvedPuzzles.Shuffle().Take(3).Select(puzzleBox)),
                unfinishedPuzzles.Length == 0 ? null : Ut.NewArray<object>(
                    new H1("Finish these puzzles:"),
                    new DIV { class_ = "choice" }._(unfinishedPuzzles.Shuffle().Take(3).Select(puzzleBox))),
                new HR(),
                new P("Send feedback and suggestions to Timwi#0551 or Goofy#1262 on Discord, or post a ticket to ", new A { href = "https://github.com/Timwi/Kyudosudoku/issues" }._("Kyudosudoku on GitHub"), "."),
                new P(new A { href = "https://legal.timwi.de" }._("Legal stuff · Impressum · Datenschutzerklärung")));
        });
    }
}
