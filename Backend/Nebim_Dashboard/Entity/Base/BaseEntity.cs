namespace Entity.Base;

/// <summary>
/// Tüm App entity'lerinin miras alacağı temel sınıf
/// </summary>
public abstract class BaseEntity : IEntity<int>
{
    public int Id { get; set; }
    
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    
    public DateTime? UpdatedAt { get; set; }
}

/// <summary>
/// GUID primary key kullanan entity'ler için base sınıf
/// </summary>
public abstract class BaseEntityGuid : IEntity<Guid>
{
    public Guid Id { get; set; } = Guid.NewGuid();
    
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    
    public DateTime? UpdatedAt { get; set; }
}
