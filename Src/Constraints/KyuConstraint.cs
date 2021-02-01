using System.Collections.Generic;
using System.Linq;
using System.Text;
using PuzzleSolvers;
using RT.Serialization;
using RT.Util;
using RT.Util.ExtensionMethods;
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

        public static IEnumerable<int> Adjacent(int cell)
        {
            var x = cell % 9;
            var y = cell / 9;
            for (var xx = x - 1; xx <= x + 1; xx++)
                if (xx >= 0 && xx < 9)
                    for (var yy = y - 1; yy <= y + 1; yy++)
                        if (yy >= 0 && yy < 9)
                            yield return xx + 9 * yy;
        }

        public static IEnumerable<int> Orthogonal(int cell)
        {
            var x = cell % 9;
            var y = cell / 9;
            for (var xx = x - 1; xx <= x + 1; xx++)
                if (xx >= 0 && xx < 9)
                    for (var yy = y - 1; yy <= y + 1; yy++)
                        if (yy >= 0 && yy < 9 && (xx == x || yy == y))
                            yield return xx + 9 * yy;
        }

        private enum Direction { Up, Right, Down, Left }
        protected static string GenerateSvgPath(int[] cells, double marginX, double marginY, double? gapX = null, double? gapY = null)
        {
            var outlines = new List<(int x, int y)[]>();
            var visitedUpArrow = Ut.NewArray<bool>(9, 9);

            for (int i = 0; i < 9; i++)
                for (int j = 0; j < 9; j++)
                    // every region must have at least one up arrow (left edge)
                    if (!visitedUpArrow[i][j] && get(cells, i, j) && !get(cells, i - 1, j))
                        outlines.Add(tracePolygon(cells, i, j, visitedUpArrow));

            var path = new StringBuilder();
            double textX = 0;
            double textY = 0;

            foreach (var outline in outlines)
            {
                path.Append("M");
                var offset = outline.MinIndex(c => c.x + 9 * c.y) + outline.Length - 1;
                textX = Kyudosudoku.SudokuX + outline[(offset + 1) % outline.Length].x + .03;
                textY = Kyudosudoku.SudokuY + outline[(offset + 1) % outline.Length].y + .25;
                for (int j = 0; j <= outline.Length; j++)
                {
                    var point1 = outline[(j + offset) % outline.Length];
                    var point2 = outline[(j + offset + 1) % outline.Length];
                    var point3 = outline[(j + offset + 2) % outline.Length];
                    var x = Kyudosudoku.SudokuX + point2.x;
                    var y = Kyudosudoku.SudokuY + point2.y;

                    var dir1 = getDir(point1, point2);
                    var dir2 = getDir(point2, point3);

                    // “Outer” corners
                    if (dir1 == Direction.Up && dir2 == Direction.Right) // top left corner
                        path.Append($" {x + marginX + (j == 0 ? gapX ?? 0 : 0) } {y + marginY + (j == outline.Length ? gapY ?? 0 : 0)}");
                    else if (dir1 == Direction.Right && dir2 == Direction.Down)  // top right corner
                        path.Append($" {x - marginX} {y + marginY}");
                    else if (dir1 == Direction.Down && dir2 == Direction.Left) // bottom right corner
                        path.Append($" {x - marginX} {y - marginY}");
                    else if (dir1 == Direction.Left && dir2 == Direction.Up) // bottom left corner
                        path.Append($" {x + marginX} {y - marginY}");

                    // “Inner” corners
                    else if (dir1 == Direction.Left && dir2 == Direction.Down) // top left corner
                        path.Append($" {x - marginX} {y - marginY}");
                    else if (dir1 == Direction.Up && dir2 == Direction.Left) // top right corner
                        path.Append($" {x + marginX} {y - marginY}");
                    else if (dir1 == Direction.Right && dir2 == Direction.Up) // bottom right corner
                        path.Append($" {x + marginX} {y + marginY}");
                    else if (dir1 == Direction.Down && dir2 == Direction.Right) // bottom left corner
                        path.Append($" {x - marginX} {y + marginY}");
                }
            }

            return path.ToString();
        }

        private static Direction getDir((int x, int y) from, (int x, int y) to) => from.x == to.x
                        ? (from.y > to.y ? Direction.Up : Direction.Down)
                        : (from.x > to.x ? Direction.Left : Direction.Right);

        private static bool get(int[] cells, int x, int y) => x >= 0 && x < 9 && y >= 0 && y < 9 && cells.Contains(x + 9 * y);

        private static (int x, int y)[] tracePolygon(int[] cells, int i, int j, bool[][] visitedUpArrow)
        {
            var result = new List<(int x, int y)>();
            var dir = Direction.Up;

            while (true)
            {
                // In each iteration of this loop, we move from the current edge to the next one.
                // We have to prioritise right-turns so that the diagonal-adjacent case is handled correctly.
                // Every time we take a 90° turn, we add the corner coordinate to the result list.
                // When we get back to the original edge, the polygon is complete.
                switch (dir)
                {
                    case Direction.Up:
                        // If we’re back at the beginning, we’re done with this polygon
                        if (visitedUpArrow[i][j])
                            return result.ToArray();

                        visitedUpArrow[i][j] = true;

                        if (!get(cells, i, j - 1))
                        {
                            result.Add((i, j));
                            dir = Direction.Right;
                        }
                        else if (get(cells, i - 1, j - 1))
                        {
                            result.Add((i, j));
                            dir = Direction.Left;
                            i--;
                        }
                        else
                            j--;
                        break;

                    case Direction.Down:
                        j++;
                        if (!get(cells, i - 1, j))
                        {
                            result.Add((i, j));
                            dir = Direction.Left;
                            i--;
                        }
                        else if (get(cells, i, j))
                        {
                            result.Add((i, j));
                            dir = Direction.Right;
                        }
                        break;

                    case Direction.Left:
                        if (!get(cells, i - 1, j - 1))
                        {
                            result.Add((i, j));
                            dir = Direction.Up;
                            j--;
                        }
                        else if (get(cells, i - 1, j))
                        {
                            result.Add((i, j));
                            dir = Direction.Down;
                        }
                        else
                            i--;
                        break;

                    case Direction.Right:
                        i++;
                        if (!get(cells, i, j))
                        {
                            result.Add((i, j));
                            dir = Direction.Down;
                        }
                        else if (get(cells, i, j - 1))
                        {
                            result.Add((i, j));
                            dir = Direction.Up;
                            j--;
                        }
                        break;
                }
            }
        }
    }
}
