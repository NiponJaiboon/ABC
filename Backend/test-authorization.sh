#!/bin/bash

# Step 12: Authorization & Scopes Test Script
# Tests OAuth client management, scope validation, and permission management

echo "🔐 Step 12: Authorization & Scopes - Testing Script"
echo "=================================================="

# Configuration
BASE_URL="https://localhost:7146/api"
OAUTH_URL="$BASE_URL/oauthmanagement"

# Colors for output
RED='\033[0;31m'
GREEN='\033[0;32m'
YELLOW='\033[1;33m'
BLUE='\033[0;34m'
NC='\033[0m' # No Color

# Test counters
TOTAL_TESTS=0
PASSED_TESTS=0
FAILED_TESTS=0

# Function to log test results
log_test() {
    TOTAL_TESTS=$((TOTAL_TESTS + 1))
    if [ $1 -eq 0 ]; then
        echo -e "${GREEN}✅ PASS${NC}: $2"
        PASSED_TESTS=$((PASSED_TESTS + 1))
    else
        echo -e "${RED}❌ FAIL${NC}: $2"
        FAILED_TESTS=$((FAILED_TESTS + 1))
    fi
}

# Function to make HTTP requests with error handling
make_request() {
    local method=$1
    local url=$2
    local data=$3
    local token=$4
    local description=$5

    echo -e "\n${BLUE}🔄 Testing:${NC} $description"
    echo -e "${YELLOW}📤 Request:${NC} $method $url"

    if [ -n "$data" ]; then
        echo -e "${YELLOW}📄 Data:${NC} $data"
    fi

    # Prepare curl command
    local curl_cmd="curl -s -w '\n%{http_code}' -X $method '$url'"

    if [ -n "$token" ]; then
        curl_cmd="$curl_cmd -H 'Authorization: Bearer $token'"
    fi

    curl_cmd="$curl_cmd -H 'Content-Type: application/json'"

    if [ -n "$data" ]; then
        curl_cmd="$curl_cmd -d '$data'"
    fi

    # Execute request
    local response=$(eval $curl_cmd)
    local http_code=$(echo "$response" | tail -n1)
    local response_body=$(echo "$response" | sed '$d')

    echo -e "${YELLOW}📥 Response:${NC} HTTP $http_code"

    if [ -n "$response_body" ]; then
        echo "$response_body" | jq . 2>/dev/null || echo "$response_body"
    fi

    return $http_code
}

# Function to authenticate and get admin token
authenticate_admin() {
    echo -e "\n${BLUE}🔑 Authenticating Admin User${NC}"

    local login_data='{
        "email": "admin@abc.com",
        "password": "Admin123!@#"
    }'

    local response=$(curl -s -X POST "$BASE_URL/account/login" \
        -H "Content-Type: application/json" \
        -d "$login_data")

    local token=$(echo "$response" | jq -r '.token // empty' 2>/dev/null)

    if [ -n "$token" ] && [ "$token" != "null" ]; then
        echo -e "${GREEN}✅ Admin authentication successful${NC}"
        echo "$token"
    else
        echo -e "${RED}❌ Admin authentication failed${NC}"
        echo "$response"
        return 1
    fi
}

# Function to authenticate and get regular user token
authenticate_user() {
    echo -e "\n${BLUE}🔑 Authenticating Regular User${NC}"

    # Try to register a test user first
    local register_data='{
        "email": "testuser@abc.com",
        "password": "Test123!@#",
        "firstName": "Test",
        "lastName": "User"
    }'

    curl -s -X POST "$BASE_URL/account/register" \
        -H "Content-Type: application/json" \
        -d "$register_data" > /dev/null

    # Now login
    local login_data='{
        "email": "testuser@abc.com",
        "password": "Test123!@#"
    }'

    local response=$(curl -s -X POST "$BASE_URL/account/login" \
        -H "Content-Type: application/json" \
        -d "$login_data")

    local token=$(echo "$response" | jq -r '.token // empty' 2>/dev/null)

    if [ -n "$token" ] && [ "$token" != "null" ]; then
        echo -e "${GREEN}✅ User authentication successful${NC}"
        echo "$token"
    else
        echo -e "${RED}❌ User authentication failed${NC}"
        echo "$response"
        return 1
    fi
}

# Main test execution
main() {
    echo -e "\n${BLUE}🚀 Starting Step 12 Authorization & Scopes Tests${NC}"
    echo "Date: $(date)"
    echo ""

    # Get authentication tokens
    ADMIN_TOKEN=$(authenticate_admin)
    if [ $? -ne 0 ]; then
        echo -e "${RED}❌ Cannot proceed without admin authentication${NC}"
        exit 1
    fi

    USER_TOKEN=$(authenticate_user)
    if [ $? -ne 0 ]; then
        echo -e "${YELLOW}⚠️ User authentication failed, some tests will be skipped${NC}"
    fi

    # Test 1: Get Available Scopes (Admin)
    make_request "GET" "$OAUTH_URL/scopes" "" "$ADMIN_TOKEN" "Get available OAuth scopes"
    log_test $([ $? -eq 200 ] && echo 0 || echo 1) "Get available scopes (Admin)"

    # Test 2: Create OAuth Client (Admin)
    local client_data='{
        "name": "Test Client App",
        "description": "Test OAuth client for Step 12",
        "clientType": "confidential",
        "grantTypes": ["authorization_code", "refresh_token"],
        "redirectUris": ["https://localhost:3000/callback"],
        "scopes": ["openid", "profile", "email"],
        "requirePkce": true
    }'

    make_request "POST" "$OAUTH_URL/clients" "$client_data" "$ADMIN_TOKEN" "Register OAuth client"
    local client_response_code=$?
    log_test $([ $client_response_code -eq 200 ] || [ $client_response_code -eq 201 ] && echo 0 || echo 1) "Register OAuth client (Admin)"

    # Extract client ID for further tests
    if [ $client_response_code -eq 200 ] || [ $client_response_code -eq 201 ]; then
        CLIENT_ID=$(echo "$response_body" | jq -r '.clientId // empty' 2>/dev/null)
        echo -e "${GREEN}📝 Created client ID: $CLIENT_ID${NC}"
    fi

    # Test 3: Get Client List (Admin)
    make_request "GET" "$OAUTH_URL/clients" "" "$ADMIN_TOKEN" "Get OAuth client list"
    log_test $([ $? -eq 200 ] && echo 0 || echo 1) "Get OAuth client list (Admin)"

    # Test 4: Get Specific Client (Admin)
    if [ -n "$CLIENT_ID" ]; then
        make_request "GET" "$OAUTH_URL/clients/$CLIENT_ID" "" "$ADMIN_TOKEN" "Get specific OAuth client"
        log_test $([ $? -eq 200 ] && echo 0 || echo 1) "Get specific OAuth client (Admin)"
    fi

    # Test 5: Try Client Operations without Admin (Should Fail)
    if [ -n "$USER_TOKEN" ]; then
        make_request "GET" "$OAUTH_URL/clients" "" "$USER_TOKEN" "Get client list as regular user (should fail)"
        log_test $([ $? -eq 403 ] && echo 0 || echo 1) "Access control - Regular user cannot access client management"
    fi

    # Test 6: Get User Permissions (User)
    if [ -n "$USER_TOKEN" ]; then
        make_request "GET" "$OAUTH_URL/permissions" "" "$USER_TOKEN" "Get user permissions"
        log_test $([ $? -eq 200 ] && echo 0 || echo 1) "Get user permissions"
    fi

    # Test 7: Grant Permission to User (Admin)
    if [ -n "$USER_TOKEN" ]; then
        local permission_data='{
            "userId": "user-id-placeholder",
            "permissions": ["portfolio:read", "projects:read"],
            "reason": "Test permission assignment"
        }'

        make_request "POST" "$OAUTH_URL/permissions/grant" "$permission_data" "$ADMIN_TOKEN" "Grant permissions to user"
        log_test $([ $? -eq 200 ] || [ $? -eq 204 ] && echo 0 || echo 1) "Grant permissions (Admin)"
    fi

    # Test 8: Validate Authorization Request
    local auth_request='{
        "clientId": "'${CLIENT_ID:-test-client}'",
        "redirectUri": "https://localhost:3000/callback",
        "responseType": "code",
        "scope": "openid profile email",
        "state": "test-state-123",
        "codeChallenge": "dBjftJeZ4CVP-mB92K27uhbUJU1p1r_wW1gFWFOEjXk",
        "codeChallengeMethod": "S256"
    }'

    make_request "POST" "$OAUTH_URL/validate" "$auth_request" "" "Validate authorization request"
    log_test $([ $? -eq 200 ] && echo 0 || echo 1) "Validate authorization request"

    # Test 9: Get Effective Permissions
    if [ -n "$USER_TOKEN" ]; then
        make_request "GET" "$OAUTH_URL/effective-permissions?scopes=openid%20profile%20email" "" "$USER_TOKEN" "Get effective permissions"
        log_test $([ $? -eq 200 ] && echo 0 || echo 1) "Get effective permissions"
    fi

    # Test 10: Consent Flow Test
    if [ -n "$USER_TOKEN" ] && [ -n "$CLIENT_ID" ]; then
        make_request "GET" "$OAUTH_URL/consent?clientId=$CLIENT_ID&scopes=openid%20profile%20email" "" "$USER_TOKEN" "Get consent model"
        log_test $([ $? -eq 200 ] && echo 0 || echo 1) "Get consent model"

        # Process consent
        local consent_data='{
            "clientId": "'$CLIENT_ID'",
            "grantedScopes": ["openid", "profile", "email"],
            "rememberConsent": true
        }'

        make_request "POST" "$OAUTH_URL/consent" "$consent_data" "$USER_TOKEN" "Process user consent"
        log_test $([ $? -eq 200 ] && echo 0 || echo 1) "Process user consent"
    fi

    # Test 11: Get User Consents
    if [ -n "$USER_TOKEN" ]; then
        make_request "GET" "$OAUTH_URL/consents" "" "$USER_TOKEN" "Get user consent history"
        log_test $([ $? -eq 200 ] && echo 0 || echo 1) "Get user consent history"
    fi

    # Test 12: Create Custom Scope (Admin)
    local scope_data='{
        "name": "test:custom",
        "displayName": "Test Custom Scope",
        "description": "Custom scope for testing",
        "isRequired": false,
        "requiredPermissions": ["test:read", "test:write"]
    }'

    make_request "POST" "$OAUTH_URL/scopes" "$scope_data" "$ADMIN_TOKEN" "Create custom scope"
    log_test $([ $? -eq 200 ] || [ $? -eq 201 ] && echo 0 || echo 1) "Create custom scope (Admin)"

    # Test 13: Update OAuth Client (Admin)
    if [ -n "$CLIENT_ID" ]; then
        local update_data='{
            "name": "Updated Test Client",
            "description": "Updated OAuth client description",
            "scopes": ["openid", "profile", "email", "test:custom"]
        }'

        make_request "PUT" "$OAUTH_URL/clients/$CLIENT_ID" "$update_data" "$ADMIN_TOKEN" "Update OAuth client"
        log_test $([ $? -eq 200 ] || [ $? -eq 204 ] && echo 0 || echo 1) "Update OAuth client (Admin)"
    fi

    # Cleanup Test: Delete OAuth Client (Admin)
    if [ -n "$CLIENT_ID" ]; then
        make_request "DELETE" "$OAUTH_URL/clients/$CLIENT_ID" "" "$ADMIN_TOKEN" "Delete OAuth client"
        log_test $([ $? -eq 200 ] || [ $? -eq 204 ] && echo 0 || echo 1) "Delete OAuth client (Admin)"
    fi

    # Print test summary
    echo ""
    echo "🏁 Test Summary"
    echo "=============="
    echo -e "Total Tests: ${BLUE}$TOTAL_TESTS${NC}"
    echo -e "Passed: ${GREEN}$PASSED_TESTS${NC}"
    echo -e "Failed: ${RED}$FAILED_TESTS${NC}"
    echo -e "Success Rate: ${BLUE}$(( PASSED_TESTS * 100 / TOTAL_TESTS ))%${NC}"

    if [ $FAILED_TESTS -eq 0 ]; then
        echo -e "\n${GREEN}🎉 All Step 12 Authorization & Scopes tests passed!${NC}"
        exit 0
    else
        echo -e "\n${RED}⚠️ Some tests failed. Please check the implementation.${NC}"
        exit 1
    fi
}

# Check dependencies
if ! command -v curl &> /dev/null; then
    echo -e "${RED}❌ curl is required but not installed${NC}"
    exit 1
fi

if ! command -v jq &> /dev/null; then
    echo -e "${YELLOW}⚠️ jq is not installed. JSON responses won't be formatted.${NC}"
fi

# Run tests
main "$@"
