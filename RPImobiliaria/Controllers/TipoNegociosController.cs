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
        // ATUALIZADO: Recebe parâmetros para pesquisa e ordenação
        public async Task<IActionResult> Index(string sortOrder, string searchString)
        {
            // Configura os ViewData para a View manter o estado dos botões
            ViewData["NameSortParm"] = String.IsNullOrEmpty(sortOrder) ? "name_desc" : "";
            ViewData["CurrentFilter"] = searchString;

            var tipos = from t in _context.TiposNegocio
                        select t;

            // Filtro de Pesquisa
            if (!String.IsNullOrEmpty(searchString))
            {
                tipos = tipos.Where(s => s.Nome.Contains(searchString));
            }

            // Ordenação
            switch (sortOrder)
            {
                case "name_desc":
                    tipos = tipos.OrderByDescending(s => s.Nome);
                    break;
                default: // Crescente por defeito
                    tipos = tipos.OrderBy(s => s.Nome);
                    break;
            }

            return View(await tipos.ToListAsync());
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

            // Carrega a lista de Imóveis associados
            var tipoNegocio = await _context.TiposNegocio
                .Include(m => m.Imoveis)
                .FirstOrDefaultAsync(m => m.Id == id);

            if (tipoNegocio == null)
            {
                return NotFound();
            }

            // Conta os imóveis dentro da coleção para a View saber se deve bloquear
            ViewBag.TotalImoveis = tipoNegocio.Imoveis?.Count ?? 0;

            return View(tipoNegocio);
        }

        // POST: TipoNegocios/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            // ATUALIZADO: Segurança do lado do servidor para impedir apagar com imóveis associados
            var tipoNegocio = await _context.TiposNegocio
                .Include(t => t.Imoveis)
                .FirstOrDefaultAsync(m => m.Id == id);

            if (tipoNegocio != null)
            {
                // Se alguém tentar forçar a submissão, o servidor bloqueia aqui:
                if (tipoNegocio.Imoveis != null && tipoNegocio.Imoveis.Count > 0)
                {
                    TempData["Erro"] = "Segurança ativada: Não é possível eliminar este Tipo de Negócio porque está associado a imóveis.";
                    return RedirectToAction(nameof(Delete), new { id = id });
                }

                _context.TiposNegocio.Remove(tipoNegocio);
                await _context.SaveChangesAsync();
            }

            return RedirectToAction(nameof(Index));
        }

        private bool TipoNegocioExists(int id)
        {
            return _context.TiposNegocio.Any(e => e.Id == id);
        }
    }
}