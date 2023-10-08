using System.Collections.Generic;
using System.Linq;
using KyudosudokuWebsite.Database;
using RT.TagSoup;
using RT.Util;
using RT.Util.ExtensionMethods;
using SvgPuzzleConstraints;

namespace KyudosudokuWebsite
{
    partial class KyudosudokuPropellerModule
    {
        private object GeneratePuzzleTable(IEnumerable<PuzzleResultInfo> puzzles, int count, PuzzleTableType type, bool sortable)
        {
            static object time(Puzzle puzzle) => Ut.NewArray<object>(
                puzzle.AverageTime == null ? "🕛" : "🕛🕐🕑🕒🕓🕔🕕🕖🕗🕘🕙🕚".Substring(((int)puzzle.AverageTime.Value) / 60 % 60 / 5 * 2, 2),
                " ", formatTime(puzzle.AverageTime));

            static object solveTime(UserPuzzle userPuzzle) => userPuzzle == null
                ? "🕛 —"
                : new[] { "🕛🕐🕑🕒🕓🕔🕕🕖🕗🕘🕙🕚".Substring(userPuzzle.Time / 60 % 60 / 5 * 2, 2), " ", formatTime(userPuzzle.Time) };

            static object lastSeen(UserPuzzle userPuzzle) => Ut.NewArray<object>(
                "😎 ",
                userPuzzle == null ? "—" : userPuzzle.SolveTime.Year == 1900 ? "(not recorded)" : userPuzzle.SolveTime.ToString("d MMM yyyy, H:mm"));

            return count == 0 ? new P("No results.") : Ut.NewArray(

                    // Big version of the table for desktop view
                    new TABLE { class_ = "big" }._(
                        new TR { class_ = "headers" }._(
                            new TH { class_ = "nowrap" }._(sortable ? new A { href = "#", class_ = "sorter" }._("Puzzle").Data("sort", "puzzleId") : "Puzzle"),
                            new TH { class_ = "nowrap" }._(sortable ? new A { href = "#", class_ = "sorter" }._("Average time").Data("sort", "avg") : "Average time"),
                            new TH { class_ = "nowrap" }._(sortable ? new A { href = "#", class_ = "sorter" }._("# solves").Data("sort", "solves") : "# solves"),
                            type != PuzzleTableType.Solved ? null : new TH { class_ = "nowrap" }._(sortable ? new A { href = "#", class_ = "sorter" }._("Your time").Data("sort", "your-time") : "Your time"),
                            type == PuzzleTableType.NotSeen ? null : new TH { class_ = "nowrap" }._(sortable ? new A { href = "#", class_ = "sorter" }._(type == PuzzleTableType.Solved ? "When solved" : "Last seen").Data("sort", "solvetime") : type == PuzzleTableType.Solved ? "When solved" : "Last seen"),
                            new TH(sortable ? new A { href = "#", class_ = "sorter" }._("Constraints").Data("sort", "numconstr") : "Constraints")),
                        puzzles.AsEnumerable().Select(inf => new TR(
                            new TD { class_ = "nowrap" }._("▶ ", new A { href = $"/puzzle/{inf.Puzzle.PuzzleID}" }._("Puzzle #", inf.Puzzle.PuzzleID)),
                            new TD { class_ = "nowrap" }._(time(inf.Puzzle)),
                            new TD { class_ = "nowrap" }._(inf.SolveCount),
                            type != PuzzleTableType.Solved ? null : new TD { class_ = "nowrap" }._(solveTime(inf.UserPuzzle)),
                            type == PuzzleTableType.NotSeen ? null : new TD { class_ = "nowrap" }._(lastSeen(inf.UserPuzzle)),
                            new TD(inf.Puzzle.ConstraintNames == null || inf.Puzzle.ConstraintNames.Length == 0 ? "(none)" : $"{inf.Puzzle.NumConstraints}: {inf.Puzzle.ConstraintNames.Substring(1, inf.Puzzle.ConstraintNames.Length - 2).Split("><").Select(cn => SvgConstraint.Constraints.FirstOrDefault(tup => tup.type.Name == cn).name).JoinString(", ")}")))),

                    // Small version of the table for mobile view
                    new TABLE { class_ = "small" }._(
                        new TR { class_ = "headers" }._(
                            new TH { class_ = "nowrap" }._(sortable ? new A { href = "#", class_ = "sorter" }._("Puzzle").Data("sort", "puzzleId") : "Puzzle"),
                            new TH { class_ = "nowrap" }._(sortable ? new A { href = "#", class_ = "sorter" }._("Average time").Data("sort", "avg") : "Average time")),
                        puzzles.AsEnumerable().Select(inf => new TR(
                            new TD("▶ ", new A { href = $"/puzzle/{inf.Puzzle.PuzzleID}" }._("Puzzle #", inf.Puzzle.PuzzleID),
                                new DIV { class_ = "constraints" }._(
                                    inf.Puzzle.ConstraintNames == null || inf.Puzzle.ConstraintNames.Length == 0 ? "(no constraints)" :
                                    inf.Puzzle.ConstraintNames.Substring(1, inf.Puzzle.ConstraintNames.Length - 2).Split("><")
                                        .Select(cn => SvgConstraint.Constraints.FirstOrDefault(tup => tup.type.Name == cn).name).Order().InsertBetween(", "))),
                            new TD { class_ = "nowrap" }._(
                                new DIV { class_ = "solve-count" }._("Solved: ", inf.SolveCount == 0 ? "never" : inf.SolveCount == 1 ? "once" : $"{inf.SolveCount} times"),
                                new DIV { class_ = "avg" }._("Average time: ", time(inf.Puzzle)),
                                type != PuzzleTableType.Solved ? null : new DIV { class_ = "solve-time" }._("Your time: ", solveTime(inf.UserPuzzle)),
                                type == PuzzleTableType.NotSeen ? null : new DIV { class_ = "last-seen" }._(type == PuzzleTableType.Solved ? "When: " : "Seen: ", lastSeen(inf.UserPuzzle)))))));
        }
    }
}
