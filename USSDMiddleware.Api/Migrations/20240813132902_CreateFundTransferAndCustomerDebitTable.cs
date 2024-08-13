using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace USSDMiddleware.Api.Migrations
{
    public partial class CreateFundTransferAndCustomerDebitTable : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "CustomerDebits",
                columns: table => new
                {
                    Id = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    RetrievalReference = table.Column<string>(type: "nvarchar(60)", maxLength: 60, nullable: false),
                    AccountNumber = table.Column<string>(type: "nvarchar(60)", maxLength: 60, nullable: false),
                    Amount = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    TransactionPin = table.Column<string>(type: "nvarchar(60)", maxLength: 60, nullable: false),
                    Narration = table.Column<string>(type: "nvarchar(60)", maxLength: 60, nullable: false),
                    GLCode = table.Column<string>(type: "nvarchar(60)", maxLength: 60, nullable: false),
                    NibssCode = table.Column<string>(type: "nvarchar(60)", maxLength: 60, nullable: false),
                    BankCode = table.Column<string>(type: "nvarchar(60)", maxLength: 60, nullable: false),
                    Fee = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    ProcessorRef = table.Column<string>(type: "nvarchar(60)", maxLength: 60, nullable: false),
                    ProviderId = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    CreatedOn = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedOn = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CustomerDebits", x => x.Id);
                    table.ForeignKey(
                        name: "FK_CustomerDebits_Providers_ProviderId",
                        column: x => x.ProviderId,
                        principalTable: "Providers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "FundTransfers",
                columns: table => new
                {
                    Id = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    WalletCode = table.Column<string>(type: "nvarchar(60)", maxLength: 60, nullable: false),
                    BeneficiaryAccountName = table.Column<string>(type: "nvarchar(60)", maxLength: 60, nullable: false),
                    SenderAccountName = table.Column<string>(type: "nvarchar(60)", maxLength: 60, nullable: false),
                    SenderAccountNumber = table.Column<string>(type: "nvarchar(60)", maxLength: 60, nullable: false),
                    PhoneNumber = table.Column<string>(type: "nvarchar(60)", maxLength: 60, nullable: false),
                    Amount = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    TransactionPin = table.Column<string>(type: "nvarchar(60)", maxLength: 60, nullable: false),
                    BeneficiaryAccountNumber = table.Column<string>(type: "nvarchar(60)", maxLength: 60, nullable: false),
                    BankCode = table.Column<string>(type: "nvarchar(60)", maxLength: 60, nullable: false),
                    WebHook = table.Column<string>(type: "nvarchar(60)", maxLength: 60, nullable: false),
                    MerchantRef = table.Column<string>(type: "nvarchar(60)", maxLength: 60, nullable: false),
                    Narration = table.Column<string>(type: "nvarchar(60)", maxLength: 60, nullable: false),
                    WalletType = table.Column<string>(type: "nvarchar(60)", maxLength: 60, nullable: false),
                    ProcessorRef = table.Column<string>(type: "nvarchar(60)", maxLength: 60, nullable: false),
                    MerchantCharge = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    CreatedOn = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedOn = table.Column<DateTime>(type: "datetime2", nullable: false),
                    ProviderId = table.Column<string>(type: "nvarchar(450)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_FundTransfers", x => x.Id);
                    table.ForeignKey(
                        name: "FK_FundTransfers_Providers_ProviderId",
                        column: x => x.ProviderId,
                        principalTable: "Providers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_CustomerDebits_ProviderId",
                table: "CustomerDebits",
                column: "ProviderId");

            migrationBuilder.CreateIndex(
                name: "IX_FundTransfers_ProviderId",
                table: "FundTransfers",
                column: "ProviderId");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "CustomerDebits");

            migrationBuilder.DropTable(
                name: "FundTransfers");
        }
    }
}
