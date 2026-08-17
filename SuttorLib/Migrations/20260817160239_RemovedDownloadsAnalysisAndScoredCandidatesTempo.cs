using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SuttorLib.Migrations
{
    /// <inheritdoc />
    public partial class RemovedDownloadsAnalysisAndScoredCandidatesTempo : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "DownloadsAnalysis");

            migrationBuilder.DropTable(
                name: "ScoredCandidates");

            migrationBuilder.UpdateData(
                table: "AspNetRoles",
                keyColumn: "Id",
                keyValue: "60909110-f307-44d3-8d74-e9e85c5b7896",
                column: "ConcurrencyStamp",
                value: "1ece73b7-9c5b-4ab6-8f14-56f7601e9964");

            migrationBuilder.UpdateData(
                table: "AspNetRoles",
                keyColumn: "Id",
                keyValue: "b28dafd1-5b59-47f2-8af2-d510cd1ecb0b",
                column: "ConcurrencyStamp",
                value: "90918aec-5082-4334-bcea-433ad4e8d744");

            migrationBuilder.UpdateData(
                table: "AspNetRoles",
                keyColumn: "Id",
                keyValue: "d72cc571-c363-4d19-8818-9bebb24fba93",
                column: "ConcurrencyStamp",
                value: "dcb58fb4-f35d-4495-96fe-f953c3a3ef4f");

            migrationBuilder.UpdateData(
                table: "AspNetUsers",
                keyColumn: "Id",
                keyValue: "29d93d5b-efbc-4ac7-999b-7b211629d8b0",
                columns: new[] { "ConcurrencyStamp", "PasswordHash", "SecurityStamp" },
                values: new object[] { "b87d7fd0-d6d7-4fc1-baf4-b787a7dad409", "AQAAAAIAAYagAAAAEMdcveMaDdhE2nxoIIA8+IpaoG/hW+EaOiPc0+/uz99jK43KSXR2Dco2qT4HXlMcIw==", "3e7a84e3-21a1-44a2-bd43-8d48bff77b6f" });

            migrationBuilder.UpdateData(
                table: "AspNetUsers",
                keyColumn: "Id",
                keyValue: "b7359b68-b61a-4991-8c9f-b6394494b11a",
                columns: new[] { "ConcurrencyStamp", "PasswordHash", "SecurityStamp" },
                values: new object[] { "1d043052-ba88-4875-9657-75190ec20701", "AQAAAAIAAYagAAAAELUWh4ZsAmXOCIGNwJfeSQjlvqDJELLGNLa6sGPQ89UGS4nUGHpSEXfh988r5fGSqw==", "55808ba9-1f4b-43a3-8dfb-e58fc874d61f" });

            migrationBuilder.UpdateData(
                table: "AspNetUsers",
                keyColumn: "Id",
                keyValue: "f422f142-09b2-40b9-a886-a14b213973d5",
                columns: new[] { "ConcurrencyStamp", "PasswordHash", "SecurityStamp" },
                values: new object[] { "446609d1-e81d-47c0-acd2-6b0ae60f3b6c", "AQAAAAIAAYagAAAAEDustRuQPQ5d65kWFZBq836/PE+r++ikoEgaXC5Ei2YbTK/EUw1uVa05qwwRznWS5A==", "432726ee-c904-4b3d-8f54-1cc8c470d98b" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "DownloadsAnalysis",
                columns: table => new
                {
                    BookId = table.Column<string>(type: "varchar(255)", maxLength: 255, nullable: false),
                    Label = table.Column<float>(type: "float", nullable: false, defaultValue: 0f),
                    UserId = table.Column<string>(type: "varchar(255)", maxLength: 255, nullable: false)
                },
                constraints: table =>
                {
                })
                .Annotation("MySQL:Charset", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "ScoredCandidates",
                columns: table => new
                {
                    BookId = table.Column<string>(type: "varchar(255)", maxLength: 255, nullable: false),
                    Score = table.Column<float>(type: "float", nullable: false),
                    UserId = table.Column<string>(type: "varchar(255)", maxLength: 255, nullable: false)
                },
                constraints: table =>
                {
                })
                .Annotation("MySQL:Charset", "utf8mb4");

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
    }
}
