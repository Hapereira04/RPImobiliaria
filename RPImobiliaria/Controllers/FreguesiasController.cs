using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using RPImobiliaria.Data;
using RPImobiliaria.Models;

namespace RPImobiliaria.Controllers;

[Authorize(Roles = "Admin")]
public class FreguesiasController : Controller
{
    private readonly ApplicationDbContext _context;
    public FreguesiasController(ApplicationDbContext context) => _context = context;

    public async Task<IActionResult> Index() => View(await _context.Freguesias.Include(f => f.Concelho).ThenInclude(c => c.Distrito).OrderBy(f => f.Concelho.Distrito.Nome).ThenBy(f => f.Concelho.Nome).ThenBy(f => f.Nome).ToListAsync());

    public async Task<IActionResult> Details(int? id)
    {
        if (id is null) return NotFound();
        var freguesia = await _context.Freguesias.Include(f => f.Concelho).ThenInclude(c => c.Distrito).FirstOrDefaultAsync(f => f.Id == id);
        return freguesia is null ? NotFound() : View(freguesia);
    }

    public async Task<IActionResult> Create()
    {
        await CarregarConcelhos();
        return View();
    }

    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> Create([Bind("Nome,ConcelhoId")] Freguesia freguesia)
    {
        if (await NomeJaExiste(freguesia.Nome, freguesia.ConcelhoId)) ModelState.AddModelError("Nome", "Já existe uma freguesia com este nome neste concelho.");
        if (!ModelState.IsValid) { await CarregarConcelhos(freguesia.ConcelhoId); return View(freguesia); }
        freguesia.Nome = freguesia.Nome.Trim();
        _context.Freguesias.Add(freguesia);
        await _context.SaveChangesAsync();
        TempData["MensagemSucesso"] = "Freguesia criada com sucesso.";
        return RedirectToAction(nameof(Index));
    }

    public async Task<IActionResult> Edit(int? id)
    {
        if (id is null) return NotFound();
        var freguesia = await _context.Freguesias.FindAsync(id);
        if (freguesia is null) return NotFound();
        await CarregarConcelhos(freguesia.ConcelhoId);
        return View(freguesia);
    }

    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(int id, [Bind("Id,Nome,ConcelhoId")] Freguesia freguesia)
    {
        if (id != freguesia.Id) return NotFound();
        if (await NomeJaExiste(freguesia.Nome, freguesia.ConcelhoId, id)) ModelState.AddModelError("Nome", "Já existe uma freguesia com este nome neste concelho.");
        if (!ModelState.IsValid) { await CarregarConcelhos(freguesia.ConcelhoId); return View(freguesia); }
        freguesia.Nome = freguesia.Nome.Trim();
        _context.Update(freguesia);
        await _context.SaveChangesAsync();
        TempData["MensagemSucesso"] = "Freguesia atualizada com sucesso.";
        return RedirectToAction(nameof(Index));
    }

    public async Task<IActionResult> Delete(int? id)
    {
        if (id is null) return NotFound();
        var freguesia = await _context.Freguesias.Include(f => f.Concelho).ThenInclude(c => c.Distrito).FirstOrDefaultAsync(f => f.Id == id);
        return freguesia is null ? NotFound() : View(freguesia);
    }

    [HttpPost, ActionName("Delete"), ValidateAntiForgeryToken]
    public async Task<IActionResult> DeleteConfirmed(int id)
    {
        var freguesia = await _context.Freguesias.FindAsync(id);
        if (freguesia is null) return RedirectToAction(nameof(Index));
        if (await _context.Imoveis.AnyAsync(i => i.FreguesiaId == id)) { TempData["MensagemErro"] = "Não é possível apagar esta freguesia porque existem imóveis associados."; return RedirectToAction(nameof(Index)); }
        _context.Freguesias.Remove(freguesia);
        await _context.SaveChangesAsync();
        TempData["MensagemSucesso"] = "Freguesia apagada com sucesso.";
        return RedirectToAction(nameof(Index));
    }

    private async Task CarregarConcelhos(int? selecionado = null)
    {
        var concelhos = await _context.Concelhos.Include(c => c.Distrito).OrderBy(c => c.Distrito.Nome).ThenBy(c => c.Nome)
            .Select(c => new { c.Id, NomeCompleto = c.Distrito.Nome + " — " + c.Nome }).ToListAsync();
        ViewBag.ConcelhoId = new SelectList(concelhos, "Id", "NomeCompleto", selecionado);
    }
    private Task<bool> NomeJaExiste(string? nome, int concelhoId, int? id = null) => _context.Freguesias.AnyAsync(f => f.ConcelhoId == concelhoId && f.Nome == (nome ?? string.Empty).Trim() && f.Id != id);
}
