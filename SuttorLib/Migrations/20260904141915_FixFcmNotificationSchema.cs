using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SuttorLib.Migrations
{
    /// <inheritdoc />
    public partial class FixFcmNotificationSchema : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_FcmUserLog_FcmLog_logId",
                table: "FcmUserLog");

            migrationBuilder.RenameColumn(
                name: "logId",
                table: "FcmUserLog",
                newName: "LogId");

            migrationBuilder.RenameIndex(
                name: "IX_FcmUserLog_logId",
                table: "FcmUserLog",
                newName: "IX_FcmUserLog_LogId");

            migrationBuilder.AddColumn<DateTime>(
                name: "CreatedAt",
                table: "FcmLog",
                type: "datetime(6)",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.AddColumn<string>(
                name: "Data",
                table: "FcmLog",
                type: "longtext",
                nullable: true);

            migrationBuilder.UpdateData(
                table: "AspNetRoles",
                keyColumn: "Id",
                keyValue: "60909110-f307-44d3-8d74-e9e85c5b7896",
                column: "ConcurrencyStamp",
                value: "abafab5e-c389-416a-8c27-3028f3ff13e5");

            migrationBuilder.UpdateData(
                table: "AspNetRoles",
                keyColumn: "Id",
                keyValue: "b28dafd1-5b59-47f2-8af2-d510cd1ecb0b",
                column: "ConcurrencyStamp",
                value: "4c7b1b7c-d385-4348-ba01-67eac9333443");

            migrationBuilder.UpdateData(
                table: "AspNetRoles",
                keyColumn: "Id",
                keyValue: "d72cc571-c363-4d19-8818-9bebb24fba93",
                column: "ConcurrencyStamp",
                value: "f42d7e47-7075-486c-820e-9bb623e959c8");

            migrationBuilder.UpdateData(
                table: "AspNetUsers",
                keyColumn: "Id",
                keyValue: "29d93d5b-efbc-4ac7-999b-7b211629d8b0",
                columns: new[] { "ConcurrencyStamp", "PasswordHash", "SecurityStamp" },
                values: new object[] { "2746d395-74f0-4d21-a9d1-353897bf698a", "AQAAAAIAAYagAAAAENn0QLlMtO4iDztT98ldqiKV9XT9cLAaMcxIN+CUUS6Xz8j2aZAE7MdOT5QN62qERw==", "b0eda4af-05ed-4de8-b164-a29dda046756" });

            migrationBuilder.UpdateData(
                table: "AspNetUsers",
                keyColumn: "Id",
                keyValue: "b7359b68-b61a-4991-8c9f-b6394494b11a",
                columns: new[] { "ConcurrencyStamp", "PasswordHash", "SecurityStamp" },
                values: new object[] { "6b5c94cf-94f0-4f92-acc6-8903bba6b494", "AQAAAAIAAYagAAAAEPbuOK2gIlS1+jv9O/pMMu1DQF7326qY+LI/7fQHN587qz8yA6AyzHoYXysk14BLCQ==", "115bbde8-4d7c-4c61-945d-d3a02c8de747" });

            migrationBuilder.UpdateData(
                table: "AspNetUsers",
                keyColumn: "Id",
                keyValue: "f422f142-09b2-40b9-a886-a14b213973d5",
                columns: new[] { "ConcurrencyStamp", "PasswordHash", "SecurityStamp" },
                values: new object[] { "c589cf3c-f558-484c-8b92-994e03952dbf", "AQAAAAIAAYagAAAAEIVUDD1MMGXGcp1uB8KvN3w12uIBAElN0eW/ccPdJQszIqOjlE9BFd2wkdN7+Reu6g==", "cc5728ef-4020-4197-a662-38d80bd89690" });

            migrationBuilder.AddForeignKey(
                name: "FK_FcmUserLog_FcmLog_LogId",
                table: "FcmUserLog",
                column: "LogId",
                principalTable: "FcmLog",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_FcmUserLog_FcmLog_LogId",
                table: "FcmUserLog");

            migrationBuilder.DropColumn(
                name: "CreatedAt",
                table: "FcmLog");

            migrationBuilder.DropColumn(
                name: "Data",
                table: "FcmLog");

            migrationBuilder.RenameColumn(
                name: "LogId",
                table: "FcmUserLog",
                newName: "logId");

            migrationBuilder.RenameIndex(
                name: "IX_FcmUserLog_LogId",
                table: "FcmUserLog",
                newName: "IX_FcmUserLog_logId");

            migrationBuilder.UpdateData(
                table: "AspNetRoles",
                keyColumn: "Id",
                keyValue: "60909110-f307-44d3-8d74-e9e85c5b7896",
                column: "ConcurrencyStamp",
                value: "ea3c9c85-0fa8-470c-aa41-8f8c72c90ce6");

            migrationBuilder.UpdateData(
                table: "AspNetRoles",
                keyColumn: "Id",
                keyValue: "b28dafd1-5b59-47f2-8af2-d510cd1ecb0b",
                column: "ConcurrencyStamp",
                value: "6b0fc290-c7cf-43d5-9c29-d106ff1d76c1");

            migrationBuilder.UpdateData(
                table: "AspNetRoles",
                keyColumn: "Id",
                keyValue: "d72cc571-c363-4d19-8818-9bebb24fba93",
                column: "ConcurrencyStamp",
                value: "55f2407a-8127-45ed-be72-f335ab4a0744");

            migrationBuilder.UpdateData(
                table: "AspNetUsers",
                keyColumn: "Id",
                keyValue: "29d93d5b-efbc-4ac7-999b-7b211629d8b0",
                columns: new[] { "ConcurrencyStamp", "PasswordHash", "SecurityStamp" },
                values: new object[] { "2fe02ec2-0a62-4527-ad3c-7fc3cc69a7eb", "AQAAAAIAAYagAAAAEPtD/OHiWtFDHAgz0hGWMfr79HX4FOZtEBhgZdBLcCP6Gt0QZqbdrn178eds3ERsWQ==", "e137e283-3539-4ebc-ac9f-61edb91d22a6" });

            migrationBuilder.UpdateData(
                table: "AspNetUsers",
                keyColumn: "Id",
                keyValue: "b7359b68-b61a-4991-8c9f-b6394494b11a",
                columns: new[] { "ConcurrencyStamp", "PasswordHash", "SecurityStamp" },
                values: new object[] { "8587f222-420b-4494-b369-f51948d8a815", "AQAAAAIAAYagAAAAEPVFjZSYM9hw27SIKRzizpktC5q1lpvUHzT0IgD+n2HDxkckMl1oLh4kDXYB758bhQ==", "3de749c0-8696-4a71-950a-8a3201dde6c2" });

            migrationBuilder.UpdateData(
                table: "AspNetUsers",
                keyColumn: "Id",
                keyValue: "f422f142-09b2-40b9-a886-a14b213973d5",
                columns: new[] { "ConcurrencyStamp", "PasswordHash", "SecurityStamp" },
                values: new object[] { "95e83ef5-5c1b-473c-b787-84fe9378b540", "AQAAAAIAAYagAAAAEErkZCApHi4HnxPQd5IKsVJH+T8+I/qVJ9tGgXQuE5VXthZuRi05ylhzcAk+O0emmw==", "e70267c4-c887-43ff-b663-e6061b359675" });

            migrationBuilder.AddForeignKey(
                name: "FK_FcmUserLog_FcmLog_logId",
                table: "FcmUserLog",
                column: "logId",
                principalTable: "FcmLog",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
