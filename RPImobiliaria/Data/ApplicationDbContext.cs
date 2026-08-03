using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using RPImobiliaria.Models;

namespace RPImobiliaria.Data
{
    public class ApplicationDbContext : IdentityDbContext
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options)
        {
        }

        // --- AS NOSSAS TABELAS PRINCIPAIS ---
        public DbSet<Imovel> Imoveis { get; set; }
        public DbSet<Cliente> Clientes { get; set; }
        public DbSet<Consultor> Consultores { get; set; }
        public DbSet<FotoImovel> Fotos { get; set; }

        // --- OS NOSSOS CATÁLOGOS (Dropdowns e Agrupamentos) ---
        public DbSet<TipoNegocio> TiposNegocio { get; set; }
        public DbSet<EstadoImovel> EstadosImovel { get; set; }
        public DbSet<StatusImovel> StatusImoveis { get; set; }
        public DbSet<CategoriaImovel> CategoriasImovel { get; set; }
        public DbSet<CertificadoEnergetico> CertificadosEnergeticos { get; set; }
        public DbSet<FichaCliente> FichasClientes { get; set; }
        public DbSet<DocumentoImovel> Documentos { get; set; }

        // As Características agora estão divididas em Grupos!
        public DbSet<GrupoCaracteristica> GruposCaracteristicas { get; set; }
        public DbSet<Caracteristica> CaracteristicasCatalogo { get; set; }

        // --- LOCALIZAÇÃO ---
        public DbSet<Distrito> Distritos { get; set; }
        public DbSet<Concelho> Concelhos { get; set; }
        public DbSet<Freguesia> Freguesias { get; set; }

        // --- TABELAS DE LIGAÇÃO (N:N) ---
        public DbSet<ImovelCaracteristica> ImoveisCaracteristicas { get; set; }
        public DbSet<ImovelProprietario> ImoveisProprietarios { get; set; }
        public DbSet<ImovelFavorito> ImoveisFavoritos { get; set; }

        protected override void OnModelCreating(ModelBuilder builder)
        {
            base.OnModelCreating(builder);

            // 1. Configurar Chaves Compostas das tabelas de ligação (N:N)
            builder.Entity<ImovelCaracteristica>().HasKey(ic => new { ic.ImovelId, ic.CaracteristicaId });
            builder.Entity<ImovelProprietario>().HasKey(ip => new { ip.ImovelId, ip.ClienteId });
            builder.Entity<ImovelFavorito>().HasKey(f => new { f.ImovelId, f.ClienteId });

            // 2. PROTEÇÃO DE DADOS (Impedir apagar categorias se estiverem em uso)
            builder.Entity<Imovel>().HasOne(i => i.TipoNegocio).WithMany(t => t.Imoveis).HasForeignKey(i => i.TipoNegocioId).OnDelete(DeleteBehavior.Restrict);
            builder.Entity<Imovel>().HasOne(i => i.EstadoImovel).WithMany(e => e.Imoveis).HasForeignKey(i => i.EstadoImovelId).OnDelete(DeleteBehavior.Restrict);
            builder.Entity<Imovel>().HasOne(i => i.StatusImovel).WithMany(s => s.Imoveis).HasForeignKey(i => i.StatusImovelId).OnDelete(DeleteBehavior.Restrict);
            builder.Entity<Imovel>().HasOne(i => i.CategoriaImovel).WithMany(c => c.Imoveis).HasForeignKey(i => i.CategoriaImovelId).OnDelete(DeleteBehavior.Restrict);
            builder.Entity<Imovel>().HasOne(i => i.CertificadoEnergetico).WithMany(c => c.Imoveis).HasForeignKey(i => i.CertificadoEnergeticoId).OnDelete(DeleteBehavior.Restrict);
            builder.Entity<ImovelCaracteristica>().HasOne(ic => ic.Caracteristica).WithMany(c => c.ImoveisCaracteristicas).HasForeignKey(ic => ic.CaracteristicaId).OnDelete(DeleteBehavior.Restrict);

            // Proteger apagar Freguesias que já tenham imóveis
            builder.Entity<Imovel>().HasOne(i => i.Freguesia).WithMany().HasForeignKey(i => i.FreguesiaId).OnDelete(DeleteBehavior.Restrict);

            // Impedir apagar um Grupo se ele tiver características lá dentro
            builder.Entity<Caracteristica>().HasOne(c => c.GrupoCaracteristica).WithMany(g => g.Caracteristicas).HasForeignKey(c => c.GrupoCaracteristicaId).OnDelete(DeleteBehavior.Restrict);

            // 3. Formatação de Moeda e Decimais (Para não dar erro no SQL Server)
            builder.Entity<Imovel>().Property(p => p.Preco).HasColumnType("decimal(18,2)");
            builder.Entity<Imovel>().Property(p => p.ValorComissao).HasColumnType("decimal(18,2)");

            builder.Entity<Cliente>()
                .HasOne(c => c.Ficha)
                .WithOne(f => f.Cliente)
                .HasForeignKey<FichaCliente>(f => f.ClienteId)
                .OnDelete(DeleteBehavior.Cascade); // Se apagar o cliente, apaga a ficha dele automaticamente

            builder.Entity<FichaCliente>()
                .Property(f => f.OrcamentoMaximo)
                .HasColumnType("decimal(18,2)");
        }
    }
}