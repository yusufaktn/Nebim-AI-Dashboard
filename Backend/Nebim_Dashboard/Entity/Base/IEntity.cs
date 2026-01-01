namespace Entity.Base;

/// <summary>
/// Marker interface - tüm entity'lerin implement etmesi gereken temel interface
/// </summary>
public interface IEntity
{
}

/// <summary>
/// ID'ye sahip entity'ler için generic interface
/// </summary>
/// <typeparam name="TKey">Primary key tipi</typeparam>
public interface IEntity<TKey> : IEntity where TKey : struct
{
    TKey Id { get; set; }
}
