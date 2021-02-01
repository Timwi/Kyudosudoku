﻿using System;
using System.Collections.Generic;
using System.Linq;
using PuzzleSolvers;
using RT.Util;
using RT.Util.ExtensionMethods;

namespace KyudosudokuWebsite
{
    sealed class Snowball : KyuConstraint
    {
        public override string Name => "Snowball";
        public override string Description => "One of the regions contains the same digits as the other, plus or minus a consistent addend. For example, if one region contains 1, 5, 7, the other might contain 2, 6, 8 or 3, 7, 9.";

        public int[] Cells1 { get; private set; }
        public int[] Cells2 { get; private set; }
        public Snowball(int[] cells1, int[] cells2) { Cells1 = cells1; Cells2 = cells2; }
        private Snowball() { }      // for Classify

        protected override Constraint getConstraint() => new OffsetCloneConstraint(Cells1, Cells2);

        public override string Svg => GenerateSvgPath(Cells1, 0, 0).Apply(path1 => GenerateSvgPath(Cells2, 0, 0).Apply(path2 => $@"
            <filter id='snowball-filter-a' color-interpolation-filters='sRGB'>
                <feGaussianBlur result='fbSourceGraphic' stdDeviation='.02' />
            </filter>
            <clipPath id='snowball-clip-a' clipPathUnits='userSpaceOnUse'><path d='{path1}' /></clipPath>
            <clipPath id='snowball-clip-b' clipPathUnits='userSpaceOnUse'><path d='{path2}' /></clipPath>
            <path d='{path1}' fill='none' stroke='black' stroke-width='.075' filter='url(#snowball-filter-a)' clip-path='url(#snowball-clip-a)' />
            <path d='{path2}' fill='none' stroke='black' stroke-width='.075' filter='url(#snowball-filter-a)' clip-path='url(#snowball-clip-b)' />
        "));

        public override bool Verify(int[] grid)
        {
            throw new NotImplementedException();
        }

        public override bool ClashesWith(KyuConstraint other) => other switch
        {
            RenbanCage => false,
            KillerCage => false,
            KyuRegionConstraint kr => Cells1.Concat(Cells2).Intersect(kr.Cells).Any(),
            _ => false
        };

        public static IList<KyuConstraint> Generate(int[] sudoku)
        {
            IEnumerable<Snowball> generateRegions(bool[] sofar1, bool[] sofar2, int dx, int dy, int offset, bool[] banned, int count)
            {
                if (count >= 2)
                    yield return new Snowball(sofar1.SelectIndexWhere(b => b).ToArray(), sofar2.SelectIndexWhere(b => b).ToArray());

                for (var adj = 0; adj < 81; adj++)
                {
                    if (banned[adj] || adj % 9 + dx < 0 || adj % 9 + dx >= 9 || adj / 9 + dy < 0 || adj / 9 + dy >= 9 || !Orthogonal(adj).Any(a => sofar1[a]) || sudoku[adj] + offset != sudoku[adj + dx + 9 * dy])
                        continue;
                    sofar1[adj] = true;
                    sofar2[adj + dx + 9 * dy] = true;
                    banned[adj] = true;
                    banned[adj + dx + 9 * dy] = true;
                    foreach (var item in generateRegions(sofar1, sofar2, dx, dy, offset, (bool[]) banned.Clone(), count + 1))
                        yield return item;
                    sofar1[adj] = false;
                    sofar2[adj + dx + 9 * dy] = false;
                    banned[adj + dx + 9 * dy] = false;
                }
            }
            var list = new List<KyuConstraint>();
            for (var s1 = 0; s1 < 81; s1++)
                for (var s2 = s1 + 1; s2 < 81; s2++)
                    list.AddRange(generateRegions(Ut.NewArray(81, x => x == s1), Ut.NewArray(81, x => x == s2), s2 % 9 - s1 % 9, s2 / 9 - s1 / 9, sudoku[s2] - sudoku[s1], Ut.NewArray(81, x => x <= s1 || x == s2), 1));
            return list;
        }
    }
}
