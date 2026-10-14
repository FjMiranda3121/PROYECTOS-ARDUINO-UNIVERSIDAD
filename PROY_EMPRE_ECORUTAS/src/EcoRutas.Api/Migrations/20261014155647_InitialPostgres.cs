using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace EcoRutas.Api.Migrations
{
    /// <inheritdoc />
    public partial class InitialPostgres : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Canecas",
                columns: table => new
                {
                    Id = table.Column<string>(type: "text", nullable: false),
                    Ubicacion = table.Column<string>(type: "text", nullable: false),
                    ProfundidadMaxCm = table.Column<double>(type: "double precision", nullable: false),
                    DistanciaCriticaCm = table.Column<double>(type: "double precision", nullable: false),
                    EstadoActual = table.Column<string>(type: "text", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Canecas", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Mediciones",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    CanecaId = table.Column<string>(type: "text", nullable: false),
                    DistanciaCm = table.Column<double>(type: "double precision", nullable: false),
                    PorcentajeLlenado = table.Column<int>(type: "integer", nullable: false),
                    EstadoClasificado = table.Column<string>(type: "text", nullable: false),
                    FechaLectura = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Mediciones", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Mediciones_Canecas_CanecaId",
                        column: x => x.CanecaId,
                        principalTable: "Canecas",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Mediciones_CanecaId",
                table: "Mediciones",
                column: "CanecaId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Mediciones");

            migrationBuilder.DropTable(
                name: "Canecas");
        }
    }
}
