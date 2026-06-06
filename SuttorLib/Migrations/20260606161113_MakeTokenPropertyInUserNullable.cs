using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SuttorLib.Migrations
{
    /// <inheritdoc />
    public partial class MakeTokenPropertyInUserNullable : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<string>(
                name: "Token",
                table: "AspNetUsers",
                type: "longtext",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "longtext");

            migrationBuilder.UpdateData(
                table: "AspNetRoles",
                keyColumn: "Id",
                keyValue: "60909110-f307-44d3-8d74-e9e85c5b7896",
                column: "ConcurrencyStamp",
                value: "8a87ff80-2144-4727-b070-d458bb2384bf");

            migrationBuilder.UpdateData(
                table: "AspNetRoles",
                keyColumn: "Id",
                keyValue: "b28dafd1-5b59-47f2-8af2-d510cd1ecb0b",
                column: "ConcurrencyStamp",
                value: "e01fef95-59ce-44dd-8d13-ee09871b914a");

            migrationBuilder.UpdateData(
                table: "AspNetRoles",
                keyColumn: "Id",
                keyValue: "d72cc571-c363-4d19-8818-9bebb24fba93",
                column: "ConcurrencyStamp",
                value: "170ff160-7b53-4e41-8f0b-c469e992f9aa");

            migrationBuilder.UpdateData(
                table: "AspNetUsers",
                keyColumn: "Id",
                keyValue: "29d93d5b-efbc-4ac7-999b-7b211629d8b0",
                columns: new[] { "ConcurrencyStamp", "PasswordHash", "SecurityStamp" },
                values: new object[] { "504dd6c8-a9dc-4aca-9458-329945120723", "AQAAAAIAAYagAAAAENEyJz7M8b4Rf8ervBZ56y1p6qvgROuF/+Gb1saZjyH0n3rwwU46kreo5+pYc0/2Tg==", "4e28e08b-3eba-4e3c-9290-2eca29efd81f" });

            migrationBuilder.UpdateData(
                table: "AspNetUsers",
                keyColumn: "Id",
                keyValue: "b7359b68-b61a-4991-8c9f-b6394494b11a",
                columns: new[] { "ConcurrencyStamp", "PasswordHash", "SecurityStamp" },
                values: new object[] { "2292d16a-5d1e-4d3e-b5f2-42d7d0a8fa9f", "AQAAAAIAAYagAAAAEAQ/jtwhmQjrZqhgzddipkNCXEQj5rq3++LnXZrSQh/ETitO0+NaV7owYq7hyJ7Ltw==", "0003369c-13c8-45d9-b2eb-7bca16c3e065" });

            migrationBuilder.UpdateData(
                table: "AspNetUsers",
                keyColumn: "Id",
                keyValue: "f422f142-09b2-40b9-a886-a14b213973d5",
                columns: new[] { "ConcurrencyStamp", "PasswordHash", "SecurityStamp" },
                values: new object[] { "edd48a86-8a97-4b94-a813-841a50dcc916", "AQAAAAIAAYagAAAAEAXCTN3Sd0wsLtealX3ioYl0GPwT8y1cuN2O8sDB+aykHeBq8Ajr2sxm/sFloVO+ww==", "1c21c7de-eb8c-4bce-b25a-0023c19cb732" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<string>(
                name: "Token",
                table: "AspNetUsers",
                type: "longtext",
                nullable: false,
                defaultValue: "",
                oldClrType: typeof(string),
                oldType: "longtext",
                oldNullable: true);

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
    }
}
