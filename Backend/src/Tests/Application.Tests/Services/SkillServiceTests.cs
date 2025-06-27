using Application.Services;
using Core.Entities;
using Core.Interfaces;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;

namespace Application.Tests.Services;

public class SkillServiceTests
{
    private readonly Mock<IUnitOfWork> _mockUnitOfWork;
    private readonly Mock<IGenericRepository<Skill>> _mockSkillRepository;
    private readonly Mock<ILogger<SkillService>> _mockLogger;
    private readonly SkillService _skillService;

    public SkillServiceTests()
    {
        _mockUnitOfWork = new Mock<IUnitOfWork>();
        _mockSkillRepository = new Mock<IGenericRepository<Skill>>();
        _mockLogger = new Mock<ILogger<SkillService>>();
        _skillService = new SkillService(_mockUnitOfWork.Object, _mockSkillRepository.Object, _mockLogger.Object);
    }

    [Fact]
    public async Task GetAllSkillsAsync_ShouldReturnAllSkills()
    {
        // Arrange
        var skills = new List<Skill>
        {
            new Skill { Id = 1, Name = "C#", Category = "Programming" },
            new Skill { Id = 2, Name = "JavaScript", Category = "Programming" }
        };
        _mockSkillRepository.Setup(r => r.GetAllAsync()).ReturnsAsync(skills);

        // Act
        var result = await _skillService.GetAllSkillsAsync();

        // Assert
        Assert.NotNull(result);
        Assert.Equal(2, result.Count());
        _mockSkillRepository.Verify(r => r.GetAllAsync(), Times.Once);
    }

    [Fact]
    public async Task GetSkillsByCategoryAsync_ShouldReturnSkillsByCategory()
    {
        // Arrange
        var skills = new List<Skill>
        {
            new Skill { Id = 1, Name = "C#", Category = "Programming" },
            new Skill { Id = 2, Name = "JavaScript", Category = "Programming" },
            new Skill { Id = 3, Name = "Photoshop", Category = "Design" }
        };
        _mockSkillRepository.Setup(r => r.GetAllAsync()).ReturnsAsync(skills);

        // Act
        var result = await _skillService.GetSkillsByCategoryAsync("Programming");

        // Assert
        Assert.NotNull(result);
        Assert.Equal(2, result.Count());
        Assert.All(result, skill => Assert.Equal("Programming", skill.Category));
    }

    [Fact]
    public async Task GetSkillByIdAsync_ShouldReturnSkill_WhenSkillExists()
    {
        // Arrange
        var skill = new Skill { Id = 1, Name = "C#", Category = "Programming" };
        _mockSkillRepository.Setup(r => r.GetByIdAsync(1)).ReturnsAsync(skill);

        // Act
        var result = await _skillService.GetSkillByIdAsync(1);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(1, result.Id);
        Assert.Equal("C#", result.Name);
    }

    [Fact]
    public async Task GetSkillByIdAsync_ShouldReturnNull_WhenSkillDoesNotExist()
    {
        // Arrange
        _mockSkillRepository.Setup(r => r.GetByIdAsync(999)).ReturnsAsync((Skill)null);

        // Act
        var result = await _skillService.GetSkillByIdAsync(999);

        // Assert
        Assert.Null(result);
    }

    [Fact]
    public async Task CreateSkillAsync_ShouldCreateSkill_WhenValidDataProvided()
    {
        // Arrange
        var skill = new Skill
        {
            Name = "Python",
            Category = "Programming",
            Description = "Python programming language"
        };

        _mockSkillRepository.Setup(r => r.AddAsync(It.IsAny<Skill>())).Returns(Task.CompletedTask);
        _mockUnitOfWork.Setup(u => u.CommitAsync()).Returns(Task.CompletedTask);

        // Act
        var result = await _skillService.CreateSkillAsync(skill);

        // Assert
        Assert.NotNull(result);
        Assert.Equal("Python", result.Name);
        Assert.Equal("Programming", result.Category);
        Assert.True(result.CreatedAt > DateTime.MinValue);
        
        _mockSkillRepository.Verify(r => r.AddAsync(It.IsAny<Skill>()), Times.Once);
        _mockUnitOfWork.Verify(u => u.CommitAsync(), Times.Once);
    }

    [Fact]
    public async Task CreateSkillAsync_ShouldThrowException_WhenNameIsEmpty()
    {
        // Arrange
        var skill = new Skill
        {
            Name = "",
            Category = "Programming"
        };

        // Act & Assert
        var exception = await Assert.ThrowsAsync<ArgumentException>(() => _skillService.CreateSkillAsync(skill));
        Assert.Equal("Skill name is required", exception.Message);
    }

    [Fact]
    public async Task CreateSkillAsync_ShouldThrowException_WhenCategoryIsEmpty()
    {
        // Arrange
        var skill = new Skill
        {
            Name = "Python",
            Category = ""
        };

        // Act & Assert
        var exception = await Assert.ThrowsAsync<ArgumentException>(() => _skillService.CreateSkillAsync(skill));
        Assert.Equal("Skill category is required", exception.Message);
    }

    [Fact]
    public async Task UpdateSkillAsync_ShouldUpdateSkill_WhenSkillExists()
    {
        // Arrange
        var skill = new Skill
        {
            Id = 1,
            Name = "Updated Python",
            Category = "Programming"
        };

        _mockSkillRepository.Setup(r => r.GetByIdAsync(1)).ReturnsAsync(new Skill { Id = 1 });
        _mockSkillRepository.Setup(r => r.UpdateAsync(It.IsAny<Skill>())).Returns(Task.CompletedTask);
        _mockUnitOfWork.Setup(u => u.CommitAsync()).Returns(Task.CompletedTask);

        // Act
        var result = await _skillService.UpdateSkillAsync(skill);

        // Assert
        Assert.NotNull(result);
        Assert.Equal("Updated Python", result.Name);
        
        _mockSkillRepository.Verify(r => r.UpdateAsync(It.IsAny<Skill>()), Times.Once);
        _mockUnitOfWork.Verify(u => u.CommitAsync(), Times.Once);
    }

    [Fact]
    public async Task UpdateSkillAsync_ShouldThrowException_WhenSkillDoesNotExist()
    {
        // Arrange
        var skill = new Skill { Id = 999, Name = "Non-existent", Category = "Programming" };
        _mockSkillRepository.Setup(r => r.GetByIdAsync(999)).ReturnsAsync((Skill)null);

        // Act & Assert
        var exception = await Assert.ThrowsAsync<ArgumentException>(() => _skillService.UpdateSkillAsync(skill));
        Assert.Equal("Skill with ID 999 not found", exception.Message);
    }

    [Fact]
    public async Task DeleteSkillAsync_ShouldDeleteSkill_WhenSkillExists()
    {
        // Arrange
        _mockSkillRepository.Setup(r => r.GetByIdAsync(1)).ReturnsAsync(new Skill { Id = 1 });
        _mockSkillRepository.Setup(r => r.DeleteAsync(1)).Returns(Task.CompletedTask);
        _mockUnitOfWork.Setup(u => u.CommitAsync()).Returns(Task.CompletedTask);

        // Act
        var result = await _skillService.DeleteSkillAsync(1);

        // Assert
        Assert.True(result);
        _mockSkillRepository.Verify(r => r.DeleteAsync(1), Times.Once);
        _mockUnitOfWork.Verify(u => u.CommitAsync(), Times.Once);
    }

    [Fact]
    public async Task DeleteSkillAsync_ShouldReturnFalse_WhenSkillDoesNotExist()
    {
        // Arrange
        _mockSkillRepository.Setup(r => r.GetByIdAsync(999)).ReturnsAsync((Skill)null);

        // Act
        var result = await _skillService.DeleteSkillAsync(999);

        // Assert
        Assert.False(result);
        _mockSkillRepository.Verify(r => r.DeleteAsync(It.IsAny<int>()), Times.Never);
        _mockUnitOfWork.Verify(u => u.CommitAsync(), Times.Never);
    }

    [Fact]
    public async Task SkillExistsAsync_ShouldReturnTrue_WhenSkillExists()
    {
        // Arrange
        _mockSkillRepository.Setup(r => r.GetByIdAsync(1)).ReturnsAsync(new Skill { Id = 1 });

        // Act
        var result = await _skillService.SkillExistsAsync(1);

        // Assert
        Assert.True(result);
    }

    [Fact]
    public async Task SkillExistsAsync_ShouldReturnFalse_WhenSkillDoesNotExist()
    {
        // Arrange
        _mockSkillRepository.Setup(r => r.GetByIdAsync(999)).ReturnsAsync((Skill)null);

        // Act
        var result = await _skillService.SkillExistsAsync(999);

        // Assert
        Assert.False(result);
    }

    [Fact]
    public async Task GetSkillCategoriesAsync_ShouldReturnDistinctCategories()
    {
        // Arrange
        var skills = new List<Skill>
        {
            new Skill { Id = 1, Name = "C#", Category = "Programming" },
            new Skill { Id = 2, Name = "JavaScript", Category = "Programming" },
            new Skill { Id = 3, Name = "Photoshop", Category = "Design" },
            new Skill { Id = 4, Name = "Illustrator", Category = "Design" }
        };
        _mockSkillRepository.Setup(r => r.GetAllAsync()).ReturnsAsync(skills);

        // Act
        var result = await _skillService.GetSkillCategoriesAsync();

        // Assert
        Assert.NotNull(result);
        Assert.Equal(2, result.Count());
        Assert.Contains("Programming", result);
        Assert.Contains("Design", result);
    }
}
