using System;
using System.Data.Entity;
using System.Linq;
using System.Transactions;
using KyudosudokuWebsite.Database;
using RT.PropellerApi;
using RT.Servers;
using RT.TagSoup;
using RT.Util;
using RT.Util.ExtensionMethods;

namespace KyudosudokuWebsite
{
    public partial class KyudosudokuPropellerModule : PropellerModuleBase<KyudosudokuSettings>
    {
        public override string Name => "Kyudosudoku";

        private UrlResolver _resolver;

        public override void Init()
        {
            System.Data.Entity.Database.SetInitializer(new MigrateDatabaseToLatestVersion<Db, Configuration>());
            Db.ConnectionString = Settings.ConnectionString;

            // Trigger any pending migrations (without this, transactions that don’t commit mess up the migrations)
            using (var db = new Db())
                Log.Info("Number of puzzles in the database: {0}".Fmt(db.Puzzles.Count()));

            _resolver = new UrlResolver(
#if DEBUG
                new UrlMapping(path: "/js", specificPath: true, handler: req => HttpResponse.File(Settings.JsFile, "text/javascript; charset=utf-8")),
                new UrlMapping(path: "/css", specificPath: true, handler: req => HttpResponse.File(Settings.CssFile, "text/css; charset=utf-8")),
#else
                new UrlMapping(path: "/js", specificPath: true, handler: req => HttpResponse.JavaScript(Resources.Js)),
                new UrlMapping(path: "/css", specificPath: true, handler: req => HttpResponse.Css(Resources.Css)),
#endif

                new UrlMapping(path: "/", specificPath: true, handler: mainPage),
                new UrlMapping(path: "/help", specificPath: true, handler: helpPage),
                new UrlMapping(path: "/auth", handler: getAuthResolver().Handle),
                new UrlMapping(path: "/puzzle", handler: req => withSession(req, (session, db) => PuzzlePage(req, session, db))),
                new UrlMapping(path: "/logo", handler: req => HttpResponse.Create(Resources.Logo, "image/png")),

                // Catch-all 404
                new UrlMapping(path: null, handler: page404));
        }

        private HttpResponse page404(HttpRequest req) => withSession(req, (session, db) =>
            RenderPageTagSoup("Not found — Kyudosudoku", session.User, new PageOptions { StatusCode = HttpStatusCode._404_NotFound }, new H1("404 — Not Found")));

        private UrlResolver getAuthResolver()
        {
            var resolver = new UrlResolver(
                new UrlMapping(path: "", specificPath: true, handler: req => authPage(req)),
                new UrlMapping(path: "/register", specificPath: true, handler: req => register(req, req.Url.WithPathParent().WithPathOnly(""))),
                new UrlMapping(path: "/login", specificPath: true, handler: req => login(req, req.Url.WithPathParent().WithPathOnly(""))),
                new UrlMapping(path: "/logout", specificPath: true, handler: req => logout(req, req.Url.WithPathParent().WithPathOnly(""))),
                new UrlMapping(path: "/update-user", specificPath: true, handler: req => updateUser(req, req.Url.WithPathParent().WithPathOnly(""))));
            return resolver;
        }

        public override HttpResponse Handle(HttpRequest req) => _resolver.Handle(req);

        private HttpResponse withSession(HttpRequest req, Func<DbSession, Db, HttpResponse> handler)
        {
            using var db = new Db();
            using var tr = new TransactionScope(TransactionScopeOption.RequiresNew, new TransactionOptions { IsolationLevel = IsolationLevel.Serializable });
            var result = new DbSession(db).EnableManual(req, session => handler(session, db));
            tr.Complete();
            return result;
        }
    }
}
