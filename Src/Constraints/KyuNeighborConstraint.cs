using System;
using System.Collections.Generic;
using System.Linq;
using PuzzleSolvers;

namespace KyudosudokuWebsite
{
    abstract class KyuNeighborConstraint : KyuConstraint
    {
        public int Cell1 { get; private set; }
        public int Cell2 { get; private set; }

        public KyuNeighborConstraint(int cell1, int cell2) { Cell1 = cell1; Cell2 = cell2; }
        protected KyuNeighborConstraint() { }   // for Classify

        protected abstract bool verify(int a, int b);

        protected sealed override IEnumerable<Constraint> getConstraints() { yield return new TwoCellLambdaConstraint(Cell1, Cell2, verify); }
        protected double x => (svgX(Cell1) + svgX(Cell2)) / 2;
        protected double y => (svgY(Cell1) + svgY(Cell2)) / 2;

        public override bool SvgAboveLines => true;
        public sealed override bool Verify(int[] grid) => verify(grid[Cell1], grid[Cell2]);
        public sealed override bool IncludesCell(int cell) => cell == Cell1 || cell == Cell2;

        public override bool ClashesWith(KyuConstraint other) => other switch
        {
            KyuNeighborConstraint cn => (cn.Cell1 == Cell1 && cn.Cell2 == Cell2) || (cn.Cell1 == Cell2 && cn.Cell2 == Cell1),
            _ => false
        };

        protected static IList<KyuConstraint> generate(int[] sudoku, Func<int, int, bool> verify, Func<int, int, KyuConstraint> constructor) =>
            Enumerable.Range(0, 81).Where(cell => cell % 9 > 0 && verify(sudoku[cell - 1], sudoku[cell])).Select(cell => constructor(cell - 1, cell))
            .Concat(Enumerable.Range(0, 81).Where(cell => cell / 9 > 0 && verify(sudoku[cell - 9], sudoku[cell])).Select(cell => constructor(cell - 9, cell)))
            .ToList();
    }
}
