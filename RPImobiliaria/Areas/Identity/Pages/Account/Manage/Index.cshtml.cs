using System;
using System.ComponentModel.DataAnnotations;
using System.IO;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using RPImobiliaria.Data;
using Microsoft.AspNetCore.Hosting; // Adicionado para gerir pastas físicas

namespace RPImobiliaria.Areas.Identity.Pages.Account.Manage
{
    public class IndexModel : PageModel
    {
        private readonly UserManager<IdentityUser> _userManager;
        private readonly SignInManager<IdentityUser> _signInManager;
        private readonly ApplicationDbContext _context;
        private readonly IWebHostEnvironment _hostEnvironment; // NOVA VARIÁVEL PARA AS PASTAS

        public IndexModel(
            UserManager<IdentityUser> userManager,
            SignInManager<IdentityUser> signInManager,
            ApplicationDbContext context,
            IWebHostEnvironment hostEnvironment) // Injetado no construtor
        {
            _userManager = userManager;
            _signInManager = signInManager;
            _context = context;
            _hostEnvironment = hostEnvironment;
        }

        public string Username { get; set; }

        [TempData]
        public string StatusMessage { get; set; }

        [BindProperty]
        public InputModel Input { get; set; }

        public class InputModel
        {
            [Display(Name = "Nome Completo")]
            public string? Nome { get; set; }

            [Phone(ErrorMessage = "Número de telemóvel inválido.")]
            [Display(Name = "Telemóvel")]
            public string? Telemovel { get; set; }

            [Display(Name = "NIF")]
            public string? NIF { get; set; }

            [Display(Name = "Licença AMI")]
            public string? LicencaAMI { get; set; }

            [Display(Name = "Nova Foto de Perfil")]
            public IFormFile? FotoUpload { get; set; }

            // Alterado de FotoBase64Atual para CaminhoFotoAtual
            public string? CaminhoFotoAtual { get; set; }
            public bool IsConsultor { get; set; }
            public bool IsCliente { get; set; }
        }

        private async Task LoadAsync(IdentityUser user)
        {
            var userName = await _userManager.GetUserNameAsync(user);
            Username = userName;

            Input = new InputModel();

            var consultor = await _context.Consultores.FirstOrDefaultAsync(c => c.IdentityUserId == user.Id);
            var cliente = await _context.Clientes.FirstOrDefaultAsync(c => c.IdentityUserId == user.Id);

            if (consultor != null)
            {
                Input.IsConsultor = true;
                Input.Nome = consultor.Nome;
                Input.Telemovel = consultor.Telemovel;
                Input.LicencaAMI = consultor.LicencaAMI;
                Input.CaminhoFotoAtual = consultor.CaminhoFotoPerfil; // Lê o caminho diretamente
            }
            else if (cliente != null)
            {
                Input.IsCliente = true;
                Input.Nome = cliente.Nome;
                Input.Telemovel = cliente.Telemovel;
                Input.NIF = cliente.NIF;
                Input.CaminhoFotoAtual = cliente.CaminhoFotoPerfil; // Lê o caminho diretamente
            }
        }

        public async Task<IActionResult> OnGetAsync()
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null) { return NotFound($"Erro ao carregar o utilizador com ID '{_userManager.GetUserId(User)}'."); }

            await LoadAsync(user);
            return Page();
        }

        public async Task<IActionResult> OnPostAsync()
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null) { return NotFound($"Erro ao carregar o utilizador com ID '{_userManager.GetUserId(User)}'."); }

            ModelState.Remove("Input.CaminhoFotoAtual");
            ModelState.Remove("Input.IsConsultor");
            ModelState.Remove("Input.IsCliente");

            if (!ModelState.IsValid)
            {
                await LoadAsync(user);
                return Page();
            }

            var consultor = await _context.Consultores.FirstOrDefaultAsync(c => c.IdentityUserId == user.Id);
            var cliente = await _context.Clientes.FirstOrDefaultAsync(c => c.IdentityUserId == user.Id);

            string caminhoNovaFoto = null;

            // NOVA LÓGICA DE UPLOAD FÍSICO
            if (Input.FotoUpload != null && Input.FotoUpload.Length > 0)
            {
                if (Input.FotoUpload.ContentType.StartsWith("image/"))
                {
                    // Descobre para que pasta vai a foto dependendo do tipo de utilizador
                    string pastaDestino = consultor != null ? "consultores" : "clientes";
                    string pastaUploads = Path.Combine(_hostEnvironment.WebRootPath, "uploads", pastaDestino);

                    if (!Directory.Exists(pastaUploads)) Directory.CreateDirectory(pastaUploads);

                    string nomeUnico = Guid.NewGuid().ToString() + "_" + Input.FotoUpload.FileName;
                    string caminhoCompleto = Path.Combine(pastaUploads, nomeUnico);

                    using (var fileStream = new FileStream(caminhoCompleto, FileMode.Create))
                    {
                        await Input.FotoUpload.CopyToAsync(fileStream);
                    }

                    caminhoNovaFoto = "/uploads/" + pastaDestino + "/" + nomeUnico;
                }
            }

            if (consultor != null)
            {
                consultor.Nome = Input.Nome;
                consultor.Telemovel = Input.Telemovel;
                if (Input.LicencaAMI != null) consultor.LicencaAMI = Input.LicencaAMI;

                if (caminhoNovaFoto != null)
                {
                    // Apaga a foto antiga do servidor se existir
                    if (!string.IsNullOrEmpty(consultor.CaminhoFotoPerfil))
                    {
                        string fotoAntiga = Path.Combine(_hostEnvironment.WebRootPath, consultor.CaminhoFotoPerfil.TrimStart('/'));
                        if (System.IO.File.Exists(fotoAntiga)) System.IO.File.Delete(fotoAntiga);
                    }
                    consultor.CaminhoFotoPerfil = caminhoNovaFoto;
                }
                _context.Update(consultor);
            }
            else if (cliente != null)
            {
                cliente.Nome = Input.Nome;
                cliente.Telemovel = Input.Telemovel;
                if (Input.NIF != null) cliente.NIF = Input.NIF;

                if (caminhoNovaFoto != null)
                {
                    // Apaga a foto antiga do servidor se existir
                    if (!string.IsNullOrEmpty(cliente.CaminhoFotoPerfil))
                    {
                        string fotoAntiga = Path.Combine(_hostEnvironment.WebRootPath, cliente.CaminhoFotoPerfil.TrimStart('/'));
                        if (System.IO.File.Exists(fotoAntiga)) System.IO.File.Delete(fotoAntiga);
                    }
                    cliente.CaminhoFotoPerfil = caminhoNovaFoto;
                }
                _context.Update(cliente);
            }

            await _context.SaveChangesAsync();
            await _signInManager.RefreshSignInAsync(user);

            TempData["MensagemSucesso"] = "O seu perfil foi atualizado com sucesso!";

            return RedirectToPage();
        }
    }
}