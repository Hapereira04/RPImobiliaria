using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using RPImobiliaria.Data;
using RPImobiliaria.Models;

namespace RPImobiliaria.Controllers;

[Authorize(Roles = "Admin")]
public class ConcelhosController : Controller
{
    private readonly ApplicationDbContext _context;
    public ConcelhosController(ApplicationDbContext context) => _context = context;

    public async Task<IActionResult> Index() => View(await _context.Concelhos.Include(c => c.Distrito).Include(c => c.Freguesias).OrderBy(c => c.Distrito.Nome).ThenBy(c => c.Nome).ToListAsync());

    public async Task<IActionResult> Details(int? id)
    {
        if (id is null) return NotFound();
        var concelho = await _context.Concelhos.Include(c => c.Distrito).Include(c => c.Freguesias).FirstOrDefaultAsync(c => c.Id == id);
        return concelho is null ? NotFound() : View(concelho);
    }

    public async Task<IActionResult> Create()
    {
        await CarregarDistritos();
        return View();
    }

    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> Create([Bind("Nome,DistritoId")] Concelho concelho)
    {
        if (await NomeJaExiste(concelho.Nome, concelho.DistritoId)) ModelState.AddModelError("Nome", "Já existe um concelho com este nome neste distrito.");
        if (!ModelState.IsValid) { await CarregarDistritos(concelho.DistritoId); return View(concelho); }
        concelho.Nome = concelho.Nome.Trim();
        _context.Concelhos.Add(concelho);
        await _context.SaveChangesAsync();
        TempData["MensagemSucesso"] = "Concelho criado com sucesso.";
        return RedirectToAction(nameof(Index));
    }

    public async Task<IActionResult> Edit(int? id)
    {
        if (id is null) return NotFound();
        var concelho = await _context.Concelhos.FindAsync(id);
        if (concelho is null) return NotFound();
        await CarregarDistritos(concelho.DistritoId);
        return View(concelho);
    }

    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(int id, [Bind("Id,Nome,DistritoId")] Concelho concelho)
    {
        if (id != concelho.Id) return NotFound();
        if (await NomeJaExiste(concelho.Nome, concelho.DistritoId, id)) ModelState.AddModelError("Nome", "Já existe um concelho com este nome neste distrito.");
        if (!ModelState.IsValid) { await CarregarDistritos(concelho.DistritoId); return View(concelho); }
        concelho.Nome = concelho.Nome.Trim();
        _context.Update(concelho);
        await _context.SaveChangesAsync();
        TempData["MensagemSucesso"] = "Concelho atualizado com sucesso.";
        return RedirectToAction(nameof(Index));
    }

    public async Task<IActionResult> Delete(int? id)
    {
        if (id is null) return NotFound();
        var concelho = await _context.Concelhos.Include(c => c.Distrito).Include(c => c.Freguesias).FirstOrDefaultAsync(c => c.Id == id);
        return concelho is null ? NotFound() : View(concelho);
    }

    [HttpPost, ActionName("Delete"), ValidateAntiForgeryToken]
    public async Task<IActionResult> DeleteConfirmed(int id)
    {
        var concelho = await _context.Concelhos.Include(c => c.Freguesias).FirstOrDefaultAsync(c => c.Id == id);
        if (concelho is null) return RedirectToAction(nameof(Index));
        if (concelho.Freguesias.Any()) { TempData["MensagemErro"] = "Não é possível apagar este concelho porque tem freguesias associadas."; return RedirectToAction(nameof(Index)); }
        _context.Concelhos.Remove(concelho);
        await _context.SaveChangesAsync();
        TempData["MensagemSucesso"] = "Concelho apagado com sucesso.";
        return RedirectToAction(nameof(Index));
    }

    private async Task CarregarDistritos(int? selecionado = null) => ViewBag.DistritoId = new SelectList(await _context.Distritos.OrderBy(d => d.Nome).ToListAsync(), "Id", "Nome", selecionado);
    private Task<bool> NomeJaExiste(string? nome, int distritoId, int? id = null) => _context.Concelhos.AnyAsync(c => c.DistritoId == distritoId && c.Nome == (nome ?? string.Empty).Trim() && c.Id != id);
}
