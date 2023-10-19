using System.IO;
using RT.TagSoup;

namespace KyudosudokuWebsite
{
    sealed class Resource
    {
        public string Raw { get; private set; }
        public string Filename { get; private set; }
        public bool IsJs { get; private set; }
        public Resource(string res, string filePath, bool isJs)
        {
            Raw = res;
            Filename = filePath;
            IsJs = isJs;
        }

        public object ToTag(KyudosudokuSettings settings) => IsJs ? new SCRIPTLiteral(GetContent(settings)) : new STYLELiteral(GetContent(settings));
        private string GetContent(KyudosudokuSettings settings) =>
#if DEBUG
            File.ReadAllText(Path.Combine(settings.ResourcesDir, Filename));
#else
            Raw;
#endif

        public static readonly Resource PuzzleJs = new(Resources.PuzzleJs, "Puzzle.js", isJs: true);
        public static readonly Resource FindJs = new(Resources.FindJs, "Find.js", isJs: true);
        public static readonly Resource ProfileJs = new(Resources.ProfileJs, "Profile.js", isJs: true);

        public static readonly Resource GeneralCss = new(Resources.GeneralCss, "General.css", isJs: false);
        public static readonly Resource PuzzleCss = new(Resources.PuzzleCss, "Puzzle.css", isJs: false);
        public static readonly Resource FindCss = new(Resources.FindCss, "Find.css", isJs: false);
        public static readonly Resource ProfileCss = new(Resources.ProfileCss, "Profile.css", isJs: false);

        public static readonly Resource EnglishJson = new(Resources.EnglishJson, "English.json", isJs: false);
    }
}
