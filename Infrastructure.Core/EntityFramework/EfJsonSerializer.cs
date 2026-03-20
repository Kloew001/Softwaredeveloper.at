using System.Text.Json;
using System.Text.Json.Nodes;
using System.Text.Json.Serialization.Metadata;

using Microsoft.EntityFrameworkCore;

using Microsoft.EntityFrameworkCore.Metadata;

namespace SoftwaredeveloperDotAt.Infrastructure.Core.EntityFramework;

public static class EfJsonSerializer
{
    public static async Task<string> Serializer(
        DbContext context, 
        Entity entity, 
        int maxDepth = 10)
    {
        var jsonNode = await BuildTreeAsync(context, entity, maxDepth);

        var options = new JsonSerializerOptions
        {
            WriteIndented = true,
            TypeInfoResolver = new DefaultJsonTypeInfoResolver()
        };

        var json = jsonNode?.ToJsonString(options);

        return json;
    }

    private static async Task<JsonObject> BuildTreeAsync(
        DbContext db,
        object rootEntity,
        int maxDepth,
        CancellationToken cancellationToken = default)
    {
        if (rootEntity == null) return null;

        var visited = new HashSet<object>(ReferenceEqualityComparer.Instance);

        return await BuildNodeAsync(
            db,
            rootEntity,
            cameFromInverse: null,
            visited,
            depth: 0,
            maxDepth,
            cancellationToken);
    }

    private static async Task<JsonObject?> BuildNodeAsync(
        DbContext db,
        object entity,
        INavigationBase? cameFromInverse,
        HashSet<object> visited,
        int depth,
        int maxDepth,
        CancellationToken cancellationToken)
    {
        if (entity == null || depth > maxDepth)
            return null;

        // verhindert Endlosschleifen bei Zyklen
        if (!visited.Add(entity))
            return null;

        var entry = db.Entry(entity);
        var entityType = entry.Metadata;
        var json = new JsonObject();

        // Scalar Properties
        foreach (var prop in entityType.GetProperties())
        {
            var value = entry.Property(prop.Name).CurrentValue;
            json[prop.Name] = value == null ? null : JsonValue.Create(value);
        }

        // Referenz- und Collection-Navigationen
        var navigations = entityType.GetNavigations().Cast<INavigationBase>()
            .Concat(entityType.GetSkipNavigations());

        foreach (var nav in navigations)
        {
            // direkte Rückkante zum Parent auslassen
            if (cameFromInverse != null && nav == cameFromInverse)
                continue;

            if (nav.IsCollection)
            {
                var collectionEntry = entry.Collection(nav.Name);
                if (!collectionEntry.IsLoaded)
                    await collectionEntry.LoadAsync(cancellationToken);

                var array = new JsonArray();

                if (collectionEntry.CurrentValue is System.Collections.IEnumerable children)
                {
                    foreach (var child in children)
                    {
                        if (child == null) continue;

                        INavigationBase? inverse = nav switch
                        {
                            INavigation n => n.Inverse,
                            ISkipNavigation s => s.Inverse,
                            _ => null
                        };

                        var childNode = await BuildNodeAsync(
                            db,
                            child,
                            inverse,
                            visited,
                            depth + 1,
                            maxDepth,
                            cancellationToken);

                        if (childNode != null)
                            array.Add(childNode);
                    }
                }

                json[nav.Name] = array;
            }
            else
            {
                var referenceEntry = entry.Reference(nav.Name);
                if (!referenceEntry.IsLoaded)
                    await referenceEntry.LoadAsync(cancellationToken);

                var child = referenceEntry.CurrentValue;
                if (child == null)
                {
                    json[nav.Name] = null;
                    continue;
                }

                INavigationBase? inverse = nav switch
                {
                    INavigation n => n.Inverse,
                    ISkipNavigation s => s.Inverse,
                    _ => null
                };

                var childNode = await BuildNodeAsync(
                    db,
                    child,
                    inverse,
                    visited,
                    depth + 1,
                    maxDepth,
                    cancellationToken);

                json[nav.Name] = childNode;
            }
        }

        return json;
    }

    private sealed class ReferenceEqualityComparer : IEqualityComparer<object>
    {
        public static readonly ReferenceEqualityComparer Instance = new();

        public new bool Equals(object? x, object? y) => ReferenceEquals(x, y);

        public int GetHashCode(object obj) =>
            System.Runtime.CompilerServices.RuntimeHelpers.GetHashCode(obj);
    }
}