
## สรุปสิ่งที่ได้:

### **📋 API Endpoints ทั้งหมด:**

**Portfolio Controller (9 endpoints):**
- Health check
- CRUD operations (Create, Read, Update, Delete)
- Get by user ID
- Get with projects
- Toggle visibility

**Project Controller (9 endpoints):**
- CRUD operations
- Get by portfolio ID
- Get active/completed projects
- Mark as completed

### **🧪 Test Script Features:**
1. **Step-by-step testing** - แบ่งเป็น steps ชัดเจน
2. **Complete coverage** - ครอบคลุมทุก API endpoints
3. **Real-world data** - ใช้ข้อมูลจริงที่มีความหมาย
4. **Error testing** - ทดสอบ error cases
5. **Cleanup testing** - ทดสอบการลบข้อมูล
6. **Quick script** - มี bash script แบบอัตโนมัติ

### **🎯 วิธีใช้:**
1. รัน API server ก่อน
2. Copy-paste curl commands ทีละ step
3. หรือใช้ Quick Test Script สำหรับทดสอบแบบอัตโนมัติ

Script นี้จะช่วยให้คุณทดสอบ API ได้อย่างครบถ้วนและเป็นระบบ! 🚀

//////////////////////////////////////////////////////////////////////

### 1. **รัน API:**
```bash
cd /Users/nevelopdevper/iDev/ABC/Backend/src
dotnet run --project API
```

### 2. **ทดสอบใน Swagger:**
- เปิด `https://localhost:5001/swagger`
- ทดสอบ endpoints ต่างๆ

### 3. **ทดสอบด้วย curl:**
```bash
# GET all portfolios
curl -X GET "https://localhost:5001/api/portfolio" -H "accept: application/json"

# GET portfolio by ID
curl -X GET "https://localhost:5001/api/portfolio/1" -H "accept: application/json"

# POST create portfolio
curl -X POST "https://localhost:5001/api/portfolio" \
  -H "accept: application/json" \
  -H "Content-Type: application/json" \
  -d '{
    "title": "My Portfolio",
    "description": "A portfolio description",
    "userId": "user123",
    "isPublic": true
  }'

# PUT update portfolio
curl -X PUT "https://localhost:5001/api/portfolio/1" \
  -H "accept: application/json" \
  -H "Content-Type: application/json" \
  -d '{
    "id": 1,
    "title": "Updated Portfolio",
    "description": "Updated description",
    "isPublic": false
  }'

# DELETE portfolio
curl -X DELETE "https://localhost:5001/api/portfolio/1" -H "accept: application/json"
```

## 📋 **API Endpoints ที่ได้:**

```
GET    /api/portfolio                     - Get all portfolios
GET    /api/portfolio/{id}                - Get portfolio by ID
GET    /api/portfolio/user/{userId}       - Get user's portfolios
GET    /api/portfolio/{id}/details        - Get portfolio with details
POST   /api/portfolio                     - Create new portfolio
PUT    /api/portfolio/{id}                - Update portfolio
DELETE /api/portfolio/{id}                - Delete portfolio
GET    /api/portfolio/ownership/{userId}/{portfolioId} - Check ownership
HEAD   /api/portfolio/{id}                - Check if portfolio exists




## 🧪 **6. ทดสอบ API:**

### **1. สร้าง Portfolio ก่อน:**
```bash
curl -X 'POST' 'http://localhost:5011/api/Portfolio' \
  -H 'Content-Type: application/json' \
  -d '{
    "title": "My Portfolio",
    "description": "Sample portfolio",
    "userId": "user123",
    "isPublic": true
  }'
```

### **2. เพิ่ม Project เข้า Portfolio:**
```bash
curl -X 'POST' 'http://localhost:5011/api/Project' \
  -H 'Content-Type: application/json' \
  -d '{
    "title": "ABC Portfolio System",
    "description": "A portfolio management system built with .NET Core and React",
    "projectUrl": "https://abc-portfolio.com",
    "gitHubUrl": "https://github.com/user/abc-portfolio",
    "startDate": "2024-01-01T00:00:00Z",
    "isCompleted": false,
    "portfolioId": 1
  }'
```

### **3. ดู Projects ใน Portfolio:**
```bash
curl -X 'GET' 'http://localhost:5011/api/Project/portfolio/1'
```

### **4. ทำโปรเจ็กต์เสร็จ:**
```bash
curl -X 'PATCH' 'http://localhost:5011/api/Project/1/complete' \
  -H 'Content-Type: application/json' \
  -d '{
    "endDate": "2024-06-26T00:00:00Z"
  }'
```

# 📋 **Complete API Testing with Bash Curl Commands**

## 🚀 **Start API Server First:**
```bash
cd /Users/nevelopdevper/iDev/ABC/Backend/src
dotnet run --project API
```

## 🧪 **Complete API Testing Script:**

### **Step 1: Portfolio Controller Tests**

#### **1.1 Health Check:**
```bash
# Test endpoint
curl -X 'GET' 'http://localhost:5011/api/Portfolio/test' \
  -H 'accept: application/json'
```

#### **1.2 Get All Portfolios (Initially Empty):**
```bash
# GET all portfolios
curl -X 'GET' 'http://localhost:5011/api/Portfolio' \
  -H 'accept: application/json'
```

#### **1.3 Create First Portfolio:**
```bash
# POST create portfolio
curl -X 'POST' 'http://localhost:5011/api/Portfolio' \
  -H 'accept: application/json' \
  -H 'Content-Type: application/json' \
  -d '{
    "title": "John Doe Portfolio",
    "description": "Full-stack developer portfolio showcasing web applications",
    "userId": "user123",
    "isPublic": true
  }'
```

#### **1.4 Create Second Portfolio:**
```bash
# POST create another portfolio
curl -X 'POST' 'http://localhost:5011/api/Portfolio' \
  -H 'accept: application/json' \
  -H 'Content-Type: application/json' \
  -d '{
    "title": "Private Projects",
    "description": "Collection of private development projects",
    "userId": "user123",
    "isPublic": false
  }'
```

#### **1.5 Create Third Portfolio (Different User):**
```bash
# POST create portfolio for different user
curl -X 'POST' 'http://localhost:5011/api/Portfolio' \
  -H 'accept: application/json' \
  -H 'Content-Type: application/json' \
  -d '{
    "title": "Mobile App Portfolio",
    "description": "Collection of mobile applications",
    "userId": "user456",
    "isPublic": true
  }'
```

#### **1.6 Get All Portfolios (Should Show 3):**
```bash
# GET all portfolios
curl -X 'GET' 'http://localhost:5011/api/Portfolio' \
  -H 'accept: application/json'
```

#### **1.7 Get Portfolio by ID:**
```bash
# GET portfolio by ID (ID 1)
curl -X 'GET' 'http://localhost:5011/api/Portfolio/1' \
  -H 'accept: application/json'

# GET portfolio by ID (ID 2)
curl -X 'GET' 'http://localhost:5011/api/Portfolio/2' \
  -H 'accept: application/json'
```

#### **1.8 Get Portfolios by User ID:**
```bash
# GET portfolios by user ID (user123)
curl -X 'GET' 'http://localhost:5011/api/Portfolio/user/user123' \
  -H 'accept: application/json'

# GET portfolios by user ID (user456)
curl -X 'GET' 'http://localhost:5011/api/Portfolio/user/user456' \
  -H 'accept: application/json'
```

#### **1.9 Update Portfolio:**
```bash
# PUT update portfolio (ID 1)
curl -X 'PUT' 'http://localhost:5011/api/Portfolio/1' \
  -H 'accept: application/json' \
  -H 'Content-Type: application/json' \
  -d '{
    "id": 1,
    "title": "Updated John Doe Portfolio",
    "description": "Senior Full-stack developer portfolio with 5+ years experience",
    "isPublic": true
  }'
```

#### **1.10 Toggle Portfolio Visibility:**
```bash
# PATCH toggle visibility (make portfolio 1 private)
curl -X 'PATCH' 'http://localhost:5011/api/Portfolio/1/toggle-visibility' \
  -H 'accept: application/json'

# Check the result
curl -X 'GET' 'http://localhost:5011/api/Portfolio/1' \
  -H 'accept: application/json'

# PATCH toggle visibility again (make it public again)
curl -X 'PATCH' 'http://localhost:5011/api/Portfolio/1/toggle-visibility' \
  -H 'accept: application/json'
```

---

### **Step 2: Project Controller Tests**

#### **2.1 Get All Projects (Initially Empty):**
```bash
# GET all projects
curl -X 'GET' 'http://localhost:5011/api/Project' \
  -H 'accept: application/json'
```

#### **2.2 Create Projects for Portfolio 1:**
```bash
# POST create first project in portfolio 1
curl -X 'POST' 'http://localhost:5011/api/Project' \
  -H 'accept: application/json' \
  -H 'Content-Type: application/json' \
  -d '{
    "title": "E-commerce Website",
    "description": "Full-stack e-commerce platform built with ASP.NET Core and React",
    "projectUrl": "https://ecommerce-demo.com",
    "gitHubUrl": "https://github.com/johndoe/ecommerce-site",
    "startDate": "2024-01-15T00:00:00Z",
    "isCompleted": false,
    "portfolioId": 1
  }'

# POST create second project in portfolio 1
curl -X 'POST' 'http://localhost:5011/api/Project' \
  -H 'accept: application/json' \
  -H 'Content-Type: application/json' \
  -d '{
    "title": "Task Management API",
    "description": "RESTful API for task management with authentication and authorization",
    "projectUrl": "https://taskapi-demo.com",
    "gitHubUrl": "https://github.com/johndoe/task-api",
    "startDate": "2024-03-01T00:00:00Z",
    "endDate": "2024-05-15T00:00:00Z",
    "isCompleted": true,
    "portfolioId": 1
  }'

# POST create third project in portfolio 1
curl -X 'POST' 'http://localhost:5011/api/Project' \
  -H 'accept: application/json' \
  -H 'Content-Type: application/json' \
  -d '{
    "title": "Weather Dashboard",
    "description": "Real-time weather monitoring dashboard with charts and analytics",
    "projectUrl": "https://weather-dash.com",
    "gitHubUrl": "https://github.com/johndoe/weather-dashboard",
    "startDate": "2024-04-10T00:00:00Z",
    "isCompleted": false,
    "portfolioId": 1
  }'
```

#### **2.3 Create Projects for Portfolio 2:**
```bash
# POST create project in portfolio 2
curl -X 'POST' 'http://localhost:5011/api/Project' \
  -H 'accept: application/json' \
  -H 'Content-Type: application/json' \
  -d '{
    "title": "Internal CRM System",
    "description": "Customer relationship management system for internal use",
    "projectUrl": "",
    "gitHubUrl": "https://github.com/johndoe/internal-crm",
    "startDate": "2024-02-01T00:00:00Z",
    "isCompleted": false,
    "portfolioId": 2
  }'
```

#### **2.4 Create Projects for Portfolio 3:**
```bash
# POST create project in portfolio 3
curl -X 'POST' 'http://localhost:5011/api/Project' \
  -H 'accept: application/json' \
  -H 'Content-Type: application/json' \
  -d '{
    "title": "Flutter Shopping App",
    "description": "Cross-platform mobile shopping application built with Flutter",
    "projectUrl": "https://play.google.com/store/apps/details?id=com.example.shop",
    "gitHubUrl": "https://github.com/user456/flutter-shop",
    "startDate": "2024-01-01T00:00:00Z",
    "endDate": "2024-04-30T00:00:00Z",
    "isCompleted": true,
    "portfolioId": 3
  }'
```

#### **2.5 Get All Projects (Should Show 5):**
```bash
# GET all projects
curl -X 'GET' 'http://localhost:5011/api/Project' \
  -H 'accept: application/json'
```

#### **2.6 Get Project by ID:**
```bash
# GET project by ID (ID 1)
curl -X 'GET' 'http://localhost:5011/api/Project/1' \
  -H 'accept: application/json'

# GET project by ID (ID 2)
curl -X 'GET' 'http://localhost:5011/api/Project/2' \
  -H 'accept: application/json'
```

#### **2.7 Get Projects by Portfolio ID:**
```bash
# GET projects in portfolio 1
curl -X 'GET' 'http://localhost:5011/api/Project/portfolio/1' \
  -H 'accept: application/json'

# GET projects in portfolio 2
curl -X 'GET' 'http://localhost:5011/api/Project/portfolio/2' \
  -H 'accept: application/json'

# GET projects in portfolio 3
curl -X 'GET' 'http://localhost:5011/api/Project/portfolio/3' \
  -H 'accept: application/json'
```

#### **2.8 Get Active Projects by Portfolio:**
```bash
# GET active projects in portfolio 1
curl -X 'GET' 'http://localhost:5011/api/Project/portfolio/1/active' \
  -H 'accept: application/json'

# GET active projects in portfolio 2
curl -X 'GET' 'http://localhost:5011/api/Project/portfolio/2/active' \
  -H 'accept: application/json'
```

#### **2.9 Get Completed Projects by Portfolio:**
```bash
# GET completed projects in portfolio 1
curl -X 'GET' 'http://localhost:5011/api/Project/portfolio/1/completed' \
  -H 'accept: application/json'

# GET completed projects in portfolio 3
curl -X 'GET' 'http://localhost:5011/api/Project/portfolio/3/completed' \
  -H 'accept: application/json'
```

#### **2.10 Update Project:**
```bash
# PUT update project (ID 1)
curl -X 'PUT' 'http://localhost:5011/api/Project/1' \
  -H 'accept: application/json' \
  -H 'Content-Type: application/json' \
  -d '{
    "id": 1,
    "title": "Advanced E-commerce Platform",
    "description": "Enterprise-level e-commerce platform with microservices architecture, built with ASP.NET Core and React",
    "projectUrl": "https://advanced-ecommerce-demo.com",
    "gitHubUrl": "https://github.com/johndoe/advanced-ecommerce",
    "startDate": "2024-01-15T00:00:00Z",
    "isCompleted": false,
    "portfolioId": 1
  }'
```

#### **2.11 Complete Project:**
```bash
# PATCH complete project (ID 1)
curl -X 'PATCH' 'http://localhost:5011/api/Project/1/complete' \
  -H 'accept: application/json' \
  -H 'Content-Type: application/json' \
  -d '{
    "endDate": "2024-06-26T00:00:00Z"
  }'

# Check the result
curl -X 'GET' 'http://localhost:5011/api/Project/1' \
  -H 'accept: application/json'

# PATCH complete project (ID 3)
curl -X 'PATCH' 'http://localhost:5011/api/Project/3/complete' \
  -H 'accept: application/json' \
  -H 'Content-Type: application/json' \
  -d '{
    "endDate": "2024-06-26T00:00:00Z"
  }'
```

---

### **Step 3: Portfolio with Projects Tests**

#### **3.1 Get Portfolio with Projects:**
```bash
# GET portfolio 1 with projects
curl -X 'GET' 'http://localhost:5011/api/Portfolio/1/with-projects' \
  -H 'accept: application/json'

# GET portfolio 2 with projects
curl -X 'GET' 'http://localhost:5011/api/Portfolio/2/with-projects' \
  -H 'accept: application/json'

# GET portfolio 3 with projects
curl -X 'GET' 'http://localhost:5011/api/Portfolio/3/with-projects' \
  -H 'accept: application/json'
```

---

### **Step 4: Error Testing (Optional)**

#### **4.1 Test Invalid Requests:**
```bash
# GET non-existent portfolio
curl -X 'GET' 'http://localhost:5011/api/Portfolio/999' \
  -H 'accept: application/json'

# GET non-existent project
curl -X 'GET' 'http://localhost:5011/api/Project/999' \
  -H 'accept: application/json'

# POST invalid portfolio (missing required fields)
curl -X 'POST' 'http://localhost:5011/api/Portfolio' \
  -H 'accept: application/json' \
  -H 'Content-Type: application/json' \
  -d '{
    "title": "",
    "description": "",
    "userId": "",
    "isPublic": true
  }'

# POST project with invalid portfolio ID
curl -X 'POST' 'http://localhost:5011/api/Project' \
  -H 'accept: application/json' \
  -H 'Content-Type: application/json' \
  -d '{
    "title": "Test Project",
    "description": "Test description",
    "startDate": "2024-01-01T00:00:00Z",
    "isCompleted": false,
    "portfolioId": 999
  }'
```

---

### **Step 5: Cleanup Tests (Delete Data)**

#### **5.1 Delete Projects:**
```bash
# DELETE project 1
curl -X 'DELETE' 'http://localhost:5011/api/Project/1' \
  -H 'accept: application/json'

# DELETE project 2
curl -X 'DELETE' 'http://localhost:5011/api/Project/2' \
  -H 'accept: application/json'

# DELETE project 3
curl -X 'DELETE' 'http://localhost:5011/api/Project/3' \
  -H 'accept: application/json'

# DELETE project 4
curl -X 'DELETE' 'http://localhost:5011/api/Project/4' \
  -H 'accept: application/json'

# DELETE project 5
curl -X 'DELETE' 'http://localhost:5011/api/Project/5' \
  -H 'accept: application/json'
```

#### **5.2 Delete Portfolios:**
```bash
# DELETE portfolio 1
curl -X 'DELETE' 'http://localhost:5011/api/Portfolio/1' \
  -H 'accept: application/json'

# DELETE portfolio 2
curl -X 'DELETE' 'http://localhost:5011/api/Portfolio/2' \
  -H 'accept: application/json'

# DELETE portfolio 3
curl -X 'DELETE' 'http://localhost:5011/api/Portfolio/3' \
  -H 'accept: application/json'
```

#### **5.3 Verify Cleanup:**
```bash
# Verify all portfolios deleted
curl -X 'GET' 'http://localhost:5011/api/Portfolio' \
  -H 'accept: application/json'

# Verify all projects deleted
curl -X 'GET' 'http://localhost:5011/api/Project' \
  -H 'accept: application/json'
```

---

## 📝 **API Endpoints Summary:**

### **Portfolio Controller:**
- `GET /api/Portfolio/test` - Health check
- `GET /api/Portfolio` - Get all portfolios
- `GET /api/Portfolio/{id}` - Get portfolio by ID
- `GET /api/Portfolio/user/{userId}` - Get portfolios by user ID
- `GET /api/Portfolio/{id}/with-projects` - Get portfolio with projects
- `POST /api/Portfolio` - Create new portfolio
- `PUT /api/Portfolio/{id}` - Update portfolio
- `PATCH /api/Portfolio/{id}/toggle-visibility` - Toggle portfolio visibility
- `DELETE /api/Portfolio/{id}` - Delete portfolio

### **Project Controller:**
- `GET /api/Project` - Get all projects
- `GET /api/Project/{id}` - Get project by ID
- `GET /api/Project/portfolio/{portfolioId}` - Get projects by portfolio ID
- `GET /api/Project/portfolio/{portfolioId}/active` - Get active projects by portfolio
- `GET /api/Project/portfolio/{portfolioId}/completed` - Get completed projects by portfolio
- `POST /api/Project` - Create new project
- `PUT /api/Project/{id}` - Update project
- `PATCH /api/Project/{id}/complete` - Mark project as completed
- `DELETE /api/Project/{id}` - Delete project

---

## 🎯 **Quick Test Script (All in One):**

```bash
#!/bin/bash

# Colors for output
RED='\033[0;31m'
GREEN='\033[0;32m'
YELLOW='\033[1;33m'
NC='\033[0m' # No Color

BASE_URL="http://localhost:5011/api"

echo -e "${GREEN}🚀 Starting ABC Portfolio API Testing...${NC}"

# Test health check
echo -e "\n${YELLOW}Testing health check...${NC}"
curl -s -X GET "$BASE_URL/Portfolio/test" | jq .

# Create portfolios
echo -e "\n${YELLOW}Creating test portfolios...${NC}"
PORTFOLIO1=$(curl -s -X POST "$BASE_URL/Portfolio" \
  -H "Content-Type: application/json" \
  -d '{"title":"Test Portfolio 1","description":"Test description","userId":"user123","isPublic":true}' | jq -r '.id')

PORTFOLIO2=$(curl -s -X POST "$BASE_URL/Portfolio" \
  -H "Content-Type: application/json" \
  -d '{"title":"Test Portfolio 2","description":"Test description","userId":"user123","isPublic":false}' | jq -r '.id')

echo "Created portfolios with IDs: $PORTFOLIO1, $PORTFOLIO2"

# Create projects
echo -e "\n${YELLOW}Creating test projects...${NC}"
PROJECT1=$(curl -s -X POST "$BASE_URL/Project" \
  -H "Content-Type: application/json" \
  -d "{\"title\":\"Test Project 1\",\"description\":\"Test description\",\"startDate\":\"2024-01-01T00:00:00Z\",\"isCompleted\":false,\"portfolioId\":$PORTFOLIO1}" | jq -r '.id')

PROJECT2=$(curl -s -X POST "$BASE_URL/Project" \
  -H "Content-Type: application/json" \
  -d "{\"title\":\"Test Project 2\",\"description\":\"Test description\",\"startDate\":\"2024-01-01T00:00:00Z\",\"isCompleted\":false,\"portfolioId\":$PORTFOLIO1}" | jq -r '.id')

echo "Created projects with IDs: $PROJECT1, $PROJECT2"

# Test all endpoints
echo -e "\n${YELLOW}Testing all endpoints...${NC}"
echo "GET all portfolios:"
curl -s -X GET "$BASE_URL/Portfolio" | jq .

echo -e "\nGET all projects:"
curl -s -X GET "$BASE_URL/Project" | jq .

echo -e "\nGET portfolio with projects:"
curl -s -X GET "$BASE_URL/Portfolio/$PORTFOLIO1/with-projects" | jq .

echo -e "\n${GREEN}✅ API testing completed!${NC}"
```
