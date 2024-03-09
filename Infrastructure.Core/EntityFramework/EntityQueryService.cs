using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;
using SoftwaredeveloperDotAt.Infrastructure.Core.Sections.ChangeTracked;
using SoftwaredeveloperDotAt.Infrastructure.Core.Sections.SupportIndex;
using SoftwaredeveloperDotAt.Infrastructure.Core.Utility;

namespace SoftwaredeveloperDotAt.Infrastructure.Core.EntityFramework
{
    [AttributeUsage(AttributeTargets.Property, AllowMultiple = false)]
    public class AutoQueryIncludeAttribute : Attribute
    {
    }

    [AttributeUsage(AttributeTargets.Property, AllowMultiple = false)]
    public class OrderByAttribute : Attribute
    {
        public enum SortDirection
        {
            Ascending,
            Descending
        }
        public int Order { get; set; }
        public SortDirection Direction { get; set; }

        public OrderByAttribute(int order = 1, SortDirection direction = SortDirection.Ascending)
        {
            Order = order;
            Direction = direction;
        }
    }

    public class EntityQueryService<TEntity> : ISingletonDependency
        where TEntity : Entity
    {
        private IMemoryCache _memoryCache;

        public EntityQueryService(IMemoryCache memoryCache)
        {
            _memoryCache = memoryCache;
        }

        public IQueryable<TEntity> AppendDefaultOrderBy(IQueryable<TEntity> query)
        {
            if (query is IOrderedQueryable<TEntity>)
                return query;

            var sortOrders =
                 _memoryCache.GetOrCreate(
                $"{nameof(EntityQueryService<TEntity>)}_{typeof(TEntity).Name}_AppendDefaultOrderBy_sortOrders", _ =>
                {
                    return typeof(TEntity).GetProperties()
                    .Select(_ =>
                    new
                    {
                        Property = _,
                        Attribute = _.GetCustomAttributes(typeof(OrderByAttribute), false).SingleOrDefault() as OrderByAttribute
                    })
                    .Where(_ => _.Attribute != null)
                    .ToList();
                });

            if (sortOrders.Any())
            {
                var isFirst = true;
                foreach (var sortOrder in sortOrders.OrderBy(_ => _.Attribute.Order))
                {
                    var direction = sortOrder.Attribute.Direction;
                    var order = sortOrder.Attribute.Order;
                    if (isFirst)
                    {
                        if (direction == OrderByAttribute.SortDirection.Ascending)
                            query = query.OrderByPropertyName(sortOrder.Property.Name);
                        else
                            query = query.OrderByPropertyNameDescending(sortOrder.Property.Name);
                    }
                    else
                    {
                        if (direction == OrderByAttribute.SortDirection.Ascending)
                            query = query.ThenByPropertyName(sortOrder.Property.Name);
                        else
                            query = query.ThenByPropertyNameDescending(sortOrder.Property.Name);

                    }

                    isFirst = false;
                }

            }
            else if (typeof(ISupportIndex).IsAssignableFrom(typeof(TEntity)))
            {
                query = query.OrderByPropertyName(nameof(ISupportIndex.Index));
            }
            else if (typeof(ISupportDisplayName).IsAssignableFrom(typeof(TEntity)))
            {
                query = query.OrderByPropertyName(nameof(ISupportDisplayName.DisplayName));
            }
            else if (typeof(ChangeTrackedEntity).IsAssignableFrom(typeof(TEntity)))
            {
                query = query.OrderByPropertyNameDescending(nameof(ChangeTrackedEntity.DateModified));
            }
            else
            {
                query = query.OrderBy(_ => _.Id);
            }

            return query;
        }

        public IQueryable<TEntity> IncludeAutoQueryProperties(IQueryable<TEntity> query)
        {
            var autoQueryIncludePropertyNames = _memoryCache.GetOrCreate(
                $"{nameof(EntityQueryService<TEntity>)}_{typeof(TEntity).Name}_AutoQueryIncludePropertyNames", _ =>
            {
                return typeof(TEntity)
                .GetProperties()
                .Where(_ => _.GetCustomAttributes(typeof(AutoQueryIncludeAttribute), true)?
                               .Any() == true)
                .Select(_ => _.Name)
                .ToList();
            });

            autoQueryIncludePropertyNames
                .ForEach(_ => query = query.Include(_));

            return query;
        }

    }
}
