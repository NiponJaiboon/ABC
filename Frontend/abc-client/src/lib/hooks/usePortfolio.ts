import { portfolioApi } from '@/lib/api/portfolio'
import type {
  CreatePortfolioRequest,
  GetPortfoliosParams,
  UpdatePortfolioRequest
} from '@/types/portfolio'
import { useMutation, useQuery, useQueryClient } from '@tanstack/react-query'

// Query key factory
export const portfolioKeys = {
  all: ['portfolios'] as const,
  lists: () => [...portfolioKeys.all, 'list'] as const,
  list: (params?: GetPortfoliosParams) => [...portfolioKeys.lists(), params] as const,
  details: () => [...portfolioKeys.all, 'detail'] as const,
  detail: (id: string) => [...portfolioKeys.details(), id] as const,
  stats: (id: string) => [...portfolioKeys.detail(id), 'stats'] as const,
  projects: (id: string) => [...portfolioKeys.detail(id), 'projects'] as const,
}

// Get all portfolios
export function usePortfolios(params?: GetPortfoliosParams) {
  return useQuery({
    queryKey: portfolioKeys.list(params),
    queryFn: () => portfolioApi.getAll(params),
    staleTime: 5 * 60 * 1000, // 5 minutes
    gcTime: 10 * 60 * 1000, // 10 minutes
  })
}

// Get single portfolio
export function usePortfolio(id: string, enabled = true) {
  return useQuery({
    queryKey: portfolioKeys.detail(id),
    queryFn: () => portfolioApi.getById(id),
    enabled: enabled && !!id,
    staleTime: 5 * 60 * 1000,
  })
}

// Get portfolio statistics
export function usePortfolioStats(id: string, enabled = true) {
  return useQuery({
    queryKey: portfolioKeys.stats(id),
    queryFn: () => portfolioApi.getStats(id),
    enabled: enabled && !!id,
    staleTime: 2 * 60 * 1000, // 2 minutes
  })
}

// Get portfolio projects
export function usePortfolioProjects(id: string, enabled = true) {
  return useQuery({
    queryKey: portfolioKeys.projects(id),
    queryFn: () => portfolioApi.getProjects(id),
    enabled: enabled && !!id,
    staleTime: 5 * 60 * 1000,
  })
}

// Create portfolio mutation
export function useCreatePortfolio() {
  const queryClient = useQueryClient()

  return useMutation({
    mutationFn: (data: CreatePortfolioRequest) => portfolioApi.create(data),
    onSuccess: (newPortfolio) => {
      // Invalidate and refetch portfolios list
      queryClient.invalidateQueries({ queryKey: portfolioKeys.lists() })

      // Add the new portfolio to cache
      queryClient.setQueryData(portfolioKeys.detail(newPortfolio.id), newPortfolio)
    },
  })
}

// Update portfolio mutation
export function useUpdatePortfolio() {
  const queryClient = useQueryClient()

  return useMutation({
    mutationFn: ({ id, data }: { id: string; data: UpdatePortfolioRequest }) =>
      portfolioApi.update(id, data),
    onSuccess: (updatedPortfolio) => {
      // Update the specific portfolio in cache
      queryClient.setQueryData(portfolioKeys.detail(updatedPortfolio.id), updatedPortfolio)

      // Invalidate lists to update any derived data
      queryClient.invalidateQueries({ queryKey: portfolioKeys.lists() })
    },
  })
}

// Delete portfolio mutation
export function useDeletePortfolio() {
  const queryClient = useQueryClient()

  return useMutation({
    mutationFn: (id: string) => portfolioApi.delete(id),
    onSuccess: (_, deletedId) => {
      // Remove from cache
      queryClient.removeQueries({ queryKey: portfolioKeys.detail(deletedId) })

      // Invalidate lists
      queryClient.invalidateQueries({ queryKey: portfolioKeys.lists() })
    },
  })
}

// Bulk operations
export function useBulkDeletePortfolios() {
  const queryClient = useQueryClient()

  return useMutation({
    mutationFn: async (ids: string[]) => {
      await Promise.all(ids.map(id => portfolioApi.delete(id)))
    },
    onSuccess: (_, deletedIds) => {
      // Remove all deleted portfolios from cache
      deletedIds.forEach(id => {
        queryClient.removeQueries({ queryKey: portfolioKeys.detail(id) })
      })

      // Invalidate lists
      queryClient.invalidateQueries({ queryKey: portfolioKeys.lists() })
    },
  })
}
