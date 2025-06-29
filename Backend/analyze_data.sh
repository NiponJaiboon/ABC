#!/bin/bash

echo "🔍 Analyzing database data before cleanup..."

# สร้าง script SQL เพื่อวิเคราะห์ข้อมูล
cat > analyze_data.sql << 'EOF'
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
EOF

echo "SQL analysis script created: analyze_data.sql"
echo ""
echo "📋 Run this analysis first:"
echo "psql \"Host=aws-0-ap-southeast-1.pooler.supabase.com;Port=5432;Database=postgres;Username=postgres.itrqgmrnicfrxllxwryv;Password=ABCP@ssw0rd!;SSL Mode=Require\" -f analyze_data.sql"
