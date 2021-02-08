using System;
using System.Collections.Generic;

namespace KyudosudokuWebsite
{
    [KyuConstraintInfo("Consecutive neighbors")]
    sealed class ConsecutiveNeighbors : KyuNeighborConstraint
    {
        public override string Description => "The two marked cells must be consecutive (have a difference of 1).";
        public static readonly Example Example = new Example
        {
            Constraints = { new ConsecutiveNeighbors(10, 11) },
            Cells = { 10, 11 },
            Good = { 3, 4 },
            Bad = { 3, 7 },
            Reason = "3 and 7 are not consecutive numbers."
        };

        public ConsecutiveNeighbors(int cell1, int cell2) : base(cell1, cell2) { }
        private ConsecutiveNeighbors() { }   // for Classify

        public override string Svg => $"<circle cx='{x}' cy='{y}' r='.15' stroke='black' stroke-width='.02' fill='white' />";
        protected override bool verify(int a, int b) => myVerify(a, b);
        private static bool myVerify(int a, int b) => Math.Abs(a - b) == 1;

        public static IList<KyuConstraint> Generate(int[] sudoku) => generate(sudoku, myVerify, (cell1, cell2) => new ConsecutiveNeighbors(cell1, cell2));
    }
}
