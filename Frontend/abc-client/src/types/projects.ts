export interface Project {
  id: string
  title: string
  description: string
  content: string
  status: 'draft' | 'in-progress' | 'completed' | 'archived'
  technologies: string[]
  startDate: string
  endDate?: string
  portfolioId: string
  userId: string
  createdAt: string
  updatedAt: string
  skills?: Skill[]
}

export interface CreateProjectRequest {
  title: string
  description: string
  content: string
  status: Project['status']
  technologies: string[]
  startDate: string
  endDate?: string
  portfolioId: string
}

export interface UpdateProjectRequest {
  title?: string
  description?: string
  content?: string
  status?: Project['status']
  technologies?: string[]
  startDate?: string
  endDate?: string
  portfolioId?: string
}

export interface GetProjectsParams {
  page?: number
  limit?: number
  search?: string
  sortBy?: 'title' | 'createdAt' | 'updatedAt' | 'startDate'
  sortOrder?: 'asc' | 'desc'
  status?: Project['status']
  portfolioId?: string
}

export interface ProjectResponse {
  data: Project[]
  total: number
  page: number
  limit: number
  totalPages: number
}

// Re-export Skill interface if needed
export interface Skill {
  id: string
  name: string
  category: string
  proficiencyLevel: 1 | 2 | 3 | 4 | 5
  experienceYears: number
  lastUsed: string
  createdAt: string
  updatedAt: string
}
