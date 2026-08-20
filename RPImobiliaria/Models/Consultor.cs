using System.ComponentModel.DataAnnotations;

namespace RPImobiliaria.Models
{
    public class Consultor
    {
        public int Id { get; set; }

        [Required(ErrorMessage = "O Nome é obrigatório.")]
        public string Nome { get; set; }

        [Required(ErrorMessage = "O Email é obrigatório para o acesso ao sistema.")]
        [EmailAddress(ErrorMessage = "Formato de email inválido.")]
        public string Email { get; set; }

        public string? Telemovel { get; set; }

        // Ligação à tabela de Logins do ASP.NET
        public string? ApplicationUserId { get; set; }

        public virtual ICollection<Imovel> ImoveisAngariados { get; set; } = new List<Imovel>();
    }
}