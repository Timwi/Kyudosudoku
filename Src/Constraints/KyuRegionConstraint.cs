namespace KyudosudokuWebsite
{
    abstract class KyuRegionConstraint : KyuConstraint
    {
        public int[] Cells { get; private set; }

        public KyuRegionConstraint(int[] cells) { Cells = cells; }
        protected KyuRegionConstraint() { }    // for Classify
    }
}
