import type { Portfolio } from '@/types/portfolio'
import { create } from 'zustand'
import { devtools } from 'zustand/middleware'

interface PortfolioState {
  // State
  selectedPortfolio: Portfolio | null
  selectedPortfolios: string[]
  isCreating: boolean
  isUpdating: boolean
  isDeleting: boolean

  // Filters and UI state
  filters: {
    search: string
    sortBy: 'name' | 'createdAt' | 'updatedAt'
    sortOrder: 'asc' | 'desc'
    isPublic?: boolean
  }

  viewMode: 'grid' | 'list'

  // Actions
  setSelectedPortfolio: (portfolio: Portfolio | null) => void
  setSelectedPortfolios: (ids: string[]) => void
  togglePortfolioSelection: (id: string) => void
  clearSelection: () => void

  // Filter actions
  setSearch: (search: string) => void
  setSortBy: (sortBy: PortfolioState['filters']['sortBy']) => void
  setSortOrder: (sortOrder: PortfolioState['filters']['sortOrder']) => void
  setIsPublicFilter: (isPublic?: boolean) => void
  resetFilters: () => void

  // UI actions
  setViewMode: (mode: PortfolioState['viewMode']) => void
  setCreating: (isCreating: boolean) => void
  setUpdating: (isUpdating: boolean) => void
  setDeleting: (isDeleting: boolean) => void
}

const initialFilters = {
  search: '',
  sortBy: 'updatedAt' as const,
  sortOrder: 'desc' as const,
  isPublic: undefined,
}

export const usePortfolioStore = create<PortfolioState>()(
  devtools(
    (set, get) => ({
      // Initial state
      selectedPortfolio: null,
      selectedPortfolios: [],
      isCreating: false,
      isUpdating: false,
      isDeleting: false,
      filters: initialFilters,
      viewMode: 'grid',

      // Actions
      setSelectedPortfolio: (portfolio) =>
        set({ selectedPortfolio: portfolio }, false, 'setSelectedPortfolio'),

      setSelectedPortfolios: (ids) =>
        set({ selectedPortfolios: ids }, false, 'setSelectedPortfolios'),

      togglePortfolioSelection: (id) => {
        const current = get().selectedPortfolios
        const newSelection = current.includes(id)
          ? current.filter(selectedId => selectedId !== id)
          : [...current, id]

        set({ selectedPortfolios: newSelection }, false, 'togglePortfolioSelection')
      },

      clearSelection: () =>
        set({
          selectedPortfolio: null,
          selectedPortfolios: []
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

      setIsPublicFilter: (isPublic) =>
        set(
          (state) => ({
            filters: { ...state.filters, isPublic }
          }),
          false,
          'setIsPublicFilter'
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
      name: 'portfolio-store',
    }
  )
)

// Selectors for better performance
export const usePortfolioFilters = () => usePortfolioStore(state => state.filters)
export const useSelectedPortfolio = () => usePortfolioStore(state => state.selectedPortfolio)
export const useSelectedPortfolios = () => usePortfolioStore(state => state.selectedPortfolios)
export const usePortfolioViewMode = () => usePortfolioStore(state => state.viewMode)
export const usePortfolioLoadingStates = () => usePortfolioStore(state => ({
  isCreating: state.isCreating,
  isUpdating: state.isUpdating,
  isDeleting: state.isDeleting,
}))
