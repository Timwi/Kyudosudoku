using System.Collections.Generic;
using System.Linq;
using KyudosudokuWebsite.Database;
using RT.Servers;
using RT.TagSoup;
using RT.Util;

namespace KyudosudokuWebsite
{
    partial class KyudosudokuPropellerModule
    {
        private HttpResponse authPage(HttpRequest req) => withSession(req, (session, db) => session.User == null ? loginPage(req, req.Url) : userPage(req, req.Url, session.User, db));

        private HttpResponse loginPage(HttpRequest req, IHttpUrl url, string loginErrorMessage = null, string registerErrorMessage = null) => RenderPageTagSoup(
            "Log in", null, isPuzzlePage: false,
            new H1("Log in"),
            loginErrorMessage.NullOr(error => new DIV { class_ = "error" }._(error)),
            new FORM { action = url.WithPath("/login").ToHref(), method = method.post }._(
                new TABLE(
                    new TR(new TD(new LABEL { for_ = "login-username", accesskey = "u" }._("Username: ".Accel('U'))), new TD(new INPUT { id = "login-username", name = "username", type = itype.text, value = req?.Post["username"].Value })),
                    new TR(new TD(new LABEL { for_ = "login-password", accesskey = "p" }._("Password: ".Accel('P'))), new TD(new INPUT { id = "login-password", name = "password", type = itype.password, value = req?.Post["password"].Value })),
                    new TR(new TD(new BUTTON { type = btype.submit, accesskey = "l" }._("Log in".Accel('L')))))),
            new H1("Register a new user"),
            registerErrorMessage.NullOr(error => new DIV { class_ = "error" }._(error)),
            new FORM { action = url.WithPath("/register").ToHref(), method = method.post }._(
                new TABLE(
                    new TR(new TD(new LABEL { for_ = "register-username", accesskey = "s" }._("Username: ".Accel('s'))), new TD(new INPUT { id = "register-username", name = "username", type = itype.text, value = req?.Post["username"].Value })),
                    new TR(new TD(new LABEL { for_ = "register-password-1", accesskey = "1" }._("Password 1: ".Accel('1'))), new TD(new INPUT { id = "register-password-1", name = "password1", type = itype.password, value = req?.Post["password1"].Value })),
                    new TR(new TD(new LABEL { for_ = "register-password-2", accesskey = "2" }._("Password 2: ".Accel('2'))), new TD(new INPUT { id = "register-password-2", name = "password2", type = itype.password, value = req?.Post["password2"].Value })),
                    new TR(new TD(new LABEL { for_ = "register-email", accesskey = "e" }._("Email address: ".Accel('E'))), new TD(new INPUT { id = "register-email", name = "email", type = itype.text, value = req?.Post["email"].Value })),
                    new TR(new TD(new BUTTON { type = btype.submit, accesskey = "r" }._("Register".Accel('R')))))));

        private HttpResponse userPage(HttpRequest req, IHttpUrl url, User user, Db db, string updateUserError = null, IEnumerable<string> updateUserSuccess = null, string teamError = null) => RenderPageTagSoup(
            user.Username, user, true,
            new H1("Welcome, ", new BDI(user.Username), "!"),
            new FORM { action = url.WithPath("/logout").ToHref(), method = method.post, class_ = "logout" }._(
                new BUTTON { type = btype.submit, accesskey = "o" }._("Log out".Accel('o'))),
            new H2("Update"),
            updateUserError.NullOr(msg => new DIV { class_ = "error" }._(msg)),
            updateUserSuccess.NullOr(msgs => new DIV { class_ = "success" }._(msgs.Count() == 1 ? (object) msgs.First() : new UL(msgs.Select(msg => new LI(msg))))),
            new FORM { action = url.WithPath("/update-user").ToHref(), method = method.post }._(
                new INPUT { type = itype.hidden, name = "user", value = user.UserID.ToString() },
                new TABLE(
                    new TR(new TD { class_ = "label" }._(new LABEL { for_ = "changeusername", accesskey = "n" }._("Username: ".Accel('n'))), new TD(new INPUT { id = "changeusername", name = "username", type = itype.text, value = req?.Post["username"].Value ?? user.Username })),
                    new TR(new TD { class_ = "label" }._(new LABEL { for_ = "changeemail", accesskey = "e" }._("Email address: ".Accel('E'))), new TD(new INPUT { id = "changeemail", name = "email", type = itype.email, value = req?.Post["email"].Value ?? user.EmailAddress })),
                    new TR(new TD { class_ = "label" }._(new LABEL { for_ = "changepassword-old", accesskey = "p" }._("Old password: ".Accel('p'))), new TD(new INPUT { id = "changepassword-old", name = "oldpassword", type = itype.password, value = req?.Post["oldpassword"].Value })),
                    new TR(new TD { class_ = "label" }._(new LABEL { for_ = "changepassword-new-1", accesskey = "1" }._("New password 1: ".Accel('1'))), new TD(new INPUT { id = "changepassword-new-1", name = "password1", type = itype.password, value = req?.Post["password1"].Value })),
                    new TR(new TD { class_ = "label" }._(new LABEL { for_ = "changepassword-new-2", accesskey = "2" }._("New password 2: ".Accel('2'))), new TD(new INPUT { id = "changepassword-new-2", name = "password2", type = itype.password, value = req?.Post["password2"].Value })),
                    new TR(new TD { class_ = "label" }._(new BUTTON { type = btype.submit, accesskey = "u" }._("Update".Accel('U')))))));

    }
}
