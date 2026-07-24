using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Text;

namespace DAL.Specification.Interface
{
    public interface ISpecification<T> 
    {
        Expression<Func<T, bool>>? Criteria { get; }

        List<Expression<Func<T, object>>> Includes { get; }
        Expression<Func<T, object>>? OrderBy { get; }
        Expression<Func<T, object>>? OrderByDesc { get; }
        public int? Take { get; set; }
        public int? Skip { get; set; }
        public bool IsPaginationEnabled { get; set; }

    }
}
