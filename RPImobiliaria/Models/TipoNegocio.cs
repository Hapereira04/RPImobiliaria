using System.ComponentModel.DataAnnotations;

namespace RPImobiliaria.Models
{
    public class TipoNegocio
    {
        public int Id { get; set; }

        [Required(ErrorMessage = "Campo obrigatório")]
        [Display(Name = "Tipo de Negócio")]
        public string Nome { get; set; } // Ex: Venda, Arrendamento

        public virtual ICollection<Imovel> Imoveis { get; set; } = new List<Imovel>();
    }
}