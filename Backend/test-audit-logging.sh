#!/bin/bash

# Step 14 Audit & Logging Test Script
# This script tests the audit logging functionality for authentication events

echo "=== Step 14 - Audit & Logging Test ==="

BASE_URL="https://localhost:7248/api"
API_URL="$BASE_URL"

# Test data
TEST_EMAIL="audit.test@example.com"
TEST_USERNAME="audituser"
TEST_PASSWORD="TestPassword123!"
TEST_FIRSTNAME="Audit"
TEST_LASTNAME="Test"

echo ""
echo "1. Testing User Registration with Audit Logging..."

REGISTER_RESPONSE=$(curl -k -s -w "HTTPSTATUS:%{http_code}" \
  -X POST "$API_URL/account/register" \
  -H "Content-Type: application/json" \
  -d '{
    "userName": "'$TEST_USERNAME'",
    "email": "'$TEST_EMAIL'",
    "password": "'$TEST_PASSWORD'",
    "firstName": "'$TEST_FIRSTNAME'",
    "lastName": "'$TEST_LASTNAME'"
  }')

HTTP_STATUS=$(echo $REGISTER_RESPONSE | tr -d '\n' | sed -e 's/.*HTTPSTATUS://')
REGISTER_BODY=$(echo $REGISTER_RESPONSE | sed -e 's/HTTPSTATUS\:.*//g')

echo "Registration HTTP Status: $HTTP_STATUS"
if [ $HTTP_STATUS -eq 200 ] || [ $HTTP_STATUS -eq 201 ]; then
    echo "✅ Registration successful"
    echo "Response: $REGISTER_BODY"
    ACCESS_TOKEN=$(echo $REGISTER_BODY | jq -r '.accessToken')
    echo "Access Token: ${ACCESS_TOKEN:0:50}..."
else
    echo "❌ Registration failed"
    echo "Response: $REGISTER_BODY"
fi

echo ""
echo "2. Testing Failed Login with Audit Logging..."

FAILED_LOGIN_RESPONSE=$(curl -k -s -w "HTTPSTATUS:%{http_code}" \
  -X POST "$API_URL/account/login" \
  -H "Content-Type: application/json" \
  -d '{
    "emailOrUsername": "'$TEST_EMAIL'",
    "password": "WrongPassword123!"
  }')

HTTP_STATUS=$(echo $FAILED_LOGIN_RESPONSE | tr -d '\n' | sed -e 's/.*HTTPSTATUS://')
FAILED_LOGIN_BODY=$(echo $FAILED_LOGIN_RESPONSE | sed -e 's/HTTPSTATUS\:.*//g')

echo "Failed Login HTTP Status: $HTTP_STATUS"
if [ $HTTP_STATUS -eq 401 ]; then
    echo "✅ Failed login properly rejected (expected)"
    echo "Response: $FAILED_LOGIN_BODY"
else
    echo "⚠️  Unexpected failed login response"
    echo "Response: $FAILED_LOGIN_BODY"
fi

echo ""
echo "3. Testing Successful Login with Audit Logging..."

LOGIN_RESPONSE=$(curl -k -s -w "HTTPSTATUS:%{http_code}" \
  -X POST "$API_URL/account/login" \
  -H "Content-Type: application/json" \
  -d '{
    "emailOrUsername": "'$TEST_EMAIL'",
    "password": "'$TEST_PASSWORD'"
  }')

HTTP_STATUS=$(echo $LOGIN_RESPONSE | tr -d '\n' | sed -e 's/.*HTTPSTATUS://')
LOGIN_BODY=$(echo $LOGIN_RESPONSE | sed -e 's/HTTPSTATUS\:.*//g')

echo "Login HTTP Status: $HTTP_STATUS"
if [ $HTTP_STATUS -eq 200 ]; then
    echo "✅ Login successful"
    ACCESS_TOKEN=$(echo $LOGIN_BODY | jq -r '.accessToken')
    echo "Access Token: ${ACCESS_TOKEN:0:50}..."

    echo ""
    echo "4. Testing Portfolio Creation with Audit Logging..."

    PORTFOLIO_RESPONSE=$(curl -k -s -w "HTTPSTATUS:%{http_code}" \
      -X POST "$API_URL/portfolio" \
      -H "Content-Type: application/json" \
      -H "Authorization: Bearer $ACCESS_TOKEN" \
      -d '{
        "title": "Test Audit Portfolio",
        "description": "A portfolio created for audit testing",
        "userId": "audit-test-user"
      }')

    HTTP_STATUS=$(echo $PORTFOLIO_RESPONSE | tr -d '\n' | sed -e 's/.*HTTPSTATUS://')
    PORTFOLIO_BODY=$(echo $PORTFOLIO_RESPONSE | sed -e 's/HTTPSTATUS\:.*//g')

    echo "Portfolio Creation HTTP Status: $HTTP_STATUS"
    if [ $HTTP_STATUS -eq 201 ]; then
        echo "✅ Portfolio created successfully"
        PORTFOLIO_ID=$(echo $PORTFOLIO_BODY | jq -r '.id')
        echo "Portfolio ID: $PORTFOLIO_ID"

        echo ""
        echo "5. Testing Portfolio Update with Audit Logging..."

        UPDATE_RESPONSE=$(curl -k -s -w "HTTPSTATUS:%{http_code}" \
          -X PUT "$API_URL/portfolio/$PORTFOLIO_ID" \
          -H "Content-Type: application/json" \
          -H "Authorization: Bearer $ACCESS_TOKEN" \
          -d '{
            "id": '$PORTFOLIO_ID',
            "title": "Updated Audit Portfolio",
            "description": "An updated portfolio for audit testing",
            "userId": "audit-test-user"
          }')

        HTTP_STATUS=$(echo $UPDATE_RESPONSE | tr -d '\n' | sed -e 's/.*HTTPSTATUS://')
        UPDATE_BODY=$(echo $UPDATE_RESPONSE | sed -e 's/HTTPSTATUS\:.*//g')

        echo "Portfolio Update HTTP Status: $HTTP_STATUS"
        if [ $HTTP_STATUS -eq 200 ]; then
            echo "✅ Portfolio updated successfully"
            echo "Response: $UPDATE_BODY"
        else
            echo "❌ Portfolio update failed"
            echo "Response: $UPDATE_BODY"
        fi

        echo ""
        echo "6. Testing Portfolio Deletion with Audit Logging..."

        DELETE_RESPONSE=$(curl -k -s -w "HTTPSTATUS:%{http_code}" \
          -X DELETE "$API_URL/portfolio/$PORTFOLIO_ID" \
          -H "Authorization: Bearer $ACCESS_TOKEN")

        HTTP_STATUS=$(echo $DELETE_RESPONSE | tr -d '\n' | sed -e 's/.*HTTPSTATUS://')

        echo "Portfolio Delete HTTP Status: $HTTP_STATUS"
        if [ $HTTP_STATUS -eq 204 ]; then
            echo "✅ Portfolio deleted successfully"
        else
            echo "❌ Portfolio deletion failed"
            echo "Response: $DELETE_RESPONSE"
        fi
    else
        echo "❌ Portfolio creation failed"
        echo "Response: $PORTFOLIO_BODY"
    fi

    echo ""
    echo "7. Testing Logout with Audit Logging..."

    LOGOUT_RESPONSE=$(curl -k -s -w "HTTPSTATUS:%{http_code}" \
      -X POST "$API_URL/account/hybrid-logout" \
      -H "Content-Type: application/json" \
      -H "Authorization: Bearer $ACCESS_TOKEN" \
      -d '{}')

    HTTP_STATUS=$(echo $LOGOUT_RESPONSE | tr -d '\n' | sed -e 's/.*HTTPSTATUS://')
    LOGOUT_BODY=$(echo $LOGOUT_RESPONSE | sed -e 's/HTTPSTATUS\:.*//g')

    echo "Logout HTTP Status: $HTTP_STATUS"
    if [ $HTTP_STATUS -eq 200 ]; then
        echo "✅ Logout successful"
        echo "Response: $LOGOUT_BODY"
    else
        echo "❌ Logout failed"
        echo "Response: $LOGOUT_BODY"
    fi
else
    echo "❌ Login failed"
    echo "Response: $LOGIN_BODY"
fi

echo ""
echo "8. Testing Multiple Failed Login Attempts (Suspicious Activity Detection)..."

for i in {1..4}
do
    echo "Failed attempt $i..."
    curl -k -s -X POST "$API_URL/account/login" \
      -H "Content-Type: application/json" \
      -d '{
        "emailOrUsername": "'$TEST_EMAIL'",
        "password": "WrongPassword123!"
      }' > /dev/null
    sleep 1
done

echo "✅ Multiple failed attempts completed (should trigger suspicious activity logging)"

echo ""
echo "=== Audit Logging Test Complete ==="
echo ""
echo "Expected audit entries in database:"
echo "1. Authentication audit logs for registration, failed logins, successful login, and logout"
echo "2. Failed login attempt tracking with multiple entries"
echo "3. User activity logs for portfolio creation, update, and deletion"
echo "4. Security audit log for suspicious activity (multiple failed logins)"
echo ""
echo "Check the ApplicationDbContext audit tables:"
echo "- AuthenticationAuditLogs"
echo "- FailedLoginAttempts"
echo "- UserActivityAuditLogs"
echo "- SecurityAuditLogs"
