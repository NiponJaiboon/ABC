using Application.Services;
using Core.Entities;
using Core.Interfaces;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;

namespace Application.Tests.Services;

public class ProjectSkillServiceTests
{
    private readonly Mock<IUnitOfWork> _mockUnitOfWork;
    private readonly Mock<IGenericRepository<ProjectSkill>> _mockProjectSkillRepository;
    private readonly Mock<IProjectService> _mockProjectService;
    private readonly Mock<ISkillService> _mockSkillService;
    private readonly Mock<ILogger<ProjectSkillService>> _mockLogger;
    private readonly ProjectSkillService _projectSkillService;

    public ProjectSkillServiceTests()
    {
        _mockUnitOfWork = new Mock<IUnitOfWork>();
        _mockProjectSkillRepository = new Mock<IGenericRepository<ProjectSkill>>();
        _mockProjectService = new Mock<IProjectService>();
        _mockSkillService = new Mock<ISkillService>();
        _mockLogger = new Mock<ILogger<ProjectSkillService>>();
        _projectSkillService = new ProjectSkillService(
            _mockUnitOfWork.Object,
            _mockProjectSkillRepository.Object,
            _mockProjectService.Object,
            _mockSkillService.Object,
            _mockLogger.Object);
    }

    [Fact]
    public async Task GetProjectSkillsAsync_ShouldReturnProjectSkills()
    {
        // Arrange
        var projectSkills = new List<ProjectSkill>
        {
            new ProjectSkill { Id = 1, ProjectId = 1, SkillId = 1, ProficiencyLevel = 4, IsPrimary = true },
            new ProjectSkill { Id = 2, ProjectId = 1, SkillId = 2, ProficiencyLevel = 3, IsPrimary = false },
            new ProjectSkill { Id = 3, ProjectId = 2, SkillId = 1, ProficiencyLevel = 5, IsPrimary = true }
        };
        _mockProjectSkillRepository.Setup(r => r.GetAllAsync()).ReturnsAsync(projectSkills);

        // Act
        var result = await _projectSkillService.GetProjectSkillsAsync(1);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(2, result.Count());
        Assert.All(result, ps => Assert.Equal(1, ps.ProjectId));
    }

    [Fact]
    public async Task GetProjectSkillAsync_ShouldReturnProjectSkill_WhenExists()
    {
        // Arrange
        var projectSkills = new List<ProjectSkill>
        {
            new ProjectSkill { Id = 1, ProjectId = 1, SkillId = 1, ProficiencyLevel = 4, IsPrimary = true }
        };
        _mockProjectSkillRepository.Setup(r => r.GetAllAsync()).ReturnsAsync(projectSkills);

        // Act
        var result = await _projectSkillService.GetProjectSkillAsync(1, 1);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(1, result.ProjectId);
        Assert.Equal(1, result.SkillId);
        Assert.Equal(4, result.ProficiencyLevel);
    }

    [Fact]
    public async Task GetProjectSkillAsync_ShouldReturnNull_WhenNotExists()
    {
        // Arrange
        _mockProjectSkillRepository.Setup(r => r.GetAllAsync()).ReturnsAsync(new List<ProjectSkill>());

        // Act
        var result = await _projectSkillService.GetProjectSkillAsync(1, 999);

        // Assert
        Assert.Null(result);
    }

    [Fact]
    public async Task AddSkillToProjectAsync_ShouldAddSkill_WhenValidData()
    {
        // Arrange
        _mockProjectService.Setup(s => s.ProjectExistsAsync(1)).ReturnsAsync(true);
        _mockSkillService.Setup(s => s.SkillExistsAsync(1)).ReturnsAsync(true);
        _mockProjectSkillRepository.Setup(r => r.GetAllAsync()).ReturnsAsync(new List<ProjectSkill>());
        _mockProjectSkillRepository.Setup(r => r.AddAsync(It.IsAny<ProjectSkill>())).Returns(Task.CompletedTask);
        _mockUnitOfWork.Setup(u => u.CommitAsync()).Returns(Task.CompletedTask);

        // Act
        var result = await _projectSkillService.AddSkillToProjectAsync(1, 1, 4, true);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(1, result.ProjectId);
        Assert.Equal(1, result.SkillId);
        Assert.Equal(4, result.ProficiencyLevel);
        Assert.True(result.IsPrimary);
        
        _mockProjectSkillRepository.Verify(r => r.AddAsync(It.IsAny<ProjectSkill>()), Times.Once);
        _mockUnitOfWork.Verify(u => u.CommitAsync(), Times.Once);
    }

    [Fact]
    public async Task AddSkillToProjectAsync_ShouldThrowException_WhenProjectNotExists()
    {
        // Arrange
        _mockProjectService.Setup(s => s.ProjectExistsAsync(999)).ReturnsAsync(false);

        // Act & Assert
        var exception = await Assert.ThrowsAsync<ArgumentException>(() =>
            _projectSkillService.AddSkillToProjectAsync(999, 1, 4, true));
        Assert.Equal("Project with ID 999 not found", exception.Message);
    }

    [Fact]
    public async Task AddSkillToProjectAsync_ShouldThrowException_WhenSkillNotExists()
    {
        // Arrange
        _mockProjectService.Setup(s => s.ProjectExistsAsync(1)).ReturnsAsync(true);
        _mockSkillService.Setup(s => s.SkillExistsAsync(999)).ReturnsAsync(false);

        // Act & Assert
        var exception = await Assert.ThrowsAsync<ArgumentException>(() =>
            _projectSkillService.AddSkillToProjectAsync(1, 999, 4, true));
        Assert.Equal("Skill with ID 999 not found", exception.Message);
    }

    [Fact]
    public async Task AddSkillToProjectAsync_ShouldThrowException_WhenSkillAlreadyExists()
    {
        // Arrange
        var existingProjectSkills = new List<ProjectSkill>
        {
            new ProjectSkill { Id = 1, ProjectId = 1, SkillId = 1, ProficiencyLevel = 3 }
        };
        _mockProjectService.Setup(s => s.ProjectExistsAsync(1)).ReturnsAsync(true);
        _mockSkillService.Setup(s => s.SkillExistsAsync(1)).ReturnsAsync(true);
        _mockProjectSkillRepository.Setup(r => r.GetAllAsync()).ReturnsAsync(existingProjectSkills);

        // Act & Assert
        var exception = await Assert.ThrowsAsync<ArgumentException>(() =>
            _projectSkillService.AddSkillToProjectAsync(1, 1, 4, true));
        Assert.Equal("Skill 1 is already assigned to project 1", exception.Message);
    }

    [Theory]
    [InlineData(0)]
    [InlineData(6)]
    public async Task AddSkillToProjectAsync_ShouldThrowException_WhenInvalidProficiencyLevel(int proficiencyLevel)
    {
        // Arrange
        _mockProjectService.Setup(s => s.ProjectExistsAsync(1)).ReturnsAsync(true);
        _mockSkillService.Setup(s => s.SkillExistsAsync(1)).ReturnsAsync(true);
        _mockProjectSkillRepository.Setup(r => r.GetAllAsync()).ReturnsAsync(new List<ProjectSkill>());

        // Act & Assert
        var exception = await Assert.ThrowsAsync<ArgumentException>(() =>
            _projectSkillService.AddSkillToProjectAsync(1, 1, proficiencyLevel, false));
        Assert.Equal("Proficiency level must be between 1 and 5", exception.Message);
    }

    [Fact]
    public async Task UpdateProjectSkillAsync_ShouldUpdateSkill_WhenExists()
    {
        // Arrange
        var existingProjectSkill = new ProjectSkill
        {
            Id = 1,
            ProjectId = 1,
            SkillId = 1,
            ProficiencyLevel = 3,
            IsPrimary = false
        };
        var projectSkills = new List<ProjectSkill> { existingProjectSkill };
        _mockProjectSkillRepository.Setup(r => r.GetAllAsync()).ReturnsAsync(projectSkills);
        _mockProjectSkillRepository.Setup(r => r.UpdateAsync(It.IsAny<ProjectSkill>())).Returns(Task.CompletedTask);
        _mockUnitOfWork.Setup(u => u.CommitAsync()).Returns(Task.CompletedTask);

        // Act
        var result = await _projectSkillService.UpdateProjectSkillAsync(1, 1, 5, true);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(5, result.ProficiencyLevel);
        Assert.True(result.IsPrimary);
        
        _mockProjectSkillRepository.Verify(r => r.UpdateAsync(It.IsAny<ProjectSkill>()), Times.Once);
        _mockUnitOfWork.Verify(u => u.CommitAsync(), Times.Once);
    }

    [Fact]
    public async Task UpdateProjectSkillAsync_ShouldThrowException_WhenNotExists()
    {
        // Arrange
        _mockProjectSkillRepository.Setup(r => r.GetAllAsync()).ReturnsAsync(new List<ProjectSkill>());

        // Act & Assert
        var exception = await Assert.ThrowsAsync<ArgumentException>(() =>
            _projectSkillService.UpdateProjectSkillAsync(1, 999, 4, true));
        Assert.Equal("Skill 999 is not assigned to project 1", exception.Message);
    }

    [Theory]
    [InlineData(0)]
    [InlineData(6)]
    public async Task UpdateProjectSkillAsync_ShouldThrowException_WhenInvalidProficiencyLevel(int proficiencyLevel)
    {
        // Arrange
        var existingProjectSkill = new ProjectSkill { Id = 1, ProjectId = 1, SkillId = 1, ProficiencyLevel = 3 };
        var projectSkills = new List<ProjectSkill> { existingProjectSkill };
        _mockProjectSkillRepository.Setup(r => r.GetAllAsync()).ReturnsAsync(projectSkills);

        // Act & Assert
        var exception = await Assert.ThrowsAsync<ArgumentException>(() =>
            _projectSkillService.UpdateProjectSkillAsync(1, 1, proficiencyLevel, true));
        Assert.Equal("Proficiency level must be between 1 and 5", exception.Message);
    }

    [Fact]
    public async Task RemoveSkillFromProjectAsync_ShouldRemoveSkill_WhenExists()
    {
        // Arrange
        var existingProjectSkill = new ProjectSkill { Id = 1, ProjectId = 1, SkillId = 1 };
        var projectSkills = new List<ProjectSkill> { existingProjectSkill };
        _mockProjectSkillRepository.Setup(r => r.GetAllAsync()).ReturnsAsync(projectSkills);
        _mockProjectSkillRepository.Setup(r => r.DeleteAsync(1)).Returns(Task.CompletedTask);
        _mockUnitOfWork.Setup(u => u.CommitAsync()).Returns(Task.CompletedTask);

        // Act
        var result = await _projectSkillService.RemoveSkillFromProjectAsync(1, 1);

        // Assert
        Assert.True(result);
        _mockProjectSkillRepository.Verify(r => r.DeleteAsync(1), Times.Once);
        _mockUnitOfWork.Verify(u => u.CommitAsync(), Times.Once);
    }

    [Fact]
    public async Task RemoveSkillFromProjectAsync_ShouldReturnFalse_WhenNotExists()
    {
        // Arrange
        _mockProjectSkillRepository.Setup(r => r.GetAllAsync()).ReturnsAsync(new List<ProjectSkill>());

        // Act
        var result = await _projectSkillService.RemoveSkillFromProjectAsync(1, 999);

        // Assert
        Assert.False(result);
        _mockProjectSkillRepository.Verify(r => r.DeleteAsync(It.IsAny<int>()), Times.Never);
        _mockUnitOfWork.Verify(u => u.CommitAsync(), Times.Never);
    }

    [Fact]
    public async Task GetProjectsBySkillAsync_ShouldReturnProjects()
    {
        // Arrange
        var projectSkills = new List<ProjectSkill>
        {
            new ProjectSkill { Id = 1, ProjectId = 1, SkillId = 1 },
            new ProjectSkill { Id = 2, ProjectId = 2, SkillId = 1 }
        };
        var project1 = new Project { Id = 1, Title = "Project 1" };
        var project2 = new Project { Id = 2, Title = "Project 2" };
        
        _mockProjectSkillRepository.Setup(r => r.GetAllAsync()).ReturnsAsync(projectSkills);
        _mockProjectService.Setup(s => s.GetProjectByIdAsync(1)).ReturnsAsync(project1);
        _mockProjectService.Setup(s => s.GetProjectByIdAsync(2)).ReturnsAsync(project2);

        // Act
        var result = await _projectSkillService.GetProjectsBySkillAsync(1);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(2, result.Count());
    }

    [Fact]
    public async Task ProjectHasSkillAsync_ShouldReturnTrue_WhenSkillExists()
    {
        // Arrange
        var projectSkills = new List<ProjectSkill>
        {
            new ProjectSkill { Id = 1, ProjectId = 1, SkillId = 1 }
        };
        _mockProjectSkillRepository.Setup(r => r.GetAllAsync()).ReturnsAsync(projectSkills);

        // Act
        var result = await _projectSkillService.ProjectHasSkillAsync(1, 1);

        // Assert
        Assert.True(result);
    }

    [Fact]
    public async Task ProjectHasSkillAsync_ShouldReturnFalse_WhenSkillNotExists()
    {
        // Arrange
        _mockProjectSkillRepository.Setup(r => r.GetAllAsync()).ReturnsAsync(new List<ProjectSkill>());

        // Act
        var result = await _projectSkillService.ProjectHasSkillAsync(1, 999);

        // Assert
        Assert.False(result);
    }
}
