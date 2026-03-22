using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ClinicaFlow.Api.Migrations;

/// <inheritdoc />
public partial class AddDoctorTaxCode : Migration
{
    /// <inheritdoc />
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.AddColumn<string>(
            name: "TaxCode",
            table: "Doctors",
            type: "nvarchar(16)",
            maxLength: 16,
            nullable: true);

        migrationBuilder.Sql(@"
            UPDATE Doctors
            SET TaxCode = 'TMPDOC' + RIGHT('0000000000' + CAST(Id AS VARCHAR(10)), 10)
            WHERE TaxCode IS NULL;
        ");

        migrationBuilder.AlterColumn<string>(
            name: "TaxCode",
            table: "Doctors",
            type: "nvarchar(16)",
            maxLength: 16,
            nullable: false,
            oldClrType: typeof(string),
            oldType: "nvarchar(16)",
            oldMaxLength: 16,
            oldNullable: true);

        migrationBuilder.CreateIndex(
            name: "IX_Doctors_TaxCode",
            table: "Doctors",
            column: "TaxCode",
            unique: true);
    }

    /// <inheritdoc />
    protected override void Down(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.DropIndex(
            name: "IX_Doctors_TaxCode",
            table: "Doctors");

        migrationBuilder.DropColumn(
            name: "TaxCode",
            table: "Doctors");
    }
}