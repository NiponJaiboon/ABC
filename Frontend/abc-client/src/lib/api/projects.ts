import type {
    CreateProjectRequest,
    GetProjectsParams,
    Project,
    ProjectResponse,
    UpdateProjectRequest
} from '@/types/projects'
import { apiClient } from './client'

export const projectApi = {
  /**
   * Get all projects with optional filtering and pagination
   */
  getAll: async (params?: GetProjectsParams): Promise<ProjectResponse> => {
    const response = await apiClient.get<ProjectResponse>('/projects', { params })
    return response
  },

  /**
   * Get a specific project by ID
   */
  getById: async (id: string): Promise<Project> => {
    const response = await apiClient.get<Project>(`/projects/${id}`)
    return response
  },

  /**
   * Create a new project
   */
  create: async (data: CreateProjectRequest): Promise<Project> => {
    const response = await apiClient.post<Project>('/projects', data)
    return response
  },

  /**
   * Update an existing project
   */
  update: async (id: string, data: UpdateProjectRequest): Promise<Project> => {
    const response = await apiClient.put<Project>(`/projects/${id}`, data)
    return response
  },

  /**
   * Delete a project
   */
  delete: async (id: string): Promise<void> => {
    await apiClient.delete(`/projects/${id}`)
  },

  /**
   * Get projects by portfolio ID
   */
  getByPortfolio: async (portfolioId: string): Promise<Project[]> => {
    const response = await apiClient.get<Project[]>(`/portfolios/${portfolioId}/projects`)
    return response
  },

  /**
   * Update project status
   */
  updateStatus: async (id: string, status: Project['status']): Promise<Project> => {
    const response = await apiClient.patch<Project>(`/projects/${id}/status`, { status })
    return response
  }
}
