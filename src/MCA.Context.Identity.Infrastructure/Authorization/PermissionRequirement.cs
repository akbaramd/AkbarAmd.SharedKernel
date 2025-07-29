using Microsoft.AspNetCore.Authorization;

namespace CleanArchitecture.Infrastructure.Authorization;

public sealed record PermissionRequirement(string Value) : IAuthorizationRequirement;