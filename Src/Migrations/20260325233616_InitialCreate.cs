using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace KyudosudokuWebsite.Migrations
{
    /// <inheritdoc />
    public partial class InitialCreate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Puzzles",
                columns: table => new
                {
                    PuzzleID = table.Column<int>(type: "int", nullable: false),
                    KyudokuGrids = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Invalid = table.Column<bool>(type: "bit", nullable: false),
                    Constraints = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    ConstraintNames = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    NumConstraints = table.Column<int>(type: "int", nullable: false),
                    AverageTime = table.Column<double>(type: "float", nullable: true),
                    TimeToGenerate = table.Column<int>(type: "int", nullable: true),
                    Generated = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Puzzles", x => x.PuzzleID);
                });

            migrationBuilder.CreateTable(
                name: "Sessions",
                columns: table => new
                {
                    SessionID = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    UserID = table.Column<int>(type: "int", nullable: true),
                    LastLogin = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Sessions", x => x.SessionID);
                });

            migrationBuilder.CreateTable(
                name: "UserPuzzles",
                columns: table => new
                {
                    UserID = table.Column<int>(type: "int", nullable: false),
                    PuzzleID = table.Column<int>(type: "int", nullable: false),
                    Progess = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Solved = table.Column<bool>(type: "bit", nullable: false),
                    Time = table.Column<int>(type: "int", nullable: false),
                    SolveTime = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_UserPuzzles", x => new { x.UserID, x.PuzzleID });
                });

            migrationBuilder.CreateTable(
                name: "Users",
                columns: table => new
                {
                    UserID = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Username = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    PasswordHash = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    EmailAddress = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    ShowErrors = table.Column<bool>(type: "bit", nullable: false),
                    SemitransparentXs = table.Column<bool>(type: "bit", nullable: false),
                    ShowSolveTime = table.Column<bool>(type: "bit", nullable: false),
                    PlayInvalidSound = table.Column<bool>(type: "bit", nullable: false),
                    BackspaceOption = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Users", x => x.UserID);
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Puzzles");

            migrationBuilder.DropTable(
                name: "Sessions");

            migrationBuilder.DropTable(
                name: "UserPuzzles");

            migrationBuilder.DropTable(
                name: "Users");
        }
    }
}
