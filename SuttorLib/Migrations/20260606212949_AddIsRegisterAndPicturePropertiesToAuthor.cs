using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SuttorLib.Migrations
{
    /// <inheritdoc />
    public partial class AddIsRegisterAndPicturePropertiesToAuthor : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<string>(
                name: "Description",
                table: "Authors",
                type: "longtext",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "longtext");

            migrationBuilder.AddColumn<bool>(
                name: "IsRegistered",
                table: "Authors",
                type: "tinyint(1)",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<string>(
                name: "Picture",
                table: "Authors",
                type: "longtext",
                nullable: true);

            migrationBuilder.UpdateData(
                table: "AspNetRoles",
                keyColumn: "Id",
                keyValue: "60909110-f307-44d3-8d74-e9e85c5b7896",
                column: "ConcurrencyStamp",
                value: "bfb24114-5f6f-41b2-9f42-f0b2710816b2");

            migrationBuilder.UpdateData(
                table: "AspNetRoles",
                keyColumn: "Id",
                keyValue: "b28dafd1-5b59-47f2-8af2-d510cd1ecb0b",
                column: "ConcurrencyStamp",
                value: "15acdbe0-3101-435c-ad11-eebdd870e9cc");

            migrationBuilder.UpdateData(
                table: "AspNetRoles",
                keyColumn: "Id",
                keyValue: "d72cc571-c363-4d19-8818-9bebb24fba93",
                column: "ConcurrencyStamp",
                value: "8c0bafae-e279-4b36-804d-57005e4c19df");

            migrationBuilder.UpdateData(
                table: "AspNetUsers",
                keyColumn: "Id",
                keyValue: "29d93d5b-efbc-4ac7-999b-7b211629d8b0",
                columns: new[] { "ConcurrencyStamp", "PasswordHash", "SecurityStamp" },
                values: new object[] { "b63d8208-7363-464a-9b52-86ceab03ea8c", "AQAAAAIAAYagAAAAEFvmEqziLsGdu+WrB+2SjCUWbZm3Haaunx9Fr7U3k/me+JcoM5XcwZ6s0S1obaZSgg==", "79816fb5-08bb-434c-b32d-3c821f466330" });

            migrationBuilder.UpdateData(
                table: "AspNetUsers",
                keyColumn: "Id",
                keyValue: "b7359b68-b61a-4991-8c9f-b6394494b11a",
                columns: new[] { "ConcurrencyStamp", "PasswordHash", "SecurityStamp" },
                values: new object[] { "8de62aaa-22cc-4b63-b185-24e8619dcc46", "AQAAAAIAAYagAAAAEAted8WmZqTTiAbZ9hpLUClzK9Jpg2iqEAyLYPKw+ZQdhxkFqpmi6KnPEGSU78rZzQ==", "2c9d1dfa-27ca-42a0-aed7-14e67bc74327" });

            migrationBuilder.UpdateData(
                table: "AspNetUsers",
                keyColumn: "Id",
                keyValue: "f422f142-09b2-40b9-a886-a14b213973d5",
                columns: new[] { "ConcurrencyStamp", "PasswordHash", "SecurityStamp" },
                values: new object[] { "133a97d1-beb4-4035-931c-b433d5916243", "AQAAAAIAAYagAAAAEJHaj6yVooq8QCoZcp5G4sp3JpBH7dpq8PPJe0D8Rx8IpuKi/gBMVhk4A82rovyXbg==", "7ec53d6a-754f-4182-91c1-0d78f6401414" });

            migrationBuilder.UpdateData(
                table: "Authors",
                keyColumn: "Id",
                keyValue: "0c238a31-fa21-469f-903a-b0b075244eac",
                column: "Picture",
                value: null);

            migrationBuilder.UpdateData(
                table: "Authors",
                keyColumn: "Id",
                keyValue: "185ed5ca-0ac3-4dee-8054-14e20608b9d9",
                column: "Picture",
                value: null);

            migrationBuilder.UpdateData(
                table: "Authors",
                keyColumn: "Id",
                keyValue: "2049063d-9270-43a6-9288-a80627e0be80",
                column: "Picture",
                value: null);

            migrationBuilder.UpdateData(
                table: "Authors",
                keyColumn: "Id",
                keyValue: "21850d05-c2a8-453b-9e6c-40d29b53571b",
                column: "Picture",
                value: null);

            migrationBuilder.UpdateData(
                table: "Authors",
                keyColumn: "Id",
                keyValue: "25ecc614-5e01-4228-9025-02a4b65125c8",
                column: "Picture",
                value: null);

            migrationBuilder.UpdateData(
                table: "Authors",
                keyColumn: "Id",
                keyValue: "2820b458-531b-4db4-abb2-99cd3e50c3b4",
                column: "Picture",
                value: null);

            migrationBuilder.UpdateData(
                table: "Authors",
                keyColumn: "Id",
                keyValue: "2c4cf33a-c18d-46f3-ac91-e25204cadf64",
                column: "Picture",
                value: null);

            migrationBuilder.UpdateData(
                table: "Authors",
                keyColumn: "Id",
                keyValue: "36cfd3ba-0b17-4a48-afb9-d7bdec093f43",
                column: "Picture",
                value: null);

            migrationBuilder.UpdateData(
                table: "Authors",
                keyColumn: "Id",
                keyValue: "3e8d1068-6a4f-4786-b48b-e110ee1061a4",
                column: "Picture",
                value: null);

            migrationBuilder.UpdateData(
                table: "Authors",
                keyColumn: "Id",
                keyValue: "3ec6a6f3-8048-4642-903e-6c2bf8333ca0",
                column: "Picture",
                value: null);

            migrationBuilder.UpdateData(
                table: "Authors",
                keyColumn: "Id",
                keyValue: "3ee308c6-3016-4cc8-ab89-a48a60e518a4",
                column: "Picture",
                value: null);

            migrationBuilder.UpdateData(
                table: "Authors",
                keyColumn: "Id",
                keyValue: "4d8243ec-f8f6-47df-b5f7-fa99b2627784",
                column: "Picture",
                value: null);

            migrationBuilder.UpdateData(
                table: "Authors",
                keyColumn: "Id",
                keyValue: "5d32e53f-134b-4c07-a870-61368bcf0dfc",
                column: "Picture",
                value: null);

            migrationBuilder.UpdateData(
                table: "Authors",
                keyColumn: "Id",
                keyValue: "6168d9bf-9ce6-421f-84cd-3fff95ba65b8",
                column: "Picture",
                value: null);

            migrationBuilder.UpdateData(
                table: "Authors",
                keyColumn: "Id",
                keyValue: "671fcbf5-46e5-421b-8fbc-ef3d86502e23",
                column: "Picture",
                value: null);

            migrationBuilder.UpdateData(
                table: "Authors",
                keyColumn: "Id",
                keyValue: "6d12378e-21fa-451d-a6d8-c39e4a42c8fa",
                column: "Picture",
                value: null);

            migrationBuilder.UpdateData(
                table: "Authors",
                keyColumn: "Id",
                keyValue: "793f21bf-7f1c-48bb-ae64-0d56bbeacd12",
                column: "Picture",
                value: null);

            migrationBuilder.UpdateData(
                table: "Authors",
                keyColumn: "Id",
                keyValue: "7be65b0d-3c1d-49d3-a529-e1bb1055331a",
                column: "Picture",
                value: null);

            migrationBuilder.UpdateData(
                table: "Authors",
                keyColumn: "Id",
                keyValue: "7c154694-cfa4-43ea-868e-cd7ecdb43641",
                column: "Picture",
                value: null);

            migrationBuilder.UpdateData(
                table: "Authors",
                keyColumn: "Id",
                keyValue: "7d265cb9-77bf-47db-b2b9-468a3441d2d9",
                column: "Picture",
                value: null);

            migrationBuilder.UpdateData(
                table: "Authors",
                keyColumn: "Id",
                keyValue: "8baa507b-fe2b-42f5-9296-b7aedfbfc596",
                column: "Picture",
                value: null);

            migrationBuilder.UpdateData(
                table: "Authors",
                keyColumn: "Id",
                keyValue: "8e8dcbf0-2e52-42f6-8ee8-ffa962a6b8e9",
                column: "Picture",
                value: null);

            migrationBuilder.UpdateData(
                table: "Authors",
                keyColumn: "Id",
                keyValue: "9039f00a-f4e3-4282-a374-cb38edf9b09d",
                column: "Picture",
                value: null);

            migrationBuilder.UpdateData(
                table: "Authors",
                keyColumn: "Id",
                keyValue: "95b35c34-593f-4701-8f15-4a08c4108db7",
                column: "Picture",
                value: null);

            migrationBuilder.UpdateData(
                table: "Authors",
                keyColumn: "Id",
                keyValue: "a03ef8b6-33f6-4fd4-93d9-8bea3d2e4f3b",
                column: "Picture",
                value: null);

            migrationBuilder.UpdateData(
                table: "Authors",
                keyColumn: "Id",
                keyValue: "a0882ae2-3942-4ef6-958b-896b367eba96",
                column: "Picture",
                value: null);

            migrationBuilder.UpdateData(
                table: "Authors",
                keyColumn: "Id",
                keyValue: "aaf67e2b-61de-455f-94d9-cfd0bb0f2e88",
                column: "Picture",
                value: null);

            migrationBuilder.UpdateData(
                table: "Authors",
                keyColumn: "Id",
                keyValue: "d2d4665f-2cc0-4181-84e1-8ae896c33496",
                column: "Picture",
                value: null);

            migrationBuilder.UpdateData(
                table: "Authors",
                keyColumn: "Id",
                keyValue: "d3f2725b-2fd7-481b-948a-bf24b3362eab",
                column: "Picture",
                value: null);

            migrationBuilder.UpdateData(
                table: "Authors",
                keyColumn: "Id",
                keyValue: "d58daee2-1ccb-4c81-bdcd-5620a0ff1ac1",
                column: "Picture",
                value: null);

            migrationBuilder.UpdateData(
                table: "Authors",
                keyColumn: "Id",
                keyValue: "d7a1117e-5f37-4859-81a1-9bf677d5a876",
                column: "Picture",
                value: null);

            migrationBuilder.UpdateData(
                table: "Authors",
                keyColumn: "Id",
                keyValue: "e166ab33-e284-4324-95ea-fb3b9348fbde",
                column: "Picture",
                value: null);

            migrationBuilder.UpdateData(
                table: "Authors",
                keyColumn: "Id",
                keyValue: "e339256a-dfc3-45bb-b5c3-4bf9f3526058",
                column: "Picture",
                value: null);

            migrationBuilder.UpdateData(
                table: "Authors",
                keyColumn: "Id",
                keyValue: "e63670dc-f437-4891-becf-c031424e4378",
                column: "Picture",
                value: null);

            migrationBuilder.UpdateData(
                table: "Authors",
                keyColumn: "Id",
                keyValue: "e69a8300-c804-4f12-b3e7-6465e31cbcd9",
                column: "Picture",
                value: null);

            migrationBuilder.UpdateData(
                table: "Authors",
                keyColumn: "Id",
                keyValue: "e74ce8f5-c6aa-4827-96dd-0193cd373e62",
                column: "Picture",
                value: null);

            migrationBuilder.UpdateData(
                table: "Authors",
                keyColumn: "Id",
                keyValue: "e9761dbd-fe17-4902-a55d-5719d6895146",
                column: "Picture",
                value: null);

            migrationBuilder.UpdateData(
                table: "Authors",
                keyColumn: "Id",
                keyValue: "eb886e63-b3ce-4fa9-8199-cb3b482ca035",
                column: "Picture",
                value: null);

            migrationBuilder.UpdateData(
                table: "Authors",
                keyColumn: "Id",
                keyValue: "ed99c064-7b5f-4135-b0a2-27788f18bbd5",
                column: "Picture",
                value: null);

            migrationBuilder.UpdateData(
                table: "Authors",
                keyColumn: "Id",
                keyValue: "edfe02ee-24ae-4b94-a99d-c7bf42df3292",
                column: "Picture",
                value: null);

            migrationBuilder.UpdateData(
                table: "Authors",
                keyColumn: "Id",
                keyValue: "ee012952-686e-4dd0-a58c-26f45540a2ca",
                column: "Picture",
                value: null);

            migrationBuilder.UpdateData(
                table: "Authors",
                keyColumn: "Id",
                keyValue: "efbc8877-e432-4ede-8b0b-6cdb92e97476",
                column: "Picture",
                value: null);

            migrationBuilder.UpdateData(
                table: "Authors",
                keyColumn: "Id",
                keyValue: "fb082272-063a-4389-8dca-4e83e19cafb6",
                column: "Picture",
                value: null);

            migrationBuilder.UpdateData(
                table: "Authors",
                keyColumn: "Id",
                keyValue: "fcd3bf3a-5aaa-4f9d-a75a-f6da3809541b",
                column: "Picture",
                value: null);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "IsRegistered",
                table: "Authors");

            migrationBuilder.DropColumn(
                name: "Picture",
                table: "Authors");

            migrationBuilder.AlterColumn<string>(
                name: "Description",
                table: "Authors",
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
    }
}
