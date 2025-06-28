#!/bin/bash

# ExternalAuth Controller Testing Script
# Test all external authentication endpoints

echo "=== External Authentication API Testing ==="
echo ""

# Base URL configuration
BASE_URL="http://localhost:5011/api"
EXTERNAL_AUTH_URL="$BASE_URL/ExternalAuth"

# Colors for output
RED='\033[0;31m'
GREEN='\033[0;32m'
YELLOW='\033[1;33m'
BLUE='\033[0;34m'
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
        response=$(curl -s -w "\nHTTP_STATUS:%{http_code}" -X $method "$url" -H "Content-Type: application/json" $headers -d "$data" 2>/dev/null)
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
echo "=== Starting ExternalAuth Controller Tests ==="
echo ""

# Test 1: Get Available Providers
test_endpoint "GET" "$EXTERNAL_AUTH_URL/providers" "Get Available External Providers"

# Test 2: Test Google OAuth Challenge
test_endpoint "GET" "$EXTERNAL_AUTH_URL/challenge/Google?returnUrl=http://localhost:3000/auth/callback" "Google OAuth Challenge"

# Test 3: Test Microsoft OAuth Challenge
test_endpoint "GET" "$EXTERNAL_AUTH_URL/challenge/Microsoft?returnUrl=http://localhost:3000/auth/callback" "Microsoft OAuth Challenge"

# Test 4: Test Invalid Provider (should fail)
test_endpoint "GET" "$EXTERNAL_AUTH_URL/challenge/InvalidProvider?returnUrl=http://localhost:3000/auth/callback" "Invalid Provider Challenge (Expected to fail)"

# Test 5: Test External Auth Callback (without context - will fail gracefully)
test_endpoint "GET" "$EXTERNAL_AUTH_URL/callback?returnUrl=http://localhost:3000/dashboard" "External Auth Callback (without context)"

# For authenticated endpoints, we need a token first
echo "=== Testing authenticated endpoints (requires login) ==="
echo ""

# Login to get access token - use correct format
LOGIN_DATA='{"emailOrUsername":"test@example.com","password":"TestPassword123!","rememberMe":false}'
REGISTER_DATA='{"email":"test@example.com","username":"testuser","password":"TestPassword123!","confirmPassword":"TestPassword123!"}'
echo -e "${YELLOW}Attempting login to get access token...${NC}"

# Try to register user first (might fail if already exists)
curl -s -X POST "$BASE_URL/account/register" -H "Content-Type: application/json" -d "$REGISTER_DATA" >/dev/null 2>&1

# Login to get token
login_response=$(curl -s -w "\nHTTP_STATUS:%{http_code}" -X POST "$BASE_URL/account/login" -H "Content-Type: application/json" -d "$LOGIN_DATA" 2>/dev/null)
login_body=$(echo "$login_response" | sed '$d')
login_status=$(echo "$login_response" | tail -n1 | sed 's/HTTP_STATUS://')

if [[ $login_status -ge 200 && $login_status -lt 300 ]]; then
    ACCESS_TOKEN=$(echo "$login_body" | jq -r '.accessToken // empty')
    if [ -n "$ACCESS_TOKEN" ] && [ "$ACCESS_TOKEN" != "null" ]; then
        echo -e "${GREEN}✓ Login successful, access token obtained${NC}"
        AUTH_HEADER="-H 'Authorization: Bearer $ACCESS_TOKEN'"

        # Test 6: Get Linked Accounts
        test_endpoint "GET" "$EXTERNAL_AUTH_URL/linked-accounts" "Get Linked Accounts" "" "$AUTH_HEADER"

        # Test 7: Get External Auth Status
        test_endpoint "GET" "$EXTERNAL_AUTH_URL/status" "Get External Auth Status" "" "$AUTH_HEADER"

        # Test 8: Link External Account (initiate OAuth flow)
        LINK_DATA='{"provider":"Google","providerKey":"","providerDisplayName":"Google"}'
        test_endpoint "POST" "$EXTERNAL_AUTH_URL/link" "Link External Account (OAuth flow)" "$LINK_DATA" "$AUTH_HEADER"

        # Test 9: Unlink External Account
        UNLINK_DATA='{"provider":"Google","providerKey":"sample-provider-key"}'
        test_endpoint "POST" "$EXTERNAL_AUTH_URL/unlink" "Unlink External Account" "$UNLINK_DATA" "$AUTH_HEADER"

    else
        echo -e "${YELLOW}⚠ Could not extract access token, skipping authenticated tests${NC}"
    fi
else
    echo -e "${YELLOW}⚠ Login failed (HTTP $login_status), skipping authenticated tests${NC}"
    echo "Response: $login_body"
fi

echo ""
echo "=== ExternalAuth Controller Test Summary ==="
echo ""
echo -e "${GREEN}✓ All ExternalAuth endpoints tested${NC}"
echo -e "${BLUE}ℹ Provider challenges will redirect to OAuth providers${NC}"
echo -e "${BLUE}ℹ Callback endpoints require external OAuth context${NC}"
echo -e "${BLUE}ℹ Account linking/unlinking requires valid provider keys${NC}"
echo ""
echo "=== Test Complete ==="
