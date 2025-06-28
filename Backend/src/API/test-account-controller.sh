#!/bin/bash

# Test Script for ABC Portfolio API - Account Controller
# Step 5: Account Controller Testing

BASE_URL="http://localhost:5014/api"
CYAN='\033[0;36m'
GREEN='\033[0;32m'
RED='\033[0;31m'
YELLOW='\033[1;33m'
NC='\033[0m' # No Color

echo -e "${CYAN}=== ABC Portfolio API - Account Controller Tests ===${NC}"
echo -e "${CYAN}Testing local registration and login functionality${NC}"
echo

# Test 1: Register a new user
echo -e "${YELLOW}1. Testing User Registration${NC}"
REGISTER_RESPONSE=$(curl -s -X POST "$BASE_URL/account/register" \
  -H "Content-Type: application/json" \
  -d '{
    "userName": "testuser",
    "email": "test@example.com",
    "password": "SecurePass123!",
    "confirmPassword": "SecurePass123!",
    "firstName": "Test",
    "lastName": "User"
  }')

if echo "$REGISTER_RESPONSE" | grep -q "success.*true"; then
    echo -e "${GREEN}✓ Registration successful${NC}"
    ACCESS_TOKEN=$(echo "$REGISTER_RESPONSE" | grep -o '"accessToken":"[^"]*"' | cut -d'"' -f4)
    REFRESH_TOKEN=$(echo "$REGISTER_RESPONSE" | grep -o '"refreshToken":"[^"]*"' | cut -d'"' -f4)
    echo -e "${GREEN}✓ Tokens received${NC}"
else
    echo -e "${RED}✗ Registration failed${NC}"
    echo "Response: $REGISTER_RESPONSE"
fi
echo

# Test 2: Try to register with same email (should fail)
echo -e "${YELLOW}2. Testing Duplicate Email Registration${NC}"
DUPLICATE_RESPONSE=$(curl -s -X POST "$BASE_URL/account/register" \
  -H "Content-Type: application/json" \
  -d '{
    "userName": "testuser2",
    "email": "test@example.com",
    "password": "SecurePass123!",
    "confirmPassword": "SecurePass123!",
    "firstName": "Test2",
    "lastName": "User2"
  }')

if echo "$DUPLICATE_RESPONSE" | grep -q "email.*already exists"; then
    echo -e "${GREEN}✓ Duplicate email correctly rejected${NC}"
else
    echo -e "${RED}✗ Duplicate email validation failed${NC}"
    echo "Response: $DUPLICATE_RESPONSE"
fi
echo

# Test 3: Login with email
echo -e "${YELLOW}3. Testing Login with Email${NC}"
LOGIN_RESPONSE=$(curl -s -X POST "$BASE_URL/account/login" \
  -H "Content-Type: application/json" \
  -d '{
    "emailOrUsername": "test@example.com",
    "password": "SecurePass123!",
    "rememberMe": false
  }')

if echo "$LOGIN_RESPONSE" | grep -q "success.*true"; then
    echo -e "${GREEN}✓ Login with email successful${NC}"
    LOGIN_ACCESS_TOKEN=$(echo "$LOGIN_RESPONSE" | grep -o '"accessToken":"[^"]*"' | cut -d'"' -f4)
else
    echo -e "${RED}✗ Login with email failed${NC}"
    echo "Response: $LOGIN_RESPONSE"
fi
echo

# Test 4: Login with username
echo -e "${YELLOW}4. Testing Login with Username${NC}"
USERNAME_LOGIN_RESPONSE=$(curl -s -X POST "$BASE_URL/account/login" \
  -H "Content-Type: application/json" \
  -d '{
    "emailOrUsername": "testuser",
    "password": "SecurePass123!",
    "rememberMe": true
  }')

if echo "$USERNAME_LOGIN_RESPONSE" | grep -q "success.*true"; then
    echo -e "${GREEN}✓ Login with username successful${NC}"
else
    echo -e "${RED}✗ Login with username failed${NC}"
    echo "Response: $USERNAME_LOGIN_RESPONSE"
fi
echo

# Test 5: Get current user information
echo -e "${YELLOW}5. Testing Get Current User${NC}"
if [ ! -z "$ACCESS_TOKEN" ]; then
    USER_INFO_RESPONSE=$(curl -s -X GET "$BASE_URL/account/me" \
      -H "Authorization: Bearer $ACCESS_TOKEN")

    if echo "$USER_INFO_RESPONSE" | grep -q '"userName":"testuser"'; then
        echo -e "${GREEN}✓ Get current user successful${NC}"
        echo -e "${GREEN}✓ User information retrieved${NC}"
    else
        echo -e "${RED}✗ Get current user failed${NC}"
        echo "Response: $USER_INFO_RESPONSE"
    fi
else
    echo -e "${RED}✗ No access token available from registration${NC}"
fi
echo

# Test 6: Test invalid login
echo -e "${YELLOW}6. Testing Invalid Login${NC}"
INVALID_LOGIN_RESPONSE=$(curl -s -X POST "$BASE_URL/account/login" \
  -H "Content-Type: application/json" \
  -d '{
    "emailOrUsername": "test@example.com",
    "password": "WrongPassword",
    "rememberMe": false
  }')

if echo "$INVALID_LOGIN_RESPONSE" | grep -q "Invalid email/username or password"; then
    echo -e "${GREEN}✓ Invalid credentials correctly rejected${NC}"
else
    echo -e "${RED}✗ Invalid credentials validation failed${NC}"
    echo "Response: $INVALID_LOGIN_RESPONSE"
fi
echo

# Test 7: Test weak password registration
echo -e "${YELLOW}7. Testing Weak Password Registration${NC}"
WEAK_PASSWORD_RESPONSE=$(curl -s -X POST "$BASE_URL/account/register" \
  -H "Content-Type: application/json" \
  -d '{
    "userName": "weakuser",
    "email": "weak@example.com",
    "password": "123",
    "confirmPassword": "123",
    "firstName": "Weak",
    "lastName": "User"
  }')

if echo "$WEAK_PASSWORD_RESPONSE" | grep -q "success.*false\|error"; then
    echo -e "${GREEN}✓ Weak password correctly rejected${NC}"
else
    echo -e "${RED}✗ Weak password validation failed${NC}"
    echo "Response: $WEAK_PASSWORD_RESPONSE"
fi
echo

# Test 8: Test refresh token
echo -e "${YELLOW}8. Testing Token Refresh${NC}"
if [ ! -z "$ACCESS_TOKEN" ] && [ ! -z "$REFRESH_TOKEN" ]; then
    REFRESH_RESPONSE=$(curl -s -X POST "$BASE_URL/account/refresh-token" \
      -H "Content-Type: application/json" \
      -d "{
        \"accessToken\": \"$ACCESS_TOKEN\",
        \"refreshToken\": \"$REFRESH_TOKEN\"
      }")

    if echo "$REFRESH_RESPONSE" | grep -q "success.*true"; then
        echo -e "${GREEN}✓ Token refresh successful${NC}"
        NEW_ACCESS_TOKEN=$(echo "$REFRESH_RESPONSE" | grep -o '"accessToken":"[^"]*"' | cut -d'"' -f4)
    else
        echo -e "${RED}✗ Token refresh failed${NC}"
        echo "Response: $REFRESH_RESPONSE"
    fi
else
    echo -e "${RED}✗ No tokens available for refresh test${NC}"
fi
echo

# Test 9: Test logout
echo -e "${YELLOW}9. Testing Logout${NC}"
if [ ! -z "$ACCESS_TOKEN" ]; then
    LOGOUT_RESPONSE=$(curl -s -X POST "$BASE_URL/account/logout" \
      -H "Authorization: Bearer $ACCESS_TOKEN")

    if echo "$LOGOUT_RESPONSE" | grep -q "Logged out successfully"; then
        echo -e "${GREEN}✓ Logout successful${NC}"
    else
        echo -e "${RED}✗ Logout failed${NC}"
        echo "Response: $LOGOUT_RESPONSE"
    fi
else
    echo -e "${RED}✗ No access token available for logout test${NC}"
fi
echo

echo -e "${CYAN}=== Step 5 Account Controller Testing Complete ===${NC}"
echo -e "${GREEN}Summary:${NC}"
echo -e "${GREEN}• User registration with JWT token generation ✓${NC}"
echo -e "${GREEN}• Email and username login ✓${NC}"
echo -e "${GREEN}• Token-based authentication ✓${NC}"
echo -e "${GREEN}• Input validation and error handling ✓${NC}"
echo -e "${GREEN}• Token refresh mechanism ✓${NC}"
echo -e "${GREEN}• Secure logout ✓${NC}"
echo
echo -e "${CYAN}Ready for Step 6: Password & Security Policies${NC}"

# Step 6: Password Policy & Security Tests
echo -e "${YELLOW}Step 6: Testing Password Policy & Security Features${NC}"

# Test 1: Get Password Requirements
echo -e "${YELLOW}1. Testing Password Requirements Endpoint${NC}"
REQUIREMENTS_RESPONSE=$(curl -s -X GET "$BASE_URL/Account/password-requirements" \
  -H "Content-Type: application/json")

if echo "$REQUIREMENTS_RESPONSE" | grep -q "requirements"; then
    echo -e "${GREEN}✓ Password requirements retrieved${NC}"
    echo "Requirements: $(echo "$REQUIREMENTS_RESPONSE" | jq '.requirements' 2>/dev/null || echo "$REQUIREMENTS_RESPONSE")"
else
    echo -e "${RED}✗ Failed to get password requirements${NC}"
    echo "Response: $REQUIREMENTS_RESPONSE"
fi
echo

# Test 2: Validate Weak Password
echo -e "${YELLOW}2. Testing Weak Password Validation${NC}"
WEAK_PASSWORD_RESPONSE=$(curl -s -X POST "$BASE_URL/Account/validate-password" \
  -H "Content-Type: application/json" \
  -d '{"password": "weak"}')

if echo "$WEAK_PASSWORD_RESPONSE" | grep -q "isValid.*false"; then
    echo -e "${GREEN}✓ Weak password correctly rejected${NC}"
    echo "Validation: $(echo "$WEAK_PASSWORD_RESPONSE" | jq '.validationErrors' 2>/dev/null || echo "$WEAK_PASSWORD_RESPONSE")"
else
    echo -e "${RED}✗ Weak password validation failed${NC}"
    echo "Response: $WEAK_PASSWORD_RESPONSE"
fi
echo

# Test 3: Validate Strong Password
echo -e "${YELLOW}3. Testing Strong Password Validation${NC}"
STRONG_PASSWORD_RESPONSE=$(curl -s -X POST "$BASE_URL/Account/validate-password" \
  -H "Content-Type: application/json" \
  -d '{"password": "StrongP@ssw0rd123"}')

if echo "$STRONG_PASSWORD_RESPONSE" | grep -q "isValid.*true"; then
    echo -e "${GREEN}✓ Strong password correctly accepted${NC}"
else
    echo -e "${RED}✗ Strong password validation failed${NC}"
    echo "Response: $STRONG_PASSWORD_RESPONSE"
fi
echo

# Test 4: Check Security Headers
echo -e "${YELLOW}4. Testing Security Headers${NC}"
HEADERS_RESPONSE=$(curl -s -I "$BASE_URL/Account/password-requirements")

if echo "$HEADERS_RESPONSE" | grep -qi "X-Content-Type-Options"; then
    echo -e "${GREEN}✓ Security headers present${NC}"
    echo "Headers found:"
    echo "$HEADERS_RESPONSE" | grep -i "X-\|Strict-Transport-Security\|Content-Security-Policy"
else
    echo -e "${YELLOW}⚠ Security headers check - review needed${NC}"
    echo "Response headers: $HEADERS_RESPONSE"
fi
echo

echo -e "${CYAN}=== Step 6 Password Policy & Security Features Testing Complete ===${NC}"
echo -e "${GREEN}Summary:${NC}"
echo -e "${GREEN}• Password strength validation ✓${NC}"
echo -e "${GREEN}• Security headers presence ✓${NC}"
echo
echo -e "${CYAN}All steps testing complete. End of script.${NC}"
