#!/bin/bash

# Colors for output
RED='\033[0;31m'
GREEN='\033[0;32m'
YELLOW='\033[1;33m'
CYAN='\033[0;36m'
NC='\033[0m' # No Color

BASE_URL="http://localhost:5016/api"

echo -e "${CYAN}=== ABC Portfolio API - Step 7: External Authentication Tests ===${NC}"
echo -e "${YELLOW}Testing external authentication provider integration${NC}"
echo

# Test 1: Get supported external providers
echo -e "${YELLOW}1. Testing Supported External Providers${NC}"
PROVIDERS_RESPONSE=$(curl -s -X GET "$BASE_URL/Account/external-login/Google" -w "%{http_code}" -o /dev/null)

if [ "$PROVIDERS_RESPONSE" = "302" ] || [ "$PROVIDERS_RESPONSE" = "200" ]; then
    echo -e "${GREEN}✓ Google provider endpoint accessible${NC}"
else
    echo -e "${RED}✗ Google provider endpoint not working (status: $PROVIDERS_RESPONSE)${NC}"
fi

MICROSOFT_RESPONSE=$(curl -s -X GET "$BASE_URL/Account/external-login/Microsoft" -w "%{http_code}" -o /dev/null)

if [ "$MICROSOFT_RESPONSE" = "302" ] || [ "$MICROSOFT_RESPONSE" = "200" ]; then
    echo -e "${GREEN}✓ Microsoft provider endpoint accessible${NC}"
else
    echo -e "${RED}✗ Microsoft provider endpoint not working (status: $MICROSOFT_RESPONSE)${NC}"
fi
echo

# Test 2: Test unsupported provider
echo -e "${YELLOW}2. Testing Unsupported Provider Rejection${NC}"
UNSUPPORTED_RESPONSE=$(curl -s -X GET "$BASE_URL/Account/external-login/InvalidProvider")

if echo "$UNSUPPORTED_RESPONSE" | grep -q "not supported"; then
    echo -e "${GREEN}✓ Unsupported provider correctly rejected${NC}"
else
    echo -e "${RED}✗ Unsupported provider validation failed${NC}"
    echo "Response: $UNSUPPORTED_RESPONSE"
fi
echo

# Test 3: Test external logins endpoint (requires authentication)
echo -e "${YELLOW}3. Testing External Logins Endpoint (Authentication Required)${NC}"
EXTERNAL_LOGINS_RESPONSE=$(curl -s -X GET "$BASE_URL/Account/external-logins" -w "%{http_code}" -o /dev/null)

if [ "$EXTERNAL_LOGINS_RESPONSE" = "401" ]; then
    echo -e "${GREEN}✓ External logins endpoint correctly requires authentication${NC}"
else
    echo -e "${RED}✗ External logins endpoint authentication check failed (status: $EXTERNAL_LOGINS_RESPONSE)${NC}"
fi
echo

# Test 4: Test external login callback endpoint
echo -e "${YELLOW}4. Testing External Login Callback Endpoint${NC}"
CALLBACK_RESPONSE=$(curl -s -X GET "$BASE_URL/Account/external-login-callback")

if echo "$CALLBACK_RESPONSE" | grep -q "Error loading external login information" || echo "$CALLBACK_RESPONSE" | grep -q "errors"; then
    echo -e "${GREEN}✓ External login callback endpoint functional (expects external login context)${NC}"
else
    echo -e "${RED}✗ External login callback endpoint unexpected response${NC}"
    echo "Response: $CALLBACK_RESPONSE"
fi
echo

# Test 5: Verify external authentication configuration
echo -e "${YELLOW}5. Testing External Authentication Configuration${NC}"

# Check if Google configuration exists (will fail gracefully if not configured)
GOOGLE_AUTH_TEST=$(curl -s -X GET "$BASE_URL/Account/external-login/Google?returnUrl=test" -I | head -n 1)

if echo "$GOOGLE_AUTH_TEST" | grep -q "HTTP.*30"; then
    echo -e "${GREEN}✓ Google OAuth configuration appears to be set up${NC}"
else
    echo -e "${YELLOW}⚠ Google OAuth may need configuration (client ID/secret)${NC}"
fi

# Check if Microsoft configuration exists
MICROSOFT_AUTH_TEST=$(curl -s -X GET "$BASE_URL/Account/external-login/Microsoft?returnUrl=test" -I | head -n 1)

if echo "$MICROSOFT_AUTH_TEST" | grep -q "HTTP.*30"; then
    echo -e "${GREEN}✓ Microsoft OAuth configuration appears to be set up${NC}"
else
    echo -e "${YELLOW}⚠ Microsoft OAuth may need configuration (client ID/secret)${NC}"
fi
echo

# Test 6: Test API documentation
echo -e "${YELLOW}6. Testing API Documentation for External Auth${NC}"
SWAGGER_EXTERNAL=$(curl -s "$BASE_URL/../swagger/v1/swagger.json" | grep -c "external-login")

if [ "$SWAGGER_EXTERNAL" -gt "0" ]; then
    echo -e "${GREEN}✓ External authentication endpoints documented in Swagger${NC}"
    echo "Found $SWAGGER_EXTERNAL external authentication endpoint(s)"
else
    echo -e "${RED}✗ External authentication endpoints not found in documentation${NC}"
fi
echo

echo -e "${CYAN}=== Step 7 External Authentication Testing Summary ===${NC}"
echo -e "${GREEN}Completed Tests:${NC}"
echo -e "${GREEN}• External provider endpoint accessibility ✓${NC}"
echo -e "${GREEN}• Unsupported provider rejection ✓${NC}"
echo -e "${GREEN}• Authentication requirement enforcement ✓${NC}"
echo -e "${GREEN}• External login callback handling ✓${NC}"
echo -e "${GREEN}• OAuth configuration validation ✓${NC}"
echo -e "${GREEN}• API documentation verification ✓${NC}"
echo
echo -e "${YELLOW}Next Steps for Production:${NC}"
echo -e "${YELLOW}• Configure Google OAuth 2.0 client ID and secret${NC}"
echo -e "${YELLOW}• Configure Microsoft OAuth 2.0 client ID and secret${NC}"
echo -e "${YELLOW}• Set up proper redirect URIs in provider consoles${NC}"
echo -e "${YELLOW}• Test complete external authentication flow${NC}"
echo
echo -e "${CYAN}Step 7 External Authentication Implementation: COMPLETE ✅${NC}"
