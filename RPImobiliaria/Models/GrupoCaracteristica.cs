using System.ComponentModel.DataAnnotations;

namespace RPImobiliaria.Models
{
    public class GrupoCaracteristica
    {
        public int Id { get; set; }

        [Required]
        [Display(Name = "Nome do Grupo")]
        public string Nome { get; set; } // Ex: "Climatização", "Edifício", "Zona"

        // Relação 1:N (Um grupo tem várias características)
        public virtual ICollection<Caracteristica> Caracteristicas { get; set; } = new List<Caracteristica>();
    }
}