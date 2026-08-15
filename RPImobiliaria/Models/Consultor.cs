using System.ComponentModel.DataAnnotations;

namespace RPImobiliaria.Models
{
    public class Consultor
    {
        public int Id { get; set; }

        [Required]
        public string Nome { get; set; }
        public string? Email { get; set; }
        public string? Telemovel { get; set; }
        public string? LicencaAMI { get; set; }
        [Display(Name = "Foto de Perfil")]
        public string? CaminhoFotoPerfil { get; set; }
        public string? ContentTypeFoto { get; set; }

        // Ligação à tabela de Logins do ASP.NET
        public string? ApplicationUserId { get; set; }

        public virtual ICollection<Imovel> ImoveisAngariados { get; set; } = new List<Imovel>();
    }
}