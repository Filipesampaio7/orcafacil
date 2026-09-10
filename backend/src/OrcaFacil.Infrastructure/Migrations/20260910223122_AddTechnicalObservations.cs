using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace OrcaFacil.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddTechnicalObservations : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "TechnicalObservations",
                table: "WorkOrders",
                type: "TEXT",
                maxLength: 1000,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "TechnicalObservations",
                table: "Quotes",
                type: "TEXT",
                maxLength: 1000,
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "TechnicalObservations",
                table: "WorkOrders");

            migrationBuilder.DropColumn(
                name: "TechnicalObservations",
                table: "Quotes");
        }
    }
}
