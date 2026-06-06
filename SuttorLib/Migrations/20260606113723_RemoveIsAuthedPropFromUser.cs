using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SuttorLib.Migrations
{
    /// <inheritdoc />
    public partial class RemoveIsAuthedPropFromUser : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "IsAuthed",
                table: "AspNetUsers");

            migrationBuilder.UpdateData(
                table: "AspNetRoles",
                keyColumn: "Id",
                keyValue: "60909110-f307-44d3-8d74-e9e85c5b7896",
                column: "ConcurrencyStamp",
                value: "5294f3db-47d5-469d-ad45-3966a924ef4c");

            migrationBuilder.UpdateData(
                table: "AspNetRoles",
                keyColumn: "Id",
                keyValue: "b28dafd1-5b59-47f2-8af2-d510cd1ecb0b",
                column: "ConcurrencyStamp",
                value: "0b12624a-043c-46a2-a287-1e1124f2e814");

            migrationBuilder.UpdateData(
                table: "AspNetRoles",
                keyColumn: "Id",
                keyValue: "d72cc571-c363-4d19-8818-9bebb24fba93",
                column: "ConcurrencyStamp",
                value: "e63bb724-a9e4-4372-a49e-41c27794d6dd");

            migrationBuilder.UpdateData(
                table: "AspNetUsers",
                keyColumn: "Id",
                keyValue: "29d93d5b-efbc-4ac7-999b-7b211629d8b0",
                columns: new[] { "ConcurrencyStamp", "PasswordHash", "SecurityStamp" },
                values: new object[] { "db461cce-9b9c-46f5-bc25-61bd2aaaa427", "AQAAAAIAAYagAAAAEE2IpIuAL977hKE5s1x2ZO5gKlWd+1OPc+uZmf7z0FrzcvChhvwr0+JcAg/5K3Hx5A==", "b994fce4-673e-4bae-9710-086cf3189096" });

            migrationBuilder.UpdateData(
                table: "AspNetUsers",
                keyColumn: "Id",
                keyValue: "b7359b68-b61a-4991-8c9f-b6394494b11a",
                columns: new[] { "ConcurrencyStamp", "PasswordHash", "SecurityStamp" },
                values: new object[] { "02e475fd-b101-4b39-996c-61b56a30e24a", "AQAAAAIAAYagAAAAEITrcQlqvJmPIvwm0i4AB0KrqrgAWBz0ESjlyHLDUxkBlCM7RTHeRWVVKTGCTze5Ew==", "8ece8c60-9350-4fd2-9478-bbfd2cb3127a" });

            migrationBuilder.UpdateData(
                table: "AspNetUsers",
                keyColumn: "Id",
                keyValue: "f422f142-09b2-40b9-a886-a14b213973d5",
                columns: new[] { "ConcurrencyStamp", "PasswordHash", "SecurityStamp" },
                values: new object[] { "a0b41ac8-fac4-447e-988c-561674dccd04", "AQAAAAIAAYagAAAAEL3x4vZDFBQ/VE8/WEkZBXelM2VTouZQ0qQnRenhR64X6lbTjexEXeJ29PqYRmzjwg==", "fdef5909-f815-4903-901d-8e37b2061548" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "IsAuthed",
                table: "AspNetUsers",
                type: "tinyint(1)",
                nullable: false,
                defaultValue: false);

            migrationBuilder.UpdateData(
                table: "AspNetRoles",
                keyColumn: "Id",
                keyValue: "60909110-f307-44d3-8d74-e9e85c5b7896",
                column: "ConcurrencyStamp",
                value: "0da40d3f-0e20-4d6c-9b78-a91c67060a48");

            migrationBuilder.UpdateData(
                table: "AspNetRoles",
                keyColumn: "Id",
                keyValue: "b28dafd1-5b59-47f2-8af2-d510cd1ecb0b",
                column: "ConcurrencyStamp",
                value: "d6f22960-f731-441e-8296-cbcc2a75e187");

            migrationBuilder.UpdateData(
                table: "AspNetRoles",
                keyColumn: "Id",
                keyValue: "d72cc571-c363-4d19-8818-9bebb24fba93",
                column: "ConcurrencyStamp",
                value: "c0d2cab2-82a4-4563-8ad9-ed1878f31da6");

            migrationBuilder.UpdateData(
                table: "AspNetUsers",
                keyColumn: "Id",
                keyValue: "29d93d5b-efbc-4ac7-999b-7b211629d8b0",
                columns: new[] { "ConcurrencyStamp", "IsAuthed", "PasswordHash", "SecurityStamp" },
                values: new object[] { "221ff105-8ff0-4bea-b179-3c68bf0a1352", true, "AQAAAAIAAYagAAAAEEZdAp3gTTg2TLfiT/r6kY1I8QCJvo4y7CD/6+BCrVH1YFCrIwDW2Ck1sF53bxeF+A==", "5b70aa76-fd4e-4143-8c3f-730f623498f6" });

            migrationBuilder.UpdateData(
                table: "AspNetUsers",
                keyColumn: "Id",
                keyValue: "b7359b68-b61a-4991-8c9f-b6394494b11a",
                columns: new[] { "ConcurrencyStamp", "IsAuthed", "PasswordHash", "SecurityStamp" },
                values: new object[] { "353134f5-4b1d-4cbe-b8f1-67583d6aeecb", true, "AQAAAAIAAYagAAAAEOzI4mM15NzdIya/ujoLZjGbVgcEatbuZNHvwG5T+qpqqp5IMo8MBVtriUPV99ivOA==", "2054fdf5-bf51-4a63-86fd-d0b0eb3ddc25" });

            migrationBuilder.UpdateData(
                table: "AspNetUsers",
                keyColumn: "Id",
                keyValue: "f422f142-09b2-40b9-a886-a14b213973d5",
                columns: new[] { "ConcurrencyStamp", "IsAuthed", "PasswordHash", "SecurityStamp" },
                values: new object[] { "34745da4-8a59-459e-8073-9bc1c9270925", true, "AQAAAAIAAYagAAAAELDfMaAcaTUQDOIxWhZ+VvtOa+Av//akTQXKD1CjLhzUHEYL8ktfOS//lvXal2HlOw==", "99fb352c-cb26-4cd9-a7b0-8418ed3e432c" });
        }
    }
}
