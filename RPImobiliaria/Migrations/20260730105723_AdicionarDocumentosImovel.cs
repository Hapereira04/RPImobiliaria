using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace RPImobiliaria.Migrations
{
    /// <inheritdoc />
    public partial class AdicionarDocumentosImovel : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Concelho",
                table: "Imoveis");

            migrationBuilder.DropColumn(
                name: "Distrito",
                table: "Imoveis");

            migrationBuilder.DropColumn(
                name: "Freguesia",
                table: "Imoveis");

            migrationBuilder.AddColumn<int>(
                name: "FreguesiaId",
                table: "Imoveis",
                type: "int",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "Distritos",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Nome = table.Column<string>(type: "nvarchar(max)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Distritos", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Documentos",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    ImovelId = table.Column<int>(type: "int", nullable: false),
                    NomeFicheiro = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    CaminhoFicheiro = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    DataUpload = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Documentos", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Documentos_Imoveis_ImovelId",
                        column: x => x.ImovelId,
                        principalTable: "Imoveis",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Concelhos",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Nome = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    DistritoId = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Concelhos", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Concelhos_Distritos_DistritoId",
                        column: x => x.DistritoId,
                        principalTable: "Distritos",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Freguesias",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Nome = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    ConcelhoId = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Freguesias", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Freguesias_Concelhos_ConcelhoId",
                        column: x => x.ConcelhoId,
                        principalTable: "Concelhos",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Imoveis_FreguesiaId",
                table: "Imoveis",
                column: "FreguesiaId");

            migrationBuilder.CreateIndex(
                name: "IX_Concelhos_DistritoId",
                table: "Concelhos",
                column: "DistritoId");

            migrationBuilder.CreateIndex(
                name: "IX_Documentos_ImovelId",
                table: "Documentos",
                column: "ImovelId");

            migrationBuilder.CreateIndex(
                name: "IX_Freguesias_ConcelhoId",
                table: "Freguesias",
                column: "ConcelhoId");

            migrationBuilder.AddForeignKey(
                name: "FK_Imoveis_Freguesias_FreguesiaId",
                table: "Imoveis",
                column: "FreguesiaId",
                principalTable: "Freguesias",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Imoveis_Freguesias_FreguesiaId",
                table: "Imoveis");

            migrationBuilder.DropTable(
                name: "Documentos");

            migrationBuilder.DropTable(
                name: "Freguesias");

            migrationBuilder.DropTable(
                name: "Concelhos");

            migrationBuilder.DropTable(
                name: "Distritos");

            migrationBuilder.DropIndex(
                name: "IX_Imoveis_FreguesiaId",
                table: "Imoveis");

            migrationBuilder.DropColumn(
                name: "FreguesiaId",
                table: "Imoveis");

            migrationBuilder.AddColumn<string>(
                name: "Concelho",
                table: "Imoveis",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Distrito",
                table: "Imoveis",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Freguesia",
                table: "Imoveis",
                type: "nvarchar(max)",
                nullable: true);
        }
    }
}
