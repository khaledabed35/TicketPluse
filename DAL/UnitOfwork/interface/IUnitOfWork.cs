using DAL.Repository.Interface;
using System;
using System.Threading.Tasks;

namespace DAL.UnitOfWork
{
    public interface IUnitOfWork : IDisposable
    {
        IGenaricRePo<TEntity> Repository<TEntity>() where TEntity : class;

        Task<int> CompleteAsync();
    }
}