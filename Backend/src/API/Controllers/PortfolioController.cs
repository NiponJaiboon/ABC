using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Application.Dtos;
using Core.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class PortfolioController : ControllerBase
    {
        private readonly IPortfolioService _portfolioService;
        private readonly ILogger<PortfolioController> _logger;

        public PortfolioController(
            IPortfolioService portfolioService,
            ILogger<PortfolioController> logger)
        {
            _portfolioService = portfolioService;
            _logger = logger;
        }

        /// <summary>
        /// Get all portfolios
        /// </summary>
        /// <returns>List of portfolios</returns>
        [HttpGet]
        public async Task<ActionResult<IEnumerable<PortfolioDto>>> GetAllPortfolios()
        {
            try
            {
                var portfolios = await _portfolioService.GetAllPortfoliosAsync();
                return Ok(portfolios);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving all portfolios");
                return StatusCode(500, "An error occurred while retrieving portfolios");
            }
        }

        /// <summary>
        /// Get portfolio by ID
        /// </summary>
        /// <param name="id">Portfolio ID</param>
        /// <returns>Portfolio details</returns>
        [HttpGet("{id}")]
        public async Task<ActionResult<PortfolioDto>> GetPortfolioById(int id)
        {
            try
            {
                var portfolio = await _portfolioService.GetPortfolioByIdAsync(id);

                if (portfolio == null)
                {
                    return NotFound($"Portfolio with ID {id} not found");
                }

                return Ok(portfolio);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving portfolio with ID: {PortfolioId}", id);
                return StatusCode(500, "An error occurred while retrieving the portfolio");
            }
        }

    }
}
