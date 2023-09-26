namespace Application.Database.Identity;

public interface IAppRolesTable
{
    static abstract SqlTable Table { get; }
    static abstract SqlStoredProcedure Delete { get; }
    static abstract SqlStoredProcedure GetAll { get; }
    static abstract SqlStoredProcedure GetAllPaginated { get; }
    static abstract SqlStoredProcedure GetById { get; }
    static abstract SqlStoredProcedure GetByName { get; }
    static abstract SqlStoredProcedure Insert { get; }
    static abstract SqlStoredProcedure Search { get; }
    static abstract SqlStoredProcedure SearchPaginated { get; }
    static abstract SqlStoredProcedure Update { get; }
    static abstract SqlStoredProcedure SetCreatedById { get; }
}