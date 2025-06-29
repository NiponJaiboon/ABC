export type SkillCategory =
  | "Technical"
  | "Programming"
  | "Framework"
  | "Database"
  | "DevOps"
  | "Design"
  | "Management"
  | "Language"
  | "Other";

export type ProficiencyLevel = "Beginner" | "Intermediate" | "Advanced" | "Expert";

export interface Skill {
  id: string
  name: string
  category: SkillCategory
  proficiencyLevel: ProficiencyLevel
  yearsOfExperience: number
  lastUsed?: string
  userId: string
  createdAt: string
  updatedAt: string
  description?: string
  tags?: string[]
  certifications?: string[]
  isActive?: boolean
  projects?: Project[]
}

export interface CreateSkillRequest {
  name: string
  category: SkillCategory
  proficiencyLevel: ProficiencyLevel
  yearsOfExperience: number
  lastUsed?: string
  description?: string
  tags?: string[]
  certifications?: string[]
  isActive?: boolean
}

export interface UpdateSkillRequest {
  name?: string
  category?: SkillCategory
  proficiencyLevel?: ProficiencyLevel
  yearsOfExperience?: number
  lastUsed?: string
  description?: string
  tags?: string[]
  certifications?: string[]
  isActive?: boolean
}

export interface GetSkillsParams {
  page?: number
  limit?: number
  search?: string
  category?: SkillCategory
  proficiencyLevel?: ProficiencyLevel
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

export interface SkillCategoryCount {
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
