using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;
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
        public abstract string Description { get; }
        public abstract string Svg { get; }
        public virtual bool SvgAboveLines => false;
        public abstract bool Verify(int[] grid);
        public abstract bool IncludesCell(int cell);
        public virtual bool IncludesRowCol(bool isCol, int rowCol, bool topLeft) => false;
        public string Name => Constraints.FirstOrDefault(c => c.type == GetType()).name;

        public virtual double ExtraTop => 0;
        public virtual double ExtraRight => 0;

        [ClassifyIgnore]
        private Constraint[] _cachedConstraint;
        public IEnumerable<Constraint> GetConstraints() => _cachedConstraint ??= getConstraints().ToArray();
        protected abstract IEnumerable<Constraint> getConstraints();

        /// <summary>Determines whether a constraint visually clashes with another (overlaps in an undesirable way).</summary>
        public abstract bool ClashesWith(KyuConstraint other);

        protected KyuConstraint() { }    // for Classify

        protected static double svgX(int cell) => cell % 9 + .5;
        protected static double svgY(int cell) => cell / 9 + .5;
        protected static PointD svgP(int cell) => new PointD(svgX(cell), svgY(cell));

        private static (string name, Type type)[] _constraintsCache = null;
        public static (string name, Type type)[] Constraints => _constraintsCache ??= typeof(KyuConstraint).Assembly.GetTypes()
            .Where(t => typeof(KyuConstraint).IsAssignableFrom(t) && !t.IsAbstract && t.GetCustomAttribute<KyuConstraintInfoAttribute>() != null)
            .Select(t => (name: t.GetCustomAttribute<KyuConstraintInfoAttribute>().Name, type: t))
            .ToArray();

        public static IEnumerable<int> Adjacent(int cell)
        {
            var x = cell % 9;
            var y = cell / 9;
            for (var xx = x - 1; xx <= x + 1; xx++)
                if (inRange(xx))
                    for (var yy = y - 1; yy <= y + 1; yy++)
                        if (inRange(yy) && (xx != x || yy != y))
                            yield return xx + 9 * yy;
        }

        public static IEnumerable<int> Orthogonal(int cell)
        {
            var x = cell % 9;
            var y = cell / 9;
            for (var xx = x - 1; xx <= x + 1; xx++)
                if (inRange(xx))
                    for (var yy = y - 1; yy <= y + 1; yy++)
                        if (inRange(yy) && (xx == x || yy == y) && (xx != x || yy != y))
                            yield return xx + 9 * yy;
        }

        public enum CellDirection { Up, Right, Down, Left }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        protected static bool inRange(int c) => c >= 0 && c < 9;
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        protected static int dx(CellDirection dir) => dir switch { CellDirection.Left => -1, CellDirection.Right => 1, _ => 0 };
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        protected static int dy(CellDirection dir) => dir switch { CellDirection.Up => -1, CellDirection.Down => 1, _ => 0 };

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
                textX = outline[(offset + 1) % outline.Length].x + .03;
                textY = outline[(offset + 1) % outline.Length].y + .25;
                for (int j = 0; j <= outline.Length; j++)
                {
                    var point1 = outline[(j + offset) % outline.Length];
                    var point2 = outline[(j + offset + 1) % outline.Length];
                    var point3 = outline[(j + offset + 2) % outline.Length];
                    var x = point2.x;
                    var y = point2.y;

                    var dir1 = getDir(point1, point2);
                    var dir2 = getDir(point2, point3);

                    // “Outer” corners
                    if (dir1 == CellDirection.Up && dir2 == CellDirection.Right) // top left corner
                        path.Append($" {x + marginX + (j == 0 ? gapX ?? 0 : 0) } {y + marginY + (j == outline.Length ? gapY ?? 0 : 0)}");
                    else if (dir1 == CellDirection.Right && dir2 == CellDirection.Down)  // top right corner
                        path.Append($" {x - marginX} {y + marginY}");
                    else if (dir1 == CellDirection.Down && dir2 == CellDirection.Left) // bottom right corner
                        path.Append($" {x - marginX} {y - marginY}");
                    else if (dir1 == CellDirection.Left && dir2 == CellDirection.Up) // bottom left corner
                        path.Append($" {x + marginX} {y - marginY}");

                    // “Inner” corners
                    else if (dir1 == CellDirection.Left && dir2 == CellDirection.Down) // top left corner
                        path.Append($" {x - marginX} {y - marginY}");
                    else if (dir1 == CellDirection.Up && dir2 == CellDirection.Left) // top right corner
                        path.Append($" {x + marginX} {y - marginY}");
                    else if (dir1 == CellDirection.Right && dir2 == CellDirection.Up) // bottom right corner
                        path.Append($" {x + marginX} {y + marginY}");
                    else if (dir1 == CellDirection.Down && dir2 == CellDirection.Right) // bottom left corner
                        path.Append($" {x - marginX} {y + marginY}");
                }
            }

            return path.ToString();
        }

        private static CellDirection getDir((int x, int y) from, (int x, int y) to) => from.x == to.x
                        ? (from.y > to.y ? CellDirection.Up : CellDirection.Down)
                        : (from.x > to.x ? CellDirection.Left : CellDirection.Right);

        private static bool get(int[] cells, int x, int y) => x >= 0 && x < 9 && y >= 0 && y < 9 && cells.Contains(x + 9 * y);

        private static (int x, int y)[] tracePolygon(int[] cells, int i, int j, bool[][] visitedUpArrow)
        {
            var result = new List<(int x, int y)>();
            var dir = CellDirection.Up;

            while (true)
            {
                // In each iteration of this loop, we move from the current edge to the next one.
                // We have to prioritise right-turns so that the diagonal-adjacent case is handled correctly.
                // Every time we take a 90° turn, we add the corner coordinate to the result list.
                // When we get back to the original edge, the polygon is complete.
                switch (dir)
                {
                    case CellDirection.Up:
                        // If we’re back at the beginning, we’re done with this polygon
                        if (visitedUpArrow[i][j])
                            return result.ToArray();

                        visitedUpArrow[i][j] = true;

                        if (!get(cells, i, j - 1))
                        {
                            result.Add((i, j));
                            dir = CellDirection.Right;
                        }
                        else if (get(cells, i - 1, j - 1))
                        {
                            result.Add((i, j));
                            dir = CellDirection.Left;
                            i--;
                        }
                        else
                            j--;
                        break;

                    case CellDirection.Down:
                        j++;
                        if (!get(cells, i - 1, j))
                        {
                            result.Add((i, j));
                            dir = CellDirection.Left;
                            i--;
                        }
                        else if (get(cells, i, j))
                        {
                            result.Add((i, j));
                            dir = CellDirection.Right;
                        }
                        break;

                    case CellDirection.Left:
                        if (!get(cells, i - 1, j - 1))
                        {
                            result.Add((i, j));
                            dir = CellDirection.Up;
                            j--;
                        }
                        else if (get(cells, i - 1, j))
                        {
                            result.Add((i, j));
                            dir = CellDirection.Down;
                        }
                        else
                            i--;
                        break;

                    case CellDirection.Right:
                        i++;
                        if (!get(cells, i, j))
                        {
                            result.Add((i, j));
                            dir = CellDirection.Down;
                        }
                        else if (get(cells, i, j - 1))
                        {
                            result.Add((i, j));
                            dir = CellDirection.Up;
                            j--;
                        }
                        break;
                }
            }
        }
    }
}
