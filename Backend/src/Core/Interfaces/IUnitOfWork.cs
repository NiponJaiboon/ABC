using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Core.Interfaces
{
    public interface IUnitOfWork
    {
        IGenericRepository<T> Repository<T>()
            where T : class;
        Task CommitAsync();
    }
}
