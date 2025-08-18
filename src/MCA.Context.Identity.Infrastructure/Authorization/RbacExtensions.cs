using Microsoft.AspNetCore.Authorization;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace MCA.Context.Identity.Infrastructure.Authorization;

public static class RbacExtensions
{
    /// <summary>
    /// Registers Caching, Dynamic Policy-Provider, and the Permission Handler.
    /// Idempotent: می‌توانید هر تعداد بار آن را صدا بزنید بدون عوارض جانبی.
    /// </summary>
    public static IServiceCollection AddRbac(this IServiceCollection services)
    {
        // 1️⃣ Cache (singleton) – فقط اگر قبلاً وجود نداشته باشد.
        services.TryAddSingleton<IMemoryCache, MemoryCache>();

        // 2️⃣ Dynamic Policy-Provider (singleton)
        services.TryAddSingleton<IAuthorizationPolicyProvider, PermissionPolicyProvider>();

        // 3️⃣ Permission Handler (scoped) – اضافه به مجموعهٔ هندلرها
        services.TryAddEnumerable(
            ServiceDescriptor.Scoped<IAuthorizationHandler, PermissionHandler>());

        return services;
    }
}