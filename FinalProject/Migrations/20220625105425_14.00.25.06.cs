using Microsoft.EntityFrameworkCore.Migrations;

namespace FinalProject.Migrations
{
    public partial class _14002506 : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "GroupChatId",
                table: "Messages",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "PrivateChatId",
                table: "Messages",
                type: "int",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_Messages_GroupChatId",
                table: "Messages",
                column: "GroupChatId");

            migrationBuilder.CreateIndex(
                name: "IX_Messages_PrivateChatId",
                table: "Messages",
                column: "PrivateChatId");

            migrationBuilder.AddForeignKey(
                name: "FK_Messages_GroupChats_GroupChatId",
                table: "Messages",
                column: "GroupChatId",
                principalTable: "GroupChats",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_Messages_PrivateChats_PrivateChatId",
                table: "Messages",
                column: "PrivateChatId",
                principalTable: "PrivateChats",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Messages_GroupChats_GroupChatId",
                table: "Messages");

            migrationBuilder.DropForeignKey(
                name: "FK_Messages_PrivateChats_PrivateChatId",
                table: "Messages");

            migrationBuilder.DropIndex(
                name: "IX_Messages_GroupChatId",
                table: "Messages");

            migrationBuilder.DropIndex(
                name: "IX_Messages_PrivateChatId",
                table: "Messages");

            migrationBuilder.DropColumn(
                name: "GroupChatId",
                table: "Messages");

            migrationBuilder.DropColumn(
                name: "PrivateChatId",
                table: "Messages");
        }
    }
}
