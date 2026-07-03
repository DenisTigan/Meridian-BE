using System;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace MeridianEmployeeHub.Data.Context.Migrations
{
    /// <inheritdoc />
    public partial class AddBuddySystem : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "BuddyAssignments",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    NewEmployeeId = table.Column<int>(type: "int", nullable: false),
                    BuddyId = table.Column<int>(type: "int", nullable: false),
                    AssignedAt = table.Column<DateTime>(type: "datetime(6)", nullable: false),
                    Notes = table.Column<string>(type: "longtext", nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    Status = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_BuddyAssignments", x => x.Id);
                    table.ForeignKey(
                        name: "FK_BuddyAssignments_Employees_BuddyId",
                        column: x => x.BuddyId,
                        principalTable: "Employees",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_BuddyAssignments_Employees_NewEmployeeId",
                        column: x => x.NewEmployeeId,
                        principalTable: "Employees",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateIndex(
                name: "IX_BuddyAssignments_BuddyId",
                table: "BuddyAssignments",
                column: "BuddyId");

            migrationBuilder.CreateIndex(
                name: "IX_BuddyAssignments_NewEmployeeId",
                table: "BuddyAssignments",
                column: "NewEmployeeId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "BuddyAssignments");
        }
    }
}
