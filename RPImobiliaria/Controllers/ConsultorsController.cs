using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using RPImobiliaria.Data;
using RPImobiliaria.Models;
using Microsoft.AspNetCore.Hosting; // Necessário para gerir caminhos de ficheiros
using System.IO; // Necessário para FileStream e manipulação de ficheiros

namespace RPImobiliaria.Controllers
{
    [Authorize(Roles = "Admin")]
    public class ConsultorsController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<ApplicationUser> _userManager;
        // 1. Variável para aceder às pastas físicas do servidor Proxmox
        private readonly IWebHostEnvironment _hostEnvironment;

        // 2. Adicionado ao construtor
        public ConsultorsController(ApplicationDbContext context, UserManager<ApplicationUser> userManager, IWebHostEnvironment hostEnvironment)
        {
            _context = context;
            _userManager = userManager;
            _hostEnvironment = hostEnvironment;
        }

        // GET: Consultors
        public async Task<IActionResult> Index()
        {
            return View(await _context.Consultores.ToListAsync());
        }

        // GET: Consultors/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var consultor = await _context.Consultores
                .FirstOrDefaultAsync(m => m.Id == id);
            if (consultor == null)
            {
                return NotFound();
            }

            return View(consultor);
        }

        // GET: Consultors/Create
        public IActionResult Create()
        {
            return View();
        }

        // POST: Consultors/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("Id,Nome,Email,Telemovel,LicencaAMI")] Consultor consultor, IFormFile? fotoUpload)
        {
            // Limpamos do ModelState os campos não preenchidos diretamente
            ModelState.Remove("CaminhoFotoPerfil"); // Alterado do antigo FotoPerfil
            ModelState.Remove("ApplicationUserId");
            ModelState.Remove("ImoveisAngariados");

            if (ModelState.IsValid)
            {
                if (string.IsNullOrEmpty(consultor.Email))
                {
                    ModelState.AddModelError("Email", "O Email é obrigatório para podermos criar o Login do consultor.");
                    return View(consultor);
                }

                var userExistente = await _userManager.FindByEmailAsync(consultor.Email);
                if (userExistente != null)
                {
                    ModelState.AddModelError("Email", "Já existe uma conta no sistema com este email.");
                    return View(consultor);
                }

                var novoUser = new ApplicationUser
                {
                    UserName = consultor.Email,
                    Email = consultor.Email,
                    EmailConfirmed = true
                };

                var resultadoIdentity = await _userManager.CreateAsync(novoUser, "Sharp123!");

                if (resultadoIdentity.Succeeded)
                {
                    await _userManager.AddToRoleAsync(novoUser, "Consultor");
                    consultor.ApplicationUserId = novoUser.Id;

                    // LÓGICA NOVA: GUARDAR A FOTO NUMA PASTA FÍSICA
                    if (fotoUpload != null && fotoUpload.Length > 0)
                    {
                        if (fotoUpload.ContentType.StartsWith("image/"))
                        {
                            string pastaUploads = Path.Combine(_hostEnvironment.WebRootPath, "uploads", "consultores");
                            if (!Directory.Exists(pastaUploads))
                            {
                                Directory.CreateDirectory(pastaUploads);
                            }

                            string nomeUnico = Guid.NewGuid().ToString() + "_" + fotoUpload.FileName;
                            string caminhoCompleto = Path.Combine(pastaUploads, nomeUnico);

                            using (var fileStream = new FileStream(caminhoCompleto, FileMode.Create))
                            {
                                await fotoUpload.CopyToAsync(fileStream);
                            }

                            // Guarda apenas o caminho de texto na base de dados
                            consultor.CaminhoFotoPerfil = "/uploads/consultores/" + nomeUnico;
                        }
                        else
                        {
                            ModelState.AddModelError("CaminhoFotoPerfil", "O ficheiro enviado não é uma imagem válida.");
                            await _userManager.DeleteAsync(novoUser);
                            return View(consultor);
                        }
                    }

                    _context.Add(consultor);
                    await _context.SaveChangesAsync();

                    return RedirectToAction(nameof(Index));
                }
                else
                {
                    foreach (var erro in resultadoIdentity.Errors)
                    {
                        ModelState.AddModelError(string.Empty, erro.Description);
                    }
                }
            }

            return View(consultor);
        }

        // GET: Consultors/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var consultor = await _context.Consultores.FindAsync(id);
            if (consultor == null)
            {
                return NotFound();
            }
            return View(consultor);
        }

        // POST: Consultors/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("Id,Nome,Email,Telemovel,LicencaAMI,ApplicationUserId")] Consultor consultor, IFormFile? fotoUpload)
        {
            if (id != consultor.Id)
            {
                return NotFound();
            }

            ModelState.Remove("CaminhoFotoPerfil"); // Alterado do antigo
            ModelState.Remove("ApplicationUserId");

            if (ModelState.IsValid)
            {
                try
                {
                    var consultorExistente = await _context.Consultores
                        .AsNoTracking()
                        .FirstOrDefaultAsync(c => c.Id == id);

                    if (consultorExistente == null) return NotFound();

                    // LÓGICA DE FOTO NO EDIT
                    if (fotoUpload != null && fotoUpload.Length > 0)
                    {
                        if (fotoUpload.ContentType.StartsWith("image/"))
                        {
                            string pastaUploads = Path.Combine(_hostEnvironment.WebRootPath, "uploads", "consultores");
                            if (!Directory.Exists(pastaUploads)) Directory.CreateDirectory(pastaUploads);

                            string nomeUnico = Guid.NewGuid().ToString() + "_" + fotoUpload.FileName;
                            string caminhoCompleto = Path.Combine(pastaUploads, nomeUnico);

                            // Guarda a nova imagem no disco
                            using (var fileStream = new FileStream(caminhoCompleto, FileMode.Create))
                            {
                                await fotoUpload.CopyToAsync(fileStream);
                            }

                            consultor.CaminhoFotoPerfil = "/uploads/consultores/" + nomeUnico;

                            // LIMPEZA: Apaga a foto antiga do servidor para poupar espaço
                            if (!string.IsNullOrEmpty(consultorExistente.CaminhoFotoPerfil))
                            {
                                string caminhoFotoAntiga = Path.Combine(_hostEnvironment.WebRootPath, consultorExistente.CaminhoFotoPerfil.TrimStart('/'));
                                if (System.IO.File.Exists(caminhoFotoAntiga))
                                {
                                    System.IO.File.Delete(caminhoFotoAntiga);
                                }
                            }
                        }
                    }
                    else
                    {
                        // Se não fizer upload de nada, mantém a foto antiga
                        consultor.CaminhoFotoPerfil = consultorExistente.CaminhoFotoPerfil;
                    }

                    if (string.IsNullOrEmpty(consultor.ApplicationUserId))
                    {
                        consultor.ApplicationUserId = consultorExistente.ApplicationUserId;
                    }

                    _context.Update(consultor);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!ConsultorExists(consultor.Id))
                    {
                        return NotFound();
                    }
                    else
                    {
                        throw;
                    }
                }
                return RedirectToAction(nameof(Index));
            }
            return View(consultor);
        }

        // GET: Consultors/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var consultor = await _context.Consultores
                .FirstOrDefaultAsync(m => m.Id == id);
            if (consultor == null)
            {
                return NotFound();
            }

            return View(consultor);
        }

        // POST: Consultors/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var consultor = await _context.Consultores.FindAsync(id);
            if (consultor != null)
            {
                // 1. Apaga a conta de acesso no Identity
                if (!string.IsNullOrEmpty(consultor.ApplicationUserId))
                {
                    var user = await _userManager.FindByIdAsync(consultor.ApplicationUserId);
                    if (user != null)
                    {
                        await _userManager.DeleteAsync(user);
                    }
                }

                // 2. Apaga a fotografia física do disco do servidor
                if (!string.IsNullOrEmpty(consultor.CaminhoFotoPerfil))
                {
                    string caminhoFotoAntiga = Path.Combine(_hostEnvironment.WebRootPath, consultor.CaminhoFotoPerfil.TrimStart('/'));
                    if (System.IO.File.Exists(caminhoFotoAntiga))
                    {
                        System.IO.File.Delete(caminhoFotoAntiga);
                    }
                }

                // 3. Apaga o registo da base de dados
                _context.Consultores.Remove(consultor);
                await _context.SaveChangesAsync();
            }

            return RedirectToAction(nameof(Index));
        }

        private bool ConsultorExists(int id)
        {
            return _context.Consultores.Any(e => e.Id == id);
        }
    }
}
