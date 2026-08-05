using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SuttorLib.Migrations
{
    /// <inheritdoc />
    public partial class NewTablesForMLRecommendations : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "DownloadsAnalysis",
                columns: table => new
                {
                    UserId = table.Column<string>(type: "varchar(255)", nullable: false),
                    BookId = table.Column<string>(type: "varchar(255)", nullable: false),
                    Label = table.Column<float>(type: "float", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_DownloadsAnalysis", x => new { x.UserId, x.BookId });
                })
                .Annotation("MySQL:Charset", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "ScoredCandidates",
                columns: table => new
                {
                    UserId = table.Column<float>(type: "float", nullable: false),
                    BookId = table.Column<float>(type: "float", nullable: false),
                    Score = table.Column<float>(type: "float", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ScoredCandidates", x => new { x.UserId, x.BookId });
                })
                .Annotation("MySQL:Charset", "utf8mb4");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "DownloadsAnalysis");

            migrationBuilder.DropTable(
                name: "ScoredCandidates");
        }
    }
}
