using System;
using System.Collections.Generic;
using System.Linq;
using PuzzleSolvers;
using RT.Util.ExtensionMethods;

namespace KyudosudokuWebsite
{
    sealed class Thermometer : KyuConstraint
    {
        public override string Name => "Thermometer";
        public override string Description => "Digits must increase from the bulb.";

        public int[] Cells { get; private set; }

        public Thermometer(int[] cells) { Cells = cells; }
        private Thermometer() { }   // for Classify

        protected override Constraint getConstraint() => new LessThanConstraint(Cells);

        public override string Svg => $@"<g opacity='.2' transform='translate({Kyudosudoku.SudokuX}, {Kyudosudoku.SudokuY})'>
            <path d='M{Cells.Select(c => $"{c % 9 + .5} {c / 9 + .5}").JoinString(" ")}' stroke='black' stroke-width='.3' stroke-linecap='round' stroke-linejoin='round' fill='none' />
            <circle cx='{Cells[0] % 9 + .5}' cy='{Cells[0] / 9 + .5}' r='.4' fill='black' />
        </g>";

        public override bool Verify(int[] grid)
        {
            for (var i = 1; i < Cells.Length; i++)
                if (grid[Cells[i]] <= grid[Cells[i - 1]])
                    return false;
            return true;
        }

        public override bool ClashesWith(KyuConstraint other) => other switch
        {
            KyuCellConstraint cc => Cells.Contains(cc.Cell),
            Thermometer th => Cells.Intersect(th.Cells).Any(),
            Palindrome pali => Cells.Intersect(pali.Cells).Any(),
            _ => false,
        };

        public static IList<KyuConstraint> Generate(int[] sudoku)
        {
            var list = new List<KyuConstraint>();

            IEnumerable<Thermometer> recurse(int[] sofar)
            {
                if (sofar.Length >= 3)
                    yield return new Thermometer(sofar);

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
                            if (yy >= 0 && yy < 9 && !sofar.Contains(xx + 9 * yy) && sudoku[xx + 9 * yy] > sudoku[sofar.Last()]
                                && (xx == x || yy == y || noDiagonalCrossingExists(x, y, xx, yy)))
                                foreach (var next in recurse(sofar.Insert(sofar.Length, xx + 9 * yy)))
                                    yield return next;
            }

            for (var startCell = 0; startCell < 81; startCell++)
                list.AddRange(recurse(new[] { startCell }));
            return list;
        }
    }
}
