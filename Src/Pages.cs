using KyudosudokuWebsite.Database;
using RT.Servers;
using RT.TagSoup;

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
        }

        private HttpResponse RenderPage(string title, User loggedInUser, PageOptions opt, params object[] body)
        {
            opt ??= new PageOptions();
            var fullTitle = $"{(title == null ? "" : $"{title} — ")}Kyudosudoku";
            return HttpResponse.Html(Tag.ToString(new HTML(
                new HEAD(
                    new TITLE($"{fullTitle}"),
                    new LINK { rel = "stylesheet", href = $"/css" },
                    new LINK { rel = "shortcut icon", type = "image/png", href = "/logo" },
                    opt.IsPuzzlePage ? new SCRIPT { src = "/js" } : null),
                new BODY { class_ = opt.IsPuzzlePage ? "is-puzzle" : null }._(
                    new DIV { class_ = "top-bar" }._(
                        new A { class_ = "home", href = "/" }._("Kyudosudoku"),
                        opt.PuzzleID == null ? null : new DIV { class_ = "puzzle-id" }._($"Puzzle #{opt.PuzzleID.Value}"),
                        new A { class_ = "right", href = "/auth" }._(loggedInUser == null ? "Log in" : "Settings"),
                        new A { class_ = "right", href = "/help" }._("How to play")),
                    body,
                    !opt.AddFooter ? null : new DIV { class_ = "footer" }._(
                        new P("Send feedback and suggestions to Timwi#0551 or Goofy#1262 on Discord, or post a ticket to ", new A { href = "https://github.com/Timwi/Kyudosudoku/issues" }._("Kyudosudoku on GitHub"), "."),
                        new P(new A { href = "https://legal.timwi.de" }._("Legal stuff · Impressum · Datenschutzerklärung")))))));
        }
    }
}
