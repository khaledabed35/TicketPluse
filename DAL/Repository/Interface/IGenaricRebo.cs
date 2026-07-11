using System;
using System.Collections.Generic;
using System.Text;

namespace DAL.Repository.Interface
{
    public interface IGenaricRebo<T>where T : class


    {

        Task<IReadOnlyList<T>> GetAllAsync();

        Task<T?> GetByIdAsync(object id);

        Task AddAsync(T entity);

        void Update(T entity);

        void Delete(T entity);
        Task<bool> savechange();
    }
}
