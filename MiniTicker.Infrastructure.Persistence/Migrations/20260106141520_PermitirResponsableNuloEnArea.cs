using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace MiniTicker.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class PermitirResponsableNuloEnArea : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateIndex(
                name: "IX_Areas_ResponsableId",
                table: "Areas",
                column: "ResponsableId");

            migrationBuilder.AddForeignKey(
                name: "FK_Areas_Usuarios_ResponsableId",
                table: "Areas",
                column: "ResponsableId",
                principalTable: "Usuarios",
                principalColumn: "Id",
                onDelete: ReferentialAction.SetNull);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Areas_Usuarios_ResponsableId",
                table: "Areas");

            migrationBuilder.DropIndex(
                name: "IX_Areas_ResponsableId",
                table: "Areas");
        }
    }
}
