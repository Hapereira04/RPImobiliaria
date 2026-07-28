using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using RPImobiliaria.Data;
using RPImobiliaria.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Hosting; // Adicionado para gerir pastas
using System.IO; // Adicionado para manipulação de ficheiros

namespace RPImobiliaria.Controllers
{
    [Authorize]
    public class ImovelsController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<IdentityUser> _userManager;
        // 1. Variável para aceder às pastas do teu servidor
        private readonly IWebHostEnvironment _hostEnvironment;

        // 2. Construtor atualizado
        public ImovelsController(ApplicationDbContext context, UserManager<IdentityUser> userManager, IWebHostEnvironment hostEnvironment)
        {
            _context = context;
            _userManager = userManager;
            _hostEnvironment = hostEnvironment;
        }

        // GET: Imoveis
        public async Task<IActionResult> Index()
        {
            var applicationDbContext = _context.Imoveis
                .Include(i => i.CategoriaImovel)
                .Include(i => i.EstadoImovel)
                .Include(i => i.TipoNegocio)
                .Include(i => i.Fotos);

            return View(await applicationDbContext.ToListAsync());
        }

        // GET: Imovels/Details/5
        [AllowAnonymous]
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null) return NotFound();

            var imovel = await _context.Imoveis
                .Include(i => i.CategoriaImovel)
                .Include(i => i.EstadoImovel)
                .Include(i => i.TipoNegocio)
                .Include(i => i.CertificadoEnergetico)
                .Include(i => i.Consultor)
                .Include(i => i.Fotos)
                .Include(i => i.Caracteristicas)
                    .ThenInclude(ic => ic.Caracteristica)
                        .ThenInclude(c => c.GrupoCaracteristica)
                .FirstOrDefaultAsync(m => m.Id == id);

            if (imovel == null) return NotFound();

            var proprietarioAtual = await _context.ImoveisProprietarios.FirstOrDefaultAsync(ip => ip.ImovelId == id);
            ViewData["ListaClientes"] = new SelectList(_context.Clientes, "Id", "Nome", proprietarioAtual?.ClienteId);

            return View(imovel);
        }

        // GET: Imovels/Create
        [Authorize(Roles = "Admin,Consultor")]
        public async Task<IActionResult> Create()
        {
            ViewData["CategoriaImovelId"] = new SelectList(_context.CategoriasImovel, "Id", "Nome");
            ViewData["TipoNegocioId"] = new SelectList(_context.TiposNegocio, "Id", "Nome");
            ViewData["EstadoImovelId"] = new SelectList(_context.EstadosImovel, "Id", "Nome");
            ViewData["StatusImovelId"] = new SelectList(_context.StatusImoveis, "Id", "Nome");
            ViewData["CertificadoEnergeticoId"] = new SelectList(_context.CertificadosEnergeticos, "Id", "Nome");
            ViewData["ListaClientes"] = new SelectList(_context.Clientes, "Id", "Nome");

            ViewBag.GruposComCaracteristicas = await _context.GruposCaracteristicas
                .Include(g => g.Caracteristicas)
                .ToListAsync();

            return View();
        }

        // POST: Imovels/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Admin,Consultor")]
        public async Task<IActionResult> Create([Bind("Id,Titulo,Preco,Descricao,Quartos,CasasBanho,Estacionamento,AreaUtil,AreaBruta,Piso,AnoConstrucao,NumeroFrentes,Distrito,Concelho,Freguesia,Zona,MoradaExata,NumeroContrato,ObservacoesInternas,ValorComissao,CategoriaImovelId,TipoNegocioId,EstadoImovelId,StatusImovelId,CertificadoEnergeticoId")] Imovel imovel, int? clienteProprietarioId, List<int> selectedCaracteristicas, List<IFormFile> fotosUpload)
        {
            ModelState.Remove("ConsultorId");

            if (ModelState.IsValid)
            {
                var user = await _userManager.GetUserAsync(User);
                var consultorLogado = await _context.Consultores.FirstOrDefaultAsync(c => c.IdentityUserId == user.Id);
                if (consultorLogado != null) imovel.ConsultorId = consultorLogado.Id;

                _context.Add(imovel);
                await _context.SaveChangesAsync();

                if (clienteProprietarioId.HasValue)
                {
                    _context.ImoveisProprietarios.Add(new ImovelProprietario { ImovelId = imovel.Id, ClienteId = clienteProprietarioId.Value });
                    await _context.SaveChangesAsync();
                }

                if (selectedCaracteristicas != null && selectedCaracteristicas.Any())
                {
                    foreach (var caracId in selectedCaracteristicas)
                    {
                        _context.ImoveisCaracteristicas.Add(new ImovelCaracteristica
                        {
                            ImovelId = imovel.Id,
                            CaracteristicaId = caracId
                        });
                    }
                    await _context.SaveChangesAsync();
                }

                // 3. UPLOAD DE FOTOS PARA A PASTA FÍSICA DO SERVIDOR
                if (fotosUpload != null && fotosUpload.Count > 0)
                {
                    string pastaUploads = Path.Combine(_hostEnvironment.WebRootPath, "uploads", "imoveis");
                    if (!Directory.Exists(pastaUploads)) Directory.CreateDirectory(pastaUploads);

                    int ordemCounter = 1;
                    foreach (var formFile in fotosUpload)
                    {
                        if (formFile.Length > 0 && formFile.ContentType.StartsWith("image/"))
                        {
                            string nomeUnico = Guid.NewGuid().ToString() + "_" + formFile.FileName;
                            string caminhoCompleto = Path.Combine(pastaUploads, nomeUnico);

                            // Guarda no disco
                            using (var fileStream = new FileStream(caminhoCompleto, FileMode.Create))
                            {
                                await formFile.CopyToAsync(fileStream);
                            }

                            // Guarda só o link na Base de Dados
                            var novaFoto = new FotoImovel
                            {
                                ImovelId = imovel.Id,
                                CaminhoImagem = "/uploads/imoveis/" + nomeUnico,
                                Ordem = ordemCounter
                            };
                            _context.Fotos.Add(novaFoto);
                            ordemCounter++;
                        }
                    }
                    await _context.SaveChangesAsync();
                }

                return RedirectToAction(nameof(Index));
            }

            ViewData["CategoriaImovelId"] = new SelectList(_context.CategoriasImovel, "Id", "Nome", imovel.CategoriaImovelId);
            ViewData["TipoNegocioId"] = new SelectList(_context.TiposNegocio, "Id", "Nome", imovel.TipoNegocioId);
            ViewData["EstadoImovelId"] = new SelectList(_context.EstadosImovel, "Id", "Nome", imovel.EstadoImovelId);
            ViewData["StatusImovelId"] = new SelectList(_context.StatusImoveis, "Id", "Nome", imovel.StatusImovelId);
            ViewData["CertificadoEnergeticoId"] = new SelectList(_context.CertificadosEnergeticos, "Id", "Nome", imovel.CertificadoEnergeticoId);
            ViewData["ListaClientes"] = new SelectList(_context.Clientes, "Id", "Nome", clienteProprietarioId);
            ViewBag.GruposComCaracteristicas = await _context.GruposCaracteristicas.Include(g => g.Caracteristicas).ToListAsync();

            return View(imovel);
        }

        // GET: Imovels/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null) return NotFound();

            var imovel = await _context.Imoveis
                .Include(i => i.Fotos)
                .Include(i => i.Caracteristicas)
                .FirstOrDefaultAsync(m => m.Id == id);

            if (imovel == null) return NotFound();

            ViewData["CategoriaImovelId"] = new SelectList(_context.CategoriasImovel, "Id", "Nome", imovel.CategoriaImovelId);
            ViewData["CertificadoEnergeticoId"] = new SelectList(_context.CertificadosEnergeticos, "Id", "Nome", imovel.CertificadoEnergeticoId);
            ViewData["ConsultorId"] = new SelectList(_context.Consultores, "Id", "Nome", imovel.ConsultorId);
            ViewData["EstadoImovelId"] = new SelectList(_context.EstadosImovel, "Id", "Nome", imovel.EstadoImovelId);
            ViewData["StatusImovelId"] = new SelectList(_context.StatusImoveis, "Id", "Nome", imovel.StatusImovelId);
            ViewData["TipoNegocioId"] = new SelectList(_context.TiposNegocio, "Id", "Nome", imovel.TipoNegocioId);

            ViewBag.GruposComCaracteristicas = await _context.GruposCaracteristicas
                .Include(g => g.Caracteristicas)
                .ToListAsync();

            ViewBag.CaracteristicasAtuais = imovel.Caracteristicas?.Select(c => c.CaracteristicaId).ToList() ?? new List<int>();

            return View(imovel);
        }

        // POST: Imovels/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("Id,Titulo,Preco,Descricao,Quartos,CasasBanho,Estacionamento,AreaUtil,AreaBruta,Piso,AnoConstrucao,NumeroFrentes,Distrito,Concelho,Freguesia,Zona,MoradaExata,NumeroContrato,ObservacoesInternas,ValorComissao,CategoriaImovelId,TipoNegocioId,EstadoImovelId,StatusImovelId,CertificadoEnergeticoId,ConsultorId")] Imovel imovel, List<int> selectedCaracteristicas, List<IFormFile> novasFotos, int? clienteProprietarioId)
        {
            if (id != imovel.Id) return NotFound();

            string existingPhotoOrder = Request.Form["existingPhotoOrder"];

            ModelState.Remove("Consultor");
            ModelState.Remove("CategoriaImovel");
            ModelState.Remove("TipoNegocio");
            ModelState.Remove("EstadoImovel");
            ModelState.Remove("StatusImovel");
            ModelState.Remove("CertificadoEnergetico");
            ModelState.Remove("Fotos");
            ModelState.Remove("Caracteristicas");

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(imovel);

                    var caracteristicasAntigas = _context.ImoveisCaracteristicas.Where(ic => ic.ImovelId == id);
                    _context.ImoveisCaracteristicas.RemoveRange(caracteristicasAntigas);

                    if (selectedCaracteristicas != null && selectedCaracteristicas.Any())
                    {
                        foreach (var caracId in selectedCaracteristicas)
                        {
                            _context.ImoveisCaracteristicas.Add(new ImovelCaracteristica { ImovelId = imovel.Id, CaracteristicaId = caracId });
                        }
                    }

                    var propAtual = _context.ImoveisProprietarios.FirstOrDefault(ip => ip.ImovelId == imovel.Id);
                    if (propAtual != null) _context.ImoveisProprietarios.Remove(propAtual);
                    if (clienteProprietarioId.HasValue) _context.ImoveisProprietarios.Add(new ImovelProprietario { ImovelId = imovel.Id, ClienteId = clienteProprietarioId.Value });

                    // 4. LÓGICA DE FOTOS E ORDENAÇÃO
                    if (!string.IsNullOrEmpty(existingPhotoOrder))
                    {
                        var itensOrdem = existingPhotoOrder.Split(',').Select(s => s.Trim()).ToList();
                        int ordemAtual = 1;

                        var listaNovasFotos = novasFotos?.ToList() ?? new List<IFormFile>();
                        string pastaUploads = Path.Combine(_hostEnvironment.WebRootPath, "uploads", "imoveis");
                        if (listaNovasFotos.Any() && !Directory.Exists(pastaUploads)) Directory.CreateDirectory(pastaUploads);

                        foreach (var item in itensOrdem)
                        {
                            if (item.StartsWith("old_"))
                            {
                                if (int.TryParse(item.Replace("old_", ""), out int fotoId))
                                {
                                    var foto = await _context.Fotos.FindAsync(fotoId);
                                    if (foto != null && foto.ImovelId == id)
                                    {
                                        foto.Ordem = ordemAtual;
                                        _context.Update(foto);
                                        ordemAtual++;
                                    }
                                }
                            }
                            else if (item.StartsWith("new_"))
                            {
                                if (int.TryParse(item.Replace("new_", ""), out int fileIndex))
                                {
                                    if (fileIndex >= 0 && fileIndex < listaNovasFotos.Count)
                                    {
                                        var formFile = listaNovasFotos[fileIndex];
                                        if (formFile.Length > 0 && formFile.ContentType.StartsWith("image/"))
                                        {
                                            string nomeUnico = Guid.NewGuid().ToString() + "_" + formFile.FileName;
                                            string caminhoCompleto = Path.Combine(pastaUploads, nomeUnico);

                                            using (var fileStream = new FileStream(caminhoCompleto, FileMode.Create))
                                            {
                                                await formFile.CopyToAsync(fileStream);
                                            }

                                            var novaFoto = new FotoImovel
                                            {
                                                ImovelId = imovel.Id,
                                                CaminhoImagem = "/uploads/imoveis/" + nomeUnico,
                                                Ordem = ordemAtual
                                            };
                                            _context.Fotos.Add(novaFoto);
                                            ordemAtual++;
                                        }
                                    }
                                }
                            }
                        }
                    }

                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!ImovelExists(imovel.Id))
                    {
                        TempData["MensagemErro"] = $"O imóvel REF: SHARP-{imovel.Id:D5} já foi apagado por outro utilizador.";
                        return RedirectToAction(nameof(Index));
                    }
                    else throw;
                }
                return RedirectToAction(nameof(Index));
            }

            ViewData["CategoriaImovelId"] = new SelectList(_context.CategoriasImovel, "Id", "Nome", imovel.CategoriaImovelId);
            ViewData["CertificadoEnergeticoId"] = new SelectList(_context.CertificadosEnergeticos, "Id", "Nome", imovel.CertificadoEnergeticoId);
            ViewData["ConsultorId"] = new SelectList(_context.Consultores, "Id", "Nome", imovel.ConsultorId);
            ViewData["EstadoImovelId"] = new SelectList(_context.EstadosImovel, "Id", "Nome", imovel.EstadoImovelId);
            ViewData["StatusImovelId"] = new SelectList(_context.StatusImoveis, "Id", "Nome", imovel.StatusImovelId);
            ViewData["TipoNegocioId"] = new SelectList(_context.TiposNegocio, "Id", "Nome", imovel.TipoNegocioId);
            var proprietarioAtual = _context.ImoveisProprietarios.FirstOrDefault(ip => ip.ImovelId == imovel.Id);
            ViewData["ListaClientes"] = new SelectList(_context.Clientes, "Id", "Nome", proprietarioAtual?.ClienteId);
            ViewBag.GruposComCaracteristicas = await _context.GruposCaracteristicas.Include(g => g.Caracteristicas).ToListAsync();
            ViewBag.CaracteristicasAtuais = selectedCaracteristicas ?? new List<int>();

            return View(imovel);
        }

        // GET: Imovels/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null) return NotFound();

            var imovel = await _context.Imoveis
                .Include(i => i.CategoriaImovel)
                .Include(i => i.CertificadoEnergetico)
                .Include(i => i.Consultor)
                .Include(i => i.EstadoImovel)
                .Include(i => i.StatusImovel)
                .Include(i => i.TipoNegocio)
                .FirstOrDefaultAsync(m => m.Id == id);

            if (imovel == null) return NotFound();

            return View(imovel);
        }

        // POST: Imovels/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            // Trazemos o imóvel juntamente com a lista das suas fotografias
            var imovel = await _context.Imoveis.Include(i => i.Fotos).FirstOrDefaultAsync(m => m.Id == id);

            if (imovel != null)
            {
                // 5. APAGAR FOTOS DO DISCO ANTES DE APAGAR O IMÓVEL
                if (imovel.Fotos != null && imovel.Fotos.Any())
                {
                    foreach (var foto in imovel.Fotos)
                    {
                        if (!string.IsNullOrEmpty(foto.CaminhoImagem))
                        {
                            string caminhoFisico = Path.Combine(_hostEnvironment.WebRootPath, foto.CaminhoImagem.TrimStart('/'));
                            if (System.IO.File.Exists(caminhoFisico))
                            {
                                System.IO.File.Delete(caminhoFisico); // Liberta o espaço no disco!
                            }
                        }
                    }
                }

                _context.Imoveis.Remove(imovel);
                await _context.SaveChangesAsync();
            }

            return RedirectToAction(nameof(Index));
        }

        private bool ImovelExists(int id)
        {
            return _context.Imoveis.Any(e => e.Id == id);
        }
    }
}