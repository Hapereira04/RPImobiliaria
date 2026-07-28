using System.ComponentModel.DataAnnotations;

namespace RPImobiliaria.Models
{
    public class CategoriaImovel
    {
        public int Id { get; set; }

        [Required(ErrorMessage = "Campo obrigatório")]
        [Display(Name = "Categoria")]
        public string Nome { get; set; } // Ex: Apartamento, Moradia

        public virtual ICollection<Imovel> Imoveis { get; set; } = new List<Imovel>();
    }
}