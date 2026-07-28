using System.ComponentModel.DataAnnotations;

namespace RPImobiliaria.Models
{
    public class CertificadoEnergetico
    {
        public int Id { get; set; }

        [Required(ErrorMessage = "Campo obrigatório")]
        [Display(Name = "Certificado Energético")]
        public string Nome { get; set; } // Ex: A+, A, B...

        public virtual ICollection<Imovel> Imoveis { get; set; } = new List<Imovel>();
    }
}