import type { Skill } from '@/types/skills'
import { create } from 'zustand'
import { devtools } from 'zustand/middleware'

interface SkillState {
  // State
  selectedSkill: Skill | null
  selectedSkills: string[]
  isCreating: boolean
  isUpdating: boolean
  isDeleting: boolean

  // Filters and UI state
  filters: {
    search: string
    sortBy: 'name' | 'category' | 'proficiencyLevel' | 'lastUsed'
    sortOrder: 'asc' | 'desc'
    category?: string
    proficiencyLevel?: Skill['proficiencyLevel']
  }

  viewMode: 'grid' | 'list' | 'chart'

  // Actions
  setSelectedSkill: (skill: Skill | null) => void
  setSelectedSkills: (ids: string[]) => void
  toggleSkillSelection: (id: string) => void
  clearSelection: () => void

  // Filter actions
  setSearch: (search: string) => void
  setSortBy: (sortBy: SkillState['filters']['sortBy']) => void
  setSortOrder: (sortOrder: SkillState['filters']['sortOrder']) => void
  setCategoryFilter: (category?: string) => void
  setProficiencyFilter: (proficiencyLevel?: Skill['proficiencyLevel']) => void
  resetFilters: () => void

  // UI actions
  setViewMode: (mode: SkillState['viewMode']) => void
  setCreating: (isCreating: boolean) => void
  setUpdating: (isUpdating: boolean) => void
  setDeleting: (isDeleting: boolean) => void
}

const initialFilters = {
  search: '',
  sortBy: 'name' as const,
  sortOrder: 'asc' as const,
  category: undefined,
  proficiencyLevel: undefined,
}

export const useSkillStore = create<SkillState>()(
  devtools(
    (set, get) => ({
      // Initial state
      selectedSkill: null,
      selectedSkills: [],
      isCreating: false,
      isUpdating: false,
      isDeleting: false,
      filters: initialFilters,
      viewMode: 'grid',

      // Actions
      setSelectedSkill: (skill) =>
        set({ selectedSkill: skill }, false, 'setSelectedSkill'),

      setSelectedSkills: (ids) =>
        set({ selectedSkills: ids }, false, 'setSelectedSkills'),

      toggleSkillSelection: (id) => {
        const current = get().selectedSkills
        const newSelection = current.includes(id)
          ? current.filter(selectedId => selectedId !== id)
          : [...current, id]

        set({ selectedSkills: newSelection }, false, 'toggleSkillSelection')
      },

      clearSelection: () =>
        set({
          selectedSkill: null,
          selectedSkills: []
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

      setCategoryFilter: (category) =>
        set(
          (state) => ({
            filters: { ...state.filters, category }
          }),
          false,
          'setCategoryFilter'
        ),

      setProficiencyFilter: (proficiencyLevel) =>
        set(
          (state) => ({
            filters: { ...state.filters, proficiencyLevel }
          }),
          false,
          'setProficiencyFilter'
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
      name: 'skill-store',
    }
  )
)

// Selectors for better performance
export const useSkillFilters = () => useSkillStore(state => state.filters)
export const useSelectedSkill = () => useSkillStore(state => state.selectedSkill)
export const useSelectedSkills = () => useSkillStore(state => state.selectedSkills)
export const useSkillViewMode = () => useSkillStore(state => state.viewMode)
export const useSkillLoadingStates = () => useSkillStore(state => ({
  isCreating: state.isCreating,
  isUpdating: state.isUpdating,
  isDeleting: state.isDeleting,
}))
