using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;

using SoftwaredeveloperDotAt.Infrastructure.Core.Sections.ChangeTracked;
using SoftwaredeveloperDotAt.Infrastructure.Core.Sections.SupportIndex;

namespace SoftwaredeveloperDotAt.Infrastructure.Core.EntityFramework;

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

public class PageResult<TItem>
{
    public int TotalCount { get; set; } = 0;
    public int PageCount => 
        PageSize == null ? 1 : 
        PageSize <= 0 ? 0 : 
        (int)Math.Ceiling((double)TotalCount / PageSize.Value);

    public int? Page { get; set; } = null;
    public int? PageSize { get; set; } = null;

    public IEnumerable<TItem> PageItems { get; set; }

    public PageResult()
    {
    }
}

public class PageFilter
{
    public int? Page { get; set; } = 0;
    public int? PageSize { get; set; } = 0;

    public bool IsPagingActive => 
        Page != null && 
        PageSize != null &&
        Page > 0 && 
        PageSize > 0;
}

[SingletonDependency]
public class EntityQueryService<TEntity>
    where TEntity : Entity
{
    private IMemoryCache _memoryCache;

    public EntityQueryService(IMemoryCache memoryCache)
    {
        _memoryCache = memoryCache;
    }

    public async Task<PageResult<TEntity>> GetPageResultAsync(IQueryable<TEntity> query, PageFilter pageFilter)
    {
        var result = new PageResult<TEntity>();
        result.Page = pageFilter.Page;
        result.PageSize = pageFilter.PageSize;

        if (pageFilter.IsPagingActive)
        {
            var totalCount = await query.CountAsync();
            result.TotalCount = totalCount;

            var pageItems = await query
                .Skip((pageFilter.Page.Value - 1) * pageFilter.PageSize.Value)
                .Take(pageFilter.PageSize.Value)
                .ToArrayAsync();

            result.PageItems = pageItems;
        }
        else
        {
            result.PageItems = await query.ToArrayAsync();
            result.TotalCount = result.PageItems.Count();
        }

        return result;
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
}
