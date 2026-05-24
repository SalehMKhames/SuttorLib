using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SuttorLib.Migrations
{
    /// <inheritdoc />
    public partial class ChangedRelationBooksAndLanguageToOneToMany : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "BookLanguages");

            migrationBuilder.DropIndex(
                name: "IX_Languages_LanguageCode",
                table: "Languages");

            migrationBuilder.DropColumn(
                name: "LanguageCode",
                table: "Languages");

            migrationBuilder.AddColumn<string>(
                name: "LanguageId",
                table: "Books",
                type: "varchar(255)",
                nullable: false,
                defaultValue: "");

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
                columns: new[] { "ConcurrencyStamp", "PasswordHash", "SecurityStamp" },
                values: new object[] { "221ff105-8ff0-4bea-b179-3c68bf0a1352", "AQAAAAIAAYagAAAAEEZdAp3gTTg2TLfiT/r6kY1I8QCJvo4y7CD/6+BCrVH1YFCrIwDW2Ck1sF53bxeF+A==", "5b70aa76-fd4e-4143-8c3f-730f623498f6" });

            migrationBuilder.UpdateData(
                table: "AspNetUsers",
                keyColumn: "Id",
                keyValue: "b7359b68-b61a-4991-8c9f-b6394494b11a",
                columns: new[] { "ConcurrencyStamp", "PasswordHash", "SecurityStamp" },
                values: new object[] { "353134f5-4b1d-4cbe-b8f1-67583d6aeecb", "AQAAAAIAAYagAAAAEOzI4mM15NzdIya/ujoLZjGbVgcEatbuZNHvwG5T+qpqqp5IMo8MBVtriUPV99ivOA==", "2054fdf5-bf51-4a63-86fd-d0b0eb3ddc25" });

            migrationBuilder.UpdateData(
                table: "AspNetUsers",
                keyColumn: "Id",
                keyValue: "f422f142-09b2-40b9-a886-a14b213973d5",
                columns: new[] { "ConcurrencyStamp", "PasswordHash", "SecurityStamp" },
                values: new object[] { "34745da4-8a59-459e-8073-9bc1c9270925", "AQAAAAIAAYagAAAAELDfMaAcaTUQDOIxWhZ+VvtOa+Av//akTQXKD1CjLhzUHEYL8ktfOS//lvXal2HlOw==", "99fb352c-cb26-4cd9-a7b0-8418ed3e432c" });

            migrationBuilder.CreateIndex(
                name: "IX_Books_LanguageId",
                table: "Books",
                column: "LanguageId");

            migrationBuilder.AddForeignKey(
                name: "FK_Books_Languages_LanguageId",
                table: "Books",
                column: "LanguageId",
                principalTable: "Languages",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Books_Languages_LanguageId",
                table: "Books");

            migrationBuilder.DropIndex(
                name: "IX_Books_LanguageId",
                table: "Books");

            migrationBuilder.DropColumn(
                name: "LanguageId",
                table: "Books");

            migrationBuilder.AddColumn<string>(
                name: "LanguageCode",
                table: "Languages",
                type: "varchar(255)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.CreateTable(
                name: "BookLanguages",
                columns: table => new
                {
                    LanguageId = table.Column<string>(type: "varchar(255)", nullable: false),
                    BookId = table.Column<string>(type: "varchar(255)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_BookLanguages", x => new { x.LanguageId, x.BookId });
                    table.ForeignKey(
                        name: "FK_BookLanguages_Books_BookId",
                        column: x => x.BookId,
                        principalTable: "Books",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_BookLanguages_Languages_LanguageId",
                        column: x => x.LanguageId,
                        principalTable: "Languages",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                })
                .Annotation("MySQL:Charset", "utf8mb4");

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

            migrationBuilder.UpdateData(
                table: "Languages",
                keyColumn: "Id",
                keyValue: "0eb15275-2db6-4cd4-b6f0-915d2fe49503",
                column: "LanguageCode",
                value: "ko");

            migrationBuilder.UpdateData(
                table: "Languages",
                keyColumn: "Id",
                keyValue: "163bd8eb-4d53-4a7b-a0bc-44d5b410c185",
                column: "LanguageCode",
                value: "hi");

            migrationBuilder.UpdateData(
                table: "Languages",
                keyColumn: "Id",
                keyValue: "1a88de30-80b1-4491-a6ec-333fa0174933",
                column: "LanguageCode",
                value: "zh");

            migrationBuilder.UpdateData(
                table: "Languages",
                keyColumn: "Id",
                keyValue: "20c60f9d-d8ad-498c-a960-02ccdec83a63",
                column: "LanguageCode",
                value: "ro");

            migrationBuilder.UpdateData(
                table: "Languages",
                keyColumn: "Id",
                keyValue: "2df2ecaf-fe98-46f5-8663-a724b63a1ab8",
                column: "LanguageCode",
                value: "pa");

            migrationBuilder.UpdateData(
                table: "Languages",
                keyColumn: "Id",
                keyValue: "2f811e46-26d8-4606-9e22-7abbce66f697",
                column: "LanguageCode",
                value: "sk");

            migrationBuilder.UpdateData(
                table: "Languages",
                keyColumn: "Id",
                keyValue: "33165761-6725-4d75-af4b-6794a72e9270",
                column: "LanguageCode",
                value: "da");

            migrationBuilder.UpdateData(
                table: "Languages",
                keyColumn: "Id",
                keyValue: "35923bc4-c519-4748-a17a-8fc6f030dde2",
                column: "LanguageCode",
                value: "fil");

            migrationBuilder.UpdateData(
                table: "Languages",
                keyColumn: "Id",
                keyValue: "3691f261-a07b-40dd-bcf6-c48a91d31347",
                column: "LanguageCode",
                value: "no");

            migrationBuilder.UpdateData(
                table: "Languages",
                keyColumn: "Id",
                keyValue: "41fe6770-e040-416e-adf4-85075230e3ba",
                column: "LanguageCode",
                value: "sv");

            migrationBuilder.UpdateData(
                table: "Languages",
                keyColumn: "Id",
                keyValue: "471ec16b-4a25-49e6-a1a4-9e9fb24d06d0",
                column: "LanguageCode",
                value: "fi");

            migrationBuilder.UpdateData(
                table: "Languages",
                keyColumn: "Id",
                keyValue: "490e8980-67c0-4cc5-bad0-9cd022bbef49",
                column: "LanguageCode",
                value: "he");

            migrationBuilder.UpdateData(
                table: "Languages",
                keyColumn: "Id",
                keyValue: "4b452292-7c43-4b83-9b99-8821f84a3e40",
                column: "LanguageCode",
                value: "bn");

            migrationBuilder.UpdateData(
                table: "Languages",
                keyColumn: "Id",
                keyValue: "4dda4135-d8e2-4bd1-aaea-c66bb06f0489",
                column: "LanguageCode",
                value: "vi");

            migrationBuilder.UpdateData(
                table: "Languages",
                keyColumn: "Id",
                keyValue: "4ef36e82-cc2b-4e52-8fa1-8b1c46e86c0b",
                column: "LanguageCode",
                value: "bg");

            migrationBuilder.UpdateData(
                table: "Languages",
                keyColumn: "Id",
                keyValue: "4f4d5e66-ca69-4364-a6c6-7ac7e6748248",
                column: "LanguageCode",
                value: "it");

            migrationBuilder.UpdateData(
                table: "Languages",
                keyColumn: "Id",
                keyValue: "593e8d92-6b37-4fff-a00c-41197b4b6386",
                column: "LanguageCode",
                value: "ja");

            migrationBuilder.UpdateData(
                table: "Languages",
                keyColumn: "Id",
                keyValue: "5b56f76e-a943-4a33-ae52-405f992c0ee1",
                column: "LanguageCode",
                value: "en");

            migrationBuilder.UpdateData(
                table: "Languages",
                keyColumn: "Id",
                keyValue: "5cd5b25e-7685-4181-9c6a-c22761acc729",
                column: "LanguageCode",
                value: "ga");

            migrationBuilder.UpdateData(
                table: "Languages",
                keyColumn: "Id",
                keyValue: "63769d61-fd77-4b02-9ab5-bb3d8de5dd09",
                column: "LanguageCode",
                value: "id");

            migrationBuilder.UpdateData(
                table: "Languages",
                keyColumn: "Id",
                keyValue: "6a3e435d-0fb4-4698-b83d-6e9dc0e9c3e0",
                column: "LanguageCode",
                value: "hr");

            migrationBuilder.UpdateData(
                table: "Languages",
                keyColumn: "Id",
                keyValue: "74ff7ceb-5df6-434c-a079-0650658df292",
                column: "LanguageCode",
                value: "pt");

            migrationBuilder.UpdateData(
                table: "Languages",
                keyColumn: "Id",
                keyValue: "7580183c-ea56-490c-9880-e665b73b634e",
                column: "LanguageCode",
                value: "fr");

            migrationBuilder.UpdateData(
                table: "Languages",
                keyColumn: "Id",
                keyValue: "7c44e07d-f447-48c6-9b99-7dff1596f5cc",
                column: "LanguageCode",
                value: "lv");

            migrationBuilder.UpdateData(
                table: "Languages",
                keyColumn: "Id",
                keyValue: "87f53ee0-6654-49dd-93ec-49f553819714",
                column: "LanguageCode",
                value: "nl");

            migrationBuilder.UpdateData(
                table: "Languages",
                keyColumn: "Id",
                keyValue: "8f3b108f-1b75-4b5a-8b2e-be5c91ef8f80",
                column: "LanguageCode",
                value: "hu");

            migrationBuilder.UpdateData(
                table: "Languages",
                keyColumn: "Id",
                keyValue: "9fbe1970-694f-4613-b7a4-a3ce4e50a3f4",
                column: "LanguageCode",
                value: "el");

            migrationBuilder.UpdateData(
                table: "Languages",
                keyColumn: "Id",
                keyValue: "a291c167-e5bb-42dc-a4f5-617e4f1ec055",
                column: "LanguageCode",
                value: "cs");

            migrationBuilder.UpdateData(
                table: "Languages",
                keyColumn: "Id",
                keyValue: "a5bcf190-d4a5-4285-bfca-8e7888fede68",
                column: "LanguageCode",
                value: "es");

            migrationBuilder.UpdateData(
                table: "Languages",
                keyColumn: "Id",
                keyValue: "a7381524-0b42-4105-b41e-d7576bb7faf4",
                column: "LanguageCode",
                value: "pl");

            migrationBuilder.UpdateData(
                table: "Languages",
                keyColumn: "Id",
                keyValue: "b41fd658-2252-423f-a891-4176875dd4c5",
                column: "LanguageCode",
                value: "ur");

            migrationBuilder.UpdateData(
                table: "Languages",
                keyColumn: "Id",
                keyValue: "c367d2d7-1850-4e99-af79-26f433383b8c",
                column: "LanguageCode",
                value: "de");

            migrationBuilder.UpdateData(
                table: "Languages",
                keyColumn: "Id",
                keyValue: "c54fc06d-e0e7-4686-91f9-1b4885fb85d7",
                column: "LanguageCode",
                value: "fa");

            migrationBuilder.UpdateData(
                table: "Languages",
                keyColumn: "Id",
                keyValue: "c7b70aeb-c5b3-437d-9f51-52a84634ad23",
                column: "LanguageCode",
                value: "ar");

            migrationBuilder.UpdateData(
                table: "Languages",
                keyColumn: "Id",
                keyValue: "c8a88ae4-18dc-44ed-b46e-9cb92f97a969",
                column: "LanguageCode",
                value: "ru");

            migrationBuilder.UpdateData(
                table: "Languages",
                keyColumn: "Id",
                keyValue: "ca83826e-5a1d-444b-a95e-7e03853933f0",
                column: "LanguageCode",
                value: "ms");

            migrationBuilder.UpdateData(
                table: "Languages",
                keyColumn: "Id",
                keyValue: "d0e3574e-3a64-420b-85bf-88c5ff1562fd",
                column: "LanguageCode",
                value: "et");

            migrationBuilder.UpdateData(
                table: "Languages",
                keyColumn: "Id",
                keyValue: "da42ed01-8bd6-4d4b-a501-c406b8b64df8",
                column: "LanguageCode",
                value: "lt");

            migrationBuilder.UpdateData(
                table: "Languages",
                keyColumn: "Id",
                keyValue: "db567ea5-c771-43da-a1d8-e0d67e411177",
                column: "LanguageCode",
                value: "tr");

            migrationBuilder.UpdateData(
                table: "Languages",
                keyColumn: "Id",
                keyValue: "e3661f50-9ba2-45d6-b2c7-f367637e00dc",
                column: "LanguageCode",
                value: "th");

            migrationBuilder.UpdateData(
                table: "Languages",
                keyColumn: "Id",
                keyValue: "ed35ec1c-5556-4553-b8f1-d2f0794a4066",
                column: "LanguageCode",
                value: "sl");

            migrationBuilder.UpdateData(
                table: "Languages",
                keyColumn: "Id",
                keyValue: "f83d2700-7f50-4a49-8885-e53dce76c117",
                column: "LanguageCode",
                value: "sr");

            migrationBuilder.UpdateData(
                table: "Languages",
                keyColumn: "Id",
                keyValue: "f982b583-a50c-491e-9ddc-e434542fc643",
                column: "LanguageCode",
                value: "uk");

            migrationBuilder.CreateIndex(
                name: "IX_Languages_LanguageCode",
                table: "Languages",
                column: "LanguageCode",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_BookLanguages_BookId",
                table: "BookLanguages",
                column: "BookId");
        }
    }
}
