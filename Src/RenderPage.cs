using System.Collections.Generic;
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
            public List<Resource> Resources = [];
        }

        private HttpResponse RenderPage(string title, User loggedInUser, PageOptions opt, params object[] body)
        {
            opt ??= new PageOptions();
            var fullTitle = $"{(title == null ? "" : $"{title} — ")}Kyudosudoku";

            var topbar = Ut.NewArray<object>(
                new A { class_ = "home", href = "/" }._("Kyudosudoku"),
                opt.PuzzleID == null ? null : new DIV { class_ = "puzzle-id" }._($"Puzzle #{opt.PuzzleID.Value}"),
                new DIV { class_ = "right" }._(
                    loggedInUser == null ? null : new A { href = $"/profile/{loggedInUser.UserID}" }._("Profile"),
                    new A { href = "/help" }._("How to play"),
                    new A { href = "/auth" }._(loggedInUser == null ? "Log in" : "Settings")));

            var sidebar = opt.IsPuzzlePage ? null : Ut.NewArray<object>(
                loggedInUser == null || opt.Db == null ? null : opt.Db.UserPuzzles.Where(up => up.UserID == loggedInUser.UserID && up.Solved).Count().Apply(solveCount => new DIV { class_ = "stats" }._(
                    new DIV("You’ve solved"),
                    new DIV { class_ = "solve-count" }._(solveCount),
                    new DIV(solveCount == 1 ? "puzzle." : "puzzles."))),
                new A { href = "/find" }._("Find puzzles"));

            var footer = opt.IsPuzzlePage ? null : Ut.NewArray<object>(
                new P { class_ = "legal" }._(new A { href = "https://legal.timwi.de" }._("Legal stuff · Impressum · Datenschutzerklärung")),
                new P(new A { href = "https://github.com/Timwi/Kyudosudoku" }._("Kyudosudoku on GitHub")));

            return HttpResponse.Html(Tag.ToString(new HTML(
                new HEAD(
                    new META { name = "viewport", content = "width=device-width,initial-scale=1.0" },
                    new TITLE($"{fullTitle}"),
                    opt.Resources.Concat([Resource.GeneralCss]).Select(r => r.ToTag(Settings)),
                    new LINK { rel = "shortcut icon", type = "image/png", href = "/logo" }),
                new BODY { class_ = opt.IsPuzzlePage ? "is-puzzle" : null }._(
                    opt.IsPuzzlePage
                        ? Ut.NewArray<object>(
                            new DIV { id = "topbar" }._(topbar),
                            new DIV { id = "main" }._(body))
                        : new TABLE { id = "layout" }._(
                            new TR(new TD { id = "topbar", colspan = opt.IsPuzzlePage ? 1 : 2 }._(topbar)),
                            new TR(sidebar.NullOr(sb => new TD { id = "sidebar" }._(sb)), new TD { id = "main" }._(body)),
                            footer.NullOr(f => new TR(new TD { id = "footer", colspan = opt.IsPuzzlePage ? 1 : 2 }._(f))))))));
        }
    }
}
