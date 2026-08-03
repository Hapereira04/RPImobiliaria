using RPImobiliaria.Models;

namespace RPImobiliaria.Models.ViewModels;

public class ImovelCatalogoViewModel
{
    public IReadOnlyList<Imovel> Imoveis { get; set; } = Array.Empty<Imovel>();
    public IReadOnlyList<TipoNegocio> TiposNegocio { get; set; } = Array.Empty<TipoNegocio>();
    public IReadOnlyList<CategoriaImovel> Categorias { get; set; } = Array.Empty<CategoriaImovel>();
    public IReadOnlyList<EstadoImovel> Estados { get; set; } = Array.Empty<EstadoImovel>();
    public IReadOnlyList<StatusImovel> Statuses { get; set; } = Array.Empty<StatusImovel>();
    public IReadOnlyList<CertificadoEnergetico> Certificados { get; set; } = Array.Empty<CertificadoEnergetico>();
    public IReadOnlyList<Distrito> Distritos { get; set; } = Array.Empty<Distrito>();
    public IReadOnlyList<Concelho> Concelhos { get; set; } = Array.Empty<Concelho>();
    public IReadOnlyList<Freguesia> Freguesias { get; set; } = Array.Empty<Freguesia>();
    public IReadOnlyList<Caracteristica> CaracteristicasDisponiveis { get; set; } = Array.Empty<Caracteristica>();

    public string? Pesquisa { get; set; }
    public string? Negocio { get; set; }
    public int? NegocioId { get; set; }
    public int? CategoriaId { get; set; }
    public int? EstadoId { get; set; }
    public int? StatusId { get; set; }
    public int? CertificadoId { get; set; }
    public int? DistritoId { get; set; }
    public int? ConcelhoId { get; set; }
    public int? FreguesiaId { get; set; }
    public int? Quartos { get; set; }
    public int? CasasBanho { get; set; }
    public int? Estacionamento { get; set; }
    public decimal? PrecoMin { get; set; }
    public decimal? PrecoMax { get; set; }
    public double? AreaMin { get; set; }
    public double? AreaMax { get; set; }
    public int? AnoConstrucaoMin { get; set; }
    public List<int> CaracteristicasIds { get; set; } = new();
    public string? OrdenarPor { get; set; }
}
