using System.ComponentModel.DataAnnotations;

namespace RPImobiliaria.Models
{
    public class Caracteristica
    {
        public int Id { get; set; }

        [Required]
        [Display(Name = "Característica")]
        public string Nome { get; set; } // Ex: "Ar Condicionado", "Elevador"

        // --- LIGAÇÃO AO GRUPO ---
        [Display(Name = "Grupo de Característica")]
        public int GrupoCaracteristicaId { get; set; }
        public virtual GrupoCaracteristica? GrupoCaracteristica { get; set; }

        // Relação N:N com os Imóveis
        public virtual ICollection<ImovelCaracteristica> ImoveisCaracteristicas { get; set; } = new List<ImovelCaracteristica>();
    }
}