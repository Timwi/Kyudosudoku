using System.Collections.Generic;
using System.Linq;

namespace KyudosudokuWebsite
{
    sealed class Battenburg : KyuFourCellConstraint
    {
        public override string Name => "Battenburg";
        public override string Description => "The four cells around the clue must form a 2×2 checkerboard of odd and even digits.";
        public static readonly Example Example = new Example
        {
            Constraints = { new Battenburg(2) },
            Cells = { 2, 3, 12, 11 },
            Good = { 3, 6, 9, 4 },
            Bad = { 3, 1, 8, 4 },
            Reason = "3 and 1 are both odd and next to each other."
        };

        public Battenburg(int topLeftCell) : base(topLeftCell) { }
        private Battenburg() { }        // for Classify

        public override bool SvgAboveLines => true;
        public override string Svg => $"<path fill='white' stroke='black' stroke-width='.02' d='M{x - .15} {y - .15} h .3 v .3 h -.3 z' /><path fill='black' stroke='none' d='M{x - .15} {y - .15} h .15 v .15 h -.15 zM{x} {y} h .15 v .15 h -.15 z' />";

        protected override bool verify(int a, int b, int c, int d) => myVerify(a, b, c, d);
        private static bool myVerify(int a, int b, int c, int d) => a % 2 != b % 2 && b % 2 != c % 2 && c % 2 != d % 2;

        public static IList<KyuConstraint> Generate(int[] sudoku) => generate(sudoku, (cell, a, b, c, d) => myVerify(a, b, c, d) ? new[] { new Battenburg(cell) } : Enumerable.Empty<KyuConstraint>());
    }
}
