using DAL.Specification.Interface;
using System;
using System.Collections.Generic;
using System.Text;

namespace DAL.Repository.Interface
{
    public interface IGenaricRePo<T>where T : class


    {

        Task<IReadOnlyList<T>> GetAllAsync();

        Task<T?> GetByIdAsync(object id);

        Task AddAsync(T entity);

        void Update(T entity);

        void Delete(T entity);
        Task<bool> savechange();
        Task<IReadOnlyList<T>> GetWithSpecAsync(ISpecification<T> spec);
    }
}
