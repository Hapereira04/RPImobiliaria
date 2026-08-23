using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace RPImobiliaria.Migrations
{
    /// <inheritdoc />
    public partial class LocalizacaoParcial : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "ConcelhoId",
                table: "Imoveis",
                type: "INTEGER",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "DistritoId",
                table: "Imoveis",
                type: "INTEGER",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_Imoveis_ConcelhoId",
                table: "Imoveis",
                column: "ConcelhoId");

            migrationBuilder.CreateIndex(
                name: "IX_Imoveis_DistritoId",
                table: "Imoveis",
                column: "DistritoId");

            migrationBuilder.AddForeignKey(
                name: "FK_Imoveis_Concelhos_ConcelhoId",
                table: "Imoveis",
                column: "ConcelhoId",
                principalTable: "Concelhos",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_Imoveis_Distritos_DistritoId",
                table: "Imoveis",
                column: "DistritoId",
                principalTable: "Distritos",
                principalColumn: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Imoveis_Concelhos_ConcelhoId",
                table: "Imoveis");

            migrationBuilder.DropForeignKey(
                name: "FK_Imoveis_Distritos_DistritoId",
                table: "Imoveis");

            migrationBuilder.DropIndex(
                name: "IX_Imoveis_ConcelhoId",
                table: "Imoveis");

            migrationBuilder.DropIndex(
                name: "IX_Imoveis_DistritoId",
                table: "Imoveis");

            migrationBuilder.DropColumn(
                name: "ConcelhoId",
                table: "Imoveis");

            migrationBuilder.DropColumn(
                name: "DistritoId",
                table: "Imoveis");
        }
    }
}
