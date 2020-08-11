using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Migrations;

namespace OxyProcess.Migrations
{
    public partial class add_tbl_Special_Permission : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "SpecialPermission",
                columns: table => new
                {
                    Id = table.Column<int>(nullable: false)
                        .Annotation("SqlServer:ValueGenerationStrategy", SqlServerValueGenerationStrategy.IdentityColumn),
                    Requested_User = table.Column<int>(nullable: false),
                    TagId = table.Column<string>(nullable: true),
                    Unique_Template_Id = table.Column<int>(nullable: false),
                    Company_Id = table.Column<int>(nullable: false),
                    AccessGranted = table.Column<bool>(nullable: false),
                    Rejected = table.Column<bool>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SpecialPermission", x => x.Id);
                });
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "SpecialPermission");
        }
    }
}
