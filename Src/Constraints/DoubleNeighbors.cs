using System.Collections.Generic;
using System.Linq;
using PuzzleSolvers;

namespace KyudosudokuWebsite
{
    sealed class DoubleNeighbors : KyuConstraint
    {
        public override string Name => "Double neighbors";
        public override string Description => "The two cells marked must have a ratio of 2 (one is double the other).";

        public int Cell1 { get; private set; }
        public int Cell2 { get; private set; }

        public DoubleNeighbors(int cell1, int cell2) { Cell1 = cell1; Cell2 = cell2; }
        private DoubleNeighbors() { }   // for Classify

        protected override Constraint getConstraint() => new TwoCellLambdaConstraint(Cell1, Cell2, (a, b) => a * 2 == b || b * 2 == a);
        public override string Svg => $"<circle cx='{(svgX(Cell1) + svgX(Cell2)) / 2}' cy='{(svgY(Cell1) + svgY(Cell2)) / 2}' r='.15' stroke='white' stroke-width='.02' fill='black' />";
        public override bool SvgAboveLines => true;
        public override bool Verify(int[] grid) => grid[Cell1] * 2 == grid[Cell2] || grid[Cell2] * 2 == grid[Cell1];

        public override bool ClashesWith(KyuConstraint other) => other switch
        {
            ConsecutiveNeighbors cn => cn.Cell1 == Cell1 && cn.Cell2 == Cell2,
            DoubleNeighbors dn => dn.Cell1 == Cell1 && dn.Cell2 == Cell2,
            _ => false
        };

        public static IList<KyuConstraint> Generate(int[] sudoku) =>
            Enumerable.Range(0, 81).Where(cell => cell % 9 > 0 && (sudoku[cell - 1] * 2 == sudoku[cell] || sudoku[cell] * 2 == sudoku[cell - 1])).Select(cell => new DoubleNeighbors(cell - 1, cell))
            .Concat(Enumerable.Range(0, 81).Where(cell => cell / 9 > 0 && (sudoku[cell - 9] * 2 == sudoku[cell] || sudoku[cell] * 2 == sudoku[cell - 9])).Select(cell => new DoubleNeighbors(cell - 9, cell)))
            .ToList<KyuConstraint>();
    }
}
