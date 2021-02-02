using System.Linq;

namespace KyudosudokuWebsite
{
    abstract class KyuRegionConstraint : KyuConstraint
    {
        public int[] Cells { get; private set; }

        public KyuRegionConstraint(int[] cells) { Cells = cells; }
        protected KyuRegionConstraint() { }    // for Classify

        public sealed override bool IncludesCell(int cell) => Cells.Contains(cell);
    }
}
