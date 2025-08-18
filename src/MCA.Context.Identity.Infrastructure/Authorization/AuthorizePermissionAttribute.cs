using Microsoft.AspNetCore.Authorization;

namespace MCA.Context.Identity.Infrastructure.Authorization;

[AttributeUsage(AttributeTargets.Class | AttributeTargets.Method, AllowMultiple = true)]
public sealed class AuthorizePermissionAttribute : Attribute, IAuthorizeData
{
    public AuthorizePermissionAttribute(Permission p) => Policy = p.Value;

    public string? Policy { get; set; }
    public string? Roles  { get; set; }
    public string? AuthenticationSchemes { get; set; }
}