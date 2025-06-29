// Authentication types
export interface User {
  id: string
  email: string
  name: string
  username?: string
  profilePicture?: string
  roles: string[]
  permissions: string[]
  createdAt: Date
  updatedAt: Date
}

export interface AuthSession {
  user: User | null
  accessToken: string | null
  refreshToken: string | null
  expiresAt: Date | null
  isAuthenticated: boolean
}

export interface LoginCredentials {
  email: string
  password: string
}

export interface RegisterData {
  email: string
  password: string
  confirmPassword: string
  firstName: string
  lastName: string
  username?: string
}

export interface AuthTokens {
  accessToken: string
  refreshToken: string
  expiresIn: number
  tokenType: string
}

export interface OAuthState {
  state: string
  codeVerifier: string
  redirectUri: string
}

// NextAuth extended session type
declare module "next-auth" {
  interface Session {
    accessToken?: string
    refreshToken?: string
    user: User
  }

  interface JWT {
    accessToken?: string
    refreshToken?: string
    expiresAt?: number
  }
}
