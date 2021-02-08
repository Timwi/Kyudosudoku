using System.IO;
using System.Linq;
using KyudosudokuWebsite.Database;
using RT.Servers;
using RT.TagSoup;
using RT.Util;

namespace KyudosudokuWebsite
{
    partial class KyudosudokuPropellerModule
    {
        sealed class PageOptions
        {
            public bool IsPuzzlePage = false;
            public HttpStatusCode StatusCode = HttpStatusCode._200_OK;
            public int? PuzzleID;
            public bool AddFooter;
            public Db Db;
            public JsFile Js;
        }

        private HttpResponse RenderPage(string title, User loggedInUser, PageOptions opt, params object[] body)
        {
            opt ??= new PageOptions();
            var fullTitle = $"{(title == null ? "" : $"{title} — ")}Kyudosudoku";
            return HttpResponse.Html(Tag.ToString(new HTML(
                new HEAD(
                    new TITLE($"{fullTitle}"),
#if DEBUG
                    opt.Js.NullOr(js => new SCRIPTLiteral(File.ReadAllText(Path.Combine(Settings.ResourcesDir, js.Filename)))),
#else
                    opt.Js.NullOr(js => new SCRIPTLiteral(js.Js)),
#endif
                    new LINK { rel = "stylesheet", href = $"/css" },
                    new LINK { rel = "shortcut icon", type = "image/png", href = "/logo" }),
                new BODY { class_ = opt.IsPuzzlePage ? "is-puzzle" : null }._(
                    new TABLE { id = "layout" }._(
                        new TR(new TD { id = "topbar", colspan = opt.IsPuzzlePage ? 1 : 2 }._(
                            new A { class_ = "home", href = "/" }._("Kyudosudoku"),
                            opt.PuzzleID == null ? null : new DIV { class_ = "puzzle-id" }._($"Puzzle #{opt.PuzzleID.Value}"),
                            new A { class_ = "right", href = "/auth" }._(loggedInUser == null ? "Log in" : "Settings"),
                            new A { class_ = "right", href = "/help" }._("How to play"))),
                        new TR(
                            opt.IsPuzzlePage ? null : new TD { id = "sidebar" }._(
                                loggedInUser == null || opt.Db == null ? null : new DIV { class_ = "stats" }._(
                                    new DIV("You’ve solved"),
                                    new DIV { class_ = "solve-count" }._(opt.Db.UserPuzzles.Where(up => up.UserID == loggedInUser.UserID && up.Solved).Count()),
                                    new DIV("puzzles.")),
                                new A { href = "/find" }._("Find puzzles")),
                            new TD { id = "main" }._(body)),
                        new TR(new TD { id = "footer", colspan = opt.IsPuzzlePage ? 1 : 2 }._(
                            new P { class_ = "legal" }._(new A { href = "https://legal.timwi.de" }._("Legal stuff · Impressum · Datenschutzerklärung")),
                            new P("Send feedback and suggestions to Timwi#0551 or Goofy#1262 on Discord, or post a ticket to ", new A { href = "https://github.com/Timwi/Kyudosudoku/issues" }._("Kyudosudoku on GitHub"), "."))))))));
        }
    }
}
