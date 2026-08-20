using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using RPImobiliaria.Data;
using RPImobiliaria.Models;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace RPImobiliaria.Controllers
{
    // APENAS ADMINS ENTRAM AQUI: Impede que consultores criem outros consultores
    [Authorize(Roles = "Admin")]
    public class ConsultoresController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<ApplicationUser> _userManager;

        public ConsultoresController(ApplicationDbContext context, UserManager<ApplicationUser> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        // GET: Consultores
        public async Task<IActionResult> Index(string sortOrder, string searchString)
        {
            ViewData["NameSortParm"] = String.IsNullOrEmpty(sortOrder) ? "name_desc" : "";
            ViewData["CurrentFilter"] = searchString;

            var query = _context.Consultores
                .Include(c => c.ImoveisAngariados)
                .AsQueryable();

            // Pesquisa por Nome ou Email
            if (!String.IsNullOrEmpty(searchString))
            {
                var termo = searchString.ToLower();
                query = query.Where(c => c.Nome.ToLower().Contains(termo) ||
                                         c.Email.ToLower().Contains(termo));
            }

            switch (sortOrder)
            {
                case "name_desc":
                    query = query.OrderByDescending(c => c.Nome);
                    break;
                default:
                    query = query.OrderBy(c => c.Nome);
                    break;
            }

            return View(await query.ToListAsync());
        }

        // GET: Consultores/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null) return NotFound();

            var consultor = await _context.Consultores
                .Include(c => c.ImoveisAngariados)
                .FirstOrDefaultAsync(m => m.Id == id);

            return consultor == null ? NotFound() : View(consultor);
        }

        // GET: Consultores/Create
        public IActionResult Create() => View();

        // POST: Consultores/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("Nome,Email,Telemovel")] Consultor consultor)
        {
            ModelState.Remove("ApplicationUserId");
            ModelState.Remove("ImoveisAngariados");

            if (ModelState.IsValid)
            {
                // 1. Verificar se email já existe no Identity
                var userExistente = await _userManager.FindByEmailAsync(consultor.Email);
                if (userExistente != null)
                {
                    ModelState.AddModelError("Email", "Já existe uma conta no sistema com este email.");
                    return View(consultor);
                }

                // 2. Criar a conta de acesso (Login) para o Consultor
                var novoUser = new ApplicationUser
                {
                    UserName = consultor.Email,
                    Email = consultor.Email,
                    EmailConfirmed = true
                };

                // Define uma senha padrão provisória que o consultor usará na primeira vez
                var resultadoIdentity = await _userManager.CreateAsync(novoUser, "Mudar@123");

                if (resultadoIdentity.Succeeded)
                {
                    // 3. Atribuir o cargo (Role) de "Consultor"
                    await _userManager.AddToRoleAsync(novoUser, "Consultor");

                    // 4. Ligar a conta ao registo do Consultor na base de dados
                    consultor.ApplicationUserId = novoUser.Id;
                    consultor.Nome = consultor.Nome.Trim();

                    _context.Add(consultor);
                    await _context.SaveChangesAsync();

                    TempData["MensagemSucesso"] = "Consultor criado. A senha provisória é Mudar@123";
                    return RedirectToAction(nameof(Index));
                }
                else
                {
                    // Se falhar a criação da conta (ex: a senha não cumpriu os requisitos do sistema)
                    foreach (var erro in resultadoIdentity.Errors)
                    {
                        ModelState.AddModelError(string.Empty, erro.Description);
                    }
                }
            }
            return View(consultor);
        }

        // GET: Consultores/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null) return NotFound();

            var consultor = await _context.Consultores.FindAsync(id);
            return consultor == null ? NotFound() : View(consultor);
        }

        // POST: Consultores/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("Id,Nome,Email,Telemovel,ApplicationUserId")] Consultor consultor)
        {
            if (id != consultor.Id) return NotFound();

            ModelState.Remove("ApplicationUserId");
            ModelState.Remove("ImoveisAngariados");

            if (ModelState.IsValid)
            {
                try
                {
                    var consultorExistente = await _context.Consultores.AsNoTracking().FirstOrDefaultAsync(c => c.Id == id);
                    if (consultorExistente == null) return NotFound();

                    // Se você editar o Email do consultor, o sistema altera também o Email/Login dele automaticamente
                    if (consultor.Email != consultorExistente.Email && !string.IsNullOrEmpty(consultor.ApplicationUserId))
                    {
                        var userEmailExistente = await _userManager.FindByEmailAsync(consultor.Email);
                        if (userEmailExistente != null && userEmailExistente.Id != consultor.ApplicationUserId)
                        {
                            ModelState.AddModelError("Email", "Já existe outra conta com este email no sistema.");
                            return View(consultor);
                        }

                        var userIdentity = await _userManager.FindByIdAsync(consultor.ApplicationUserId);
                        if (userIdentity != null)
                        {
                            userIdentity.Email = consultor.Email;
                            userIdentity.UserName = consultor.Email;
                            await _userManager.UpdateAsync(userIdentity);
                        }
                    }

                    consultor.Nome = consultor.Nome.Trim();
                    _context.Update(consultor);
                    await _context.SaveChangesAsync();

                    TempData["MensagemSucesso"] = "Dados do consultor atualizados com sucesso.";
                    return RedirectToAction(nameof(Index));
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!ConsultorExists(consultor.Id)) return NotFound();
                    else throw;
                }
            }
            return View(consultor);
        }

        // GET: Consultores/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null) return NotFound();

            var consultor = await _context.Consultores
                .Include(c => c.ImoveisAngariados)
                .FirstOrDefaultAsync(m => m.Id == id);

            if (consultor == null) return NotFound();

            // Guardamos a contagem para avisar no ecrã de apagar
            ViewBag.TotalImoveis = consultor.ImoveisAngariados?.Count ?? 0;

            return View(consultor);
        }

        // POST: Consultores/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var consultor = await _context.Consultores
                .Include(c => c.ImoveisAngariados)
                .FirstOrDefaultAsync(c => c.Id == id);

            if (consultor != null)
            {
                // Bloqueio de Segurança para não deixar Imóveis órfãos
                if (consultor.ImoveisAngariados != null && consultor.ImoveisAngariados.Any())
                {
                    TempData["MensagemErro"] = "Bloqueado: Não é possível apagar este consultor porque tem imóveis angariados associados a si.";
                    return RedirectToAction(nameof(Index));
                }

                // 1. Apaga a conta de acesso/login no Identity
                if (!string.IsNullOrEmpty(consultor.ApplicationUserId))
                {
                    var user = await _userManager.FindByIdAsync(consultor.ApplicationUserId);
                    if (user != null) await _userManager.DeleteAsync(user);
                }

                // 2. Apaga o registo do consultor da base de dados
                _context.Consultores.Remove(consultor);
                await _context.SaveChangesAsync();

                TempData["MensagemSucesso"] = "Consultor (e respetiva conta de acesso) apagados com sucesso.";
            }

            return RedirectToAction(nameof(Index));
        }

        private bool ConsultorExists(int id) => _context.Consultores.Any(e => e.Id == id);
    }
}