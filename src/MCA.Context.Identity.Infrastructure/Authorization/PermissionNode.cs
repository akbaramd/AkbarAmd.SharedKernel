using System.Text.Json.Serialization;

namespace CleanArchitecture.Infrastructure.Authorization;

public sealed record PermissionNode(
    string Value,
    string Title,
    string CategoryPath,
    string? Icon,
    List<PermissionNode> Children)
{
    [JsonIgnore] public IReadOnlyList<PermissionNode> ReadOnlyChildren => Children;
}