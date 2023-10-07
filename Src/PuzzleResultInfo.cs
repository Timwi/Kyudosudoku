using KyudosudokuWebsite.Database;

namespace KyudosudokuWebsite
{
    sealed class PuzzleResultInfo
    {
        public Puzzle Puzzle { get; private set; }
        public UserPuzzle UserPuzzle { get; private set; }
        public int SolveCount { get; private set; }
        public PuzzleResultInfo(Puzzle puzzle, UserPuzzle userPuzzle, int solveCount)
        {
            Puzzle = puzzle;
            UserPuzzle = userPuzzle;
            SolveCount = solveCount;
        }
    }
}
