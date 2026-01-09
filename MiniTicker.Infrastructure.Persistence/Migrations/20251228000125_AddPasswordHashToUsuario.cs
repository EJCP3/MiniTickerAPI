using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace MiniTicker.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddPasswordHashToUsuario : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "PasswordHash",
                table: "Usuarios",
                type: "nvarchar(200)",
                maxLength: 200,
                nullable: false,
                defaultValue: "");

            migrationBuilder.CreateTable(
                name: "TicketEvents",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    TicketId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    UsuarioId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    TipoEvento = table.Column<int>(type: "int", nullable: false),
                    Fecha = table.Column<DateTime>(type: "datetime2", nullable: false),
                    EstadoAnterior = table.Column<int>(type: "int", nullable: true),
                    EstadoNuevo = table.Column<int>(type: "int", nullable: true),
                    TipoComentario = table.Column<int>(type: "int", nullable: true),
                    Texto = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    VisibleParaSolicitante = table.Column<bool>(type: "bit", nullable: true),
                    VisibleSoloGestores = table.Column<bool>(type: "bit", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TicketEvents", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Tickets_Estado",
                table: "Tickets",
                column: "Estado");

            migrationBuilder.CreateIndex(
                name: "IX_Tickets_Prioridad",
                table: "Tickets",
                column: "Prioridad");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "TicketEvents");

            migrationBuilder.DropIndex(
                name: "IX_Tickets_Estado",
                table: "Tickets");

            migrationBuilder.DropIndex(
                name: "IX_Tickets_Prioridad",
                table: "Tickets");

            migrationBuilder.DropColumn(
                name: "PasswordHash",
                table: "Usuarios");
        }
    }
}
