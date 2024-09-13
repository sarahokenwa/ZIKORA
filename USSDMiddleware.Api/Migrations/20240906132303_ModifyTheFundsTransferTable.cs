using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace USSDMiddleware.Api.Migrations
{
    public partial class ModifyTheFundsTransferTable : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_CustomerDebits_Providers_ProviderId",
                table: "CustomerDebits");

            migrationBuilder.DropIndex(
                name: "IX_CustomerDebits_ProviderId",
                table: "CustomerDebits");

            migrationBuilder.DropColumn(
                name: "BeneficiaryAccountName",
                table: "FundTransfers");

            migrationBuilder.RenameColumn(
                name: "SenderAccountNumber",
                table: "FundTransfers",
                newName: "SenderName");

            migrationBuilder.RenameColumn(
                name: "SenderAccountName",
                table: "FundTransfers",
                newName: "BeneficiaryName");

            migrationBuilder.RenameColumn(
                name: "BeneficiaryAccountNumber",
                table: "FundTransfers",
                newName: "AccountNumber");

            migrationBuilder.AlterColumn<string>(
                name: "ProviderId",
                table: "CustomerDebits",
                type: "nvarchar(max)",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(450)");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "SenderName",
                table: "FundTransfers",
                newName: "SenderAccountNumber");

            migrationBuilder.RenameColumn(
                name: "BeneficiaryName",
                table: "FundTransfers",
                newName: "SenderAccountName");

            migrationBuilder.RenameColumn(
                name: "AccountNumber",
                table: "FundTransfers",
                newName: "BeneficiaryAccountNumber");

            migrationBuilder.AddColumn<string>(
                name: "BeneficiaryAccountName",
                table: "FundTransfers",
                type: "nvarchar(60)",
                maxLength: 60,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AlterColumn<string>(
                name: "ProviderId",
                table: "CustomerDebits",
                type: "nvarchar(450)",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)");

            migrationBuilder.CreateIndex(
                name: "IX_CustomerDebits_ProviderId",
                table: "CustomerDebits",
                column: "ProviderId");

            migrationBuilder.AddForeignKey(
                name: "FK_CustomerDebits_Providers_ProviderId",
                table: "CustomerDebits",
                column: "ProviderId",
                principalTable: "Providers",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
