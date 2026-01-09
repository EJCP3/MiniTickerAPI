using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace MiniTicker.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AjusteRelaciones : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_TicketEvents_Usuarios_UsuarioId",
                table: "TicketEvents");

            migrationBuilder.DropForeignKey(
                name: "FK_TipoSolicitudes_Areas_AreaId1",
                table: "TipoSolicitudes");

            migrationBuilder.DropIndex(
                name: "IX_TipoSolicitudes_AreaId1",
                table: "TipoSolicitudes");

            migrationBuilder.DropColumn(
                name: "AreaId1",
                table: "TipoSolicitudes");

            migrationBuilder.AlterColumn<string>(
                name: "TipoEvento",
                table: "TicketEvents",
                type: "nvarchar(50)",
                maxLength: 50,
                nullable: false,
                oldClrType: typeof(int),
                oldType: "int");

            migrationBuilder.AlterColumn<string>(
                name: "Texto",
                table: "TicketEvents",
                type: "nvarchar(1000)",
                maxLength: 1000,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)",
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "EstadoNuevo",
                table: "TicketEvents",
                type: "nvarchar(50)",
                maxLength: 50,
                nullable: true,
                oldClrType: typeof(int),
                oldType: "int",
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "EstadoAnterior",
                table: "TicketEvents",
                type: "nvarchar(50)",
                maxLength: 50,
                nullable: true,
                oldClrType: typeof(int),
                oldType: "int",
                oldNullable: true);

            migrationBuilder.AddForeignKey(
                name: "FK_TicketEvents_Usuarios_UsuarioId",
                table: "TicketEvents",
                column: "UsuarioId",
                principalTable: "Usuarios",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_TicketEvents_Usuarios_UsuarioId",
                table: "TicketEvents");

            migrationBuilder.AddColumn<Guid>(
                name: "AreaId1",
                table: "TipoSolicitudes",
                type: "uniqueidentifier",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"));

            migrationBuilder.AlterColumn<int>(
                name: "TipoEvento",
                table: "TicketEvents",
                type: "int",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(50)",
                oldMaxLength: 50);

            migrationBuilder.AlterColumn<string>(
                name: "Texto",
                table: "TicketEvents",
                type: "nvarchar(max)",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(1000)",
                oldMaxLength: 1000,
                oldNullable: true);

            migrationBuilder.AlterColumn<int>(
                name: "EstadoNuevo",
                table: "TicketEvents",
                type: "int",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(50)",
                oldMaxLength: 50,
                oldNullable: true);

            migrationBuilder.AlterColumn<int>(
                name: "EstadoAnterior",
                table: "TicketEvents",
                type: "int",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(50)",
                oldMaxLength: 50,
                oldNullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_TipoSolicitudes_AreaId1",
                table: "TipoSolicitudes",
                column: "AreaId1");

            migrationBuilder.AddForeignKey(
                name: "FK_TicketEvents_Usuarios_UsuarioId",
                table: "TicketEvents",
                column: "UsuarioId",
                principalTable: "Usuarios",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_TipoSolicitudes_Areas_AreaId1",
                table: "TipoSolicitudes",
                column: "AreaId1",
                principalTable: "Areas",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
