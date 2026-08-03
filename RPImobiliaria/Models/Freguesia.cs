namespace RPImobiliaria.Models
{
    public class Freguesia
    {
        public int Id { get; set; }
        public string Nome { get; set; }
        public int ConcelhoId { get; set; }
        public virtual Concelho Concelho { get; set; }
    }
}