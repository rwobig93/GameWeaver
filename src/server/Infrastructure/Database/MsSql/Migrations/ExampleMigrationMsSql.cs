using Application.Database.MsSql;
using Infrastructure.Database.MsSql.Identity;

namespace Infrastructure.Database.MsSql.Migrations;

public class ExampleMigrationMsSql : IMsSqlMigration
{
    public Version VersionTarget => new(0, 0, 0, 0);

    public string Up => $@"
            IF EXISTS (SELECT * FROM sys.objects WHERE type = 'U' AND OBJECT_ID = OBJECT_ID('[dbo].[{AppUsersTableMsSql.Table.TableName}]'))
            begin
                ALTER TABLE [dbo].[{AppUsersTableMsSql.Table.TableName}] ALTER COLUMN [Username] NVARCHAR(512);
                ALTER TABLE [dbo].[{AppUsersTableMsSql.Table.TableName}] ALTER COLUMN [ProfilePictureDataUrl] NVARCHAR(1024);
            end";

    public string Down => $@"
            IF EXISTS (SELECT * FROM sys.objects WHERE type = 'U' AND OBJECT_ID = OBJECT_ID('[dbo].[{AppUsersTableMsSql.Table.TableName}]'))
            begin
                DROP INDEX [IX_User_UserName] ON [dbo].[{AppUsersTableMsSql.Table.TableName}];
                ALTER TABLE [dbo].[{AppUsersTableMsSql.Table.TableName}] ALTER COLUMN [Username] NVARCHAR(256);
                ALTER TABLE [dbo].[{AppUsersTableMsSql.Table.TableName}] ALTER COLUMN [ProfilePictureDataUrl] NVARCHAR(400);
                CREATE INDEX [IX_User_UserName] ON [dbo].[{AppUsersTableMsSql.Table.TableName}] ([Username]);
            end";
}