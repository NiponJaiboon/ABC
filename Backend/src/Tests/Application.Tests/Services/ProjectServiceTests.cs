using Application.Services;
using Core.Entities;
using Core.Interfaces;
using FluentAssertions;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;

namespace Application.Tests.Services;

public class ProjectServiceTests
{
    private readonly Mock<IUnitOfWork> _mockUnitOfWork;
    private readonly Mock<IProjectRepository> _mockProjectRepository;
    private readonly Mock<IPortfolioService> _mockPortfolioService;
    private readonly Mock<ILogger<ProjectService>> _mockLogger;
    private readonly Mock<IGenericRepository<Project>> _mockGenericRepository;
    private readonly ProjectService _projectService;

    public ProjectServiceTests()
    {
        _mockUnitOfWork = new Mock<IUnitOfWork>();
        _mockProjectRepository = new Mock<IProjectRepository>();
        _mockPortfolioService = new Mock<IPortfolioService>();
        _mockLogger = new Mock<ILogger<ProjectService>>();
        _mockGenericRepository = new Mock<IGenericRepository<Project>>();

        // Setup UnitOfWork to return the mock generic repository
        _mockUnitOfWork.Setup(u => u.Repository<Project>())
                      .Returns(_mockGenericRepository.Object);

        _projectService = new ProjectService(
            _mockUnitOfWork.Object,
            _mockProjectRepository.Object,
            _mockPortfolioService.Object,
            _mockLogger.Object
        );
    }

    [Fact]
    public async Task GetAllProjectsAsync_ReturnsAllProjects()
    {
        // Arrange
        var expectedProjects = new List<Project>
        {
            new Project { Id = 1, Title = "Project 1", PortfolioId = 1 },
            new Project { Id = 2, Title = "Project 2", PortfolioId = 1 },
            new Project { Id = 3, Title = "Project 3", PortfolioId = 2 }
        };

        _mockGenericRepository.Setup(r => r.GetAllAsync())
                             .ReturnsAsync(expectedProjects);

        // Act
        var result = await _projectService.GetAllProjectsAsync();

        // Assert
        result.Should().NotBeNull();
        result.Should().HaveCount(3);
        result.Should().BeEquivalentTo(expectedProjects);
    }

    [Fact]
    public async Task GetProjectByIdAsync_WithExistingProject_ReturnsProject()
    {
        // Arrange
        var projectId = 1;
        var expectedProject = new Project
        {
            Id = projectId,
            Title = "Test Project",
            Description = "Test Description",
            PortfolioId = 1
        };

        _mockGenericRepository.Setup(r => r.GetByIdAsync(projectId))
                             .ReturnsAsync(expectedProject);

        // Act
        var result = await _projectService.GetProjectByIdAsync(projectId);

        // Assert
        result.Should().NotBeNull();
        result!.Id.Should().Be(projectId);
        result.Title.Should().Be("Test Project");
        result.Description.Should().Be("Test Description");
        result.PortfolioId.Should().Be(1);
    }

    [Fact]
    public async Task GetProjectByIdAsync_WithNonExistentProject_ReturnsNull()
    {
        // Arrange
        var projectId = 999;
        _mockGenericRepository.Setup(r => r.GetByIdAsync(projectId))
                             .ReturnsAsync((Project?)null);

        // Act
        var result = await _projectService.GetProjectByIdAsync(projectId);

        // Assert
        result.Should().BeNull();
    }

    [Fact]
    public async Task GetProjectsByPortfolioIdAsync_ReturnsProjectsForPortfolio()
    {
        // Arrange
        var portfolioId = 1;
        var expectedProjects = new List<Project>
        {
            new Project { Id = 1, Title = "Project 1", PortfolioId = portfolioId },
            new Project { Id = 2, Title = "Project 2", PortfolioId = portfolioId }
        };

        _mockProjectRepository.Setup(r => r.GetProjectsByPortfolioIdAsync(portfolioId))
                             .ReturnsAsync(expectedProjects);

        // Act
        var result = await _projectService.GetProjectsByPortfolioIdAsync(portfolioId);

        // Assert
        result.Should().NotBeNull();
        result.Should().HaveCount(2);
        result.All(p => p.PortfolioId == portfolioId).Should().BeTrue();
    }

    [Fact]
    public async Task CreateProjectAsync_WithValidProject_ReturnsCreatedProject()
    {
        // Arrange
        var project = new Project
        {
            Title = "New Project",
            Description = "Project Description",
            PortfolioId = 1,
            StartDate = DateTime.UtcNow
        };

        _mockPortfolioService.Setup(p => p.PortfolioExistsAsync(1))
                           .ReturnsAsync(true);
        _mockGenericRepository.Setup(r => r.AddAsync(It.IsAny<Project>()))
                             .Returns(Task.CompletedTask);
        _mockUnitOfWork.Setup(u => u.CommitAsync())
                      .Returns(Task.CompletedTask);

        // Act
        var result = await _projectService.CreateProjectAsync(project);

        // Assert
        result.Should().NotBeNull();
        result.Title.Should().Be("New Project");
        result.CreatedAt.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromMinutes(1));

        // Verify that methods were called
        _mockPortfolioService.Verify(p => p.PortfolioExistsAsync(1), Times.Once);
        _mockGenericRepository.Verify(r => r.AddAsync(project), Times.Once);
        _mockUnitOfWork.Verify(u => u.CommitAsync(), Times.Once);
    }

    [Fact]
    public async Task CreateProjectAsync_WithNonExistentPortfolio_ThrowsArgumentException()
    {
        // Arrange
        var project = new Project
        {
            Title = "New Project",
            PortfolioId = 999
        };

        _mockPortfolioService.Setup(p => p.PortfolioExistsAsync(999))
                           .ReturnsAsync(false);

        // Act & Assert
        await _projectService.Invoking(p => p.CreateProjectAsync(project))
                           .Should().ThrowAsync<ArgumentException>()
                           .WithMessage("Portfolio with ID 999 does not exist");

        // Verify portfolio check was called but add/commit were not
        _mockPortfolioService.Verify(p => p.PortfolioExistsAsync(999), Times.Once);
        _mockGenericRepository.Verify(r => r.AddAsync(It.IsAny<Project>()), Times.Never);
        _mockUnitOfWork.Verify(u => u.CommitAsync(), Times.Never);
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public async Task CreateProjectAsync_WithInvalidTitle_ThrowsArgumentException(string? invalidTitle)
    {
        // Arrange
        var project = new Project
        {
            Title = invalidTitle!,
            PortfolioId = 1
        };

        _mockPortfolioService.Setup(p => p.PortfolioExistsAsync(1))
                           .ReturnsAsync(true);

        // Act & Assert
        await _projectService.Invoking(p => p.CreateProjectAsync(project))
                           .Should().ThrowAsync<ArgumentException>()
                           .WithMessage("Project title is required");

        // Verify add/commit were not called
        _mockGenericRepository.Verify(r => r.AddAsync(It.IsAny<Project>()), Times.Never);
        _mockUnitOfWork.Verify(u => u.CommitAsync(), Times.Never);
    }

    [Fact]
    public async Task CreateProjectAsync_WithDefaultStartDate_SetsStartDateToNow()
    {
        // Arrange
        var project = new Project
        {
            Title = "New Project",
            PortfolioId = 1,
            StartDate = default // default DateTime
        };

        _mockPortfolioService.Setup(p => p.PortfolioExistsAsync(1))
                           .ReturnsAsync(true);
        _mockGenericRepository.Setup(r => r.AddAsync(It.IsAny<Project>()))
                             .Returns(Task.CompletedTask);
        _mockUnitOfWork.Setup(u => u.CommitAsync())
                      .Returns(Task.CompletedTask);

        // Act
        var result = await _projectService.CreateProjectAsync(project);

        // Assert
        result.StartDate.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromMinutes(1));
    }

    [Fact]
    public async Task UpdateProjectAsync_WithValidProject_ReturnsUpdatedProject()
    {
        // Arrange
        var existingProject = new Project
        {
            Id = 1,
            Title = "Original Title",
            PortfolioId = 1
        };

        var updatedProject = new Project
        {
            Id = 1,
            Title = "Updated Title",
            Description = "Updated Description",
            PortfolioId = 1,
            IsCompleted = true
        };

        _mockGenericRepository.Setup(r => r.GetByIdAsync(1))
                             .ReturnsAsync(existingProject);
        _mockGenericRepository.Setup(r => r.UpdateAsync(It.IsAny<Project>()))
                             .Returns(Task.CompletedTask);
        _mockUnitOfWork.Setup(u => u.CommitAsync())
                      .Returns(Task.CompletedTask);

        // Act
        var result = await _projectService.UpdateProjectAsync(updatedProject);

        // Assert
        result.Should().NotBeNull();
        result.Title.Should().Be("Updated Title");
        result.UpdatedAt.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromMinutes(1));

        // Verify methods were called
        _mockGenericRepository.Verify(r => r.GetByIdAsync(1), Times.Once);
        _mockGenericRepository.Verify(r => r.UpdateAsync(updatedProject), Times.Once);
        _mockUnitOfWork.Verify(u => u.CommitAsync(), Times.Once);
    }

    [Fact]
    public async Task UpdateProjectAsync_WithNonExistentProject_ThrowsArgumentException()
    {
        // Arrange
        var project = new Project
        {
            Id = 999,
            Title = "Non-existent Project"
        };

        _mockGenericRepository.Setup(r => r.GetByIdAsync(999))
                             .ReturnsAsync((Project?)null);

        // Act & Assert
        await _projectService.Invoking(p => p.UpdateProjectAsync(project))
                           .Should().ThrowAsync<ArgumentException>()
                           .WithMessage("Project with ID 999 does not exist");

        // Verify update and commit were not called
        _mockGenericRepository.Verify(r => r.UpdateAsync(It.IsAny<Project>()), Times.Never);
        _mockUnitOfWork.Verify(u => u.CommitAsync(), Times.Never);
    }

    [Fact]
    public async Task DeleteProjectAsync_WithExistingProject_ReturnsTrue()
    {
        // Arrange
        var projectId = 1;
        var existingProject = new Project { Id = projectId, Title = "Test Project" };

        _mockGenericRepository.Setup(r => r.GetByIdAsync(projectId))
                             .ReturnsAsync(existingProject);
        _mockGenericRepository.Setup(r => r.DeleteAsync(projectId))
                             .Returns(Task.CompletedTask);
        _mockUnitOfWork.Setup(u => u.CommitAsync())
                      .Returns(Task.CompletedTask);

        // Act
        var result = await _projectService.DeleteProjectAsync(projectId);

        // Assert
        result.Should().BeTrue();

        // Verify methods were called
        _mockGenericRepository.Verify(r => r.GetByIdAsync(projectId), Times.Once);
        _mockGenericRepository.Verify(r => r.DeleteAsync(projectId), Times.Once);
        _mockUnitOfWork.Verify(u => u.CommitAsync(), Times.Once);
    }

    [Fact]
    public async Task DeleteProjectAsync_WithNonExistentProject_ReturnsFalse()
    {
        // Arrange
        var projectId = 999;
        _mockGenericRepository.Setup(r => r.GetByIdAsync(projectId))
                             .ReturnsAsync((Project?)null);

        // Act
        var result = await _projectService.DeleteProjectAsync(projectId);

        // Assert
        result.Should().BeFalse();

        // Verify delete and commit were not called
        _mockGenericRepository.Verify(r => r.DeleteAsync(It.IsAny<int>()), Times.Never);
        _mockUnitOfWork.Verify(u => u.CommitAsync(), Times.Never);
    }

    [Fact]
    public async Task ProjectExistsAsync_WithExistingProject_ReturnsTrue()
    {
        // Arrange
        var projectId = 1;
        var existingProject = new Project { Id = projectId, Title = "Test Project" };

        _mockGenericRepository.Setup(r => r.GetByIdAsync(projectId))
                             .ReturnsAsync(existingProject);

        // Act
        var result = await _projectService.ProjectExistsAsync(projectId);

        // Assert
        result.Should().BeTrue();
    }

    [Fact]
    public async Task ProjectExistsAsync_WithNonExistentProject_ReturnsFalse()
    {
        // Arrange
        var projectId = 999;
        _mockGenericRepository.Setup(r => r.GetByIdAsync(projectId))
                             .ReturnsAsync((Project?)null);

        // Act
        var result = await _projectService.ProjectExistsAsync(projectId);

        // Assert
        result.Should().BeFalse();
    }

    [Fact]
    public async Task UserOwnsProjectAsync_WithOwnerUser_ReturnsTrue()
    {
        // Arrange
        var userId = "user123";
        var projectId = 1;
        var project = new Project
        {
            Id = projectId,
            Portfolio = new Portfolio { UserId = userId }
        };

        _mockProjectRepository.Setup(r => r.GetProjectWithPortfolioAsync(projectId))
                             .ReturnsAsync(project);

        // Act
        var result = await _projectService.UserOwnsProjectAsync(userId, projectId);

        // Assert
        result.Should().BeTrue();
    }

    [Fact]
    public async Task UserOwnsProjectAsync_WithNonOwnerUser_ReturnsFalse()
    {
        // Arrange
        var userId = "user123";
        var projectId = 1;
        var project = new Project
        {
            Id = projectId,
            Portfolio = new Portfolio { UserId = "different-user" }
        };

        _mockProjectRepository.Setup(r => r.GetProjectWithPortfolioAsync(projectId))
                             .ReturnsAsync(project);

        // Act
        var result = await _projectService.UserOwnsProjectAsync(userId, projectId);

        // Assert
        result.Should().BeFalse();
    }

    [Fact]
    public async Task UserOwnsProjectAsync_WithNonExistentProject_ReturnsFalse()
    {
        // Arrange
        var userId = "user123";
        var projectId = 999;

        _mockProjectRepository.Setup(r => r.GetProjectWithPortfolioAsync(projectId))
                             .ReturnsAsync((Project?)null);

        // Act
        var result = await _projectService.UserOwnsProjectAsync(userId, projectId);

        // Assert
        result.Should().BeFalse();
    }

    [Fact]
    public async Task UserOwnsProjectAsync_WithProjectWithoutPortfolio_ReturnsFalse()
    {
        // Arrange
        var userId = "user123";
        var projectId = 1;
        var project = new Project
        {
            Id = projectId,
            Portfolio = null
        };

        _mockProjectRepository.Setup(r => r.GetProjectWithPortfolioAsync(projectId))
                             .ReturnsAsync(project);

        // Act
        var result = await _projectService.UserOwnsProjectAsync(userId, projectId);

        // Assert
        result.Should().BeFalse();
    }
}
