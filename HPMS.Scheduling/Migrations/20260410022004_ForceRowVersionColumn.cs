using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace HPMS.Scheduling.Migrations
{
    /// <inheritdoc />
    public partial class ForceRowVersionColumn : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // 1. Drop the existing column that SQL Server won't let us alter
            migrationBuilder.DropColumn(
                name: "RowVersion",
                table: "Appointments");

            // 2. Add it back fresh as a true rowversion
            migrationBuilder.AddColumn<byte[]>(
                name: "RowVersion",
                table: "Appointments",
                type: "rowversion",
                rowVersion: true,
                nullable: true); // SQL Server manages this, EF handles the mapping
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "RowVersion",
                table: "Appointments");

            migrationBuilder.AddColumn<byte[]>(
                name: "RowVersion",
                table: "Appointments",
                type: "varbinary(max)",
                nullable: false);
        }
    }
}
