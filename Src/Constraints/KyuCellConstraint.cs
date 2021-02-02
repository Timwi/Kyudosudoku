namespace KyudosudokuWebsite
{
    abstract class KyuCellConstraint : KyuConstraint
    {
        public int Cell { get; private set; }

        public KyuCellConstraint(int cell) { Cell = cell; }
        protected KyuCellConstraint() { }    // for Classify

        public override bool ClashesWith(KyuConstraint other) => other is KyuCellConstraint;
        public sealed override bool IncludesCell(int cell) => cell == Cell;
    }
}
