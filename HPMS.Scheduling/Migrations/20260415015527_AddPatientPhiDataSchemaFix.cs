using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace HPMS.Scheduling.Migrations
{
    /// <inheritdoc />
    public partial class AddPatientPhiDataSchemaFix : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "PHI_Data",
                table: "Patients",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: string.Empty);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "PHI_Data",
                table: "Patients");
        }
    }
}
