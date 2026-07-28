using System.ComponentModel.DataAnnotations;

namespace RPImobiliaria.Models
{
    public class Cliente
    {
        public int Id { get; set; }

        [Required]
        public string Nome { get; set; }
        public string? Email { get; set; }
        public string? Telemovel { get; set; }
        public string? NIF { get; set; }

        // Ligação à tabela de Logins do ASP.NET (Pode ser nulo se for só um cliente registado em papel)
        public string? IdentityUserId { get; set; }

        [Display(Name = "Foto de Perfil")]
        public string? CaminhoFotoPerfil { get; set; }

        // Relações
        public virtual FichaCliente? Ficha { get; set; }
        public virtual ICollection<ImovelProprietario> ImoveisPropriedade { get; set; } = new List<ImovelProprietario>();
        public virtual ICollection<ImovelFavorito> Favoritos { get; set; } = new List<ImovelFavorito>();
    }
}