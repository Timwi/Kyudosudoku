using System.Collections.Generic;
using PuzzleSolvers;
using RT.Serialization;
using RT.Util.Geometry;

namespace KyudosudokuWebsite
{
    abstract class KyuConstraint
    {
        public abstract string Name { get; }
        public abstract string Description { get; }
        protected abstract Constraint getConstraint();
        public abstract string Svg { get; }
        public virtual bool SvgAboveLines => false;
        public abstract bool Verify(int[] grid);

        public virtual double ExtraTop => 0;
        public virtual double ExtraRight => 0;

        [ClassifyIgnore]
        private Constraint _cachedConstraint;
        public Constraint GetConstraint() => _cachedConstraint ??= getConstraint();

        /// <summary>Determines whether a constraint visually clashes with another (overlaps in an undesirable way).</summary>
        public abstract bool ClashesWith(KyuConstraint other);

        protected KyuConstraint() { }    // for Classify

        protected static double svgX(int cell) => Kyudosudoku.SudokuX + cell % 9 + .5;
        protected static double svgY(int cell) => Kyudosudoku.SudokuY + cell / 9 + .5;
        protected static PointD svgP(int cell) => new PointD(svgX(cell), svgY(cell));

        protected static IEnumerable<int> adjacent(int cell)
        {
            var x = cell % 9;
            var y = cell / 9;
            for (var xx = x - 1; xx <= x + 1; xx++)
                if (xx >= 0 && xx < 9)
                    for (var yy = y - 1; yy <= y + 1; yy++)
                        if (yy >= 0 && yy < 9)
                            yield return xx + 9 * yy;
        }

        protected static IEnumerable<int> orthogonal(int cell)
        {
            var x = cell % 9;
            var y = cell / 9;
            for (var xx = x - 1; xx <= x + 1; xx++)
                if (xx >= 0 && xx < 9)
                    for (var yy = y - 1; yy <= y + 1; yy++)
                        if (yy >= 0 && yy < 9 && (xx == x || yy == y))
                            yield return xx + 9 * yy;
        }
    }
}
