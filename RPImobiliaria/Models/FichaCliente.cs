using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace RPImobiliaria.Models
{
    public class FichaCliente
    {
        public int Id { get; set; }

        // Ligação 1:1 com o Cliente
        public int ClienteId { get; set; }
        public virtual Cliente? Cliente { get; set; }

        [Display(Name = "Procura Comprar")]
        public bool PerfilComprador { get; set; }

        [Display(Name = "Quer Vender")]
        public bool PerfilVendedor { get; set; }

        [Display(Name = "Procura Arrendar")]
        public bool PerfilArrendatario { get; set; }

        [Display(Name = "Investidor")]
        public bool PerfilInvestidor { get; set; }

        [DataType(DataType.Currency)]
        [Display(Name = "Orçamento Máximo")]
        public decimal? OrcamentoMaximo { get; set; }

        [Display(Name = "Zonas de Preferência")]
        public string? ZonasPreferencia { get; set; }

        [Display(Name = "Tipologias Procuradas")]
        public string? TipologiasProcuradas { get; set; }

        [DataType(DataType.MultilineText)]
        [Display(Name = "Notas de Qualificação / Requisitos Extra")]
        public string? NotasRequisitos { get; set; }

        [Display(Name = "Última Atualização")]
        public DateTime UltimaAtualizacao { get; set; } = DateTime.Now;

        [Display(Name = "Preenchido pelo Próprio?")]
        public bool PreenchidoPeloCliente { get; set; } = false;
    }
}