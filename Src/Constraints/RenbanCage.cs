using System;
using System.Collections.Generic;
using System.Linq;
using PuzzleSolvers;
using RT.Util;
using RT.Util.ExtensionMethods;

namespace KyudosudokuWebsite
{
    sealed class RenbanCage : KyuRegionConstraint
    {
        public override string Name => "Renban cage";
        public override string Description => $"Digits within the cage must form a consecutive set.";

        public RenbanCage(int[] cells) : base(cells) { }

        private RenbanCage() { }    // for Classify

        protected override Constraint getConstraint() => new ConsecutiveUniquenessConstraint(Cells);

        public override bool Verify(int[] grid)
        {
            var numbers = Cells.Select(c => grid[c]).ToArray();
            return numbers.Count(c => !numbers.Contains(c + 1)) == 1;
        }

        public override bool ClashesWith(KyuConstraint other) => other is KyuRegionConstraint kc && kc.Cells.Intersect(Cells).Any();

        private enum Direction { Up, Right, Down, Left }
        public override string Svg
        {
            get
            {
                return $@"<g>
                    <pattern id='pattern-renban' width='2' height='2' patternTransform='rotate(45) scale(.35355) translate(.5, .5)' patternUnits='userSpaceOnUse'>
                        <path d='M0 0h1v1H0zM1 1h1v1H1z' />
                    </pattern>
                    <path d='{GenerateSvgPath(.25, .25)}' fill='url(#pattern-renban)' stroke='none' opacity='.2' />
                </g>";
            }
        }

        public static IList<KyuConstraint> Generate(int[] sudoku) => generateUniquenessRegions(sudoku)
            .Where(region => region.Select(c => sudoku[c]).Apply(numbers => numbers.Count(n => !numbers.Contains(n + 1)) == 1))
            .Select(region => (KyuConstraint) new RenbanCage(region))
            .ToList();
    }
}