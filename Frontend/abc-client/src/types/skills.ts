export interface Skill {
  id: string
  name: string
  category: string
  proficiencyLevel: 1 | 2 | 3 | 4 | 5
  experienceYears: number
  lastUsed: string
  userId: string
  createdAt: string
  updatedAt: string
  certifications?: string[]
  projects?: Project[]
}

export interface CreateSkillRequest {
  name: string
  category: string
  proficiencyLevel: Skill['proficiencyLevel']
  experienceYears: number
  lastUsed: string
  certifications?: string[]
}

export interface UpdateSkillRequest {
  name?: string
  category?: string
  proficiencyLevel?: Skill['proficiencyLevel']
  experienceYears?: number
  lastUsed?: string
  certifications?: string[]
}

export interface GetSkillsParams {
  page?: number
  limit?: number
  search?: string
  category?: string
  proficiencyLevel?: Skill['proficiencyLevel']
  sortBy?: 'name' | 'category' | 'proficiencyLevel' | 'lastUsed'
  sortOrder?: 'asc' | 'desc'
}

export interface SkillResponse {
  data: Skill[]
  total: number
  page: number
  limit: number
  totalPages: number
}

export interface SkillCategory {
  name: string
  count: number
}

// Re-export Project interface if needed
export interface Project {
  id: string
  title: string
  description: string
  status: 'draft' | 'in-progress' | 'completed' | 'archived'
  portfolioId: string
  createdAt: string
  updatedAt: string
}
