using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Core.Entities;

namespace Core.Interfaces
{
    public interface IPortfolioRepository
    {
        Task<IEnumerable<Portfolio>> GetPortfolioByUserAsync(string userId);
        Task<Portfolio> GetPortfolioWithDetailsAsync(int portfolioId);
        Task<Portfolio> GetPortfolioWithProjectsAsync(int id);
    }
}
