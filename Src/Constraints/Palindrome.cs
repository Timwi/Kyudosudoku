using System;
using System.Collections.Generic;
using System.Linq;
using PuzzleSolvers;
using RT.Util;
using RT.Util.ExtensionMethods;

namespace KyudosudokuWebsite
{
    sealed class Palindrome : KyuConstraint
    {
        public override string Name => "Palindrome";
        public override string Description => "The digits along the line must form a palindrome (same sequence of digits when read from either end).";

        public int[] Cells { get; private set; }

        public Palindrome(int[] cells) { Cells = cells; }
        private Palindrome() { }   // for Classify

        protected override Constraint getConstraint() => new CloneConstraint(Cells.Subarray(0, Cells.Length / 2), Cells.Subarray((Cells.Length + 1) / 2).ReverseInplace());

        public override string Svg => $@"<g opacity='.2'>
            <path d='M{Cells.Select(c => $"{svgX(c)} {svgY(c)}").JoinString(" ")}' stroke='black' stroke-width='.2' stroke-linecap='square' stroke-linejoin='bevel' fill='none' />
            {(Cells.Length % 2 == 1 ? svgP(Cells[Cells.Length / 2]) : (svgP(Cells[Cells.Length / 2 - 1]) + svgP(Cells[Cells.Length / 2])) / 2).Apply(p => $"<path d='M -.4 0 0 -.4 .4 0 0 .4 z' transform='translate({p.X}, {p.Y})' fill='black'/>")}
        </g>";

        public override bool Verify(int[] grid)
        {
            for (var i = 0; i < Cells.Length / 2; i++)
                if (grid[Cells[i]] != grid[Cells[Cells.Length - i]])
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

            IEnumerable<Palindrome> recurse(int[] cells1, int[] cells2)
            {
                bool noDiagonalCrossingExists(int[] arr, int c1, int c2)
                {
                    var p1 = Array.IndexOf(arr, c1 % 9 + 9 * (c2 / 9));
                    var p2 = Array.IndexOf(arr, c2 % 9 + 9 * (c1 / 9));
                    return p1 == -1 || p2 == -1 || Math.Abs(p1 - p2) != 1;
                }

                if (cells1.Length >= 3)
                    yield return new Palindrome((Adjacent(cells1[1]).Contains(cells2[1]) ? cells1.Skip(1) : cells1).Reverse().Concat(cells2.Skip(1)).ToArray());

                if (cells1.Length > 4)
                    yield break;

                foreach (var adj1 in Adjacent(cells1.Last()))
                    if (!cells1.Contains(adj1) && !cells2.Contains(adj1) && noDiagonalCrossingExists(cells1, cells1.Last(), adj1))
                        foreach (var adj2 in Adjacent(cells2.Last()))
                            if (adj2 != adj1 && sudoku[adj1] == sudoku[adj2] && !cells1.Contains(adj2) && !cells2.Contains(adj2) && noDiagonalCrossingExists(cells2, cells2.Last(), adj2))
                                foreach (var item in recurse(cells1.Insert(cells1.Length, adj1), cells2.Insert(cells2.Length, adj2)))
                                    yield return item;
            }

            for (var startCell = 0; startCell < 81; startCell++)
                list.AddRange(recurse(new[] { startCell }, new[] { startCell }));
            return list;
        }
    }
}
