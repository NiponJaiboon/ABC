#!/bin/bash

# Colors for output
RED='\033[0;31m'
GREEN='\033[0;32m'
YELLOW='\033[1;33m'
BLUE='\033[0;34m'
NC='\033[0m' # No Color

BASE_URL="http://localhost:5001/api"

echo -e "${GREEN}🚀 Starting ABC Portfolio API Testing...${NC}"
echo -e "${BLUE}================================================${NC}"

# Function to check if API is running
check_api() {
    echo -e "\n${YELLOW}Checking if API is running...${NC}"
    if curl -s -f "$BASE_URL/Portfolio/test" > /dev/null; then
        echo -e "${GREEN}✅ API is running!${NC}"
        return 0
    else
        echo -e "${RED}❌ API is not running. Please start the API first.${NC}"
        echo -e "${YELLOW}Run: cd /Users/nevelopdevper/iDev/ABC/Backend/src && dotnet run --project API${NC}"
        exit 1
    fi
}

# Function to display step
display_step() {
    echo -e "\n${BLUE}🔹 $1${NC}"
    echo -e "${BLUE}----------------------------------------${NC}"
}

# Check if API is running
check_api

# Test health check
display_step "Step 1: Testing Health Check"
curl -s -X GET "$BASE_URL/Portfolio/test" | jq .

# Get initial state (should be empty)
display_step "Step 2: Check Initial State"
echo "Initial Portfolios:"
curl -s -X GET "$BASE_URL/Portfolio" | jq .
echo -e "\nInitial Projects:"
curl -s -X GET "$BASE_URL/Project" | jq .

# Create portfolios
display_step "Step 3: Creating Test Portfolios"
echo "Creating Portfolio 1..."
PORTFOLIO1=$(curl -s -X POST "$BASE_URL/Portfolio" \
  -H "Content-Type: application/json" \
  -d '{
    "title": "John Doe Portfolio",
    "description": "Full-stack developer portfolio",
    "userId": "user123",
    "isPublic": true
  }' | jq -r '.id')

echo "Creating Portfolio 2..."
PORTFOLIO2=$(curl -s -X POST "$BASE_URL/Portfolio" \
  -H "Content-Type: application/json" \
  -d '{
    "title": "Private Projects",
    "description": "Collection of private projects",
    "userId": "user123",
    "isPublic": false
  }' | jq -r '.id')

echo "Creating Portfolio 3..."
PORTFOLIO3=$(curl -s -X POST "$BASE_URL/Portfolio" \
  -H "Content-Type: application/json" \
  -d '{
    "title": "Mobile Apps",
    "description": "Mobile application portfolio",
    "userId": "user456",
    "isPublic": true
  }' | jq -r '.id')

echo -e "${GREEN}Created portfolios with IDs: $PORTFOLIO1, $PORTFOLIO2, $PORTFOLIO3${NC}"

# Create projects
display_step "Step 4: Creating Test Projects"
echo "Creating Project 1 in Portfolio $PORTFOLIO1..."
PROJECT1=$(curl -s -X POST "$BASE_URL/Project" \
  -H "Content-Type: application/json" \
  -d "{
    \"title\": \"E-commerce Website\",
    \"description\": \"Full-stack e-commerce platform\",
    \"projectUrl\": \"https://ecommerce-demo.com\",
    \"gitHubUrl\": \"https://github.com/johndoe/ecommerce\",
    \"startDate\": \"2024-01-01T00:00:00Z\",
    \"isCompleted\": false,
    \"portfolioId\": $PORTFOLIO1
  }" | jq -r '.id')

echo "Creating Project 2 in Portfolio $PORTFOLIO1..."
PROJECT2=$(curl -s -X POST "$BASE_URL/Project" \
  -H "Content-Type: application/json" \
  -d "{
    \"title\": \"Task Management API\",
    \"description\": \"RESTful API for task management\",
    \"projectUrl\": \"https://taskapi-demo.com\",
    \"gitHubUrl\": \"https://github.com/johndoe/task-api\",
    \"startDate\": \"2024-02-01T00:00:00Z\",
    \"endDate\": \"2024-04-01T00:00:00Z\",
    \"isCompleted\": true,
    \"portfolioId\": $PORTFOLIO1
  }" | jq -r '.id')

echo "Creating Project 3 in Portfolio $PORTFOLIO2..."
PROJECT3=$(curl -s -X POST "$BASE_URL/Project" \
  -H "Content-Type: application/json" \
  -d "{
    \"title\": \"Internal CRM\",
    \"description\": \"Customer relationship management system\",
    \"gitHubUrl\": \"https://github.com/johndoe/crm\",
    \"startDate\": \"2024-03-01T00:00:00Z\",
    \"isCompleted\": false,
    \"portfolioId\": $PORTFOLIO2
  }" | jq -r '.id')

echo -e "${GREEN}Created projects with IDs: $PROJECT1, $PROJECT2, $PROJECT3${NC}"

# Test all endpoints
display_step "Step 5: Testing All Endpoints"

echo "GET all portfolios:"
curl -s -X GET "$BASE_URL/Portfolio" | jq '.[] | {id, title, isPublic, userId}'

echo -e "\nGET all projects:"
curl -s -X GET "$BASE_URL/Project" | jq '.[] | {id, title, isCompleted, portfolioId}'

echo -e "\nGET portfolios by user (user123):"
curl -s -X GET "$BASE_URL/Portfolio/user/user123" | jq '.[] | {id, title, isPublic}'

echo -e "\nGET projects in portfolio $PORTFOLIO1:"
curl -s -X GET "$BASE_URL/Project/portfolio/$PORTFOLIO1" | jq '.[] | {id, title, isCompleted}'

echo -e "\nGET active projects in portfolio $PORTFOLIO1:"
curl -s -X GET "$BASE_URL/Project/portfolio/$PORTFOLIO1/active" | jq '.[] | {id, title}'

echo -e "\nGET completed projects in portfolio $PORTFOLIO1:"
curl -s -X GET "$BASE_URL/Project/portfolio/$PORTFOLIO1/completed" | jq '.[] | {id, title}'

echo -e "\nGET portfolio $PORTFOLIO1 with projects:"
curl -s -X GET "$BASE_URL/Portfolio/$PORTFOLIO1/with-projects" | jq '{id, title, projects: .projects[] | {id, title, isCompleted}}'

# Test updates
display_step "Step 6: Testing Updates"

echo "Updating Portfolio $PORTFOLIO1..."
curl -s -X PUT "$BASE_URL/Portfolio/$PORTFOLIO1" \
  -H "Content-Type: application/json" \
  -d "{
    \"id\": $PORTFOLIO1,
    \"title\": \"Updated John Doe Portfolio\",
    \"description\": \"Senior Full-stack developer portfolio with 5+ years experience\",
    \"isPublic\": true
  }" | jq '{id, title, description}'

echo -e "\nToggling visibility of Portfolio $PORTFOLIO1..."
curl -s -X PATCH "$BASE_URL/Portfolio/$PORTFOLIO1/toggle-visibility" | jq '{id, title, isPublic}'

echo -e "\nCompleting Project $PROJECT1..."
curl -s -X PATCH "$BASE_URL/Project/$PROJECT1/complete" \
  -H "Content-Type: application/json" \
  -d '{
    "endDate": "2024-06-26T00:00:00Z"
  }' | jq '{id, title, isCompleted, endDate}'

# Test error cases
display_step "Step 7: Testing Error Cases"

echo "Testing non-existent portfolio (ID 999):"
curl -s -X GET "$BASE_URL/Portfolio/999" | jq .

echo -e "\nTesting non-existent project (ID 999):"
curl -s -X GET "$BASE_URL/Project/999" | jq .

echo -e "\nTesting invalid portfolio creation:"
curl -s -X POST "$BASE_URL/Portfolio" \
  -H "Content-Type: application/json" \
  -d '{
    "title": "",
    "description": "",
    "userId": "",
    "isPublic": true
  }' | jq .

# Summary
display_step "Step 8: Final Summary"

echo "Final Portfolios:"
curl -s -X GET "$BASE_URL/Portfolio" | jq '.[] | {id, title, isPublic, userId, createdAt}'

echo -e "\nFinal Projects:"
curl -s -X GET "$BASE_URL/Project" | jq '.[] | {id, title, isCompleted, portfolioId, startDate, endDate}'

echo -e "\n${GREEN}🎉 API testing completed successfully!${NC}"
echo -e "${BLUE}================================================${NC}"

# Optional: Cleanup (uncomment if you want to clean up test data)
# display_step "Step 9: Cleanup (Optional)"
# echo "Deleting test projects..."
# curl -s -X DELETE "$BASE_URL/Project/$PROJECT1"
# curl -s -X DELETE "$BASE_URL/Project/$PROJECT2"
# curl -s -X DELETE "$BASE_URL/Project/$PROJECT3"
#
# echo "Deleting test portfolios..."
# curl -s -X DELETE "$BASE_URL/Portfolio/$PORTFOLIO1"
# curl -s -X DELETE "$BASE_URL/Portfolio/$PORTFOLIO2"
# curl -s -X DELETE "$BASE_URL/Portfolio/$PORTFOLIO3"
#
# echo -e "${GREEN}✅ Cleanup completed!${NC}"
