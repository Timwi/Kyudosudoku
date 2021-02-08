using System.Collections.Generic;

namespace KyudosudokuWebsite
{
    [KyuConstraintInfo("Double neighbors")]
    sealed class DoubleNeighbors : KyuNeighborConstraint
    {
        public override string Description => "The two marked cells must have a ratio of 2 (one is double the other).";
        public static readonly Example Example = new Example
        {
            Constraints = { new DoubleNeighbors(10, 11) },
            Cells = { 10, 11 },
            Good = { 2, 4 },
            Bad = { 2, 6 },
            Reason = "6 is not twice 2, and 2 is not twice 6."
        };

        public DoubleNeighbors(int cell1, int cell2) : base(cell1, cell2) { }
        private DoubleNeighbors() { }   // for Classify

        public override string Svg => $"<circle cx='{x}' cy='{y}' r='.15' stroke='white' stroke-width='.02' fill='black' />";
        protected override bool verify(int a, int b) => myVerify(a, b);
        private static bool myVerify(int a, int b) => a * 2 == b || b * 2 == a;

        public static IList<KyuConstraint> Generate(int[] sudoku) => generate(sudoku, myVerify, (cell1, cell2) => new DoubleNeighbors(cell1, cell2));
    }
}
