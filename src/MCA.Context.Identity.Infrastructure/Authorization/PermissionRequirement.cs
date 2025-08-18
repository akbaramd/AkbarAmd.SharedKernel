using Microsoft.AspNetCore.Authorization;

namespace MCA.Context.Identity.Infrastructure.Authorization;

public sealed record PermissionRequirement(string Value) : IAuthorizationRequirement;