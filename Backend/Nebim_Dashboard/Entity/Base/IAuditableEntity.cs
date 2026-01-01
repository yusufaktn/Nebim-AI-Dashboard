namespace Entity.Base;

/// <summary>
/// Audit bilgisi tutulması gereken entity'ler için interface
/// Kim oluşturdu, kim güncelledi bilgisini içerir
/// </summary>
public interface IAuditableEntity
{
    string? CreatedBy { get; set; }
    string? UpdatedBy { get; set; }
}

/// <summary>
/// Soft delete destekleyen entity'ler için interface
/// </summary>
public interface ISoftDeletable
{
    bool IsDeleted { get; set; }
    DateTime? DeletedAt { get; set; }
    string? DeletedBy { get; set; }
}

/// <summary>
/// Audit + Soft Delete özellikli base entity
/// </summary>
public abstract class AuditableEntity : BaseEntity, IAuditableEntity, ISoftDeletable
{
    public string? CreatedBy { get; set; }
    public string? UpdatedBy { get; set; }
    
    public bool IsDeleted { get; set; } = false;
    public DateTime? DeletedAt { get; set; }
    public string? DeletedBy { get; set; }
}
