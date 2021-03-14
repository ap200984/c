using System;
using System.Net;
using System.Net.Mail;


namespace BGP_sessions2
{
    public class cSendMail
    {
        public cSendMail(string _from, string _to, string _login, string _passwd, string _subject, string _message)
        {
            try
            {
                MailMessage mail = new MailMessage();
                mail.From = new MailAddress(_from);
                mail.To.Add(new MailAddress(_to));
                mail.Subject = _subject;
                mail.Body = _message;
                //if (!string.IsNullOrEmpty(attachFile))
                //mail.Attachments.Add(new Attachment(attachFile));
                SmtpClient client = new SmtpClient();                
                client.Host = "smtp.ddos-guard.net";
                client.Port = 25;
                client.EnableSsl = true;                

                client.Credentials = new NetworkCredential(_login,_passwd);
                client.DeliveryMethod = SmtpDeliveryMethod.Network;
                client.Send(mail);
                mail.Dispose();
            }
            catch (Exception e)
            {
                    throw new Exception("Mail.Send: " + e.Message);
            }
                
        }
    }
}