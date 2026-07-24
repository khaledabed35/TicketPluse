using DAL.Repository.Interface;
using DAL.Specification.Class;
using DAL.Specification.Interface;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Text;

namespace DAL.Repository.Class
{
    public class GenaricRebo<T> : IGenaricRePo<T> where T : class
    {
        protected readonly AppDbContext _context;

        public GenaricRebo(AppDbContext  context)
        {
            _context = context;
        }
        public async Task AddAsync(T entity)
        {
            await _context.Set<T>().AddAsync(entity);
        }

        public void Delete(T entity)
        {
            _context.Set<T>().Remove(entity);
        }
        public async Task<IReadOnlyList<T>> GetWithSpecAsync(ISpecification<T> spec)
        {
            return await ApplySpecification(spec).ToListAsync();
        }

        public async Task<IReadOnlyList<T>> GetAllAsync()
        {
            return await _context.Set<T>().ToListAsync<T>();
        }

        public async Task<T?> GetByIdAsync(object id)
        {
            return await _context.Set<T>().FindAsync(id);
        }

        public async Task<bool> savechange()
        {
            return await _context.SaveChangesAsync() > 0;
        }

        public void Update(T entity)
        {
            _context.Set<T>().Update(entity);

        }
        private IQueryable<T> ApplySpecification(ISpecification<T> spec)
        {
            return Evaluator<T>.GetQuery(_context.Set<T>(), spec);
        }
    }
}
