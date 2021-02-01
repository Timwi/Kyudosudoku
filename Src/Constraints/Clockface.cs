using System.Collections.Generic;
using System.Linq;

namespace KyudosudokuWebsite
{
    sealed class Clockface : KyuFourCellConstraint
    {
        public override string Name => "Clockface";
        public override string Description => $"The digits around the circle must be in {(Clockwise ? "clockwise" : "counter-clockwise")} order.";
        public bool Clockwise { get; private set; }

        public Clockface(int topLeftCell, bool clockwise) : base(topLeftCell) { Clockwise = clockwise; }
        private Clockface() { }     // for Classify

        protected override bool verify(int a, int b, int c, int d) => verify(Clockwise, a, b, c, d);
        private static bool verify(bool clockwise, int a, int b, int c, int d) => clockwise
            ? (a < b && b < c && c < d) || (b < c && c < d && d < a) || (c < d && d < a && a < b) || (d < a && a < b && b < c)
            : (a > b && b > c && c > d) || (b > c && c > d && d > a) || (c > d && d > a && a > b) || (d > a && a > b && b > c);

        public override string Svg => $"<circle cx='{x}' cy='{y}' r='.15' fill='{(Clockwise ? "white" : "black")}' stroke-width='.02' stroke='{(Clockwise ? "black" : "white")}' />";

        public static IList<KyuConstraint> Generate(int[] sudoku) =>
            generate(sudoku, (cell, a, b, c, d) => verify(true, a, b, c, d) ? new[] { new Clockface(cell, true) } : verify(false, a, b, c, d) ? new[] { new Clockface(cell, false) } : Enumerable.Empty<KyuFourCellConstraint>());
    }
}
