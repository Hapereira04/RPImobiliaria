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
    public class StatusImovelsController : Controller
    {
        private readonly ApplicationDbContext _context;

        public StatusImovelsController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET: StatusImovels
        public async Task<IActionResult> Index(string sortOrder, string searchString)
        {
            ViewData["NameSortParm"] = String.IsNullOrEmpty(sortOrder) ? "name_desc" : "";
            ViewData["CurrentFilter"] = searchString;

            var query = _context.StatusImoveis.AsQueryable();

            // Lógica de pesquisa ignorando maiúsculas/minúsculas
            if (!String.IsNullOrEmpty(searchString))
            {
                var termo = searchString.ToLower();
                query = query.Where(c => c.Nome.ToLower().Contains(termo));
            }

            // Lógica de Ordenação
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

        // GET: StatusImovels/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var statusImovel = await _context.StatusImoveis
                .Include(s => s.Imoveis)
                .FirstOrDefaultAsync(m => m.Id == id);

            if (statusImovel == null)
            {
                return NotFound();
            }

            return View(statusImovel);
        }

        // GET: StatusImovels/Create
        public IActionResult Create()
        {
            return View();
        }

        // POST: StatusImovels/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("Id,Nome,TemEtiqueta,CorHex")] StatusImovel statusImovel)
        {
            // Se o utilizador desmarcar a etiqueta, podemos garantir uma cor predefinida limpa
            if (!statusImovel.TemEtiqueta && string.IsNullOrWhiteSpace(statusImovel.CorHex))
            {
                statusImovel.CorHex = "#fd7e14";
            }

            if (ModelState.IsValid)
            {
                _context.Add(statusImovel);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            return View(statusImovel);
        }

        // GET: StatusImovels/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var statusImovel = await _context.StatusImoveis.FindAsync(id);
            if (statusImovel == null)
            {
                return NotFound();
            }
            return View(statusImovel);
        }

        // POST: StatusImovels/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("Id,Nome,TemEtiqueta,CorHex")] StatusImovel statusImovel)
        {
            if (id != statusImovel.Id)
            {
                return NotFound();
            }

            if (!statusImovel.TemEtiqueta && string.IsNullOrWhiteSpace(statusImovel.CorHex))
            {
                statusImovel.CorHex = "#fd7e14";
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(statusImovel);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!StatusImovelExists(statusImovel.Id))
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
            return View(statusImovel);
        }

        // GET: StatusImovels/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var statusImovel = await _context.StatusImoveis
                .Include(m => m.Imoveis)
                .FirstOrDefaultAsync(m => m.Id == id);

            if (statusImovel == null)
            {
                return NotFound();
            }

            ViewBag.TotalImoveis = statusImovel.Imoveis.Count;

            return View(statusImovel);
        }

        // POST: StatusImovels/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var statusImovel = await _context.StatusImoveis
                .Include(m => m.Imoveis)
                .FirstOrDefaultAsync(m => m.Id == id);

            if (statusImovel != null)
            {
                // Bloqueio de segurança: não permite apagar se tiver imóveis associados
                if (statusImovel.Imoveis != null && statusImovel.Imoveis.Any())
                {
                    TempData["MensagemErro"] = "Não é possível eliminar este estado porque existem imóveis associados a ele.";
                    return RedirectToAction(nameof(Index));
                }

                _context.StatusImoveis.Remove(statusImovel);
                await _context.SaveChangesAsync();
            }

            return RedirectToAction(nameof(Index));
        }

        private bool StatusImovelExists(int id)
        {
            return _context.StatusImoveis.Any(e => e.Id == id);
        }
    }
}