using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Core.Entities;

namespace Core.Interfaces
{
    public interface IPortfolioService
    {
        Task<IEnumerable<Portfolio>> GetAllPortfoliosAsync();
        Task<Portfolio> GetPortfolioByIdAsync(int id);
        Task<IEnumerable<Portfolio>> GetPortfoliosByUserIdAsync(string userId);
        Task<Portfolio> GetPortfolioWithDetailsAsync(int portfolioId);
        Task<Portfolio> CreatePortfolioAsync(Portfolio portfolio);
        Task<Portfolio> UpdatePortfolioAsync(Portfolio portfolio);
        Task<bool> DeletePortfolioAsync(int portfolioId);
        Task<bool> PortfolioExistsAsync(int id);
        Task<bool> UserOwnsPortfolioAsync(string userId, int portfolioId);
    }
}
