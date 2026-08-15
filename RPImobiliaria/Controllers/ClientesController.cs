using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using Microsoft.EntityFrameworkCore;
using RPImobiliaria.Data;
using RPImobiliaria.Models;

namespace RPImobiliaria.Controllers
{
    [Authorize(Roles = "Admin,Consultor")]
    public class ClientesController : Controller
    {
        private readonly ApplicationDbContext _context;

        public ClientesController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET: Clientes
        public async Task<IActionResult> Index()
        {
            // Select que puxa os clientes e a respetiva Ficha para sabermos os perfis
            var clientes = await _context.Clientes.Include(c => c.Ficha).ToListAsync();
            return View(clientes);
        }

        // GET: Clientes/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null) return NotFound();

            var cliente = await _context.Clientes
                .Include(c => c.Ficha)
                .Include(c => c.Favoritos).ThenInclude(f => f.Imovel)
                .Include(c => c.ImoveisPropriedade).ThenInclude(p => p.Imovel)
                .FirstOrDefaultAsync(m => m.Id == id);

            if (cliente == null) return NotFound();

            return View(cliente);
        }

        // GET: Clientes/Create
        public IActionResult Create()
        {
            return View();
        }

        // POST: Clientes/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("Id,Nome,Email,Telemovel,NIF,Ficha")] Cliente cliente)
        {
            if (ModelState.IsValid)
            {
                // 1. AUTOPREENCHIMENTO IDENTITY: Se houver email, tenta procurar a conta correspondente
                if (!string.IsNullOrEmpty(cliente.Email))
                {
                    var usuarioIdentity = await _context.Users
                        .FirstOrDefaultAsync(u => u.Email == cliente.Email);

                    if (usuarioIdentity != null)
                    {
                        cliente.ApplicationUserId = usuarioIdentity.Id;
                    }
                }

                // 2. CONFIGURAÇÃO DA FICHA TÉCNICA
                if (cliente.Ficha != null)
                {
                    cliente.Ficha.UltimaAtualizacao = DateTime.Now;

                    // DETEÇÃO DE PERFIL: Se quem está a criar é um Consultor (Backoffice)
                    if (User.Identity?.IsAuthenticated == true && !User.IsInRole("Cliente"))
                    {
                        cliente.Ficha.PreenchidoPeloCliente = false; // Foi o consultor no CRM
                    }
                    else
                    {
                        cliente.Ficha.PreenchidoPeloCliente = true; // Foi criado via portal público pelo próprio
                    }
                }

                _context.Add(cliente);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            return View(cliente);
        }

        // GET: Clientes/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null) return NotFound();

            // Carrega o cliente com a ficha dele para podermos editar tudo no mesmo ecrã
            var cliente = await _context.Clientes.Include(c => c.Ficha).FirstOrDefaultAsync(c => c.Id == id);

            if (cliente == null) return NotFound();

            return View(cliente);
        }

        // POST: Clientes/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("Id,Nome,Email,Telemovel,NIF,ApplicationUserId,Ficha")] Cliente cliente)
        {
            if (id != cliente.Id) return NotFound();

            if (ModelState.IsValid)
            {
                try
                {
                    // Buscamos a ficha antiga ou criamos uma se não existir para evitar erros de tracking
                    var fichaExistente = await _context.FichasClientes.FirstOrDefaultAsync(f => f.ClienteId == id);

                    if (cliente.Ficha != null)
                    {
                        if (fichaExistente != null)
                        {
                            // Atualiza os campos da ficha existente
                            _context.Entry(fichaExistente).CurrentValues.SetValues(cliente.Ficha);
                            fichaExistente.UltimaAtualizacao = DateTime.Now;
                        }
                        else
                        {
                            // Se o cliente não tinha ficha (antigo), cria uma nova agora
                            cliente.Ficha.ClienteId = id;
                            cliente.Ficha.UltimaAtualizacao = DateTime.Now;
                            _context.Add(cliente.Ficha);
                        }
                    }

                    _context.Update(cliente);
                    // Ignora a propriedade de navegação direta para não duplicar o tracking
                    _context.Entry(cliente).Reference(c => c.Ficha).IsModified = false;

                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!ClienteExists(cliente.Id)) return NotFound();
                    else throw;
                }
                return RedirectToAction(nameof(Index));
            }
            return View(cliente);
        }

        // GET: Clientes/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null) return NotFound();

            var cliente = await _context.Clientes
                .Include(c => c.Ficha)
                .FirstOrDefaultAsync(m => m.Id == id);

            if (cliente == null) return NotFound();

            return View(cliente);
        }

        // POST: Clientes/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var cliente = await _context.Clientes.FindAsync(id);
            if (cliente != null)
            {
                _context.Clientes.Remove(cliente);
            }

            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool ClienteExists(int id)
        {
            return _context.Clientes.Any(e => e.Id == id);
        }
    }
}
