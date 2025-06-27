using Core.Entities;
using Core.Interfaces;
using Microsoft.Extensions.Logging;

#nullable enable

namespace Application.Services
{
    public class PortfolioService : IPortfolioService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IPortfolioRepository _portfolioRepository;
        private readonly ILogger<PortfolioService> _logger;

        public PortfolioService(
            IPortfolioRepository portfolioRepository,
            IUnitOfWork unitOfWork,
            ILogger<PortfolioService> logger)
        {
            _portfolioRepository = portfolioRepository;
            _unitOfWork = unitOfWork;
            _logger = logger;
        }

        public async Task<IEnumerable<Portfolio>> GetAllPortfoliosAsync()
        {
            try
            {
                _logger.LogInformation("Retrieving all portfolios");
                return await _unitOfWork.Repository<Portfolio>().GetAllAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving all portfolios");
                throw;
            }
        }

        public async Task<Portfolio?> GetPortfolioByIdAsync(int id)
        {
            try
            {
                _logger.LogInformation("Retrieving portfolio with ID: {PortfolioId}", id);
                return await _unitOfWork.Repository<Portfolio>().GetByIdAsync(id);
            }
            catch (KeyNotFoundException)
            {
                _logger.LogInformation("Portfolio with ID {PortfolioId} not found", id);
                return null;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving portfolio with ID: {PortfolioId}", id);
                throw;
            }
        }

        public async Task<Portfolio> GetPortfolioWithProjectsAsync(int id)
        {
            try
            {
                _logger.LogInformation("Retrieving portfolio with projects for ID: {PortfolioId}", id);

                // Use the specialized repository method or generic repository with includes
                var portfolio = await _portfolioRepository.GetPortfolioWithProjectsAsync(id);

                if (portfolio == null)
                {
                    _logger.LogWarning("Portfolio with ID {PortfolioId} not found", id);
                    throw new KeyNotFoundException($"Portfolio with ID {id} not found");
                }

                return portfolio;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving portfolio with projects for ID: {PortfolioId}", id);
                throw;
            }
        }

        public async Task<IEnumerable<Portfolio>> GetPortfoliosByUserIdAsync(string userId)
        {
            try
            {
                _logger.LogInformation("Retrieving portfolios for user ID: {UserId}", userId);
                return await _portfolioRepository.GetPortfolioByUserAsync(userId);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving portfolios for user ID: {UserId}", userId);
                throw;
            }
        }

        public async Task<Portfolio> GetPortfolioWithDetailsAsync(int portfolioId)
        {
            try
            {
                _logger.LogInformation("Retrieving portfolio details for ID: {PortfolioId}", portfolioId);
                return await _portfolioRepository.GetPortfolioWithDetailsAsync(portfolioId);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving portfolio details for ID: {PortfolioId}", portfolioId);
                throw;
            }
        }

        public async Task<Portfolio> CreatePortfolioAsync(Portfolio portfolio)
        {
            try
            {
                _logger.LogInformation("Creating new portfolio: {PortfolioTitle}", portfolio.Title);

                // Validation
                ValidatePortfolio(portfolio);

                await _unitOfWork.Repository<Portfolio>().AddAsync(portfolio);
                await _unitOfWork.CommitAsync();

                _logger.LogInformation("Portfolio created successfully with ID: {PortfolioId}", portfolio.Id);
                return portfolio;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating portfolio: {PortfolioTitle}", portfolio?.Title);
                throw;
            }
        }

        public async Task<Portfolio> UpdatePortfolioAsync(Portfolio portfolio)
        {
            try
            {
                _logger.LogInformation("Updating portfolio with ID: {PortfolioId}", portfolio.Id);

                // ✅ Ensure UpdatedAt is set
                portfolio.UpdatedAt = DateTime.UtcNow;
                // Check if portfolio exists
                var existingPortfolio = await GetPortfolioByIdAsync(portfolio.Id);
                if (existingPortfolio == null)
                {
                    throw new ArgumentException($"Portfolio with ID {portfolio.Id} not found");
                }

                // Validation
                ValidatePortfolio(portfolio);

                await _unitOfWork.Repository<Portfolio>().UpdateAsync(portfolio);
                await _unitOfWork.CommitAsync();

                _logger.LogInformation("Portfolio updated successfully: {PortfolioId}", portfolio.Id);
                return portfolio;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating portfolio with ID: {PortfolioId}", portfolio?.Id);
                throw;
            }
        }

        public async Task<bool> DeletePortfolioAsync(int portfolioId)
        {
            try
            {
                _logger.LogInformation("Deleting portfolio with ID: {PortfolioId}", portfolioId);

                var portfolio = await GetPortfolioByIdAsync(portfolioId);
                if (portfolio == null)
                {
                    _logger.LogWarning("Portfolio with ID {PortfolioId} not found for deletion", portfolioId);
                    return false;
                }

                await _unitOfWork.Repository<Portfolio>().DeleteAsync(portfolioId);
                await _unitOfWork.CommitAsync();

                _logger.LogInformation("Portfolio deleted successfully: {PortfolioId}", portfolioId);
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting portfolio with ID: {PortfolioId}", portfolioId);
                throw;
            }
        }

        public async Task<bool> PortfolioExistsAsync(int id)
        {
            try
            {
                var portfolio = await GetPortfolioByIdAsync(id);
                return portfolio != null;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error checking if portfolio exists: {PortfolioId}", id);
                throw;
            }
        }

        public async Task<bool> UserOwnsPortfolioAsync(string userId, int portfolioId)
        {
            try
            {
                _logger.LogInformation("Checking if user {UserId} owns portfolio {PortfolioId}", userId, portfolioId);

                var portfolio = await GetPortfolioByIdAsync(portfolioId);
                return portfolio?.UserId == userId;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error checking portfolio ownership: UserId={UserId}, PortfolioId={PortfolioId}", userId, portfolioId);
                throw;
            }
        }

        private static void ValidatePortfolio(Portfolio portfolio)
        {
            if (portfolio == null)
                throw new ArgumentNullException(nameof(portfolio));

            if (string.IsNullOrWhiteSpace(portfolio.Title))
                throw new ArgumentException("Portfolio name is required", nameof(portfolio));

            if (portfolio.Title.Length > 200)
                throw new ArgumentException("Portfolio name cannot exceed 200 characters", nameof(portfolio));
        }

    }
}
