using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace HPMS.Modules.Identity.Migrations
{
    /// <inheritdoc />
    public partial class AddRolesAndSeedData : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "RoleId",
                table: "Users",
                type: "int",
                nullable: false,
                defaultValue: 0);
            
            migrationBuilder.Sql("UPDATE Users SET RoleId = 3"); // Gives everyone a valid role before the constraint is applied

            migrationBuilder.CreateTable(
                name: "Roles",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Name = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Description = table.Column<string>(type: "nvarchar(max)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Roles", x => x.Id);
                });

            migrationBuilder.InsertData(
                table: "Roles",
                columns: new[] { "Id", "Description", "Name" },
                values: new object[,]
                {
                    { 1, "Platform owner", "SystemAdmin" },
                    { 2, "Manages the clinic", "ClinicAdmin" },
                    { 3, "Doctors and Therapists", "Provider" },
                    { 4, "Handles finances", "BillingManager" },
                    { 5, "Schedules and checks-in", "FrontDesk" }
                });

            migrationBuilder.InsertData(
                table: "Tenants",
                columns: new[] { "Id", "CreatedAt", "IsActive", "Name" },
                values: new object[] { new Guid("00000000-0000-0000-0000-000000000001"), new DateTime(2026, 2, 25, 5, 23, 2, 682, DateTimeKind.Utc).AddTicks(939), true, "HPMS System Administration" });

            migrationBuilder.InsertData(
                table: "Users",
                columns: new[] { "Id", "Email", "FirstName", "IsDeleted", "LastName", "PasswordHash", "RoleId", "TenantId", "Username" },
                values: new object[] { new Guid("00000000-0000-0000-0000-000000000001"), "admin@company.com", "System", false, "Admin", "$2a$11$j6hNFljRKXuzL3EL05KGW.FWVs7FVvtZgd6cOG8/7cIzCBVH/9PTu", 1, new Guid("00000000-0000-0000-0000-000000000001"), "sysadmin" });

            migrationBuilder.CreateIndex(
                name: "IX_Users_RoleId",
                table: "Users",
                column: "RoleId");

            migrationBuilder.AddForeignKey(
                name: "FK_Users_Roles_RoleId",
                table: "Users",
                column: "RoleId",
                principalTable: "Roles",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Users_Roles_RoleId",
                table: "Users");

            migrationBuilder.DropTable(
                name: "Roles");

            migrationBuilder.DropIndex(
                name: "IX_Users_RoleId",
                table: "Users");

            migrationBuilder.DeleteData(
                table: "Tenants",
                keyColumn: "Id",
                keyValue: new Guid("00000000-0000-0000-0000-000000000001"));

            migrationBuilder.DeleteData(
                table: "Users",
                keyColumn: "Id",
                keyValue: new Guid("00000000-0000-0000-0000-000000000001"));

            migrationBuilder.DropColumn(
                name: "RoleId",
                table: "Users");
        }
    }
}
