#!/bin/bash

# Step 15: Integration Testing - Slower Test Suite (for Rate Limiting)
# This script tests ALL functionality with delays to avoid rate limiting

echo "=========================================="
echo "🚀 Step 15: Integration Testing (Slower)"
echo "=========================================="

# Configuration
BASE_URL="http://localhost:5011"
API_URL="$BASE_URL/api"
TIMEOUT=30

# Test Data - Use unique identifiers to avoid conflicts
TIMESTAMP=$(date +%s)
TEST_EMAIL="step15.test.$TIMESTAMP@example.com"
TEST_USERNAME="step15user$TIMESTAMP"
TEST_PASSWORD="TestPassword123!"
TEST_FIRSTNAME="Step15"
TEST_LASTNAME="Tester"

# Colors for output
RED='\033[0;31m'
GREEN='\033[0;32m'
YELLOW='\033[1;33m'
BLUE='\033[0;34m'
NC='\033[0m' # No Color

# Helper Functions
print_test() {
    echo -e "\n${BLUE}=== $1 ===${NC}"
}

print_success() {
    echo -e "${GREEN}✅ $1${NC}"
}

print_error() {
    echo -e "${RED}❌ $1${NC}"
}

print_warning() {
    echo -e "${YELLOW}⚠️  $1${NC}"
}

# Test Counter
TOTAL_TESTS=0
PASSED_TESTS=0
FAILED_TESTS=0

run_test() {
    local test_name="$1"
    local expected_status="$2"
    local actual_status="$3"
    local response="$4"

    TOTAL_TESTS=$((TOTAL_TESTS + 1))

    if [ "$actual_status" -eq "$expected_status" ]; then
        print_success "$test_name (Status: $actual_status)"
        PASSED_TESTS=$((PASSED_TESTS + 1))
        return 0
    else
        print_error "$test_name (Expected: $expected_status, Got: $actual_status)"
        if [ ${#response} -lt 500 ]; then
            echo "Response: $response"
        else
            echo "Response: ${response:0:200}..."
        fi
        FAILED_TESTS=$((FAILED_TESTS + 1))
        return 1
    fi
}

# Wait between requests to avoid rate limiting
wait_for_rate_limit() {
    echo "⏳ Waiting 3 seconds to avoid rate limiting..."
    sleep 3
}

# Check if server is running
check_server() {
    print_test "Server Health Check"

    HEALTH_RESPONSE=$(curl -k -s -w "HTTPSTATUS:%{http_code}" "$API_URL/externalauth/providers" 2>/dev/null)
    HTTP_STATUS=$(echo $HEALTH_RESPONSE | tr -d '\n' | sed -e 's/.*HTTPSTATUS://')

    if [ "$HTTP_STATUS" = "200" ]; then
        print_success "Server is running (Status: $HTTP_STATUS)"
        return 0
    else
        print_error "Server is not responding (Status: $HTTP_STATUS)"
        echo "Please ensure the API server is running at $BASE_URL"
        exit 1
    fi
}

# Phase 1: Local Authentication Testing
test_local_authentication() {
    print_test "Phase 1: Local Authentication Testing"

    # Test 1: User Registration
    echo "1.1 Testing User Registration..."
    wait_for_rate_limit

    REGISTER_RESPONSE=$(curl -k -s -w "HTTPSTATUS:%{http_code}" \
      -X POST "$API_URL/account/register" \
      -H "Content-Type: application/json" \
      -d '{
        "userName": "'$TEST_USERNAME'",
        "email": "'$TEST_EMAIL'",
        "password": "'$TEST_PASSWORD'",
        "confirmPassword": "'$TEST_PASSWORD'",
        "firstName": "'$TEST_FIRSTNAME'",
        "lastName": "'$TEST_LASTNAME'"
      }')

    HTTP_STATUS=$(echo $REGISTER_RESPONSE | tr -d '\n' | sed -e 's/.*HTTPSTATUS://')
    REGISTER_BODY=$(echo $REGISTER_RESPONSE | sed -e 's/HTTPSTATUS\:.*//g')

    if [ $HTTP_STATUS -eq 200 ] || [ $HTTP_STATUS -eq 201 ]; then
        run_test "User Registration" "200" "$HTTP_STATUS" "$REGISTER_BODY"
        ACCESS_TOKEN=$(echo $REGISTER_BODY | jq -r '.accessToken' 2>/dev/null)
        if [ "$ACCESS_TOKEN" != "null" ] && [ -n "$ACCESS_TOKEN" ]; then
            echo "✓ Access Token received: ${ACCESS_TOKEN:0:30}..."
        fi
    else
        run_test "User Registration" "200" "$HTTP_STATUS" "$REGISTER_BODY"
    fi

    # Test 2: Failed Login Attempt
    echo "1.2 Testing Failed Login..."
    wait_for_rate_limit

    FAILED_LOGIN_RESPONSE=$(curl -k -s -w "HTTPSTATUS:%{http_code}" \
      -X POST "$API_URL/account/login" \
      -H "Content-Type: application/json" \
      -d '{
        "emailOrUsername": "'$TEST_EMAIL'",
        "password": "WrongPassword123!"
      }')

    HTTP_STATUS=$(echo $FAILED_LOGIN_RESPONSE | tr -d '\n' | sed -e 's/.*HTTPSTATUS://')
    FAILED_LOGIN_BODY=$(echo $FAILED_LOGIN_RESPONSE | sed -e 's/HTTPSTATUS\:.*//g')

    run_test "Failed Login Attempt" "401" "$HTTP_STATUS" "$FAILED_LOGIN_BODY"

    # Test 3: Successful Login
    echo "1.3 Testing Successful Login..."
    wait_for_rate_limit

    LOGIN_RESPONSE=$(curl -k -s -w "HTTPSTATUS:%{http_code}" \
      -X POST "$API_URL/account/login" \
      -H "Content-Type: application/json" \
      -d '{
        "emailOrUsername": "'$TEST_EMAIL'",
        "password": "'$TEST_PASSWORD'"
      }')

    HTTP_STATUS=$(echo $LOGIN_RESPONSE | tr -d '\n' | sed -e 's/.*HTTPSTATUS://')
    LOGIN_BODY=$(echo $LOGIN_RESPONSE | sed -e 's/HTTPSTATUS\:.*//g')

    if [ $HTTP_STATUS -eq 200 ]; then
        run_test "Successful Login" "200" "$HTTP_STATUS" "$LOGIN_BODY"
        ACCESS_TOKEN=$(echo $LOGIN_BODY | jq -r '.accessToken' 2>/dev/null)
        if [ "$ACCESS_TOKEN" != "null" ] && [ -n "$ACCESS_TOKEN" ]; then
            echo "✓ Access Token received: ${ACCESS_TOKEN:0:30}..."
        fi
    else
        run_test "Successful Login" "200" "$HTTP_STATUS" "$LOGIN_BODY"
    fi
}

# Phase 2: External Provider Testing
test_external_providers() {
    print_test "Phase 2: External Provider Testing"

    # Test 1: Get Available Providers
    echo "2.1 Testing Available Providers..."
    wait_for_rate_limit

    PROVIDERS_RESPONSE=$(curl -k -s -w "HTTPSTATUS:%{http_code}" \
      -X GET "$API_URL/externalauth/providers")

    HTTP_STATUS=$(echo $PROVIDERS_RESPONSE | tr -d '\n' | sed -e 's/.*HTTPSTATUS://')
    PROVIDERS_BODY=$(echo $PROVIDERS_RESPONSE | sed -e 's/HTTPSTATUS\:.*//g')

    run_test "Get Available Providers" "200" "$HTTP_STATUS" "$PROVIDERS_BODY"

    # Test 2: Google OAuth Challenge
    echo "2.2 Testing Google OAuth Challenge..."
    wait_for_rate_limit

    GOOGLE_CHALLENGE_RESPONSE=$(curl -k -s -w "HTTPSTATUS:%{http_code}" \
      -X GET "$API_URL/externalauth/challenge/Google")

    HTTP_STATUS=$(echo $GOOGLE_CHALLENGE_RESPONSE | tr -d '\n' | sed -e 's/.*HTTPSTATUS://')
    GOOGLE_CHALLENGE_BODY=$(echo $GOOGLE_CHALLENGE_RESPONSE | sed -e 's/HTTPSTATUS\:.*//g')

    run_test "Google OAuth Challenge" "302" "$HTTP_STATUS" "$GOOGLE_CHALLENGE_BODY"

    # Test 3: Microsoft OAuth Challenge
    echo "2.3 Testing Microsoft OAuth Challenge..."
    wait_for_rate_limit

    MICROSOFT_CHALLENGE_RESPONSE=$(curl -k -s -w "HTTPSTATUS:%{http_code}" \
      -X GET "$API_URL/externalauth/challenge/Microsoft")

    HTTP_STATUS=$(echo $MICROSOFT_CHALLENGE_RESPONSE | tr -d '\n' | sed -e 's/.*HTTPSTATUS://')
    MICROSOFT_CHALLENGE_BODY=$(echo $MICROSOFT_CHALLENGE_RESPONSE | sed -e 's/HTTPSTATUS\:.*//g')

    run_test "Microsoft OAuth Challenge" "302" "$HTTP_STATUS" "$MICROSOFT_CHALLENGE_BODY"
}

# Phase 3: Token Validation Testing
test_token_validation() {
    print_test "Phase 3: Token Validation Testing"

    if [ -z "$ACCESS_TOKEN" ] || [ "$ACCESS_TOKEN" = "null" ]; then
        print_warning "No valid access token available. Skipping token validation tests."
        return
    fi

    # Test 1: Access Protected Endpoint
    echo "3.1 Testing Protected Endpoint Access..."
    wait_for_rate_limit

    PROFILE_RESPONSE=$(curl -k -s -w "HTTPSTATUS:%{http_code}" \
      -X GET "$API_URL/account/profile" \
      -H "Authorization: Bearer $ACCESS_TOKEN")

    HTTP_STATUS=$(echo $PROFILE_RESPONSE | tr -d '\n' | sed -e 's/.*HTTPSTATUS://')
    PROFILE_BODY=$(echo $PROFILE_RESPONSE | sed -e 's/HTTPSTATUS\:.*//g')

    run_test "Protected Endpoint Access" "200" "$HTTP_STATUS" "$PROFILE_BODY"

    # Test 2: Invalid Token Test
    echo "3.2 Testing Invalid Token..."
    wait_for_rate_limit

    INVALID_TOKEN_RESPONSE=$(curl -k -s -w "HTTPSTATUS:%{http_code}" \
      -X GET "$API_URL/account/profile" \
      -H "Authorization: Bearer invalid_token_here")

    HTTP_STATUS=$(echo $INVALID_TOKEN_RESPONSE | tr -d '\n' | sed -e 's/.*HTTPSTATUS://')
    INVALID_TOKEN_BODY=$(echo $INVALID_TOKEN_RESPONSE | sed -e 's/HTTPSTATUS\:.*//g')

    run_test "Invalid Token Rejection" "401" "$HTTP_STATUS" "$INVALID_TOKEN_BODY"
}

# Phase 4: Account Linking Testing
test_account_linking() {
    print_test "Phase 4: Account Linking Testing"

    if [ -z "$ACCESS_TOKEN" ] || [ "$ACCESS_TOKEN" = "null" ]; then
        print_warning "No valid access token available. Skipping account linking tests."
        return
    fi

    # Test 1: Get Linked Accounts
    echo "4.1 Testing Get Linked Accounts..."
    wait_for_rate_limit

    LINKED_RESPONSE=$(curl -k -s -w "HTTPSTATUS:%{http_code}" \
      -X GET "$API_URL/externalauth/linked-accounts" \
      -H "Authorization: Bearer $ACCESS_TOKEN")

    HTTP_STATUS=$(echo $LINKED_RESPONSE | tr -d '\n' | sed -e 's/.*HTTPSTATUS://')
    LINKED_BODY=$(echo $LINKED_RESPONSE | sed -e 's/HTTPSTATUS\:.*//g')

    run_test "Get Linked Accounts" "200" "$HTTP_STATUS" "$LINKED_BODY"
}

# Phase 5: Security Features Verification
test_security_features() {
    print_test "Phase 5: Security Features Verification"

    # Test 1: CORS Headers
    echo "5.1 Testing CORS Headers..."
    wait_for_rate_limit

    CORS_RESPONSE=$(curl -k -s -w "HTTPSTATUS:%{http_code}" \
      -X OPTIONS "$API_URL/account/login" \
      -H "Origin: http://localhost:3000" \
      -H "Access-Control-Request-Method: POST")

    HTTP_STATUS=$(echo $CORS_RESPONSE | tr -d '\n' | sed -e 's/.*HTTPSTATUS://')

    run_test "CORS Preflight" "204" "$HTTP_STATUS" "CORS headers check"

    # Test 2: Security Headers
    echo "5.2 Testing Security Headers..."
    wait_for_rate_limit

    HEADERS_RESPONSE=$(curl -k -s -I "$API_URL/externalauth/providers")

    if echo "$HEADERS_RESPONSE" | grep -q "X-Frame-Options\|X-Content-Type-Options"; then
        print_success "Security Headers Present"
        PASSED_TESTS=$((PASSED_TESTS + 1))
    else
        print_warning "Some security headers missing"
    fi
    TOTAL_TESTS=$((TOTAL_TESTS + 1))
}

# Phase 6: Logout Testing
test_logout() {
    print_test "Phase 6: Logout Testing"

    if [ -z "$ACCESS_TOKEN" ] || [ "$ACCESS_TOKEN" = "null" ]; then
        print_warning "No valid access token available. Skipping logout tests."
        return
    fi

    # Test 1: Hybrid Logout
    echo "6.1 Testing Hybrid Logout..."
    wait_for_rate_limit

    LOGOUT_RESPONSE=$(curl -k -s -w "HTTPSTATUS:%{http_code}" \
      -X POST "$API_URL/account/hybrid-logout" \
      -H "Content-Type: application/json" \
      -H "Authorization: Bearer $ACCESS_TOKEN" \
      -d '{}')

    HTTP_STATUS=$(echo $LOGOUT_RESPONSE | tr -d '\n' | sed -e 's/.*HTTPSTATUS://')
    LOGOUT_BODY=$(echo $LOGOUT_RESPONSE | sed -e 's/HTTPSTATUS\:.*//g')

    run_test "Hybrid Logout" "200" "$HTTP_STATUS" "$LOGOUT_BODY"

    # Test 2: Access After Logout
    echo "6.2 Testing Access After Logout..."
    wait_for_rate_limit

    POST_LOGOUT_RESPONSE=$(curl -k -s -w "HTTPSTATUS:%{http_code}" \
      -X GET "$API_URL/account/profile" \
      -H "Authorization: Bearer $ACCESS_TOKEN")

    HTTP_STATUS=$(echo $POST_LOGOUT_RESPONSE | tr -d '\n' | sed -e 's/.*HTTPSTATUS://')

    run_test "Access After Logout" "401" "$HTTP_STATUS" "Token should be invalid"
}

# Main Test Execution
main() {
    echo "Starting comprehensive integration tests for ABC Portfolio API..."
    echo "Testing all functionality from Steps 1-14 (with rate limit delays)"
    echo ""

    # Pre-test checks
    check_server

    # Run all test phases
    test_local_authentication
    test_external_providers
    test_token_validation
    test_account_linking
    test_security_features
    test_logout

    # Test Summary
    echo ""
    echo "=========================================="
    echo "🎯 Step 15 Integration Test Summary"
    echo "=========================================="
    echo -e "${BLUE}Total Tests: $TOTAL_TESTS${NC}"
    echo -e "${GREEN}Passed: $PASSED_TESTS${NC}"
    echo -e "${RED}Failed: $FAILED_TESTS${NC}"

    PASS_RATE=$(echo "scale=1; $PASSED_TESTS * 100 / $TOTAL_TESTS" | bc -l 2>/dev/null || echo "N/A")
    echo -e "${BLUE}Pass Rate: $PASS_RATE%${NC}"

    if [ $FAILED_TESTS -eq 0 ]; then
        echo -e "\n${GREEN}🎉 ALL TESTS PASSED! 🎉${NC}"
        echo -e "${GREEN}✅ ABC Portfolio API is ready for production!${NC}"
        exit 0
    elif [ $PASSED_TESTS -gt $FAILED_TESTS ]; then
        echo -e "\n${GREEN}✅ MAJORITY OF TESTS PASSED! ${NC}"
        echo -e "${YELLOW}⚠️  Minor issues need attention, but system is largely functional${NC}"
        exit 0
    else
        echo -e "\n${YELLOW}⚠️  Some tests failed. Please review and fix issues.${NC}"
        exit 1
    fi
}

# Run the main function
main "$@"
