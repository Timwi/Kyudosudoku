using System;
using System.Data.Entity;
using System.Linq;
using KyudosudokuWebsite.Database;
using RT.Json;
using RT.Servers;
using RT.TagSoup;
using RT.Util;
using RT.Util.ExtensionMethods;

namespace KyudosudokuWebsite
{
    partial class KyudosudokuPropellerModule
    {
        private HttpResponse findPuzzlesPage(HttpRequest req) => withSession(req, (session, db) =>
        {
            if (session == null || session.User == null)
                return RenderPage("Not logged in", null, new PageOptions { StatusCode = HttpStatusCode._401_Unauthorized }, new DIV { class_ = "main" }._(new H1("You are not logged in.")));

            return RenderPage(null, session.User, new PageOptions { AddFooter = true, Db = db, Resources = { Resource.FindJs, Resource.FindCss } },
                new DIV { class_ = "main" }._(
                    new FORM { method = method.get, id = "find-form" }.Data("constraints", KyuConstraint.Constraints.OrderBy(c => c.name).ToJsonList(elem => new JsonDict { ["name"] = elem.name, ["id"] = elem.type.Name }).ToString())._(
                        new H1("Find puzzles"),
                        new DIV { class_ = "controls" }._(
                            "Find puzzles that I’ve: ",
                            new SPAN { class_ = "inner" }._(
                                new INPUT { class_ = "trigger", type = itype.radio, name = "what", value = "solved", id = "solved", accesskey = "1", checked_ = true },
                                new LABEL { for_ = "solved" }._("solved"),
                                new INPUT { class_ = "trigger", type = itype.radio, name = "what", value = "started", id = "started", accesskey = "2" },
                                new LABEL { for_ = "started" }._("started"),
                                new INPUT { class_ = "trigger", type = itype.radio, name = "what", value = "not-seen", id = "not-seen", accesskey = "3" },
                                new LABEL { for_ = "not-seen" }._("not seen"))),
                        new DIV { class_ = "controls" }._(
                            "Average time between: ",
                            new SPAN { class_ = "inner" }._(
                                new INPUT { class_ = "trigger", id = "filter-avg-min", type = itype.number, name = "filteravgmin", min = "0", value = "0" }, " and ",
                                new INPUT { class_ = "trigger", id = "filter-avg-max", type = itype.number, name = "filteravgmax", min = "0", value = "86400" }, " minutes")),
                        new DIV { class_ = "controls" }._("Include constraints: ", new SPAN { id = "include-constraints", class_ = "inner" }),
                        new DIV { class_ = "controls" }._("Exclude constraints: ", new SPAN { id = "exclude-constraints", class_ = "inner" }),
                        new DIV { id = "results" })));
        });

        private HttpResponse findPuzzlesHandler(HttpRequest req) => withSession(req, (session, db) =>
        {
            const int numPuzzlesPerPage = 20;

            var json = JsonDict.Parse(req.Post["criteria"].Value);
            var nu = session.User == null;

            var puzzles = (nu
                ? db.Puzzles.Select(p => new { Puzzle = p, UserPuzzle = (UserPuzzle) null })
                : db.Puzzles.Select(p => new { Puzzle = p, UserPuzzle = db.UserPuzzles.FirstOrDefault(up => up.PuzzleID == p.PuzzleID && up.UserID == session.User.UserID) }))
                .Select(inf => new { inf.Puzzle, inf.UserPuzzle, SolveCount = db.UserPuzzles.Count(up => up.PuzzleID == inf.Puzzle.PuzzleID && up.Solved) });


            /* FILTERS */

            var what = json["what"].GetString();
            if (session.User != null)
            {
                switch (what)
                {
                    case "solved":
                        puzzles = puzzles.Where(inf => inf.UserPuzzle != null && inf.UserPuzzle.Solved);
                        break;
                    case "started":
                        puzzles = puzzles.Where(inf => inf.UserPuzzle != null && !inf.UserPuzzle.Solved);
                        break;
                    case "not-seen":
                        puzzles = puzzles.Where(inf => inf.UserPuzzle == null);
                        break;
                }
            }

            var filteravgmin = json["filteravgmin"].GetIntSafe();
            var filteravgmax = json["filteravgmax"].GetIntSafe();
            if (filteravgmin != null)
                puzzles = puzzles.Where(inf => inf.Puzzle.AverageTime == null || inf.Puzzle.AverageTime.Value >= filteravgmin.Value * 60);
            if (filteravgmax != null)
                puzzles = puzzles.Where(inf => inf.Puzzle.AverageTime == null || inf.Puzzle.AverageTime.Value < (filteravgmax.Value + 1) * 60);

            JsonList lst;
            if ((lst = json.Safe["constraints"].Safe["include-constraints"].GetListSafe()) != null)
            {
                var strs = lst.Select(v => v.GetStringSafe().NullOr(s => $"%<{s}>%")).ToArray();
                foreach (var val in strs)
                    if (val != null)
                        puzzles = puzzles.Where(p => DbFunctions.Like(p.Puzzle.ConstraintNames, val));
            }
            if ((lst = json.Safe["constraints"].Safe["exclude-constraints"].GetListSafe()) != null)
            {
                var strs = lst.Select(v => v.GetStringSafe().NullOr(s => $"%<{s}>%")).ToArray();
                foreach (var val in strs)
                    if (val != null)
                        puzzles = puzzles.Where(p => !DbFunctions.Like(p.Puzzle.ConstraintNames, val));
            }

            var count = puzzles.Count();
            var pageNum = json.Safe["page"].GetIntSafe() ?? 0;
            var pageCount = Math.Max(1, (count + numPuzzlesPerPage - 1) / numPuzzlesPerPage);
            if (pageNum >= pageCount)
                pageNum = pageCount - 1;


            /* SORT */

            var asc = json["asc"].GetBool();
            puzzles = json["sort"].GetString() switch
            {
                "puzzleId" => asc ? puzzles.OrderBy(p => p.Puzzle.PuzzleID) : puzzles.OrderByDescending(p => p.Puzzle.PuzzleID),
                "avg" => asc ? puzzles.OrderBy(p => p.Puzzle.AverageTime) : puzzles.OrderByDescending(p => p.Puzzle.AverageTime),
                "solves" => asc ? puzzles.OrderBy(p => p.SolveCount) : puzzles.OrderByDescending(p => p.SolveCount),
                "your-time" => nu ? puzzles : asc ? puzzles.OrderBy(p => p.UserPuzzle.Time) : puzzles.OrderByDescending(p => p.UserPuzzle.Time),
                "solvetime" => nu ? puzzles : asc ? puzzles.OrderBy(p => p.UserPuzzle.SolveTime) : puzzles.OrderByDescending(p => p.UserPuzzle.SolveTime),
                _ => puzzles
            };

            puzzles.OrderByDescending(inf => inf.Puzzle.AverageTime).ThenBy(inf => inf.Puzzle.PuzzleID);


            /* PAGINATION */

            puzzles = puzzles.Skip(pageNum * numPuzzlesPerPage).Take(numPuzzlesPerPage);


            /* GENERATE HTML */

            static object time(Puzzle puzzle) => Ut.NewArray<object>(
                puzzle.AverageTime == null ? "🕛" : "🕛🕐🕑🕒🕓🕔🕕🕖🕗🕘🕙🕚".Substring(((int) puzzle.AverageTime.Value) / 60 % 60 / 5 * 2, 2),
                " ", formatTime(puzzle.AverageTime));

            static object solveTime(UserPuzzle userPuzzle) => userPuzzle == null
                ? "🕛 —"
                : new[] { "🕛🕐🕑🕒🕓🕔🕕🕖🕗🕘🕙🕚".Substring(userPuzzle.Time / 60 % 60 / 5 * 2, 2), " ", formatTime(userPuzzle.Time) };

            static object lastSeen(UserPuzzle userPuzzle) => Ut.NewArray<object>(
                "😎 ",
                userPuzzle == null ? "—" : userPuzzle.SolveTime.Year == 1900 ? "(not recorded)" : userPuzzle.SolveTime.ToString("d MMM yyyy, H:mm"));

            return HttpResponse.Json(new JsonDict
            {
                ["html"] = Tag.ToString(count == 0 ? new P("No results.") : Ut.NewArray(

                    // Big version of the table for desktop view
                    new TABLE { class_ = "big" }._(
                        new TR { class_ = "headers" }._(
                            new TH { class_ = "nowrap" }._(new A { href = "#", class_ = "sorter" }._("Puzzle").Data("sort", "puzzleId")),
                            new TH { class_ = "nowrap" }._(new A { href = "#", class_ = "sorter" }._("Average time").Data("sort", "avg")),
                            new TH { class_ = "nowrap" }._(new A { href = "#", class_ = "sorter" }._("# solves").Data("sort", "solves")),
                            what != "solved" ? null : new TH { class_ = "nowrap" }._(new A { href = "#", class_ = "sorter" }._("Your time").Data("sort", "your-time")),
                            what == "not-seen" ? null : new TH { class_ = "nowrap" }._(new A { href = "#", class_ = "sorter" }._(what == "solved" ? "When solved" : "Last seen").Data("sort", "solvetime")),
                            new TH("Constraints")),
                        puzzles.AsEnumerable().Select(inf => new TR(
                            new TD { class_ = "nowrap" }._("▶ ", new A { href = $"/puzzle/{inf.Puzzle.PuzzleID}" }._("Puzzle #", inf.Puzzle.PuzzleID)),
                            new TD { class_ = "nowrap" }._(time(inf.Puzzle)),
                            new TD { class_ = "nowrap" }._(inf.SolveCount),
                            what != "solved" ? null : new TD { class_ = "nowrap" }._(solveTime(inf.UserPuzzle)),
                            what == "not-seen" ? null : new TD { class_ = "nowrap" }._(lastSeen(inf.UserPuzzle)),
                            new TD(inf.Puzzle.ConstraintNames == null || inf.Puzzle.ConstraintNames.Length == 0 ? "(none)" : inf.Puzzle.ConstraintNames.Substring(1, inf.Puzzle.ConstraintNames.Length - 2).Split("><").Select(cn => KyuConstraint.Constraints.FirstOrDefault(tup => tup.type.Name == cn).name).JoinString(", "))))),

                    // Small version of the table for mobile view
                    new TABLE { class_ = "small" }._(
                        new TR { class_ = "headers" }._(
                            new TH { class_ = "nowrap" }._(new A { href = "#", class_ = "sorter" }._("Puzzle").Data("sort", "puzzleId")),
                            new TH { class_ = "nowrap" }._(new A { href = "#", class_ = "sorter" }._("Average time").Data("sort", "avg"))),
                        puzzles.AsEnumerable().Select(inf => new TR(
                            new TD("▶ ", new A { href = $"/puzzle/{inf.Puzzle.PuzzleID}" }._("Puzzle #", inf.Puzzle.PuzzleID),
                                //new DIV { class_ = "constraints" }._(
                                //    inf.Puzzle.ConstraintNames == null || inf.Puzzle.ConstraintNames.Length == 0 ? "(no constraints)" :
                                //    inf.Puzzle.ConstraintNames.Substring(1, inf.Puzzle.ConstraintNames.Length - 2).Split("><")
                                //        .Select(cn => new SPAN { class_ = $"constr {cn}", title = KyuConstraint.Constraints.FirstOrDefault(tup => tup.type.Name == cn).name }))),
                                new DIV { class_ = "constraints" }._(
                                    inf.Puzzle.ConstraintNames == null || inf.Puzzle.ConstraintNames.Length == 0 ? "(no constraints)" :
                                    inf.Puzzle.ConstraintNames.Substring(1, inf.Puzzle.ConstraintNames.Length - 2).Split("><")
                                        .Select(cn => KyuConstraint.Constraints.FirstOrDefault(tup => tup.type.Name == cn).name).Order().InsertBetween(", "))),
                            new TD { class_ = "nowrap" }._(
                                new DIV { class_ = "solve-count" }._("Solved: ", inf.SolveCount == 0 ? "never" : inf.SolveCount == 1 ? "once" : $"{inf.SolveCount} times"),
                                new DIV { class_ = "avg" }._("Average time: ", time(inf.Puzzle)),
                                what != "solved" ? null : new DIV { class_ = "solve-time" }._("Your time: ", solveTime(inf.UserPuzzle)),
                                what == "not-seen" ? null : new DIV { class_ = "last-seen" }._(what == "solved" ? "When: " : "Seen: ", lastSeen(inf.UserPuzzle)))))))),

                ["pageNum"] = pageNum,
                ["pageCount"] = pageCount
            });
        });
    }
}
