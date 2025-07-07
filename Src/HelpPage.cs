using System.Collections.Generic;
using System.Linq;
using RT.Servers;
using RT.TagSoup;
using RT.Util;
using RT.Util.ExtensionMethods;
using SvgPuzzleConstraints;

namespace KyudosudokuWebsite
{
    partial class KyudosudokuPropellerModule
    {
        private HttpResponse helpPage(HttpRequest req) => withSession(req, (session, db) =>
        {
            var kyudoExampleGrid1 = new[] { 3, 1, 5, 7, 1, 5, 1, 2, 1, 9, 2, 5, 4, 8, 2, 6, 6, 2, 3, 7, 3, 5, 4, 8, 1, 2, 4, 6, 7, 2, 2, 6, 5, 1, 3, 6 };
            var kyudoExample = Ut.NewArray<int[]>(
                [1, 9, 9, 4, 6, 9, 1, 7, 7, 5, 4, 7, 9, 5, 8, 1, 9, 7, 3, 8, 3, 5, 5, 8, 8, 4, 6, 6, 5, 2, 4, 9, 1, 5, 5, 3],
                [8, 3, 9, 1, 4, 1, 6, 6, 2, 2, 8, 2, 8, 4, 6, 5, 4, 4, 8, 1, 2, 7, 1, 4, 8, 3, 5, 9, 3, 5, 2, 6, 9, 9, 5, 3],
                [7, 8, 5, 1, 3, 2, 4, 1, 5, 6, 5, 2, 4, 5, 9, 9, 8, 8, 7, 8, 7, 9, 9, 8, 5, 3, 4, 3, 6, 6, 3, 8, 7, 6, 9, 2],
                [8, 3, 8, 8, 1, 9, 6, 2, 9, 4, 3, 7, 1, 3, 5, 2, 9, 3, 5, 4, 3, 5, 2, 5, 9, 5, 8, 7, 1, 9, 2, 8, 7, 2, 7, 2]);

            static object kyudokuGrid(int[] grid, bool? glowRed = null, int[] highlight = null, int[] circled = null, int[] xed = null, int corner = 0) =>
                new RawTag($@"<svg style='width: 7cm' viewBox='{-.5 + 6.75 * (corner % 2)} {-.5 + 6.75 * (corner / 2)} 7 7' text-anchor='middle' font-family='Bitter' font-size='.65'>{kyudokuGridSvg(corner, 1, grid, highlight, circled, xed, glowRed)}</svg>");

            object kyudokuGrids(int[][] highlight = null, int[][] circled = null, int[][] xed = null) =>
                new RawTag($@"<svg style='width: 13.75cm' viewBox='-.5 -.5 13.75 13.75' text-anchor='middle' font-family='Bitter' font-size='.65'>
                    {kyudokuGridSvg(0, 1, kyudoExample[0], highlight?[0], circled?[0], xed?[0])}
                    {kyudokuGridSvg(1, 1, kyudoExample[1], highlight?[1], circled?[1], xed?[1])}
                    {kyudokuGridSvg(2, 1, kyudoExample[2], highlight?[2], circled?[2], xed?[2])}
                    {kyudokuGridSvg(3, 1, kyudoExample[3], highlight?[3], circled?[3], xed?[3])}
                </svg>");

            return RenderPage("How to play Kyudosudoku", session.User, new PageOptions { AddFooter = true, Db = db },
                new DIV { class_ = "main" }._(
                    new P { class_ = "jump" }._("Jump to: ", new A { href = "#rules" }._("Rules"), " | ", new A { href = "#controls" }._("Controls"), " | ", new A { href = "#strategies" }._("Common strategies")),
                    new H1 { id = "rules" }._("Rules of Kyudosudoku"),
                    new P("Kyudosudoku is a logic puzzle that combines Kyudoku with variety Sudoku."),
                    new P("Each puzzle consists of four 6×6 grids filled with digits 1–9 — the ", new CITE("Kyudoku grids"), " — and a blank 9×9 grid, the ", new CITE("Sudoku grid"), ", often with some extra graphics in or around it."),
                    new H2("The Kyudoku part"),
                    new P("In each Kyudoku grid, exactly one of each digit 1–9 must be circled in such a way that the circled digits in each row or column never add up to more than 9."),
                    new TABLE { style = "width:100%", class_ = "examples" }._(
                        new TR(new TD(kyudokuGrid(kyudoExampleGrid1, glowRed: true, circled: [0, 1, 2, 9, 12, 14, 23, 28, 30, 31])), new TD(new P("Invalid: two 2’s are circled."))),
                        new TR(new TD(kyudokuGrid(kyudoExampleGrid1, glowRed: true, circled: [0, 1, 2, 9, 23, 28, 30, 31])), new TD(new P("Invalid: none of the 4’s are circled."))),
                        new TR(new TD(kyudokuGrid(kyudoExampleGrid1, glowRed: true, circled: [0, 1, 2, 9, 12, 23, 28, 29, 31])), new TD(new P("Invalid: the last column has 2 and 8 circled, which add up to 10, which is more than 9."))),
                        new TR(new TD(kyudokuGrid(kyudoExampleGrid1, glowRed: false, circled: [0, 1, 2, 9, 12, 23, 28, 30, 31])), new TD(new P("Valid example.")))),
                    new H2("The Sudoku part"),
                    new P("The Sudoku grid must be filled with digits 1–9 in such a way that every row, every column and every outlined 3×3 box contains the digits 1–9 exactly once."),
                    new P("Furthermore, each Kyudoku grid is linked with a 6×6 “corner” of the Sudoku grid (the coloring helps to visualize this). Every circled digit in a Kyudoku grid transfers that digit to the equivalent location on the Sudoku grid, as shown below:"),
                    new P(new RawTag($@"<svg viewBox='-.25 -.25 23 13.25' font-family='Bitter' font-size='.65' text-anchor='middle'>
                        <defs><marker id='marker4663' orient='auto' overflow='visible'><path fill='#e33838' d='M-1.926-1.21L1.352-.005l-3.278 1.206a2.05 2.05 0 000-2.411z'/></marker></defs>
                        {kyudokuGridSvg(0, 0, "273651445176484965991291788391968623".Select(ch => ch - '0').ToArray(), circled: [18])}
                        {kyudokuGridSvg(1, 0, "713137114328375481938675988428919599".Select(ch => ch - '0').ToArray(), circled: [25])}
                        {kyudokuGridSvg(2, 0, "331754629723365946582838679862457848".Select(ch => ch - '0').ToArray(), circled: [35])}
                        {kyudokuGridSvg(3, 0, "428935481829776637922249136259183762".Select(ch => ch - '0').ToArray(), circled: [13])}
                        <g transform='translate(13.5, 0)'>{sudokuGridSvg(0, [], forHelpPage: true, givens: new Dictionary<int, int?> { [27] = 9, [40] = 8, [77] = 8, [49] = 7 })}</g>
                        <g fill='none' stroke='#e33838' stroke-width='.3' transform='translate(-.25 -.25)'>
                            <path marker-end='url(#marker4663)' d='M1.46 3.45c4.028-1.6 8.054-1.6 12.081.001'/>
                            <path marker-end='url(#marker4663)' d='M9.242 4.592a21.327 21.327 0 018.264 0'/>
                            <path marker-end='url(#marker4663)' d='M9.063 8.967c2.43-2.17 5.232-3.078 8.405-3.202'/>
                            <path marker-end='url(#marker4663)' d='M6.36 12.076c3.49-2.294 8.082-3.221 12.17-3.317'/>
                        </g>
                    </svg>")),
                    new P("Note that the same is not true in reverse. If a digit you place in the Sudoku matches the corresponding digit in a Kyudoku grid, it does not necessarily follow that the digit must be circled. Similarly, a crossed-out digit in a Kyudoku grid does not necessarily imply that the corresponding Sudoku cell can’t have that digit in it."),
                    new P("Each Kyudoku grid in isolation may not have a unique solution, but there is only one way to solve the entire puzzle."),
                    new H2 { id = "constraints" }._("Variety Sudoku constraints"),
                    new P("Each puzzle may have additional graphics in or around the Sudoku grid, which represent additional constraints that must be followed in order to arrive at the correct solution."),
                    new P("These are explained in-game with a tooltip (which you can turn on and off by toggling the tooltip button)."),
                    new P(new A { href = "constraints", style = "font-weight: bold" }._("Complete list of variety constraints")),

                    new H1 { id = "controls" }._("Controls"),
                    new H2("Keyboard"),
                    new TABLE { class_ = "help-controls" }._(
                        new TR(new TH("Arrow keys"), new TD("Moves the selection within the Sudoku grid.")),
                        new TR(new TH("Shift+Arrow keys"), new TD("Extends the selection within the Sudoku grid.")),
                        new TR(new TH("Ctrl+Arrow keys, Ctrl+Space"), new TD("Can be used to select multiple cells that may not be contiguous.")),
                        new TR(new TH("Z"), new TD("Switches to “normal” mode: full-size digits are entered into the Sudoku grid. Use this to enter digits that you have fully deduced.")),
                        new TR(new TH("X"), new TD("Switches to “corner” mode: multiple digits can be notated in the corners of Sudoku cells. This is usually used to notate which cells within a 3×3 box a digit can go.")),
                        new TR(new TH("C"), new TD("Switches to “center” mode: multiple digits can be notated in the centers of Sudoku cells. This is usually used to notate the possible digits for a particular cell.")),
                        new TR(new TH("Q"), new TD("While hovering the mouse over a Kyudoku cell, circles that cell.")),
                        new TR(new TH("W"), new TD("While hovering the mouse over a Kyudoku cell, crosses that cell out.")),
                        new TR(new TH("E"), new TD("While hovering the mouse over a Kyudoku cell, removes the circle or cross.")),
                        new TR(new TH("Ctrl+Z or Backspace"), new TD("Undoes the last change.")),
                        new TR(new TH("Ctrl+Y or Shift+Backspace"), new TD("Redoes the change last undone.")),
                        new TR(new TH("Escape"), new TD("Removes any selection or highlight.")),
                        new TR(new TH("Digits"), new TD("When one or multiple cells in the Sudoku grid are selected, the digit is entered into the cell according to the current mode (normal, corner or center). When no cell is selected, all occurrences of the digit in all grids (except for those crossed out in Kyudoku grids) are highlighted.")),
                        new TR(new TH("Ctrl+Digits"), new TD("Enters a digit in center notation.")),
                        new TR(new TH("Shift+Digits"), new TD("Enters a digit in corner notation."))),
                    new H2("Mouse"),
                    new TABLE { class_ = "help-controls" }._(
                        new TR(new TH("Click (Kyudoku cell)"), new TD("Cycle unmarked → crossed out → circled")),
                        new TR(new TH("Shift+Click (Kyudoku cell)"), new TD("Cycle unmarked → circled → crossed out")),
                        new TR(new TH("Click and drag (Sudoku cell)"), new TD("Select any number of cells.")),
                        new TR(new TH("Shift+Click and drag (Sudoku cell)"), new TD("Add any number of cells to the selection."))),

                    new H1 { id = "strategies" }._("Common strategies"),
                    new P("To get started, here are some common deductions that can help you get started on a Kyudosudoku puzzle:"),
                    new TABLE { class_ = "examples" }._(
                        new TR(new TD(kyudokuGrid(kyudoExample[3], corner: 3, highlight: [6])), new TD(new P("There is only a single 6 in this Kyudoku grid, so it must be circled."))),
                        new TR(new TD(kyudokuGrid(kyudoExample[3], corner: 3, circled: [6], highlight: [0, 8, 9, 11, 18, 24])), new TD(new P("All values in the same row or column that would bring the sum above 9 can now be crossed out."))),
                        new TR(new TD(kyudokuGrid(kyudoExample[3], corner: 3, circled: [6], xed: [0, 8, 9, 11, 18, 24], highlight: [4, 12, 16, 28])), new TD(new P("All of the 1’s in the grid are in a row or column with the highlighted 9. This means the 9 can be crossed out because circling it would rule out all of the 1’s."))),
                        new TR(new TD(kyudokuGrid(kyudoExample[3], corner: 3, circled: [6], xed: [0, 8, 9, 11, 16, 18, 24], highlight: [5, 29])), new TD(new P("All of the remaining 9’s are in the same column. No matter which one ends up getting circled, the other digits in the same column would bring the column’s sum above 9, so they can all be crossed out.")))),
                    new TABLE { class_ = "examples" }._(
                        new TR(new TD(kyudokuGrids(
                            circled: [null, null, null, [6]],
                            xed: [null, null, null, [0, 8, 9, 11, 16, 18, 24, 17, 23, 35]],
                            highlight: [[26], [6, 31], [33], null])), new TD(new P("Remember that the 6 we circled is transferred to the Sudoku grid. Any 6 in the other Kyudoku grids can be crossed out if it would place another 6 within the same row, column, or 3×3 box of the Sudoku grid."))),
                        new TR(new TD(kyudokuGrids(
                            circled: [null, null, null, [6]],
                            xed: [[26], [6, 31], [33], [0, 8, 9, 11, 16, 18, 24, 17, 23, 35]],
                            highlight: [null, [24], null, null])), new TD(new P("Similarly, this 8 can be crossed out because we now know that the corresponding Sudoku cell is already a 6."))),
                        new TR(new TD(kyudokuGrids(
                            circled: [null, null, null, [6]],
                            xed: [[26], [6, 24, 31], [33], [0, 8, 9, 11, 16, 18, 24, 17, 23, 35]],
                            highlight: [[27], null, [9], null])), new TD(new P("However, these 6’s cannot be crossed out (", new EM("nor"), " circled) as they correspond to the exact same cell in the Sudoku grid. Transferring the same digit into the same Sudoku cell multiple times is allowed.")))),

                    new P("These strategies should get you through some of the easiest puzzles. For some of the harder puzzles, many more advanced strategies can be discovered."),
                    new P("Enjoy!")));
        });

        private HttpResponse constraintsPage(HttpRequest req) => withSession(req, (session, db) =>
        {
            return RenderPage("How to play Kyudosudoku", session.User, new PageOptions { AddFooter = true, Db = db },
                new DIV { class_ = "main" }._(
                    new H1("Variety Sudoku constraints"),
                    new P("Each puzzle may have additional graphics in or around the Sudoku grid, which represent additional constraints that must be followed in order to arrive at the correct solution."),
                    new P("These are explained in-game with a tooltip (which you can turn on and off by toggling the tooltip button). For the curious, here is a complete list of them."),
                    renderExamples(
                        // Single cells
                        OddEven.Example,
                        AntiBishop.Example,
                        AntiKnight.Example,
                        AntiKing.Example,
                        NoConsecutive.Example,
                        MaximumCell.Example,
                        MinimumCell.Example,
                        FindThe9.Example,
                        Means.Example,

                        // Two cells
                        ConsecutiveNeighbors.Example,
                        DoubleNeighbors.Example,

                        // Four cells
                        Clockface.Example,
                        Inclusion.Example,
                        Battenburg.Example,

                        // Regions
                        KillerCage.Example,
                        RenbanCage.Example,
                        Thermometer.Example,
                        Arrow.Example,
                        Palindrome.Example,
                        CappedLine.Example,
                        GermanWhisper.Example,
                        Snowball.Example,

                        // Other
                        LittleKiller.Example,

                        // Rows/columns
                        Sandwich.Example,
                        ToroidalSandwich.Example,
                        Skyscraper.Example,
                        SkyscraperSum.Example,
                        Battlefield.Example,
                        Binairo.Example,
                        XSum.Example,
                        YSum.Example
                    )));
        });

        private IEnumerable<object> renderExamples(params Example[] examples)
        {
            yield return new P(examples
                .Select(ex => new A { href = $"#constraint-{ex.Constraints.First().GetType().Name}" }._(ex.Constraints.First().Name))
                .InsertBetween<object>(" | "));
            foreach (var example in examples)
            {
                yield return new HR();
                var constraintName = example.Constraints.First().Name;
                var constraintId = example.Constraints.First().GetType().Name;
                var invalidTd = new TD { class_ = $"{(example.Wide ? "wide " : null)}incorrect" }._(clippedSudokuGrid(example.Constraints, glowRed: true, givens: example.BadGivens, wide: example.Wide),
                    new DIV(new SPAN("✗ Invalid", example.Reason == null ? "." : ":"), example.Reason.NullOr(r => new DIV(r))));
                var validTd = new TD { class_ = $"{(example.Wide ? "wide " : null)}correct" }._(clippedSudokuGrid(example.Constraints, glowRed: false, givens: example.GoodGivens, wide: example.Wide),
                    new DIV(new SPAN("✓ Valid.")));
                if (example.Wide)
                    yield return new TABLE { class_ = "example", id = $"constraint-{constraintId}" }._(
                        new TR(
                            new TD { rowspan = 2 }._(clippedSudokuGrid(example.Constraints)),
                            new TD { rowspan = 2, class_ = "explanation" }._(new H4(constraintName), example.Constraints.Select(c => new P(c.Description))),
                            invalidTd),
                        new TR(validTd));
                else
                    yield return new TABLE { class_ = "example", id = $"constraint-{constraintId}" }._(
                        new TR(
                            new TD(clippedSudokuGrid(example.Constraints)),
                            new TD { class_ = "explanation" }._(new H4(constraintName), example.Constraints.Select(c => new P(c.Description))),
                            invalidTd,
                            validTd));
            }
        }

        private object clippedSudokuGrid(IEnumerable<SvgConstraint> constraints, bool? glowRed = null, Dictionary<int, int?> givens = null, bool wide = false) =>
            new RawTag($@"<svg viewBox='-1 {(wide ? -.5 : -1)} {(wide ? 10.5 : 5.5)} {(wide ? 2 : 4.5)}' stroke-width='0' text-anchor='middle' font-family='Bitter' font-size='.65'><defs>{constraints.SelectMany(c => c.SvgDefs).Distinct().JoinString()}</defs>{sudokuGridSvg(1, constraints, true, givens, glowRed)}</svg>");
    }
}
