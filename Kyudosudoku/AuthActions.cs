using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Mail;
using System.Security.Cryptography;
using KyudosudokuWebsite.Database;
using RT.Servers;
using RT.TagSoup;
using RT.Util;
using RT.Util.ExtensionMethods;

namespace KyudosudokuWebsite
{
    partial class KyudosudokuPropellerModule
    {
        private object hiddens(params (string name, object value)[] items) => items.Select(item => new INPUT { type = itype.hidden, name = item.name, value = item.value.ToString() });

        private HttpResponse logout(HttpRequest req, IHttpUrl redirectTo) => withSession(req, (session, db) =>
        {
            if (session != null)
                session.Action = SessionAction.Delete;
            return HttpResponse.Redirect(redirectTo);
        });

        private HttpResponse login(HttpRequest req, IHttpUrl redirectTo) => withSession(req, (session, db) =>
        {
            var username = req.Post["username"].Value;
            var user = db.Users.FirstOrDefault(u => u.Username == username) ?? db.Users.FirstOrDefault(u => u.Username.Equals(username, StringComparison.InvariantCultureIgnoreCase) || u.EmailAddress.Equals(username, StringComparison.InvariantCultureIgnoreCase));
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
                PasswordHash = createPasswordHash(password)
            });
            db.SaveChanges();
            session.User = newUser;
            if (!string.IsNullOrWhiteSpace(email) && email.Contains('@'))
                sendMail($"Kyudosudoku registration",
                    $"<p style='font-weight: bold'>Thank you for registering for Kyudosudoku!</p>" +
                    $"<p>Your username is {username}.</p>" +
                    $"<p>You can <a href='{redirectTo.ToFull()}'>log in</a> at any time.",
                    new MailAddress(email, username));
            return HttpResponse.Redirect(redirectTo);
        });

        private HttpResponse updateUser(HttpRequest req, IHttpUrl redirectTo) => withSession(req, (session, db) =>
        {
            if (session.User == null)
                return HttpResponse.Redirect(redirectTo);
            if (req.Post["user"].Value != session.User.UserID.ToString())
                return userPage(req, redirectTo, session.User, db, updateUserError: "It appears that you logged out and back in as someone else. Please try again.");

            var changingPassword = !string.IsNullOrWhiteSpace(req.Post["password1"].Value);
            if (changingPassword)
            {
                if (!verifyPasswordHash(req.Post["oldpassword"].Value, session.User.PasswordHash))
                    return userPage(req, redirectTo, session.User, db, updateUserError: "The specified old password was not correct.");
                if (req.Post["password1"].Value != req.Post["password2"].Value)
                    return userPage(req, redirectTo, session.User, db, updateUserError: "The two new passwords do not match.");
            }

            var messages = new List<string>();

            var newUsername = req.Post["username"].Value;
            if (!string.IsNullOrWhiteSpace(newUsername) && newUsername != session.User.Username)
            {
                if (db.Users.Any(u => u.UserID != session.User.UserID && (u.Username == newUsername || u.EmailAddress == newUsername)))
                    return userPage(req, redirectTo, session.User, db, updateUserError: "I’m afraid that username is already taken!");

                session.User.Username = newUsername;
                messages.Add("Username changed.");
            }

            var newEmail = req.Post["email"].Value;
            if (!string.IsNullOrWhiteSpace(newEmail) && newEmail != session.User.EmailAddress)
            {
                if (db.Users.Any(u => u.UserID != session.User.UserID && (u.Username == newEmail || u.EmailAddress == newEmail)))
                    return userPage(req, redirectTo, session.User, db, updateUserError: "I’m afraid that email address is already taken!");

                session.User.EmailAddress = newEmail;
                messages.Add("Email address changed.");
            }
            if (changingPassword)
            {
                session.User.PasswordHash = createPasswordHash(req.Post["password1"].Value);
                messages.Add("Password changed.");
            }
            if (messages.Count == 0)
                messages.Add("No changes were made.");

            db.SaveChanges();
            return userPage(req, redirectTo, session.User, db, updateUserSuccess: messages);
        });

        private static string createPasswordHash(string password)
        {
            var salt = new byte[8];
            new RNGCryptoServiceProvider().GetBytes(salt);
            using var sha = SHA256.Create();
            return salt.ToHex().ToLowerInvariant() + ":" + sha.ComputeHash((salt.ToHex().ToLowerInvariant() + password).ToUtf8()).ToHex();
        }

        private static bool verifyPasswordHash(string password, string hash)
        {
            if (hash == null || hash.Length == 0)
                return password.Length == 0;
            var parts = hash.Split(':');
            if (parts.Length != 2)
                return false;
            using var sha = SHA256.Create();
            return string.Equals(parts[1], sha.ComputeHash((parts[0].ToLowerInvariant() + password).ToUtf8()).ToHex(), StringComparison.OrdinalIgnoreCase);
        }
    }
}
