using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SuttorLib.Migrations
{
    /// <inheritdoc />
    public partial class changeRelationBetweenUserAndFcmToManyToMany : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_FcmLog_AspNetUsers_UserId",
                table: "FcmLog");

            migrationBuilder.DropForeignKey(
                name: "FK_FcmToken_AspNetUsers_UserId",
                table: "FcmToken");

            migrationBuilder.DropIndex(
                name: "IX_FcmToken_UserId",
                table: "FcmToken");

            migrationBuilder.DropIndex(
                name: "IX_FcmLog_UserId",
                table: "FcmLog");

            migrationBuilder.DropColumn(
                name: "UserId",
                table: "FcmLog");

            migrationBuilder.AlterColumn<string>(
                name: "UserId",
                table: "FcmToken",
                type: "longtext",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "varchar(255)");

            migrationBuilder.CreateTable(
                name: "FcmUserLog",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "char(36)", nullable: false),
                    UserId = table.Column<string>(type: "varchar(255)", nullable: false),
                    logId = table.Column<Guid>(type: "char(36)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_FcmUserLog", x => x.Id);
                    table.ForeignKey(
                        name: "FK_FcmUserLog_AspNetUsers_UserId",
                        column: x => x.UserId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_FcmUserLog_FcmLog_logId",
                        column: x => x.logId,
                        principalTable: "FcmLog",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                })
                .Annotation("MySQL:Charset", "utf8mb4");

            migrationBuilder.CreateIndex(
                name: "IX_FcmUserLog_logId",
                table: "FcmUserLog",
                column: "logId");

            migrationBuilder.CreateIndex(
                name: "IX_FcmUserLog_UserId",
                table: "FcmUserLog",
                column: "UserId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "FcmUserLog");

            migrationBuilder.AlterColumn<string>(
                name: "UserId",
                table: "FcmToken",
                type: "varchar(255)",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "longtext");

            migrationBuilder.AddColumn<string>(
                name: "UserId",
                table: "FcmLog",
                type: "varchar(255)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.CreateIndex(
                name: "IX_FcmToken_UserId",
                table: "FcmToken",
                column: "UserId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_FcmLog_UserId",
                table: "FcmLog",
                column: "UserId",
                unique: true);

            migrationBuilder.AddForeignKey(
                name: "FK_FcmLog_AspNetUsers_UserId",
                table: "FcmLog",
                column: "UserId",
                principalTable: "AspNetUsers",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_FcmToken_AspNetUsers_UserId",
                table: "FcmToken",
                column: "UserId",
                principalTable: "AspNetUsers",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
