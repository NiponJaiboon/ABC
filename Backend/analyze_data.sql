-- ตรวจสอบข้อมูลใน Users table
SELECT 
    COUNT(*) as "TotalUsers",
    COUNT(CASE WHEN "Id" IS NULL OR "Id" = '' THEN 1 END) as "UsersWithEmptyId"
FROM "Users";

-- ตรวจสอบข้อมูลใน Portfolios table
SELECT 
    COUNT(*) as "TotalPortfolios",
    COUNT(CASE WHEN "UserId" IS NULL OR "UserId" = '' THEN 1 END) as "PortfoliosWithEmptyUserId"
FROM "Portfolios";

-- แสดง portfolios ที่มีปัญหา
SELECT "Id", "Title", "UserId" 
FROM "Portfolios" 
WHERE "UserId" IS NULL OR "UserId" = '' 
OR "UserId" NOT IN (SELECT "Id" FROM "Users")
LIMIT 10;
