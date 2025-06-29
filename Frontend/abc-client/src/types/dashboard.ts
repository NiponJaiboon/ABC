export interface DashboardStats {
  portfolioCount: number
  projectCount: number
  skillCount: number
  activeProjectCount: number
  completedProjectCount: number
  draftProjectCount: number
  completionRate: number
  totalSkillProficiency: number
  recentActivity: ActivityItem[]
}

export interface ActivityItem {
  id: string
  type: 'portfolio' | 'project' | 'skill'
  action: 'created' | 'updated' | 'deleted'
  entityName: string
  entityId: string
  timestamp: string
  description: string
}

export interface ChartData {
  labels: string[]
  datasets: ChartDataset[]
}

export interface ChartDataset {
  label: string
  data: number[]
  backgroundColor?: string | string[]
  borderColor?: string | string[]
  borderWidth?: number
}

export interface ProjectStatusDistribution {
  draft: number
  inProgress: number
  completed: number
  archived: number
}

export interface SkillProficiencyDistribution {
  level1: number
  level2: number
  level3: number
  level4: number
  level5: number
}

export interface MonthlyProgress {
  month: string
  portfolios: number
  projects: number
  skills: number
}

export interface DashboardResponse {
  stats: DashboardStats
  projectStatusDistribution: ProjectStatusDistribution
  skillProficiencyDistribution: SkillProficiencyDistribution
  monthlyProgress: MonthlyProgress[]
}
