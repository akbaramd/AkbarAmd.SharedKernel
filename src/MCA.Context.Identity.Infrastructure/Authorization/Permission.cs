using System.Collections.Concurrent;
using MCA.SharedKernel.Domain;

namespace MCA.Context.Identity.Infrastructure.Authorization;

/// <summary>
///  Value-Object؛ تنها راه ساخت نمونه از طریق <see cref="Of"/> است.
/// </summary>
public sealed class Permission : ValueObject
{
    private static readonly ConcurrentDictionary<string, Permission> _registry =
        new(StringComparer.OrdinalIgnoreCase);

    private Permission(string value) => Value = value;

    public string Value { get; }

    public static Permission Of(string value) =>
        _registry.GetOrAdd(value, v => new Permission(v));

    public static IEnumerable<Permission> All => _registry.Values;

    public static implicit operator string(Permission p) => p.Value;

    protected override IEnumerable<object> GetEqualityComponents()
    {
        yield return Value;
    }

    public override string ToString() => Value;
    
}


