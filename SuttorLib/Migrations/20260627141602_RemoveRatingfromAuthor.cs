using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SuttorLib.Migrations
{
    /// <inheritdoc />
    public partial class RemoveRatingfromAuthor : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Rating",
                table: "Authors");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<float>(
                name: "Rating",
                table: "Authors",
                type: "float",
                nullable: false,
                defaultValue: 0f);
        }
    }
}
