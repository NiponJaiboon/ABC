import { authApi } from '@/lib/api/auth'
import { apiClient } from '@/lib/api/client'
import { LoginCredentials, User } from '@/types/auth'
import { create } from 'zustand'
import { persist } from 'zustand/middleware'

interface AuthState {
  // State
  user: User | null
  accessToken: string | null
  refreshToken: string | null
  isAuthenticated: boolean
  isLoading: boolean
  error: string | null

  // Actions
  login: (credentials: LoginCredentials) => Promise<void>
  logout: () => Promise<void>
  refreshTokens: () => Promise<void>
  setUser: (user: User | null) => void
  setTokens: (accessToken: string | null, refreshToken: string | null) => void
  clearError: () => void
  setLoading: (loading: boolean) => void
}

export const useAuthStore = create<AuthState>()(
  persist(
    (set, get) => ({
      // Initial state
      user: null,
      accessToken: null,
      refreshToken: null,
      isAuthenticated: false,
      isLoading: false,
      error: null,

      // Login action
      login: async (credentials: LoginCredentials) => {
        set({ isLoading: true, error: null })

        try {
          const response = await authApi.login(credentials)
          const { user, tokens } = response

          // Set tokens in API client
          apiClient.setAccessToken(tokens.accessToken)

          set({
            user,
            accessToken: tokens.accessToken,
            refreshToken: tokens.refreshToken,
            isAuthenticated: true,
            isLoading: false,
            error: null
          })
        } catch (error) {
          const errorMessage = error instanceof Error ? error.message : 'Login failed'
          set({
            user: null,
            accessToken: null,
            refreshToken: null,
            isAuthenticated: false,
            isLoading: false,
            error: errorMessage
          })
          throw error
        }
      },

      // Logout action
      logout: async () => {
        set({ isLoading: true })

        try {
          // Call logout API if authenticated
          if (get().isAuthenticated) {
            await authApi.logout()
          }
        } catch (error) {
          console.warn('Logout API call failed:', error)
        } finally {
          // Clear local state regardless of API call result
          apiClient.setAccessToken(null)
          set({
            user: null,
            accessToken: null,
            refreshToken: null,
            isAuthenticated: false,
            isLoading: false,
            error: null
          })
        }
      },

      // Refresh tokens
      refreshTokens: async () => {
        const { refreshToken } = get()

        if (!refreshToken) {
          throw new Error('No refresh token available')
        }

        try {
          const tokens = await authApi.refreshToken(refreshToken)

          // Update API client with new token
          apiClient.setAccessToken(tokens.accessToken)

          set({
            accessToken: tokens.accessToken,
            refreshToken: tokens.refreshToken,
            error: null
          })
        } catch (error) {
          // If refresh fails, logout user
          await get().logout()
          throw error
        }
      },

      // Set user
      setUser: (user: User | null) => {
        set({ user, isAuthenticated: !!user })
      },

      // Set tokens
      setTokens: (accessToken: string | null, refreshToken: string | null) => {
        if (accessToken) {
          apiClient.setAccessToken(accessToken)
        }

        set({
          accessToken,
          refreshToken,
          isAuthenticated: !!accessToken
        })
      },

      // Clear error
      clearError: () => {
        set({ error: null })
      },

      // Set loading state
      setLoading: (loading: boolean) => {
        set({ isLoading: loading })
      }
    }),
    {
      name: 'auth-storage',
      partialize: (state) => ({
        user: state.user,
        accessToken: state.accessToken,
        refreshToken: state.refreshToken,
        isAuthenticated: state.isAuthenticated
      })
    }
  )
)

// Auth hook for easy access
export const useAuth = () => {
  const auth = useAuthStore()

  return {
    ...auth,
    // Computed values
    isLoggedIn: auth.isAuthenticated && !!auth.user,
    hasRole: (role: string) => auth.user?.roles.includes(role) ?? false,
    hasPermission: (permission: string) => auth.user?.permissions.includes(permission) ?? false
  }
}
