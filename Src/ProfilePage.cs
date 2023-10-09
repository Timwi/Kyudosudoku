using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Globalization;
using System.Linq;
using KyudosudokuWebsite.Database;
using RT.Json;
using RT.Servers;
using RT.TagSoup;
using RT.Util.ExtensionMethods;

namespace KyudosudokuWebsite
{
    partial class KyudosudokuPropellerModule
    {
        private HttpResponse profilePage(HttpRequest req) => withSession(req, (session, db) =>
        {
            var linkUserIdStr = req.Url.Path.Length == 0 ? "" : req.Url.Path.Substring(1);
            if (!int.TryParse(linkUserIdStr, out int linkUserId) || linkUserId < 0)
                return page404(req);

            var linkUser = db.Users.FirstOrDefault(user => user.UserID == linkUserId);

            if (linkUser == null)
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

            return RenderPage(null, session.User, new PageOptions { AddFooter = true, Db = db, Resources = { Resource.FindCss, Resource.ProfileCss, Resource.ProfileJs } },
                new DIV { class_ = "main" }._(
                    new DIV { class_ = "profile-container" }.Data("userid", linkUser.UserID).Data("month", DateTime.UtcNow.Month).Data("year", DateTime.UtcNow.Year)._(
                        new DIV { class_ = "left" }._(
                            new H1($"{linkUser.Username}’s profile"),
                            new H2($"Puzzles solved: {db.UserPuzzles.Count(puzzle => puzzle.UserID == linkUserId && puzzle.Solved)}"),
                            new UL(
                                new LI($"Puzzle solve times better than the average: {db.UserPuzzles.Count(puzzle => puzzle.UserID == linkUserId && puzzle.Solved && puzzle.Time < db.Puzzles.FirstOrDefault(p => p.PuzzleID == puzzle.PuzzleID).AverageTime)}"),
                                new LI($"Puzzle solve times worse than the average: {db.UserPuzzles.Count(puzzle => puzzle.UserID == linkUserId && puzzle.Solved && puzzle.Time > db.Puzzles.FirstOrDefault(p => p.PuzzleID == puzzle.PuzzleID).AverageTime)}"),
                                new LI($"Puzzle solve times equal to the average: {db.UserPuzzles.Count(puzzle => puzzle.UserID == linkUserId && puzzle.Solved && puzzle.Time == db.Puzzles.FirstOrDefault(p => p.PuzzleID == puzzle.PuzzleID).AverageTime)}"))),
                        new DIV { class_ = "chart-container" }._(
                            new BUTTON { class_ = "btn", id = "leftArrow" }._("◀"),
                            new H1 { id = "date-text" }._(DateTime.UtcNow.ToString("MMMM yyyy")),
                            new BUTTON { class_ = "btn", id = "rightArrow" }._("▶"),
                            new DIV { class_ = "chart" }._(profileActivityTable(db, DateTime.UtcNow.Year, DateTime.UtcNow.Month, linkUserId)))),
                    recentPuzzles.Length == 0 ? null : new H2("Latest puzzles:"),
                    recentPuzzles.Length == 0 ? null : new DIV { id = "results" }._(GeneratePuzzleTable(recentPuzzles, recentPuzzles.Length, PuzzleTableType.Solved, sortable: false))));
        });

        private object profileActivityTable(Db db, int year, int month, int linkUserId)
        {
            int dowInt(DayOfWeek dow) => dow switch { DayOfWeek.Sunday => 6, _ => (int)dow - 1 };

            var startDate = new DateTime(year, month, 1, 0, 0, 0, DateTimeKind.Utc);
            var endDate = startDate.AddMonths(1);

            var data = db.UserPuzzles.Where(up => up.UserID == linkUserId && up.Solved)
                .Where(up => up.SolveTime >= startDate && up.SolveTime < endDate)
                .GroupBy(up => new { up.SolveTime.Day, up.SolveTime.Month, up.SolveTime.Year })
                .Select(gr => new { gr.Key.Day, Count = gr.Count() })
                .ToArray();

            var calendar = new GregorianCalendar(GregorianCalendarTypes.USEnglish);
            var firstDowInMonth = dowInt(calendar.GetDayOfWeek(new DateTime(year, month, 1)));
            var numDays = calendar.GetDaysInMonth(year, month);
            var numRows = (numDays + firstDowInMonth + 6) / 7;

            var table = new int?[7 * numRows];
            for (int cell = firstDowInMonth, day = 1; day <= numDays; cell++, day++)
                table[cell] = data.FirstOrDefault(inf => inf.Day == day)?.Count ?? 0;

            string getColorValue(int numSolvedToday) => numSolvedToday == 0
                ? "background-color: hsl(220, 50%, 90%); color: black;"
                : $"background-color: rgba(0, 100, 0, {(numSolvedToday * .1 + .5).ClipMax(1)})";

            return new TABLE(
                new TR(
                    new TH(),
                    new TH("Mo"),
                    new TH("Tu"),
                    new TH("We"),
                    new TH("Th"),
                    new TH("Fr"),
                    new TH("Sa"),
                    new TH("Su")),
                Enumerable.Range(0, numRows).Select(wk => new TR(
                    new TD { class_ = "week-text" }._($"Week {wk + 1}"),
                    Enumerable.Range(0, 7)
                        .Select(day => table[7 * wk + day])
                        .Select(numSolved => numSolved == null ? new TD { class_ = "non-day" } : new TD { class_ = "day-square", style = getColorValue(numSolved.Value) }._(numSolved)))));
        }

        private HttpResponse profilePageActivity(HttpRequest req) => withSession(req, (session, db) =>
        {
            var linkUser = int.Parse(req.Post["userId"].Value);
            var month = int.Parse(req.Post["month"].Value);
            var year = int.Parse(req.Post["year"].Value);

            return HttpResponse.Json(new JsonDict
            {
                ["dateText"] = new DateTime(year, month, 1, 0, 0, 0, DateTimeKind.Utc).ToString("MMMM yyyy"),
                ["html"] = Tag.ToString(profileActivityTable(db, year, month, linkUser))
            });
        });
    }
}
