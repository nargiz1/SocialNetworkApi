using Microsoft.EntityFrameworkCore.Migrations;

namespace FinalProject.Migrations
{
    public partial class postsUpdate : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "PostId",
                table: "PostComments",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<string>(
                name: "PostId1",
                table: "PostComments",
                type: "nvarchar(450)",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_PostComments_PostId1",
                table: "PostComments",
                column: "PostId1");

            migrationBuilder.AddForeignKey(
                name: "FK_PostComments_AspNetUsers_PostId1",
                table: "PostComments",
                column: "PostId1",
                principalTable: "AspNetUsers",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_PostComments_AspNetUsers_PostId1",
                table: "PostComments");

            migrationBuilder.DropIndex(
                name: "IX_PostComments_PostId1",
                table: "PostComments");

            migrationBuilder.DropColumn(
                name: "PostId",
                table: "PostComments");

            migrationBuilder.DropColumn(
                name: "PostId1",
                table: "PostComments");
        }
    }
}
