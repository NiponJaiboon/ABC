import type { Project } from '@/types/projects'
import { create } from 'zustand'
import { devtools } from 'zustand/middleware'

interface ProjectState {
  // State
  selectedProject: Project | null
  selectedProjects: string[]
  isCreating: boolean
  isUpdating: boolean
  isDeleting: boolean

  // Filters and UI state
  filters: {
    search: string
    sortBy: 'title' | 'createdAt' | 'updatedAt' | 'startDate'
    sortOrder: 'asc' | 'desc'
    status?: Project['status']
    portfolioId?: string
  }

  viewMode: 'grid' | 'list' | 'timeline'

  // Actions
  setSelectedProject: (project: Project | null) => void
  setSelectedProjects: (ids: string[]) => void
  toggleProjectSelection: (id: string) => void
  clearSelection: () => void

  // Filter actions
  setSearch: (search: string) => void
  setSortBy: (sortBy: ProjectState['filters']['sortBy']) => void
  setSortOrder: (sortOrder: ProjectState['filters']['sortOrder']) => void
  setStatusFilter: (status?: Project['status']) => void
  setPortfolioFilter: (portfolioId?: string) => void
  resetFilters: () => void

  // UI actions
  setViewMode: (mode: ProjectState['viewMode']) => void
  setCreating: (isCreating: boolean) => void
  setUpdating: (isUpdating: boolean) => void
  setDeleting: (isDeleting: boolean) => void
}

const initialFilters = {
  search: '',
  sortBy: 'updatedAt' as const,
  sortOrder: 'desc' as const,
  status: undefined,
  portfolioId: undefined,
}

export const useProjectStore = create<ProjectState>()(
  devtools(
    (set, get) => ({
      // Initial state
      selectedProject: null,
      selectedProjects: [],
      isCreating: false,
      isUpdating: false,
      isDeleting: false,
      filters: initialFilters,
      viewMode: 'grid',

      // Actions
      setSelectedProject: (project) =>
        set({ selectedProject: project }, false, 'setSelectedProject'),

      setSelectedProjects: (ids) =>
        set({ selectedProjects: ids }, false, 'setSelectedProjects'),

      toggleProjectSelection: (id) => {
        const current = get().selectedProjects
        const newSelection = current.includes(id)
          ? current.filter(selectedId => selectedId !== id)
          : [...current, id]

        set({ selectedProjects: newSelection }, false, 'toggleProjectSelection')
      },

      clearSelection: () =>
        set({
          selectedProject: null,
          selectedProjects: []
        }, false, 'clearSelection'),

      // Filter actions
      setSearch: (search) =>
        set(
          (state) => ({
            filters: { ...state.filters, search }
          }),
          false,
          'setSearch'
        ),

      setSortBy: (sortBy) =>
        set(
          (state) => ({
            filters: { ...state.filters, sortBy }
          }),
          false,
          'setSortBy'
        ),

      setSortOrder: (sortOrder) =>
        set(
          (state) => ({
            filters: { ...state.filters, sortOrder }
          }),
          false,
          'setSortOrder'
        ),

      setStatusFilter: (status) =>
        set(
          (state) => ({
            filters: { ...state.filters, status }
          }),
          false,
          'setStatusFilter'
        ),

      setPortfolioFilter: (portfolioId) =>
        set(
          (state) => ({
            filters: { ...state.filters, portfolioId }
          }),
          false,
          'setPortfolioFilter'
        ),

      resetFilters: () =>
        set({ filters: initialFilters }, false, 'resetFilters'),

      // UI actions
      setViewMode: (viewMode) =>
        set({ viewMode }, false, 'setViewMode'),

      setCreating: (isCreating) =>
        set({ isCreating }, false, 'setCreating'),

      setUpdating: (isUpdating) =>
        set({ isUpdating }, false, 'setUpdating'),

      setDeleting: (isDeleting) =>
        set({ isDeleting }, false, 'setDeleting'),
    }),
    {
      name: 'project-store',
    }
  )
)

// Selectors for better performance
export const useProjectFilters = () => useProjectStore(state => state.filters)
export const useSelectedProject = () => useProjectStore(state => state.selectedProject)
export const useSelectedProjects = () => useProjectStore(state => state.selectedProjects)
export const useProjectViewMode = () => useProjectStore(state => state.viewMode)
export const useProjectLoadingStates = () => useProjectStore(state => ({
  isCreating: state.isCreating,
  isUpdating: state.isUpdating,
  isDeleting: state.isDeleting,
}))
