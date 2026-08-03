namespace RPImobiliaria.Models
{
    public class Concelho
    {
        public int Id { get; set; }
        public string Nome { get; set; }
        public int DistritoId { get; set; }
        public virtual Distrito Distrito { get; set; }
        public virtual ICollection<Freguesia> Freguesias { get; set; } = new List<Freguesia>();
    }
}
