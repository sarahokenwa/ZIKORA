using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace USSDMiddleware.Api.Migrations
{
    public partial class addbillstable : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            //migrationBuilder.CreateTable(
            //    name: "Providers",
            //    columns: table => new
            //    {
            //        Id = table.Column<string>(type: "nvarchar(450)", nullable: false),
            //        Name = table.Column<string>(type: "nvarchar(max)", nullable: false)
            //    },
            //    constraints: table =>
            //    {
            //        table.PrimaryKey("PK_Providers", x => x.Id);
            //    });

            //migrationBuilder.CreateTable(
            //    name: "ValidationLogs",
            //    columns: table => new
            //    {
            //        Id = table.Column<string>(type: "nvarchar(450)", nullable: false),
            //        ValidationReference = table.Column<string>(type: "nvarchar(max)", nullable: false),
            //        LastName = table.Column<string>(type: "nvarchar(max)", nullable: false),
            //        FirstName = table.Column<string>(type: "nvarchar(max)", nullable: false),
            //        OtherNames = table.Column<string>(type: "nvarchar(max)", nullable: true),
            //        Bvn = table.Column<string>(type: "nvarchar(max)", nullable: false),
            //        Dob = table.Column<string>(type: "nvarchar(max)", nullable: false),
            //        PhoneNumber = table.Column<string>(type: "nvarchar(max)", nullable: false),
            //        Valid = table.Column<bool>(type: "bit", nullable: false),
            //        CreatedOn = table.Column<DateTime>(type: "datetime2", nullable: false)
            //    },
            //    constraints: table =>
            //    {
            //        table.PrimaryKey("PK_ValidationLogs", x => x.Id);
            //    });

            migrationBuilder.CreateTable(
                name: "BillsPayments",
                columns: table => new
                {
                    Id = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    CustomerId = table.Column<string>(type: "nvarchar(60)", maxLength: 60, nullable: false),
                    PhoneNumber = table.Column<string>(type: "nvarchar(60)", maxLength: 60, nullable: false),
                    ProviderId = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    itemcode = table.Column<string>(type: "nvarchar(60)", maxLength: 60, nullable: false),
                    validationref = table.Column<string>(type: "nvarchar(60)", maxLength: 60, nullable: false),
                    merchantref = table.Column<string>(type: "nvarchar(60)", maxLength: 60, nullable: false),
                    requeryresponsecode = table.Column<string>(type: "nvarchar(60)", maxLength: 60, nullable: false),
                    responsecode = table.Column<string>(type: "nvarchar(60)", maxLength: 60, nullable: false),
                    processorRef = table.Column<string>(type: "nvarchar(60)", maxLength: 60, nullable: false),
                    Amount = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    Fee = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    CreatedOn = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedOn = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_BillsPayments", x => x.Id);
                    table.ForeignKey(
                        name: "FK_BillsPayments_Providers_ProviderId",
                        column: x => x.ProviderId,
                        principalTable: "Providers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            //migrationBuilder.CreateTable(
            //    name: "Users",
            //    columns: table => new
            //    {
            //        Id = table.Column<string>(type: "nvarchar(450)", nullable: false),
            //        CustomerId = table.Column<string>(type: "nvarchar(max)", nullable: false),
            //        PhoneNumber = table.Column<string>(type: "nvarchar(max)", nullable: false),
            //        ProviderId = table.Column<string>(type: "nvarchar(450)", nullable: false),
            //        CustomerName = table.Column<string>(type: "nvarchar(max)", nullable: false),
            //        Address = table.Column<string>(type: "nvarchar(max)", nullable: false),
            //        Email = table.Column<string>(type: "nvarchar(max)", nullable: false),
            //        BankVerificationNumber = table.Column<string>(type: "nvarchar(max)", nullable: false),
            //        DateOfBirth = table.Column<string>(type: "nvarchar(max)", nullable: false),
            //        TransactionPin = table.Column<string>(type: "nvarchar(max)", nullable: false),
            //        CreatedOn = table.Column<DateTime>(type: "datetime2", nullable: false),
            //        UpdatedOn = table.Column<DateTime>(type: "datetime2", nullable: false)
            //    },
            //    constraints: table =>
            //    {
            //        table.PrimaryKey("PK_Users", x => x.Id);
            //        table.ForeignKey(
            //            name: "FK_Users_Providers_ProviderId",
            //            column: x => x.ProviderId,
            //            principalTable: "Providers",
            //            principalColumn: "Id",
            //            onDelete: ReferentialAction.Cascade);
            //    });

            //migrationBuilder.CreateTable(
            //    name: "Accounts",
            //    columns: table => new
            //    {
            //        Id = table.Column<string>(type: "nvarchar(450)", nullable: false),
            //        CustomerID = table.Column<string>(type: "nvarchar(max)", nullable: false),
            //        LastName = table.Column<string>(type: "nvarchar(max)", nullable: true),
            //        OtherNames = table.Column<string>(type: "nvarchar(max)", nullable: true),
            //        BVN = table.Column<string>(type: "nvarchar(max)", nullable: true),
            //        AccountName = table.Column<string>(type: "nvarchar(max)", nullable: true),
            //        AccountNumber = table.Column<string>(type: "nvarchar(max)", nullable: true),
            //        PhoneNo = table.Column<string>(type: "nvarchar(max)", nullable: true),
            //        Gender = table.Column<int>(type: "int", nullable: true),
            //        DateOfBirth = table.Column<string>(type: "nvarchar(max)", nullable: true),
            //        Email = table.Column<string>(type: "nvarchar(max)", nullable: true),
            //        IsActive = table.Column<bool>(type: "bit", nullable: true),
            //        Address = table.Column<string>(type: "nvarchar(max)", nullable: true),
            //        UserId = table.Column<string>(type: "nvarchar(450)", nullable: true)
            //    },
            //    constraints: table =>
            //    {
            //        table.PrimaryKey("PK_Accounts", x => x.Id);
            //        table.ForeignKey(
            //            name: "FK_Accounts_Users_UserId",
            //            column: x => x.UserId,
            //            principalTable: "Users",
            //            principalColumn: "Id");
            //    });

            //migrationBuilder.CreateIndex(
            //    name: "IX_Accounts_UserId",
            //    table: "Accounts",
            //    column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_BillsPayments_ProviderId",
                table: "BillsPayments",
                column: "ProviderId");

            //migrationBuilder.CreateIndex(
            //    name: "IX_Users_ProviderId",
            //    table: "Users",
            //    column: "ProviderId");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            //migrationBuilder.DropTable(
            //    name: "Accounts");

            migrationBuilder.DropTable(
                name: "BillsPayments");

            //migrationBuilder.DropTable(
            //    name: "ValidationLogs");

            //migrationBuilder.DropTable(
            //    name: "Users");

            //migrationBuilder.DropTable(
            //    name: "Providers");
        }
    }
}
