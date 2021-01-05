using System.Linq;
using RT.Servers;
using RT.TagSoup;
using RT.Util.ExtensionMethods;

namespace KyudosudokuWebsite
{
    partial class KyudosudokuPropellerModule
    {
        private HttpResponse PuzzlePage(HttpRequest req)
        {
            var puzzleNumStr = req.Url.Path.Substring(1);
            if (!int.TryParse(puzzleNumStr, out int puzzleNum))
                return Page404(req);

            var puzzle = Kyudosudoku.Generate(puzzleNum);

            return HttpResponse.Html(
                new HTML(
                    new HEAD(
                        new SCRIPT { src = "/js" },
                        new LINK { rel = "stylesheet", href = "/css" },
                        new TITLE("Daily Kyudosudoku")),
                    new BODY(
                        new H1($"Kyudosudoku Puzzle #{puzzleNum}"),
                        new DIV { class_ = "puzzle", id = $"puzzle-{puzzleNum}", tabindex = 0 }._(
                            new RawTag($@"<svg viewBox='-.5 -.5 24 13.75' stroke='black' text-anchor='middle' font-family='Bitter' font-size='.65'>
                                {Enumerable.Range(0, 4).Select(corner => $@"
                                    {Enumerable.Range(0, 36).Select(cell => $@"
                                        <rect class='clickable kyudo-cell' id='kyudo-{corner}-cell-{cell}' x='{cell % 6 + 6.75 * (corner % 2)}' y='{cell / 6 + 6.75 * (corner / 2)}' width='1' height='1' stroke-width='.005' />
                                        <text id='kyudo-{corner}-text-{cell}' x='{cell % 6 + 6.75 * (corner % 2) + .5}' y='{cell / 6 + 6.75 * (corner / 2) + .725}' stroke='none'>{puzzle.Grids[corner][cell]}</text>
                                        <g opacity='0' id='kyudo-{corner}-circle-{cell}'>
                                            <circle cx='{cell % 6 + 6.75 * (corner % 2) + .5}' cy='{cell / 6 + 6.75 * (corner / 2) + .5}' r='.38' stroke='#000000' stroke-width='.15' fill='none' />
                                            <circle cx='{cell % 6 + 6.75 * (corner % 2) + .5}' cy='{cell / 6 + 6.75 * (corner / 2) + .5}' r='.38' stroke='#00aa00' stroke-width='.075' fill='none' />
                                        </g>
                                        <g opacity='0' id='kyudo-{corner}-x-{cell}'>
                                            <line x1='{cell % 6 + 6.75 * (corner % 2) + .3}' y1='{cell / 6 + 6.75 * (corner / 2) + .3}' x2='{cell % 6 + 6.75 * (corner % 2) + .7}' y2='{cell / 6 + 6.75 * (corner / 2) + .7}' stroke='#000000' stroke-width='.2' fill='none' stroke-linecap='square' />
                                            <line x1='{cell % 6 + 6.75 * (corner % 2) + .3}' y1='{cell / 6 + 6.75 * (corner / 2) + .7}' x2='{cell % 6 + 6.75 * (corner % 2) + .7}' y2='{cell / 6 + 6.75 * (corner / 2) + .3}' stroke='#000000' stroke-width='.2' fill='none' stroke-linecap='square' />
                                            <line x1='{cell % 6 + 6.75 * (corner % 2) + .3}' y1='{cell / 6 + 6.75 * (corner / 2) + .3}' x2='{cell % 6 + 6.75 * (corner % 2) + .7}' y2='{cell / 6 + 6.75 * (corner / 2) + .7}' stroke='#df1f1f' stroke-width='.1' fill='none' stroke-linecap='square' />
                                            <line x1='{cell % 6 + 6.75 * (corner % 2) + .3}' y1='{cell / 6 + 6.75 * (corner / 2) + .7}' x2='{cell % 6 + 6.75 * (corner % 2) + .7}' y2='{cell / 6 + 6.75 * (corner / 2) + .3}' stroke='#df1f1f' stroke-width='.1' fill='none' stroke-linecap='square' />
                                        </g>
                                    ").JoinString()}
                                    <rect x='{6.75 * (corner % 2)}' y='{6.75 * (corner / 2)}' width='6' height='6' stroke-width='.05' fill='none' />
                                    <line x1='{3 + 6.75 * (corner % 2)}' y1='{6.75 * (corner / 2)}' x2='{3 + 6.75 * (corner % 2)}' y2='{6 + 6.75 * (corner / 2)}' stroke-width='.03' />
                                    <line x1='{6.75 * (corner % 2)}' y1='{3 + 6.75 * (corner / 2)}' x2='{6 + 6.75 * (corner % 2)}' y2='{3 + 6.75 * (corner / 2)}' stroke-width='.03' />
                                ").JoinString()}
                                {Enumerable.Range(0, 81).Select(cell => $@"
                                    <rect class='clickable sudoku-cell' id='sudoku-cell-{cell}' x='{cell % 9 + 14}' y='{cell / 9 + 1.875}' width='1' height='1' stroke-width='.005' />
                                    <text id='sudoku-text-{cell}' x='{cell % 9 + 14.5}' y='{cell / 9 + 1.875 + .725}' stroke='none'></text>
                                    <text id='sudoku-center-text-{cell}' x='{cell % 9 + 14.5}' y='{cell / 9 + 1.875 + .625}' stroke='none' font-size='.3' fill='#1d6ae5'></text>
                                    <text id='sudoku-corner-text-{cell}-0' x='{cell % 9 + 14.05}' y='{cell / 9 + 1.875 + .25}' stroke='none' font-size='.25' fill='#1d6ae5' text-anchor='start'></text>
                                    <text id='sudoku-corner-text-{cell}-1' x='{cell % 9 + 14.95}' y='{cell / 9 + 1.875 + .25}' stroke='none' font-size='.25' fill='#1d6ae5' text-anchor='end'></text>
                                    <text id='sudoku-corner-text-{cell}-2' x='{cell % 9 + 14.05}' y='{cell / 9 + 1.875 + .95}' stroke='none' font-size='.25' fill='#1d6ae5' text-anchor='start'></text>
                                    <text id='sudoku-corner-text-{cell}-3' x='{cell % 9 + 14.95}' y='{cell / 9 + 1.875 + .95}' stroke='none' font-size='.25' fill='#1d6ae5' text-anchor='end'></text>
                                    <text id='sudoku-corner-text-{cell}-4' x='{cell % 9 + 14.5}' y='{cell / 9 + 1.875 + .25}' stroke='none' font-size='.25' fill='#1d6ae5' text-anchor='middle'></text>
                                    <text id='sudoku-corner-text-{cell}-5' x='{cell % 9 + 14.95}' y='{cell / 9 + 1.875 + .6125}' stroke='none' font-size='.25' fill='#1d6ae5' text-anchor='end'></text>
                                    <text id='sudoku-corner-text-{cell}-6' x='{cell % 9 + 14.5}' y='{cell / 9 + 1.875 + .95}' stroke='none' font-size='.25' fill='#1d6ae5' text-anchor='middle'></text>
                                    <text id='sudoku-corner-text-{cell}-7' x='{cell % 9 + 14.05}' y='{cell / 9 + 1.875 + .6125}' stroke='none' font-size='.25' fill='#1d6ae5' text-anchor='start'></text>
                                ").JoinString()}
                                <rect x='14' y='1.875' width='9' height='9' stroke-width='.05' fill='none' />
                                <line x1='17' y1='1.875' x2='17' y2='10.875' stroke-width='.03' />
                                <line x1='20' y1='1.875' x2='20' y2='10.875' stroke-width='.03' />
                                <line x1='14' y1='4.875' x2='23' y2='4.875' stroke-width='.03' />
                                <line x1='14' y1='7.875' x2='23' y2='7.875' stroke-width='.03' />
                            </svg>")))));
        }
    }
}
