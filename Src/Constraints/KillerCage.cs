using System.Collections.Generic;
using System.Linq;
using System.Text;
using PuzzleSolvers;
using RT.Util;
using RT.Util.ExtensionMethods;

namespace KyudosudokuWebsite
{
    sealed class KillerCage : KyuRegionConstraint
    {
        public override string Name => "Killer cage";
        public override string Description => $"Digits within the cage must be different{Sum.NullOr(s => $" and must add up to {s}")}.";

        public KillerCage(int[] cells, int? sum) : base(cells)
        {
            Sum = sum;
        }
        private KillerCage() { }    // for Classify

        public int? Sum { get; private set; }

        protected override Constraint getConstraint() => Sum == null ? new UniquenessConstraint(Cells) : new SumUniquenessConstraint(Sum.Value, Cells);

        public override bool Verify(int[] grid)
        {
            for (var i = 0; i < Cells.Length; i++)
                for (var j = i + 1; j < Cells.Length; j++)
                    if (grid[i] == grid[j])
                        return false;
            return Sum == null || Cells.Sum(c => grid[c]) == Sum.Value;
        }

        public override bool ClashesWith(KyuConstraint other) => other is KyuRegionConstraint kc && kc.Cells.Intersect(Cells).Any();

        public override string Svg => $"<path d='{GenerateSvgPath(.06, .06, Sum.NullOr(s => .275), Sum.NullOr(s => .25))}' fill='none' stroke='black' stroke-width='.025' stroke-dasharray='.09,.07' />"
            + Sum.NullOr(s => $"<text x='{svgX(Cells.Min()) - .46}' y='{svgY(Cells.Min()) - .25}' text-anchor='start' font-size='.25'>{s}</text>");

        public static IList<KyuConstraint> Generate(int[] sudoku) => generateUniquenessRegions(sudoku)
            .SelectMany(region => new KyuConstraint[] { new KillerCage(region, region.Sum(c => sudoku[c])), new KillerCage(region, null) })
            .ToList();
    }
}