using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using RPImobiliaria.Data;
using RPImobiliaria.Models;
using RPImobiliaria.Models.ViewModels;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Hosting;
using System.IO;

namespace RPImobiliaria.Controllers
{
    [Authorize]
    public class ImovelsController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly IWebHostEnvironment _hostEnvironment;

        public ImovelsController(ApplicationDbContext context, UserManager<ApplicationUser> userManager, IWebHostEnvironment hostEnvironment)
        {
            _context = context;
            _userManager = userManager;
            _hostEnvironment = hostEnvironment;
        }

        // =========================================================================
        // GET: Imoveis (CATÁLOGO PÚBLICO)
        // =========================================================================
        [AllowAnonymous]
        public async Task<IActionResult> Index(ImovelCatalogoViewModel filtros)
        {
            var query = _context.Imoveis
                .Include(i => i.CategoriaImovel)
                .Include(i => i.EstadoImovel)
                .Include(i => i.TipoNegocio)
                .Include(i => i.StatusImovel)
                .Include(i => i.CertificadoEnergetico)
                .Include(i => i.Fotos)
                .Include(i => i.Caracteristicas)
                .Include(i => i.Freguesia).ThenInclude(f => f.Concelho).ThenInclude(c => c.Distrito)
                .AsQueryable();

            if (!string.IsNullOrWhiteSpace(filtros.Pesquisa))
            {
                var pesquisa = filtros.Pesquisa.Trim();
                query = query.Where(i => i.Titulo.Contains(pesquisa)
                    || (i.Referencia != null && i.Referencia.Contains(pesquisa))
                    || i.Id.ToString() == pesquisa
                    || (i.Zona != null && i.Zona.Contains(pesquisa))
                    || (i.Freguesia != null && (i.Freguesia.Nome.Contains(pesquisa) || i.Freguesia.Concelho.Nome.Contains(pesquisa))));
            }

            if (filtros.NegocioId.HasValue) query = query.Where(i => i.TipoNegocioId == filtros.NegocioId.Value);
            else if (!string.IsNullOrWhiteSpace(filtros.Negocio)) query = query.Where(i => i.TipoNegocio != null && i.TipoNegocio.Nome.Contains(filtros.Negocio));
            if (filtros.CategoriaId.HasValue) query = query.Where(i => i.CategoriaImovelId == filtros.CategoriaId.Value);
            if (filtros.EstadoId.HasValue) query = query.Where(i => i.EstadoImovelId == filtros.EstadoId.Value);
            if (filtros.StatusId.HasValue) query = query.Where(i => i.StatusImovelId == filtros.StatusId.Value);
            if (filtros.CertificadoId.HasValue) query = query.Where(i => i.CertificadoEnergeticoId == filtros.CertificadoId.Value);
            if (filtros.DistritoId.HasValue) query = query.Where(i => i.Freguesia != null && i.Freguesia.Concelho.DistritoId == filtros.DistritoId.Value);
            if (filtros.ConcelhoId.HasValue) query = query.Where(i => i.Freguesia != null && i.Freguesia.ConcelhoId == filtros.ConcelhoId.Value);
            if (filtros.FreguesiaId.HasValue) query = query.Where(i => i.FreguesiaId == filtros.FreguesiaId.Value);
            if (filtros.Quartos.HasValue) query = filtros.Quartos.Value >= 4 ? query.Where(i => i.Quartos >= 4) : query.Where(i => i.Quartos == filtros.Quartos.Value);
            if (filtros.CasasBanho.HasValue) query = query.Where(i => i.CasasBanho >= filtros.CasasBanho.Value);
            if (filtros.Estacionamento.HasValue) query = query.Where(i => i.Estacionamento >= filtros.Estacionamento.Value);
            if (filtros.PrecoMin.HasValue) query = query.Where(i => i.Preco >= filtros.PrecoMin.Value);
            if (filtros.PrecoMax.HasValue) query = query.Where(i => i.Preco <= filtros.PrecoMax.Value);
            if (filtros.AreaMin.HasValue) query = query.Where(i => i.AreaUtil >= filtros.AreaMin.Value);
            if (filtros.AreaMax.HasValue) query = query.Where(i => i.AreaUtil <= filtros.AreaMax.Value);
            if (filtros.AnoConstrucaoMin.HasValue) query = query.Where(i => i.AnoConstrucao >= filtros.AnoConstrucaoMin.Value);

            foreach (var caracteristicaId in filtros.CaracteristicasIds.Distinct())
                query = query.Where(i => i.Caracteristicas.Any(ic => ic.CaracteristicaId == caracteristicaId));

            switch (filtros.OrdenarPor)
            {
                case "preco_asc": query = query.OrderBy(i => i.Preco); break;
                case "preco_desc": query = query.OrderByDescending(i => i.Preco); break;
                case "area_desc": query = query.OrderByDescending(i => i.AreaUtil); break;
                case "titulo": query = query.OrderBy(i => i.Titulo); break;
                default: query = query.OrderByDescending(i => i.DataRegisto).ThenByDescending(i => i.Id); break;
            }

            filtros.Imoveis = await query.AsNoTracking().ToListAsync();
            filtros.TiposNegocio = await _context.TiposNegocio.AsNoTracking().OrderBy(t => t.Nome).ToListAsync();
            filtros.Categorias = await _context.CategoriasImovel.AsNoTracking().OrderBy(c => c.Nome).ToListAsync();
            filtros.Estados = await _context.EstadosImovel.AsNoTracking().OrderBy(e => e.Nome).ToListAsync();
            filtros.Statuses = await _context.StatusImoveis.AsNoTracking().OrderBy(s => s.Nome).ToListAsync();
            filtros.Certificados = await _context.CertificadosEnergeticos.AsNoTracking().OrderBy(c => c.Nome).ToListAsync();
            filtros.Distritos = await _context.Distritos.AsNoTracking().OrderBy(d => d.Nome).ToListAsync();
            filtros.Concelhos = filtros.DistritoId.HasValue
                ? await _context.Concelhos.AsNoTracking().Where(c => c.DistritoId == filtros.DistritoId).OrderBy(c => c.Nome).ToListAsync()
                : Array.Empty<Concelho>();
            filtros.Freguesias = filtros.ConcelhoId.HasValue
                ? await _context.Freguesias.AsNoTracking().Where(f => f.ConcelhoId == filtros.ConcelhoId).OrderBy(f => f.Nome).ToListAsync()
                : Array.Empty<Freguesia>();
            filtros.CaracteristicasDisponiveis = await _context.GruposCaracteristicas
                .SelectMany(g => g.Caracteristicas)
                .Where(c => _context.ImoveisCaracteristicas.Any(ic => ic.CaracteristicaId == c.Id))
                .OrderBy(c => c.Nome)
                .AsNoTracking()
                .ToListAsync();

            return View(filtros);
        }

        [Authorize(Roles = "Admin,Consultor")]
        public async Task<IActionResult> Gestao()
        {
            var query = _context.Imoveis
                .Include(i => i.TipoNegocio)
                .Include(i => i.StatusImovel)
                .Include(i => i.Consultor)
                .Include(i => i.Freguesia).ThenInclude(f => f.Concelho)
                .AsQueryable();

            if (!User.IsInRole("Admin"))
            {
                var userId = _userManager.GetUserId(User);
                query = query.Where(i => i.Consultor != null && i.Consultor.ApplicationUserId == userId);
            }

            return View(await query.OrderByDescending(i => i.DataRegisto).ThenByDescending(i => i.Id).ToListAsync());
        }

        [AllowAnonymous]
        [HttpGet]
        public async Task<IActionResult> ObterConcelhos(int distritoId)
        {
            var concelhos = await _context.Concelhos
                .AsNoTracking()
                .Where(c => c.DistritoId == distritoId)
                .OrderBy(c => c.Nome)
                .Select(c => new { c.Id, c.Nome })
                .ToListAsync();

            return Json(concelhos);
        }

        [AllowAnonymous]
        [HttpGet]
        public async Task<IActionResult> ObterFreguesias(int concelhoId)
        {
            var freguesias = await _context.Freguesias
                .AsNoTracking()
                .Where(f => f.ConcelhoId == concelhoId)
                .OrderBy(f => f.Nome)
                .Select(f => new { f.Id, f.Nome })
                .ToListAsync();

            return Json(freguesias);
        }

        // =========================================================================
        // GET: Imovels/Details/5 (PÚBLICO)
        // =========================================================================
        [AllowAnonymous]
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null) return NotFound();

            var imovel = await _context.Imoveis
                .Include(i => i.CategoriaImovel)
                .Include(i => i.EstadoImovel)
                .Include(i => i.TipoNegocio)
                .Include(i => i.StatusImovel)
                .Include(i => i.CertificadoEnergetico)
                .Include(i => i.Consultor)
                .Include(i => i.Fotos)
                .Include(i => i.Documentos)
                .Include(i => i.Freguesia).ThenInclude(f => f.Concelho).ThenInclude(c => c.Distrito)
                .Include(i => i.Caracteristicas).ThenInclude(ic => ic.Caracteristica).ThenInclude(c => c.GrupoCaracteristica)
                .FirstOrDefaultAsync(m => m.Id == id);

            if (imovel == null) return NotFound();

            return View(imovel);
        }

        // =========================================================================
        // GET: Imovels/Create
        // =========================================================================
        [Authorize(Roles = "Admin,Consultor")]
        public async Task<IActionResult> Create()
        {
            await CarregarDropdownsAsync();
            return View();
        }

        // =========================================================================
        // POST: Imovels/Create
        // =========================================================================
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Admin,Consultor")]
        public async Task<IActionResult> Create([Bind("Id,Referencia,Titulo,Preco,Descricao,Quartos,CasasBanho,Estacionamento,AreaUtil,AreaBruta,Piso,AnoConstrucao,NumeroFrentes,FreguesiaId,Zona,MoradaExata,NumeroContrato,ObservacoesInternas,ValorComissao,CategoriaImovelId,TipoNegocioId,EstadoImovelId,StatusImovelId,CertificadoEnergeticoId")] Imovel imovel, int? clienteProprietarioId, List<int> selectedCaracteristicas, List<IFormFile> fotosUpload, List<IFormFile> documentosUpload)
        {
            ModelState.Remove("ConsultorId");

            if (ModelState.IsValid)
            {
                var user = await _userManager.GetUserAsync(User);
                var consultorLogado = await _context.Consultores.FirstOrDefaultAsync(c => c.ApplicationUserId == user.Id);
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
                        _context.ImoveisCaracteristicas.Add(new ImovelCaracteristica { ImovelId = imovel.Id, CaracteristicaId = caracId });
                    }
                    await _context.SaveChangesAsync();
                }

                await ProcessarUploadFotos(imovel.Id, fotosUpload);
                await ProcessarUploadDocumentos(imovel.Id, documentosUpload);

                return RedirectToAction(nameof(Index));
            }

            await CarregarDropdownsAsync(imovel, clienteProprietarioId, selectedCaracteristicas);
            return View(imovel);
        }

        // =========================================================================
        // GET: Imovels/Edit/5
        // =========================================================================
        [Authorize(Roles = "Admin,Consultor")]
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null) return NotFound();

            var imovel = await _context.Imoveis
                .Include(i => i.Fotos)
                .Include(i => i.Documentos)
                .Include(i => i.Caracteristicas)
                .FirstOrDefaultAsync(m => m.Id == id);

            if (imovel == null) return NotFound();

            var proprietarioAtual = await _context.ImoveisProprietarios.FirstOrDefaultAsync(ip => ip.ImovelId == id);
            await CarregarDropdownsAsync(imovel, proprietarioAtual?.ClienteId, imovel.Caracteristicas.Select(c => c.CaracteristicaId).ToList());

            return View(imovel);
        }

        // =========================================================================
        // POST: Imovels/Edit/5
        // =========================================================================
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Admin,Consultor")]
        public async Task<IActionResult> Edit(int id, [Bind("Id,Referencia,Titulo,Preco,Descricao,Quartos,CasasBanho,Estacionamento,AreaUtil,AreaBruta,Piso,AnoConstrucao,NumeroFrentes,FreguesiaId,Zona,MoradaExata,NumeroContrato,ObservacoesInternas,ValorComissao,CategoriaImovelId,TipoNegocioId,EstadoImovelId,StatusImovelId,CertificadoEnergeticoId,ConsultorId")] Imovel imovel, List<int> selectedCaracteristicas, List<IFormFile> novasFotos, List<IFormFile> documentosUpload, List<int> documentosRemover, int? clienteProprietarioId)
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
            ModelState.Remove("Documentos");
            ModelState.Remove("Caracteristicas");
            ModelState.Remove("Freguesia");

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
                                if (int.TryParse(item.Replace("new_", ""), out int fileIndex) && fileIndex >= 0 && fileIndex < listaNovasFotos.Count)
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

                                        _context.Fotos.Add(new FotoImovel { ImovelId = imovel.Id, CaminhoImagem = "/uploads/imoveis/" + nomeUnico, Ordem = ordemAtual });
                                        ordemAtual++;
                                    }
                                }
                            }
                        }
                    }

                    await ProcessarUploadDocumentos(imovel.Id, documentosUpload);

                    if (documentosRemover != null && documentosRemover.Any())
                    {
                        foreach (var docId in documentosRemover)
                        {
                            var docToDelete = await _context.Documentos.FindAsync(docId);
                            if (docToDelete != null && docToDelete.ImovelId == id)
                            {
                                string caminhoFisico = Path.Combine(_hostEnvironment.WebRootPath, docToDelete.CaminhoFicheiro.TrimStart('/'));
                                if (System.IO.File.Exists(caminhoFisico)) System.IO.File.Delete(caminhoFisico);

                                _context.Documentos.Remove(docToDelete);
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

            await CarregarDropdownsAsync(imovel, clienteProprietarioId, selectedCaracteristicas);
            return View(imovel);
        }

        // =========================================================================
        // GET: Imovels/Delete/5
        // =========================================================================
        [Authorize(Roles = "Admin,Consultor")]
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null) return NotFound();

            var imovel = await _context.Imoveis
                .Include(i => i.Consultor)
                .Include(i => i.Freguesia).ThenInclude(f => f.Concelho).ThenInclude(c => c.Distrito)
                .FirstOrDefaultAsync(m => m.Id == id);

            if (imovel == null) return NotFound();

            return View(imovel);
        }

        // =========================================================================
        // POST: Imovels/Delete/5
        // =========================================================================
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Admin,Consultor")]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var imovel = await _context.Imoveis
                .Include(i => i.Fotos)
                .Include(i => i.Documentos)
                .FirstOrDefaultAsync(m => m.Id == id);

            if (imovel != null)
            {
                if (imovel.Fotos != null)
                {
                    foreach (var foto in imovel.Fotos)
                    {
                        if (!string.IsNullOrEmpty(foto.CaminhoImagem))
                        {
                            string caminho = Path.Combine(_hostEnvironment.WebRootPath, foto.CaminhoImagem.TrimStart('/'));
                            if (System.IO.File.Exists(caminho)) System.IO.File.Delete(caminho);
                        }
                    }
                }

                if (imovel.Documentos != null)
                {
                    foreach (var doc in imovel.Documentos)
                    {
                        if (!string.IsNullOrEmpty(doc.CaminhoFicheiro))
                        {
                            string caminho = Path.Combine(_hostEnvironment.WebRootPath, doc.CaminhoFicheiro.TrimStart('/'));
                            if (System.IO.File.Exists(caminho)) System.IO.File.Delete(caminho);
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

        // --- MÉTODOS AUXILIARES ---
        private async Task CarregarDropdownsAsync(Imovel? imovel = null, int? clienteProprietarioId = null, List<int>? selectedCaracteristicas = null)
        {
            ViewData["CategoriaImovelId"] = new SelectList(_context.CategoriasImovel, "Id", "Nome", imovel?.CategoriaImovelId);
            ViewData["TipoNegocioId"] = new SelectList(_context.TiposNegocio, "Id", "Nome", imovel?.TipoNegocioId);
            ViewData["EstadoImovelId"] = new SelectList(_context.EstadosImovel, "Id", "Nome", imovel?.EstadoImovelId);
            ViewData["StatusImovelId"] = new SelectList(_context.StatusImoveis, "Id", "Nome", imovel?.StatusImovelId);
            ViewData["CertificadoEnergeticoId"] = new SelectList(_context.CertificadosEnergeticos, "Id", "Nome", imovel?.CertificadoEnergeticoId);
            ViewData["ListaClientes"] = new SelectList(_context.Clientes, "Id", "Nome", clienteProprietarioId);
            ViewData["ConsultorId"] = new SelectList(_context.Consultores, "Id", "Nome", imovel?.ConsultorId);

            var localizacaoSelecionada = imovel?.FreguesiaId is int freguesiaId
                ? await _context.Freguesias.Include(f => f.Concelho).FirstOrDefaultAsync(f => f.Id == freguesiaId)
                : null;
            var distritoId = localizacaoSelecionada?.Concelho?.DistritoId;
            var concelhoId = localizacaoSelecionada?.ConcelhoId;

            ViewBag.DistritoId = new SelectList(_context.Distritos.OrderBy(d => d.Nome), "Id", "Nome", distritoId);
            ViewBag.ConcelhoId = new SelectList(
                distritoId.HasValue ? _context.Concelhos.Where(c => c.DistritoId == distritoId).OrderBy(c => c.Nome) : Enumerable.Empty<Concelho>(),
                "Id", "Nome", concelhoId);
            ViewBag.FreguesiaId = new SelectList(
                concelhoId.HasValue ? _context.Freguesias.Where(f => f.ConcelhoId == concelhoId).OrderBy(f => f.Nome) : Enumerable.Empty<Freguesia>(),
                "Id", "Nome", imovel?.FreguesiaId);

            ViewBag.GruposComCaracteristicas = _context.GruposCaracteristicas.Include(g => g.Caracteristicas).ToList();
            ViewBag.CaracteristicasAtuais = selectedCaracteristicas ?? new List<int>();
        }

        private async Task ProcessarUploadFotos(int imovelId, List<IFormFile> fotosUpload)
        {
            if (fotosUpload == null || fotosUpload.Count == 0) return;

            string pastaUploads = Path.Combine(_hostEnvironment.WebRootPath, "uploads", "imoveis");
            if (!Directory.Exists(pastaUploads)) Directory.CreateDirectory(pastaUploads);

            int ordemCounter = 1;
            foreach (var formFile in fotosUpload)
            {
                if (formFile.Length > 0 && formFile.ContentType.StartsWith("image/"))
                {
                    string nomeUnico = Guid.NewGuid().ToString() + "_" + formFile.FileName;
                    string caminhoCompleto = Path.Combine(pastaUploads, nomeUnico);

                    using (var stream = new FileStream(caminhoCompleto, FileMode.Create))
                    {
                        await formFile.CopyToAsync(stream);
                    }
                    _context.Fotos.Add(new FotoImovel { ImovelId = imovelId, CaminhoImagem = "/uploads/imoveis/" + nomeUnico, Ordem = ordemCounter++ });
                }
            }
            await _context.SaveChangesAsync();
        }

        private async Task ProcessarUploadDocumentos(int imovelId, List<IFormFile> documentosUpload)
        {
            if (documentosUpload == null || documentosUpload.Count == 0) return;

            string pastaDocs = Path.Combine(_hostEnvironment.WebRootPath, "uploads", "documentos");
            if (!Directory.Exists(pastaDocs)) Directory.CreateDirectory(pastaDocs);

            foreach (var doc in documentosUpload)
            {
                if (doc.Length > 0)
                {
                    string nomeOriginal = Path.GetFileName(doc.FileName);
                    string nomeUnico = Guid.NewGuid().ToString() + "_" + nomeOriginal;
                    string caminhoCompleto = Path.Combine(pastaDocs, nomeUnico);

                    using (var stream = new FileStream(caminhoCompleto, FileMode.Create))
                    {
                        await doc.CopyToAsync(stream);
                    }
                    _context.Documentos.Add(new DocumentoImovel { ImovelId = imovelId, NomeFicheiro = nomeOriginal, CaminhoFicheiro = "/uploads/documentos/" + nomeUnico, DataUpload = DateTime.Now });
                }
            }
            await _context.SaveChangesAsync();
        }
    }
}
