using System;
using System.Collections.Generic;
using System.Linq;
using PuzzleSolvers;

namespace KyudosudokuWebsite
{
    sealed class ConsecutiveNeighbors : KyuConstraint
    {
        public override string Name => "Consecutive neighbors";
        public override string Description => "The two cells marked must be consecutive (have a difference of 1).";

        public int Cell1 { get; private set; }
        public int Cell2 { get; private set; }

        public ConsecutiveNeighbors(int cell1, int cell2) { Cell1 = cell1; Cell2 = cell2; }
        private ConsecutiveNeighbors() { }   // for Classify

        protected override Constraint getConstraint() => new TwoCellLambdaConstraint(Cell1, Cell2, (a, b) => Math.Abs(a - b) == 1);
        public override string Svg => $"<circle cx='{(svgX(Cell1) + svgX(Cell2)) / 2}' cy='{(svgY(Cell1) + svgY(Cell2)) / 2}' r='.15' stroke='black' stroke-width='.02' fill='white' />";
        public override bool SvgAboveLines => true;
        public override bool Verify(int[] grid) => Math.Abs(grid[Cell1] - grid[Cell2]) == 1;

        public override bool ClashesWith(KyuConstraint other) => other switch
        {
            ConsecutiveNeighbors cn => cn.Cell1 == Cell1 && cn.Cell2 == Cell2,
            DoubleNeighbors dn => dn.Cell1 == Cell1 && dn.Cell2 == Cell2,
            _ => false
        };

        public static IList<KyuConstraint> Generate(int[] sudoku) =>
            Enumerable.Range(0, 81).Where(cell => cell % 9 > 0 && Math.Abs(sudoku[cell - 1] - sudoku[cell]) == 1).Select(cell => new ConsecutiveNeighbors(cell - 1, cell))
            .Concat(Enumerable.Range(0, 81).Where(cell => cell / 9 > 0 && Math.Abs(sudoku[cell - 9] - sudoku[cell]) == 1).Select(cell => new ConsecutiveNeighbors(cell - 9, cell)))
            .ToList<KyuConstraint>();
    }
}
