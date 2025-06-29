import { AuthTokens, LoginCredentials, RegisterData, User } from '@/types/auth'
import { apiClient } from './client'

// API Response types
interface ApiResponse<T> {
  success: boolean
  data: T
  message?: string
  errors?: string[]
}

interface LoginResponse {
  user: User
  tokens: AuthTokens
}

interface RegisterResponse {
  user: User
  message: string
}

// Authentication API calls
export const authApi = {
  // Login with email and password
  login: async (credentials: LoginCredentials): Promise<LoginResponse> => {
    return apiClient.post<LoginResponse>('/auth/login', credentials)
  },

  // Register new user
  register: async (data: RegisterData): Promise<RegisterResponse> => {
    return apiClient.post<RegisterResponse>('/auth/register', data)
  },

  // Logout
  logout: async (): Promise<void> => {
    return apiClient.post<void>('/auth/logout')
  },

  // Refresh token
  refreshToken: async (refreshToken: string): Promise<AuthTokens> => {
    return apiClient.post<AuthTokens>('/auth/refresh', { refreshToken })
  },

  // Get current user profile
  getProfile: async (): Promise<User> => {
    return apiClient.get<User>('/auth/profile')
  },

  // Update user profile
  updateProfile: async (data: Partial<User>): Promise<User> => {
    return apiClient.put<User>('/auth/profile', data)
  },

  // Verify email
  verifyEmail: async (token: string): Promise<void> => {
    return apiClient.post<void>('/auth/verify-email', { token })
  },

  // Request password reset
  requestPasswordReset: async (email: string): Promise<void> => {
    return apiClient.post<void>('/auth/forgot-password', { email })
  },

  // Reset password
  resetPassword: async (token: string, newPassword: string): Promise<void> => {
    return apiClient.post<void>('/auth/reset-password', { token, newPassword })
  },

  // OAuth2 Authorization URL
  getAuthorizationUrl: async (state: string, codeVerifier: string): Promise<string> => {
    const params = new URLSearchParams({
      response_type: 'code',
      client_id: process.env.NEXT_PUBLIC_OAUTH_CLIENT_ID || '',
      redirect_uri: process.env.NEXT_PUBLIC_OAUTH_REDIRECT_URI || '',
      scope: process.env.NEXT_PUBLIC_OAUTH_SCOPE || '',
      state,
      code_challenge: codeVerifier,
      code_challenge_method: 'S256'
    })

    return `${process.env.NEXT_PUBLIC_API_URL}/connect/authorize?${params.toString()}`
  },

  // Exchange authorization code for tokens
  exchangeCodeForTokens: async (code: string, codeVerifier: string, state: string): Promise<AuthTokens> => {
    return apiClient.post<AuthTokens>('/connect/token', {
      grant_type: 'authorization_code',
      client_id: process.env.NEXT_PUBLIC_OAUTH_CLIENT_ID,
      code,
      code_verifier: codeVerifier,
      redirect_uri: process.env.NEXT_PUBLIC_OAUTH_REDIRECT_URI,
      state
    })
  }
}

export default authApi
