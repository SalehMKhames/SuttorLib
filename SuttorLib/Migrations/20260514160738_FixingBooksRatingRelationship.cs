using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SuttorLib.Migrations
{
    /// <inheritdoc />
    public partial class FixingBooksRatingRelationship : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<float>(
                name: "Rating",
                table: "Book_Rating",
                type: "float",
                nullable: false,
                defaultValue: 0f,
                oldClrType: typeof(int),
                oldType: "int",
                oldDefaultValue: 0);

            migrationBuilder.UpdateData(
                table: "AspNetRoles",
                keyColumn: "Id",
                keyValue: "60909110-f307-44d3-8d74-e9e85c5b7896",
                column: "ConcurrencyStamp",
                value: "e8ff9bd8-6748-4bbe-92a3-2dd158739b98");

            migrationBuilder.UpdateData(
                table: "AspNetRoles",
                keyColumn: "Id",
                keyValue: "b28dafd1-5b59-47f2-8af2-d510cd1ecb0b",
                column: "ConcurrencyStamp",
                value: "234842ad-d6a0-44c4-b8bf-9cc7de63f559");

            migrationBuilder.UpdateData(
                table: "AspNetRoles",
                keyColumn: "Id",
                keyValue: "d72cc571-c363-4d19-8818-9bebb24fba93",
                column: "ConcurrencyStamp",
                value: "941e62e5-1952-4343-8b3e-96e57a0b477f");

            migrationBuilder.UpdateData(
                table: "AspNetUsers",
                keyColumn: "Id",
                keyValue: "29d93d5b-efbc-4ac7-999b-7b211629d8b0",
                columns: new[] { "ConcurrencyStamp", "PasswordHash", "SecurityStamp" },
                values: new object[] { "d057a474-f6ba-49bd-aff1-8fa50a477551", "AQAAAAIAAYagAAAAEOJUkacLXu+KWf92LypmY/08zqTLDEE86Xhi/wXmlO1+Gfd6mJrA6qmq00kWLxpUHA==", "6f158157-27d1-45cb-9d8b-90586b284f25" });

            migrationBuilder.UpdateData(
                table: "AspNetUsers",
                keyColumn: "Id",
                keyValue: "b7359b68-b61a-4991-8c9f-b6394494b11a",
                columns: new[] { "ConcurrencyStamp", "PasswordHash", "SecurityStamp" },
                values: new object[] { "61d283a7-1899-4586-82c7-c2db335e9d36", "AQAAAAIAAYagAAAAED63Kyer3t3ZbMR3xqloPxSvyYRw50LdN6nXKY1QGrtOahckIsGY7okQsdGPP3jvDw==", "d6da99b6-ed30-4071-b87c-45a142875ef3" });

            migrationBuilder.UpdateData(
                table: "AspNetUsers",
                keyColumn: "Id",
                keyValue: "f422f142-09b2-40b9-a886-a14b213973d5",
                columns: new[] { "ConcurrencyStamp", "PasswordHash", "SecurityStamp" },
                values: new object[] { "b6374ad5-9caf-4a4c-a11c-5223c4a6474f", "AQAAAAIAAYagAAAAEMudi3VZ/h6gP6GENvwN/OGam8Bkb8Xi7wijHgqOQJnEC+cdcrnXHnDueAaACYLD+A==", "6e911042-d678-454e-877f-e3dc56dc496f" });

            migrationBuilder.CreateIndex(
                name: "IX_Book_Rating_BookId",
                table: "Book_Rating",
                column: "BookId");

            migrationBuilder.AddForeignKey(
                name: "FK_Book_Rating_AspNetUsers_UserId",
                table: "Book_Rating",
                column: "UserId",
                principalTable: "AspNetUsers",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_Book_Rating_Books_BookId",
                table: "Book_Rating",
                column: "BookId",
                principalTable: "Books",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Book_Rating_AspNetUsers_UserId",
                table: "Book_Rating");

            migrationBuilder.DropForeignKey(
                name: "FK_Book_Rating_Books_BookId",
                table: "Book_Rating");

            migrationBuilder.DropIndex(
                name: "IX_Book_Rating_BookId",
                table: "Book_Rating");

            migrationBuilder.AlterColumn<int>(
                name: "Rating",
                table: "Book_Rating",
                type: "int",
                nullable: false,
                defaultValue: 0,
                oldClrType: typeof(float),
                oldType: "float",
                oldDefaultValue: 0f);

            migrationBuilder.UpdateData(
                table: "AspNetRoles",
                keyColumn: "Id",
                keyValue: "60909110-f307-44d3-8d74-e9e85c5b7896",
                column: "ConcurrencyStamp",
                value: "d8f1ea89-f207-4bd6-a808-ccfa3eeeb9be");

            migrationBuilder.UpdateData(
                table: "AspNetRoles",
                keyColumn: "Id",
                keyValue: "b28dafd1-5b59-47f2-8af2-d510cd1ecb0b",
                column: "ConcurrencyStamp",
                value: "a8012466-74d1-4557-a184-117b132bde16");

            migrationBuilder.UpdateData(
                table: "AspNetRoles",
                keyColumn: "Id",
                keyValue: "d72cc571-c363-4d19-8818-9bebb24fba93",
                column: "ConcurrencyStamp",
                value: "5766a9f9-bb47-46a4-af1d-b4ef067302b8");

            migrationBuilder.UpdateData(
                table: "AspNetUsers",
                keyColumn: "Id",
                keyValue: "29d93d5b-efbc-4ac7-999b-7b211629d8b0",
                columns: new[] { "ConcurrencyStamp", "PasswordHash", "SecurityStamp" },
                values: new object[] { "26e565ab-8f98-4f4c-be2e-667555da9d19", "AQAAAAIAAYagAAAAEPta4qe4hnZVJJvlcLby/KD9zkYYFYzzBbuWBo/kD981NgP9GP3V9yT1GJJC2xO//Q==", "657e8106-42eb-4352-b5f0-6a67bc380fbb" });

            migrationBuilder.UpdateData(
                table: "AspNetUsers",
                keyColumn: "Id",
                keyValue: "b7359b68-b61a-4991-8c9f-b6394494b11a",
                columns: new[] { "ConcurrencyStamp", "PasswordHash", "SecurityStamp" },
                values: new object[] { "3c763efa-90c2-4342-8019-38393efcf7dd", "AQAAAAIAAYagAAAAECnVNISpm8fOvHFz6Fn0OSeGe1a220+Kr7wHsJyJFI4bm9jCu8WDxpT57BvIGBLnYg==", "6b226724-eda3-4360-8f70-6b0d32d3d0a4" });

            migrationBuilder.UpdateData(
                table: "AspNetUsers",
                keyColumn: "Id",
                keyValue: "f422f142-09b2-40b9-a886-a14b213973d5",
                columns: new[] { "ConcurrencyStamp", "PasswordHash", "SecurityStamp" },
                values: new object[] { "6950255b-638f-4288-9c95-63fb6ae029a1", "AQAAAAIAAYagAAAAEHUNGwOqumxP9QSvkCAyZAlTIXhf8Tx102yAw/5sqDxulYaLYTAmw7Mb3i+/l4izIA==", "c1e5362f-0cb4-48f1-a1a2-5025f0edb6a8" });
        }
    }
}
