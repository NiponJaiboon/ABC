#!/bin/bash

echo "🗄️ Cleaning up invalid portfolio data before migration..."

# สร้าง script SQL เพื่อลบข้อมูลที่มีปัญหา
cat > cleanup_portfolios.sql << 'EOF'
-- ลบ portfolios ที่มี UserId เป็น null หรือ empty
DELETE FROM "Portfolios" WHERE "UserId" IS NULL OR "UserId" = '';

-- ลบ portfolios ที่ UserId ไม่มีอยู่ใน Users table
DELETE FROM "Portfolios" 
WHERE "UserId" NOT IN (SELECT "Id" FROM "Users");

-- แสดงข้อมูลที่เหลือ
SELECT COUNT(*) as "RemainingPortfolios" FROM "Portfolios";
SELECT COUNT(*) as "TotalUsers" FROM "Users";
EOF

echo "SQL cleanup script created: cleanup_portfolios.sql"
echo ""
echo "📋 Script content:"
cat cleanup_portfolios.sql
