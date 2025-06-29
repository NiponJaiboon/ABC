import type {
  CreateSkillRequest,
  GetSkillsParams,
  Skill,
  SkillCategory,
  SkillResponse,
  UpdateSkillRequest
} from '@/types/skills'
import { apiClient } from './client'

export const skillApi = {
  /**
   * Get all skills with optional filtering and pagination
   */
  getAll: async (params?: GetSkillsParams): Promise<SkillResponse> => {
    const response = await apiClient.get<SkillResponse>('/skills', { params })
    return response
  },

  /**
   * Get a specific skill by ID
   */
  getById: async (id: string): Promise<Skill> => {
    const response = await apiClient.get<Skill>(`/skills/${id}`)
    return response
  },

  /**
   * Create a new skill
   */
  create: async (data: CreateSkillRequest): Promise<Skill> => {
    const response = await apiClient.post<Skill>('/skills', data)
    return response
  },

  /**
   * Update an existing skill
   */
  update: async (id: string, data: UpdateSkillRequest): Promise<Skill> => {
    const response = await apiClient.put<Skill>(`/skills/${id}`, data)
    return response
  },

  /**
   * Delete a skill
   */
  delete: async (id: string): Promise<void> => {
    await apiClient.delete(`/skills/${id}`)
  },

  /**
   * Get skill categories
   */
  getCategories: async (): Promise<SkillCategory[]> => {
    const response = await apiClient.get<SkillCategory[]>('/skills/categories')
    return response
  },

  /**
   * Bulk create skills
   */
  bulkCreate: async (skills: CreateSkillRequest[]): Promise<Skill[]> => {
    const response = await apiClient.post<Skill[]>('/skills/bulk', { skills })
    return response
  },

  /**
   * Update skill proficiency level
   */
  updateProficiency: async (id: string, proficiencyLevel: Skill['proficiencyLevel']): Promise<Skill> => {
    const response = await apiClient.patch<Skill>(`/skills/${id}/proficiency`, { proficiencyLevel })
    return response
  }
}
