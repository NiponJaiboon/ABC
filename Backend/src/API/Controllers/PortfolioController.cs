using Application.Dtos;
using Application.Services;
using Core.Entities;
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

        public PortfolioController(IPortfolioService portfolioService, ILogger<PortfolioController> logger)
        {
            _portfolioService = portfolioService ?? throw new ArgumentNullException(nameof(portfolioService));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        [HttpGet]
        public async Task<ActionResult<IEnumerable<PortfolioDto>>> GetPortfolios()
        {
            try
            {
                _logger.LogInformation("Getting all portfolios");
                var portfolios = await _portfolioService.GetAllPortfoliosAsync();
                var portfolioDtos = portfolios.Select(p => new PortfolioDto
                {
                    Id = p.Id,
                    Title = p.Title,
                    Description = p.Description,
                    UserId = p.UserId
                });
                return Ok(portfolioDtos);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting portfolios");
                return StatusCode(500, new { Error = "Internal server error", Message = ex.Message });
            }
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<Portfolio>> GetPortfolio(int id)
        {
            try
            {
                _logger.LogInformation("Getting portfolio with ID: {Id}", id);
                var portfolio = await _portfolioService.GetPortfolioByIdAsync(id);

                if (portfolio == null)
                {
                    return NotFound($"Portfolio with ID {id} not found");
                }

                return Ok(portfolio);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting portfolio with ID: {Id}", id);
                return StatusCode(500, new { Error = "Internal server error", Message = ex.Message });
            }
        }

        [HttpPost]
        public async Task<ActionResult<Portfolio>> CreatePortfolio([FromBody] Portfolio portfolio)
        {
            try
            {
                _logger.LogInformation("Creating new portfolio");
                var createdPortfolio = await _portfolioService.CreatePortfolioAsync(portfolio);
                return CreatedAtAction(nameof(GetPortfolio), new { id = createdPortfolio.Id }, createdPortfolio);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating portfolio");
                return StatusCode(500, new { Error = "Internal server error", Message = ex.Message });
            }
        }

        // เพิ่ม endpoint ทดสอบง่ายๆ
        [HttpGet("test")]
        public IActionResult Test()
        {
            _logger.LogInformation("Portfolio controller test endpoint called");
            return Ok(new { Message = "Portfolio Controller is working!", Timestamp = DateTime.Now });
        }
    }
}