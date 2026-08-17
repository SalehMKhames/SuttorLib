using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SuttorLib.Migrations
{
    /// <inheritdoc />
    public partial class RemovedPksFromDownloadAnalysisAndScoredCandidates : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropPrimaryKey(
                name: "PK_ScoredCandidates",
                table: "ScoredCandidates");

            migrationBuilder.DropPrimaryKey(
                name: "PK_DownloadsAnalysis",
                table: "DownloadsAnalysis");

            migrationBuilder.AlterColumn<string>(
                name: "BookId",
                table: "ScoredCandidates",
                type: "varchar(255)",
                maxLength: 255,
                nullable: false,
                oldClrType: typeof(float),
                oldType: "float");

            migrationBuilder.AlterColumn<string>(
                name: "UserId",
                table: "ScoredCandidates",
                type: "varchar(255)",
                maxLength: 255,
                nullable: false,
                oldClrType: typeof(float),
                oldType: "float");

            migrationBuilder.AlterColumn<float>(
                name: "Label",
                table: "DownloadsAnalysis",
                type: "float",
                nullable: false,
                defaultValue: 0f,
                oldClrType: typeof(float),
                oldType: "float");

            migrationBuilder.UpdateData(
                table: "AspNetRoles",
                keyColumn: "Id",
                keyValue: "60909110-f307-44d3-8d74-e9e85c5b7896",
                column: "ConcurrencyStamp",
                value: "cef3b36b-4c14-4d54-814f-492e9ee20111");

            migrationBuilder.UpdateData(
                table: "AspNetRoles",
                keyColumn: "Id",
                keyValue: "b28dafd1-5b59-47f2-8af2-d510cd1ecb0b",
                column: "ConcurrencyStamp",
                value: "345e39e6-4f7d-4113-b8e3-aa05bb9c6d28");

            migrationBuilder.UpdateData(
                table: "AspNetRoles",
                keyColumn: "Id",
                keyValue: "d72cc571-c363-4d19-8818-9bebb24fba93",
                column: "ConcurrencyStamp",
                value: "9fdb372b-6810-4f1d-b43e-ce39f2d4402c");

            migrationBuilder.UpdateData(
                table: "AspNetUsers",
                keyColumn: "Id",
                keyValue: "29d93d5b-efbc-4ac7-999b-7b211629d8b0",
                columns: new[] { "ConcurrencyStamp", "PasswordHash", "SecurityStamp" },
                values: new object[] { "8714c24b-91ab-43bb-a5c3-bbb5358285e7", "AQAAAAIAAYagAAAAEGCy7g5M8UsSRtlo5X14RJr4CjlotJqRAZcXbuhuX/0/1exRZMv3HKOq5jwCUl4hyA==", "3df1166d-f86e-4ae0-88d1-6994d4117024" });

            migrationBuilder.UpdateData(
                table: "AspNetUsers",
                keyColumn: "Id",
                keyValue: "b7359b68-b61a-4991-8c9f-b6394494b11a",
                columns: new[] { "ConcurrencyStamp", "PasswordHash", "SecurityStamp" },
                values: new object[] { "acbdbd42-cdea-48fc-bb75-5a93d3ae0315", "AQAAAAIAAYagAAAAEBUKgocIpamtFy7ZfIO+WLWHgPkDN3foD8cKLmaz6foD4K7tnvP9TN917RYl9bCUgg==", "5bb75ca3-ccf3-464f-b4ac-daec78f4f62e" });

            migrationBuilder.UpdateData(
                table: "AspNetUsers",
                keyColumn: "Id",
                keyValue: "f422f142-09b2-40b9-a886-a14b213973d5",
                columns: new[] { "ConcurrencyStamp", "PasswordHash", "SecurityStamp" },
                values: new object[] { "b8e83ec2-6898-4b85-aea7-5d37355c2fa9", "AQAAAAIAAYagAAAAEJ32wNqxkbGODfEJGPEBa5iWKYUQFHR5jSb7kkI4ef9A5QxsVlfgS+IUBO7zVWJ9Xw==", "30c056f3-3dff-4297-807e-db1257e56a03" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<float>(
                name: "UserId",
                table: "ScoredCandidates",
                type: "float",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "varchar(255)",
                oldMaxLength: 255);

            migrationBuilder.AlterColumn<float>(
                name: "BookId",
                table: "ScoredCandidates",
                type: "float",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "varchar(255)",
                oldMaxLength: 255);

            migrationBuilder.AlterColumn<float>(
                name: "Label",
                table: "DownloadsAnalysis",
                type: "float",
                nullable: false,
                oldClrType: typeof(float),
                oldType: "float",
                oldDefaultValue: 0f);

            migrationBuilder.AddPrimaryKey(
                name: "PK_ScoredCandidates",
                table: "ScoredCandidates",
                columns: new[] { "UserId", "BookId" });

            migrationBuilder.AddPrimaryKey(
                name: "PK_DownloadsAnalysis",
                table: "DownloadsAnalysis",
                columns: new[] { "UserId", "BookId" });

            migrationBuilder.UpdateData(
                table: "AspNetRoles",
                keyColumn: "Id",
                keyValue: "60909110-f307-44d3-8d74-e9e85c5b7896",
                column: "ConcurrencyStamp",
                value: "d76669d8-a26d-4994-9eca-9cafdd0fd06c");

            migrationBuilder.UpdateData(
                table: "AspNetRoles",
                keyColumn: "Id",
                keyValue: "b28dafd1-5b59-47f2-8af2-d510cd1ecb0b",
                column: "ConcurrencyStamp",
                value: "1154b135-e33e-4588-95a0-25edbc523403");

            migrationBuilder.UpdateData(
                table: "AspNetRoles",
                keyColumn: "Id",
                keyValue: "d72cc571-c363-4d19-8818-9bebb24fba93",
                column: "ConcurrencyStamp",
                value: "0d923c97-abb0-483d-82d7-8e724305d1ac");

            migrationBuilder.UpdateData(
                table: "AspNetUsers",
                keyColumn: "Id",
                keyValue: "29d93d5b-efbc-4ac7-999b-7b211629d8b0",
                columns: new[] { "ConcurrencyStamp", "PasswordHash", "SecurityStamp" },
                values: new object[] { "a7b1c510-42e0-4685-88b3-faa3a7e70964", "AQAAAAIAAYagAAAAEJQUGS02cBpId6m5MqsXFsnNKCQixBavKBwF05Q0sHKZ9V6l65iTI/jNrPu5e4mNqg==", "4c3cbc5b-934e-4727-bf76-b9b498584ff4" });

            migrationBuilder.UpdateData(
                table: "AspNetUsers",
                keyColumn: "Id",
                keyValue: "b7359b68-b61a-4991-8c9f-b6394494b11a",
                columns: new[] { "ConcurrencyStamp", "PasswordHash", "SecurityStamp" },
                values: new object[] { "242cbb1e-07f4-4f67-be9d-3be94eec8615", "AQAAAAIAAYagAAAAENNthm6RWxLFPWZILPre7e+SUw3ajuzCB657KksJSd4YIuPKId3DlSK+pT/CH6trHA==", "f7a8c031-ab6d-4797-b38e-a960a1147aba" });

            migrationBuilder.UpdateData(
                table: "AspNetUsers",
                keyColumn: "Id",
                keyValue: "f422f142-09b2-40b9-a886-a14b213973d5",
                columns: new[] { "ConcurrencyStamp", "PasswordHash", "SecurityStamp" },
                values: new object[] { "df7c58e4-e7a4-44b0-972c-299447a801ad", "AQAAAAIAAYagAAAAEAJ+J+E1PXGG/yCiuZyADEG9K+jLhAdE/sU2YShvf1PCDwU5gRPNGO/UY4WimwIw2Q==", "33b44535-2363-4837-9c7f-ee1afad9f277" });
        }
    }
}
