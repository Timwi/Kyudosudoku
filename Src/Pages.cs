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
        }

        private HttpResponse RenderPageTagSoup(string title, User loggedInUser, PageOptions opt, params object[] body) => RenderPageString(title, loggedInUser, opt, body: Tag.ToString(body));

        private HttpResponse RenderPageString(string title, User loggedInUser, PageOptions opt, string body)
        {
            opt ??= new PageOptions();
            var fullTitle = $"{(title == null ? "" : $"{title} — ")}Kyudosudoku";
            return HttpResponse.Html(Tag.ToString(new HTML(
                new HEAD(
                    new TITLE($"{fullTitle}"),
                    new LINK { rel = "stylesheet", href = $"/css" },
                    new LINK { rel = "shortcut icon", type = "image/png", href = "/logo" },
                    new SCRIPT { src = "/js" }),
                new BODY { class_ = opt.IsPuzzlePage ? "is-puzzle" : null }._(
                    new DIV { class_ = "top-bar" }._(new A { class_ = "home", href = "/" }._("Kyudosudoku"), new A { class_ = "right", href = "/auth" }._(loggedInUser == null ? "Log in" : "Settings"), new A { class_ = "right", href = "/help" }._("How to play")),
                    opt.IsPuzzlePage ? new RawTag(body) : new DIV { class_ = "main" }._(new RawTag(body))))));
        }
    }
}
