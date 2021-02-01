using System.Collections.Generic;
using System.Linq;
using PuzzleSolvers;
using RT.Util.ExtensionMethods;

namespace KyudosudokuWebsite
{
    sealed class LittleKiller : KyuConstraint
    {
        public override string Name => "Diagonal sum";
        public override string Description => "The digits along the indicated diagonal must sum up to the specified total. (The digits need not necessarily be unique.)";

        public override double ExtraRight => Direction == ClueDirection.SouthWest ? .25 : 0;
        public override double ExtraTop => Direction == ClueDirection.SouthEast ? .25 : 0;

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
        protected override Constraint getConstraint() => new SumConstraint(Sum, AffectedCells);
        public override bool Verify(int[] grid) => AffectedCells.Sum(c => grid[c]) == Sum;

        // SouthEast, 7
        public override string Svg
        {
            get
            {
                const double arrLen = .275;
                const double arrWidth = .2;
                const double margin = .1;
                string path = null, text = null;
                switch (Direction)
                {
                    case ClueDirection.SouthEast:
                        path = $"<path d='M {Offset - margin - arrLen} {-margin - arrLen} {Offset - margin} {-margin} M {Offset - margin - arrWidth} {-margin} h {arrWidth} v {-arrWidth}' stroke='black' stroke-width='.02' fill='none' />";
                        text = $"<text text-anchor='middle' x='{Offset - margin - arrLen - margin}' y='{-margin - arrLen - margin + .05}' font-size='.3'>{Sum}</text>";
                        break;
                    case ClueDirection.SouthWest:
                        path = $"<path d='M {9 + margin + arrLen} {Offset - margin - arrLen} {9 + margin} {Offset - margin} M {9 + margin} {Offset - margin - arrWidth} v {arrWidth} h {arrWidth}' stroke='black' stroke-width='.02' fill='none' />";
                        text = $"<text text-anchor='middle' x='{9 + margin + arrLen + margin}' y='{Offset - margin - arrLen - margin + .05}' font-size='.3'>{Sum}</text>";
                        break;
                    case ClueDirection.NorthWest:
                        path = $"<path d='M {9 - Offset + margin + arrLen} {9 + margin + arrLen} {9 - Offset + margin} {9 + margin} M {9 - Offset + margin + arrWidth} {9 + margin} h {-arrWidth} v {arrWidth}' stroke='black' stroke-width='.02' fill='none' />";
                        text = $"<text text-anchor='middle' x='{9 - Offset + margin + arrLen + margin}' y='{9 + margin + arrLen + margin + .2}' font-size='.3'>{Sum}</text>";
                        break;
                    case ClueDirection.NorthEast:
                        path = $"<path d='M {-margin - arrLen} {9 - Offset + margin + arrLen} {-margin} {9 - Offset + margin} M {-margin - arrWidth} {9 - Offset + margin} h {arrWidth} v {arrWidth}' stroke='black' stroke-width='.02' fill='none' />";
                        text = $"<text text-anchor='middle' x='{-margin - arrLen - margin}' y='{9 - Offset + margin + arrLen + margin + .2}' font-size='.3'>{Sum}</text>";
                        break;
                }
                return $"<g transform='translate({Kyudosudoku.SudokuX}, {Kyudosudoku.SudokuY})'>{path}{text}</g>";
            }
        }

        public override bool ClashesWith(KyuConstraint other) => other switch
        {
            KyuRowColConstraint rc => Direction switch
            {
                ClueDirection.SouthEast => rc.IsCol && rc.RowCol == Offset - 1 && rc.ShownTopLeft,
                ClueDirection.SouthWest => !rc.IsCol && rc.RowCol == Offset - 1 && !rc.ShownTopLeft,
                ClueDirection.NorthWest => rc.IsCol && rc.RowCol == 9 - Offset && !rc.ShownTopLeft,
                ClueDirection.NorthEast => !rc.IsCol && rc.RowCol == 9 - Offset && rc.ShownTopLeft,
                _ => false
            },
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
