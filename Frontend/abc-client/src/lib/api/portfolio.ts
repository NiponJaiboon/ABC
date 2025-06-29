import type {
    CreatePortfolioRequest,
    GetPortfoliosParams,
    Portfolio,
    PortfolioResponse,
    Project,
    UpdatePortfolioRequest
} from '@/types/portfolio'
import { apiClient } from './client'

export const portfolioApi = {
  /**
   * Get all portfolios with optional filtering and pagination
   */
  getAll: async (params?: GetPortfoliosParams): Promise<PortfolioResponse> => {
    const response = await apiClient.get<PortfolioResponse>('/portfolios', { params })
    return response
  },

  /**
   * Get a specific portfolio by ID
   */
  getById: async (id: string): Promise<Portfolio> => {
    const response = await apiClient.get<Portfolio>(`/portfolios/${id}`)
    return response
  },

  /**
   * Create a new portfolio
   */
  create: async (data: CreatePortfolioRequest): Promise<Portfolio> => {
    const response = await apiClient.post<Portfolio>('/portfolios', data)
    return response
  },

  /**
   * Update an existing portfolio
   */
  update: async (id: string, data: UpdatePortfolioRequest): Promise<Portfolio> => {
    const response = await apiClient.put<Portfolio>(`/portfolios/${id}`, data)
    return response
  },

  /**
   * Delete a portfolio
   */
  delete: async (id: string): Promise<void> => {
    await apiClient.delete(`/portfolios/${id}`)
  },

  /**
   * Get portfolio statistics
   */
  getStats: async (id: string): Promise<{ projectCount: number; skillCount: number }> => {
    const response = await apiClient.get<{ projectCount: number; skillCount: number }>(`/portfolios/${id}/stats`)
    return response
  },

  /**
   * Get projects within a portfolio
   */
  getProjects: async (id: string): Promise<Project[]> => {
    const response = await apiClient.get<Project[]>(`/portfolios/${id}/projects`)
    return response
  }
}
