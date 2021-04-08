using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using RT.Json;
using RT.Serialization;
using RT.Util;
using RT.Util.ExtensionMethods;
using SvgPuzzleConstraints;

namespace KyudosudokuWebsite.Database
{
    public sealed class Puzzle
    {
        [Key, DatabaseGenerated(DatabaseGeneratedOption.None)]
        public int PuzzleID { get; set; }
        public string KyudokuGrids { get; set; }
        public bool Invalid { get; set; }
        public string Constraints { get; set; }             // ClassifyJson of SvgConstraint[]
        public string ConstraintNames { get; set; }     // for searching/filtering
        public double? AverageTime { get; set; }
        public int? TimeToGenerate { get; set; }
        public DateTime? Generated { get; set; }

        public bool IsSolved(string json)
        {
            var state = JsonValue.Parse(json);
            var kyudokuGrids = KyudokuGrids.Split(36).Select(grid => grid.Select(ch => ch - '0').ToArray()).ToArray();
            var constraints = Constraints == null ? new SvgConstraint[0] : ClassifyJson.Deserialize<SvgConstraint[]>(JsonValue.Parse(Constraints));

            // Check that all cells in the Sudoku grid have a digit
            var sudokuDigits = new int[81];
            for (int cell = 0; cell < 81; cell++)
            {
                var kyCells = Enumerable.Range(0, 4)
                    .Where(c => cell % 9 >= 3 * (c % 2) && cell % 9 < 6 + 3 * (c % 2) && cell / 9 >= 3 * (c / 2) && cell / 9 < 6 + 3 * (c / 2))
                    .Select(c => (corner: c, kyCell: cell % 9 - 3 * (c % 2) + 6 * ((cell / 9) - 3 * (c / 2))))
                    .Where(inf => state["circledDigits"][inf.corner][inf.kyCell].Apply(v => v != null && v.GetBool()))
                    .ToArray();
                if (kyCells.Length > 1 && kyCells.Any(inf => kyudokuGrids[inf.corner][inf.kyCell] != kyudokuGrids[kyCells[0].corner][kyCells[0].kyCell]))
                    return false;
                else if (kyCells.Length >= 1)
                    sudokuDigits[cell] = kyudokuGrids[kyCells[0].corner][kyCells[0].kyCell];
                else if (state["enteredDigits"][cell] != null)
                    sudokuDigits[cell] = state["enteredDigits"][cell].GetInt();
                else
                    return false;
            }

            // Check the Sudoku rules (rows, columns and regions)
            for (var i = 0; i < 9; i++)
            {
                for (var colA = 0; colA < 9; colA++)
                    for (var colB = colA + 1; colB < 9; colB++)
                        if (sudokuDigits[colA + 9 * i] == sudokuDigits[colB + 9 * i])
                            return false;
                for (var rowA = 0; rowA < 9; rowA++)
                    for (var rowB = rowA + 1; rowB < 9; rowB++)
                        if (sudokuDigits[i + 9 * rowA] == sudokuDigits[i + 9 * rowB])
                            return false;
                for (var cellA = 0; cellA < 9; cellA++)
                    for (var cellB = cellA + 1; cellB < 9; cellB++)
                        if (sudokuDigits[cellA % 3 + 3 * (i % 3) + 9 * ((cellA / 3) + 3 * (i / 3))] == sudokuDigits[cellB % 3 + 3 * (i % 3) + 9 * ((cellB / 3) + 3 * (i / 3))])
                            return false;
            }

            // Check the Sudoku constraints
            foreach (var constr in constraints)
                if (!constr.Verify(sudokuDigits))
                    return false;

            // Check all of the Kyudokus
            for (var corner = 0; corner < 4; corner++)
            {
                var digitCounts = new int[9];
                var rowSums = new int[6];
                var colSums = new int[6];
                for (var cell = 0; cell < 36; cell++)
                {
                    if (state["circledDigits"][corner][cell].Apply(v => v != null && v.GetBool()))
                    {
                        digitCounts[kyudokuGrids[corner][cell] - 1]++;
                        rowSums[cell / 6] += kyudokuGrids[corner][cell];
                        colSums[cell % 6] += kyudokuGrids[corner][cell];
                    }
                }
                if (rowSums.Any(r => r > 9) || colSums.Any(c => c > 9) || digitCounts.Any(c => c != 1))
                    return false;
            }

            return true;
        }
    }
}
