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
    public class CategoriaImovelsController : Controller
    {
        private readonly ApplicationDbContext _context;

        public CategoriaImovelsController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET: CategoriaImovels
        public async Task<IActionResult> Index(string sortOrder, string searchString)
        {
            ViewData["NameSortParm"] = String.IsNullOrEmpty(sortOrder) ? "name_desc" : "";
            ViewData["CurrentFilter"] = searchString;

            // Nota: Confirme se o seu DbSet se chama CategoriaImovels ou Categorias
            var query = _context.CategoriasImovel.AsQueryable();

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

        // GET: CategoriaImovels/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var categoriaImovel = await _context.CategoriasImovel
                .FirstOrDefaultAsync(m => m.Id == id);
            if (categoriaImovel == null)
            {
                return NotFound();
            }

            return View(categoriaImovel);
        }

        // GET: CategoriaImovels/Create
        public IActionResult Create()
        {
            return View();
        }

        // POST: CategoriaImovels/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("Id,Nome")] CategoriaImovel categoriaImovel)
        {
            if (ModelState.IsValid)
            {
                _context.Add(categoriaImovel);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            return View(categoriaImovel);
        }

        // GET: CategoriaImovels/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var categoriaImovel = await _context.CategoriasImovel.FindAsync(id);
            if (categoriaImovel == null)
            {
                return NotFound();
            }
            return View(categoriaImovel);
        }

        // POST: CategoriaImovels/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("Id,Nome")] CategoriaImovel categoriaImovel)
        {
            if (id != categoriaImovel.Id)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(categoriaImovel);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!CategoriaImovelExists(categoriaImovel.Id))
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
            return View(categoriaImovel);
        }

        // GET: CategoriaImovels/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            // Usamos o .Include(m => m.Imoveis) para ele trazer a "mochila" de imóveis junto com a categoria
            var categoriaImovel = await _context.CategoriasImovel
                .Include(m => m.Imoveis)
                .FirstOrDefaultAsync(m => m.Id == id);

            if (categoriaImovel == null)
            {
                return NotFound();
            }

            // Agora é só contar os imóveis que vieram dentro dela! Sem erros de nomes!
            ViewBag.TotalImoveis = categoriaImovel.Imoveis.Count;

            return View(categoriaImovel);
        }

        // POST: CategoriaImovels/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var categoriaImovel = await _context.CategoriasImovel.FindAsync(id);
            if (categoriaImovel != null)
            {
                _context.CategoriasImovel.Remove(categoriaImovel);
            }

            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool CategoriaImovelExists(int id)
        {
            return _context.CategoriasImovel.Any(e => e.Id == id);
        }
    }
}
