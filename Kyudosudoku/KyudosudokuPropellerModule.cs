using RT.Json;
using RT.PropellerApi;
using RT.Servers;
using RT.Util;

namespace KyudosudokuWebsite
{
    public partial class KyudosudokuPropellerModule : PropellerModuleBase<KyudosudokuSettings>
    {
        public override string Name => "Kyudosudoku";

        private UrlResolver _resolver;

        public override void Init(LoggerBase log)
        {
            _resolver = new UrlResolver(
#if DEBUG
                new UrlMapping(path: "/js", specificPath: true, handler: req => HttpResponse.File(Settings.JsFile, "text/javascript; charset=utf-8")),
                new UrlMapping(path: "/css", specificPath: true, handler: req => HttpResponse.File(Settings.CssFile, "text/css; charset=utf-8")),
#else
                new UrlMapping(path: "/js", specificPath: true, handler: req => HttpResponse.JavaScript(Resources.Js)),
                new UrlMapping(path: "/css", specificPath: true, handler: req => HttpResponse.Css(Resources.Css)),
#endif

                new UrlMapping(path: "/", specificPath: true, handler: MainPage),
                new UrlMapping(path: "/puzzle", handler: PuzzlePage)
            );

            base.Init(log);
        }

        public override HttpResponse Handle(HttpRequest req) => _resolver.Handle(req);
    }
}
