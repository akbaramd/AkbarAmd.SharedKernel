using System.Reflection;
using Microsoft.Extensions.Localization;

namespace CleanArchitecture.Infrastructure.Authorization;

public static class PermissionTree
{
    /// <summary>
    /// Build a culture-aware tree using the supplied localizer.
    /// </summary>
    public static IReadOnlyList<PermissionNode> Build(
        IStringLocalizer localizer,
        IEnumerable<Assembly>? assemblies = null)
    {
        assemblies ??= AppDomain.CurrentDomain.GetAssemblies();
        var nodes = new Dictionary<string, PermissionNode>(StringComparer.OrdinalIgnoreCase);

        foreach (var fieldInfo in assemblies.SelectMany(a => a.GetTypes())
                     .Where(t => t.IsClass && t.IsAbstract && t.IsSealed)
                     .SelectMany(t => t.GetFields(BindingFlags.Public | BindingFlags.Static |
                                                  BindingFlags.FlattenHierarchy)))
        {
            var meta = fieldInfo.GetCustomAttribute<PermissionMetadataAttribute>();
            if (meta is null) continue;

            var perm   = (Permission)fieldInfo.GetValue(null)!;
            var catKey = meta.CategoryPathKey.Split('/', StringSplitOptions.RemoveEmptyEntries);

            string current = string.Empty;
            PermissionNode? parent = null;

            // build/lookup category branches
            foreach (var key in catKey)
            {
                current = current.Length == 0 ? key : $"{current}/{key}";

                if (!nodes.TryGetValue(current, out var catNode))
                {
                    catNode = new PermissionNode(
                        Value: string.Empty,
                        Title: localizer[key],
                        CategoryPath: current,
                        Icon: null,
                        Children: []);

                    nodes[current] = catNode;

                    if (parent is not null) parent.Children.Add(catNode);
                }
                parent = catNode;
            }

            // leaf node (actual permission)
            var leaf = new PermissionNode(
                Value: perm.Value,
                Title: localizer[meta.TitleKey],
                CategoryPath: meta.CategoryPathKey,
                Icon: meta.Icon,
                Children: []);

            parent!.Children.Add(leaf);
        }

        // return only top-level categories
        return nodes.Values
            .Where(n => !n.CategoryPath.Contains('/'))
            .ToList();
    }
}