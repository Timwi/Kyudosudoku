using System.Collections.Generic;
using System.Linq;
using PuzzleSolvers;
using RT.Util.ExtensionMethods;

namespace KyudosudokuWebsite
{
    [KyuConstraintInfo("Maximum cell")]
    sealed class MaximumCell : KyuCellConstraint
    {
        public override string Description => "This digit must be greater than the digits orthogonally adjacent to it.";

        public static readonly Example Example = new Example
        {
            Constraints = { new MaximumCell(11) },
            Cells = { 2, 10, 11, 12, 20 },
            Good = { 1, 5, 7, 4, 2 },
            Bad = { 1, 5, 7, 9, 2 },
            Reason = "9 is greater than 7."
        };

        public MaximumCell(int cell) : base(cell) { }
        private MaximumCell() { }    // for Classify

        protected override IEnumerable<Constraint> getConstraints() => Orthogonal(Cell).Select(adj => new LessThanConstraint(new[] { adj, Cell }));
        public override string Svg => $"<path transform='translate({Cell % 9}, {Cell / 9})' d='M.5 .05 .7 .2 .3 .2z M.95 .5 .8 .7 .8 .3z M.5 .95 .3 .8 .7 .8z M.05 .5 .2 .3 .2 .7z' opacity='.2' />";

        public override bool Verify(int[] grid)
        {
            foreach (var c in Orthogonal(Cell))
                if (grid[c] >= grid[Cell])
                    return false;
            return true;
        }

        public static IList<KyuConstraint> Generate(int[] sudoku) => Enumerable.Range(0, 81)
            .Where(cell => Orthogonal(cell).All(c => sudoku[cell] > sudoku[c]))
            .Select(cell => new MaximumCell(cell))
            .ToArray();
    }
}
