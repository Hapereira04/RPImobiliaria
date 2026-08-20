using Microsoft.AspNetCore.Identity.UI.Services;
using System.Net;
using System.Net.Mail;
using System.Threading.Tasks;

namespace RPImobiliaria.Services
{
    public class EmailSender : IEmailSender
    {
        public async Task SendEmailAsync(string email, string subject, string htmlMessage)
        {
            // Substitua pelos dados do email da sua imobiliária
            var mailRemetente = "ruipereira.imo@gmail.com";
            var senhaApp = "ihxdkmfmupnjupih"; // Ver aviso abaixo!

            var client = new SmtpClient("smtp.gmail.com", 587)
            {
                EnableSsl = true,
                UseDefaultCredentials = false,
                Credentials = new NetworkCredential(mailRemetente, senhaApp)
            };

            var mailMessage = new MailMessage(from: mailRemetente, to: email, subject, htmlMessage)
            {
                IsBodyHtml = true
            };

            await client.SendMailAsync(mailMessage);
        }
    }
}