# Next.js Frontend Architecture for ABC Portfolio

## 🎯 **Architecture Overview**
This document outlines the Next.js 15 frontend architecture for the ABC Portfolio Management System, designed to integrate seamlessly with the .NET 8 backend API.

## 📂 **Recommended Project Structure**

```
src/
├── app/                          # Next.js 15 App Router
│   ├── (auth)/                  # Route groups for auth pages
│   │   ├── login/
│   │   │   └── page.tsx
│   │   ├── register/
│   │   │   └── page.tsx
│   │   ├── oauth/
│   │   │   └── callback/
│   │   │       └── page.tsx
│   │   └── layout.tsx           # Auth layout
│   ├── (dashboard)/             # Protected routes group
│   │   ├── portfolio/
│   │   │   ├── page.tsx
│   │   │   └── [id]/
│   │   │       └── page.tsx
│   │   ├── projects/
│   │   │   ├── page.tsx
│   │   │   └── [id]/
│   │   │       └── page.tsx
│   │   ├── skills/
│   │   │   └── page.tsx
│   │   └── layout.tsx           # Dashboard layout
│   ├── api/                     # API routes for auth callbacks
│   │   └── auth/
│   │       └── [...nextauth]/
│   │           └── route.ts
│   ├── layout.tsx               # Root layout
│   ├── page.tsx                 # Landing page
│   ├── loading.tsx              # Global loading UI
│   ├── error.tsx                # Global error UI
│   └── not-found.tsx            # 404 page
├── components/                   # Reusable components
│   ├── ui/                      # Basic UI components
│   │   ├── Button.tsx
│   │   ├── Input.tsx
│   │   ├── Modal.tsx
│   │   ├── Card.tsx
│   │   ├── Badge.tsx
│   │   └── index.ts
│   ├── auth/                    # Authentication components
│   │   ├── LoginForm.tsx
│   │   ├── RegisterForm.tsx
│   │   ├── AuthGuard.tsx
│   │   ├── OAuthButton.tsx
│   │   └── LogoutButton.tsx
│   ├── navigation/              # Navigation components
│   │   ├── Navbar.tsx
│   │   ├── Sidebar.tsx
│   │   ├── UserMenu.tsx
│   │   └── Breadcrumb.tsx
│   ├── portfolio/               # Portfolio-specific components
│   │   ├── PortfolioCard.tsx
│   │   ├── PortfolioForm.tsx
│   │   ├── PortfolioList.tsx
│   │   └── PortfolioDetails.tsx
│   ├── projects/                # Project-specific components
│   │   ├── ProjectCard.tsx
│   │   ├── ProjectForm.tsx
│   │   ├── ProjectList.tsx
│   │   └── ProjectDetails.tsx
│   └── layout/                  # Layout components
│       ├── Header.tsx
│       ├── Footer.tsx
│       ├── Container.tsx
│       └── PageTitle.tsx
├── lib/                         # Utility libraries
│   ├── auth/                    # Authentication logic
│   │   ├── config.ts            # NextAuth configuration
│   │   ├── providers.tsx        # Auth providers wrapper
│   │   ├── oauth-client.ts      # OAuth client config
│   │   └── utils.ts             # Auth utility functions
│   ├── api/                     # API client
│   │   ├── client.ts            # Axios client configuration
│   │   ├── auth.ts              # Auth API calls
│   │   ├── portfolio.ts         # Portfolio API calls
│   │   ├── projects.ts          # Projects API calls
│   │   ├── skills.ts            # Skills API calls
│   │   └── types.ts             # API type definitions
│   ├── store/                   # State management
│   │   ├── auth-store.ts        # Auth state (Zustand)
│   │   ├── portfolio-store.ts   # Portfolio state
│   │   ├── ui-store.ts          # UI state
│   │   └── index.ts             # Store exports
│   └── utils/                   # Helper functions
│       ├── validation.ts        # Form validation schemas
│       ├── format.ts            # Data formatting utils
│       ├── constants.ts         # App constants
│       └── env.ts               # Environment variables
├── hooks/                       # Custom React hooks
│   ├── useAuth.ts               # Authentication hook
│   ├── useApi.ts                # API interaction hook
│   ├── useLocalStorage.ts       # Local storage hook
│   ├── usePortfolio.ts          # Portfolio management hook
│   ├── useProjects.ts           # Projects management hook
│   └── useDebounce.ts           # Debounce hook
├── types/                       # TypeScript definitions
│   ├── auth.ts                  # Authentication types
│   ├── api.ts                   # API response types
│   ├── portfolio.ts             # Portfolio types
│   ├── projects.ts              # Projects types
│   ├── skills.ts                # Skills types
│   └── global.ts                # Global types
└── middleware.ts                # Next.js middleware for auth
```

## 🔧 **Technology Stack**

### Core Framework
- **Next.js 15.3.4** with App Router
- **TypeScript** for type safety
- **Tailwind CSS** for styling

### Authentication & State Management
- **NextAuth.js v4** for OAuth 2.0 integration
- **Zustand** for lightweight global state
- **React Query (@tanstack/react-query)** for server state

### HTTP & API Integration
- **Axios** for HTTP client with interceptors
- **js-cookie** for cookie management

### UI & Forms
- **Headless UI** for accessible components
- **Heroicons** for consistent iconography
- **React Hook Form** with Yup validation
- **React Hot Toast** for notifications

## 🔐 **Authentication Integration**

### NextAuth.js Configuration
```typescript
// lib/auth/config.ts
import { NextAuthOptions } from 'next-auth'

export const authOptions: NextAuthOptions = {
  providers: [
    {
      id: "abc-oauth",
      name: "ABC Portfolio",
      type: "oauth",
      authorization: {
        url: "http://localhost:5001/connect/authorize",
        params: {
          scope: "openid profile email portfolio:read portfolio:write",
          response_type: "code",
          grant_type: "authorization_code"
        }
      },
      token: "http://localhost:5001/connect/token",
      userinfo: "http://localhost:5001/connect/userinfo",
      clientId: process.env.OAUTH_CLIENT_ID,
      clientSecret: process.env.OAUTH_CLIENT_SECRET,
      checks: ["pkce"],
    }
  ],
  callbacks: {
    async jwt({ token, account }) {
      if (account) {
        token.accessToken = account.access_token
        token.refreshToken = account.refresh_token
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

### Route Protection Middleware
```typescript
// middleware.ts
import { withAuth } from "next-auth/middleware"

export default withAuth(
  function middleware(req) {
    // Additional middleware logic
  },
  {
    callbacks: {
      authorized: ({ token, req }) => {
        // Check if user has required permissions
        const protectedPaths = ['/dashboard', '/portfolio', '/projects']
        return protectedPaths.some(path =>
          req.nextUrl.pathname.startsWith(path)
        ) ? !!token : true
      },
    },
  }
)

export const config = {
  matcher: ['/dashboard/:path*', '/portfolio/:path*', '/projects/:path*']
}
```

## 📊 **State Management Strategy**

### Zustand for Client State
- Authentication state
- UI preferences
- Form state
- Navigation state

### React Query for Server State
- API data caching
- Background refetching
- Optimistic updates
- Error handling

## 🎨 **UI Component System**

### Design Tokens
- Consistent color palette
- Typography scale
- Spacing system
- Component variants

### Accessibility
- WCAG 2.1 AA compliance
- Keyboard navigation
- Screen reader support
- Focus management

## 🚀 **Performance Optimizations**

- Server Components for static content
- Client Components for interactivity
- Code splitting by routes
- Image optimization
- Bundle analysis

## 📋 **Development Workflow**

1. **Component Development**: Storybook integration
2. **Type Safety**: Strict TypeScript configuration
3. **Code Quality**: ESLint + Prettier
4. **Testing**: Jest + React Testing Library
5. **Performance**: Next.js built-in optimizations

---

*Created: June 29, 2025*
*For: ABC Portfolio Management System - STEP 11*
