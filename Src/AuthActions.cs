using System.Security.Cryptography;
using KyudosudokuWebsite.Database;
using RT.Servers;
using RT.Util;
using RT.Util.ExtensionMethods;

namespace KyudosudokuWebsite
{
    public partial class KyudosudokuPropellerModule
    {
        private static HttpResponse logout(HttpRequest req, IHttpUrl redirectTo) => withSession(req, (session, db) =>
        {
            session?.Action = SessionAction.Delete;
            return HttpResponse.Redirect(redirectTo);
        });

        private HttpResponse login(HttpRequest req, IHttpUrl redirectTo) => withSession(req, (session, db) =>
        {
            var username = req.Post["username"].Value;
            if (string.IsNullOrWhiteSpace(username))
                return loginPage(req, req.Url.WithPathParent(), "You must specify a username.");
            var user = db.Users.FirstOrDefault(u => u.Username == username) ??
                db.Users.AsEnumerable().FirstOrDefault(u =>
                    username.Equals(u.Username, StringComparison.InvariantCultureIgnoreCase) ||
                    username.Equals(u.EmailAddress, StringComparison.InvariantCultureIgnoreCase));
            if (user == null)
                return loginPage(req, req.Url.WithPathParent(), "The specified username does not exist.");
            if (!verifyPasswordHash(req.Post["password"].Value, user.PasswordHash))
                return loginPage(req, req.Url.WithPathParent(), "The specified password is not correct.");
            session.User = user;
            return HttpResponse.Redirect(redirectTo);
        });

        private HttpResponse register(HttpRequest req, IHttpUrl redirectTo) => withSession(req, (session, db) =>
        {
            var username = req.Post["username"].Value;
            if (string.IsNullOrWhiteSpace(username))
                return loginPage(req, redirectTo, registerErrorMessage: "Your username cannot be empty.");
            var email = req.Post["email"].Value;
            if (!string.IsNullOrWhiteSpace(email) && db.Users.Any(u => u.Username == username || u.EmailAddress == username || u.Username == email || u.EmailAddress == email))
                return loginPage(req, redirectTo, registerErrorMessage: "I’m afraid that username oder email address is already taken! Maybe you can just reset your password and re-use that account?");
            var password = req.Post["password1"].Value;
            if (req.Post["password2"].Value != password)
                return loginPage(req, redirectTo, registerErrorMessage: "The two passwords don’t match. Please try again.");
            var newUser = db.Users.Add(new User
            {
                Username = username,
                EmailAddress = string.IsNullOrWhiteSpace(req.Post["email"].Value) ? null : req.Post["email"].Value,
                PasswordHash = CreatePasswordHash(password),
                ShowErrors = true,
                SemitransparentXs = false,
                ShowSolveTime = true,
                PlayInvalidSound = false,
                BackspaceOption = 0
            }).Entity;
            db.SaveChanges();
            session.User = newUser;
            //if (!string.IsNullOrWhiteSpace(email) && email.Contains('@'))
            //    sendMail($"Kyudosudoku registration",
            //        $"<p style='font-weight: bold'>Thank you for registering for Kyudosudoku!</p>" +
            //        $"<p>Your username is {username}.</p>" +
            //        $"<p>You can <a href='{redirectTo.ToFull()}'>log in</a> at any time.",
            //        new MailAddress(email, username));
            return HttpResponse.Redirect(redirectTo);
        });

        private HttpResponse updateUser(HttpRequest req, IHttpUrl redirectTo) => withSession(req, (session, db) =>
        {
            if (session.User == null)
                return HttpResponse.Redirect(redirectTo);
            if (req.Post["user"].Value != session.User.UserID.ToString())
                return userPage(req, redirectTo, session.User, updateUserError: "It appears that you logged out and back in as someone else. Please try again.");

            var changingPassword = !string.IsNullOrWhiteSpace(req.Post["password1"].Value);
            if (changingPassword)
            {
                if (!verifyPasswordHash(req.Post["oldpassword"].Value, session.User.PasswordHash))
                    return userPage(req, redirectTo, session.User, updateUserError: "The specified old password was not correct.");
                if (req.Post["password1"].Value != req.Post["password2"].Value)
                    return userPage(req, redirectTo, session.User, updateUserError: "The two new passwords do not match.");
            }

            var messages = new List<string>();

            var newUsername = req.Post["username"].Value;
            if (!string.IsNullOrWhiteSpace(newUsername) && newUsername != session.User.Username)
            {
                if (db.Users.Any(u => u.UserID != session.User.UserID && (u.Username == newUsername || u.EmailAddress == newUsername)))
                    return userPage(req, redirectTo, session.User, updateUserError: "I’m afraid that username is already taken!");

                session.User.Username = newUsername;
                messages.Add("Username updated.");
            }

            var newEmail = req.Post["email"].Value;
            if (!string.IsNullOrWhiteSpace(newEmail) && newEmail != session.User.EmailAddress)
            {
                if (db.Users.Any(u => u.UserID != session.User.UserID && (u.Username == newEmail || u.EmailAddress == newEmail)))
                    return userPage(req, redirectTo, session.User, updateUserError: "I’m afraid that email address is already taken!");

                session.User.EmailAddress = newEmail;
                messages.Add("Email address updated.");
            }
            if (changingPassword)
            {
                session.User.PasswordHash = CreatePasswordHash(req.Post["password1"].Value);
                messages.Add("Password updated.");
            }

            var changingGameOptions = false;
            foreach (var (curVal, setter, key) in Ut.NewArray<(bool curVal, Action<bool> setter, string key)>(
                (session.User.ShowErrors, v => { session.User.ShowErrors = v; }, "opt-show-errors"),
                (session.User.SemitransparentXs, v => { session.User.SemitransparentXs = v; }, "opt-semitransparent-xs"),
                (session.User.ShowSolveTime, v => { session.User.ShowSolveTime = v; }, "opt-show-solve-time"),
                (session.User.PlayInvalidSound, v => { session.User.PlayInvalidSound = v; }, "opt-play-invalid-sound")
            ))
            {
                var newVal = req.Post[key].Value == "1";
                if (curVal != newVal)
                {
                    setter(newVal);
                    changingGameOptions = true;
                }
            }
            if (int.TryParse(req.Post["opt-backspace-option"].Value, out var backspaceOption) && session.User.BackspaceOption != backspaceOption)
            {
                session.User.BackspaceOption = backspaceOption;
                changingGameOptions = true;
            }

            if (changingGameOptions)
                messages.Add("Game options updated.");

            if (messages.Count == 0)
                messages.Add("No changes were made.");

            db.SaveChanges();
            return userPage(req, redirectTo, session.User, updateUserSuccess: messages);
        });

        public static string CreatePasswordHash(string password)
        {
            var salt = RandomNumberGenerator.GetBytes(8);
            return salt.ToHex().ToLowerInvariant() + ":" + SHA256.HashData((salt.ToHex().ToLowerInvariant() + password).ToUtf8()).ToHex();
        }

        private static bool verifyPasswordHash(string password, string hash)
        {
            if (hash == null || hash.Length == 0)
                return password.Length == 0;
            var parts = hash.Split(':');
            if (parts.Length != 2)
                return false;
            return string.Equals(parts[1], SHA256.HashData((parts[0].ToLowerInvariant() + password).ToUtf8()).ToHex(), StringComparison.OrdinalIgnoreCase);
        }
    }
}
