using Microsoft.AspNetCore.Identity;
using RPImobiliaria.Models;

namespace RPImobiliaria.Data
{
    public static class DbInitializer
    {
        public static async Task Initialize(IServiceProvider serviceProvider)
        {
            var roleManager = serviceProvider.GetRequiredService<RoleManager<IdentityRole>>();
            var userManager = serviceProvider.GetRequiredService<UserManager<IdentityUser>>();

            // 1. Criar os Cargos (Roles) se não existirem
            string[] cargos = { "Admin", "Consultor", "Cliente" };

            foreach (var cargo in cargos)
            {
                if (!await roleManager.RoleExistsAsync(cargo))
                {
                    await roleManager.CreateAsync(new IdentityRole(cargo));
                }
            }

            // 2. Criar o Utilizador "Super Admin" por defeito
            string emailAdmin = "admin@RPImobiliaria.pt";
            string passwordAdmin = "Admin123!";

            if (await userManager.FindByEmailAsync(emailAdmin) == null)
            {
                var adminUser = new IdentityUser
                {
                    UserName = emailAdmin,
                    Email = emailAdmin,
                    EmailConfirmed = true
                };

                var resultado = await userManager.CreateAsync(adminUser, passwordAdmin);

                if (resultado.Succeeded)
                {
                    await userManager.AddToRoleAsync(adminUser, "Admin");
                }
            }
        }

        public static async Task SeedCatalogoDataAsync(ApplicationDbContext context)
        {
            // 1. Tipos de Negócio
            if (!context.TiposNegocio.Any())
            {
                context.TiposNegocio.AddRange(
                    new TipoNegocio { Nome = "Comprar" },
                    new TipoNegocio { Nome = "Arrendar" },
                    new TipoNegocio { Nome = "Trespasse" }
                );
            }

            // 2. Categorias de Imóvel
            if (!context.CategoriasImovel.Any())
            {
                context.CategoriasImovel.AddRange(
                    new CategoriaImovel { Nome = "Apartamento" },
                    new CategoriaImovel { Nome = "Moradia" },
                    new CategoriaImovel { Nome = "Terreno" },
                    new CategoriaImovel { Nome = "Loja" },
                    new CategoriaImovel { Nome = "Armazém" },
                    new CategoriaImovel { Nome = "Escritório" }
                );
            }

            // 3. Certificados Energéticos
            if (!context.CertificadosEnergeticos.Any())
            {
                context.CertificadosEnergeticos.AddRange(
                    new CertificadoEnergetico { Nome = "A+" }, new CertificadoEnergetico { Nome = "A" },
                    new CertificadoEnergetico { Nome = "B" }, new CertificadoEnergetico { Nome = "B-" },
                    new CertificadoEnergetico { Nome = "C" }, new CertificadoEnergetico { Nome = "D" },
                    new CertificadoEnergetico { Nome = "E" }, new CertificadoEnergetico { Nome = "F" },
                    new CertificadoEnergetico { Nome = "Isento" }
                );
            }

            // 4. Estados do Imóvel
            if (!context.EstadosImovel.Any())
            {
                context.EstadosImovel.AddRange(
                    new EstadoImovel { Nome = "Novo" },
                    new EstadoImovel { Nome = "Como Novo" },
                    new EstadoImovel { Nome = "Renovado" },
                    new EstadoImovel { Nome = "Usado" },
                    new EstadoImovel { Nome = "Para Recuperar" },
                    new EstadoImovel { Nome = "Em Construção" }
                );
            }

            // 5. Grupos e Características 
            if (!context.GruposCaracteristicas.Any())
            {
                var climatizacao = new GrupoCaracteristica { Nome = "Climatização" };
                var edificio = new GrupoCaracteristica { Nome = "Edifício" };
                var seguranca = new GrupoCaracteristica { Nome = "Segurança e Domótica" };
                var zona = new GrupoCaracteristica { Nome = "Zona" };
                var extras = new GrupoCaracteristica { Nome = "Extras e Equipamentos" };

                context.GruposCaracteristicas.AddRange(climatizacao, edificio, seguranca, zona, extras);
                await context.SaveChangesAsync(); // Grava os grupos primeiro para ter IDs

                context.CaracteristicasCatalogo.AddRange(
                    new Caracteristica { Nome = "Ar Condicionado", GrupoCaracteristicaId = climatizacao.Id },
                    new Caracteristica { Nome = "Aquecimento Central", GrupoCaracteristicaId = climatizacao.Id },
                    new Caracteristica { Nome = "Lareira com Recuperador", GrupoCaracteristicaId = climatizacao.Id },
                    new Caracteristica { Nome = "Piso Radiante", GrupoCaracteristicaId = climatizacao.Id },

                    new Caracteristica { Nome = "Elevador", GrupoCaracteristicaId = edificio.Id },
                    new Caracteristica { Nome = "Acesso para Deficientes", GrupoCaracteristicaId = edificio.Id },
                    new Caracteristica { Nome = "Isolamento Térmico/Acústico", GrupoCaracteristicaId = edificio.Id },
                    new Caracteristica { Nome = "Gás Canalizado", GrupoCaracteristicaId = edificio.Id },

                    new Caracteristica { Nome = "Alarme", GrupoCaracteristicaId = seguranca.Id },
                    new Caracteristica { Nome = "Vídeo Porteiro", GrupoCaracteristicaId = seguranca.Id },
                    new Caracteristica { Nome = "Porta Blindada", GrupoCaracteristicaId = seguranca.Id },
                    new Caracteristica { Nome = "CCTV", GrupoCaracteristicaId = seguranca.Id },

                    new Caracteristica { Nome = "Perto de Transportes Públicos", GrupoCaracteristicaId = zona.Id },
                    new Caracteristica { Nome = "Perto de Escolas", GrupoCaracteristicaId = zona.Id },
                    new Caracteristica { Nome = "Vista de Cidade", GrupoCaracteristicaId = zona.Id },
                    new Caracteristica { Nome = "Vista de Mar/Rio", GrupoCaracteristicaId = zona.Id },

                    new Caracteristica { Nome = "Piscina", GrupoCaracteristicaId = extras.Id },
                    new Caracteristica { Nome = "Cozinha Equipada", GrupoCaracteristicaId = extras.Id },
                    new Caracteristica { Nome = "Estores Elétricos", GrupoCaracteristicaId = extras.Id },
                    new Caracteristica { Nome = "Painéis Solares", GrupoCaracteristicaId = extras.Id },
                    new Caracteristica { Nome = "Ginásio", GrupoCaracteristicaId = extras.Id }
                );
            }

            // 6. Distritos
            if (!context.Distritos.Any())
            {
                context.Distritos.Add(new Distrito { Nome = "Aveiro" });
                await context.SaveChangesAsync();
            }

            // 7. Concelhos
            if (!context.Concelhos.Any())
            {
                var aveiro = context.Distritos.FirstOrDefault(d => d.Nome == "Aveiro");
                if (aveiro != null)
                {
                    context.Concelhos.AddRange(
                        new Concelho { Nome = "Ovar", DistritoId = aveiro.Id },
                        new Concelho { Nome = "Espinho", DistritoId = aveiro.Id },
                        new Concelho { Nome = "Santa Maria da Feira", DistritoId = aveiro.Id }
                    );
                    await context.SaveChangesAsync();
                }
            }

            // 8. Freguesias (Focadas em Ovar)
            if (!context.Freguesias.Any())
            {
                var ovar = context.Concelhos.FirstOrDefault(c => c.Nome == "Ovar");
                if (ovar != null)
                {
                    context.Freguesias.AddRange(
                        new Freguesia { Nome = "União das Freguesias de Ovar, São João, Arada e São Vicente de Pereira Jusã", ConcelhoId = ovar.Id },
                        new Freguesia { Nome = "Esmoriz", ConcelhoId = ovar.Id },
                        new Freguesia { Nome = "Cortegaça", ConcelhoId = ovar.Id },
                        new Freguesia { Nome = "Maceda", ConcelhoId = ovar.Id },
                        new Freguesia { Nome = "Válega", ConcelhoId = ovar.Id }
                    );
                }
            }

            await context.SaveChangesAsync();
        }
    }
}