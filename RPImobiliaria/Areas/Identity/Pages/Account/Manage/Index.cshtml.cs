using System;
using System.ComponentModel.DataAnnotations;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using RPImobiliaria.Data;

namespace RPImobiliaria.Areas.Identity.Pages.Account.Manage
{
    public class IndexModel : PageModel
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly SignInManager<ApplicationUser> _signInManager;
        private readonly ApplicationDbContext _context;

        public IndexModel(
            UserManager<ApplicationUser> userManager,
            SignInManager<ApplicationUser> signInManager,
            ApplicationDbContext context)
        {
            _userManager = userManager;
            _signInManager = signInManager;
            _context = context;
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

            public bool IsConsultor { get; set; }
            public bool IsCliente { get; set; }
        }

        private async Task LoadAsync(ApplicationUser user)
        {
            var userName = await _userManager.GetUserNameAsync(user);
            Username = userName;

            Input = new InputModel();

            var consultor = await _context.Consultores.FirstOrDefaultAsync(c => c.ApplicationUserId == user.Id);
            var cliente = await _context.Clientes.FirstOrDefaultAsync(c => c.ApplicationUserId == user.Id);

            if (consultor != null)
            {
                Input.IsConsultor = true;
                Input.Nome = consultor.Nome;
                Input.Telemovel = consultor.Telemovel;
            }
            else if (cliente != null)
            {
                Input.IsCliente = true;
                Input.Nome = cliente.Nome;
                Input.Telemovel = cliente.Telemovel;
                Input.NIF = cliente.NIF;
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

            // Remove os validadores das flags para não bloquearem o form
            ModelState.Remove("Input.IsConsultor");
            ModelState.Remove("Input.IsCliente");

            if (!ModelState.IsValid)
            {
                await LoadAsync(user);
                return Page();
            }

            var consultor = await _context.Consultores.FirstOrDefaultAsync(c => c.ApplicationUserId == user.Id);
            var cliente = await _context.Clientes.FirstOrDefaultAsync(c => c.ApplicationUserId == user.Id);

            if (consultor != null)
            {
                consultor.Nome = Input.Nome;
                consultor.Telemovel = Input.Telemovel;
                _context.Update(consultor);
            }
            else if (cliente != null)
            {
                cliente.Nome = Input.Nome;
                cliente.Telemovel = Input.Telemovel;
                if (Input.NIF != null) cliente.NIF = Input.NIF;
                _context.Update(cliente);
            }

            await _context.SaveChangesAsync();
            await _signInManager.RefreshSignInAsync(user);

            TempData["MensagemSucesso"] = "O seu perfil foi atualizado com sucesso!";

            return RedirectToPage();
        }
    }
}