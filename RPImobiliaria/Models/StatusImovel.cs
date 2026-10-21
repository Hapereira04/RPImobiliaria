using System.ComponentModel.DataAnnotations;

namespace RPImobiliaria.Models
{
    public class StatusImovel
    {
        public int Id { get; set; }

        [Required(ErrorMessage = "O nome do estado é obrigatório.")]
        [StringLength(100)]
        [Display(Name = "Nome do Estado")]
        public string Nome { get; set; }

        [Display(Name = "Apresentar Etiqueta / Faixa")]
        public bool TemEtiqueta { get; set; } = false;

        [Display(Name = "Cor da Etiqueta")]
        [StringLength(9)]
        public string? CorHex { get; set; } = "#fd7e14"; // Laranja predefinido

        // Lista de navegação para imóveis com este estado
        public virtual ICollection<Imovel> Imoveis { get; set; } = new List<Imovel>();
    }
}