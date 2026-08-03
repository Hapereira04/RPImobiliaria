using System;

namespace RPImobiliaria.Models
{
    public class DocumentoImovel
    {
        public int Id { get; set; }

        public int ImovelId { get; set; }
        public virtual Imovel Imovel { get; set; }

        public string NomeFicheiro { get; set; }
        public string CaminhoFicheiro { get; set; }
        public DateTime DataUpload { get; set; } = DateTime.Now;
    }
}