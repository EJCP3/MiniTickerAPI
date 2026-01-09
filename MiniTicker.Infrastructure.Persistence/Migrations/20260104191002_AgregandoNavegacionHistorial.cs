using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace MiniTicker.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AgregandoNavegacionHistorial : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<Guid>(
                name: "TicketId1",
                table: "TicketEvents",
                type: "uniqueidentifier",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "TicketId1",
                table: "Comentarios",
                type: "uniqueidentifier",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_TicketEvents_TicketId1",
                table: "TicketEvents",
                column: "TicketId1");

            migrationBuilder.CreateIndex(
                name: "IX_Comentarios_TicketId1",
                table: "Comentarios",
                column: "TicketId1");

            migrationBuilder.AddForeignKey(
                name: "FK_Comentarios_Tickets_TicketId1",
                table: "Comentarios",
                column: "TicketId1",
                principalTable: "Tickets",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_TicketEvents_Tickets_TicketId1",
                table: "TicketEvents",
                column: "TicketId1",
                principalTable: "Tickets",
                principalColumn: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Comentarios_Tickets_TicketId1",
                table: "Comentarios");

            migrationBuilder.DropForeignKey(
                name: "FK_TicketEvents_Tickets_TicketId1",
                table: "TicketEvents");

            migrationBuilder.DropIndex(
                name: "IX_TicketEvents_TicketId1",
                table: "TicketEvents");

            migrationBuilder.DropIndex(
                name: "IX_Comentarios_TicketId1",
                table: "Comentarios");

            migrationBuilder.DropColumn(
                name: "TicketId1",
                table: "TicketEvents");

            migrationBuilder.DropColumn(
                name: "TicketId1",
                table: "Comentarios");
        }
    }
}
