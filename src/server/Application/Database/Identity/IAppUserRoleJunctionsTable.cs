namespace Application.Database.Identity;

public interface IAppUserRoleJunctionsTable
{
    static abstract SqlTable Table { get; }
    static abstract SqlStoredProcedure Delete { get; }
    static abstract SqlStoredProcedure DeleteForUser { get; }
    static abstract SqlStoredProcedure DeleteForRole { get; }
    static abstract SqlStoredProcedure GetAll { get; }
    static abstract SqlStoredProcedure GetByUserRoleId { get; }
    static abstract SqlStoredProcedure GetRolesOfUser { get; }
    static abstract SqlStoredProcedure GetUsersOfRole { get; }
    static abstract SqlStoredProcedure Insert { get; }
    static abstract SqlStoredProcedure Search { get; }
}