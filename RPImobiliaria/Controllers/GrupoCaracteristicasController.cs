using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using RPImobiliaria.Data;
using RPImobiliaria.Models;

namespace RPImobiliaria.Controllers
{
    public class GrupoCaracteristicasController : Controller
    {
        private readonly ApplicationDbContext _context;

        public GrupoCaracteristicasController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET: GrupoCaracteristicas
        public async Task<IActionResult> Index()
        {
            return View(await _context.GruposCaracteristicas.ToListAsync());
        }

        // GET: GrupoCaracteristicas/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var grupoCaracteristica = await _context.GruposCaracteristicas
                .FirstOrDefaultAsync(m => m.Id == id);
            if (grupoCaracteristica == null)
            {
                return NotFound();
            }

            return View(grupoCaracteristica);
        }

        // GET: GrupoCaracteristicas/Create
        public IActionResult Create()
        {
            return View();
        }

        // POST: GrupoCaracteristicas/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("Id,Nome")] GrupoCaracteristica grupoCaracteristica)
        {
            if (ModelState.IsValid)
            {
                _context.Add(grupoCaracteristica);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            return View(grupoCaracteristica);
        }

        // GET: GrupoCaracteristicas/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var grupoCaracteristica = await _context.GruposCaracteristicas.FindAsync(id);
            if (grupoCaracteristica == null)
            {
                return NotFound();
            }
            return View(grupoCaracteristica);
        }

        // POST: GrupoCaracteristicas/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("Id,Nome")] GrupoCaracteristica grupoCaracteristica)
        {
            if (id != grupoCaracteristica.Id)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(grupoCaracteristica);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!GrupoCaracteristicaExists(grupoCaracteristica.Id))
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
            return View(grupoCaracteristica);
        }

        // GET: GruposCaracteristicas/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var grupoCaracteristica = await _context.GruposCaracteristicas
                .Include(m => m.Caracteristicas) // Traz as características que pertencem a este grupo
                .FirstOrDefaultAsync(m => m.Id == id);

            if (grupoCaracteristica == null)
            {
                return NotFound();
            }

            // Passa para a vista quantas características estão dentro deste grupo
            ViewBag.TotalCaracteristicas = grupoCaracteristica.Caracteristicas?.Count ?? 0;

            return View(grupoCaracteristica);
        }

        // POST: GrupoCaracteristicas/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var grupoCaracteristica = await _context.GruposCaracteristicas.FindAsync(id);
            if (grupoCaracteristica != null)
            {
                _context.GruposCaracteristicas.Remove(grupoCaracteristica);
            }

            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool GrupoCaracteristicaExists(int id)
        {
            return _context.GruposCaracteristicas.Any(e => e.Id == id);
        }
    }
}
