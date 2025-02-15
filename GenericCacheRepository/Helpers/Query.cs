using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;

namespace GenericCacheRepository.Helpers
{
    public class Query<T> where T : class
    {
        private Expression<Func<T, bool>> _criteria;

        public Query(Expression<Func<T, bool>> criteria)
        {
            _criteria = criteria;
        }

        public IQueryable<T> Apply(IQueryable<T> query)
        {
            return query.Where(_criteria);
        }
    }

}
