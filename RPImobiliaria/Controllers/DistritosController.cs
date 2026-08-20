using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using RPImobiliaria.Data;
using RPImobiliaria.Models;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace RPImobiliaria.Controllers
{
    [Authorize(Roles = "Admin")]
    public class DistritosController : Controller
    {
        private readonly ApplicationDbContext _context;
        public DistritosController(ApplicationDbContext context) => _context = context;

        // GET: Distritos
        public async Task<IActionResult> Index(string sortOrder, string searchString)
        {
            ViewData["NameSortParm"] = String.IsNullOrEmpty(sortOrder) ? "name_desc" : "";
            ViewData["CurrentFilter"] = searchString;

            // Incluímos Concelhos e Freguesias para podermos contar na View
            var query = _context.Distritos
                .Include(d => d.Concelhos)
                    .ThenInclude(c => c.Freguesias)
                .AsQueryable();

            // Lógica de pesquisa ignorando maiúsculas/minúsculas
            if (!String.IsNullOrEmpty(searchString))
            {
                var termo = searchString.ToLower();
                query = query.Where(d => d.Nome.ToLower().Contains(termo));
            }

            // Lógica de Ordenação
            switch (sortOrder)
            {
                case "name_desc":
                    query = query.OrderByDescending(d => d.Nome);
                    break;
                default:
                    query = query.OrderBy(d => d.Nome);
                    break;
            }

            return View(await query.ToListAsync());
        }

        // GET: Distritos/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id is null) return NotFound();

            var distrito = await _context.Distritos
                .Include(d => d.Concelhos)
                    .ThenInclude(c => c.Freguesias)
                .FirstOrDefaultAsync(d => d.Id == id);

            return distrito is null ? NotFound() : View(distrito);
        }

        // GET: Distritos/Create
        public IActionResult Create() => View();

        // POST: Distritos/Create
        [HttpPost, ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("Nome")] Distrito distrito)
        {
            if (await NomeJaExiste(distrito.Nome))
                ModelState.AddModelError("Nome", "Já existe um distrito com este nome.");

            if (!ModelState.IsValid) return View(distrito);

            distrito.Nome = distrito.Nome.Trim();
            _context.Distritos.Add(distrito);
            await _context.SaveChangesAsync();

            TempData["MensagemSucesso"] = "Distrito criado com sucesso.";
            return RedirectToAction(nameof(Index));
        }

        // GET: Distritos/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id is null) return NotFound();
            var distrito = await _context.Distritos.FindAsync(id);
            return distrito is null ? NotFound() : View(distrito);
        }

        // POST: Distritos/Edit/5
        [HttpPost, ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("Id,Nome")] Distrito distrito)
        {
            if (id != distrito.Id) return NotFound();

            if (await NomeJaExiste(distrito.Nome, id))
                ModelState.AddModelError("Nome", "Já existe um distrito com este nome.");

            if (!ModelState.IsValid) return View(distrito);

            distrito.Nome = distrito.Nome.Trim();
            _context.Update(distrito);
            await _context.SaveChangesAsync();

            TempData["MensagemSucesso"] = "Distrito atualizado com sucesso.";
            return RedirectToAction(nameof(Index));
        }

        // GET: Distritos/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id is null) return NotFound();

            // Incluímos as Freguesias também para sabermos o impacto total da eliminação
            var distrito = await _context.Distritos
                .Include(d => d.Concelhos)
                    .ThenInclude(c => c.Freguesias)
                .FirstOrDefaultAsync(d => d.Id == id);

            if (distrito is null) return NotFound();

            // Passamos a contagem para a View Delete poder avisar o utilizador
            ViewBag.TotalConcelhos = distrito.Concelhos?.Count ?? 0;
            ViewBag.TotalFreguesias = distrito.Concelhos?.Sum(c => c.Freguesias?.Count ?? 0) ?? 0;

            return View(distrito);
        }

        // POST: Distritos/Delete/5
        [HttpPost, ActionName("Delete"), ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var distrito = await _context.Distritos
                .Include(d => d.Concelhos)
                .FirstOrDefaultAsync(d => d.Id == id);

            if (distrito is null) return RedirectToAction(nameof(Index));

            // Bloqueio de segurança robusto
            if (distrito.Concelhos != null && distrito.Concelhos.Any())
            {
                TempData["MensagemErro"] = "Não é possível apagar este distrito porque tem concelhos associados.";
                return RedirectToAction(nameof(Index));
            }

            _context.Distritos.Remove(distrito);
            await _context.SaveChangesAsync();

            TempData["MensagemSucesso"] = "Distrito apagado com sucesso.";
            return RedirectToAction(nameof(Index));
        }

        // Helper Method
        private Task<bool> NomeJaExiste(string? nome, int? id = null) =>
            _context.Distritos.AnyAsync(d => d.Nome == (nome ?? string.Empty).Trim() && d.Id != id);
    }
}