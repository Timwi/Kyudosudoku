using System;
using System.Data.Entity;
using System.Linq;
using KyudosudokuWebsite.Database;
using RT.PropellerApi;
using RT.Servers;
using RT.TagSoup;
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

            // This also triggers any pending migrations. Without doing some DB stuff here, transactions that don’t commit mess up the migrations.
            using (var db = new Db())
            {
                foreach (var puzzle in db.Puzzles.Where(p => p.AverageTime == null && db.UserPuzzles.Any(up => up.Solved && up.PuzzleID == p.PuzzleID)).ToArray())
                    puzzle.AverageTime = db.CalculateAveragePuzzleTime(puzzle.PuzzleID);
                db.SaveChanges();
            }

            _resolver = new UrlResolver(
                new UrlMapping(path: "/", specificPath: true, handler: mainPage),
                new UrlMapping(path: "/help", specificPath: true, handler: helpPage),
                new UrlMapping(path: "/find", specificPath: true, handler: findPuzzlesPage),
                new UrlMapping(path: "/find-puzzles", specificPath: true, handler: findPuzzlesHandler),
                new UrlMapping(path: "/auth", handler: getAuthResolver().Handle),
                new UrlMapping(path: "/puzzle", handler: req => withSession(req, (session, db) => PuzzlePage(req, session, db))),
                new UrlMapping(path: "/logo", handler: req => HttpResponse.Create(Resources.Logo, "image/png")),
#if DEBUG
                new UrlMapping(path: "/remote-log", handler: remoteLog),
#endif

                // Catch-all 404
                new UrlMapping(path: null, handler: page404));
        }

        private HttpResponse page404(HttpRequest req) => withSession(req, (session, db) =>
            RenderPage("Not found — Kyudosudoku", session.User, new PageOptions { StatusCode = HttpStatusCode._404_NotFound }, new DIV { class_ = "main" }._(new H1("404 — Not Found"))));

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
            return new DbSession(db).EnableManual(req, session => handler(session, db));
        }
    }
}
