using System;
using System.Collections.Generic;
using System.Linq;
using PuzzleSolvers;
using RT.Util.ExtensionMethods;

namespace KyudosudokuWebsite
{
    [KyuConstraintInfo("Arrow")]
    sealed class Arrow : KyuConstraint
    {
        public override string Description => "The digits along the arrow must sum to the digit in the circle. (These digits need not necessarily be different.)";
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

        protected override IEnumerable<Constraint> getConstraints() { yield return new IndirectSumConstraint(Cells[0], Cells.Subarray(1)); }

        public override string Svg
        {
            get
            {
                // Try to reorder the cells so that there are fewer obtuse angles in it
                IEnumerable<(int[] result, int numObtuse)> rearrange(int[] sofar, int ix, int numObtuse)
                {
                    if (ix == Cells.Length)
                    {
                        yield return (sofar.ToArray(), numObtuse);
                        yield break;
                    }
                    for (var i = 0; i < Cells.Length; i++)
                    {
                        if (Math.Abs(Cells[i] % 9 - sofar[ix - 1] % 9) > 1 || Math.Abs(Cells[i] / 9 - sofar[ix - 1] / 9) > 1)
                            continue;
                        if (sofar.Take(ix).Contains(Cells[i]))
                            continue;
                        sofar[ix] = Cells[i];
                        var nob = numObtuse;
                        if (ix >= 2 && (
                            (sofar[ix] % 9 == sofar[ix - 2] % 9 && Math.Abs(sofar[ix] / 9 - sofar[ix - 2] / 9) == 1) ||
                            (sofar[ix] / 9 == sofar[ix - 2] / 9 && Math.Abs(sofar[ix] % 9 - sofar[ix - 2] % 9) == 1)))
                            nob++;
                        foreach (var res in rearrange(sofar, ix + 1, nob))
                            yield return res;
                    }
                }
                var (res, numOb) = rearrange(Cells.ToArray(), 1, 0).MinElement(tup => tup.numObtuse);

                static int angleDeg(int c1, int c2) => (c2 % 9 - c1 % 9, c2 / 9 - c1 / 9) switch { (-1, -1) => 225, (0, -1) => 270, (1, -1) => 315, (-1, 0) => 180, (1, 0) => 0, (-1, 1) => 135, (0, 1) => 90, (1, 1) => 45, _ => 10 };
                static double angle(int c1, int c2) => angleDeg(c1, c2) * Math.PI / 180;
                var f = res[0];
                var s = res[1];
                var sl = res[res.Length - 2];
                var l = res[res.Length - 1];
                return $@"<g fill='none' stroke='black' stroke-width='.05' opacity='.2'>
                    <circle cx='{svgX(f)}' cy='{svgY(f)}' r='.4' />
                    <path d='M{svgX(f) + .4 * Math.Cos(angle(f, s))} {svgY(f) + .4 * Math.Sin(angle(f, s))} {res.Skip(1).SkipLast(1).Select(c => $"{svgX(c)} {svgY(c)}").JoinString(" ")} {svgX(l) + .3 * Math.Cos(angle(sl, l))} {svgY(l) + .3 * Math.Sin(angle(sl, l))}' />
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

                var last = sofar.Last();
                foreach (var adj in Adjacent(last))
                    if (!sofar.Contains(adj) && sumSofar + sudoku[adj] <= sudoku[sofar[0]] && noDiagonalCrossingExists(last % 9, last / 9, adj % 9, adj / 9))
                        foreach (var next in recurse(sofar.Insert(sofar.Length, adj), sumSofar + sudoku[adj]))
                            yield return next;
            }

            for (var startCell = 0; startCell < 81; startCell++)
                list.AddRange(recurse(new[] { startCell }, 0));
            return list;
        }
    }
}
