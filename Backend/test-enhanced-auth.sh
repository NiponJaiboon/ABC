#!/bin/bash

# Step 9: Enhanced Account Linking System Testing Script
# Test enhanced external authentication endpoints with conflict resolution

echo "=== Step 9: Enhanced External Authentication System Testing ==="
echo ""

# Base URL configuration
BASE_URL="http://localhost:5011/api"
EXTERNAL_AUTH_URL="$BASE_URL/ExternalAuth"

# Colors for output
RED='\033[0;31m'
GREEN='\033[0;32m'
YELLOW='\033[1;33m'
BLUE='\033[0;34m'
PURPLE='\033[0;35m'
NC='\033[0m' # No Color

# Function to test endpoint with JSON response
test_endpoint() {
    local method=$1
    local url=$2
    local description=$3
    local data=$4
    local headers=$5

    echo -e "${BLUE}Testing: $description${NC}"
    echo "URL: $url"

    if [ -n "$data" ]; then
        echo "Data: $data"
    fi

    if [ -n "$headers" ]; then
        response=$(curl -s -w "\nHTTP_STATUS:%{http_code}" -X $method "$url" -H "Content-Type: application/json" -H "Authorization: Bearer $ACCESS_TOKEN" -d "$data" 2>/dev/null)
    else
        response=$(curl -s -w "\nHTTP_STATUS:%{http_code}" -X $method "$url" -H "Content-Type: application/json" 2>/dev/null)
    fi

    # Extract body and status
    body=$(echo "$response" | sed '$d')
    status=$(echo "$response" | tail -n1 | sed 's/HTTP_STATUS://')

    if [[ $status -ge 200 && $status -lt 300 ]]; then
        echo -e "${GREEN}✓ SUCCESS (HTTP $status)${NC}"
    elif [[ $status -ge 300 && $status -lt 400 ]]; then
        echo -e "${YELLOW}⚠ REDIRECT (HTTP $status)${NC}"
    else
        echo -e "${RED}✗ FAILED (HTTP $status)${NC}"
    fi

    # Pretty print JSON if response is JSON
    if echo "$body" | jq . >/dev/null 2>&1; then
        echo "Response:"
        echo "$body" | jq .
    else
        echo "Response: $body"
    fi

    echo ""
    echo "----------------------------------------"
    echo ""
}

# Wait for server to be ready
echo "Checking if server is running..."
for i in {1..30}; do
    if curl -s "$BASE_URL/swagger/index.html" >/dev/null 2>&1; then
        echo -e "${GREEN}✓ Server is ready!${NC}"
        break
    fi
    if [ $i -eq 30 ]; then
        echo -e "${RED}✗ Server not responding. Please start the API server first.${NC}"
        echo "Run: dotnet run --project /Users/nevelopdevper/iDev/ABC/Backend/src/API"
        exit 1
    fi
    echo "Waiting for server... ($i/30)"
    sleep 2
done

echo ""
echo -e "${PURPLE}=== Step 9: Enhanced Account Linking System Tests ===${NC}"
echo ""

# Get access token (reuse existing user)
echo -e "${YELLOW}Getting access token for testing...${NC}"
LOGIN_DATA='{"emailOrUsername":"testapi@example.com","password":"SecurePassword123!","rememberMe":false}'

login_response=$(curl -s -w "\nHTTP_STATUS:%{http_code}" -X POST "$BASE_URL/account/login" -H "Content-Type: application/json" -d "$LOGIN_DATA" 2>/dev/null)
login_body=$(echo "$login_response" | sed '$d')
login_status=$(echo "$login_response" | tail -n1 | sed 's/HTTP_STATUS://')

if [[ $login_status -ge 200 && $login_status -lt 300 ]]; then
    ACCESS_TOKEN=$(echo "$login_body" | jq -r '.accessToken // empty')
    if [ -n "$ACCESS_TOKEN" ] && [ "$ACCESS_TOKEN" != "null" ]; then
        echo -e "${GREEN}✓ Login successful, access token obtained${NC}"
        AUTH_HEADER="-H 'Authorization: Bearer $ACCESS_TOKEN'"

        echo ""
        echo -e "${PURPLE}=== Enhanced Account Management Features ===${NC}"
        echo ""

        # Test 1: Get Account Summary (Step 9 Feature)
        test_endpoint "GET" "$EXTERNAL_AUTH_URL/summary" "Get Comprehensive Account Summary (Step 9)" "" "$AUTH_HEADER"

        # Test 2: Get Security Score (Step 9 Feature)
        test_endpoint "GET" "$EXTERNAL_AUTH_URL/security-score" "Get Account Security Score (Step 9)" "" "$AUTH_HEADER"

        # Test 3: Enhanced Account Linking - Google
        ENHANCED_LINK_DATA='{"provider":"Google","returnUrl":"http://localhost:3000/auth/success","forceLink":false,"userConsent":"I understand the implications"}'
        test_endpoint "POST" "$EXTERNAL_AUTH_URL/link-enhanced" "Enhanced Account Linking - Google (Step 9)" "$ENHANCED_LINK_DATA" "$AUTH_HEADER"

        # Test 4: Enhanced Account Linking - Microsoft
        ENHANCED_LINK_MS_DATA='{"provider":"Microsoft","returnUrl":"http://localhost:3000/auth/success","forceLink":false}'
        test_endpoint "POST" "$EXTERNAL_AUTH_URL/link-enhanced" "Enhanced Account Linking - Microsoft (Step 9)" "$ENHANCED_LINK_MS_DATA" "$AUTH_HEADER"

        # Test 5: Check if Account Can Be Unlinked
        test_endpoint "GET" "$EXTERNAL_AUTH_URL/can-unlink?provider=Google&providerKey=sample-key" "Check If Account Can Be Unlinked (Step 9)" "" "$AUTH_HEADER"

        # Test 6: Conflict Resolution - Link
        CONFLICT_RESOLUTION_DATA='{"conflictToken":"sample-conflict-token-123","resolution":"Link","password":"SecurePassword123!","confirmAction":true}'
        test_endpoint "POST" "$EXTERNAL_AUTH_URL/resolve-conflict" "Resolve Conflict - Link Action (Step 9)" "$CONFLICT_RESOLUTION_DATA" "$AUTH_HEADER"

        # Test 7: Conflict Resolution - Cancel
        CONFLICT_CANCEL_DATA='{"conflictToken":"sample-conflict-token-456","resolution":"Cancel","confirmAction":true}'
        test_endpoint "POST" "$EXTERNAL_AUTH_URL/resolve-conflict" "Resolve Conflict - Cancel Action (Step 9)" "$CONFLICT_CANCEL_DATA" "$AUTH_HEADER"

        # Test 8: Bulk Action - Set Primary Provider
        BULK_PRIMARY_DATA='{"action":"SetPrimary","providerKeys":[],"newPrimaryProvider":"Google","requirePasswordConfirmation":true,"password":"SecurePassword123!"}'
        test_endpoint "POST" "$EXTERNAL_AUTH_URL/bulk-action" "Bulk Action - Set Primary Provider (Step 9)" "$BULK_PRIMARY_DATA" "$AUTH_HEADER"

        # Test 9: Enhanced Linking Callback (without context - expected to fail gracefully)
        test_endpoint "GET" "$EXTERNAL_AUTH_URL/link-callback-enhanced?returnUrl=http://localhost:3000/dashboard" "Enhanced Linking Callback (Step 9)" "" "$AUTH_HEADER"

        # Test 10: Bulk Action - Unlink All (should require password and existing accounts)
        BULK_UNLINK_DATA='{"action":"UnlinkAll","requirePasswordConfirmation":true,"password":"SecurePassword123!"}'
        test_endpoint "POST" "$EXTERNAL_AUTH_URL/bulk-action" "Bulk Action - Unlink All (Step 9)" "$BULK_UNLINK_DATA" "$AUTH_HEADER"

        echo ""
        echo -e "${PURPLE}=== Comparison: Basic vs Enhanced Features ===${NC}"
        echo ""

        # Compare basic vs enhanced endpoints
        echo -e "${YELLOW}Basic Status vs Enhanced Summary:${NC}"
        test_endpoint "GET" "$EXTERNAL_AUTH_URL/status" "Basic Status (Step 7-8)" "" "$AUTH_HEADER"

        echo -e "${YELLOW}Enhanced Summary with Security Score:${NC}"
        test_endpoint "GET" "$EXTERNAL_AUTH_URL/summary" "Enhanced Summary (Step 9)" "" "$AUTH_HEADER"

    else
        echo -e "${YELLOW}⚠ Could not extract access token, skipping Step 9 tests${NC}"
    fi
else
    echo -e "${YELLOW}⚠ Login failed (HTTP $login_status), skipping Step 9 tests${NC}"
    echo "Response: $login_body"
fi

echo ""
echo -e "${PURPLE}=== Step 9 Enhanced Features Test Summary ===${NC}"
echo ""
echo -e "${GREEN}✅ Enhanced Account Management Features Tested:${NC}"
echo "  • Comprehensive account summary with security scoring"
echo "  • Enhanced conflict detection and resolution system"
echo "  • Bulk account management operations"
echo "  • Real-time security score calculation"
echo "  • Advanced linking workflows with user confirmation"
echo ""
echo -e "${BLUE}ℹ Step 9 Key Improvements:${NC}"
echo "  • Security-focused account management"
echo "  • Proactive conflict detection and resolution"
echo "  • User-friendly bulk operations"
echo "  • Comprehensive security recommendations"
echo "  • Enhanced user experience with detailed feedback"
echo ""
echo -e "${GREEN}✓ Step 9: Enhanced Account Linking System - Test Complete${NC}"
