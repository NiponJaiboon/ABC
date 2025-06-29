#!/bin/bash

# Supabase Database Analysis Script
echo "🔍 Running database analysis on Supabase..."

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

echo "📋 Running SQL analysis..."
psql -h "$DB_HOST" -p "$DB_PORT" -d "$DB_NAME" -U "$DB_USER" -f analyze_data.sql

echo ""
echo "✅ Analysis completed!"
echo "📝 Review the results above to understand data issues before cleanup"
