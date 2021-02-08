using System;
using System.Collections.Generic;
using System.Linq;
using PuzzleSolvers;
using RT.Util.ExtensionMethods;

namespace KyudosudokuWebsite
{
    [KyuConstraintInfo("No-consecutive")]
    sealed class NoConsecutive : KyuCellConstraint
    {
        public override string Description => "A digit that’s 1 more or 1 less than this digit can’t be orthogonally adjacent to it.";
        public static readonly Example Example = new Example
        {
            Constraints = { new NoConsecutive(20) },
            Cells = { 11, 20 },
            Good = { 2, 7 },
            Bad = { 6, 7 },
            Reason = "6 and 7 are consecutive, so the 6 can’t be orthogonally adjacent to the 7."
        };

        public NoConsecutive(int cell) : base(cell) { }
        private NoConsecutive() { }    // for Classify

        protected override IEnumerable<Constraint> getConstraints() { yield return new NoConsecutiveConstraint(9, 9, includeDiagonals: false, enforcedCells: new[] { Cell }); }
        public override string Svg => $"<path transform='translate({Cell % 9}, {Cell / 9})' d='m 0 .5 .1 -.1 .1 .1 -.1 .1z M .4 .1 l .1 -.1 .1 .1 -.1 .1z M .8 .5 l .1 -.1 .1 .1 -.1 .1z M .4 .9 l .1 -.1 .1 .1 -.1 .1z' opacity='.2' />";

        public override bool Verify(int[] grid)
        {
            foreach (var c in NoConsecutiveConstraint.AdjacentCells(Cell, 9, 9, false))
                if (grid[c] == grid[Cell])
                    return false;
            return true;
        }

        public static IList<KyuConstraint> Generate(int[] sudoku) => Enumerable.Range(0, 81)
            .Where(cell => NoConsecutiveConstraint.AdjacentCells(cell, 9, 9, false).All(c => Math.Abs(sudoku[c] - sudoku[cell]) != 1))
            .Select(cell => new NoConsecutive(cell))
            .ToArray();
    }
}
