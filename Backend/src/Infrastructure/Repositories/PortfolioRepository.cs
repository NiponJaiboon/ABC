using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Core.Entities;
using Core.Interfaces;
using Dapper;
using Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Repositories
{
    public class PortfolioRepository : IPortfolioRepository
    {
        private readonly ApplicationDbContext _applicationDbContext;
        public PortfolioRepository(ApplicationDbContext applicationDbContext)
        {
            _applicationDbContext = applicationDbContext;

        }
        public async Task<IEnumerable<Portfolio>> GetPortfolioByUserAsync(string userId)
        {
            using (var connection = _applicationDbContext.Database.GetDbConnection())
            {
                var sql = "SELECT * FROM Portfolios WHERE UserId = @UserId";
                var portfolios = await connection.QueryAsync<Portfolio>(sql, new { UserId = userId });
                return portfolios;
            }
        }

        public async Task<Portfolio> GetPortfolioWithDetailsAsync(int portfolioId)
        {
            using (var connection = _applicationDbContext.Database.GetDbConnection())
            {
                var sql = "SELECT * FROM Portfolios WHERE Id = @Id";
                var portfolio = await connection.QueryFirstOrDefaultAsync<Portfolio>(
                    sql,
                    new { Id = portfolioId }
                );
                return portfolio;
            }
        }
    }
}
