using System;
using System.Collections.Generic;
using System.Linq;
using PuzzleSolvers;
using RT.Util;
using RT.Util.ExtensionMethods;

namespace KyudosudokuWebsite
{
    [KyuConstraintInfo("Capped line")]
    sealed class CappedLine : KyuConstraint
    {
        public override string Description => "The digits along the line must be numerically between the digits at the ends.";
        public static readonly Example Example = new Example
        {
            Constraints = { new CappedLine(new[] { 9, 19, 20, 12, 3 }) },
            Cells = { 9, 19, 20, 12, 3 },
            Good = { 2, 5, 7, 3, 8 },
            Bad = { 3, 5, 7, 2, 8 },
            Reason = "The 2 is not between 3 and 8."
        };

        public int[] Cells { get; private set; }

        public CappedLine(int[] cells) { Cells = cells; }
        private CappedLine() { }   // for Classify

        protected override IEnumerable<Constraint> getConstraints() { yield return new CappedLineConstraint(Cells[0], Cells[Cells.Length - 1], Cells.Skip(1).SkipLast(1).ToArray()); }

        private static object _lockObject = new object();
        private static int _svgIdCounter = 1;
        public override string Svg
        {
            get
            {
                var svgId = 0;
                lock (_lockObject)
                    svgId = _svgIdCounter++;
                static int angleDeg(int c1, int c2) => (c2 % 9 - c1 % 9, c2 / 9 - c1 / 9) switch { (-1, -1) => 225, (0, -1) => 270, (1, -1) => 315, (-1, 0) => 180, (1, 0) => 0, (-1, 1) => 135, (0, 1) => 90, (1, 1) => 45, _ => 10 };
                var f = Cells[0];
                var s = Cells[1];
                var sl = Cells[Cells.Length - 2];
                var l = Cells[Cells.Length - 1];
                var path = $"M{Cells.Select(c => $"{svgX(c)} {svgY(c)}").JoinString(" ")}";
                return $@"<g opacity='.2'>
                    <mask id='capped-line-mask-{svgId}'>
                        <rect fill='white' x='0' y='0' width='9' height='9' stroke='none' />
                        <path d='{path}' stroke='black' stroke-width='.1' stroke-linejoin='miter' fill='none' />
                    </mask>
                    <path d='M{Cells.Select(c => $"{svgX(c)} {svgY(c)}").JoinString(" ")}' stroke='black' stroke-width='.3' stroke-linejoin='miter' fill='none' mask='url(#capped-line-mask-{svgId})' />
                    <path d='M -.2 -.3 .4 0 -.2 .3z' fill='black' stroke='none' transform='translate({svgX(l)}, {svgY(l)}) rotate({angleDeg(sl, l)})' />
                    <path d='M -.2 -.3 .4 0 -.2 .3z' fill='black' stroke='none' transform='translate({svgX(f)}, {svgY(f)}) rotate({angleDeg(s, f)})' />
                </g>";
            }
        }

        public override bool Verify(int[] grid)
        {
            var min = Math.Min(grid[Cells[0]], grid[Cells[Cells.Length - 1]]);
            var max = Math.Max(grid[Cells[0]], grid[Cells[Cells.Length - 1]]);
            return Cells.Skip(1).SkipLast(1).All(c => grid[c] > min && grid[c] < max);
        }
        public override bool IncludesCell(int cell) => Cells.Contains(cell);

        public override bool ClashesWith(KyuConstraint other) => other switch
        {
            KyuCellConstraint cc => Cells.Contains(cc.Cell),
            RenbanCage rb => Cells.Intersect(rb.Cells).Any(),
            Thermometer th => Cells.Intersect(th.Cells).Any(),
            Arrow ar => Cells.Intersect(ar.Cells).Any(),
            Palindrome pa => Cells.Intersect(pa.Cells).Any(),
            CappedLine cl => Cells.Intersect(cl.Cells).Any(),
            _ => false,
        };

        public static IList<KyuConstraint> Generate(int[] sudoku)
        {
            var list = new List<KyuConstraint>();

            IEnumerable<CappedLine> recurse(int[] sofar)
            {
                if (sofar.Length >= 3 && sofar.SkipLast(1).All(c => sudoku[c] < sudoku[sofar.Last()]))
                    yield return new CappedLine(sofar);
                if (sofar.Length >= 9)
                    yield break;

                bool noDiagonalCrossingExists(int x1, int y1, int x2, int y2)
                {
                    var p1 = Array.IndexOf(sofar, x1 + 9 * y2);
                    var p2 = Array.IndexOf(sofar, x2 + 9 * y1);
                    return p1 == -1 || p2 == -1 || Math.Abs(p1 - p2) != 1;
                }

                bool isSmallTurn(int x1, int y1, int x2, int y2, int x3, int y3)
                {
                    var dx = (x3 - x2) - (x2 - x1);
                    var dy = (y3 - y2) - (y2 - y1);
                    return (Math.Abs(dx) <= 1 && dy == 0) || (Math.Abs(dy) <= 1 && dx == 0);
                }

                var last = sofar[sofar.Length - 1];
                if (last == 9)
                    yield break;
                var secondLast = sofar.Length == 1 ? 0 : sofar[sofar.Length - 2];
                foreach (var adj in Adjacent(last))
                    if (!sofar.Contains(adj) && sudoku[adj] > sudoku[sofar[0]] && noDiagonalCrossingExists(last % 9, last / 9, adj % 9, adj / 9)
                        && (sofar.Length == 1 || isSmallTurn(secondLast % 9, secondLast / 9, last % 9, last / 9, adj % 9, adj / 9)))
                        foreach (var next in recurse(sofar.Insert(sofar.Length, adj)))
                            yield return next;
            }

            for (var startCell = 0; startCell < 81; startCell++)
                list.AddRange(recurse(new[] { startCell }));
            return list;
        }

        sealed class CappedLineConstraint : Constraint
        {
            public int Cap1 { get; private set; }
            public int Cap2 { get; private set; }
            public int[] InnerCells { get; private set; }
            public CappedLineConstraint(int cap1, int cap2, int[] innerCells) : base(innerCells.Concat(cap1).Concat(cap2))
            {
                if (cap1 == cap2)
                    throw new ArgumentException("cap1 and cap2 can’t be equal.", nameof(cap2));
                Cap1 = cap1;
                Cap2 = cap2;
                InnerCells = innerCells ?? throw new ArgumentNullException(nameof(innerCells));
            }

            public override IEnumerable<Constraint> MarkTakens(bool[][] takens, int?[] grid, int? ix, int minValue, int maxValue)
            {
                if (ix == null)
                {
                    for (var icIx = 0; icIx < InnerCells.Length; icIx++)
                    {
                        takens[InnerCells[icIx]][0] = true;
                        takens[InnerCells[icIx]][takens[InnerCells[icIx]].Length - 1] = true;
                    }
                    return null;
                }

                // If both caps are filled in, all the inner cells must simply be between them.
                if (grid[Cap1] != null && grid[Cap2] != null)
                {
                    // We don’t need to recompute this multiple times.
                    if (!(ix == Cap1 || ix == Cap2))
                        return null;
                    var min = Math.Min(grid[Cap1].Value, grid[Cap2].Value) + minValue;
                    var max = Math.Max(grid[Cap1].Value, grid[Cap2].Value) + minValue;
                    for (var icIx = 0; icIx < InnerCells.Length; icIx++)
                        if (grid[InnerCells[icIx]] == null)
                            for (var v = 0; v < takens[InnerCells[icIx]].Length; v++)
                                if (v + minValue <= min || v + minValue >= max)
                                    takens[InnerCells[icIx]][v] = true;
                }
                // If one cap is filled in, all the inner cells must simply be different from it.
                else if (grid[Cap1] != null || grid[Cap2] != null)
                {
                    // We don’t need to recompute this multiple times.
                    if (!(ix == Cap1 || ix == Cap2))
                        return null;
                    var v = grid[grid[Cap1] != null ? Cap1 : Cap2].Value;
                    for (var icIx = 0; icIx < InnerCells.Length; icIx++)
                        takens[InnerCells[icIx]][v] = true;
                }

                var curMin = InnerCells.Where(c => grid[c] != null).MinOrNull(c => grid[c].Value + minValue);
                if (curMin == null)
                    return null;
                var curMax = InnerCells.Where(c => grid[c] != null).Max(c => grid[c].Value) + minValue;

                // If neither cap is filled in, both must be outside the range but we don’t yet know which one is the min and which one is the max
                if (grid[Cap1] == null && grid[Cap2] == null)
                {
                    for (var v = 0; v < takens[Cap1].Length; v++)
                        if (v + minValue >= curMin.Value && v + minValue <= curMax)
                            takens[Cap1][v] = true;
                    for (var v = 0; v < takens[Cap2].Length; v++)
                        if (v + minValue >= curMin.Value && v + minValue <= curMax)
                            takens[Cap2][v] = true;
                }
                else
                {
                    // Check if Cap1 is filled in and Cap2 not, and then also check the reverse
                    var cap1 = Cap1;
                    var cap2 = Cap2;
                    iter:
                    if (grid[cap1] == null && grid[cap2] != null)
                    {
                        if (grid[cap2].Value + minValue > curMax)
                        {
                            // grid[cap1] must be < curMin
                            for (var v = 0; v < takens[cap1].Length; v++)
                                if (v + minValue >= curMin.Value)
                                    takens[cap1][v] = true;
                        }
                        else if (grid[cap2].Value + minValue < curMin.Value)
                        {
                            // grid[cap1] must be > curMax
                            for (var v = 0; v < takens[cap1].Length; v++)
                                if (v + minValue <= curMax)
                                    takens[cap1][v] = true;
                        }
                        else
                            throw new InternalErrorException("CappedLineConstraint encountered an internal bug.");
                    }
                    if (cap1 == Cap1)
                    {
                        cap1 = Cap2;
                        cap2 = Cap1;
                        goto iter;
                    }
                }
                return null;
            }
        }
    }
}
