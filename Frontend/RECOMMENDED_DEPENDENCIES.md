# Recommended Dependencies for ABC Portfolio Management System

## 📦 Frontend Dependencies Overview

### Core Dependencies (Required)
```json
{
  "dependencies": {
    "next": "^15.3.4",
    "react": "^18.3.1",
    "react-dom": "^18.3.1",
    "typescript": "^5.7.3"
  }
}
```

### Authentication & API Integration
```json
{
  "dependencies": {
    "@next-auth/core": "^0.7.5",
    "next-auth": "^4.24.10",
    "axios": "^1.7.9",
    "ky": "^1.7.3",
    "react-query": "^3.39.5",
    "@tanstack/react-query": "^5.68.1"
  }
}
```

### UI Components & Styling
```json
{
  "dependencies": {
    "tailwindcss": "^3.4.18",
    "@headlessui/react": "^2.2.0",
    "@heroicons/react": "^2.2.0",
    "clsx": "^2.1.1",
    "class-variance-authority": "^0.7.1",
    "tailwind-merge": "^2.5.5"
  }
}
```

### Form Handling & Validation
```json
{
  "dependencies": {
    "react-hook-form": "^7.54.2",
    "@hookform/resolvers": "^3.10.0",
    "zod": "^3.24.1",
    "yup": "^1.5.0"
  }
}
```

### State Management
```json
{
  "dependencies": {
    "zustand": "^5.0.2",
    "@reduxjs/toolkit": "^2.4.0",
    "react-redux": "^9.2.0"
  }
}
```

### Date & Time Utilities
```json
{
  "dependencies": {
    "date-fns": "^4.1.0",
    "react-datepicker": "^7.5.0"
  }
}
```

### Development Dependencies
```json
{
  "devDependencies": {
    "@types/node": "^22.10.5",
    "@types/react": "^18.3.17",
    "@types/react-dom": "^18.3.5",
    "eslint": "^9.16.0",
    "eslint-config-next": "^15.3.4",
    "@typescript-eslint/eslint-plugin": "^8.17.0",
    "@typescript-eslint/parser": "^8.17.0",
    "prettier": "^3.4.2",
    "tailwindcss": "^3.4.18",
    "postcss": "^8.5.4",
    "autoprefixer": "^10.4.20"
  }
}
```

---

## 🔧 Configuration Files

### 1. Environment Variables (.env.local)
```bash
# API Configuration
NEXT_PUBLIC_API_URL=http://localhost:5001
NEXT_PUBLIC_API_VERSION=v1

# OAuth 2.0 Configuration
NEXT_PUBLIC_OAUTH_CLIENT_ID=ABC_NextJS_Client
NEXT_PUBLIC_OAUTH_REDIRECT_URI=http://localhost:3000/auth/callback
NEXT_PUBLIC_OAUTH_SCOPE=openid profile email api_access

# NextAuth Configuration
NEXTAUTH_URL=http://localhost:3000
NEXTAUTH_SECRET=your-nextauth-secret-key-here

# Feature Flags
NEXT_PUBLIC_ENABLE_ANALYTICS=false
NEXT_PUBLIC_ENABLE_LOGGING=true
```

### 2. TypeScript Configuration (tsconfig.json)
```json
{
  "compilerOptions": {
    "target": "ES2022",
    "lib": ["dom", "dom.iterable", "ES6"],
    "allowJs": true,
    "skipLibCheck": true,
    "strict": true,
    "forceConsistentCasingInFileNames": true,
    "noEmit": true,
    "esModuleInterop": true,
    "module": "esnext",
    "moduleResolution": "bundler",
    "resolveJsonModule": true,
    "isolatedModules": true,
    "jsx": "preserve",
    "incremental": true,
    "plugins": [
      {
        "name": "next"
      }
    ],
    "baseUrl": ".",
    "paths": {
      "@/*": ["./src/*"],
      "@/components/*": ["./src/components/*"],
      "@/lib/*": ["./src/lib/*"],
      "@/hooks/*": ["./src/hooks/*"],
      "@/types/*": ["./src/types/*"],
      "@/utils/*": ["./src/utils/*"],
      "@/services/*": ["./src/services/*"],
      "@/store/*": ["./src/store/*"]
    }
  },
  "include": ["next-env.d.ts", "**/*.ts", "**/*.tsx", ".next/types/**/*.ts"],
  "exclude": ["node_modules"]
}
```

### 3. ESLint Configuration (eslint.config.mjs)
```javascript
import { dirname } from "path";
import { fileURLToPath } from "url";
import { FlatCompat } from "@eslint/eslintrc";

const __filename = fileURLToPath(import.meta.url);
const __dirname = dirname(__filename);

const compat = new FlatCompat({
  baseDirectory: __dirname,
});

const eslintConfig = [
  ...compat.extends("next/core-web-vitals", "next/typescript"),
  {
    rules: {
      "@typescript-eslint/no-unused-vars": "warn",
      "@typescript-eslint/no-explicit-any": "warn",
      "react-hooks/exhaustive-deps": "warn",
      "prefer-const": "error",
      "no-var": "error"
    }
  }
];

export default eslintConfig;
```

### 4. Tailwind Configuration (tailwind.config.ts)
```typescript
import type { Config } from "tailwindcss";

export default {
  content: [
    "./src/pages/**/*.{js,ts,jsx,tsx,mdx}",
    "./src/components/**/*.{js,ts,jsx,tsx,mdx}",
    "./src/app/**/*.{js,ts,jsx,tsx,mdx}",
  ],
  theme: {
    extend: {
      colors: {
        background: "var(--background)",
        foreground: "var(--foreground)",
        primary: {
          50: '#eff6ff',
          500: '#3b82f6',
          600: '#2563eb',
          700: '#1d4ed8',
        },
        secondary: {
          50: '#f8fafc',
          500: '#64748b',
          600: '#475569',
          700: '#334155',
        }
      },
      fontFamily: {
        sans: ['Inter', 'system-ui', 'sans-serif'],
      }
    },
  },
  plugins: [
    require('@tailwindcss/forms'),
    require('@tailwindcss/typography'),
  ],
} satisfies Config;
```

---

## 📋 Installation Commands

### 1. Install Core Dependencies
```bash
npm install next@^15.3.4 react@^18.3.1 react-dom@^18.3.1 typescript@^5.7.3
```

### 2. Install Authentication Dependencies
```bash
npm install next-auth@^4.24.10 axios@^1.7.9 @tanstack/react-query@^5.68.1
```

### 3. Install UI Dependencies
```bash
npm install @headlessui/react@^2.2.0 @heroicons/react@^2.2.0 clsx@^2.1.1 tailwind-merge@^2.5.5
```

### 4. Install Form Dependencies
```bash
npm install react-hook-form@^7.54.2 @hookform/resolvers@^3.10.0 zod@^3.24.1
```

### 5. Install State Management
```bash
npm install zustand@^5.0.2
```

### 6. Install Development Dependencies
```bash
npm install -D @types/node@^22.10.5 @types/react@^18.3.17 @types/react-dom@^18.3.5 prettier@^3.4.2
```

---

## 🎯 Priority Installation Order

### Phase 1: Core Setup
1. Next.js, React, TypeScript
2. Tailwind CSS setup
3. ESLint and Prettier

### Phase 2: Authentication
1. NextAuth.js or OAuth 2.0 client
2. Axios for API calls
3. React Query for data fetching

### Phase 3: UI Components
1. Headless UI components
2. Heroicons
3. Form handling libraries

### Phase 4: Advanced Features
1. State management (Zustand)
2. Date utilities
3. Additional UI libraries

---

## ⚡ Quick Start Command
```bash
# Install all essential dependencies at once
npm install next@^15.3.4 react@^18.3.1 react-dom@^18.3.1 typescript@^5.7.3 \
  axios@^1.7.9 @tanstack/react-query@^5.68.1 \
  @headlessui/react@^2.2.0 @heroicons/react@^2.2.0 \
  react-hook-form@^7.54.2 @hookform/resolvers@^3.10.0 zod@^3.24.1 \
  zustand@^5.0.2 clsx@^2.1.1 tailwind-merge@^2.5.5

# Install dev dependencies
npm install -D @types/node@^22.10.5 @types/react@^18.3.17 @types/react-dom@^18.3.5 \
  prettier@^3.4.2 @tailwindcss/forms @tailwindcss/typography
```

---

## 🔍 Package Selection Rationale

### Authentication
- **NextAuth.js**: Industry standard for Next.js authentication
- **Axios**: Reliable HTTP client with interceptor support
- **React Query**: Excellent caching and synchronization

### UI Framework
- **Headless UI**: Unstyled, accessible components
- **Heroicons**: Beautiful SVG icons from Tailwind team
- **Tailwind CSS**: Utility-first CSS framework

### Forms
- **React Hook Form**: Performance-focused form library
- **Zod**: Type-safe schema validation
- **TypeScript**: Static type checking

### State Management
- **Zustand**: Lightweight, simple state management
- **React Query**: Server state management

---

*Updated: June 29, 2025*
*Ready for STEP 11 Implementation*
