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
    public class EstadoImovelsController : Controller
    {
        private readonly ApplicationDbContext _context;

        public EstadoImovelsController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET: EstadoImovels
        public async Task<IActionResult> Index(string sortOrder, string searchString)
        {
            ViewData["NameSortParm"] = String.IsNullOrEmpty(sortOrder) ? "name_desc" : "";
            ViewData["CurrentFilter"] = searchString;

            var query = _context.EstadosImovel.AsQueryable();

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

        // GET: EstadoImovels/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var estadoImovel = await _context.EstadosImovel
                .FirstOrDefaultAsync(m => m.Id == id);
            if (estadoImovel == null)
            {
                return NotFound();
            }

            return View(estadoImovel);
        }

        // GET: EstadoImovels/Create
        public IActionResult Create()
        {
            return View();
        }

        // POST: EstadoImovels/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("Id,Nome")] EstadoImovel estadoImovel)
        {
            if (ModelState.IsValid)
            {
                _context.Add(estadoImovel);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            return View(estadoImovel);
        }

        // GET: EstadoImovels/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var estadoImovel = await _context.EstadosImovel.FindAsync(id);
            if (estadoImovel == null)
            {
                return NotFound();
            }
            return View(estadoImovel);
        }

        // POST: EstadoImovels/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("Id,Nome")] EstadoImovel estadoImovel)
        {
            if (id != estadoImovel.Id)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(estadoImovel);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!EstadoImovelExists(estadoImovel.Id))
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
            return View(estadoImovel);
        }

        // GET: EstadoImovels/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var estadoImovel = await _context.EstadosImovel
                .Include(m => m.Imoveis)
                .FirstOrDefaultAsync(m => m.Id == id);

            if (estadoImovel == null)
            {
                return NotFound();
            }

            ViewBag.TotalImoveis = estadoImovel.Imoveis.Count;

            return View(estadoImovel);
        }

        // POST: EstadoImovels/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var estadoImovel = await _context.EstadosImovel.FindAsync(id);
            if (estadoImovel == null) return RedirectToAction(nameof(Index));
            if (await _context.Imoveis.AnyAsync(i => i.EstadoImovelId == id))
            {
                TempData["MensagemErro"] = "Não é possível apagar um estado que está a ser utilizado por imóveis.";
                return RedirectToAction(nameof(Index));
            }
            _context.EstadosImovel.Remove(estadoImovel);
            await _context.SaveChangesAsync();
            TempData["MensagemSucesso"] = "Estado apagado com sucesso.";
            return RedirectToAction(nameof(Index));
        }

        private bool EstadoImovelExists(int id)
        {
            return _context.EstadosImovel.Any(e => e.Id == id);
        }
    }
}
