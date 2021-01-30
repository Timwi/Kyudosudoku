using System;
using System.Collections.Generic;
using System.Linq;
using PuzzleSolvers;
using RT.Util.ExtensionMethods;

namespace KyudosudokuWebsite
{
    sealed class NoConsecutive : KyuCellConstraint
    {
        public override string Name => "No-consecutive";
        public override string Description => "A digit that’s 1 more or 1 less than this digit can’t be orthogonally adjacent to it.";
        protected override Constraint getConstraint() => new NoConsecutiveConstraint(9, 9, includeDiagonals: false, enforcedCells: new[] { Cell });

        public NoConsecutive(int cell) : base(cell) { }
        private NoConsecutive() { }    // for Classify

        public override string Svg => $"<path transform='translate({Kyudosudoku.SudokuX + Cell % 9}, {Kyudosudoku.SudokuY + Cell / 9})' d='M.5 .05 .7 .2 .3 .2z M.95 .5 .8 .7 .8 .3z M.5 .95 .3 .8 .7 .8z M.05 .5 .2 .3 .2 .7z' opacity='.2' />";

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
