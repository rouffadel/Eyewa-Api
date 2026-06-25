using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Eyewa_new_api.Migrations
{
    /// <inheritdoc />
    public partial class AddDbLogs : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "DbLogs",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Message = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Level = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Timestamp = table.Column<DateTime>(type: "datetime2", nullable: false),
                    Exception = table.Column<string>(type: "nvarchar(max)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_DbLogs", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "NotificationSettings",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    TwilioAccountSid = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    TwilioAuthToken = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    TwilioPhoneNumber = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    WhatsAppApiUrl = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    WhatsAppAccessToken = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    WhatsAppSenderNumber = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    FcmServerKey = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    FcmSenderId = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_NotificationSettings", x => x.Id);
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "DbLogs");

            migrationBuilder.DropTable(
                name: "NotificationSettings");
        }
    }
}
