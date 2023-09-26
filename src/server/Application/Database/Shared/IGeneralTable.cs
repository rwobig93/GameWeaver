namespace Application.Database.Shared;

public interface IGeneralTable
{
    static abstract SqlStoredProcedure GetRowCount { get; }
    static abstract SqlStoredProcedure VerifyConnectivity { get; }
}