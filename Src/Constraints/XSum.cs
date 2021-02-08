using System.Collections.Generic;
using System.Linq;
using PuzzleSolvers;
using RT.Util;
using RT.Util.ExtensionMethods;

namespace KyudosudokuWebsite
{
    [KyuConstraintInfo("X-sum")]
    sealed class XSum : KyuRowColConstraint
    {
        public override string Description => $"The sum of the first X digits in this {(IsCol ? "column" : "row")} must add up to {Clue}, where X is the first digit in the {(IsCol ? "column" : "row")}.";
        public override double ExtraTop => IsCol && !Reverse ? .5 : 0;
        public override double ExtraRight => !IsCol && Reverse ? .25 : 0;
        public override bool ShownTopLeft => !Reverse;
        public static readonly Example Example = new Example
        {
            Constraints = { new XSum(false, 0, false, 19) },
            Cells = { 0, 1, 2, 3, 4, 5, 6, 7, 8 },
            Good = { 4, 7, 6, 2, 1, 9, 8, 5, 3 },
            Bad = { 5, 7, 6, 2, 1, 9, 8, 4, 3 },
            Reason = "The first 5 digits are summed, giving 5+7+6+2+1 = 21.",
            Wide = true
        };

        public XSum(bool isCol, int rowCol, bool reverse, int clue) : base(isCol, rowCol)
        {
            Clue = clue;
            Reverse = reverse;
        }
        private XSum() { }    // for Classify

        public int Clue { get; private set; }
        public bool Reverse { get; private set; }

        protected override IEnumerable<Constraint> getConstraints() { yield return new XSumUniquenessConstraint(Clue, GetAffectedCells(Reverse)); }

        public override bool Verify(int[] grid) => GetAffectedCells(Reverse).Select(cell => grid[cell]).ToArray().Apply(numbers => numbers.Take(numbers[0]).Sum() == Clue);

        public override string Svg => $@"<g transform='translate({(IsCol ? RowCol : Reverse ? 8.8 : -.8)}, {(IsCol ? (Reverse ? 9 : -.8) : RowCol + .1)})'>
            <text x='.5' y='.325' font-size='.3'>XΣ</text>
            <text x='.5' y='.65' font-size='.3'>{Clue}</text>
        </g>";

        public static IList<KyuConstraint> Generate(int[] sudoku)
        {
            var constraints = new List<KyuConstraint>();
            foreach (var isCol in new[] { false, true })
                foreach (var reverse in new[] { false, true })
                    for (var rowCol = 0; rowCol < 9; rowCol++)
                        constraints.Add(new XSum(isCol, rowCol, reverse, Ut.NewArray(9, x => sudoku[isCol ? (rowCol + 9 * (reverse ? 8 - x : x)) : ((reverse ? 8 - x : x) + 9 * rowCol)]).Apply(numbers => numbers.Take(numbers[0]).Sum())));
            return constraints;
        }
    }
}