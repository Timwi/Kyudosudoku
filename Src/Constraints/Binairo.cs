using System.Collections.Generic;
using PuzzleSolvers;
using RT.Util;
using RT.Util.ExtensionMethods;

namespace KyudosudokuWebsite
{
    sealed class Binairo : KyuRowColConstraint
    {
        public override string Name => "Binairo";
        public override string Description => $"In this {(IsCol ? "column" : "row")}, no three adjacent digits can be all odd or all even.";
        public override bool ShownTopLeft => true;
        public override double ExtraTop => IsCol ? .5 : 0;

        public Binairo(bool isCol, int rowCol) : base(isCol, rowCol) { }
        private Binairo() { }   // for Classify

        protected override Constraint getConstraint() => new BinairoRowConstraint(GetAffectedCells(false));

        public override string Svg => $@"<g stroke='black' stroke-width='.075' fill='none' transform='translate({Kyudosudoku.SudokuX + (IsCol ? RowCol + .5 : -.5)}, {Kyudosudoku.SudokuY + (IsCol ? -.5 : RowCol + .5)}) scale(.7)'>
            <circle cx='-.25' cy='-.25' r='.2' />
            <circle cx='.25' cy='.25' r='.2' />
            <path d='M .25 -.45 v .4 M -.25 .05 v .4' />
        </g>";

        public override bool Verify(int[] grid) => VerifyBinairo(GetAffectedCells(false).Select(c => grid[c]).ToArray());

        public static bool VerifyBinairo(int[] numbers)
        {
            for (var i = 1; i < numbers.Length - 1; i++)
                if (numbers[i - 1] % 2 == numbers[i] % 2 && numbers[i + 1] % 2 == numbers[i] % 2)
                    return false;
            return true;
        }

        public static IList<KyuConstraint> Generate(int[] sudoku)
        {
            var constraints = new List<KyuConstraint>();
            foreach (var isCol in new[] { false, true })
                for (var rowCol = 0; rowCol < 9; rowCol++)
                    if (VerifyBinairo(Ut.NewArray(9, x => isCol ? (rowCol + 9 * x) : (x + 9 * rowCol)).Select(ix => sudoku[ix]).ToArray()))
                        constraints.Add(new Binairo(isCol, rowCol));
            return constraints;
        }

        public sealed class BinairoRowConstraint : Constraint
        {
            private static readonly (int offset, int toEnforce)[] _combinations = new (int offset, int toEnforce)[] { (-2, -1), (-1, -2), (-1, 1), (1, -1), (1, 2), (2, 1) };
            public BinairoRowConstraint(int[] affectedCells) : base(affectedCells) { }
            public override IEnumerable<Constraint> MarkTakens(bool[][] takens, int?[] grid, int? ix, int minValue, int maxValue)
            {
                if (ix == null)
                    return null;

                var i = ix.Value;
                var x = AffectedCells.IndexOf(i);

                //if ((i == 5 + 9 * 7 && grid[i] + minValue == 1) || (i == 6 + 9 * 7 && grid[i] + minValue == 9) || (i == 7 + 9 * 7 && grid[i] + minValue == 5))
                //    System.Diagnostics.Debugger.Break();

                foreach (var (offset, toEnforce) in _combinations)
                {
                    if (x + offset >= 0 && x + offset < 9 && x + toEnforce >= 0 && x + toEnforce < 9 && grid[AffectedCells[x + offset]] != null && grid[AffectedCells[x + offset]].Value % 2 == grid[i].Value % 2 && grid[AffectedCells[x + toEnforce]] == null)
                        for (var v = 0; v < takens[AffectedCells[x + toEnforce]].Length; v++)
                            if (v % 2 == grid[i].Value % 2)
                                takens[AffectedCells[x + toEnforce]][v] = true;
                }
                return null;
            }

            //public sealed class BinairoRowConstraint : Constraint
            //{
            //    public BinairoRowConstraint(int[] affectedCells) : base(affectedCells) { }
            //    public override IEnumerable<Constraint> MarkTakens(bool[][] takens, int?[] grid, int? ix, int minValue, int maxValue)
            //    {
            //        if (ix == null)
            //            return null;

            //        var pos = Array.IndexOf(AffectedCells, ix.Value);
            //        if (pos == -1)
            //            return null;

            //        // Sandwich between pos-2 and pos
            //        if (pos > 1 && grid[AffectedCells[pos - 2]] != null && grid[AffectedCells[pos - 2]].Value % 2 == grid[ix.Value].Value % 2)
            //            for (var v = 0; v < takens[AffectedCells[pos]].Length; v++)
            //                if ((v + minValue) % 2 == grid[ix.Value].Value % 2)
            //                    takens[AffectedCells[pos - 1]][v] = true;

            //        // Either side of pos-1 and pos
            //        if (pos > 0 && grid[AffectedCells[pos - 1]] != null && grid[AffectedCells[pos - 1]].Value % 2 == grid[ix.Value].Value % 2)
            //            for (var v = 0; v < takens[AffectedCells[pos]].Length; v++)
            //            {
            //                if ((v + minValue) % 2 == grid[ix.Value].Value % 2 && pos > 1)
            //                    takens[AffectedCells[pos - 2]][v] = true;
            //                if ((v + minValue) % 2 == grid[ix.Value].Value % 2 && pos < AffectedCells.Length - 1)
            //                    takens[AffectedCells[pos + 1]][v] = true;
            //            }

            //        // Either side of pos and pos+1
            //        if (pos < AffectedCells.Length - 1 && grid[AffectedCells[pos + 1]] != null && grid[AffectedCells[pos + 1]].Value % 2 == grid[ix.Value].Value % 2)
            //            for (var v = 0; v < takens[AffectedCells[pos]].Length; v++)
            //            {
            //                if ((v + minValue) % 2 == grid[ix.Value].Value % 2 && pos > 0)
            //                    takens[AffectedCells[pos - 1]][v] = true;
            //                if ((v + minValue) % 2 == grid[ix.Value].Value % 2 && pos < AffectedCells.Length - 2)
            //                    takens[AffectedCells[pos + 2]][v] = true;
            //            }

            //        // Sandwich between pos and pos+2
            //        if (pos < AffectedCells.Length - 2 && grid[AffectedCells[pos + 2]] != null && grid[AffectedCells[pos + 2]].Value % 2 == grid[ix.Value].Value % 2)
            //            for (var v = 0; v < takens[AffectedCells[pos]].Length; v++)
            //                if ((v + minValue) % 2 == grid[ix.Value].Value % 2)
            //                    takens[AffectedCells[pos + 1]][v] = true;

            //        return null;
            //    }
        }
    }
}
