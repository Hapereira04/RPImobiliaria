using System.ComponentModel.DataAnnotations;

namespace RPImobiliaria.Models
{
    public class ImovelFavorito
    {
        public int ImovelId { get; set; }
        public virtual Imovel? Imovel { get; set; }

        public int ClienteId { get; set; }
        public virtual Cliente? Cliente { get; set; }

        [Display(Name = "Adicionado a")]
        public DateTime DataAdicionado { get; set; } = DateTime.Now;
    }
}