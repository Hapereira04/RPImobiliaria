using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using RPImobiliaria.Data;
using RPImobiliaria.Models;

namespace RPImobiliaria.Controllers
{
    [Authorize(Roles = "Admin")]
    public class ConcelhosController : Controller
    {
        private readonly ApplicationDbContext _context;

        public ConcelhosController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET: Concelhos
        public async Task<IActionResult> Index(string sortOrder, string searchString)
        {
            ViewData["NameSortParm"] = String.IsNullOrEmpty(sortOrder) ? "name_desc" : "";
            ViewData["CurrentFilter"] = searchString;

            var query = _context.Concelhos
                .Include(c => c.Distrito)
                .Include(c => c.Freguesias)
                .AsQueryable();

            // Pesquisa por nome de concelho ou por nome de distrito
            if (!String.IsNullOrEmpty(searchString))
            {
                var termo = searchString.ToLower();
                query = query.Where(c => c.Nome.ToLower().Contains(termo) ||
                                         (c.Distrito != null && c.Distrito.Nome.ToLower().Contains(termo)));
            }

            // Ordenação
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

        // GET: Concelhos/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null) return NotFound();

            var concelho = await _context.Concelhos
                .Include(c => c.Distrito)
                .Include(c => c.Freguesias)
                .FirstOrDefaultAsync(m => m.Id == id);

            if (concelho == null) return NotFound();

            return View(concelho);
        }

        // GET: Concelhos/Create
        public IActionResult Create()
        {
            ViewData["DistritoId"] = new SelectList(_context.Distritos.OrderBy(d => d.Nome), "Id", "Nome");
            return View();
        }

        // POST: Concelhos/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("Id,Nome,DistritoId")] Concelho concelho)
        {
            // Removemos os objetos navegacionais do ModelState para evitar falhas de validação
            ModelState.Remove("Distrito");
            ModelState.Remove("Freguesias");

            if (await NomeJaExisteNoDistrito(concelho.Nome, concelho.DistritoId))
            {
                ModelState.AddModelError("Nome", "Já existe um concelho com este nome neste distrito.");
            }

            if (ModelState.IsValid)
            {
                concelho.Nome = concelho.Nome.Trim();
                _context.Add(concelho);
                await _context.SaveChangesAsync();
                TempData["MensagemSucesso"] = "Concelho criado com sucesso.";
                return RedirectToAction(nameof(Index));
            }

            ViewData["DistritoId"] = new SelectList(_context.Distritos.OrderBy(d => d.Nome), "Id", "Nome", concelho.DistritoId);
            return View(concelho);
        }

        // GET: Concelhos/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null) return NotFound();

            var concelho = await _context.Concelhos.FindAsync(id);
            if (concelho == null) return NotFound();

            ViewData["DistritoId"] = new SelectList(_context.Distritos.OrderBy(d => d.Nome), "Id", "Nome", concelho.DistritoId);
            return View(concelho);
        }

        // POST: Concelhos/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("Id,Nome,DistritoId")] Concelho concelho)
        {
            if (id != concelho.Id) return NotFound();

            ModelState.Remove("Distrito");
            ModelState.Remove("Freguesias");

            if (await NomeJaExisteNoDistrito(concelho.Nome, concelho.DistritoId, id))
            {
                ModelState.AddModelError("Nome", "Já existe um concelho com este nome neste distrito.");
            }

            if (ModelState.IsValid)
            {
                try
                {
                    concelho.Nome = concelho.Nome.Trim();
                    _context.Update(concelho);
                    await _context.SaveChangesAsync();
                    TempData["MensagemSucesso"] = "Concelho atualizado com sucesso.";
                    return RedirectToAction(nameof(Index));
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!ConcelhoExists(concelho.Id)) return NotFound();
                    else throw;
                }
            }

            ViewData["DistritoId"] = new SelectList(_context.Distritos.OrderBy(d => d.Nome), "Id", "Nome", concelho.DistritoId);
            return View(concelho);
        }

        // GET: Concelhos/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null) return NotFound();

            var concelho = await _context.Concelhos
                .Include(c => c.Distrito)
                .Include(c => c.Freguesias)
                .FirstOrDefaultAsync(m => m.Id == id);

            if (concelho == null) return NotFound();

            ViewBag.TotalFreguesias = concelho.Freguesias?.Count ?? 0;

            return View(concelho);
        }

        // POST: Concelhos/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var concelho = await _context.Concelhos
                .Include(c => c.Freguesias)
                .FirstOrDefaultAsync(c => c.Id == id);

            if (concelho != null)
            {
                if (concelho.Freguesias != null && concelho.Freguesias.Any())
                {
                    TempData["MensagemErro"] = "Não é possível apagar este concelho porque tem freguesias associadas.";
                    return RedirectToAction(nameof(Index));
                }

                _context.Concelhos.Remove(concelho);
                await _context.SaveChangesAsync();
                TempData["MensagemSucesso"] = "Concelho apagado com sucesso.";
            }

            return RedirectToAction(nameof(Index));
        }

        private bool ConcelhoExists(int id) => _context.Concelhos.Any(e => e.Id == id);

        private Task<bool> NomeJaExisteNoDistrito(string? nome, int distritoId, int? id = null) =>
            _context.Concelhos.AnyAsync(c => c.DistritoId == distritoId && c.Nome == (nome ?? string.Empty).Trim() && c.Id != id);
    }
}