using Microsoft.EntityFrameworkCore.Migrations;

namespace SocialNetworkApi.Migrations
{
    public partial class SetNullTriggerForUsersAndFriends : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql
                (@$"
                    CREATE TRIGGER set_null_in_UserToFriends_after_deleting_from_AspNetUsers
                    ON [dbo].[AspNetUsers]
                    AFTER DELETE
                    AS
                    BEGIN
                    UPDATE [dbo].[{nameof(Models.ApplicationContext.UserToFriends)}] SET {nameof(Models.NToNs.UserToFriend.FriendId)} = NULL WHERE {nameof(Models.NToNs.UserToFriend.FriendId)} = (SELECT {nameof(Models.User.Id)} FROM deleted);
                    END"
                );
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql("DROP TRIGGER set_null_in_UserToFriends_after_deleting_from_AspNetUsers");
        }
    }
}
