using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using RPImobiliaria.Data;
using RPImobiliaria.Models;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace RPImobiliaria.Controllers
{
    [Authorize(Roles = "Admin")]
    public class FreguesiasController : Controller
    {
        private readonly ApplicationDbContext _context;
        public FreguesiasController(ApplicationDbContext context) => _context = context;

        // GET: Freguesias
        public async Task<IActionResult> Index(string sortOrder, string searchString)
        {
            ViewData["NameSortParm"] = String.IsNullOrEmpty(sortOrder) ? "name_desc" : "";
            ViewData["CurrentFilter"] = searchString;

            var query = _context.Freguesias
                .Include(f => f.Concelho)
                    .ThenInclude(c => c.Distrito)
                .AsQueryable();

            // 1. Pesquisa Inteligente: Procura por nome da Freguesia, Concelho ou Distrito
            if (!String.IsNullOrEmpty(searchString))
            {
                var termo = searchString.ToLower();
                query = query.Where(f =>
                    f.Nome.ToLower().Contains(termo) ||
                    (f.Concelho != null && f.Concelho.Nome.ToLower().Contains(termo)) ||
                    (f.Concelho != null && f.Concelho.Distrito != null && f.Concelho.Distrito.Nome.ToLower().Contains(termo))
                );
            }

            // 2. Ordenação
            switch (sortOrder)
            {
                case "name_desc":
                    query = query.OrderByDescending(f => f.Nome);
                    break;
                default:
                    // Por defeito, mantém a excelente organização hierárquica que já tinha
                    query = query.OrderBy(f => f.Concelho.Distrito.Nome)
                                 .ThenBy(f => f.Concelho.Nome)
                                 .ThenBy(f => f.Nome);
                    break;
            }

            return View(await query.ToListAsync());
        }

        // GET: Freguesias/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id is null) return NotFound();

            var freguesia = await _context.Freguesias
                .Include(f => f.Concelho)
                    .ThenInclude(c => c.Distrito)
                .FirstOrDefaultAsync(f => f.Id == id);

            return freguesia is null ? NotFound() : View(freguesia);
        }

        // GET: Freguesias/Create
        public async Task<IActionResult> Create()
        {
            await CarregarConcelhos();
            return View();
        }

        // POST: Freguesias/Create
        [HttpPost, ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("Id,Nome,ConcelhoId")] Freguesia freguesia)
        {
            // Evita que o EF Core bloqueie a gravação por falta do objeto "Concelho" preenchido
            ModelState.Remove("Concelho");
            ModelState.Remove("Imoveis"); // Caso a sua Freguesia tenha lista de Imoveis

            if (await NomeJaExiste(freguesia.Nome, freguesia.ConcelhoId))
                ModelState.AddModelError("Nome", "Já existe uma freguesia com este nome neste concelho.");

            if (!ModelState.IsValid)
            {
                await CarregarConcelhos(freguesia.ConcelhoId);
                return View(freguesia);
            }

            freguesia.Nome = freguesia.Nome.Trim();
            _context.Freguesias.Add(freguesia);
            await _context.SaveChangesAsync();

            TempData["MensagemSucesso"] = "Freguesia criada com sucesso.";
            return RedirectToAction(nameof(Index));
        }

        // GET: Freguesias/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id is null) return NotFound();

            var freguesia = await _context.Freguesias.FindAsync(id);
            if (freguesia is null) return NotFound();

            await CarregarConcelhos(freguesia.ConcelhoId);
            return View(freguesia);
        }

        // POST: Freguesias/Edit/5
        [HttpPost, ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("Id,Nome,ConcelhoId")] Freguesia freguesia)
        {
            if (id != freguesia.Id) return NotFound();

            ModelState.Remove("Concelho");
            ModelState.Remove("Imoveis");

            if (await NomeJaExiste(freguesia.Nome, freguesia.ConcelhoId, id))
                ModelState.AddModelError("Nome", "Já existe uma freguesia com este nome neste concelho.");

            if (!ModelState.IsValid)
            {
                await CarregarConcelhos(freguesia.ConcelhoId);
                return View(freguesia);
            }

            try
            {
                freguesia.Nome = freguesia.Nome.Trim();
                _context.Update(freguesia);
                await _context.SaveChangesAsync();
                TempData["MensagemSucesso"] = "Freguesia atualizada com sucesso.";
                return RedirectToAction(nameof(Index));
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!FreguesiaExists(freguesia.Id)) return NotFound();
                else throw;
            }
        }

        // GET: Freguesias/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id is null) return NotFound();

            var freguesia = await _context.Freguesias
                .Include(f => f.Concelho)
                    .ThenInclude(c => c.Distrito)
                .FirstOrDefaultAsync(f => f.Id == id);

            if (freguesia is null) return NotFound();

            // Verificamos quantos imóveis estão associados para podermos avisar na View!
            ViewBag.TotalImoveis = await _context.Imoveis.CountAsync(i => i.FreguesiaId == id);

            return View(freguesia);
        }

        // POST: Freguesias/Delete/5
        [HttpPost, ActionName("Delete"), ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var freguesia = await _context.Freguesias.FindAsync(id);
            if (freguesia is null) return RedirectToAction(nameof(Index));

            // Bloqueio de Segurança para não deixar base de dados corrompida
            if (await _context.Imoveis.AnyAsync(i => i.FreguesiaId == id))
            {
                TempData["MensagemErro"] = "Não é possível apagar esta freguesia porque existem imóveis associados.";
                return RedirectToAction(nameof(Index));
            }

            _context.Freguesias.Remove(freguesia);
            await _context.SaveChangesAsync();

            TempData["MensagemSucesso"] = "Freguesia apagada com sucesso.";
            return RedirectToAction(nameof(Index));
        }

        // Helpers
        private bool FreguesiaExists(int id) => _context.Freguesias.Any(e => e.Id == id);

        private async Task CarregarConcelhos(int? selecionado = null)
        {
            var concelhos = await _context.Concelhos
                .Include(c => c.Distrito)
                .OrderBy(c => c.Distrito.Nome)
                .ThenBy(c => c.Nome)
                .Select(c => new { c.Id, NomeCompleto = c.Distrito.Nome + " — " + c.Nome })
                .ToListAsync();

            ViewBag.ConcelhoId = new SelectList(concelhos, "Id", "NomeCompleto", selecionado);
        }

        private Task<bool> NomeJaExiste(string? nome, int concelhoId, int? id = null) =>
            _context.Freguesias.AnyAsync(f => f.ConcelhoId == concelhoId && f.Nome == (nome ?? string.Empty).Trim() && f.Id != id);
    }
}