using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Core.Entities;
using Core.Interfaces;
using Dapper;
using Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Repositories
{
    public class PortfolioRepository : IPortfolioRepository
    {
        private readonly ApplicationDbContext _applicationDbContext;
        public PortfolioRepository(ApplicationDbContext applicationDbContext)
        {
            _applicationDbContext = applicationDbContext;

        }
        public async Task<IEnumerable<Portfolio>> GetPortfolioByUserAsync(string userId)
        {
            using (var connection = _applicationDbContext.Database.GetDbConnection())
            {
                var sql = @"
                SELECT Id, UserId, Title, Description, CreatedAt, UpdatedAt
                FROM Portfolios
                WHERE UserId = @UserId";

                var portfolios = await connection.QueryAsync<Portfolio>(sql, new { UserId = userId });
                return portfolios;
            }
        }

        public async Task<Portfolio?> GetPortfolioWithDetailsAsync(int portfolioId)
        {
            using (var connection = _applicationDbContext.Database.GetDbConnection())
            {
                var sql = @"
                SELECT Id, UserId, Title, Description, CreatedAt, UpdatedAt
                FROM Portfolios
                WHERE Id = @Id";
                var portfolio = await connection.QueryFirstOrDefaultAsync<Portfolio>(
                    sql,
                    new { Id = portfolioId }
                );
                return portfolio;
            }
        }


        public async Task<Portfolio?> GetPortfolioWithProjectsAsync(int id)
        {
            // เปิดการเชื่อมต่อฐานข้อมูลจาก Entity Framework context
            using (var connection = _applicationDbContext.Database.GetDbConnection())
            {
                // ดึงข้อมูล Portfolio ตาม ID ที่ระบุ
                var portfolioSql = @"
                SELECT Id, UserId, Title, Description, CreatedAt, UpdatedAt
                FROM Portfolios
                WHERE Id = @Id";
                var portfolio = await connection.QueryFirstOrDefaultAsync<Portfolio>(portfolioSql, new { Id = id });

                // ตรวจสอบว่าพบ Portfolio หรือไม่ ถ้าไม่พบให้คืนค่า null
                if (portfolio == null)
                    return null;

                // ดึงข้อมูล Projects ทั้งหมดที่เชื่อมโยงกับ Portfolio นี้
                var projectsSql = @"
                SELECT Id, PortfolioId, Title, Description, CreatedAt, UpdatedAt
                FROM Projects
                WHERE PortfolioId = @PortfolioId";
                var projects = await connection.QueryAsync<Project>(projectsSql, new { PortfolioId = id });

                // กำหนด Projects ให้กับ Portfolio object
                portfolio.Projects = projects.ToList();

                // วนลูปผ่าน Projects แต่ละตัวเพื่อดึงข้อมูล Skills ที่เกี่ยวข้อง
                foreach (var project in portfolio.Projects)
                {
                    // SQL query สำหรับดึงข้อมูล ProjectSkills และ Skills ที่เชื่อมโยง
                    var skillsSql = @"
                    SELECT ps.Id, ps.ProjectId, ps.SkillId, s.Id, s.Name, s.Category
                    FROM ProjectSkills ps
                    INNER JOIN Skills s ON ps.SkillId = s.Id
                    WHERE ps.ProjectId = @ProjectId";

                    // ดึงข้อมูลและ map ความสัมพันธ์ระหว่าง ProjectSkill และ Skill
                    var projectSkills = await connection.QueryAsync<ProjectSkill, Skill, ProjectSkill>(
                    skillsSql,
                    (projectSkill, skill) =>
                    {
                        // กำหนด Skill object ให้กับ ProjectSkill
                        projectSkill.Skill = skill;
                        return projectSkill;
                    },
                    new { ProjectId = project.Id },
                    splitOn: "Id" // แยกข้อมูลโดยใช้ Id column เป็นจุดแบ่ง
                    );

                    // กำหนด ProjectSkills ให้กับ Project object
                    project.ProjectSkills = projectSkills.ToList();
                }

                // คืนค่า Portfolio object ที่มีข้อมูลครบถ้วน
                return portfolio;
            }
        }
    }
}
