-- Cleanup script for Portfolios table
-- Remove portfolios with empty or invalid UserId

BEGIN;

-- Step 1: Show records that will be deleted
SELECT 'Records to be deleted:' as action;
SELECT "Id", "Title", "UserId", "CreatedAt"
FROM "Portfolios" 
WHERE "UserId" IS NULL 
   OR "UserId" = '' 
   OR "UserId" NOT IN (SELECT "Id" FROM "Users" WHERE "Id" IS NOT NULL);

-- Step 2: Delete invalid records
DELETE FROM "Portfolios" 
WHERE "UserId" IS NULL 
   OR "UserId" = '' 
   OR "UserId" NOT IN (SELECT "Id" FROM "Users" WHERE "Id" IS NOT NULL);

-- Step 3: Show remaining records
SELECT 'Remaining records after cleanup:' as action;
SELECT "Id", "Title", "UserId", "CreatedAt"
FROM "Portfolios" 
ORDER BY "Id";

-- Step 4: Show summary
SELECT 
    COUNT(*) as "RemainingPortfolios",
    COUNT(CASE WHEN "UserId" IS NULL OR "UserId" = '' THEN 1 END) as "PortfoliosWithEmptyUserId"
FROM "Portfolios";

COMMIT;
