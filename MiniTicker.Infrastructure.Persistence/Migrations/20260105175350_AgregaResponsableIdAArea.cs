using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace MiniTicker.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AgregaResponsableIdAArea : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<Guid>(
                name: "ResponsableId",
                table: "Areas",
                type: "uniqueidentifier",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "ResponsableId",
                table: "Areas");
        }
    }
}
