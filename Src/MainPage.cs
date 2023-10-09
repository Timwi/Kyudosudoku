using System;
using System.Linq;
using KyudosudokuWebsite.Database;
using RT.Servers;
using RT.TagSoup;
using RT.Util;
using RT.Util.ExtensionMethods;
using SvgPuzzleConstraints;

namespace KyudosudokuWebsite
{
    partial class KyudosudokuPropellerModule
    {
        private static readonly NewsItem[] _news = Ut.NewArray
        (
            new NewsItem
            {
                Date = new DateTime(2023, 10, 9),
                Title = "Two new constraints: German Whisper and Means",
                MessageHtml = Ut.NewArray<object>(
                    new P("We have introduced two new variety Sudoku constraints!"),
                    new P("The German Whisper line may well be familiar to seasoned Sudoku solvers. Simply keep adjacent digits at a difference of at least 5! Sounds simple, but can give rise to deceptively intricate deductions."),
                    new P("The Means constraint is a novel idea. It regulates how many of the digits surrounding a cell can cause the constrained cell to be their arithmetic mean ((a+b)/2) or geometric mean (√(ab)). Something for the maths enthusiasts!"),
                    new P("To try out these new constraints, check out puzzles #", new A { href = "/puzzle/3034" }._("3034"), " and #", new A { href = "/puzzle/3045" }._("3045"), "."))
            },

            new NewsItem
            {
                Date = new DateTime(2021, 6, 15),
                Title = "Graphics changes and a new constraint",
                MessageHtml = Ut.NewArray<object>(
                    new P("Hello everyone! I’m proud to announce the following changes and enhancements."),
                    new OL(
                        new LI("We have a new variety constraint: the Skyscraper Sum! Check out the ", new A { href = "/constraints" }._("constraints"), " page or dive right in and ", new A { href = "/find" }._("find a puzzle"), "."),
                        new LI("I remade the graphics for the Skyscraper clue. The old one frankly looked like a blocky candle."),
                        new LI("The constraint that was previously called “Capped Line” is now called “Between Line” which is what it is generally called in the world of variety Sudoku. The graphics have also been updated to look like their usual representation (two circles connected by a line)."),
                        new LI("The Palindrome constraint has been changed to look the way that Capped Line used to. Its double-struck line looking like an equals sign, and its two arrows pointing in opposite directions, seem to fit the idea of a palindrome much better."),
                        new LI("The front page will now always offer one random puzzle with no variety constraints. Hopefully this provides an easier intro to new players.")),
                    new P("Enjoy!"))
            },

            new NewsItem
            {
                Date = new DateTime(2021, 6, 15),
                Title = "News section started",
                MessageHtml = new P("This is the beginning of this News page.")
            }
        );

        private HttpResponse mainPage(HttpRequest req) => withSession(req, (session, db) =>
        {
            var unfinishedPuzzles = session.User == null ? new Puzzle[0] : db.Puzzles.Where(p => db.UserPuzzles.Any(up => up.UserID == session.User.UserID && up.PuzzleID == p.PuzzleID && !up.Solved)).ToArray();
            var unfinishedPuzzleIds = unfinishedPuzzles.Select(p => p.PuzzleID).ToArray();
            var unsolvedPuzzles = (session.User == null ? db.Puzzles : db.Puzzles.Where(p => !db.UserPuzzles.Any(up => up.UserID == session.User.UserID && up.PuzzleID == p.PuzzleID))).Where(p => !unfinishedPuzzleIds.Contains(p.PuzzleID)).ToArray();
            var tmpUnsolvedPuzzles = unsolvedPuzzles.Shuffle().OrderBy(p => !string.IsNullOrEmpty(p.ConstraintNames)).ToArray();
            var showUnsolvedPuzzles = tmpUnsolvedPuzzles.Take(1).Concat(tmpUnsolvedPuzzles.Subarray((tmpUnsolvedPuzzles.Length - 2).ClipMin(0))).Distinct().ToArray();

            static object puzzleBox(Puzzle pz) => new A { href = $"/puzzle/{pz.PuzzleID}" }._(
                new DIV { class_ = "puzzle-id" }._("Puzzle", new BR(), $"#{pz.PuzzleID}"),
                new DIV { class_ = "average-time" }._(new SPAN { class_ = "label" }._("Average time"), formatTime(pz.AverageTime)),
                new DIV { class_ = "constraints" }._(string.IsNullOrWhiteSpace(pz.ConstraintNames) ? new EM("(no variety constraints)") : pz.ConstraintNames.Substring(1, pz.ConstraintNames.Length - 2).Split("><").Select(id => SvgConstraint.Constraints.First(c => c.type.Name == id).name).Distinct().Order().Select(name => new SPAN(name)).InsertBetween<object>(", ")));

            return RenderPage(null, session.User, new PageOptions { AddFooter = true, Db = db },
                new DIV { class_ = "main" }._(
                    session.User != null ? null : new DIV { class_ = "warning" }._(new STRONG("You are not logged in."), " Your puzzle progress is only saved to your local browser. If you log in with an account, the website can restore your puzzle progress across multiple devices and keep track of which puzzles you’ve already solved."),
                    _news.FirstOrDefault(n => (DateTime.UtcNow - n.Date).TotalDays < 30).NullOr(n => new DIV { id = "news" }._(new DIV { class_ = "date" }._(n.Date.ToString("yyyy-MMM-dd")), new DIV { class_ = "title" }._(new A { href = "/news" }._(n.Title)))),
                    showUnsolvedPuzzles.Length == 0 && unfinishedPuzzles.Length == 0
                        ? new P("Looks like you’ve solved all puzzles on this website. Come back in an hour and a wild new puzzle may appear!")
                        : Ut.NewArray<object>(
                            new H1("Try these puzzles:"),
                            new DIV { class_ = "choice" }._(showUnsolvedPuzzles.Select(puzzleBox)),
                            unfinishedPuzzles.Length == 0 ? null : Ut.NewArray<object>(
                                new H1("Finish these puzzles:"),
                                new DIV { class_ = "choice" }._(unfinishedPuzzles.Shuffle().Take(3).Select(puzzleBox))))));
        });

        private HttpResponse newsPage(HttpRequest req) => withSession(req, (session, db) =>
        {
            return RenderPage(null, session.User, new PageOptions { AddFooter = true, Db = db },
                new DIV { class_ = "main" }._(
                    new H1("News"),
                    _news.Select(n => new SECTION { class_ = "news-item" }._(
                        new DIV { class_ = "date" }._(n.Date.ToString("yyyy-MMM-dd")),
                        new H2(n.Title),
                        new DIV { class_ = "message" }._(n.MessageHtml)))));
        });

        private static string formatTime(double? seconds)
        {
            if (seconds == null)
                return "unkn.";
            var val = (int) seconds.Value;
            return val >= 3600 ? $"{val / 3600} h {(val / 60) % 60} min" : $"{val / 60} min";
        }
    }
}
