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
    public class CertificadoEnergeticoesController : Controller
    {
        private readonly ApplicationDbContext _context;

        public CertificadoEnergeticoesController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET: CertificadoEnergeticoes
        public async Task<IActionResult> Index()
        {
            return View(await _context.CertificadosEnergeticos.ToListAsync());
        }

        // GET: CertificadoEnergeticoes/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var certificadoEnergetico = await _context.CertificadosEnergeticos
                .FirstOrDefaultAsync(m => m.Id == id);
            if (certificadoEnergetico == null)
            {
                return NotFound();
            }

            return View(certificadoEnergetico);
        }

        // GET: CertificadoEnergeticoes/Create
        public IActionResult Create()
        {
            return View();
        }

        // POST: CertificadoEnergeticoes/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("Id,Nome")] CertificadoEnergetico certificadoEnergetico)
        {
            if (ModelState.IsValid)
            {
                _context.Add(certificadoEnergetico);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            return View(certificadoEnergetico);
        }

        // GET: CertificadoEnergeticoes/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var certificadoEnergetico = await _context.CertificadosEnergeticos.FindAsync(id);
            if (certificadoEnergetico == null)
            {
                return NotFound();
            }
            return View(certificadoEnergetico);
        }

        // POST: CertificadoEnergeticoes/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("Id,Nome")] CertificadoEnergetico certificadoEnergetico)
        {
            if (id != certificadoEnergetico.Id)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(certificadoEnergetico);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!CertificadoEnergeticoExists(certificadoEnergetico.Id))
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
            return View(certificadoEnergetico);
        }

        // GET: CertificadoEnergeticoes/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var certificadoEnergetico = await _context.CertificadosEnergeticos
                .Include(m => m.Imoveis)
                .FirstOrDefaultAsync(m => m.Id == id);

            if (certificadoEnergetico == null)
            {
                return NotFound();
            }

            ViewBag.TotalImoveis = certificadoEnergetico.Imoveis.Count;

            return View(certificadoEnergetico);
        }

        // POST: CertificadoEnergeticoes/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var certificadoEnergetico = await _context.CertificadosEnergeticos.FindAsync(id);
            if (certificadoEnergetico != null)
            {
                _context.CertificadosEnergeticos.Remove(certificadoEnergetico);
            }

            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool CertificadoEnergeticoExists(int id)
        {
            return _context.CertificadosEnergeticos.Any(e => e.Id == id);
        }
    }
}
