using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ClinicaFlow.Api.Migrations
{
    /// <inheritdoc />
    public partial class AddAuthUserAccounts : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_UserAccounts_DoctorId",
                table: "UserAccounts");

            migrationBuilder.DropIndex(
                name: "IX_UserAccounts_PatientId",
                table: "UserAccounts");

            migrationBuilder.AlterColumn<string>(
                name: "Role",
                table: "UserAccounts",
                type: "nvarchar(20)",
                maxLength: 20,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)");

            migrationBuilder.AddColumn<bool>(
                name: "IsActive",
                table: "UserAccounts",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.CreateIndex(
                name: "IX_UserAccounts_DoctorId",
                table: "UserAccounts",
                column: "DoctorId",
                unique: true,
                filter: "[DoctorId] IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "IX_UserAccounts_PatientId",
                table: "UserAccounts",
                column: "PatientId",
                unique: true,
                filter: "[PatientId] IS NOT NULL");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_UserAccounts_DoctorId",
                table: "UserAccounts");

            migrationBuilder.DropIndex(
                name: "IX_UserAccounts_PatientId",
                table: "UserAccounts");

            migrationBuilder.DropColumn(
                name: "IsActive",
                table: "UserAccounts");

            migrationBuilder.AlterColumn<string>(
                name: "Role",
                table: "UserAccounts",
                type: "nvarchar(max)",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(20)",
                oldMaxLength: 20);

            migrationBuilder.CreateIndex(
                name: "IX_UserAccounts_DoctorId",
                table: "UserAccounts",
                column: "DoctorId");

            migrationBuilder.CreateIndex(
                name: "IX_UserAccounts_PatientId",
                table: "UserAccounts",
                column: "PatientId");
        }
    }
}
