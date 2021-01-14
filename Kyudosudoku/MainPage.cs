using System.Collections.Generic;
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
        private HttpResponse MainPage(HttpRequest req) => withSession(req, (session, db) =>
        {
            var unfinishedPuzzleIds = session.User == null ? null : db.UserPuzzles.Where(up => up.UserID == session.User.UserID && !up.Solved).Select(up => up.PuzzleID).ToArray();

            // Get three random puzzles the user hasn’t solved yet
            var infs = (session.User == null ? db.Puzzles : db.Puzzles.Where(p => !db.UserPuzzles.Any(up => up.UserID == session.User.UserID && up.PuzzleID == p.PuzzleID))).Select(p => p.PuzzleID).ToList();
            var randomPuzzleIds = new List<int>();
            while (randomPuzzleIds.Count < 3 && infs.Count > 0)
            {
                var ix = Rnd.Next(0, infs.Count);
                randomPuzzleIds.Add(infs[ix]);
                infs.RemoveAt(ix);
            }

            // Find their average times
            var averages = db.UserPuzzles
                .Where(up => (randomPuzzleIds.Contains(up.PuzzleID) || unfinishedPuzzleIds.Contains(up.PuzzleID)) && up.Solved)
                .GroupBy(up => up.PuzzleID)
                .Select(gr => new { gr.Key, Average = gr.Average(up => up.Time) })
                .ToDictionary(inf => inf.Key, inf => inf.Average);

            // Potentially choose some random numbers for new puzzles
            while (randomPuzzleIds.Count < 3)
            {
                var rnd = Rnd.Next(0, 1000);
                while (db.Puzzles.Any(p => p.PuzzleID == rnd))
                    rnd += Rnd.Next(0, 1000);
                randomPuzzleIds.Add(rnd);
            }

            return RenderPageTagSoup(null, session.User, isPuzzlePage: false,
                  new H1("Try these puzzles:"),
                  new DIV { class_ = "choice" }._(randomPuzzleIds.Select(pzId =>
                    new A { href = $"/puzzle/{pzId}" }._(
                        new DIV { class_ = "puzzle-id" }._("Puzzle", new BR(), $"#{pzId}"),
                        new DIV { class_ = "average-time" }._(new SPAN { class_ = "label" }._("Average time"), averages.TryGetValue(pzId, out var time) ? $"{(time + 30) / 60} min" : "unknown")))),
                  unfinishedPuzzleIds.Length == 0 ? null : Ut.NewArray<object>(
                      new H1("Finish these puzzles:"),
                      new DIV { class_ = "choice" }._(unfinishedPuzzleIds.Select(pzId =>
                        new A { href = $"/puzzle/{pzId}" }._(
                            new DIV { class_ = "puzzle-id" }._("Puzzle", new BR(), $"#{pzId}"),
                            new DIV { class_ = "average-time" }._(new SPAN { class_ = "label" }._("Average time"), averages.TryGetValue(pzId, out var time) ? $"{(time + 30) / 60} min" : "unknown"))))));
        });
    }
}
