using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace USSDMiddleware.Api.Migrations
{
    public partial class AddIntraBankTransferTable : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "IntraBankTransfers",
                columns: table => new
                {
                    Id = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    FromAccountNumber = table.Column<string>(type: "nvarchar(60)", maxLength: 60, nullable: false),
                    ToAccountNumber = table.Column<string>(type: "nvarchar(60)", maxLength: 60, nullable: false),
                    Fee = table.Column<decimal>(type: "decimal(18,2)", nullable: true),
                    RetrievalReference = table.Column<string>(type: "nvarchar(60)", maxLength: 60, nullable: false),
                    Narration = table.Column<string>(type: "nvarchar(60)", maxLength: 60, nullable: false),
                    Amount = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    IsSuccessful = table.Column<bool>(type: "bit", nullable: false),
                    ResponseMessage = table.Column<string>(type: "nvarchar(60)", maxLength: 60, nullable: false),
                    ResponseCode = table.Column<string>(type: "nvarchar(60)", maxLength: 60, nullable: false),
                    Reference = table.Column<string>(type: "nvarchar(60)", maxLength: 60, nullable: false),
                    ProcessorRef = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    ProviderId = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    CreatedOn = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedOn = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_IntraBankTransfers", x => x.Id);
                });
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "IntraBankTransfers");
        }
    }
}
