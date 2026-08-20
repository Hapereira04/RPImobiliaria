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
        public async Task<IActionResult> Index(string sortOrder, string searchString)
        {
            ViewData["NameSortParm"] = String.IsNullOrEmpty(sortOrder) ? "name_desc" : "";
            ViewData["CurrentFilter"] = searchString;

            var query = _context.CaracteristicasCatalogo.Include(c => c.GrupoCaracteristica).AsQueryable();

            // 1. CORREÇÃO DA PESQUISA: Ignorar Maiúsculas/Minúsculas usando ToLower()
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
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("Id,Nome,GrupoCaracteristicaId")] Caracteristica caracteristica)
        {
            ModelState.Remove("GrupoCaracteristica");
            ModelState.Remove("ImoveisCaracteristicas");

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Add(caracteristica);
                    await _context.SaveChangesAsync();
                    return RedirectToAction(nameof(Index));
                }
                catch (Exception ex)
                {
                    // 2. CORREÇÃO CREATE: Se a BD bloquear, mostramos o erro exato no ecrã!
                    var erroReal = ex.InnerException?.Message ?? ex.Message;
                    ModelState.AddModelError("", $"Erro ao gravar na Base de Dados. Verifique se precisa de associar o Grupo de Característica. (Detalhe: {erroReal})");
                }
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
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("Id,Nome,GrupoCaracteristicaId")] Caracteristica caracteristica)
        {
            if (id != caracteristica.Id)
            {
                return NotFound();
            }

            ModelState.Remove("GrupoCaracteristica");
            ModelState.Remove("ImoveisCaracteristicas");

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(caracteristica);
                    await _context.SaveChangesAsync();
                    return RedirectToAction(nameof(Index));
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
                catch (Exception ex)
                {
                    // 3. CORREÇÃO EDIT: Se a BD bloquear, mostramos o erro exato no ecrã!
                    var erroReal = ex.InnerException?.Message ?? ex.Message;
                    ModelState.AddModelError("", $"Erro ao atualizar na Base de Dados. (Detalhe: {erroReal})");
                }
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

            var caracteristica = await _context.CaracteristicasCatalogo
                .Include(m => m.ImoveisCaracteristicas)
                .FirstOrDefaultAsync(m => m.Id == id);

            if (caracteristica == null)
            {
                return NotFound();
            }

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