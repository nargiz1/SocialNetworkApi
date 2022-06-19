using System;
using Microsoft.EntityFrameworkCore.Migrations;

namespace FinalProject.Migrations
{
    public partial class chatsUodate : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_GroupChatToUser_GroupChat_GroupChatId",
                table: "GroupChatToUser");

            migrationBuilder.DropPrimaryKey(
                name: "PK_GroupChat",
                table: "GroupChat");

            migrationBuilder.RenameTable(
                name: "GroupChat",
                newName: "GroupChats");

            migrationBuilder.AddPrimaryKey(
                name: "PK_GroupChats",
                table: "GroupChats",
                column: "Id");

            migrationBuilder.CreateTable(
                name: "PrivateChats",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    UserOneId = table.Column<string>(type: "nvarchar(450)", nullable: true),
                    UserTwoId = table.Column<string>(type: "nvarchar(450)", nullable: true),
                    Created = table.Column<DateTime>(type: "datetime2", nullable: false),
                    Updated = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PrivateChats", x => x.Id);
                    table.ForeignKey(
                        name: "FK_PrivateChats_AspNetUsers_UserOneId",
                        column: x => x.UserOneId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_PrivateChats_AspNetUsers_UserTwoId",
                        column: x => x.UserTwoId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_PrivateChats_UserOneId",
                table: "PrivateChats",
                column: "UserOneId");

            migrationBuilder.CreateIndex(
                name: "IX_PrivateChats_UserTwoId",
                table: "PrivateChats",
                column: "UserTwoId");

            migrationBuilder.AddForeignKey(
                name: "FK_GroupChatToUser_GroupChats_GroupChatId",
                table: "GroupChatToUser",
                column: "GroupChatId",
                principalTable: "GroupChats",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_GroupChatToUser_GroupChats_GroupChatId",
                table: "GroupChatToUser");

            migrationBuilder.DropTable(
                name: "PrivateChats");

            migrationBuilder.DropPrimaryKey(
                name: "PK_GroupChats",
                table: "GroupChats");

            migrationBuilder.RenameTable(
                name: "GroupChats",
                newName: "GroupChat");

            migrationBuilder.AddPrimaryKey(
                name: "PK_GroupChat",
                table: "GroupChat",
                column: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_GroupChatToUser_GroupChat_GroupChatId",
                table: "GroupChatToUser",
                column: "GroupChatId",
                principalTable: "GroupChat",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
