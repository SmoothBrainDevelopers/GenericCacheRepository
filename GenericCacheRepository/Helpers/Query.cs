using System.Linq.Expressions;

namespace GenericCacheRepository.Helpers
{
    public class Query<T> where T : class
    {
        private Expression<Func<T, bool>> _criteria;
        private string? _sortBy;
        private int? _sortByIndex;
        private bool _ascending;

        public Query(Expression<Func<T, bool>> criteria, string? sortBy = null, int? sortByIndex = null, bool ascending = true)
        {
            _criteria = criteria;
            _sortBy = sortBy;
            _sortByIndex = sortByIndex;
            _ascending = ascending;
        }

        public IQueryable<T> Apply(IQueryable<T> query)
        {
            query = query.Where(_criteria);
            return ApplySorting(query);
        }

        private IQueryable<T> ApplySorting(IQueryable<T> query)
        {
            var properties = typeof(T).GetProperties().OrderBy(p => p.MetadataToken).ToArray();

            // Determine sorting property
            string propertyToSortBy = _sortBy ?? (properties.ElementAtOrDefault(_sortByIndex ?? -1)?.Name ?? "Id");

            var param = Expression.Parameter(typeof(T), "x");
            var property = Expression.Property(param, propertyToSortBy);
            var sortExpression = Expression.Lambda(property, param);

            string methodName = _ascending ? "OrderBy" : "OrderByDescending";
            var sortedQuery = typeof(Queryable)
                .GetMethods()
                .First(m => m.Name == methodName && m.GetParameters().Length == 2)
                .MakeGenericMethod(typeof(T), property.Type)
                .Invoke(null, new object[] { query, sortExpression });

            return (IQueryable<T>)sortedQuery;
        }

        public string GetCacheKey()
        {
            return $"{typeof(T).Name}:{_criteria.Body}:{_sortBy ?? _sortByIndex?.ToString() ?? "default"}:{_ascending}";
        }
    }


}
