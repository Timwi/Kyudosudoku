namespace KyudosudokuWebsite
{
    sealed class JsFile
    {
        public string Js { get; private set; }
        public string Filename { get; private set; }
        public JsFile(string js, string filePath)
        {
            Js = js;
            Filename = filePath;
        }

        public static readonly JsFile Puzzle = new JsFile(Resources.PuzzleJs, "Kyudosudoku.js");
        public static readonly JsFile Find = new JsFile(Resources.FindJs, "Find.js");
    }
}
