using Microsoft.AspNetCore.Authorization;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Options;

namespace CleanArchitecture.Infrastructure.Authorization;

public sealed class PermissionPolicyProvider(
    IOptions<AuthorizationOptions> opts,
    IMemoryCache cache)
    : IAuthorizationPolicyProvider
{
    private readonly DefaultAuthorizationPolicyProvider _fallback = new(opts);

    private static AuthorizationPolicy Build(string perm) =>
        new AuthorizationPolicyBuilder()
            .AddRequirements(new PermissionRequirement(perm))
            .Build();

    public Task<AuthorizationPolicy> GetDefaultPolicyAsync()   => _fallback.GetDefaultPolicyAsync();
    public Task<AuthorizationPolicy?> GetFallbackPolicyAsync() => _fallback.GetFallbackPolicyAsync();

    public Task<AuthorizationPolicy?> GetPolicyAsync(string name) =>
        Task.FromResult(cache.GetOrCreate(name, e =>
        {
            e.SlidingExpiration = TimeSpan.FromHours(24);
            return Build(name);
        }));
}