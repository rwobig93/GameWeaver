namespace Application.Database.Lifecycle;

public interface IAuditTrailsTable
{
    static abstract SqlTable Table { get; }
    static abstract SqlStoredProcedure GetAll { get; }
    static abstract SqlStoredProcedure GetAllWithUsers { get; }
    static abstract SqlStoredProcedure GetAllPaginated { get; }
    static abstract SqlStoredProcedure GetAllPaginatedWithUsers { get; }
    static abstract SqlStoredProcedure GetById { get; }
    static abstract SqlStoredProcedure GetByIdWithUser { get; }
    static abstract SqlStoredProcedure GetByChangedBy { get; }
    static abstract SqlStoredProcedure GetByRecordId { get; }
    static abstract SqlStoredProcedure Insert { get; }
    static abstract SqlStoredProcedure Search { get; }
    static abstract SqlStoredProcedure SearchPaginated { get; }
    static abstract SqlStoredProcedure SearchWithUser { get; }
    static abstract SqlStoredProcedure SearchPaginatedWithUser { get; }
    static abstract SqlStoredProcedure DeleteOlderThan { get; }
}