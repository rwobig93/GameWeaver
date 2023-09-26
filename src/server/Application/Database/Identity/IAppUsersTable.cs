namespace Application.Database.Identity;

public interface IAppUsersTable
{
    static abstract SqlTable Table { get; }
    static abstract SqlStoredProcedure Delete { get; }
    static abstract SqlStoredProcedure GetAll { get; }
    static abstract SqlStoredProcedure GetAllServiceAccountsForPermissions { get; }
    static abstract SqlStoredProcedure GetAllPaginated { get; }
    static abstract SqlStoredProcedure GetAllServiceAccountsPaginated { get; }
    static abstract SqlStoredProcedure GetAllDisabledPaginated { get; }
    static abstract SqlStoredProcedure GetAllLockedOutPaginated  { get; }
    static abstract SqlStoredProcedure GetAllDeleted { get; }
    static abstract SqlStoredProcedure GetAllLockedOut { get; }
    static abstract SqlStoredProcedure GetByEmail { get; }
    static abstract SqlStoredProcedure GetByEmailFull { get; }
    static abstract SqlStoredProcedure GetById { get; }
    static abstract SqlStoredProcedure GetByIdSecurity { get; }
    static abstract SqlStoredProcedure GetByIdFull { get; }
    static abstract SqlStoredProcedure GetByUsername { get; }
    static abstract SqlStoredProcedure GetByUsernameFull { get; }
    static abstract SqlStoredProcedure GetByUsernameSecurity { get; }
    static abstract SqlStoredProcedure Insert { get; }
    static abstract SqlStoredProcedure Search { get; }
    static abstract SqlStoredProcedure SearchPaginated { get; }
    static abstract SqlStoredProcedure Update { get; }
    static abstract SqlStoredProcedure SetUserId { get; }
    static abstract SqlStoredProcedure SetCreatedById { get; }
}