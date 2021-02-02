using System;
using System.Collections.Generic;
using System.Linq;
using PuzzleSolvers;
using RT.Util.ExtensionMethods;

namespace KyudosudokuWebsite
{
    sealed class Arrow : KyuConstraint
    {
        public override string Name => "Arrow";
        public override string Description => "The digits along the arrow must sum to the digit in the circle.";
        public static readonly Example Example = new Example
        {
            Constraints = { new Arrow(new[] { 18, 10, 20, 12, 3 }) },
            Cells = { 18, 10, 20, 12, 3 },
            Good = { 7, 1, 2, 3, 1 },
            Bad = { 8, 1, 2, 3, 1 },
            Reason = "The digits add up to 7, but the circle does not contain a 7."
        };

        public int[] Cells { get; private set; }

        public Arrow(int[] cells) { Cells = cells; }
        private Arrow() { }   // for Classify

        protected override Constraint getConstraint() => new IndirectSumConstraint(Cells[0], Cells.Subarray(1));

        public override string Svg
        {
            get
            {
                static int angleDeg(int c1, int c2) => (c2 % 9 - c1 % 9, c2 / 9 - c1 / 9) switch { (-1, -1) => 225, (0, -1) => 270, (1, -1) => 315, (-1, 0) => 180, (1, 0) => 0, (-1, 1) => 135, (0, 1) => 90, (1, 1) => 45, _ => 10 };
                static double angle(int c1, int c2) => angleDeg(c1, c2) * Math.PI / 180;
                var f = Cells[0];
                var s = Cells[1];
                var sl = Cells[Cells.Length - 2];
                var l = Cells[Cells.Length - 1];
                return $@"<g fill='none' stroke='black' stroke-width='.05' opacity='.2'>
                    <circle cx='{svgX(f)}' cy='{svgY(f)}' r='.4' />
                    <path d='M{svgX(f) + .4 * Math.Cos(angle(f, s))} {svgY(f) + .4 * Math.Sin(angle(f, s))} {Cells.Skip(1).SkipLast(1).Select(c => $"{svgX(c)} {svgY(c)}").JoinString(" ")} {svgX(l) + .3 * Math.Cos(angle(sl, l))} {svgY(l) + .3 * Math.Sin(angle(sl, l))}' />
                    <path d='M -.2 -.2 .3 0 -.2 .2' transform='translate({svgX(l)}, {svgY(l)}) rotate({angleDeg(sl, l)})' />
                </g>";
            }
        }

        public override bool Verify(int[] grid) => Cells.Skip(1).Sum(c => grid[c]) == grid[Cells[0]];
        public override bool IncludesCell(int cell) => Cells.Contains(cell);

        public override bool ClashesWith(KyuConstraint other) => other switch
        {
            KyuCellConstraint cc => Cells[0] == cc.Cell,
            Thermometer th => Cells[0] == th.Cells[0],
            _ => false,
        };

        public static IList<KyuConstraint> Generate(int[] sudoku)
        {
            var list = new List<KyuConstraint>();

            IEnumerable<Arrow> recurse(int[] sofar, int sumSofar)
            {
                if (sofar.Length >= 3 && sumSofar == sudoku[sofar[0]])
                    yield return new Arrow(sofar);

                bool noDiagonalCrossingExists(int x1, int y1, int x2, int y2)
                {
                    var p1 = Array.IndexOf(sofar, x1 + 9 * y2);
                    var p2 = Array.IndexOf(sofar, x2 + 9 * y1);
                    return p1 == -1 || p2 == -1 || Math.Abs(p1 - p2) != 1;
                }

                var x = sofar.Last() % 9;
                var y = sofar.Last() / 9;
                for (var xx = x - 1; xx <= x + 1; xx++)
                    if (xx >= 0 && xx < 9)
                        for (var yy = y - 1; yy <= y + 1; yy++)
                            if (yy >= 0 && yy < 9 && !sofar.Contains(xx + 9 * yy) && sumSofar + sudoku[xx + 9 * yy] <= sudoku[sofar[0]]
                                && (xx == x || yy == y || noDiagonalCrossingExists(x, y, xx, yy)))
                                foreach (var next in recurse(sofar.Insert(sofar.Length, xx + 9 * yy), sumSofar + sudoku[xx + 9 * yy]))
                                    yield return next;
            }

            for (var startCell = 0; startCell < 81; startCell++)
                list.AddRange(recurse(new[] { startCell }, 0));
            return list;
        }
    }
}
