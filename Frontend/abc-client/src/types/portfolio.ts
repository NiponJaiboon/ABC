export interface Portfolio {
  id: string
  name: string
  description: string
  isPublic: boolean
  createdAt: string
  updatedAt: string
  projectCount?: number
  skillCount?: number
  userId: string
  projects?: Project[]
}

export interface CreatePortfolioRequest {
  name: string
  description: string
  isPublic: boolean
}

export interface UpdatePortfolioRequest {
  name?: string
  description?: string
  isPublic?: boolean
}

export interface GetPortfoliosParams {
  page?: number
  limit?: number
  search?: string
  sortBy?: 'name' | 'createdAt' | 'updatedAt'
  sortOrder?: 'asc' | 'desc'
  isPublic?: boolean
}

export interface PortfolioResponse {
  data: Portfolio[]
  total: number
  page: number
  limit: number
  totalPages: number
}

// Re-export Project interface if needed
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
  createdAt: string
  updatedAt: string
}
