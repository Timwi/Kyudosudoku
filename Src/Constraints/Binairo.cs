using System.Collections.Generic;
using System.Linq;
using PuzzleSolvers;
using RT.Util;
using RT.Util.ExtensionMethods;

namespace KyudosudokuWebsite
{
    [KyuConstraintInfo("Binairo")]
    sealed class Binairo : KyuRowColConstraint
    {
        public override string Description => $"In this {(IsCol ? "column" : "row")}, no three adjacent digits can be all odd or all even.";
        public override bool ShownTopLeft => true;
        public override double ExtraTop => IsCol ? .5 : 0;
        public override double ExtraLeft => IsCol ? 0 : .5;
        public static readonly Example Example = new Example
        {
            Constraints = { new Binairo(false, 0) },
            Cells = { 0, 1, 2, 3, 4, 5, 6, 7, 8 },
            Good = { 1, 5, 4, 3, 7, 6, 8, 9, 2 },
            Bad = { 1, 5, 4, 6, 7, 3, 9, 8, 2 },
            Reason = "7/3/9 are three odd digits next to one another.",
            Wide = true
        };

        public Binairo(bool isCol, int rowCol) : base(isCol, rowCol) { }
        private Binairo() { }   // for Classify

        protected override IEnumerable<Constraint> getConstraints() { yield return new ParityNoTripletsConstraint(GetAffectedCells(false)); }

        public override string Svg => $@"<g stroke='black' stroke-width='.075' fill='none' transform='translate({(IsCol ? RowCol + .5 : -.5)}, {(IsCol ? -.5 : RowCol + .5)}) scale(.7)'>
            <circle cx='.25' cy='-.25' r='.2' />
            <circle cx='-.25' cy='.25' r='.2' />
            <path d='M -.25 -.45 v .4 M .25 .05 v .4' />
        </g>";

        public override bool Verify(int[] grid) => VerifyBinairo(GetAffectedCells(false).Select(c => grid[c]).ToArray());

        public static bool VerifyBinairo(int[] numbers)
        {
            for (var i = 1; i < numbers.Length - 1; i++)
                if (numbers[i - 1] % 2 == numbers[i] % 2 && numbers[i + 1] % 2 == numbers[i] % 2)
                    return false;
            return true;
        }

        public static IList<KyuConstraint> Generate(int[] sudoku)
        {
            var constraints = new List<KyuConstraint>();
            foreach (var isCol in new[] { false, true })
                for (var rowCol = 0; rowCol < 9; rowCol++)
                    if (VerifyBinairo(Ut.NewArray(9, x => isCol ? (rowCol + 9 * x) : (x + 9 * rowCol)).Select(ix => sudoku[ix]).ToArray()))
                        constraints.Add(new Binairo(isCol, rowCol));
            return constraints;
        }
    }
}
