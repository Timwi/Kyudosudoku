using System;
using System.Linq;
using KyudosudokuWebsite.Database;
using RT.Servers;
using RT.Util.ExtensionMethods;
using Session = KyudosudokuWebsite.Database.Session;
using SessionBase = RT.Servers.Session;

namespace KyudosudokuWebsite
{
    sealed class DbSession(Db db) : SessionBase
    {
        private Session _session;
        private User _user;

        public User User
        {
            get => _user;
            set
            {
                _user = value;
                Action = value == null ? SessionAction.Delete : SessionAction.Save;
                SessionModified = true;
            }
        }

        protected override bool ReadSession()
        {
            _session = db.Sessions.FirstOrDefault(s => s.SessionID == SessionID);
            if (_session == null)
                return false;
            User = db.Users.FirstOrDefault(u => u.UserID == _session.UserID);
            return true;
        }

        protected override void SaveSession()
        {
            if (_user == null)
            {
                if (_session != null)
                    db.Sessions.Remove(_session);
            }
            else if (_session == null)
                db.Sessions.Add(new Session { UserID = _user.UserID, SessionID = SessionID, LastLogin = DateTime.UtcNow });
            else
            {
                _session.UserID = _user.UserID;
                _session.SessionID = SessionID;
            }
            db.SaveChanges();
        }

        protected override void DeleteSession()
        {
            if (_session != null)
            {
                db.Sessions.Remove(_session);
                db.SaveChanges();
            }
        }
    }
}
