using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using RPImobiliaria.Data;
using RPImobiliaria.Models;

namespace RPImobiliaria.Controllers;

[Authorize(Roles = "Admin")]
public class DistritosController : Controller
{
    private readonly ApplicationDbContext _context;
    public DistritosController(ApplicationDbContext context) => _context = context;

    public async Task<IActionResult> Index() => View(await _context.Distritos.Include(d => d.Concelhos).OrderBy(d => d.Nome).ToListAsync());

    public async Task<IActionResult> Details(int? id)
    {
        if (id is null) return NotFound();
        var distrito = await _context.Distritos.Include(d => d.Concelhos).ThenInclude(c => c.Freguesias).FirstOrDefaultAsync(d => d.Id == id);
        return distrito is null ? NotFound() : View(distrito);
    }

    public IActionResult Create() => View();

    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> Create([Bind("Nome")] Distrito distrito)
    {
        if (await NomeJaExiste(distrito.Nome)) ModelState.AddModelError("Nome", "Já existe um distrito com este nome.");
        if (!ModelState.IsValid) return View(distrito);
        distrito.Nome = distrito.Nome.Trim();
        _context.Distritos.Add(distrito);
        await _context.SaveChangesAsync();
        TempData["MensagemSucesso"] = "Distrito criado com sucesso.";
        return RedirectToAction(nameof(Index));
    }

    public async Task<IActionResult> Edit(int? id)
    {
        if (id is null) return NotFound();
        var distrito = await _context.Distritos.FindAsync(id);
        return distrito is null ? NotFound() : View(distrito);
    }

    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(int id, [Bind("Id,Nome")] Distrito distrito)
    {
        if (id != distrito.Id) return NotFound();
        if (await NomeJaExiste(distrito.Nome, id)) ModelState.AddModelError("Nome", "Já existe um distrito com este nome.");
        if (!ModelState.IsValid) return View(distrito);
        distrito.Nome = distrito.Nome.Trim();
        _context.Update(distrito);
        await _context.SaveChangesAsync();
        TempData["MensagemSucesso"] = "Distrito atualizado com sucesso.";
        return RedirectToAction(nameof(Index));
    }

    public async Task<IActionResult> Delete(int? id)
    {
        if (id is null) return NotFound();
        var distrito = await _context.Distritos.Include(d => d.Concelhos).FirstOrDefaultAsync(d => d.Id == id);
        return distrito is null ? NotFound() : View(distrito);
    }

    [HttpPost, ActionName("Delete"), ValidateAntiForgeryToken]
    public async Task<IActionResult> DeleteConfirmed(int id)
    {
        var distrito = await _context.Distritos.Include(d => d.Concelhos).FirstOrDefaultAsync(d => d.Id == id);
        if (distrito is null) return RedirectToAction(nameof(Index));
        if (distrito.Concelhos.Any())
        {
            TempData["MensagemErro"] = "Não é possível apagar este distrito porque tem concelhos associados.";
            return RedirectToAction(nameof(Index));
        }
        _context.Distritos.Remove(distrito);
        await _context.SaveChangesAsync();
        TempData["MensagemSucesso"] = "Distrito apagado com sucesso.";
        return RedirectToAction(nameof(Index));
    }

    private Task<bool> NomeJaExiste(string? nome, int? id = null) => _context.Distritos.AnyAsync(d => d.Nome == (nome ?? string.Empty).Trim() && d.Id != id);
}
