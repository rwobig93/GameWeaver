using Application.Database;
using Application.Database.MsSql;
using Application.Helpers.Runtime;

namespace Infrastructure.Database.MsSql.GameServer;

public class HostCheckInTableMsSql : IMsSqlEnforcedEntity
{
    private const string TableName = "HostCheckIns";

    public IEnumerable<ISqlDatabaseScript> GetDbScripts() => typeof(HostCheckInTableMsSql).GetDbScriptsFromClass();
    
    public static readonly SqlTable Table = new()
    {
        EnforcementOrder = 10,
        TableName = TableName,
        SqlStatement = $@"
            IF NOT EXISTS (SELECT * FROM sys.objects WHERE type = 'U' AND OBJECT_ID = OBJECT_ID('[dbo].[{TableName}]'))
            begin
                CREATE TABLE [dbo].[{TableName}](
                    [Id] INT IDENTITY(1,1) PRIMARY KEY,
                    [HostId] UNIQUEIDENTIFIER NOT NULL,
                    [SendTimestamp] DATETIME2 NOT NULL,
                    [ReceiveTimestamp] DATETIME2 NOT NULL,
                    [CpuUsage] FLOAT(24) NOT NULL,
                    [RamUsage] FLOAT(24) NOT NULL,
                    [Uptime] FLOAT(24) NOT NULL,
                    [NetworkOutBytes] INT NOT NULL,
                    [NetworkInBytes] INT NOT NULL
                )
            end"
    };
    
    public static readonly SqlStoredProcedure GetAll = new()
    {
        Table = Table,
        Action = "GetAll",
        SqlStatement = @$"
            CREATE OR ALTER PROCEDURE [dbo].[sp{Table.TableName}_GetAll]
            AS
            begin
                SELECT h.*
                FROM dbo.[{Table.TableName}] h;
            end"
    };

    public static readonly SqlStoredProcedure GetAllPaginated = new()
    {
        Table = Table,
        Action = "GetAllPaginated",
        SqlStatement = @$"
            CREATE OR ALTER PROCEDURE [dbo].[sp{Table.TableName}_GetAllPaginated]
                @Offset INT,
                @PageSize INT
            AS
            begin
                SELECT COUNT(*) OVER() AS TotalCount, h.*
                FROM dbo.[{Table.TableName}] h
                ORDER BY h.Id DESC OFFSET @Offset ROWS FETCH NEXT @PageSize ROWS ONLY;
            end"
    };
    
    public static readonly SqlStoredProcedure GetAllAfter = new()
    {
        Table = Table,
        Action = "GetAllAfter",
        SqlStatement = @$"
            CREATE OR ALTER PROCEDURE [dbo].[sp{Table.TableName}_GetAllAfter]
                @AfterDate DATETIME2
            AS
            begin
                SELECT h.*
                FROM dbo.[{Table.TableName}] h
                WHERE h.ReceiveTimestamp > @AfterDate
                ORDER BY h.ReceiveTimestamp DESC;
            end"
    };
    
    public static readonly SqlStoredProcedure GetAfterByHostId = new()
    {
        Table = Table,
        Action = "GetAfterByHostId",
        SqlStatement = @$"
            CREATE OR ALTER PROCEDURE [dbo].[sp{Table.TableName}_GetAfterByHostId]
                @Id UNIQUEIDENTIFIER,
                @AfterDate DATETIME2
            AS
            begin
                SELECT h.*
                FROM dbo.[{Table.TableName}] h
                WHERE h.HostId = @Id AND h.ReceiveTimestamp > @AfterDate
                ORDER BY h.ReceiveTimestamp DESC;
            end"
    };
    
    public static readonly SqlStoredProcedure GetById = new()
    {
        Table = Table,
        Action = "GetById",
        SqlStatement = @$"
            CREATE OR ALTER PROCEDURE [dbo].[sp{Table.TableName}_GetById]
                @Id int
            AS
            begin
                SELECT TOP 1 h.*
                FROM dbo.[{Table.TableName}] h
                WHERE h.Id = @Id
                ORDER BY h.Id;
            end"
    };
    
    public static readonly SqlStoredProcedure GetByHostId = new()
    {
        Table = Table,
        Action = "GetByHostId",
        SqlStatement = @$"
            CREATE OR ALTER PROCEDURE [dbo].[sp{Table.TableName}_GetByHostId]
                @HostId NVARCHAR(256)
            AS
            begin
                SELECT h.*
                FROM dbo.[{Table.TableName}] h
                WHERE h.HostId = @HostId
                ORDER BY h.Id;
            end"
    };
    
    public static readonly SqlStoredProcedure GetByHostIdLatest = new()
    {
        Table = Table,
        Action = "GetByHostIdLatest",
        SqlStatement = @$"
            CREATE OR ALTER PROCEDURE [dbo].[sp{Table.TableName}_GetByHostIdLatest]
                @HostId NVARCHAR(256),
                @Count INT
            AS
            begin
                SELECT TOP (@Count) *
                FROM dbo.[{Table.TableName}] h
                WHERE h.HostId = @HostId
                ORDER BY h.SendTimestamp DESC;
            end"
    };
    
    public static readonly SqlStoredProcedure Insert = new()
    {
        Table = Table,
        Action = "Insert",
        SqlStatement = @$"
            CREATE OR ALTER PROCEDURE [dbo].[sp{Table.TableName}_Insert]
                @HostId UNIQUEIDENTIFIER,
                @SendTimestamp datetime2,
                @ReceiveTimestamp datetime2,
                @CpuUsage float(24),
                @RamUsage float(24),
                @Uptime float(24),
                @NetworkOutBytes int,
                @NetworkInBytes int
            AS
            begin
                INSERT into dbo.[{Table.TableName}] (HostId, SendTimestamp, ReceiveTimestamp, CpuUsage, RamUsage, Uptime, NetworkOutBytes, NetworkInBytes)
                OUTPUT INSERTED.Id
                VALUES (@HostId, @SendTimestamp, @ReceiveTimestamp, @CpuUsage, @RamUsage, @Uptime, @NetworkOutBytes, @NetworkInBytes);
            end"
    };
    
    public static readonly SqlStoredProcedure Search = new()
    {
        Table = Table,
        Action = "Search",
        SqlStatement = @$"
            CREATE OR ALTER PROCEDURE [dbo].[sp{Table.TableName}_Search]
                @SearchTerm NVARCHAR(256)
            AS
            begin
                SET nocount on;
                
                SELECT h.*
                FROM dbo.[{Table.TableName}] h
                WHERE h.Id LIKE '%' + @SearchTerm + '%'
                    OR h.HostId LIKE '%' + @SearchTerm + '%';
            end"
    };
    
    public static readonly SqlStoredProcedure SearchPaginated = new()
    {
        Table = Table,
        Action = "SearchPaginated",
        SqlStatement = @$"
            CREATE OR ALTER PROCEDURE [dbo].[sp{Table.TableName}_SearchPaginated]
                @SearchTerm NVARCHAR(256),
                @Offset INT,
                @PageSize INT
            AS
            begin
                SELECT COUNT(*) OVER() AS TotalCount, h.*
                FROM dbo.[{Table.TableName}] h
                WHERE h.Id LIKE '%' + @SearchTerm + '%'
                    OR h.HostId LIKE '%' + @SearchTerm + '%'
                ORDER BY h.Id DESC OFFSET @Offset ROWS FETCH NEXT @PageSize ROWS ONLY;
            end"
    };
    
    public static readonly SqlStoredProcedure DeleteAllForHostId = new()
    {
        Table = Table,
        Action = "DeleteAllForHostId",
        SqlStatement = @$"
            CREATE OR ALTER PROCEDURE [dbo].[sp{Table.TableName}_DeleteAllForHostId]
                @HostId UNIQUEIDENTIFIER
            AS
            begin
                DELETE
                FROM dbo.[{Table.TableName}]
                WHERE HostId = @HostId;
            end"
    };
    
    public static readonly SqlStoredProcedure DeleteOlderThan = new()
    {
        Table = Table,
        Action = "DeleteOlderThan",
        SqlStatement = @$"
            CREATE OR ALTER PROCEDURE [dbo].[sp{Table.TableName}_DeleteOlderThan]
                @OlderThan DATETIME2
            AS
            begin
                DELETE
                FROM dbo.[{Table.TableName}]
                WHERE ReceiveTimestamp < @OlderThan;
            end"
    };
}