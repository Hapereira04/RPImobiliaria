namespace RPImobiliaria.Models
{
    public class ImovelCaracteristica
    {
        public int ImovelId { get; set; }
        public virtual Imovel Imovel { get; set; }

        public int CaracteristicaId { get; set; }
        public virtual Caracteristica Caracteristica { get; set; }
    }
}