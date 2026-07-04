using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace MeridianEmployeeHub.Data.Context.Migrations
{
    /// <inheritdoc />
    public partial class FixLeaveBalanceDeleteBehavior : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_LeaveBalances_Employees_EmployeeId",
                table: "LeaveBalances");

            migrationBuilder.AddForeignKey(
                name: "FK_LeaveBalances_Employees_EmployeeId",
                table: "LeaveBalances",
                column: "EmployeeId",
                principalTable: "Employees",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_LeaveBalances_Employees_EmployeeId",
                table: "LeaveBalances");

            migrationBuilder.AddForeignKey(
                name: "FK_LeaveBalances_Employees_EmployeeId",
                table: "LeaveBalances",
                column: "EmployeeId",
                principalTable: "Employees",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
