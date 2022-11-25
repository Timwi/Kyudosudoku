using System;
using System.Collections.Generic;
using System.Linq;
using KyudosudokuWebsite.Database;
using PuzzleSolvers;
using RT.Serialization;
using RT.Util;
using RT.Util.ExtensionMethods;
using SvgPuzzleConstraints;

namespace KyudosudokuWebsite
{
    public sealed class Kyudosudoku
    {
        public int[][] Grids { get; private set; }
        public SvgConstraint[] Constraints { get; private set; }

        public Kyudosudoku(int[][] grids, SvgConstraint[] constraints)
        {
            if (grids == null)
                throw new ArgumentNullException(nameof(grids));
            if (grids.Length != 4)
                throw new ArgumentException("There must be four grids.", nameof(grids));
            if (grids.Any(g => g.Length != 36))
                throw new ArgumentException("The grids must all have 36 entries.", nameof(grids));

            Grids = grids;
            Constraints = constraints ?? throw new ArgumentNullException(nameof(constraints));
        }

        private static (int[] topLeft, int[] topRight, int[] bottomLeft, int[] bottomRight)[] GetAllKyudokuCombinations(int[] sudoku)
        {
            var emptyArray = new int[0][];
            IEnumerable<int[]> getDigits(int dx, int dy)
            {
                for (var i = 0; i < 36; i++)
                    if (sudoku[i % 6 + dx + 9 * (i / 6 + dy)] == 9)
                        for (var h = 0; h < 36; h++)
                            if (sudoku[h % 6 + dx + 9 * (h / 6 + dy)] == 8 && h % 6 != i % 6 && h / 6 != i / 6)
                                for (var g = 0; g < 36; g++)
                                    if (sudoku[g % 6 + dx + 9 * (g / 6 + dy)] == 7 && g % 6 != i % 6 && g / 6 != i / 6 && g % 6 != h % 6 && g / 6 != h / 6)
                                        for (var f = 0; f < 36; f++)
                                            if (sudoku[f % 6 + dx + 9 * (f / 6 + dy)] == 6 && f % 6 != i % 6 && f / 6 != i / 6 && f % 6 != h % 6 && f / 6 != h / 6 && f % 6 != g % 6 && f / 6 != g / 6)
                                                for (var e = 0; e < 36; e++)
                                                    if (sudoku[e % 6 + dx + 9 * (e / 6 + dy)] == 5 && e % 6 != i % 6 && e / 6 != i / 6 && e % 6 != h % 6 && e / 6 != h / 6 && e % 6 != g % 6 && e / 6 != g / 6 && e % 6 != f % 6 && e / 6 != f / 6)
                                                        for (var d = 0; d < 36; d++)
                                                            if (sudoku[d % 6 + dx + 9 * (d / 6 + dy)] == 4)
                                                                for (var c = 0; c < 36; c++)
                                                                    if (sudoku[c % 6 + dx + 9 * (c / 6 + dy)] == 3)
                                                                        for (var b = 0; b < 36; b++)
                                                                            if (sudoku[b % 6 + dx + 9 * (b / 6 + dy)] == 2)
                                                                                for (var a = 0; a < 36; a++)
                                                                                    if (sudoku[a % 6 + dx + 9 * (a / 6 + dy)] == 1)
                                                                                    {
                                                                                        var arr = new[] { a, b, c, d, e, f, g, h, i };
                                                                                        if (Enumerable.Range(0, 6).All(row => Enumerable.Range(0, 6).Sum(col => arr.Contains(col + 6 * row) ? sudoku[col + dx + 9 * (row + dy)] : 0) <= 9) &&
                                                                                            Enumerable.Range(0, 6).All(col => Enumerable.Range(0, 6).Sum(row => arr.Contains(col + 6 * row) ? sudoku[col + dx + 9 * (row + dy)] : 0) <= 9))
                                                                                        {
                                                                                            Array.Sort(arr);
                                                                                            yield return arr;
                                                                                        }
                                                                                    }
            }

            var topLefts = getDigits(0, 0).ToArray();
            var topRights = topLefts.Length == 0 ? emptyArray : getDigits(3, 0).ToArray();
            var bottomLefts = topRights.Length == 0 ? emptyArray : getDigits(0, 3).ToArray();
            var bottomRights = bottomLefts.Length == 0 ? emptyArray : getDigits(3, 3).ToArray();

            return (from k1 in topLefts from k2 in topRights from k3 in bottomLefts from k4 in bottomRights select (topLeft: k1, topRight: k2, bottomLeft: k3, bottomRight: k4)).ToArray();
        }

        public static Kyudosudoku Generate(int seed)
        {
            var rnd = new Random(seed);

            tryAgain:

            // Generate a random Sudoku grid
            var sudoku = new Sudoku().Solve(new SolverInstructions { Randomizer = rnd }).First();

            // Find all possible sequences 1–9 for each quadrant that could be a valid Kyudoku solution
            var allKyudokus = GetAllKyudokuCombinations(sudoku);
            if (allKyudokus.Length == 0)
                goto tryAgain;
            allKyudokus.Shuffle(rnd);

            // Constraints that we will use to make the Sudoku unique
            var allKyConstraints = GenerateConstraints(sudoku, rnd).ToArray().Shuffle(rnd);

            // Process every combination of Kyudoku solutions to find one that leads to a valid puzzle
            for (var kyIx = 0; kyIx < allKyudokus.Length; kyIx++)
            {
                var (topLeft, topRight, bottomLeft, bottomRight) = allKyudokus[kyIx];
                var givensFromKyu = new List<GivenConstraint>();
                for (var cell = 0; cell < 81; cell++)
                    if ((cell % 9 < 6 && cell / 9 < 6 && topLeft.Contains(cell % 9 + 6 * (cell / 9))) ||
                        (cell % 9 >= 3 && cell / 9 < 6 && topRight.Contains(cell % 9 - 3 + 6 * (cell / 9))) ||
                        (cell % 9 < 6 && cell / 9 >= 3 && bottomLeft.Contains(cell % 9 + 6 * (cell / 9 - 3))) ||
                        (cell % 9 >= 3 && cell / 9 >= 3 && bottomRight.Contains(cell % 9 - 3 + 6 * (cell / 9 - 3))))
                        givensFromKyu.Add(new GivenConstraint(cell, sudoku[cell]));

                if (new Sudoku().AddConstraints(givensFromKyu, avoidColors: true).AddConstraints(allKyConstraints.SelectMany(s => s.GetConstraints()), avoidColors: true).Solve().Take(2).Count() > 1)
                    // The Sudoku is ambiguous even with all the constraints.
                    continue;

                // Check if the Sudoku works without any constraints.
                var kyConstraints = new SvgConstraint[0];
                if (new Sudoku().AddConstraints(givensFromKyu, avoidColors: true).Solve().Take(2).Count() > 1)
                {
                    // Remove constraints that would be redundant.
                    var attempts = 3;
                    tryRRagain:
                    kyConstraints = Ut.ReduceRequiredSet(
                        Enumerable.Range(0, allKyConstraints.Length),
                        state => new Sudoku().AddConstraints(givensFromKyu, avoidColors: true).AddConstraints(state.SetToTest.SelectMany(ix => allKyConstraints[ix].GetConstraints()), avoidColors: true).Solve().Take(2).Count() == 1,
                        skipConsistencyTest: true)
                            .Select(ix => allKyConstraints[ix])
                            .ToArray();

                    // Don’t allow combinations of constraints that would visually clash on the screen
                    for (var i = 0; i < kyConstraints.Length; i++)
                        for (var j = i + 1; j < kyConstraints.Length; j++)
                            if (kyConstraints[i].ClashesWith(kyConstraints[j]) || kyConstraints[j].ClashesWith(kyConstraints[i]))
                            {
                                attempts--;
                                if (attempts == 0)
                                    goto busted2;
                                allKyConstraints.Shuffle(rnd);
                                goto tryRRagain;
                            }
                }

                // Try up to 10 random fillings of the Kyudoku grids. If none of these results in a valid puzzle, we start again from scratch.
                var kyudokus = new[] { topLeft, topRight, bottomLeft, bottomRight };
                for (var seed2 = 0; seed2 < 10; seed2++)
                {
                    var grids = Enumerable.Range(0, 4)
                        .Select(corner => Ut.NewArray(36, ix => kyudokus[corner].Contains(ix) ? sudoku[ix % 6 + 3 * (corner % 2) + 9 * (ix / 6 + 3 * (corner / 2))] : rnd.Next(1, 10)))
                        .ToArray();

                    // Find all possible Kyudoku solutions for the newly filled grids
                    var allSolutions = new int[4][][];
                    for (var corner = 0; corner < 4; corner++)
                    {
                        var ixs = kyudokus[corner];
                        var kyudoku = new PuzzleSolvers.Puzzle(36, 0, 1);
                        kyudoku.AddConstraint(new Kyudoku6x6Constraint(grids[corner]));
                        allSolutions[corner] = kyudoku.Solve().Select(solution => solution.SelectIndexWhere(v => v == 0).ToArray()).ToArray();
                        if (!allSolutions[corner].Any(solution => solution.SequenceEqual(ixs)))
                            goto busted1;
                    }

                    // Now test every combination of Kyudoku solutions to make sure that all of them result in an unsolvable Sudoku, except for one, which needs to be unique

                    var numValids = 0;
                    var numAmbiguous = 0;

                    foreach (var kys in from s1 in allSolutions[0]
                                        from s2 in allSolutions[1]
                                        from s3 in allSolutions[2]
                                        from s4 in allSolutions[3]
                                        select new[] { s1, s2, s3, s4 })
                    {
                        // Test the uniqueness of the Sudoku resulting from this combination of Kyudoku solutions
                        var sud = new Sudoku();
                        var givens = new int?[81];
                        for (var corner = 0; corner < 4; corner++)
                        {
                            foreach (var kyCell in kys[corner])
                            {
                                var sudokuCell = kyCell % 6 + 3 * (corner % 2) + 9 * (kyCell / 6 + 3 * (corner / 2));
                                // If two Kyudoku solutions transfer different digits to the same cell in the Sudoku, this combination
                                // is already invalid (which is good; we want them all to be invalid except for one)
                                if (givens[sudokuCell] != null && givens[sudokuCell] != grids[corner][kyCell])
                                    goto alright;
                                givens[sudokuCell] = grids[corner][kyCell];
                            }
                        }
                        sud.AddGivens(givens);
                        foreach (var constr in kyConstraints)
                            sud.AddConstraints(constr.GetConstraints());
                        var sols = sud.Solve().Take(2).ToArray();

                        if (sols.Length == 1)       // Sudoku is valid
                            numValids++;
                        else if (sols.Length > 1)   // Sudoku is ambiguous
                            numAmbiguous++;

                        if (numAmbiguous > 0 || numValids > 1)
                            goto busted1;
                        alright:;
                    }

                    return new Kyudosudoku(grids, kyConstraints);

                    busted1:;
                }
                busted2:;
            }
            goto tryAgain;
        }

        private static IEnumerable<SvgConstraint> GenerateConstraints(int[] sudoku, Random rnd)
        {
            // Start by generating all orthogonally contiguous regions that contain unique digits.
            // Several constraints make use of these, so it makes sense to generate them only once.
            var uniquenessRegions = GenerateUniqueContiguousRegions(sudoku);

            foreach (var gen in ConstraintGenerator.All)
            {
                var generated = gen.generator(sudoku, uniquenessRegions);

                // Variant of Fisher-Yates shuffle that stops once we have the required number of elements
                for (int j = 0; j < generated.Count && (gen.probability == null || j < gen.probability.Value); j++)
                {
                    int item = rnd.Next(j, generated.Count);
                    if (item > j)
                        (generated[j], generated[item]) = (generated[item], generated[j]);
                    yield return generated[j];
                }
            }
        }

        public static int[][] GenerateUniqueContiguousRegions(int[] sudoku)
        {
            IEnumerable<bool[]> generateRegions(bool[] sofar, bool[] banned, int count)
            {
                if (count >= 2)
                    yield return sofar;
                if (count >= 9)
                    yield break;

                for (var adj = 0; adj < 81; adj++)
                {
                    if (banned[adj] || !PuzzleUtil.Orthogonal(adj).Any(a => sofar[a]) || sofar.Any((b, ix) => b && ix != adj && sudoku[ix] == sudoku[adj]))
                        continue;
                    sofar[adj] = true;
                    banned[adj] = true;
                    foreach (var item in generateRegions(sofar, (bool[]) banned.Clone(), count + 1))
                        yield return item;
                    sofar[adj] = false;
                }
            }
            var uniquenessRegions = Enumerable.Range(0, 81)
                .SelectMany(startIx => generateRegions(Ut.NewArray(81, x => x == startIx), Ut.NewArray(81, x => x <= startIx), 1))
                .Select(region => region.SelectIndexWhere(b => b).ToArray())
                .ToArray();
            return uniquenessRegions;
        }

        public void SaveToDb(int puzzleId, int? timeToGenerate)
        {
            using var db = new Db();
            db.Puzzles.Add(new Database.Puzzle
            {
                PuzzleID = puzzleId,
                KyudokuGrids = Grids.SelectMany(grid => grid.Select(i => (char) (i + '0'))).JoinString(),
                Constraints = ClassifyJson.Serialize(Constraints).ToString(),
                ConstraintNames = Constraints.Select(c => $"<{c.GetType().Name}>").Distinct().Order().JoinString(),
                NumConstraints = Constraints.Length,
                TimeToGenerate = timeToGenerate,
                Generated = DateTime.UtcNow
            });
            db.SaveChanges();
        }

        public abstract class ReevaluateResult { }
        public sealed class ReevaluateError : ReevaluateResult { public string Error; }
        public sealed class CanReduce : ReevaluateResult { public SvgConstraint[] NewConstraints; }
        public ReevaluateResult Reevaluate()
        {
            // Find all possible Kyudoku solutions
            var cornerSolutions = new int[4][][];
            for (var corner = 0; corner < 4; corner++)
            {
                var kyudoku = new PuzzleSolvers.Puzzle(36, 0, 1);
                kyudoku.AddConstraint(new Kyudoku6x6Constraint(Grids[corner]));
                cornerSolutions[corner] = kyudoku.Solve().Select(solution => solution.SelectIndexWhere(v => v == 0).ToArray()).ToArray();
                if (cornerSolutions[corner].Length == 0)
                    return new ReevaluateError { Error = $"Corner {corner} is unsolvable." };
            }

            var givenGrids = new List<int?[]>();
            foreach (var topLeft in cornerSolutions[0])
                foreach (var topRight in cornerSolutions[1])
                    foreach (var bottomLeft in cornerSolutions[2])
                        foreach (var bottomRight in cornerSolutions[3])
                        {
                            var grid = new int?[9 * 9];
                            for (var cell = 0; cell < 81; cell++)
                            {
                                var values = new HashSet<int>();
                                if (cell % 9 < 6 && cell / 9 < 6 && topLeft.Contains(cell % 9 + 6 * (cell / 9)))
                                    values.Add(Grids[0][cell % 9 + 6 * (cell / 9)]);
                                if (cell % 9 >= 3 && cell / 9 < 6 && topRight.Contains(cell % 9 - 3 + 6 * (cell / 9)))
                                    values.Add(Grids[1][cell % 9 - 3 + 6 * (cell / 9)]);
                                if (cell % 9 < 6 && cell / 9 >= 3 && bottomLeft.Contains(cell % 9 + 6 * (cell / 9 - 3)))
                                    values.Add(Grids[2][cell % 9 + 6 * (cell / 9 - 3)]);
                                if (cell % 9 >= 3 && cell / 9 >= 3 && bottomRight.Contains(cell % 9 - 3 + 6 * (cell / 9 - 3)))
                                    values.Add(Grids[3][cell % 9 - 3 + 6 * (cell / 9 - 3)]);
                                if (values.Count > 1)
                                    goto busted;
                                grid[cell] = values.FirstOrNull();
                            }
                            givenGrids.Add(grid);
                            busted:;
                        }

            if (Constraints.Length == 0)
                return null;

            // Reduce the constraints
            var reqConstraints = Ut.ReduceRequiredSet(Enumerable.Range(0, Constraints.Length), skipConsistencyTest: true, test: state =>
            {
                //Console.WriteLine(Enumerable.Range(0, Constraints.Length).Select(c => state.SetToTest.Contains(c) ? "█" : "░").JoinString());
                var found = false;
                var busted = false;
                var lockObj = new object();
                Enumerable.Range(0, givenGrids.Count).ParallelForEach(Environment.ProcessorCount, ggIx =>
                //for (var ggIx = 0; ggIx < givenGrids.Count; ggIx++)
                {
                    if (busted)
                        goto loopOut;
                    var sudoku = new Sudoku().AddGivens(givenGrids[ggIx]);
                    foreach (var constrIx in state.SetToTest)
                        sudoku.AddConstraints(Constraints[constrIx].GetConstraints());
                    var sol = sudoku.Solve().Take(2).Count();
                    lock (lockObj)
                    {
                        if (sol > 1 || (sol == 1 && found))
                            busted = true;
                        else if (sol == 1)
                            found = true;
                    }
                    loopOut:;
                });
                if (busted)
                    return false;
                if (found)
                    return true;
                throw new InvalidOperationException($"Puzzle looks unsolvable.");
            }).ToArray();

            return reqConstraints.Length == Constraints.Length ? null : new CanReduce { NewConstraints = reqConstraints.Select(c => Constraints[c]).ToArray() };
        }
    }
}
