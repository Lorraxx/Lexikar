using Microsoft.EntityFrameworkCore.Migrations;

namespace Lexikar.Migrations
{
    public partial class InitialMigration : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "CorpusFiles",
                columns: table => new
                {
                    ID = table.Column<int>(nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Name = table.Column<string>(nullable: true),
                    CloneID = table.Column<int>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CorpusFiles", x => x.ID);
                });

            migrationBuilder.CreateTable(
                name: "DescTexts",
                columns: table => new
                {
                    ID = table.Column<int>(nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    text = table.Column<string>(nullable: true),
                    what = table.Column<string>(nullable: true),
                    CloneID = table.Column<int>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_DescTexts", x => x.ID);
                });

            migrationBuilder.CreateTable(
                name: "Translations",
                columns: table => new
                {
                    ID = table.Column<int>(nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Nom = table.Column<bool>(nullable: false),
                    Verb = table.Column<bool>(nullable: false),
                    Ost = table.Column<bool>(nullable: false),
                    FR = table.Column<string>(nullable: true),
                    CS = table.Column<string>(nullable: true),
                    Zdroj = table.Column<string>(nullable: true),
                    Systematika = table.Column<string>(nullable: true),
                    Nezamnenovat = table.Column<string>(nullable: true),
                    Poznamka = table.Column<string>(nullable: true),
                    CloneID = table.Column<int>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Translations", x => x.ID);
                });

            migrationBuilder.CreateTable(
                name: "CorpusSources",
                columns: table => new
                {
                    ID = table.Column<int>(nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    TextRow = table.Column<string>(nullable: true),
                    RowNumber = table.Column<int>(nullable: false),
                    FileSourceID = table.Column<int>(nullable: false),
                    CloneID = table.Column<int>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CorpusSources", x => x.ID);
                    table.ForeignKey(
                        name: "FK_CorpusSources_CorpusFiles_FileSourceID",
                        column: x => x.FileSourceID,
                        principalTable: "CorpusFiles",
                        principalColumn: "ID",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_CorpusSources_FileSourceID",
                table: "CorpusSources",
                column: "FileSourceID");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "CorpusSources");

            migrationBuilder.DropTable(
                name: "DescTexts");

            migrationBuilder.DropTable(
                name: "Translations");

            migrationBuilder.DropTable(
                name: "CorpusFiles");
        }
    }
}
