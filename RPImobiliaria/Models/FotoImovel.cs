using System.ComponentModel.DataAnnotations;

namespace RPImobiliaria.Models
{
    public class FotoImovel
    {
        public int Id { get; set; }

        [Required]
        public string CaminhoImagem { get; set; }

        // --- DRAG & DROP ESTÁ AQUI ---
        [Display(Name = "Ordem de Apresentação")]
        public int Ordem { get; set; }

        // --- LIGAÇÃO AO IMÓVEL ---
        public int ImovelId { get; set; }
        public virtual Imovel? Imovel { get; set; }
    }
}