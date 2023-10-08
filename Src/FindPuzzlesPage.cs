using System;
using System.Data.Entity;
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
        private HttpResponse findPuzzlesPage(HttpRequest req) => withSession(req, (session, db) =>
        {
            return RenderPage(null, session.User, new PageOptions { AddFooter = true, Db = db, Resources = { Resource.FindJs, Resource.FindCss } },
                new DIV { class_ = "main" }._(
                    new FORM { method = method.get, id = "find-form" }.Data("constraints", SvgConstraint.Constraints.OrderBy(c => c.name).Where(c => ConstraintGenerator.All.Any(g => g.type.Equals(c.type))).ToJsonList(elem => new JsonDict { ["name"] = elem.name, ["id"] = elem.type.Name }).ToString())._(
                        new H1("Find puzzles"),
                        session == null || session.User == null ? null : new DIV { class_ = "controls" }._(
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

            var what = "not-seen";
            if (session.User != null)
            {
                what = json["what"].GetString();
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
            {
                var min = filteravgmin.Value.ClipMin(0).ClipMax(86400) * 60;
                puzzles = puzzles.Where(inf => inf.Puzzle.AverageTime == null || inf.Puzzle.AverageTime.Value >= min);
            }
            if (filteravgmax != null)
            {
                var max = (filteravgmax.Value.ClipMin(0).ClipMax(86400) + 1) * 60;
                puzzles = puzzles.Where(inf => inf.Puzzle.AverageTime == null || inf.Puzzle.AverageTime.Value < max);
            }

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
                        puzzles = puzzles.Where(p => p.Puzzle.ConstraintNames == null || !DbFunctions.Like(p.Puzzle.ConstraintNames, val));
            }

            var count = puzzles.Count();
            var pageNum = json.Safe["page"].GetIntSafe() ?? 0;
            var pageCount = Math.Max(1, (count + numPuzzlesPerPage - 1) / numPuzzlesPerPage);
            if (pageNum >= pageCount)
                pageNum = pageCount - 1;


            /* SORT */

            var asc = json["asc"].GetBool();
            var sortedPuzzles = json["sort"].GetString() switch
            {
                "avg" => asc ? puzzles.OrderBy(p => p.Puzzle.AverageTime) : puzzles.OrderByDescending(p => p.Puzzle.AverageTime),
                "solves" => asc ? puzzles.OrderBy(p => p.SolveCount) : puzzles.OrderByDescending(p => p.SolveCount),
                "your-time" => nu ? puzzles.OrderBy(p => p.Puzzle.PuzzleID) : asc ? puzzles.OrderBy(p => p.UserPuzzle.Time) : puzzles.OrderByDescending(p => p.UserPuzzle.Time),
                "solvetime" => nu ? puzzles.OrderBy(p => p.Puzzle.PuzzleID) : asc ? puzzles.OrderBy(p => p.UserPuzzle.SolveTime) : puzzles.OrderByDescending(p => p.UserPuzzle.SolveTime),
                "numconstr" => asc ? puzzles.OrderBy(p => p.Puzzle.NumConstraints) : puzzles.OrderByDescending(p => p.Puzzle.NumConstraints),
                _ /* puzzleId */ => asc ? puzzles.OrderBy(p => p.Puzzle.PuzzleID) : puzzles.OrderByDescending(p => p.Puzzle.PuzzleID)
            };


            /* PAGINATION */

            puzzles = sortedPuzzles.Skip(pageNum * numPuzzlesPerPage).Take(numPuzzlesPerPage);
            var results = puzzles.AsEnumerable().Select(inf => new PuzzleResultInfo(inf.Puzzle, inf.UserPuzzle, inf.SolveCount));


            /* GENERATE HTML */

            return HttpResponse.Json(new JsonDict
            {
                ["html"] = Tag.ToString(GeneratePuzzleTable(results, count,
                    what switch { "solved" => PuzzleTableType.Solved, "started" => PuzzleTableType.Started, _ => PuzzleTableType.NotSeen }, sortable: true)),
                ["pageNum"] = pageNum,
                ["pageCount"] = pageCount
            });
        });
    }
}
