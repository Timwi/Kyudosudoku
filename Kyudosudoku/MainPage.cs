using RT.Servers;
using RT.TagSoup;

namespace KyudosudokuWebsite
{
    partial class KyudosudokuPropellerModule
    {
        private HttpResponse MainPage(HttpRequest req)
        {
            return HttpResponse.Html(
                new HTML(
                    new HEAD(
                        new TITLE("Daily Kyudosudoku")),
                    new BODY(
                        new H1("Behold, the puzzles:"),
                        new UL(
                            new LI(new A { href = "puzzle/1" }._("Puzzle #1"))))));
        }
    }
}
