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
    public class CaracteristicasController : Controller
    {
        private readonly ApplicationDbContext _context;

        public CaracteristicasController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET: Caracteristicas
        public async Task<IActionResult> Index()
        {
            var applicationDbContext = _context.CaracteristicasCatalogo.Include(c => c.GrupoCaracteristica);
            return View(await applicationDbContext.ToListAsync());
        }

        // GET: Caracteristicas/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var caracteristica = await _context.CaracteristicasCatalogo
                .Include(c => c.GrupoCaracteristica)
                .FirstOrDefaultAsync(m => m.Id == id);
            if (caracteristica == null)
            {
                return NotFound();
            }

            return View(caracteristica);
        }

        // GET: Caracteristicas/Create
        public IActionResult Create()
        {
            ViewData["GrupoCaracteristicaId"] = new SelectList(_context.GruposCaracteristicas, "Id", "Nome");
            return View();
        }

        // POST: Caracteristicas/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("Id,Nome,GrupoCaracteristicaId")] Caracteristica caracteristica)
        {
            if (ModelState.IsValid)
            {
                _context.Add(caracteristica);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            ViewData["GrupoCaracteristicaId"] = new SelectList(_context.GruposCaracteristicas, "Id", "Nome", caracteristica.GrupoCaracteristicaId);
            return View(caracteristica);
        }

        // GET: Caracteristicas/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var caracteristica = await _context.CaracteristicasCatalogo.FindAsync(id);
            if (caracteristica == null)
            {
                return NotFound();
            }
            ViewData["GrupoCaracteristicaId"] = new SelectList(_context.GruposCaracteristicas, "Id", "Nome", caracteristica.GrupoCaracteristicaId);
            return View(caracteristica);
        }

        // POST: Caracteristicas/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("Id,Nome,GrupoCaracteristicaId")] Caracteristica caracteristica)
        {
            if (id != caracteristica.Id)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(caracteristica);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!CaracteristicaExists(caracteristica.Id))
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
            ViewData["GrupoCaracteristicaId"] = new SelectList(_context.GruposCaracteristicas, "Id", "Nome", caracteristica.GrupoCaracteristicaId);
            return View(caracteristica);
        }

        // GET: Caracteristicas/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            // 1. Usamos o nome correto do DbSet: CaracteristicasCatalogo
            // 2. Incluímos a tabela de ligação: ImoveisCaracteristicas
            var caracteristica = await _context.CaracteristicasCatalogo
                .Include(m => m.ImoveisCaracteristicas)
                .FirstOrDefaultAsync(m => m.Id == id);

            if (caracteristica == null)
            {
                return NotFound();
            }

            // 3. Contamos os registos na tabela de ligação para saber o impacto!
            ViewBag.TotalImoveis = caracteristica.ImoveisCaracteristicas?.Count ?? 0;

            return View(caracteristica);
        }

        // POST: Caracteristicas/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var caracteristica = await _context.CaracteristicasCatalogo.FindAsync(id);
            if (caracteristica != null)
            {
                _context.CaracteristicasCatalogo.Remove(caracteristica);
            }

            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool CaracteristicaExists(int id)
        {
            return _context.CaracteristicasCatalogo.Any(e => e.Id == id);
        }
    }
}
