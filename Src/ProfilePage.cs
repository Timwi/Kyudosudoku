using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Data.Entity.Infrastructure;
using System.Diagnostics;
using System.Linq;
using System.Windows.Forms;
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

            string getColorValue(int dbQuery)
            {
                string tempString = "background-color: rgba(0, 100, 0, ";
                int solvedPuzzlesToday = dbQuery;

                tempString += (solvedPuzzlesToday / 10 + 0.5).ToString() + ")";

                if (solvedPuzzlesToday == 0) tempString = "background-color: hsl(220, 50%, 90%); color: black;";

                return tempString;
            }

            DateTime dayIndex = new DateTime(2023, 10, 1);
            IEnumerable<object> createDayTableRow(int week)
            {
                yield return new TD { class_ = "weekText" }._($"Week {week}");

                for (int i = 0; i < 7; i++)
                {
                    int dbQuery = db.UserPuzzles.Count(puzzle => puzzle.UserID == linkUserId && puzzle.Solved && puzzle.SolveTime.Year == DateTime.Now.Year && puzzle.SolveTime.Month == DateTime.Now.Month && puzzle.SolveTime.Day == dayIndex.Day);
                    yield return new TD() { class_ = "daySquare", style = getColorValue(dbQuery) }._(dbQuery.ToString());
                    dayIndex = dayIndex.AddDays(1);
                }
            }

            IEnumerable<object> createLastDayTableRow()
            {
                int currentMonth = DateTime.Now.Month;
                int loopEnd;
                
                switch(currentMonth)
                {
                    case 1:
                    case 3:
                    case 5:
                    case 7:
                    case 8:
                    case 10:
                    case 12:
                        loopEnd = 3;
                        break;
                    case 2:
                        loopEnd = 1;
                        break;
                    default:
                        loopEnd = 2;
                        break;
                }

                yield return new TD { class_ = "weekText" }._("Week 5");

                for(int i = 0; i < loopEnd; i++)
                {
                    int dbQuery = db.UserPuzzles.Count(puzzle => puzzle.UserID == linkUserId && puzzle.Solved && puzzle.SolveTime.Year == DateTime.Now.Year && puzzle.SolveTime.Month == DateTime.Now.Month && puzzle.SolveTime.Day == dayIndex.Day);
                    yield return new TD() { class_ = "daySquare", style = getColorValue(dbQuery) }._(dbQuery.ToString());
                    dayIndex = dayIndex.AddDays(1);
                }
            }

            return RenderPage(null, session.User, new PageOptions { AddFooter = true, Db = db, Resources = { Resource.FindCss, Resource.ProfileCSS } },
                new DIV { class_ = "main" }._(
                    new DIV { class_ = "profileContainer" }._(
                        new DIV { class_ = "left" }._(
                            new H1($"{db.Users.First(user => user.UserID == linkUserId).Username}'s profile"),
                            new H2($"Puzzles solved: {db.UserPuzzles.Count(puzzle => puzzle.UserID == linkUserId && puzzle.Solved)}"),
                            new UL(
                                new LI($"Puzzles above average: {db.UserPuzzles.Count(puzzle => puzzle.UserID == linkUserId && puzzle.Solved && puzzle.Time > db.Puzzles.FirstOrDefault(p => p.PuzzleID == puzzle.PuzzleID).AverageTime)}"),
                                new LI($"Puzzles below average: {db.UserPuzzles.Count(puzzle => puzzle.UserID == linkUserId && puzzle.Solved && puzzle.Time < db.Puzzles.FirstOrDefault(p => p.PuzzleID == puzzle.PuzzleID).AverageTime)}"),
                                new LI($"Puzzles equal the average: {db.UserPuzzles.Count(puzzle => puzzle.UserID == linkUserId && puzzle.Solved && puzzle.Time == db.Puzzles.FirstOrDefault(p => p.PuzzleID == puzzle.PuzzleID).AverageTime)}")
                            )
                        ),
                        new DIV { class_ = "chartContainer" }._(
                            new DIV { class_ = "chart" }._(
                                new H1("Recent activity"),
                                new TABLE(
                                    new TR(
                                        new TH(),
                                        new TH("Mo"),
                                        new TH("Tu"),
                                        new TH("We"),
                                        new TH("Th"),
                                        new TH("Fr"),
                                        new TH("Sa"),
                                        new TH("Su")
                                    ),
                                    new TR(
                                        createDayTableRow(1)
                                    ),
                                    new TR(
                                        createDayTableRow(2)
                                    ),
                                    new TR(
                                        createDayTableRow(3)
                                    ),
                                    new TR(
                                        createDayTableRow(4)
                                    ),
                                    DateTime.Now.Year % 4 != 0 ? new TR(
                                        createLastDayTableRow()
                                    ) : null
                                )
                            )
                        )
                    ),
                    recentPuzzles.Count() != 0 ? new H2($"Latest puzzles:") : null,
                    recentPuzzles.Count() != 0 ? new DIV { id = "results" }._(
                        GeneratePuzzleTable(recentPuzzles, recentPuzzles.Length, PuzzleTableType.Solved, sortable: false)
                    ) : null
                )
            );
        });
    }
}
