using System;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading;
using KyudosudokuWebsite.Database;
using RT.Json;
using RT.Serialization;
using RT.Servers;
using RT.TagSoup;
using RT.Util;
using RT.Util.ExtensionMethods;

namespace KyudosudokuWebsite
{
    partial class KyudosudokuPropellerModule
    {
        private HttpResponse PuzzlePage(HttpRequest req, DbSession session, Db db)
        {
            Match m;
            if ((m = Regex.Match(req.Url.Path, @"^/db-update/(\d+)$")).Success && req.Method == HttpMethod.Post)
                return dbUpdate(req, session, db, int.Parse(m.Groups[1].Value));

            var puzzleIdStr = req.Url.Path.Length == 0 ? "" : req.Url.Path.Substring(1);
            if (!int.TryParse(puzzleIdStr, out int puzzleId) || puzzleId < 0)
                return page404(req);

            Kyudosudoku puzzle;

            getAgain:
            var dbPuzzle = db.Puzzles.FirstOrDefault(p => p.PuzzleID == puzzleId);
            if (dbPuzzle != null && dbPuzzle.Invalid)
                return HttpResponse.Empty(HttpStatusCode._500_InternalServerError);
            else if (dbPuzzle != null && dbPuzzle.KyudokuGrids == null)
            {
                Thread.Sleep(1000);
                goto getAgain;
            }
            else if (dbPuzzle == null)
            {
                try
                {
                    dbPuzzle = new Puzzle { PuzzleID = puzzleId, KyudokuGrids = null };
                    db.Puzzles.Add(dbPuzzle);
                    db.SaveChanges();
                }
                catch
                {
                    goto getAgain;
                }

                try
                {
                    puzzle = Kyudosudoku.Generate(puzzleId);   // potentially long operation
                }
                catch (Exception e)
                {
                    Log.Exception(e);
                    dbPuzzle.Invalid = true;
                    db.SaveChanges();
                    return HttpResponse.Empty(HttpStatusCode._500_InternalServerError);
                }
                dbPuzzle.KyudokuGrids = puzzle.Grids.Select(grid => grid.Select(i => (char) (i + '0')).JoinString()).JoinString();
                dbPuzzle.Constraints = ClassifyJson.Serialize(puzzle.Constraints).ToString();
                dbPuzzle.ConstraintNames = puzzle.Constraints.Select(c => $"<{c.GetType().Name}>").Distinct().Order().JoinString();
                db.SaveChanges();
            }
            else
            {
                puzzle = new Kyudosudoku(dbPuzzle.KyudokuGrids.Split(36).Select(subgrid => subgrid.Select(ch => ch - '0').ToArray()).ToArray(),
                    dbPuzzle.Constraints == null ? new KyuConstraint[0] : ClassifyJson.Deserialize<KyuConstraint[]>(JsonValue.Parse(dbPuzzle.Constraints)));
            }

            var userPuzzle = session.User == null ? null : db.UserPuzzles.FirstOrDefault(up => up.UserID == session.User.UserID && up.PuzzleID == puzzleId);

            var extraTop = puzzle.Constraints.MaxOrDefault(c => c.ExtraTop, 0);
            var extraRight = puzzle.Constraints.MaxOrDefault(c => c.ExtraRight, 0);

            return RenderPageTagSoup($"#{puzzleId}", session.User, new PageOptions { IsPuzzlePage = true, PuzzleID = puzzleId },
                session.User != null ? null : new DIV { class_ = "warning" }._(new STRONG("You are not logged in."), " If you log in with an account, the website can restore your puzzle progress across multiple devices and keep track of which puzzles you’ve solved."),
                new DIV { class_ = "puzzle", id = $"puzzle-{puzzleId}", tabindex = 0 }.Data("constraints", dbPuzzle.Constraints).Data("progress", userPuzzle.NullOr(up => up.Progess)).Data("showerrors", (session?.User?.ShowErrors ?? true) ? "1" : "0")._(
                    new RawTag($@"<svg viewBox='-.5 {-.5 - extraTop} {24 + extraRight} {13.75 + extraTop}' stroke-width='0' text-anchor='middle' font-family='Bitter' font-size='.65'>
                        <defs>
                            <linearGradient id='p-{puzzleId}-gradient' x1='0' y1='-1' x2='0' y2='1' gradientUnits='userSpaceOnUse'>
                                <stop stop-color='white' stop-opacity='1' offset='0'></stop> 
                                <stop stop-color='hsl(216, 70%, 75%)' stop-opacity='1' offset='1'></stop> 
                            </linearGradient>
                        </defs>
                        {Enumerable.Range(0, 4).Select(corner => kyudokuGridSvg(corner, puzzleId, puzzle.Grids[corner])).JoinString()}
                        <rect class='solve-glow frame' id='p-{puzzleId}-sudoku-frame' x='{Kyudosudoku.SudokuX}' y='{Kyudosudoku.SudokuY}' width='9' height='9' stroke-width='.2' fill='none' filter='blur(.1)' />
                        {Enumerable.Range(0, 81).Select(cell => $@"
                            <rect class='clickable sudoku-cell' id='p-{puzzleId}-sudoku-cell-{cell}' x='{cell % 9 + Kyudosudoku.SudokuX}' y='{cell / 9 + Kyudosudoku.SudokuY}' width='1' height='1' />
                            <text id='p-{puzzleId}-sudoku-text-{cell}' x='{cell % 9 + Kyudosudoku.SudokuX + .5}' y='{cell / 9 + Kyudosudoku.SudokuY + .725}'></text>
                            <text id='p-{puzzleId}-sudoku-center-text-{cell}' x='{cell % 9 + Kyudosudoku.SudokuX + .5}' y='{cell / 9 + Kyudosudoku.SudokuY + .625}' font-size='.3' fill='#1d6ae5'></text>
                            <text id='p-{puzzleId}-sudoku-corner-text-{cell}-0' x='{cell % 9 + Kyudosudoku.SudokuX + .1}' y='{cell / 9 + Kyudosudoku.SudokuY + .3}' font-size='.25' fill='#1d6ae5' text-anchor='start'></text>
                            <text id='p-{puzzleId}-sudoku-corner-text-{cell}-1' x='{cell % 9 + Kyudosudoku.SudokuX + .9}' y='{cell / 9 + Kyudosudoku.SudokuY + .3}' font-size='.25' fill='#1d6ae5' text-anchor='end'></text>
                            <text id='p-{puzzleId}-sudoku-corner-text-{cell}-2' x='{cell % 9 + Kyudosudoku.SudokuX + .1}' y='{cell / 9 + Kyudosudoku.SudokuY + .85}' font-size='.25' fill='#1d6ae5' text-anchor='start'></text>
                            <text id='p-{puzzleId}-sudoku-corner-text-{cell}-3' x='{cell % 9 + Kyudosudoku.SudokuX + .9}' y='{cell / 9 + Kyudosudoku.SudokuY + .85}' font-size='.25' fill='#1d6ae5' text-anchor='end'></text>
                            <text id='p-{puzzleId}-sudoku-corner-text-{cell}-4' x='{cell % 9 + Kyudosudoku.SudokuX + .5}' y='{cell / 9 + Kyudosudoku.SudokuY + .25}' font-size='.25' fill='#1d6ae5' text-anchor='middle'></text>
                            <text id='p-{puzzleId}-sudoku-corner-text-{cell}-5' x='{cell % 9 + Kyudosudoku.SudokuX + .9}' y='{cell / 9 + Kyudosudoku.SudokuY + .6125}' font-size='.25' fill='#1d6ae5' text-anchor='end'></text>
                            <text id='p-{puzzleId}-sudoku-corner-text-{cell}-6' x='{cell % 9 + Kyudosudoku.SudokuX + .5}' y='{cell / 9 + Kyudosudoku.SudokuY + .9}' font-size='.25' fill='#1d6ae5' text-anchor='middle'></text>
                            <text id='p-{puzzleId}-sudoku-corner-text-{cell}-7' x='{cell % 9 + Kyudosudoku.SudokuX + .1}' y='{cell / 9 + Kyudosudoku.SudokuY + .6125}' font-size='.25' fill='#1d6ae5' text-anchor='start'></text>
                        ").JoinString()}
                        {puzzle.Constraints.Where(c => !c.SvgAboveLines).Select(c => c.Svg).JoinString()}
                        {Enumerable.Range(0, 8).Select(i => $@"<line x1='{Kyudosudoku.SudokuX + i + 1}' y1='{Kyudosudoku.SudokuY}' x2='{Kyudosudoku.SudokuX + i + 1}' y2='{Kyudosudoku.SudokuY + 9}' stroke='black' stroke-width='{(i % 3 == 2 ? ".03" : ".01")}' />").JoinString()}
                        {Enumerable.Range(0, 8).Select(i => $@"<line x1='{Kyudosudoku.SudokuX}' y1='{Kyudosudoku.SudokuY + i + 1}' x2='{Kyudosudoku.SudokuX + 9}' y2='{Kyudosudoku.SudokuY + i + 1}' stroke='black' stroke-width='{(i % 3 == 2 ? ".03" : ".01")}' />").JoinString()}
                        <rect x='{Kyudosudoku.SudokuX}' y='{Kyudosudoku.SudokuY}' width='9' height='9' stroke='black' stroke-width='.05' fill='none' />
                        {puzzle.Constraints.Where(c => c.SvgAboveLines).Select(c => c.Svg).JoinString()}
                                
                        {Enumerable.Range(0, 9).Select(btn => $@"
                            <g class='button' id='p-{puzzleId}-num-{btn + 1}' transform='translate({Kyudosudoku.ControlsX + (.8 + 9 * .2 / 8) * btn}, {Kyudosudoku.ControlsY})'>
                                <rect class='clickable' x='0' y='0' width='.8' height='.8' stroke-width='.025' rx='.08' ry='.08'/>
                                <text x='.4' y='.6' font-size='.55' text-anchor='middle'>{btn + 1}</text> 
                            </g>
                        ").JoinString()}
                        {"Normal,Corner,Center,Restart,Undo,Redo".Split(',').Select((btn, btnIx) => $@"
                            <g class='button' id='p-{puzzleId}-btn-{btn.ToLowerInvariant()}' transform='translate({Kyudosudoku.ControlsX + 3.25 * (btnIx % 3)}, {Kyudosudoku.ControlsY + 1.1 * (1 + btnIx / 3)})'>
                                <rect class='clickable' x='0' y='0' width='2.5' height='.8' stroke-width='.025' rx='.08' ry='.08'/>
                                <text x='1.25' y='.6' font-size='.55' text-anchor='middle'>{btn}</text> 
                            </g>
                        ").JoinString()}

                        <g transform='translate(11.5, 6) rotate(-15)' class='solve-glow'>
                            <rect x='-8' y='-1' width='16' height='2.6' fill='url(#p-{puzzleId}-gradient)' stroke-width='.1' stroke='black' />
                            <text x='0' y='.72' text-anchor='middle' font-size='2' font-weight='bold'>PUZZLE SOLVED</text>
                            <g font-size='.45' transform='translate(0, 1.3)'>
                                <text text-anchor='start' x='-7.7' y='0'>Solved:</text>
                                <text class='inf-count' text-anchor='start' x='-6.1' y='0' font-weight='bold'></text>
                                <text text-anchor='start' x='-4' y='0'>Your time:</text>
                                <text class='inf-time' text-anchor='start' x='-1.7' y='0' font-weight='bold'></text>
                                <text text-anchor='start' x='2.05' y='0'>Average:</text>
                                <text class='inf-avg' text-anchor='start' x='4' y='0' font-weight='bold'></text>
                            </g>
                        </g>
                    </svg>")));
        }

        private static readonly string[] _cellColors = "white;hsl(0, 100%, 94%);white;hsl(52, 100%, 89%);hsl(0, 0%, 94%);hsl(226, 100%, 94%);white;hsl(103, 84%, 95%);white".Split(';');
        private static readonly string[] _highlightedCellColors = "#aaa;hsl(0, 70%, 50%);#aaa;hsl(52, 80%, 40%);#999;hsl(226, 60%, 50%);#aaa;hsl(103, 50%, 50%);#aaa".Split(';');

        private static string kyudokuGridSvg(int corner, int puzzleId, int[] grid, bool setColors = false, int[] highlight = null, int[] circled = null, int[] xed = null, bool glowRed = false) => $@"
            <rect class='solve-glow frame{(glowRed ? " invalid-glow" : null)}' id='p-{puzzleId}-kyudo-{corner}-frame' x='{6.75 * (corner % 2)}' y='{6.75 * (corner / 2)}' width='6' height='6' stroke='black' stroke-width='.2' fill='none' filter='blur(.1)' />
            {Enumerable.Range(0, 36).Select(cell => $@"
                <rect class='clickable kyudo-cell' id='p-{puzzleId}-kyudo-{corner}-cell-{cell}' x='{cell % 6 + 6.75 * (corner % 2)}' y='{cell / 6 + 6.75 * (corner / 2)}' width='1' height='1' stroke='black' stroke-width='.005'{(setColors ? $" fill='{(highlight == null || !highlight.Contains(cell) ? _cellColors : _highlightedCellColors)[((cell % 6) / 3 + corner % 2) + 3 * ((cell / 6) / 3 + corner / 2)]}'" : null)} />
                <text id='p-{puzzleId}-kyudo-{corner}-text-{cell}' x='{cell % 6 + 6.75 * (corner % 2) + .5}' y='{cell / 6 + 6.75 * (corner / 2) + .725}'{(setColors ? $" fill='{(highlight == null || !highlight.Contains(cell) ? "black" : "white")}'" : null)}>{grid[cell]}</text>
                <g opacity='{(circled == null || !circled.Contains(cell) ? "0" : "1")}' id='p-{puzzleId}-kyudo-{corner}-circle-{cell}'>
                    <circle cx='{cell % 6 + 6.75 * (corner % 2) + .5}' cy='{cell / 6 + 6.75 * (corner / 2) + .5}' r='.38' stroke='#000000' stroke-width='.15' fill='none' />
                    <circle cx='{cell % 6 + 6.75 * (corner % 2) + .5}' cy='{cell / 6 + 6.75 * (corner / 2) + .5}' r='.38' stroke='#00aa00' stroke-width='.075' fill='none' />
                </g>
                <g opacity='{(xed == null || !xed.Contains(cell) ? "0" : "1")}' id='p-{puzzleId}-kyudo-{corner}-x-{cell}'>
                    <line x1='{cell % 6 + 6.75 * (corner % 2) + .3}' y1='{cell / 6 + 6.75 * (corner / 2) + .3}' x2='{cell % 6 + 6.75 * (corner % 2) + .7}' y2='{cell / 6 + 6.75 * (corner / 2) + .7}' stroke='#000000' stroke-width='.2' fill='none' stroke-linecap='square' />
                    <line x1='{cell % 6 + 6.75 * (corner % 2) + .3}' y1='{cell / 6 + 6.75 * (corner / 2) + .7}' x2='{cell % 6 + 6.75 * (corner % 2) + .7}' y2='{cell / 6 + 6.75 * (corner / 2) + .3}' stroke='#000000' stroke-width='.2' fill='none' stroke-linecap='square' />
                    <line x1='{cell % 6 + 6.75 * (corner % 2) + .3}' y1='{cell / 6 + 6.75 * (corner / 2) + .3}' x2='{cell % 6 + 6.75 * (corner % 2) + .7}' y2='{cell / 6 + 6.75 * (corner / 2) + .7}' stroke='#df1f1f' stroke-width='.1' fill='none' stroke-linecap='square' />
                    <line x1='{cell % 6 + 6.75 * (corner % 2) + .3}' y1='{cell / 6 + 6.75 * (corner / 2) + .7}' x2='{cell % 6 + 6.75 * (corner % 2) + .7}' y2='{cell / 6 + 6.75 * (corner / 2) + .3}' stroke='#df1f1f' stroke-width='.1' fill='none' stroke-linecap='square' />
                </g>
            ").JoinString()}
            <rect x='{6.75 * (corner % 2)}' y='{6.75 * (corner / 2)}' width='6' height='6' stroke='black' stroke-width='.05' fill='none' />
            <line x1='{3 + 6.75 * (corner % 2)}' y1='{6.75 * (corner / 2)}' x2='{3 + 6.75 * (corner % 2)}' y2='{6 + 6.75 * (corner / 2)}' stroke='black' stroke-width='.03' />
            <line x1='{6.75 * (corner % 2)}' y1='{3 + 6.75 * (corner / 2)}' x2='{6 + 6.75 * (corner % 2)}' y2='{3 + 6.75 * (corner / 2)}' stroke='black' stroke-width='.03' />
        ";

        private HttpResponse dbUpdate(HttpRequest req, DbSession session, Db db, int puzzleId)
        {
            var puzzle = db.Puzzles.FirstOrDefault(p => p.PuzzleID == puzzleId);
            if (puzzle == null)
                return HttpResponse.Empty(HttpStatusCode._500_InternalServerError);

            UserPuzzle already = null;
            if (session.User != null)
            {
                already = db.UserPuzzles.FirstOrDefault(up => up.UserID == session.User.UserID && up.PuzzleID == puzzleId);
                if (already == null)
                {
                    already = new UserPuzzle { PuzzleID = puzzleId, UserID = session.User.UserID, Progess = req.Post["progress"].Value, Solved = puzzle.IsSolved(req.Post["progress"].Value), Time = 10, SolveTime = DateTime.UtcNow };
                    db.UserPuzzles.Add(already);
                    db.SaveChanges();
                    return HttpResponse.Empty(HttpStatusCode._200_OK);
                }

                if (!already.Solved)
                {
                    already.Solved = puzzle.IsSolved(req.Post["progress"].Value);
                    already.SolveTime = DateTime.UtcNow;
                    already.Progess = req.Post["progress"].Value;
                    already.Time += req.Post["time"].Value == null || !int.TryParse(req.Post["time"].Value, out int time) ? 10 : time;
                    db.SaveChanges();
                }
            }

            if (req.Post["getdata"].Value == "1")
            {
                var average = (int?) db.UserPuzzles.Where(up => up.PuzzleID == puzzleId && up.Solved).Average(up => (double?) up.Time);
                var count = db.UserPuzzles.Where(up => up.PuzzleID == puzzleId && up.Solved).Count();
                return HttpResponse.Json(new JsonDict { { "time", already?.Time }, { "avg", average }, { "count", count } });
            }

            return HttpResponse.Empty(HttpStatusCode._200_OK);
        }
    }
}
