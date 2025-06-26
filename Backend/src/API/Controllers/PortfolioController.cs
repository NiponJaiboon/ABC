using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Application.Dtos;
using AutoMapper;
using Core.Entities;
using Core.Interfaces;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;

namespace API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class PortfolioController : ControllerBase
    {
        private readonly IPortfolioService _portfolioService;
        private readonly IMapper _mapper;
        private readonly ILogger<PortfolioController> _logger;

        public PortfolioController(
            IPortfolioService portfolioService,
            IMapper mapper,
            ILogger<PortfolioController> logger)
        {
            _portfolioService = portfolioService ?? throw new ArgumentNullException(nameof(portfolioService));
            _mapper = mapper ?? throw new ArgumentNullException(nameof(mapper));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        /// <summary>
        /// Get all portfolios
        /// </summary>
        /// <returns>List of all portfolios</returns>
        [HttpGet]
        [ProducesResponseType(typeof(IEnumerable<PortfolioDto>), 200)]
        [ProducesResponseType(500)]
        public async Task<ActionResult<IEnumerable<PortfolioDto>>> GetAllPortfolios()
        {
            try
            {
                _logger.LogInformation("Retrieving all portfolios");
                var portfolios = await _portfolioService.GetAllPortfoliosAsync();
                var portfolioDtos = _mapper.Map<IEnumerable<PortfolioDto>>(portfolios);

                _logger.LogInformation("Retrieved {Count} portfolios", portfolioDtos.Count());
                return Ok(portfolioDtos);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving all portfolios");
                return StatusCode(500, new {
                    Error = "Internal server error",
                    Message = "An error occurred while retrieving portfolios"
                });
            }
        }

        /// <summary>
        /// Get portfolio by ID
        /// </summary>
        /// <param name="id">Portfolio ID</param>
        /// <returns>Portfolio details</returns>
        [HttpGet("{id}")]
        [ProducesResponseType(typeof(PortfolioDto), 200)]
        [ProducesResponseType(404)]
        [ProducesResponseType(500)]
        public async Task<ActionResult<PortfolioDto>> GetPortfolioById(int id)
        {
            try
            {
                _logger.LogInformation("Retrieving portfolio with ID: {PortfolioId}", id);

                if (id <= 0)
                {
                    return BadRequest("Portfolio ID must be greater than 0");
                }

                var portfolio = await _portfolioService.GetPortfolioByIdAsync(id);

                if (portfolio == null)
                {
                    _logger.LogWarning("Portfolio with ID {PortfolioId} not found", id);
                    return NotFound(new { Message = $"Portfolio with ID {id} not found" });
                }

                var portfolioDto = _mapper.Map<PortfolioDto>(portfolio);
                return Ok(portfolioDto);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving portfolio with ID: {PortfolioId}", id);
                return StatusCode(500, new {
                    Error = "Internal server error",
                    Message = "An error occurred while retrieving the portfolio"
                });
            }
        }

        /// <summary>
        /// Get portfolios by user ID
        /// </summary>
        /// <param name="userId">User ID</param>
        /// <returns>List of user's portfolios</returns>
        [HttpGet("user/{userId}")]
        [ProducesResponseType(typeof(IEnumerable<PortfolioDto>), 200)]
        [ProducesResponseType(400)]
        [ProducesResponseType(500)]
        public async Task<ActionResult<IEnumerable<PortfolioDto>>> GetPortfoliosByUserId(string userId)
        {
            try
            {
                _logger.LogInformation("Retrieving portfolios for user: {UserId}", userId);

                if (string.IsNullOrWhiteSpace(userId))
                {
                    return BadRequest("User ID is required");
                }

                var portfolios = await _portfolioService.GetPortfoliosByUserIdAsync(userId);
                var portfolioDtos = _mapper.Map<IEnumerable<PortfolioDto>>(portfolios);

                _logger.LogInformation("Retrieved {Count} portfolios for user {UserId}",
                    portfolioDtos.Count(), userId);

                return Ok(portfolioDtos);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving portfolios for user: {UserId}", userId);
                return StatusCode(500, new {
                    Error = "Internal server error",
                    Message = "An error occurred while retrieving portfolios"
                });
            }
        }

        /// <summary>
        /// Create new portfolio
        /// </summary>
        /// <param name="request">Portfolio creation data</param>
        /// <returns>Created portfolio</returns>
        [HttpPost]
        [ProducesResponseType(typeof(PortfolioDto), 201)]
        [ProducesResponseType(400)]
        [ProducesResponseType(500)]
        public async Task<ActionResult<PortfolioDto>> CreatePortfolio([FromBody] CreatePortfolioRequest request)
        {
            try
            {
                _logger.LogInformation("Creating new portfolio: {PortfolioTitle} for User: {UserId}",
                    request.Title, request.UserId);

                if (!ModelState.IsValid)
                {
                    return BadRequest(ModelState);
                }

                // Map DTO to Entity using AutoMapper
                var portfolio = _mapper.Map<Portfolio>(request);

                var createdPortfolio = await _portfolioService.CreatePortfolioAsync(portfolio);
                var portfolioDto = _mapper.Map<PortfolioDto>(createdPortfolio);

                _logger.LogInformation("Portfolio created successfully with ID: {PortfolioId}", createdPortfolio.Id);

                return CreatedAtAction(
                    nameof(GetPortfolioById),
                    new { id = createdPortfolio.Id },
                    portfolioDto);
            }
            catch (ArgumentException ex)
            {
                _logger.LogWarning(ex, "Invalid portfolio data provided");
                return BadRequest(new { Message = ex.Message });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating portfolio");
                return StatusCode(500, new {
                    Error = "Internal server error",
                    Message = "An error occurred while creating the portfolio"
                });
            }
        }

        /// <summary>
        /// Update existing portfolio
        /// </summary>
        /// <param name="id">Portfolio ID</param>
        /// <param name="request">Portfolio update data</param>
        /// <returns>Updated portfolio</returns>
        [HttpPut("{id}")]
        [ProducesResponseType(typeof(PortfolioDto), 200)]
        [ProducesResponseType(400)]
        [ProducesResponseType(404)]
        [ProducesResponseType(500)]
        public async Task<ActionResult<PortfolioDto>> UpdatePortfolio(int id, [FromBody] UpdatePortfolioRequest request)
        {
            try
            {
                _logger.LogInformation("Updating portfolio with ID: {PortfolioId}", id);

                if (id <= 0)
                {
                    return BadRequest("Portfolio ID must be greater than 0");
                }

                if (id != request.Id)
                {
                    return BadRequest(new { Message = "Portfolio ID in URL does not match ID in request body" });
                }

                if (!ModelState.IsValid)
                {
                    return BadRequest(ModelState);
                }

                // Check if portfolio exists
                if (!await _portfolioService.PortfolioExistsAsync(id))
                {
                    _logger.LogWarning("Portfolio with ID {PortfolioId} not found for update", id);
                    return NotFound(new { Message = $"Portfolio with ID {id} not found" });
                }

                // Map DTO to Entity using AutoMapper
                var portfolio = _mapper.Map<Portfolio>(request);

                var updatedPortfolio = await _portfolioService.UpdatePortfolioAsync(portfolio);
                var portfolioDto = _mapper.Map<PortfolioDto>(updatedPortfolio);

                _logger.LogInformation("Portfolio with ID {PortfolioId} updated successfully", id);
                return Ok(portfolioDto);
            }
            catch (ArgumentException ex)
            {
                _logger.LogWarning(ex, "Invalid portfolio data provided for update");
                return BadRequest(new { Message = ex.Message });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating portfolio with ID: {PortfolioId}", id);
                return StatusCode(500, new {
                    Error = "Internal server error",
                    Message = "An error occurred while updating the portfolio"
                });
            }
        }

        /// <summary>
        /// Delete portfolio
        /// </summary>
        /// <param name="id">Portfolio ID</param>
        /// <returns>No content if successful</returns>
        [HttpDelete("{id}")]
        [ProducesResponseType(204)]
        [ProducesResponseType(404)]
        [ProducesResponseType(500)]
        public async Task<ActionResult> DeletePortfolio(int id)
        {
            try
            {
                _logger.LogInformation("Deleting portfolio with ID: {PortfolioId}", id);

                if (id <= 0)
                {
                    return BadRequest("Portfolio ID must be greater than 0");
                }

                var deleted = await _portfolioService.DeletePortfolioAsync(id);

                if (!deleted)
                {
                    _logger.LogWarning("Portfolio with ID {PortfolioId} not found for deletion", id);
                    return NotFound(new { Message = $"Portfolio with ID {id} not found" });
                }

                _logger.LogInformation("Portfolio with ID {PortfolioId} deleted successfully", id);
                return NoContent();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting portfolio with ID: {PortfolioId}", id);
                return StatusCode(500, new {
                    Error = "Internal server error",
                    Message = "An error occurred while deleting the portfolio"
                });
            }
        }

        /// <summary>
        /// Get portfolio with projects included
        /// </summary>
        /// <param name="id">Portfolio ID</param>
        /// <returns>Portfolio with projects</returns>
        [HttpGet("{id}/with-projects")]
        [ProducesResponseType(typeof(PortfolioWithProjectsDto), 200)]
        [ProducesResponseType(404)]
        [ProducesResponseType(500)]
        public async Task<ActionResult<PortfolioWithProjectsDto>> GetPortfolioWithProjects(int id)
        {
            try
            {
                _logger.LogInformation("Retrieving portfolio with projects for ID: {PortfolioId}", id);

                if (id <= 0)
                {
                    return BadRequest("Portfolio ID must be greater than 0");
                }

                var portfolio = await _portfolioService.GetPortfolioWithProjectsAsync(id);

                if (portfolio == null)
                {
                    _logger.LogWarning("Portfolio with ID {PortfolioId} not found", id);
                    return NotFound(new { Message = $"Portfolio with ID {id} not found" });
                }

                var portfolioDto = _mapper.Map<PortfolioWithProjectsDto>(portfolio);
                return Ok(portfolioDto);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving portfolio with projects for ID: {PortfolioId}", id);
                return StatusCode(500, new {
                    Error = "Internal server error",
                    Message = "An error occurred while retrieving the portfolio"
                });
            }
        }

        /// <summary>
        /// Toggle portfolio visibility (public/private)
        /// </summary>
        /// <param name="id">Portfolio ID</param>
        /// <returns>Updated portfolio</returns>
        [HttpPatch("{id}/toggle-visibility")]
        [ProducesResponseType(typeof(PortfolioDto), 200)]
        [ProducesResponseType(404)]
        [ProducesResponseType(500)]
        public async Task<ActionResult<PortfolioDto>> TogglePortfolioVisibility(int id)
        {
            try
            {
                _logger.LogInformation("Toggling visibility for portfolio: {PortfolioId}", id);

                if (id <= 0)
                {
                    return BadRequest("Portfolio ID must be greater than 0");
                }

                var portfolio = await _portfolioService.GetPortfolioByIdAsync(id);
                if (portfolio == null)
                {
                    _logger.LogWarning("Portfolio with ID {PortfolioId} not found", id);
                    return NotFound(new { Message = $"Portfolio with ID {id} not found" });
                }

                portfolio.IsPublic = !portfolio.IsPublic;
                portfolio.UpdatedAt = DateTime.UtcNow;

                var updatedPortfolio = await _portfolioService.UpdatePortfolioAsync(portfolio);
                var portfolioDto = _mapper.Map<PortfolioDto>(updatedPortfolio);

                _logger.LogInformation("Portfolio {PortfolioId} visibility toggled to {IsPublic}",
                    id, portfolio.IsPublic);

                return Ok(portfolioDto);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error toggling visibility for portfolio: {PortfolioId}", id);
                return StatusCode(500, new {
                    Error = "Internal server error",
                    Message = "An error occurred while updating portfolio visibility"
                });
            }
        }

        /// <summary>
        /// Test endpoint for health check
        /// </summary>
        /// <returns>Test response</returns>
        [HttpGet("test")]
        [ProducesResponseType(200)]
        public IActionResult Test()
        {
            _logger.LogInformation("Portfolio controller test endpoint called");
            return Ok(new {
                Message = "Portfolio Controller is working!",
                Timestamp = DateTime.UtcNow,
                Controller = "PortfolioController",
                Version = "1.0"
            });
        }
    }
}
