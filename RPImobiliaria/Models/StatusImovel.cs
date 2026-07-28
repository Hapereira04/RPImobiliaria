using System.ComponentModel.DataAnnotations;

namespace RPImobiliaria.Models
{
    public class StatusImovel
    {
        public int Id { get; set; }

        [Required(ErrorMessage = "Campo obrigatório")]
        [Display(Name = "Status no Sistema")]
        public string Nome { get; set; } // Ex: Disponível, Reservado

        public virtual ICollection<Imovel> Imoveis { get; set; } = new List<Imovel>();
    }
}