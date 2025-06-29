import type {
    ActivityItem,
    DashboardResponse,
    DashboardStats,
    MonthlyProgress,
    ProjectStatusDistribution,
    SkillProficiencyDistribution
} from '@/types/dashboard'
import { apiClient } from './client'

export const dashboardApi = {
  /**
   * Get comprehensive dashboard data
   */
  getDashboard: async (): Promise<DashboardResponse> => {
    const response = await apiClient.get<DashboardResponse>('/dashboard')
    return response
  },

  /**
   * Get dashboard statistics
   */
  getStats: async (): Promise<DashboardStats> => {
    const response = await apiClient.get<DashboardStats>('/dashboard/stats')
    return response
  },

  /**
   * Get project status distribution
   */
  getProjectStatusDistribution: async (): Promise<ProjectStatusDistribution> => {
    const response = await apiClient.get<ProjectStatusDistribution>('/dashboard/projects/status-distribution')
    return response
  },

  /**
   * Get skill proficiency distribution
   */
  getSkillProficiencyDistribution: async (): Promise<SkillProficiencyDistribution> => {
    const response = await apiClient.get<SkillProficiencyDistribution>('/dashboard/skills/proficiency-distribution')
    return response
  },

  /**
   * Get monthly progress data
   */
  getMonthlyProgress: async (months?: number): Promise<MonthlyProgress[]> => {
    const response = await apiClient.get<MonthlyProgress[]>('/dashboard/progress/monthly', {
      params: { months }
    })
    return response
  },

  /**
   * Get recent activity
   */
  getRecentActivity: async (limit?: number): Promise<ActivityItem[]> => {
    const response = await apiClient.get<ActivityItem[]>('/dashboard/activity/recent', {
      params: { limit }
    })
    return response
  },

  /**
   * Get analytics data for charts
   */
  getAnalytics: async (type: 'portfolio' | 'project' | 'skill', period: 'week' | 'month' | 'year') => {
    const response = await apiClient.get(`/dashboard/analytics/${type}`, {
      params: { period }
    })
    return response
  }
}
