namespace Application.Database.Identity;

public interface IAppUserExtendedAttributesTable
{
    static abstract SqlTable Table { get; }
    static abstract SqlStoredProcedure Delete { get; }
    static abstract SqlStoredProcedure DeleteAllForOwner { get; }
    static abstract SqlStoredProcedure GetById { get; }
    static abstract SqlStoredProcedure GetByOwnerId { get; }
    static abstract SqlStoredProcedure GetByName { get; }
    static abstract SqlStoredProcedure GetByTypeAndValue { get; }
    static abstract SqlStoredProcedure GetByTypeAndValueForOwner { get; }
    static abstract SqlStoredProcedure GetAll { get; }
    static abstract SqlStoredProcedure GetAllOfType { get; }
    static abstract SqlStoredProcedure GetAllOfTypeForOwner { get; }
    static abstract SqlStoredProcedure GetAllOfNameForOwner { get; }
    static abstract SqlStoredProcedure Insert { get; }
    static abstract SqlStoredProcedure Update { get; }
}