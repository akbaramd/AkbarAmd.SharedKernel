using System.Reflection;

namespace AkbarAmd.SharedKernel.Application.Mappers.Extensions
{
    public sealed class MapperRegistrationOptions
    {
        private readonly HashSet<Assembly> _assemblies = new();

        public IReadOnlyCollection<Assembly> Assemblies => _assemblies;

        public MapperRegistrationOptions AddAssembly(Assembly assembly)
        {
            if (assembly != null)
            {
                _assemblies.Add(assembly);
            }
            return this;
        }

        public MapperRegistrationOptions AddAssemblies(params Assembly[] assemblies)
        {
            if (assemblies == null) return this;
            foreach (var asm in assemblies)
            {
                AddAssembly(asm);
            }
            return this;
        }

        public MapperRegistrationOptions AddAssemblyOf<T>()
        {
            return AddAssembly(typeof(T).Assembly);
        }

        public MapperRegistrationOptions AddAssembliesOf(params Type[] markerTypes)
        {
            if (markerTypes == null) return this;
            foreach (var t in markerTypes)
            {
                if (t != null) AddAssembly(t.Assembly);
            }
            return this;
        }

        public MapperRegistrationOptions AddCurrentDomain(Func<Assembly, bool>? predicate = null)
        {
            var assemblies = AppDomain.CurrentDomain.GetAssemblies();
            if (predicate != null)
            {
                assemblies = assemblies.Where(predicate).ToArray();
            }
            AddAssemblies(assemblies);
            return this;
        }
    }
}


