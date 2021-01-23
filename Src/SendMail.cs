using System.Collections.Generic;
using System.Net.Mail;
using RT.Util;

namespace KyudosudokuWebsite
{
    partial class KyudosudokuPropellerModule
    {
        //private void sendMail(string subject, string body, params MailAddress[] to) => sendMail(to, subject, body);
        //private void sendMail(IEnumerable<MailAddress> to, string subject, string body)
        //{
        //    using var smtp = new RTSmtpClient(new SmtpSettings { Encryption = SmtpEncryption.Ssl, Host = Settings.SmtpServer, Password = Settings.SmtpPassword, Port = Settings.SmtpPort, Username = Settings.SmtpUsername }, Log);
        //    smtp.SendEmail(new MailAddress(Settings.SmtpFromAddress, "Kyudosudoku"), to, subject, null, body);
        //}
    }

    sealed class SmtpSettings : RTSmtpSettings
    {
        protected override string DecryptPassword(string encrypted) => encrypted;
        protected override string EncryptPassword(string decrypted) => decrypted;
    }
}
