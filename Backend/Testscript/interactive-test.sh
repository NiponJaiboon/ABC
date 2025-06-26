#!/bin/bash

# Colors for output
RED='\033[0;31m'
GREEN='\033[0;32m'
YELLOW='\033[1;33m'
BLUE='\033[0;34m'
NC='\033[0m' # No Color

BASE_URL="http://localhost:5011/api"

# Function to wait for user input
wait_for_enter() {
    echo -e "\n${YELLOW}Press Enter to continue...${NC}"
    read
}

# Function to display step
display_step() {
    clear
    echo -e "${BLUE}================================================${NC}"
    echo -e "${GREEN}🚀 ABC Portfolio API Testing${NC}"
    echo -e "${BLUE}================================================${NC}"
    echo -e "\n${BLUE}🔹 $1${NC}"
    echo -e "${BLUE}----------------------------------------${NC}"
}

echo -e "${GREEN}🎯 Interactive API Testing Script${NC}"
echo -e "${YELLOW}This script will test all API endpoints step by step.${NC}"
echo -e "${YELLOW}Make sure your API is running on http://localhost:5011${NC}"
wait_for_enter

# Check API health
display_step "Step 1: Health Check"
echo "Testing API health..."
curl -s -X GET "$BASE_URL/Portfolio/test" | jq .
wait_for_enter

# Create portfolios
display_step "Step 2: Create Portfolios"
echo "Creating test portfolios..."

echo -e "\n${YELLOW}Creating Portfolio 1 (John Doe)...${NC}"
PORTFOLIO1=$(curl -s -X POST "$BASE_URL/Portfolio" \
  -H "Content-Type: application/json" \
  -d '{
    "title": "John Doe Portfolio",
    "description": "Full-stack developer portfolio",
    "userId": "user123",
    "isPublic": true
  }' | jq .)
echo "$PORTFOLIO1"
P1_ID=$(echo "$PORTFOLIO1" | jq -r '.id')

echo -e "\n${YELLOW}Creating Portfolio 2 (Private)...${NC}"
PORTFOLIO2=$(curl -s -X POST "$BASE_URL/Portfolio" \
  -H "Content-Type: application/json" \
  -d '{
    "title": "Private Projects",
    "description": "Collection of private projects",
    "userId": "user123",
    "isPublic": false
  }' | jq .)
echo "$PORTFOLIO2"
P2_ID=$(echo "$PORTFOLIO2" | jq -r '.id')

echo -e "\n${GREEN}Created portfolios with IDs: $P1_ID, $P2_ID${NC}"
wait_for_enter

# Create projects
display_step "Step 3: Create Projects"
echo "Creating test projects..."

echo -e "\n${YELLOW}Creating Project 1 in Portfolio $P1_ID...${NC}"
PROJECT1=$(curl -s -X POST "$BASE_URL/Project" \
  -H "Content-Type: application/json" \
  -d "{
    \"title\": \"E-commerce Website\",
    \"description\": \"Full-stack e-commerce platform\",
    \"projectUrl\": \"https://ecommerce-demo.com\",
    \"gitHubUrl\": \"https://github.com/johndoe/ecommerce\",
    \"startDate\": \"2024-01-01T00:00:00Z\",
    \"isCompleted\": false,
    \"portfolioId\": $P1_ID
  }" | jq .)
echo "$PROJECT1"
PR1_ID=$(echo "$PROJECT1" | jq -r '.id')

echo -e "\n${YELLOW}Creating Project 2 (Completed) in Portfolio $P1_ID...${NC}"
PROJECT2=$(curl -s -X POST "$BASE_URL/Project" \
  -H "Content-Type: application/json" \
  -d "{
    \"title\": \"Task Management API\",
    \"description\": \"RESTful API for task management\",
    \"startDate\": \"2024-02-01T00:00:00Z\",
    \"endDate\": \"2024-04-01T00:00:00Z\",
    \"isCompleted\": true,
    \"portfolioId\": $P1_ID
  }" | jq .)
echo "$PROJECT2"
PR2_ID=$(echo "$PROJECT2" | jq -r '.id')

echo -e "\n${GREEN}Created projects with IDs: $PR1_ID, $PR2_ID${NC}"
wait_for_enter

# Test GET endpoints
display_step "Step 4: Test GET Endpoints"

echo -e "${YELLOW}GET all portfolios:${NC}"
curl -s -X GET "$BASE_URL/Portfolio" | jq '.[] | {id, title, isPublic, userId}'
wait_for_enter

echo -e "${YELLOW}GET all projects:${NC}"
curl -s -X GET "$BASE_URL/Project" | jq '.[] | {id, title, isCompleted, portfolioId}'
wait_for_enter

echo -e "${YELLOW}GET portfolio $P1_ID with projects:${NC}"
curl -s -X GET "$BASE_URL/Portfolio/$P1_ID/with-projects" | jq .
wait_for_enter

# Test updates
display_step "Step 5: Test Updates"

echo -e "${YELLOW}Updating Portfolio $P1_ID...${NC}"
curl -s -X PUT "$BASE_URL/Portfolio/$P1_ID" \
  -H "Content-Type: application/json" \
  -d "{
    \"id\": $P1_ID,
    \"title\": \"Updated John Doe Portfolio\",
    \"description\": \"Senior Full-stack developer with 5+ years experience\",
    \"isPublic\": true
  }" | jq .
wait_for_enter

echo -e "${YELLOW}Completing Project $PR1_ID...${NC}"
curl -s -X PATCH "$BASE_URL/Project/$PR1_ID/complete" \
  -H "Content-Type: application/json" \
  -d '{
    "endDate": "2024-06-26T00:00:00Z"
  }' | jq .
wait_for_enter

echo -e "${YELLOW}Toggling visibility of Portfolio $P1_ID...${NC}"
curl -s -X PATCH "$BASE_URL/Portfolio/$P1_ID/toggle-visibility" | jq .
wait_for_enter

# Final summary
display_step "Step 6: Final Summary"

echo -e "${YELLOW}Final state - All portfolios:${NC}"
curl -s -X GET "$BASE_URL/Portfolio" | jq .

echo -e "\n${YELLOW}Final state - All projects:${NC}"
curl -s -X GET "$BASE_URL/Project" | jq .

echo -e "\n${GREEN}🎉 Interactive testing completed!${NC}"
echo -e "${BLUE}You can now manually test other endpoints or run cleanup.${NC}"

# Optional cleanup
echo -e "\n${YELLOW}Do you want to cleanup test data? (y/n):${NC}"
read -r cleanup

if [ "$cleanup" = "y" ] || [ "$cleanup" = "Y" ]; then
    echo -e "\n${YELLOW}Cleaning up test data...${NC}"
    curl -s -X DELETE "$BASE_URL/Project/$PR1_ID"
    curl -s -X DELETE "$BASE_URL/Project/$PR2_ID"
    curl -s -X DELETE "$BASE_URL/Portfolio/$P1_ID"
    curl -s -X DELETE "$BASE_URL/Portfolio/$P2_ID"
    echo -e "${GREEN}✅ Cleanup completed!${NC}"
else
    echo -e "${BLUE}Test data preserved for manual testing.${NC}"
fi
