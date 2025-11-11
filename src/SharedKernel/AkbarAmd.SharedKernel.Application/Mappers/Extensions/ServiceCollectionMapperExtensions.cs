using System.Reflection;
using AkbarAmd.SharedKernel.Application.Contracts;
using Microsoft.Extensions.DependencyInjection;

namespace AkbarAmd.SharedKernel.Application.Mappers.Extensions
{
    public static class ServiceCollectionMapperExtensions
    {
        public static IServiceCollection AddMappers(this IServiceCollection services, params Assembly[] assemblies)
        {
            return services.AddMappersFromAssemblies(assemblies);
        }

        public static IServiceCollection AddMappers(this IServiceCollection services, params Type[] markerTypes)
        {
            if (services == null) throw new ArgumentNullException(nameof(services));
            if (markerTypes == null || markerTypes.Length == 0) return services;
            var assemblies = markerTypes.Where(t => t != null).Select(t => t.Assembly).Distinct().ToArray();
            return services.AddMappersFromAssemblies(assemblies);
        }

        public static IServiceCollection AddMappers(this IServiceCollection services, Action<MapperRegistrationOptions> configure)
        {
            if (services == null) throw new ArgumentNullException(nameof(services));
            if (configure == null) throw new ArgumentNullException(nameof(configure));

            var options = new MapperRegistrationOptions();
            configure(options);
            var assemblies = options.Assemblies.Distinct().ToArray();
            return services.AddMappersFromAssemblies(assemblies);
        }

        public static IServiceCollection AddMappersFromAssemblies(this IServiceCollection services, params Assembly[] assemblies)
        {
            if (services == null) throw new ArgumentNullException(nameof(services));
            if (assemblies == null || assemblies.Length == 0) return services;

            foreach (var assembly in assemblies.Where(a => a != null))
            {
                RegisterMappersFromAssembly(services, assembly);
            }

            return services;
        }

        public static IServiceCollection AddMappersFromAssemblyOf<TMarker>(this IServiceCollection services)
        {
            if (services == null) throw new ArgumentNullException(nameof(services));
            var assembly = typeof(TMarker).Assembly;
            return services.AddMappersFromAssemblies(assembly);
        }

        public static IServiceCollection AddMappersFromCurrentDomain(this IServiceCollection services, Func<Assembly, bool>? predicate = null)
        {
            if (services == null) throw new ArgumentNullException(nameof(services));
            var assemblies = AppDomain.CurrentDomain.GetAssemblies();
            if (predicate != null)
            {
                assemblies = assemblies.Where(predicate).ToArray();
            }

            return services.AddMappersFromAssemblies(assemblies);
        }

        private static void RegisterMappersFromAssembly(IServiceCollection services, Assembly assembly)
        {
            Type mapperOpenInterface = typeof(IMapper<,>);

            Type[] types;
            try
            {
                types = assembly.GetTypes();
            }
            catch (ReflectionTypeLoadException ex)
            {
                types = ex.Types.Where(t => t != null).ToArray()!;
            }

            foreach (var implementationType in types)
            {
                if (implementationType == null) continue;
                if (!implementationType.IsClass || implementationType.IsAbstract) continue;
                if (implementationType.IsGenericTypeDefinition) continue;

                var mapperInterfaces = implementationType
                    .GetInterfaces()
                    .Where(i => i.IsGenericType && i.GetGenericTypeDefinition() == mapperOpenInterface)
                    .ToArray();

                if (mapperInterfaces.Length == 0) continue;

                foreach (var serviceInterface in mapperInterfaces)
                {
                    // Avoid duplicate registrations
                    bool alreadyRegistered = services.Any(sd => sd.ServiceType == serviceInterface && sd.ImplementationType == implementationType);
                    if (alreadyRegistered) continue;

                    services.AddTransient(serviceInterface, implementationType);
                }
            }
        }
    }
}


