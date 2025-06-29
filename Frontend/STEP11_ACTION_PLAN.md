# STEP 11 ACTION PLAN: Frontend Authentication Integration

## 🎯 Overview
STEP 11 จะดำเนินการรวม Frontend (Next.js) เข้ากับ Backend Authentication System ที่สร้างขึ้นใน STEP 1-10 โดยใช้ OAuth 2.0 / OpenID Connect protocol

---

## 📋 Current Status

### ✅ Completed (STEP 1-10)
- Backend API พร้อม OAuth 2.0 provider
- OpenIddict integration
- JWT token system
- User registration และ authentication
- API authorization middleware
- External authentication support
- Database schema และ migrations
- API endpoints tested และ validated

### 🔧 Ready for Implementation
- Next.js 15 project structure
- Architecture planning
- Dependencies identified
- Configuration templates prepared

---

## 🚀 STEP 11 Implementation Plan

### Phase 1: Project Setup & Dependencies (1-2 hours)

#### 1.1 Environment Setup
```bash
cd /Users/nevelopdevper/iDev/ABC/Frontend/abc-client

# Install core dependencies
npm install next@^15.3.4 react@^18.3.1 react-dom@^18.3.1 typescript@^5.7.3

# Install authentication dependencies
npm install next-auth@^4.24.10 axios@^1.7.9 @tanstack/react-query@^5.68.1

# Install UI dependencies
npm install @headlessui/react@^2.2.0 @heroicons/react@^2.2.0 clsx@^2.1.1

# Install form handling
npm install react-hook-form@^7.54.2 @hookform/resolvers@^3.10.0 zod@^3.24.1

# Install dev dependencies
npm install -D @types/node@^22.10.5 @types/react@^18.3.17 @types/react-dom@^18.3.5
```

#### 1.2 Configuration Files
- [ ] Update `tsconfig.json` with path aliases
- [ ] Configure `next.config.ts` for API proxy
- [ ] Setup `.env.local` with API endpoints
- [ ] Configure ESLint และ Prettier

#### 1.3 Folder Structure Creation
```
src/
├── app/
│   ├── auth/
│   │   ├── login/
│   │   ├── register/
│   │   ├── callback/
│   │   └── logout/
│   ├── dashboard/
│   ├── portfolio/
│   └── api/
├── components/
│   ├── auth/
│   ├── ui/
│   ├── forms/
│   └── layout/
├── lib/
│   ├── auth/
│   ├── api/
│   └── utils/
├── hooks/
├── types/
├── stores/
└── utils/
```

### Phase 2: Authentication Implementation (3-4 hours)

#### 2.1 API Client Setup
- [ ] สร้าง Axios instance พร้อม interceptors
- [ ] ตั้งค่า base URL และ headers
- [ ] เพิ่ม token refresh logic
- [ ] Error handling และ retry logic

#### 2.2 Authentication Service
- [ ] OAuth 2.0 client implementation
- [ ] Login/logout functions
- [ ] Token management (storage, refresh)
- [ ] User session handling

#### 2.3 NextAuth.js Configuration
```typescript
// lib/auth/nextauth.ts
import NextAuth from 'next-auth'

export const authOptions = {
  providers: [
    {
      id: "abc-oauth",
      name: "ABC Portfolio System",
      type: "oauth",
      authorization: {
        url: "http://localhost:5001/connect/authorize",
        params: {
          scope: "openid profile email api_access",
          response_type: "code",
          client_id: process.env.NEXT_PUBLIC_OAUTH_CLIENT_ID,
        }
      },
      token: "http://localhost:5001/connect/token",
      userinfo: "http://localhost:5001/connect/userinfo",
      clientId: process.env.NEXT_PUBLIC_OAUTH_CLIENT_ID,
      clientSecret: process.env.OAUTH_CLIENT_SECRET,
    }
  ],
  callbacks: {
    async jwt({ token, account }) {
      if (account) {
        token.accessToken = account.access_token
      }
      return token
    },
    async session({ session, token }) {
      session.accessToken = token.accessToken
      return session
    }
  }
}
```

#### 2.4 Protected Route Implementation
- [ ] HOC สำหรับ protected pages
- [ ] Middleware สำหรับ route protection
- [ ] Loading states และ error handling

### Phase 3: UI Components Development (2-3 hours)

#### 3.1 Authentication Components
- [ ] Login form component
- [ ] Registration form component
- [ ] Logout button component
- [ ] User profile dropdown

#### 3.2 Layout Components
- [ ] Navigation bar พร้อม auth state
- [ ] Protected page layout
- [ ] Loading และ error states

#### 3.3 Form Components
```typescript
// components/auth/LoginForm.tsx
'use client'

import { useForm } from 'react-hook-form'
import { zodResolver } from '@hookform/resolvers/zod'
import { z } from 'zod'

const loginSchema = z.object({
  email: z.string().email('Invalid email address'),
  password: z.string().min(6, 'Password must be at least 6 characters')
})

type LoginFormData = z.infer<typeof loginSchema>

export function LoginForm() {
  const { register, handleSubmit, formState: { errors } } = useForm<LoginFormData>({
    resolver: zodResolver(loginSchema)
  })

  const onSubmit = async (data: LoginFormData) => {
    // Handle login logic
  }

  return (
    <form onSubmit={handleSubmit(onSubmit)}>
      {/* Form implementation */}
    </form>
  )
}
```

### Phase 4: State Management & Data Fetching (2-3 hours)

#### 4.1 Zustand Store Setup
```typescript
// stores/authStore.ts
import { create } from 'zustand'

interface User {
  id: string
  email: string
  name: string
}

interface AuthState {
  user: User | null
  token: string | null
  isAuthenticated: boolean
  login: (email: string, password: string) => Promise<void>
  logout: () => void
  refreshToken: () => Promise<void>
}

export const useAuthStore = create<AuthState>((set, get) => ({
  user: null,
  token: null,
  isAuthenticated: false,
  login: async (email, password) => {
    // Login implementation
  },
  logout: () => {
    // Logout implementation
  },
  refreshToken: async () => {
    // Token refresh implementation
  }
}))
```

#### 4.2 React Query Setup
- [ ] QueryClient configuration
- [ ] API query hooks
- [ ] Mutation hooks for auth operations
- [ ] Cache invalidation strategies

### Phase 5: Testing & Integration (1-2 hours)

#### 5.1 Frontend Testing
- [ ] Test login flow
- [ ] Test registration flow
- [ ] Test protected routes
- [ ] Test token refresh
- [ ] Test logout functionality

#### 5.2 Backend Integration Testing
- [ ] Verify OAuth flow with backend
- [ ] Test API calls with authentication
- [ ] Validate token exchange
- [ ] Check CORS settings

---

## 🔧 Technical Implementation Details

### Environment Variables
```bash
# .env.local
NEXT_PUBLIC_API_URL=http://localhost:5001
NEXT_PUBLIC_OAUTH_CLIENT_ID=ABC_NextJS_Client
NEXT_PUBLIC_OAUTH_REDIRECT_URI=http://localhost:3000/auth/callback
NEXT_PUBLIC_OAUTH_SCOPE=openid profile email api_access

NEXTAUTH_URL=http://localhost:3000
NEXTAUTH_SECRET=your-nextauth-secret-key-here
OAUTH_CLIENT_SECRET=your-oauth-client-secret
```

### API Integration Points
1. **Authorization Endpoint**: `http://localhost:5001/connect/authorize`
2. **Token Endpoint**: `http://localhost:5001/connect/token`
3. **UserInfo Endpoint**: `http://localhost:5001/connect/userinfo`
4. **Portfolio API**: `http://localhost:5001/api/v1/portfolios`
5. **Account API**: `http://localhost:5001/api/v1/account`

### Security Considerations
- [ ] PKCE for OAuth 2.0
- [ ] Secure token storage
- [ ] CSRF protection
- [ ] XSS prevention
- [ ] Content Security Policy

---

## 📝 Deliverables

### Code Files
1. **Authentication Setup**
   - `lib/auth/nextauth.ts`
   - `lib/api/client.ts`
   - `stores/authStore.ts`

2. **Components**
   - `components/auth/LoginForm.tsx`
   - `components/auth/RegisterForm.tsx`
   - `components/layout/Navbar.tsx`
   - `components/auth/ProtectedRoute.tsx`

3. **Pages**
   - `app/auth/login/page.tsx`
   - `app/auth/register/page.tsx`
   - `app/auth/callback/page.tsx`
   - `app/dashboard/page.tsx`

4. **Configuration**
   - `next.config.ts`
   - `middleware.ts`
   - Environment variables

### Documentation
- [ ] API integration guide
- [ ] Authentication flow diagram
- [ ] Troubleshooting guide
- [ ] Testing procedures

---

## ⏱️ Timeline Estimate

| Phase | Duration | Tasks |
|-------|----------|-------|
| Phase 1 | 1-2 hours | Project setup, dependencies, configuration |
| Phase 2 | 3-4 hours | Authentication implementation |
| Phase 3 | 2-3 hours | UI components development |
| Phase 4 | 2-3 hours | State management, data fetching |
| Phase 5 | 1-2 hours | Testing และ integration |
| **Total** | **9-14 hours** | **Complete frontend auth integration** |

---

## 🚧 Potential Challenges

### 1. OAuth Flow Issues
- **Problem**: CORS errors ในการเชื่อมต่อ backend
- **Solution**: Configure CORS ใน backend และ proxy ใน Next.js

### 2. Token Management
- **Problem**: Token expiry และ refresh logic
- **Solution**: Implement automatic token refresh with retry logic

### 3. State Synchronization
- **Problem**: Auth state ไม่ sync ระหว่าง components
- **Solution**: Use Zustand store with React Query integration

### 4. Type Safety
- **Problem**: API response types ไม่ตรงกับ TypeScript interfaces
- **Solution**: Generate types จาก OpenAPI schema หรือ manual typing

---

## ✅ Success Criteria

### Functional Requirements
- [ ] ผู้ใช้สามารถ login/logout ได้สำเร็จ
- [ ] Registration flow ทำงานถูกต้อง
- [ ] Protected routes ป้องกันการเข้าถึงได้
- [ ] Token refresh automatic
- [ ] API calls authenticate ได้

### Technical Requirements
- [ ] TypeScript ไม่มี errors
- [ ] ESLint ผ่านทุก rules
- [ ] Components render ถูกต้อง
- [ ] Error handling ครอบคลุม
- [ ] Loading states ทำงานดี

### User Experience
- [ ] Login/logout flow smooth
- [ ] Error messages ชัดเจน
- [ ] Loading indicators responsive
- [ ] Navigation intuitive

---

## 🔄 Next Steps (After STEP 11)

### STEP 12: Portfolio Management UI
- Portfolio list และ detail pages
- CRUD operations for portfolios
- Data visualization components

### STEP 13: Advanced Features
- Real-time updates
- Notifications
- Advanced filtering

### STEP 14: Testing & Security
- Unit และ integration tests
- Security audit
- Performance optimization

### STEP 15: Deployment & Documentation
- Production deployment
- User documentation
- Admin documentation

---

*Created: June 29, 2025*
*Status: Ready for Implementation*
*Estimated Completion: 1-2 working days*
