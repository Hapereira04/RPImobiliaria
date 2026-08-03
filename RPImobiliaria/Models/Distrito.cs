namespace RPImobiliaria.Models
{
    public class Distrito
    {
        public int Id { get; set; }
        public string Nome { get; set; }
        public virtual ICollection<Concelho> Concelhos { get; set; } = new List<Concelho>();
    }
}
