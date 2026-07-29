using Microsoft.AspNetCore.Mvc;
using System.Diagnostics;
using System.Net;
using System.Net.Mail;
using Microsoft.Extensions.Configuration; // Adicionar isto
using RPImobiliaria.Models; // Confirma que o namespace do teu projeto está correto

namespace RPImobiliaria.Controllers
{
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;
        private readonly IConfiguration _configuration; // Para ler o appsettings.json

        // Atualizar o construtor para receber as configurações
        public HomeController(ILogger<HomeController> logger, IConfiguration configuration)
        {
            _logger = logger;
            _configuration = configuration;
        }

        public IActionResult Index()
        {
            return View();
        }

        public IActionResult QuemSomos()
        {
            return View();
        }

        // LÓGICA DE ENVIO DO FORMULÁRIO DE CONTACTO
        [HttpPost]
        public IActionResult EnviarContacto(string Nome, string Contacto, string Assunto, string Mensagem)
        {
            try
            {
                // 1. Ler as configurações do appsettings.json
                var smtpServer = _configuration["EmailSettings:SmtpServer"];
                var smtpPort = int.Parse(_configuration["EmailSettings:SmtpPort"]);
                var senderEmail = _configuration["EmailSettings:SenderEmail"];
                var password = _configuration["EmailSettings:Password"];
                var destinationEmail = _configuration["EmailSettings:DestinationEmail"];
                var senderName = _configuration["EmailSettings:SenderName"];

                // 2. Construir o Email
                MailMessage mail = new MailMessage();
                mail.From = new MailAddress(senderEmail, senderName);
                mail.To.Add(destinationEmail); // Para onde vai o email (o teu próprio email)

                // Assunto
                mail.Subject = $"Novo Contacto pelo Site: {Assunto ?? "Sem Assunto"}";

                // Corpo do Email (formatação HTML bonita)
                mail.IsBodyHtml = true;
                mail.Body = $@"
                    <div style='font-family: Arial, sans-serif; padding: 20px; border: 1px solid #ccc; border-radius: 10px;'>
                        <h2 style='color: #0d6efd;'>Novo Pedido de Contacto</h2>
                        <p><strong>Nome do Cliente:</strong> {Nome}</p>
                        <p><strong>Telefone / Email de Contacto:</strong> {Contacto}</p>
                        <p><strong>Assunto:</strong> {Assunto ?? "N/A"}</p>
                        <hr/>
                        <p><strong>Mensagem:</strong></p>
                        <p style='background-color: #f8f9fa; padding: 15px; border-left: 4px solid #0d6efd;'>
                            {Mensagem.Replace("\n", "<br>")}
                        </p>
                        <br/>
                        <p style='font-size: 12px; color: #888;'>Mensagem enviada automaticamente a partir do formulário da Página Inicial.</p>
                    </div>";

                // 3. Configurar o Cliente SMTP do Gmail e Enviar
                using (SmtpClient smtp = new SmtpClient(smtpServer, smtpPort))
                {
                    smtp.Credentials = new NetworkCredential(senderEmail, password);
                    smtp.EnableSsl = true; // OBRIGATÓRIO PARA GMAIL
                    smtp.Send(mail);
                }

                // 4. Se correr bem, redireciona para a Home com mensagem de sucesso (Podes usar TempData)
                TempData["MensagemSucesso"] = "A sua mensagem foi enviada com sucesso! Entraremos em contacto em breve.";
                return RedirectToAction("Index", "Home");
            }
            catch (Exception ex)
            {
                // Se der erro, guarda o erro no logger e avisa o utilizador
                _logger.LogError(ex, "Erro ao enviar email de contacto.");
                TempData["MensagemErro"] = "Ocorreu um erro ao enviar a mensagem. Por favor, tente novamente mais tarde ou ligue diretamente.";
                return RedirectToAction("Index", "Home");
            }
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}