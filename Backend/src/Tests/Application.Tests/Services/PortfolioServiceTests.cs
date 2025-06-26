// Tests/Application.Tests/Services/PortfolioServiceTests.cs
using Application.Services;
using Core.Entities;
using Core.Interfaces;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;
using FluentAssertions;

namespace Application.Tests.Services
{
    public class PortfolioServiceTests
    {
        private readonly Mock<IUnitOfWork> _mockUnitOfWork;
        private readonly Mock<IPortfolioRepository> _mockPortfolioRepository;
        private readonly Mock<ILogger<PortfolioService>> _mockLogger;
        private readonly Mock<IGenericRepository<Portfolio>> _mockGenericRepository;
        private readonly PortfolioService _portfolioService;

        public PortfolioServiceTests()
        {
            _mockUnitOfWork = new Mock<IUnitOfWork>();
            _mockPortfolioRepository = new Mock<IPortfolioRepository>();
            _mockLogger = new Mock<ILogger<PortfolioService>>();
            _mockGenericRepository = new Mock<IGenericRepository<Portfolio>>();

            // Setup UnitOfWork to return mock repository
            _mockUnitOfWork.Setup(u => u.Repository<Portfolio>())
                          .Returns(_mockGenericRepository.Object);

            _portfolioService = new PortfolioService(
                _mockPortfolioRepository.Object,
                _mockUnitOfWork.Object,
                _mockLogger.Object);
        }

        [Fact]
        public async Task GetPortfolioByIdAsync_WhenPortfolioExists_ReturnsPortfolio()
        {
            // Arrange
            var portfolioId = 1;
            var expectedPortfolio = new Portfolio
            {
                Id = portfolioId,
                Title = "Test Portfolio",
                UserId = "user123"
            };

            _mockGenericRepository.Setup(r => r.GetByIdAsync(portfolioId))
                                 .ReturnsAsync(expectedPortfolio);

            // Act
            var result = await _portfolioService.GetPortfolioByIdAsync(portfolioId);

            // Assert
            result.Should().NotBeNull();
            result!.Id.Should().Be(portfolioId);
            result.Title.Should().Be("Test Portfolio");
            result.UserId.Should().Be("user123");
        }

        [Fact]
        public async Task GetPortfolioByIdAsync_WhenPortfolioDoesNotExist_ReturnsNull()
        {
            // Arrange
            var portfolioId = 999;
            _mockGenericRepository.Setup(r => r.GetByIdAsync(portfolioId))
                                 .ReturnsAsync((Portfolio?)null);

            // Act
            var result = await _portfolioService.GetPortfolioByIdAsync(portfolioId);

            // Assert
            result.Should().BeNull();
        }

        [Fact]
        public async Task CreatePortfolioAsync_WithValidPortfolio_ReturnsCreatedPortfolio()
        {
            // Arrange
            var portfolio = new Portfolio
            {
                Title = "New Portfolio",
                Description = "Test Description",
                UserId = "user123"
            };

            _mockGenericRepository.Setup(r => r.AddAsync(It.IsAny<Portfolio>()))
                                 .Returns(Task.CompletedTask);
            _mockUnitOfWork.Setup(u => u.CommitAsync())
                          .Returns(Task.CompletedTask);

            // Act
            var result = await _portfolioService.CreatePortfolioAsync(portfolio);

            // Assert
            result.Should().NotBeNull();
            result.Title.Should().Be("New Portfolio");

            // Verify that methods were called
            _mockGenericRepository.Verify(r => r.AddAsync(portfolio), Times.Once);
            _mockUnitOfWork.Verify(u => u.CommitAsync(), Times.Once);
        }

        [Theory]
        [InlineData(null)]
        [InlineData("")]
        [InlineData("   ")]
        public async Task CreatePortfolioAsync_WithInvalidTitle_ThrowsArgumentException(string? invalidTitle)
        {
            // Arrange
            var portfolio = new Portfolio
            {
                Title = invalidTitle!,
                Description = "Test Description",
                UserId = "user123"
            };

            // Act & Assert
            await Assert.ThrowsAsync<ArgumentException>(() =>
                _portfolioService.CreatePortfolioAsync(portfolio));
        }

        [Fact]
        public async Task CreatePortfolioAsync_WithTitleTooLong_ThrowsArgumentException()
        {
            // Arrange
            var portfolio = new Portfolio
            {
                Title = new string('A', 201), // 201 characters
                Description = "Test Description",
                UserId = "user123"
            };

            // Act & Assert
            var exception = await Assert.ThrowsAsync<ArgumentException>(() =>
                _portfolioService.CreatePortfolioAsync(portfolio));

            exception.Message.Should().Contain("cannot exceed 200 characters");
        }

        [Fact]
        public async Task DeletePortfolioAsync_WhenPortfolioExists_ReturnsTrue()
        {
            // Arrange
            var portfolioId = 1;
            var portfolio = new Portfolio { Id = portfolioId, Title = "Test Portfolio" };

            _mockGenericRepository.Setup(r => r.GetByIdAsync(portfolioId))
                                 .ReturnsAsync(portfolio);
            _mockGenericRepository.Setup(r => r.DeleteAsync(portfolioId))
                                 .Returns(Task.CompletedTask);
            _mockUnitOfWork.Setup(u => u.CommitAsync())
                          .Returns(Task.CompletedTask);

            // Act
            var result = await _portfolioService.DeletePortfolioAsync(portfolioId);

            // Assert
            result.Should().BeTrue();
            _mockGenericRepository.Verify(r => r.DeleteAsync(portfolioId), Times.Once);
            _mockUnitOfWork.Verify(u => u.CommitAsync(), Times.Once);
        }

        [Fact]
        public async Task DeletePortfolioAsync_WhenPortfolioDoesNotExist_ReturnsFalse()
        {
            // Arrange
            var portfolioId = 999;
            _mockGenericRepository.Setup(r => r.GetByIdAsync(portfolioId))
                                 .ReturnsAsync((Portfolio?)null);

            // Act
            var result = await _portfolioService.DeletePortfolioAsync(portfolioId);

            // Assert
            result.Should().BeFalse();
            _mockGenericRepository.Verify(r => r.DeleteAsync(It.IsAny<int>()), Times.Never);
            _mockUnitOfWork.Verify(u => u.CommitAsync(), Times.Never);
        }

        [Fact]
        public async Task UserOwnsPortfolioAsync_WhenUserOwnsPortfolio_ReturnsTrue()
        {
            // Arrange
            var userId = "user123";
            var portfolioId = 1;
            var portfolio = new Portfolio { Id = portfolioId, UserId = userId };

            _mockGenericRepository.Setup(r => r.GetByIdAsync(portfolioId))
                                 .ReturnsAsync(portfolio);

            // Act
            var result = await _portfolioService.UserOwnsPortfolioAsync(userId, portfolioId);

            // Assert
            result.Should().BeTrue();
        }

        [Fact]
        public async Task UserOwnsPortfolioAsync_WhenUserDoesNotOwnPortfolio_ReturnsFalse()
        {
            // Arrange
            var userId = "user123";
            var portfolioId = 1;
            var portfolio = new Portfolio { Id = portfolioId, UserId = "different-user" };

            _mockGenericRepository.Setup(r => r.GetByIdAsync(portfolioId))
                                 .ReturnsAsync(portfolio);

            // Act
            var result = await _portfolioService.UserOwnsPortfolioAsync(userId, portfolioId);

            // Assert
            result.Should().BeFalse();
        }
    }
}
