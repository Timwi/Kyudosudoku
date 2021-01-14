using KyudosudokuWebsite.Database;
using RT.Servers;
using RT.TagSoup;

namespace KyudosudokuWebsite
{
    partial class KyudosudokuPropellerModule
    {
        private HttpResponse RenderPageTagSoup(string title, User loggedInUser, bool isPuzzlePage, params object[] body) => RenderPageString(title, loggedInUser, isPuzzlePage, body: Tag.ToString(body));

        private HttpResponse RenderPageString(string title, User loggedInUser, bool isPuzzlePage, string body)
        {
            var fullTitle = $"{(title == null ? "" : $"{title} — ")}Kyudosudoku";
            return HttpResponse.Html(Tag.ToString(new HTML(
                new HEAD(
                    new TITLE($"{fullTitle}"),
                    new LINK { rel = "stylesheet", href = $"/css" },
                    new LINK { rel = "shortcut icon", type = "image/png", href = "/logo" },
                    new SCRIPT { src = "/js" }),
                new BODY { class_ = isPuzzlePage ? "is-puzzle" : null }._(
                    new DIV { class_ = "top-bar" }._(new A { class_ = "home", href = "/" }._("Kyudosudoku"), new A { class_ = "right", href = "/auth" }._(loggedInUser == null ? "Log in" : "Settings"), new A { class_ = "right", href = "/help" }._("How to play")),
                    isPuzzlePage ? new RawTag(body) : new DIV { class_ = "main" }._(new RawTag(body))))));
        }
    }
}
