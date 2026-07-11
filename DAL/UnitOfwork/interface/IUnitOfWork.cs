using DAL.Repository.Interface;
using System;
using System.Threading.Tasks;

namespace DAL.UnitOfWork
{
    public interface IUnitOfWork : IDisposable
    {
        IGenaricRebo<TEntity> Repository<TEntity>() where TEntity : class;

        Task<int> CompleteAsync();
    }
}