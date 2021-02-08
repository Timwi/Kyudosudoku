using System.Collections.Generic;
using System.Linq;
using PuzzleSolvers;
using RT.Util.ExtensionMethods;

namespace KyudosudokuWebsite
{
    [KyuConstraintInfo("Diagonal sum")]
    sealed class LittleKiller : KyuConstraint
    {
        public override string Description => "The digits along the indicated diagonal must sum to the specified total. (The digits need not necessarily be different.)";
        public override double ExtraRight => Direction == ClueDirection.SouthWest ? .25 : 0;
        public override double ExtraTop => Direction == ClueDirection.SouthEast ? .25 : 0;
        public override bool IncludesCell(int cell) => false;
        public override bool IncludesRowCol(bool isCol, int rowCol, bool topLeft) => Direction switch
        {
            ClueDirection.SouthEast => isCol && rowCol == Offset - 1 && topLeft,
            ClueDirection.SouthWest => !isCol && rowCol == Offset - 1 && !topLeft,
            ClueDirection.NorthWest => isCol && rowCol == 9 - Offset && !topLeft,
            ClueDirection.NorthEast => !isCol && rowCol == 9 - Offset && topLeft,
            _ => false
        };
        public static readonly Example Example = new Example
        {
            Constraints = { new LittleKiller(ClueDirection.NorthEast, 7, 8) },
            Cells = { 1, 9 },
            Good = { 2, 6 },
            Bad = { 3, 6 }
        };

        public enum ClueDirection { SouthEast, SouthWest, NorthWest, NorthEast }
        public ClueDirection Direction { get; private set; }
        public int Offset { get; private set; }
        public int Sum { get; private set; }

        public LittleKiller(ClueDirection dir, int offset, int sum) { Direction = dir; Offset = offset; Sum = sum; }
        private LittleKiller() { }  // for Classify

        public int[] AffectedCells => GetAffectedCells(Direction, Offset);
        private static int[] GetAffectedCells(ClueDirection dir, int offset) => dir switch
        {
            ClueDirection.SouthEast => Enumerable.Range(0, 9 - offset).Select(i => offset + 10 * i).ToArray(),
            ClueDirection.SouthWest => Enumerable.Range(0, 9 - offset).Select(i => 8 + 9 * offset + 8 * i).ToArray(),
            ClueDirection.NorthWest => Enumerable.Range(0, 9 - offset).Select(i => 80 - offset - 10 * i).ToArray(),
            ClueDirection.NorthEast => Enumerable.Range(0, 9 - offset).Select(i => 72 - 9 * offset - 8 * i).ToArray(),
            _ => null,
        };
        protected override IEnumerable<Constraint> getConstraints() { yield return new SumConstraint(Sum, AffectedCells); }
        public override bool Verify(int[] grid) => AffectedCells.Sum(c => grid[c]) == Sum;

        const double svgArrLen = .275;
        const double svgArrWidth = .2;
        const double svgMargin = .1;
        public override string Svg => Direction switch
        {
            ClueDirection.SouthEast =>
                $"<path d='M {Offset - svgMargin - svgArrLen} {-svgMargin - svgArrLen} {Offset - svgMargin} {-svgMargin} M {Offset - svgMargin - svgArrWidth} {-svgMargin} h {svgArrWidth} v {-svgArrWidth}' stroke='black' stroke-width='.02' fill='none' />" +
                $"<text text-anchor='middle' x='{Offset - svgMargin - svgArrLen - svgMargin}' y='{-svgMargin - svgArrLen - svgMargin + .05}' font-size='.3'>{Sum}</text>",
            ClueDirection.SouthWest =>
                $"<path d='M {9 + svgMargin + svgArrLen} {Offset - svgMargin - svgArrLen} {9 + svgMargin} {Offset - svgMargin} M {9 + svgMargin} {Offset - svgMargin - svgArrWidth} v {svgArrWidth} h {svgArrWidth}' stroke='black' stroke-width='.02' fill='none' />" +
                $"<text text-anchor='middle' x='{9 + svgMargin + svgArrLen + svgMargin}' y='{Offset - svgMargin - svgArrLen - svgMargin + .05}' font-size='.3'>{Sum}</text>",
            ClueDirection.NorthWest =>
                $"<path d='M {9 - Offset + svgMargin + svgArrLen} {9 + svgMargin + svgArrLen} {9 - Offset + svgMargin} {9 + svgMargin} M {9 - Offset + svgMargin + svgArrWidth} {9 + svgMargin} h {-svgArrWidth} v {svgArrWidth}' stroke='black' stroke-width='.02' fill='none' />" +
                $"<text text-anchor='middle' x='{9 - Offset + svgMargin + svgArrLen + svgMargin}' y='{9 + svgMargin + svgArrLen + svgMargin + .2}' font-size='.3'>{Sum}</text>",
            ClueDirection.NorthEast =>
                $"<path d='M {-svgMargin - svgArrLen} {9 - Offset + svgMargin + svgArrLen} {-svgMargin} {9 - Offset + svgMargin} M {-svgMargin - svgArrWidth} {9 - Offset + svgMargin} h {svgArrWidth} v {svgArrWidth}' stroke='black' stroke-width='.02' fill='none' />" +
                $"<text text-anchor='middle' x='{-svgMargin - svgArrLen - svgMargin}' y='{9 - Offset + svgMargin + svgArrLen + svgMargin + .2}' font-size='.3'>{Sum}</text>",
            _ => null
        };

        public override bool ClashesWith(KyuConstraint other) => other switch
        {
            KyuRowColConstraint rc => IncludesRowCol(rc.IsCol, rc.RowCol, rc.ShownTopLeft),
            LittleKiller lk => lk.Direction == Direction && lk.Offset == Offset,
            _ => false
        };

        public static IList<KyuConstraint> Generate(int[] sudoku)
        {
            var list = new List<KyuConstraint>();
            for (var offset = 0; offset < 8; offset++)
                list.Add(new LittleKiller(ClueDirection.SouthEast, offset, GetAffectedCells(ClueDirection.SouthEast, offset).Sum(c => sudoku[c])));
            for (var offset = 1; offset < 8; offset++)
                list.Add(new LittleKiller(ClueDirection.SouthWest, offset, GetAffectedCells(ClueDirection.SouthWest, offset).Sum(c => sudoku[c])));
            for (var offset = 1; offset < 8; offset++)
                list.Add(new LittleKiller(ClueDirection.NorthWest, offset, GetAffectedCells(ClueDirection.NorthWest, offset).Sum(c => sudoku[c])));
            for (var offset = 0; offset < 8; offset++)
                list.Add(new LittleKiller(ClueDirection.NorthEast, offset, GetAffectedCells(ClueDirection.NorthEast, offset).Sum(c => sudoku[c])));
            return list;
        }
    }
}
