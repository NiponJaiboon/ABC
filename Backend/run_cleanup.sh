#!/bin/bash

# Supabase Database Cleanup Script
echo "🧹 Running database cleanup on Supabase..."

# Export PostgreSQL path
export PATH="/opt/homebrew/opt/postgresql@14/bin:$PATH"

# Connection details
DB_HOST="aws-0-ap-southeast-1.pooler.supabase.com"
DB_PORT="5432"
DB_NAME="postgres"
DB_USER="postgres.itrqgmrnicfrxllxwryv"
DB_PASS="ABCP@ssw0rd!"

# Set PGPASSWORD environment variable to avoid password prompt
export PGPASSWORD="$DB_PASS"

echo "⚠️  WARNING: This will delete invalid portfolio records!"
echo "🔄 Running cleanup SQL script..."

psql -h "$DB_HOST" -p "$DB_PORT" -d "$DB_NAME" -U "$DB_USER" -f cleanup_portfolios.sql

echo ""
echo "✅ Cleanup completed!"
echo "🔄 You can now run EF Core migrations safely"
