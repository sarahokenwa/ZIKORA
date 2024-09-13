using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace USSDMiddleware.Api.Migrations
{
    public partial class AddBlockAccountTable : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "BlockAccounts",
                columns: table => new
                {
                    Id = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    OwnersPhoneNumber = table.Column<string>(type: "nvarchar(60)", maxLength: 60, nullable: false),
                    RequestPhoneNumber = table.Column<string>(type: "nvarchar(60)", maxLength: 60, nullable: false),
                    AccountNo = table.Column<string>(type: "nvarchar(60)", maxLength: 60, nullable: false),
                    RequestStatus = table.Column<bool>(type: "bit", nullable: false),
                    ResponseDescription = table.Column<string>(type: "nvarchar(60)", maxLength: 60, nullable: false),
                    ResponseStatus = table.Column<string>(type: "nvarchar(60)", maxLength: 60, nullable: false),
                    ProviderId = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    CreatedOn = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedOn = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_BlockAccounts", x => x.Id);
                });
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "BlockAccounts");
        }
    }
}
