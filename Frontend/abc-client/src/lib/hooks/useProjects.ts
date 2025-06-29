import { projectApi } from '@/lib/api/projects'
import type {
  CreateProjectRequest,
  GetProjectsParams,
  Project,
  UpdateProjectRequest
} from '@/types/projects'
import { useMutation, useQuery, useQueryClient } from '@tanstack/react-query'

// Query key factory
export const projectKeys = {
  all: ['projects'] as const,
  lists: () => [...projectKeys.all, 'list'] as const,
  list: (params?: GetProjectsParams) => [...projectKeys.lists(), params] as const,
  details: () => [...projectKeys.all, 'detail'] as const,
  detail: (id: string) => [...projectKeys.details(), id] as const,
  byPortfolio: (portfolioId: string) => [...projectKeys.all, 'portfolio', portfolioId] as const,
}

// Get all projects
export function useProjects(params?: GetProjectsParams) {
  return useQuery({
    queryKey: projectKeys.list(params),
    queryFn: () => projectApi.getAll(params),
    staleTime: 5 * 60 * 1000, // 5 minutes
    gcTime: 10 * 60 * 1000, // 10 minutes
  })
}

// Get single project
export function useProject(id: string, enabled = true) {
  return useQuery({
    queryKey: projectKeys.detail(id),
    queryFn: () => projectApi.getById(id),
    enabled: enabled && !!id,
    staleTime: 5 * 60 * 1000,
  })
}

// Get projects by portfolio
export function useProjectsByPortfolio(portfolioId: string, enabled = true) {
  return useQuery({
    queryKey: projectKeys.byPortfolio(portfolioId),
    queryFn: () => projectApi.getByPortfolio(portfolioId),
    enabled: enabled && !!portfolioId,
    staleTime: 5 * 60 * 1000,
  })
}

// Create project mutation
export function useCreateProject() {
  const queryClient = useQueryClient()

  return useMutation({
    mutationFn: (data: CreateProjectRequest) => projectApi.create(data),
    onSuccess: (newProject) => {
      // Invalidate and refetch projects list
      queryClient.invalidateQueries({ queryKey: projectKeys.lists() })

      // Add the new project to cache
      queryClient.setQueryData(projectKeys.detail(newProject.id), newProject)

      // Invalidate portfolio projects if this project belongs to a portfolio
      if (newProject.portfolioId) {
        queryClient.invalidateQueries({
          queryKey: projectKeys.byPortfolio(newProject.portfolioId)
        })
      }
    },
  })
}

// Update project mutation
export function useUpdateProject() {
  const queryClient = useQueryClient()

  return useMutation({
    mutationFn: ({ id, data }: { id: string; data: UpdateProjectRequest }) =>
      projectApi.update(id, data),
    onSuccess: (updatedProject) => {
      // Update the specific project in cache
      queryClient.setQueryData(projectKeys.detail(updatedProject.id), updatedProject)

      // Invalidate lists to update any derived data
      queryClient.invalidateQueries({ queryKey: projectKeys.lists() })

      // Invalidate portfolio projects
      if (updatedProject.portfolioId) {
        queryClient.invalidateQueries({
          queryKey: projectKeys.byPortfolio(updatedProject.portfolioId)
        })
      }
    },
  })
}

// Update project status mutation
export function useUpdateProjectStatus() {
  const queryClient = useQueryClient()

  return useMutation({
    mutationFn: ({ id, status }: { id: string; status: Project['status'] }) =>
      projectApi.updateStatus(id, status),
    onSuccess: (updatedProject) => {
      // Update the specific project in cache
      queryClient.setQueryData(projectKeys.detail(updatedProject.id), updatedProject)

      // Invalidate lists to update any derived data
      queryClient.invalidateQueries({ queryKey: projectKeys.lists() })

      // Invalidate portfolio projects
      if (updatedProject.portfolioId) {
        queryClient.invalidateQueries({
          queryKey: projectKeys.byPortfolio(updatedProject.portfolioId)
        })
      }
    },
  })
}

// Delete project mutation
export function useDeleteProject() {
  const queryClient = useQueryClient()

  return useMutation({
    mutationFn: (id: string) => projectApi.delete(id),
    onSuccess: (_, deletedId) => {
      // Remove from cache
      queryClient.removeQueries({ queryKey: projectKeys.detail(deletedId) })

      // Invalidate lists
      queryClient.invalidateQueries({ queryKey: projectKeys.lists() })

      // Invalidate all portfolio project lists (we don't know which portfolio it belonged to)
      queryClient.invalidateQueries({ queryKey: [...projectKeys.all, 'portfolio'] })
    },
  })
}

// Bulk operations
export function useBulkDeleteProjects() {
  const queryClient = useQueryClient()

  return useMutation({
    mutationFn: async (ids: string[]) => {
      await Promise.all(ids.map(id => projectApi.delete(id)))
    },
    onSuccess: (_, deletedIds) => {
      // Remove all deleted projects from cache
      deletedIds.forEach(id => {
        queryClient.removeQueries({ queryKey: projectKeys.detail(id) })
      })

      // Invalidate lists
      queryClient.invalidateQueries({ queryKey: projectKeys.lists() })

      // Invalidate all portfolio project lists
      queryClient.invalidateQueries({ queryKey: [...projectKeys.all, 'portfolio'] })
    },
  })
}
