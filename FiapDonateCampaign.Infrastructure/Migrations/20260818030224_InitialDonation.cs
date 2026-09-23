using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace FiapDonateCampaign.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class InitialDonation : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropPrimaryKey(
                name: "PK_Doacoes",
                table: "Doacoes");

            migrationBuilder.RenameTable(
                name: "Doacoes",
                newName: "Donation");

            migrationBuilder.RenameIndex(
                name: "IX_Doacoes_CampanhaId",
                table: "Donation",
                newName: "IX_Donation_CampanhaId");

            migrationBuilder.AddPrimaryKey(
                name: "PK_Donation",
                table: "Donation",
                column: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropPrimaryKey(
                name: "PK_Donation",
                table: "Donation");

            migrationBuilder.RenameTable(
                name: "Donation",
                newName: "Doacoes");

            migrationBuilder.RenameIndex(
                name: "IX_Donation_CampanhaId",
                table: "Doacoes",
                newName: "IX_Doacoes_CampanhaId");

            migrationBuilder.AddPrimaryKey(
                name: "PK_Doacoes",
                table: "Doacoes",
                column: "Id");
        }
    }
}
