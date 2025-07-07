using KyudosudokuWebsite.Database;

namespace KyudosudokuWebsite
{
    sealed class PuzzleResultInfo(Puzzle puzzle, UserPuzzle userPuzzle, int solveCount)
    {
        public Puzzle Puzzle { get; private set; } = puzzle;
        public UserPuzzle UserPuzzle { get; private set; } = userPuzzle;
        public int SolveCount { get; private set; } = solveCount;
    }
}
