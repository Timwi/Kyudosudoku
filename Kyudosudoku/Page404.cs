using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using RT.Servers;
using RT.TagSoup;

namespace KyudosudokuWebsite
{
    partial class KyudosudokuPropellerModule
    {
        private HttpResponse Page404(HttpRequest req)
        {
            return HttpResponse.Html(
                new HTML(
                    new HEAD(new TITLE("404 not found")),
                    new BODY(new H1("404 — Not Found"))));
        }
    }
}
