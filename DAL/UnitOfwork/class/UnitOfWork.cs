using DAL.Repository.Class;
using DAL.Repository.Interface;
using System;
using System.Collections;
using System.Threading.Tasks;

namespace DAL.UnitOfWork
{
    public class UnitOfWork : IUnitOfWork
    {
        private readonly AppDbContext _context;
        private Hashtable _repositories; 

        public UnitOfWork(AppDbContext context)
        {
            _context = context;
        }

        public IGenaricRebo<TEntity> Repository<TEntity>() where TEntity : class
        {
            if (_repositories == null) _repositories = new Hashtable();

            var type = typeof(TEntity).Name; // بياخد اسم الـ Entity (مثلا Order)

            if (!_repositories.ContainsKey(type))
            {
                var repositoryType = typeof(GenaricRebo<>);
                var repositoryInstance = Activator.CreateInstance(repositoryType.MakeGenericType(typeof(TEntity)), _context);

                _repositories.Add(type, repositoryInstance);
            }

            return (IGenaricRebo<TEntity>)_repositories[type]!;
        }

        public async Task<int> CompleteAsync()
        {
            return await _context.SaveChangesAsync();
        }

        public void Dispose()
        {
            _context.Dispose();
        }
    }
}