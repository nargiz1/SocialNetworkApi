using System;
using Microsoft.EntityFrameworkCore.Migrations;

namespace FinalProject.Migrations
{
    public partial class comment : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "CommentComments");

            migrationBuilder.AddColumn<int>(
                name: "CommentId",
                table: "PostComments",
                type: "int",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_PostComments_CommentId",
                table: "PostComments",
                column: "CommentId");

            migrationBuilder.AddForeignKey(
                name: "FK_PostComments_PostComments_CommentId",
                table: "PostComments",
                column: "CommentId",
                principalTable: "PostComments",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_PostComments_PostComments_CommentId",
                table: "PostComments");

            migrationBuilder.DropIndex(
                name: "IX_PostComments_CommentId",
                table: "PostComments");

            migrationBuilder.DropColumn(
                name: "CommentId",
                table: "PostComments");

            migrationBuilder.CreateTable(
                name: "CommentComments",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    CommentId = table.Column<int>(type: "int", nullable: false),
                    Created = table.Column<DateTime>(type: "datetime2", nullable: false),
                    Text = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: false),
                    Updated = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UserId = table.Column<string>(type: "nvarchar(450)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CommentComments", x => x.Id);
                    table.ForeignKey(
                        name: "FK_CommentComments_AspNetUsers_UserId",
                        column: x => x.UserId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_CommentComments_PostComments_CommentId",
                        column: x => x.CommentId,
                        principalTable: "PostComments",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_CommentComments_CommentId",
                table: "CommentComments",
                column: "CommentId");

            migrationBuilder.CreateIndex(
                name: "IX_CommentComments_UserId",
                table: "CommentComments",
                column: "UserId");
        }
    }
}
