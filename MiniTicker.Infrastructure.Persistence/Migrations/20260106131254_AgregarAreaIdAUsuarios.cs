using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace MiniTicker.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AgregarAreaIdAUsuarios : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Tickets_Areas_AreaId1",
                table: "Tickets");

            migrationBuilder.DropIndex(
                name: "IX_Tickets_AreaId1",
                table: "Tickets");

            migrationBuilder.DropColumn(
                name: "AreaId1",
                table: "Tickets");

            migrationBuilder.CreateIndex(
                name: "IX_Usuarios_AreaId",
                table: "Usuarios",
                column: "AreaId");

            migrationBuilder.AddForeignKey(
                name: "FK_Usuarios_Areas_AreaId",
                table: "Usuarios",
                column: "AreaId",
                principalTable: "Areas",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Usuarios_Areas_AreaId",
                table: "Usuarios");

            migrationBuilder.DropIndex(
                name: "IX_Usuarios_AreaId",
                table: "Usuarios");

            migrationBuilder.AddColumn<Guid>(
                name: "AreaId1",
                table: "Tickets",
                type: "uniqueidentifier",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_Tickets_AreaId1",
                table: "Tickets",
                column: "AreaId1");

            migrationBuilder.AddForeignKey(
                name: "FK_Tickets_Areas_AreaId1",
                table: "Tickets",
                column: "AreaId1",
                principalTable: "Areas",
                principalColumn: "Id");
        }
    }
}
