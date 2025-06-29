import { env } from '@/lib/utils/env'
import axios, { AxiosInstance, AxiosRequestConfig, AxiosResponse } from 'axios'

// API Client configuration
class ApiClient {
  private client: AxiosInstance
  private accessToken: string | null = null

  constructor() {
    this.client = axios.create({
      baseURL: `${env.NEXT_PUBLIC_API_URL}/api/${env.NEXT_PUBLIC_API_VERSION}`,
      timeout: 10000,
      headers: {
        'Content-Type': 'application/json',
      },
    })

    this.setupInterceptors()
  }

  private setupInterceptors() {
    // Request interceptor to add auth token
    this.client.interceptors.request.use(
      (config) => {
        if (this.accessToken) {
          config.headers.Authorization = `Bearer ${this.accessToken}`
        }

        if (env.NEXT_PUBLIC_ENABLE_LOGGING) {
          console.log('API Request:', config.method?.toUpperCase(), config.url)
        }

        return config
      },
      (error) => {
        return Promise.reject(error)
      }
    )

    // Response interceptor for error handling and token refresh
    this.client.interceptors.response.use(
      (response) => {
        if (env.NEXT_PUBLIC_ENABLE_LOGGING) {
          console.log('API Response:', response.status, response.config.url)
        }
        return response
      },
      async (error) => {
        const originalRequest = error.config

        if (error.response?.status === 401 && !originalRequest._retry) {
          originalRequest._retry = true

          try {
            // Try to refresh token
            await this.refreshToken()
            return this.client(originalRequest)
          } catch (refreshError) {
            // Redirect to login if refresh fails
            if (typeof window !== 'undefined') {
              window.location.href = '/auth/login'
            }
            return Promise.reject(refreshError)
          }
        }

        return Promise.reject(error)
      }
    )
  }

  // Set access token
  setAccessToken(token: string | null) {
    this.accessToken = token
  }

  // Get access token
  getAccessToken(): string | null {
    return this.accessToken
  }

  // Refresh token method
  private async refreshToken(): Promise<void> {
    // Implementation will be added when we integrate with NextAuth
    throw new Error('Token refresh not implemented yet')
  }

  // Generic request methods
  async get<T>(url: string, config?: AxiosRequestConfig): Promise<T> {
    const response: AxiosResponse<T> = await this.client.get(url, config)
    return response.data
  }

  async post<T>(url: string, data?: any, config?: AxiosRequestConfig): Promise<T> {
    const response: AxiosResponse<T> = await this.client.post(url, data, config)
    return response.data
  }

  async put<T>(url: string, data?: any, config?: AxiosRequestConfig): Promise<T> {
    const response: AxiosResponse<T> = await this.client.put(url, data, config)
    return response.data
  }

  async patch<T>(url: string, data?: any, config?: AxiosRequestConfig): Promise<T> {
    const response: AxiosResponse<T> = await this.client.patch(url, data, config)
    return response.data
  }

  async delete<T>(url: string, config?: AxiosRequestConfig): Promise<T> {
    const response: AxiosResponse<T> = await this.client.delete(url, config)
    return response.data
  }
}

// Create and export singleton instance
export const apiClient = new ApiClient()
export default apiClient
