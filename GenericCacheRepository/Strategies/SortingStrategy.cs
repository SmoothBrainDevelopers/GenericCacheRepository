using GenericCacheRepository.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;

namespace GenericCacheRepository.Strategies
{
    public class PropertySortingStrategy<T> : ISortingStrategy<T>
    {
        private readonly string _propertyName;

        public PropertySortingStrategy(string propertyName)
        {
            _propertyName = propertyName;
        }

        public IQueryable<T> ApplySorting(IQueryable<T> query, bool ascending)
        {
            var param = Expression.Parameter(typeof(T), "x");
            var property = Expression.Property(param, _propertyName);
            var sortExpression = Expression.Lambda(property, param);

            string methodName = ascending ? "OrderBy" : "OrderByDescending";
            return (IQueryable<T>)typeof(Queryable)
                .GetMethods()
                .First(m => m.Name == methodName && m.GetParameters().Length == 2)
                .MakeGenericMethod(typeof(T), property.Type)
                .Invoke(null, new object[] { query, sortExpression });
        }
    }

}
