using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Transactions;
using Core.Attributes;
using Core.Interfaces;
using Infrastructure.Repositories;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Data
{
    public class UnitOfWork : IUnitOfWork, IDisposable
    {
        private readonly ApplicationDbContext _applicationDbContext;
        // private readonly BusinessDbContext _businessDbContext;        // เพิ่มใหม่
        // private readonly ReportingDbContext _reportingDbContext;      // เพิ่มใหม่

        private readonly Dictionary<Type, object> _repositories = new();
        private bool _disposed = false;

        public UnitOfWork(
            ApplicationDbContext applicationDbContext
            // BusinessDbContext businessDbContext,        // เพิ่มใหม่
            // ReportingDbContext reportingDbContext       // เพิ่มใหม่
            )
        {
            _applicationDbContext = applicationDbContext;
        }

        public IGenericRepository<T> Repository<T>()
            where T : class
        {
            var type = typeof(T);
            if (_repositories.ContainsKey(type))
                return (IGenericRepository<T>)_repositories[type];

            // อ่าน Attribute เพื่อเลือก DbContext
            var attr =
                type.GetCustomAttributes(typeof(DbContextNameAttribute), false).FirstOrDefault()
                as DbContextNameAttribute;

            DbContext dbContext = attr?.Name switch
            {
                "ApplicationDbContext" => _applicationDbContext,
                // "BusinessDbContext" => _businessDbContext,        // เพิ่มใหม่
                // "ReportingDbContext" => _reportingDbContext,      // เพิ่มใหม่
                _ => _applicationDbContext, // Default
            };

            var repoInstance = new GenericRepository<T>(dbContext);
            _repositories[type] = repoInstance;
            return repoInstance;
        }

        public async Task CommitAsync()
        {
            // ใช้ TransactionScope เพื่อให้แน่ใจว่าการเปลี่ยนแปลงในทั้งสอง DbContext จะถูก Commit หรือ Rollback พร้อมกัน
            // TransactionScopeAsyncFlowOption.Enabled ช่วยให้สามารถใช้ TransactionScope ใน async method ได้
            using (var scope = new TransactionScope(TransactionScopeAsyncFlowOption.Enabled))
            {
                try
                {
                    await _applicationDbContext.SaveChangesAsync();
                    // await _businessDbContext.SaveChangesAsync();        // เพิ่มใหม่
                    // await _reportingDbContext.SaveChangesAsync();      // เพิ่มใหม่

                    scope.Complete();
                }
                catch (Exception ex)
                {
                    // Logging example (uncomment if using ILogger)
                    // _logger?.LogError(ex, "Error occurred during CommitAsync in UnitOfWork.");
                    Console.WriteLine($"Error in CommitAsync: {ex.Message}");
                    throw;
                }
            }
        }

        /// <summary>
        /// Rollback changes in all tracked entities for both DbContexts.
        /// </summary>
        public Task RollbackAsync()
        {
            RollbackDbContext(_applicationDbContext);
            // RollbackDbContext(_businessDbContext);        // เพิ่มใหม่
            // RollbackDbContext(_reportingDbContext);      // เพิ่มใหม่
            return Task.CompletedTask;
        }

        private void RollbackDbContext(DbContext dbContext)
        {
            foreach (var entry in dbContext.ChangeTracker.Entries())
            {
                switch (entry.State)
                {
                    case EntityState.Modified:
                    case EntityState.Deleted:
                        entry.State = EntityState.Unchanged;
                        break;
                    case EntityState.Added:
                        entry.State = EntityState.Detached;
                        break;
                }
            }
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (!_disposed)
            {
                if (disposing)
                {
                    _applicationDbContext?.Dispose();
                    // _businessDbContext?.Dispose();        // เพิ่มใหม่
                    // _reportingDbContext?.Dispose();      // เพิ่มใหม่
                }
                _disposed = true;
            }
        }
    }
}
