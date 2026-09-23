using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace FiapDonateCampaign.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class InitialCampanhas : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Campaigns",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Titulo = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    Descricao = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: false),
                    DataInicio = table.Column<DateTime>(type: "datetime2", nullable: false),
                    DataFim = table.Column<DateTime>(type: "datetime2", nullable: false),
                    MetaFinanceira = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    ValorArrecadado = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    Status = table.Column<string>(type: "nvarchar(max)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Campaigns", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Doacoes",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    CampanhaId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    DoadorId = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    ValorDoacao = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    DataDoacao = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Doacoes", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Doacoes_CampanhaId",
                table: "Doacoes",
                column: "CampanhaId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Campaigns");

            migrationBuilder.DropTable(
                name: "Doacoes");
        }
    }
}
