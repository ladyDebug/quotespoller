using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace BlockchainApp.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class InitialCreate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "BlockchainData",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    Json = table.Column<string>(type: "text", nullable: false),
                    BlockchainApi = table.Column<string>(type: "text", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_BlockchainData", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_BlockchainData_BlockchainApi",
                table: "BlockchainData",
                column: "BlockchainApi");

            migrationBuilder.CreateIndex(
                name: "IX_BlockchainData_BlockchainApi_CreatedAt",
                table: "BlockchainData",
                columns: new[] { "BlockchainApi", "CreatedAt" });

            migrationBuilder.CreateIndex(
                name: "IX_BlockchainData_CreatedAt",
                table: "BlockchainData",
                column: "CreatedAt");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "BlockchainData");
        }
    }
}
