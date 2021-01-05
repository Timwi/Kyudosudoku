using System;
using System.Linq;
using PuzzleSolvers;
using RT.Util;
using RT.Util.ExtensionMethods;

namespace KyudosudokuWebsite
{
    sealed class Kyudosudoku
    {
        public int[][] Grids { get; private set; }

        public Kyudosudoku(int[][] grids)
        {
            if (grids == null)
                throw new ArgumentNullException(nameof(grids));
            if (grids.Length != 4)
                throw new ArgumentException("There must be four grids.", nameof(grids));
            if (grids.Any(g => g.Length != 36))
                throw new ArgumentException("The grids must all have 36 entries.", nameof(grids));

            Grids = grids;
        }

        private static (int[] topLeft, int[] topRight, int[] bottomLeft, int[] bottomRight)[] GetAllKyudokuCombinations(int[] sudoku)
        {
            int[][] getDigits(int dx, int dy) => (
                from i in Enumerable.Range(0, 36)
                where sudoku[i % 6 + dx + 9 * (i / 6 + dy)] == 9
                from h in Enumerable.Range(0, 36)
                where sudoku[h % 6 + dx + 9 * (h / 6 + dy)] == 8 && h % 6 != i % 6 && h / 6 != i / 6
                from g in Enumerable.Range(0, 36)
                where sudoku[g % 6 + dx + 9 * (g / 6 + dy)] == 7 && g % 6 != i % 6 && g / 6 != i / 6 && g % 6 != h % 6 && g / 6 != h / 6
                from f in Enumerable.Range(0, 36)
                where sudoku[f % 6 + dx + 9 * (f / 6 + dy)] == 6 && f % 6 != i % 6 && f / 6 != i / 6 && f % 6 != h % 6 && f / 6 != h / 6 && f % 6 != g % 6 && f / 6 != g / 6
                from e in Enumerable.Range(0, 36)
                where sudoku[e % 6 + dx + 9 * (e / 6 + dy)] == 5
                from d in Enumerable.Range(0, 36)
                where sudoku[d % 6 + dx + 9 * (d / 6 + dy)] == 4
                from c in Enumerable.Range(0, 36)
                where sudoku[c % 6 + dx + 9 * (c / 6 + dy)] == 3
                from b in Enumerable.Range(0, 36)
                where sudoku[b % 6 + dx + 9 * (b / 6 + dy)] == 2
                from a in Enumerable.Range(0, 36)
                where sudoku[a % 6 + dx + 9 * (a / 6 + dy)] == 1
                let arr = new[] { a, b, c, d, e, f, g, h, i }
                where Enumerable.Range(0, 6).All(row => Enumerable.Range(0, 6).Sum(col => arr.Contains(col + 6 * row) ? sudoku[col + dx + 9 * (row + dy)] : 0) <= 9)
                    && Enumerable.Range(0, 6).All(col => Enumerable.Range(0, 6).Sum(row => arr.Contains(col + 6 * row) ? sudoku[col + dx + 9 * (row + dy)] : 0) <= 9)
                select arr.Order().ToArray()).ToArray();

            var topLefts = getDigits(0, 0);
            var topRights = topLefts.Length == 0 ? new int[0][] : getDigits(3, 0);
            var bottomLefts = topRights.Length == 0 ? new int[0][] : getDigits(0, 3);
            var bottomRights = bottomLefts.Length == 0 ? new int[0][] : getDigits(3, 3);

            return (from k1 in topLefts from k2 in topRights from k3 in bottomLefts from k4 in bottomRights select (topLeft: k1, topRight: k2, bottomLeft: k3, bottomRight: k4)).ToArray();
        }

        public static Kyudosudoku Generate(int seed)
        {
            var dxs = new[] { 0, 3, 0, 3 };
            var dys = new[] { 0, 0, 3, 3 };

            var lockObj = new object();
            var rnd = new Random(seed);

            tryAgain:
            var sudoku = new Sudoku().Solve(new SolverInstructions { Randomizer = rnd }).First();
            var allKyudokus = GetAllKyudokuCombinations(sudoku);
            if (allKyudokus.Length == 0)
                goto tryAgain;

            for (var kyIx = 0; kyIx < allKyudokus.Length; kyIx++)
            {
                var (topLeft, topRight, bottomLeft, bottomRight) = allKyudokus[kyIx];
                var testSudoku = new Sudoku();
                for (var cell = 0; cell < 81; cell++)
                {
                    var c =
                        (cell % 9 < 6 && cell / 9 < 6 && topLeft.Contains(cell % 9 + 6 * (cell / 9)) ? 1 : 0) +
                        (cell % 9 >= 3 && cell / 9 < 6 && topRight.Contains(cell % 9 - 3 + 6 * (cell / 9)) ? 2 : 0) +
                        (cell % 9 < 6 && cell / 9 >= 3 && bottomLeft.Contains(cell % 9 + 6 * (cell / 9 - 3)) ? 4 : 0) +
                        (cell % 9 >= 3 && cell / 9 >= 3 && bottomRight.Contains(cell % 9 - 3 + 6 * (cell / 9 - 3)) ? 8 : 0);
                    if (c != 0)
                        testSudoku.AddConstraint(new GivenConstraint(cell, sudoku[cell]));
                }

                if (testSudoku.Solve().Take(2).Count() != 1)
                    continue;

                var kyudokus = new[] { topLeft, topRight, bottomLeft, bottomRight };

                for (var seed2 = 0; seed2 < 10; seed2++)
                {
                    var grids = Enumerable.Range(0, 4)
                        .Select(corner => Ut.NewArray(36, ix => kyudokus[corner].Contains(ix) ? sudoku[ix % 6 + dxs[corner] + 9 * (ix / 6 + dys[corner])] : rnd.Next(1, 10)))
                        .ToArray();

                    var allSolutions = new int[4][][];
                    for (var corner = 0; corner < 4; corner++)
                    {
                        var ixs = kyudokus[corner];
                        var dx = dxs[corner];
                        var dy = dys[corner];
                        var kyudoku = new Puzzle(36, 0, 1);
                        kyudoku.AddConstraint(new Kyudoku6x6Constraint(grids[corner]));
                        allSolutions[corner] = kyudoku.Solve().Select(solution => solution.SelectIndexWhere(v => v == 0).ToArray()).ToArray();
                        if (!allSolutions[corner].Any(solution => solution.SequenceEqual(ixs)))
                            goto busted;
                    }

                    var numValids = 0;
                    var numAmbiguous = 0;
                    Sudoku foundSudoku = null;
                    int[] foundSolution = null;

                    foreach (var kys in from s1 in allSolutions[0]
                                        from s2 in allSolutions[1]
                                        from s3 in allSolutions[2]
                                        from s4 in allSolutions[3]
                                        select new[] { s1, s2, s3, s4 })
                    {
                        var sud = new Sudoku();
                        for (var cell = 0; cell < 81; cell++)
                        {
                            var givenValue = -1;
                            var givenColor = 0;
                            for (var corner = 0; corner < 4; corner++)
                            {
                                if (!(cell % 9 - dxs[corner] >= 0 && cell % 9 - dxs[corner] < 6 && cell / 9 - dys[corner] >= 0))
                                    continue;
                                var kyCell = cell % 9 - dxs[corner] + 6 * (cell / 9 - dys[corner]);
                                if (!kys[corner].Contains(kyCell))
                                    continue;

                                if (givenValue != -1 && givenValue != grids[corner][kyCell])
                                    goto alright;
                                givenValue = grids[corner][kyCell];
                                givenColor |= 1 << corner;
                            }
                            if (givenValue != -1)
                                sud.AddConstraint(new GivenConstraint(cell, givenValue));
                        }
                        var sols = sud.Solve().Take(2).ToArray();
                        if (sols.Length == 1)
                        {
                            foundSolution = sols[0];
                            foundSudoku = sud;
                            numValids++;
                        }
                        else if (sols.Length > 1)
                            numAmbiguous++;
                        if (numAmbiguous > 0 || numValids > 1)
                            goto busted;
                        alright:;
                    }

                    return new Kyudosudoku(grids);

                    busted:;
                }
            }
            goto tryAgain;
        }
    }
}
