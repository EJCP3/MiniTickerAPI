using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace MiniTicker.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AgregarPrefijoArea : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "TipoComentario",
                table: "TicketEvents");

            migrationBuilder.DropColumn(
                name: "VisibleParaSolicitante",
                table: "TicketEvents");

            migrationBuilder.DropColumn(
                name: "VisibleSoloGestores",
                table: "TicketEvents");

            migrationBuilder.DropColumn(
                name: "FechaCreacion",
                table: "Areas");

            migrationBuilder.DropColumn(
                name: "FechaModificacion",
                table: "Areas");

            migrationBuilder.AddColumn<Guid>(
                name: "AreaId1",
                table: "TipoSolicitudes",
                type: "uniqueidentifier",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"));

            migrationBuilder.AddColumn<Guid>(
                name: "AreaId1",
                table: "Tickets",
                type: "uniqueidentifier",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Prefijo",
                table: "Areas",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.CreateIndex(
                name: "IX_TipoSolicitudes_AreaId1",
                table: "TipoSolicitudes",
                column: "AreaId1");

            migrationBuilder.CreateIndex(
                name: "IX_Tickets_AreaId1",
                table: "Tickets",
                column: "AreaId1");

            migrationBuilder.CreateIndex(
                name: "IX_TicketEvents_TicketId",
                table: "TicketEvents",
                column: "TicketId");

            migrationBuilder.CreateIndex(
                name: "IX_TicketEvents_UsuarioId",
                table: "TicketEvents",
                column: "UsuarioId");

            migrationBuilder.AddForeignKey(
                name: "FK_TicketEvents_Tickets_TicketId",
                table: "TicketEvents",
                column: "TicketId",
                principalTable: "Tickets",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_TicketEvents_Usuarios_UsuarioId",
                table: "TicketEvents",
                column: "UsuarioId",
                principalTable: "Usuarios",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_Tickets_Areas_AreaId1",
                table: "Tickets",
                column: "AreaId1",
                principalTable: "Areas",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_TipoSolicitudes_Areas_AreaId1",
                table: "TipoSolicitudes",
                column: "AreaId1",
                principalTable: "Areas",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_TicketEvents_Tickets_TicketId",
                table: "TicketEvents");

            migrationBuilder.DropForeignKey(
                name: "FK_TicketEvents_Usuarios_UsuarioId",
                table: "TicketEvents");

            migrationBuilder.DropForeignKey(
                name: "FK_Tickets_Areas_AreaId1",
                table: "Tickets");

            migrationBuilder.DropForeignKey(
                name: "FK_TipoSolicitudes_Areas_AreaId1",
                table: "TipoSolicitudes");

            migrationBuilder.DropIndex(
                name: "IX_TipoSolicitudes_AreaId1",
                table: "TipoSolicitudes");

            migrationBuilder.DropIndex(
                name: "IX_Tickets_AreaId1",
                table: "Tickets");

            migrationBuilder.DropIndex(
                name: "IX_TicketEvents_TicketId",
                table: "TicketEvents");

            migrationBuilder.DropIndex(
                name: "IX_TicketEvents_UsuarioId",
                table: "TicketEvents");

            migrationBuilder.DropColumn(
                name: "AreaId1",
                table: "TipoSolicitudes");

            migrationBuilder.DropColumn(
                name: "AreaId1",
                table: "Tickets");

            migrationBuilder.DropColumn(
                name: "Prefijo",
                table: "Areas");

            migrationBuilder.AddColumn<int>(
                name: "TipoComentario",
                table: "TicketEvents",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "VisibleParaSolicitante",
                table: "TicketEvents",
                type: "bit",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "VisibleSoloGestores",
                table: "TicketEvents",
                type: "bit",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "FechaCreacion",
                table: "Areas",
                type: "datetime2",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.AddColumn<DateTime>(
                name: "FechaModificacion",
                table: "Areas",
                type: "datetime2",
                nullable: true);
        }
    }
}
