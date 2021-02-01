using System.Collections.Generic;
using System.Linq;
using PuzzleSolvers;
using RT.Util.ExtensionMethods;

namespace KyudosudokuWebsite
{
    sealed class OddEven : KyuCellConstraint
    {
        public override string Name => "Odd/even";
        public override string Description => $"The digit in this cell must be {(Odd ? "odd" : "even")}.";
        protected override Constraint getConstraint() => new OneCellLambdaConstraint(Cell, v => v % 2 == (Odd ? 1 : 0));

        public bool Odd { get; private set; }

        public OddEven(int cell, bool odd) : base(cell) { Odd = odd; }
        private OddEven() { }    // for Classify

        public override string Svg => Odd
            ? $@"<circle cx='{Kyudosudoku.SudokuX + Cell % 9 + .5}' cy='{Kyudosudoku.SudokuY + Cell / 9 + .5}' r='.4' fill='rgba(0, 0, 0, .1)' />"
            : $@"<rect x='{Kyudosudoku.SudokuX + Cell % 9 + .1}' y='{Kyudosudoku.SudokuY + Cell / 9 + .1}' width='.8' height='.8' fill='rgba(0, 0, 0, .1)' />";
        public override bool Verify(int[] grid) => grid[Cell] % 2 == (Odd ? 1 : 0);

        public static IList<KyuConstraint> Generate(int[] sudoku) =>
            Enumerable.Range(0, 81).Select(cell => new OddEven(cell, sudoku[cell] % 2 != 0)).ToArray();
    }
}
