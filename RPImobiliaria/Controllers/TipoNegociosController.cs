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
    public class TipoNegociosController : Controller
    {
        private readonly ApplicationDbContext _context;

        public TipoNegociosController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET: TipoNegocios
        public async Task<IActionResult> Index()
        {
            return View(await _context.TiposNegocio.ToListAsync());
        }

        // GET: TipoNegocios/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var tipoNegocio = await _context.TiposNegocio
                .FirstOrDefaultAsync(m => m.Id == id);
            if (tipoNegocio == null)
            {
                return NotFound();
            }

            return View(tipoNegocio);
        }

        // GET: TipoNegocios/Create
        public IActionResult Create()
        {
            return View();
        }

        // POST: TipoNegocios/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("Id,Nome")] TipoNegocio tipoNegocio)
        {
            if (ModelState.IsValid)
            {
                _context.Add(tipoNegocio);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            return View(tipoNegocio);
        }

        // GET: TipoNegocios/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var tipoNegocio = await _context.TiposNegocio.FindAsync(id);
            if (tipoNegocio == null)
            {
                return NotFound();
            }
            return View(tipoNegocio);
        }

        // POST: TipoNegocios/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("Id,Nome")] TipoNegocio tipoNegocio)
        {
            if (id != tipoNegocio.Id)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(tipoNegocio);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!TipoNegocioExists(tipoNegocio.Id))
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
            return View(tipoNegocio);
        }

        // GET: TipoNegocios/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            // Usamos o Include para carregar a lista de Imóveis associados
            var tipoNegocio = await _context.TiposNegocio // Confirme se não é outra grafia no DbContext
            .Include(m => m.Imoveis)
            .FirstOrDefaultAsync(m => m.Id == id);

            if (tipoNegocio == null)
            {
                return NotFound();
            }

            // Contamos os imóveis dentro da coleção
            ViewBag.TotalImoveis = tipoNegocio.Imoveis.Count;

            return View(tipoNegocio);
        }

        // POST: TipoNegocios/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var tipoNegocio = await _context.TiposNegocio.FindAsync(id);
            if (tipoNegocio != null)
            {
                _context.TiposNegocio.Remove(tipoNegocio);
            }

            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool TipoNegocioExists(int id)
        {
            return _context.TiposNegocio.Any(e => e.Id == id);
        }
    }
}
