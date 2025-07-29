using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Localization;

namespace CleanArchitecture.Infrastructure.Authorization;

public static class LocalizationExtensions
{
    public static IServiceCollection AddPermissionLocalization(this IServiceCollection services)
    {
        services.AddLocalization();                     // uses Resources/*.resx
        return services;
    }

    public static IReadOnlyList<PermissionNode> GetLocalizedPermissionTree(this IServiceProvider sp)
    {
        var loc = sp.GetRequiredService<IStringLocalizer<PermissionResources>>();
        return PermissionTree.Build(loc);
    }
}
public sealed class PermissionResources { }