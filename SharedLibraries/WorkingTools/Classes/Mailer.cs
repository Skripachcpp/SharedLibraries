using System;
using System.Net;
using System.Net.Mail;
using System.Threading;
using System.Threading.Tasks;

namespace WorkingTools.Classes
{
    public class Mailer
    {
        private readonly MailerConnection _cnt;

        public Mailer(MailerConnection cnt)
        {
            _cnt = cnt;
        }

        /// <summary>
        /// Отправка письма на почтовый ящик C# mail send
        /// </summary>
        /// <param name="mailto">Адрес получателя</param>
        /// <param name="caption">Тема письма</param>
        /// <param name="body">Сообщение</param>
        /// <param name="isBodyHtml">Сообщение является html</param>
        public async Task SendMail(string mailto, string caption, string body, bool isBodyHtml = false)
        {
            if (string.IsNullOrWhiteSpace(mailto)) return;

            var fromAddress = new MailAddress(_cnt.MailFrom);
            var toAddress = new MailAddress(mailto);

            using (var smtp = new SmtpClient
            {
                Host = _cnt.Host,
                Port = _cnt.Port,
                EnableSsl = _cnt.EnableSsl,
                DeliveryMethod = _cnt.DeliveryMethod,
                UseDefaultCredentials = _cnt.UseDefaultCredentials,
                TargetName = _cnt.TargetName,
                Credentials = new NetworkCredential(_cnt.Login, _cnt.Pass)
            })
            using (var mes = new MailMessage(fromAddress, toAddress)
            {
                Subject = caption,
                Body = body,
                IsBodyHtml = isBodyHtml,
            })
            {
                Exception exception = null;
                for (int i = 0; i < 3; i++)
                {
                    exception = null;
                    try
                    {
                        await smtp.SendMailAsync(mes);
                    }
                    catch (Exception ex)
                    {
                        exception = ex;
                        await Task.Delay(250);
                        continue;
                    }
                    break;
                }

                if (exception != null) throw exception;
            }
        }
    }


    public class MailerConnection
    {
        public string MailFrom { get; set; }

        public string Host { get; set; }
        public string TargetName { get; set; }
        public string Login { get; set; }
        public string Pass { get; set; }
        public int Port { get; set; }
        public bool EnableSsl { get; set; } = true;
        public SmtpDeliveryMethod DeliveryMethod { get; set; } = SmtpDeliveryMethod.Network;
        public bool UseDefaultCredentials { get; set; } = false;
    }
}
