namespace Domain.Contracts;

public interface IAuditableEntity<TId> : IAuditableEntity, IEntity<TId>
{

}

public interface IAuditableEntity : IEntity
{
    Guid CreatedBy { get; set; }

    DateTime CreatedOn { get; set; }

    Guid? LastModifiedBy { get; set; }

    DateTime? LastModifiedOn { get; set; }
}
