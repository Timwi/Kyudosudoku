using RT.Util;

namespace KyudosudokuWebsite
{
    abstract class KyuRowColConstraint : KyuConstraint
    {
        public bool IsCol { get; private set; }
        public int RowCol { get; private set; }

        /// <summary>
        ///     Determines whether the constraint is visually shown on the top of a column/left of a row (<c>true</c>) or the
        ///     bottom of a column/right of a row (<c>false</c>).</summary>
        public abstract bool ShownTopLeft { get; }

        public KyuRowColConstraint(bool isCol, int rowCol) { IsCol = isCol; RowCol = rowCol; }
        protected KyuRowColConstraint() { }    // for Classify

        public override bool ClashesWith(KyuConstraint other) => other is KyuRowColConstraint cc && IsCol == cc.IsCol && RowCol == cc.RowCol && ShownTopLeft == cc.ShownTopLeft;
        protected int[] GetAffectedCells(bool reverse) => Ut.NewArray(9, x => IsCol ? (RowCol + 9 * (reverse ? 8 - x : x)) : ((reverse ? 8 - x : x) + 9 * RowCol));
    }
}
