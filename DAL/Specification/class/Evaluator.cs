using DAL.Specification.Interface;
using Microsoft.EntityFrameworkCore;

namespace DAL.Specification.Class
{
    public class Evaluator<TEntity> where TEntity : class
    {
        public static IQueryable<TEntity> GetQuery(
            IQueryable<TEntity> inputQuery,
            ISpecification<TEntity> spec)
        {
            var query = inputQuery;

            if (spec.Criteria != null)
                query = query.Where(spec.Criteria);

            query = spec.Includes.Aggregate(query,
                (current, include) => current.Include(include));

            if (spec.OrderBy != null)
                query = query.OrderBy(spec.OrderBy);

            if (spec.OrderByDesc != null)
                query = query.OrderByDescending(spec.OrderByDesc);

            if (spec.IsPaginationEnabled)
            {
                if (spec.Skip.HasValue)
                    query = query.Skip(spec.Skip.Value);

                if (spec.Take.HasValue)
                    query = query.Take(spec.Take.Value);
            }

            return query;
        }
    }
}