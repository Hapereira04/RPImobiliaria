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
        public async Task<IActionResult> Index(string searchString)
        {
            ViewData["CurrentFilter"] = searchString;

            // Vai buscar os clientes com as respetivas Fichas
            var clientes = from c in _context.Clientes.Include(c => c.Ficha)
                           select c;

            // Se escrevermos algo na pesquisa...
            if (!String.IsNullOrEmpty(searchString))
            {
                clientes = clientes.Where(s =>
                    (s.Nome != null && s.Nome.Contains(searchString)) ||
                    (s.Email != null && s.Email.Contains(searchString)) ||
                    (s.NIF != null && s.NIF.Contains(searchString)) ||
                    (s.Telemovel != null && s.Telemovel.Contains(searchString))
                );
            }

            return View(await clientes.ToListAsync());
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

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, Cliente cliente)
        {
            if (id != cliente.Id)
            {
                return NotFound();
            }

            // Ignorar validações de listas relacionadas que não vêm no formulário
            ModelState.Remove("ImoveisPropriedade");
            ModelState.Remove("Favoritos");
            ModelState.Remove("ConsultorResponsavel");

            if (ModelState.IsValid)
            {
                try
                {
                    // 1. Ir buscar o cliente atual e a sua Ficha à base de dados
                    var clienteAtual = await _context.Clientes
                        .Include(c => c.Ficha)
                        .FirstOrDefaultAsync(c => c.Id == id);

                    if (clienteAtual == null)
                    {
                        return NotFound();
                    }

                    // 2. Atualizar apenas os dados do Cliente
                    clienteAtual.Nome = cliente.Nome;
                    clienteAtual.Email = cliente.Email;
                    clienteAtual.Telemovel = cliente.Telemovel;
                    clienteAtual.NIF = cliente.NIF;

                    // 3. Atualizar os dados da Ficha (se existir)
                    if (clienteAtual.Ficha != null && cliente.Ficha != null)
                    {
                        clienteAtual.Ficha.PerfilComprador = cliente.Ficha.PerfilComprador;
                        clienteAtual.Ficha.PerfilVendedor = cliente.Ficha.PerfilVendedor;
                        clienteAtual.Ficha.PerfilArrendatario = cliente.Ficha.PerfilArrendatario;
                        clienteAtual.Ficha.PerfilInvestidor = cliente.Ficha.PerfilInvestidor;

                        clienteAtual.Ficha.OrcamentoMaximo = cliente.Ficha.OrcamentoMaximo;
                        clienteAtual.Ficha.ZonasPreferencia = cliente.Ficha.ZonasPreferencia;
                        clienteAtual.Ficha.TipologiasProcuradas = cliente.Ficha.TipologiasProcuradas;
                        clienteAtual.Ficha.NotasRequisitos = cliente.Ficha.NotasRequisitos;

                        // Atualizamos a data para o momento exato em que foi editado
                        clienteAtual.Ficha.UltimaAtualizacao = DateTime.Now;
                    }

                    // 4. Guardar as alterações. O EF Core sabe perfeitamente o que mudou!
                    await _context.SaveChangesAsync();

                    TempData["MensagemSucesso"] = "Dados do cliente atualizados com sucesso!";
                    return RedirectToAction(nameof(Index));
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!ClienteExists(cliente.Id))
                    {
                        return NotFound();
                    }
                    else
                    {
                        throw;
                    }
                }
            }

            TempData["MensagemErro"] = "Verifique os erros no formulário antes de submeter.";
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
