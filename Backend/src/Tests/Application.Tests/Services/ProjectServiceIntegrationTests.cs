using Application.Services;
using Core.Entities;
using Core.Interfaces;
using FluentAssertions;
using Infrastructure.Data;
using Infrastructure.Repositories;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;

namespace Application.Tests.Services;

public class ProjectServiceIntegrationTests : IDisposable
{
    private readonly ApplicationDbContext _context;
    private readonly ProjectService _projectService;
    private readonly UnitOfWork _unitOfWork;
    private readonly ProjectRepository _projectRepository;
    private readonly Mock<IPortfolioService> _mockPortfolioService;

    public ProjectServiceIntegrationTests()
    {
        var options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;

        _context = new ApplicationDbContext(options);
        _unitOfWork = new UnitOfWork(_context);
        _projectRepository = new ProjectRepository(_context);
        _mockPortfolioService = new Mock<IPortfolioService>();
        var mockLogger = new Mock<ILogger<ProjectService>>();

        _projectService = new ProjectService(
            _unitOfWork,
            _projectRepository,
            _mockPortfolioService.Object,
            mockLogger.Object
        );

        SeedTestData();
    }

    private void SeedTestData()
    {
        var portfolios = new List<Portfolio>
        {
            new Portfolio
            {
                Id = 1,
                Title = "Test Portfolio 1",
                Description = "Portfolio for testing",
                UserId = "user123",
                IsPublic = true,
                CreatedAt = DateTime.UtcNow.AddDays(-10),
                UpdatedAt = DateTime.UtcNow.AddDays(-5)
            },
            new Portfolio
            {
                Id = 2,
                Title = "Another Portfolio",
                Description = "Second portfolio",
                UserId = "user456",
                IsPublic = false,
                CreatedAt = DateTime.UtcNow.AddDays(-8),
                UpdatedAt = DateTime.UtcNow.AddDays(-3)
            }
        };

        var projects = new List<Project>
        {
            new Project
            {
                Id = 1,
                Title = "Test Project 1",
                Description = "First test project",
                PortfolioId = 1,
                StartDate = DateTime.UtcNow.AddDays(-30),
                EndDate = DateTime.UtcNow.AddDays(-10),
                IsCompleted = true,
                CreatedAt = DateTime.UtcNow.AddDays(-30),
                UpdatedAt = DateTime.UtcNow.AddDays(-10)
            },
            new Project
            {
                Id = 2,
                Title = "Active Project",
                Description = "Project in progress",
                PortfolioId = 1,
                StartDate = DateTime.UtcNow.AddDays(-15),
                IsCompleted = false,
                CreatedAt = DateTime.UtcNow.AddDays(-15),
                UpdatedAt = DateTime.UtcNow.AddDays(-5)
            },
            new Project
            {
                Id = 3,
                Title = "Another User Project",
                Description = "Project for different user",
                PortfolioId = 2,
                StartDate = DateTime.UtcNow.AddDays(-20),
                IsCompleted = false,
                CreatedAt = DateTime.UtcNow.AddDays(-20),
                UpdatedAt = DateTime.UtcNow.AddDays(-2)
            }
        };

        _context.Portfolios.AddRange(portfolios);
        _context.Projects.AddRange(projects);
        _context.SaveChanges();
    }

    [Fact]
    public async Task GetAllProjectsAsync_ReturnsAllProjects()
    {
        // Act
        var result = await _projectService.GetAllProjectsAsync();

        // Assert
        result.Should().NotBeNull();
        result.Should().HaveCount(3);
        result.Select(p => p.Title).Should().Contain(new[] { "Test Project 1", "Active Project", "Another User Project" });
    }

    [Fact]
    public async Task GetProjectByIdAsync_WithExistingProject_ReturnsProject()
    {
        // Act
        var result = await _projectService.GetProjectByIdAsync(1);

        // Assert
        result.Should().NotBeNull();
        result!.Id.Should().Be(1);
        result.Title.Should().Be("Test Project 1");
        result.PortfolioId.Should().Be(1);
        result.IsCompleted.Should().BeTrue();
        result.EndDate.Should().NotBeNull();
    }

    [Fact]
    public async Task GetProjectByIdAsync_WithNonExistentProject_ReturnsNull()
    {
        // Act
        var result = await _projectService.GetProjectByIdAsync(999);

        // Assert
        result.Should().BeNull();
    }

    [Fact]
    public async Task GetProjectsByPortfolioIdAsync_ReturnsProjectsForPortfolio()
    {
        // Act
        var result = await _projectService.GetProjectsByPortfolioIdAsync(1);

        // Assert
        result.Should().NotBeNull();
        result.Should().HaveCount(2);
        result.All(p => p.PortfolioId == 1).Should().BeTrue();
        result.Select(p => p.Title).Should().Contain(new[] { "Test Project 1", "Active Project" });
    }

    [Fact]
    public async Task GetProjectsByPortfolioIdAsync_WithEmptyPortfolio_ReturnsEmptyList()
    {
        // Act
        var result = await _projectService.GetProjectsByPortfolioIdAsync(999);

        // Assert
        result.Should().NotBeNull();
        result.Should().BeEmpty();
    }

    [Fact]
    public async Task CreateProjectAsync_WithValidData_CreatesProject()
    {
        // Arrange
        var newProject = new Project
        {
            Title = "New Integration Project",
            Description = "Created in integration test",
            PortfolioId = 1,
            StartDate = DateTime.UtcNow,
            IsCompleted = false
        };

        // Mock portfolio exists
        _mockPortfolioService.Setup(p => p.PortfolioExistsAsync(1))
                           .ReturnsAsync(true);

        // Act
        var result = await _projectService.CreateProjectAsync(newProject);

        // Assert
        result.Should().NotBeNull();
        result.Id.Should().BeGreaterThan(0);
        result.Title.Should().Be("New Integration Project");
        result.PortfolioId.Should().Be(1);
        result.CreatedAt.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromMinutes(1));

        // Verify it was persisted
        var persistedProject = await _context.Projects.FindAsync(result.Id);
        persistedProject.Should().NotBeNull();
        persistedProject!.Title.Should().Be("New Integration Project");
    }

    [Fact]
    public async Task CreateProjectAsync_WithNonExistentPortfolio_ThrowsArgumentException()
    {
        // Arrange
        var newProject = new Project
        {
            Title = "Project for Non-existent Portfolio",
            PortfolioId = 999
        };

        // Mock portfolio does not exist
        _mockPortfolioService.Setup(p => p.PortfolioExistsAsync(999))
                           .ReturnsAsync(false);

        // Act & Assert
        await _projectService.Invoking(p => p.CreateProjectAsync(newProject))
                           .Should().ThrowAsync<ArgumentException>()
                           .WithMessage("Portfolio with ID 999 does not exist");
    }

    [Fact]
    public async Task CreateProjectAsync_WithEmptyTitle_ThrowsArgumentException()
    {
        // Arrange
        var newProject = new Project
        {
            Title = "",
            PortfolioId = 1
        };

        // Mock portfolio exists
        _mockPortfolioService.Setup(p => p.PortfolioExistsAsync(1))
                           .ReturnsAsync(true);

        // Act & Assert
        await _projectService.Invoking(p => p.CreateProjectAsync(newProject))
                           .Should().ThrowAsync<ArgumentException>()
                           .WithMessage("Project title is required");
    }

    [Fact]
    public async Task UpdateProjectAsync_WithValidData_UpdatesProject()
    {
        // Arrange
        var updatedProject = new Project
        {
            Id = 2,
            Title = "Updated Active Project",
            Description = "Updated description",
            PortfolioId = 1,
            StartDate = DateTime.UtcNow.AddDays(-15),
            IsCompleted = true,
            EndDate = DateTime.UtcNow,
            CreatedAt = DateTime.UtcNow.AddDays(-15)
        };

        // Act
        var result = await _projectService.UpdateProjectAsync(updatedProject);

        // Assert
        result.Should().NotBeNull();
        result.Title.Should().Be("Updated Active Project");
        result.IsCompleted.Should().BeTrue();
        result.UpdatedAt.Should().NotBeNull();
        result.UpdatedAt.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromMinutes(1));

        // Verify persistence
        var persistedProject = await _context.Projects.FindAsync(2);
        persistedProject.Should().NotBeNull();
        persistedProject!.Title.Should().Be("Updated Active Project");
        persistedProject.IsCompleted.Should().BeTrue();
    }

    [Fact]
    public async Task UpdateProjectAsync_WithNonExistentProject_ThrowsArgumentException()
    {
        // Arrange
        var nonExistentProject = new Project
        {
            Id = 999,
            Title = "Non-existent Project",
            PortfolioId = 1
        };

        // Act & Assert
        await _projectService.Invoking(p => p.UpdateProjectAsync(nonExistentProject))
                           .Should().ThrowAsync<ArgumentException>()
                           .WithMessage("Project with ID 999 does not exist");
    }

    [Fact]
    public async Task DeleteProjectAsync_WithExistingProject_DeletesAndReturnsTrue()
    {
        // Act
        var result = await _projectService.DeleteProjectAsync(3);

        // Assert
        result.Should().BeTrue();

        // Verify deletion
        var deletedProject = await _context.Projects.FindAsync(3);
        deletedProject.Should().BeNull();
    }

    [Fact]
    public async Task DeleteProjectAsync_WithNonExistentProject_ReturnsFalse()
    {
        // Act
        var result = await _projectService.DeleteProjectAsync(999);

        // Assert
        result.Should().BeFalse();
    }

    [Fact]
    public async Task ProjectExistsAsync_WithExistingProject_ReturnsTrue()
    {
        // Act
        var result = await _projectService.ProjectExistsAsync(1);

        // Assert
        result.Should().BeTrue();
    }

    [Fact]
    public async Task ProjectExistsAsync_WithNonExistentProject_ReturnsFalse()
    {
        // Act
        var result = await _projectService.ProjectExistsAsync(999);

        // Assert
        result.Should().BeFalse();
    }

    [Fact]
    public async Task UserOwnsProjectAsync_WithOwnerUser_ReturnsTrue()
    {
        // Act
        var result = await _projectService.UserOwnsProjectAsync("user123", 1);

        // Assert
        result.Should().BeTrue();
    }

    [Fact]
    public async Task UserOwnsProjectAsync_WithNonOwnerUser_ReturnsFalse()
    {
        // Act
        var result = await _projectService.UserOwnsProjectAsync("different-user", 1);

        // Assert
        result.Should().BeFalse();
    }

    [Fact]
    public async Task UserOwnsProjectAsync_WithNonExistentProject_ReturnsFalse()
    {
        // Act
        var result = await _projectService.UserOwnsProjectAsync("user123", 999);

        // Assert
        result.Should().BeFalse();
    }

    private bool _disposed = false;

    public void Dispose()
    {
        Dispose(true);
        GC.SuppressFinalize(this);
    }

    protected virtual void Dispose(bool disposing)
    {
        if (!_disposed && disposing)
        {
            _context?.Dispose();
            _disposed = true;
        }
    }
}
