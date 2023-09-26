namespace Application.Database.Identity;

public interface IAppUserPreferencesTable
{
    static abstract SqlTable Table { get; }
    static abstract SqlStoredProcedure Delete { get; }
    static abstract SqlStoredProcedure DeleteForUser { get; }
    static abstract SqlStoredProcedure GetAll { get; }
    static abstract SqlStoredProcedure GetById { get; }
    static abstract SqlStoredProcedure GetByOwnerId { get; }
    static abstract SqlStoredProcedure Insert { get; }
    static abstract SqlStoredProcedure Update { get; }
}