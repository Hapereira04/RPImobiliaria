using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace RPImobiliaria.Models
{
    public class Imovel
    {
        public int Id { get; set; }

        [Display(Name = "Referência Interna")]
        public string? Referencia { get; set; }

        public bool EmDestaque { get; set; } = false;

        // --- DADOS PÚBLICOS ---
        [Required(ErrorMessage = "O título é obrigatório.")]
        [StringLength(150, ErrorMessage = "O título não pode exceder 150 caracteres.")]
        public string Titulo { get; set; }

        [Required(ErrorMessage = "O preço é obrigatório.")]
        [Column(TypeName = "decimal(18,2)")]
        [Display(Name = "Preço (€)")]
        public decimal Preco { get; set; }

        [Display(Name = "Descrição")]
        public string? Descricao { get; set; }

        // --- CARACTERÍSTICAS RESIDENCIAIS / COMERCIAIS (OPCIONAIS P/ TERRENOS E NEGÓCIOS) ---
        [Display(Name = "Quartos / Tipologia")]
        public int? Quartos { get; set; }

        [Display(Name = "Casas de Banho")]
        public int? CasasBanho { get; set; }

        [Display(Name = "Lugares de Estacionamento")]
        public int? Estacionamento { get; set; }

        [Display(Name = "Área Útil (m²)")]
        public double? AreaUtil { get; set; }

        // Área Total ou Bruta (obrigatória em habitações, lojas e terrenos)
        [Required(ErrorMessage = "A área bruta / total do terreno é obrigatória.")]
        [Display(Name = "Área Bruta / Total (m²)")]
        public double AreaBruta { get; set; }

        [Display(Name = "Piso / Andar")]
        public int? Piso { get; set; }

        [Display(Name = "Ano de Construção")]
        public int? AnoConstrucao { get; set; }

        [Display(Name = "Número de Frentes")]
        public int? NumeroFrentes { get; set; }

        // --- LOCALIZAÇÃO PÚBLICA ---
        public int? DistritoId { get; set; }
        public virtual Distrito? Distrito { get; set; }

        public int? ConcelhoId { get; set; }
        public virtual Concelho? Concelho { get; set; }

        public int? FreguesiaId { get; set; }
        public virtual Freguesia? Freguesia { get; set; }

        [Display(Name = "Zona / Localidade")]
        public string? Zona { get; set; }

        // --- DADOS PRIVADOS (SÓ O CONSULTOR VÊ) ---
        [Display(Name = "Morada Exata (Privado)")]
        public string? MoradaExata { get; set; }

        [Display(Name = "Número do Contrato")]
        public string? NumeroContrato { get; set; }

        [Display(Name = "Observações Internas")]
        public string? ObservacoesInternas { get; set; }

        [Display(Name = "Comissão Prevista")]
        [Column(TypeName = "decimal(18,2)")]
        public decimal? ValorComissao { get; set; }

        [Display(Name = "Data de Registo")]
        public DateTime DataRegisto { get; set; } = DateTime.Now;

        // --- CHAVES ESTRANGEIRAS ---
        [Display(Name = "Categoria")]
        public int? CategoriaImovelId { get; set; }
        public virtual CategoriaImovel? CategoriaImovel { get; set; }

        [Display(Name = "Tipo de Negócio")]
        public int? TipoNegocioId { get; set; }
        public virtual TipoNegocio? TipoNegocio { get; set; }

        [Display(Name = "Estado")]
        public int? EstadoImovelId { get; set; }
        public virtual EstadoImovel? EstadoImovel { get; set; }

        [Display(Name = "Status")]
        public int? StatusImovelId { get; set; }
        public virtual StatusImovel? StatusImovel { get; set; }

        [Display(Name = "Certificado Energético")]
        public int? CertificadoEnergeticoId { get; set; }
        public virtual CertificadoEnergetico? CertificadoEnergetico { get; set; }

        [Display(Name = "Consultor Responsável")]
        public int? ConsultorId { get; set; }
        public virtual Consultor? Consultor { get; set; }

        // --- LISTAS DE LIGAÇÃO ---
        public virtual ICollection<FotoImovel> Fotos { get; set; } = new List<FotoImovel>();
        public virtual ICollection<ImovelCaracteristica> Caracteristicas { get; set; } = new List<ImovelCaracteristica>();
        public virtual ICollection<ImovelProprietario> Proprietarios { get; set; } = new List<ImovelProprietario>();
        public virtual ICollection<ImovelFavorito> Favoritos { get; set; } = new List<ImovelFavorito>();
        public virtual ICollection<DocumentoImovel> Documentos { get; set; } = new List<DocumentoImovel>();
    }
}