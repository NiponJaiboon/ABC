import { skillApi } from '@/lib/api/skills'
import type {
    CreateSkillRequest,
    GetSkillsParams,
    Skill,
    UpdateSkillRequest
} from '@/types/skills'
import { useMutation, useQuery, useQueryClient } from '@tanstack/react-query'

// Query key factory
export const skillKeys = {
  all: ['skills'] as const,
  lists: () => [...skillKeys.all, 'list'] as const,
  list: (params?: GetSkillsParams) => [...skillKeys.lists(), params] as const,
  details: () => [...skillKeys.all, 'detail'] as const,
  detail: (id: string) => [...skillKeys.details(), id] as const,
  categories: () => [...skillKeys.all, 'categories'] as const,
}

// Get all skills
export function useSkills(params?: GetSkillsParams) {
  return useQuery({
    queryKey: skillKeys.list(params),
    queryFn: () => skillApi.getAll(params),
    staleTime: 5 * 60 * 1000, // 5 minutes
    gcTime: 10 * 60 * 1000, // 10 minutes
  })
}

// Get single skill
export function useSkill(id: string, enabled = true) {
  return useQuery({
    queryKey: skillKeys.detail(id),
    queryFn: () => skillApi.getById(id),
    enabled: enabled && !!id,
    staleTime: 5 * 60 * 1000,
  })
}

// Get skill categories
export function useSkillCategories() {
  return useQuery({
    queryKey: skillKeys.categories(),
    queryFn: () => skillApi.getCategories(),
    staleTime: 15 * 60 * 1000, // 15 minutes - categories don't change often
  })
}

// Create skill mutation
export function useCreateSkill() {
  const queryClient = useQueryClient()

  return useMutation({
    mutationFn: (data: CreateSkillRequest) => skillApi.create(data),
    onSuccess: (newSkill) => {
      // Invalidate and refetch skills list
      queryClient.invalidateQueries({ queryKey: skillKeys.lists() })

      // Add the new skill to cache
      queryClient.setQueryData(skillKeys.detail(newSkill.id), newSkill)

      // Invalidate categories to update counts
      queryClient.invalidateQueries({ queryKey: skillKeys.categories() })
    },
  })
}

// Update skill mutation
export function useUpdateSkill() {
  const queryClient = useQueryClient()

  return useMutation({
    mutationFn: ({ id, data }: { id: string; data: UpdateSkillRequest }) =>
      skillApi.update(id, data),
    onSuccess: (updatedSkill) => {
      // Update the specific skill in cache
      queryClient.setQueryData(skillKeys.detail(updatedSkill.id), updatedSkill)

      // Invalidate lists to update any derived data
      queryClient.invalidateQueries({ queryKey: skillKeys.lists() })

      // Invalidate categories to update counts
      queryClient.invalidateQueries({ queryKey: skillKeys.categories() })
    },
  })
}

// Update skill proficiency mutation
export function useUpdateSkillProficiency() {
  const queryClient = useQueryClient()

  return useMutation({
    mutationFn: ({ id, proficiencyLevel }: { id: string; proficiencyLevel: Skill['proficiencyLevel'] }) =>
      skillApi.updateProficiency(id, proficiencyLevel),
    onSuccess: (updatedSkill) => {
      // Update the specific skill in cache
      queryClient.setQueryData(skillKeys.detail(updatedSkill.id), updatedSkill)

      // Invalidate lists to update any derived data
      queryClient.invalidateQueries({ queryKey: skillKeys.lists() })
    },
  })
}

// Delete skill mutation
export function useDeleteSkill() {
  const queryClient = useQueryClient()

  return useMutation({
    mutationFn: (id: string) => skillApi.delete(id),
    onSuccess: (_, deletedId) => {
      // Remove from cache
      queryClient.removeQueries({ queryKey: skillKeys.detail(deletedId) })

      // Invalidate lists
      queryClient.invalidateQueries({ queryKey: skillKeys.lists() })

      // Invalidate categories to update counts
      queryClient.invalidateQueries({ queryKey: skillKeys.categories() })
    },
  })
}

// Bulk create skills mutation
export function useBulkCreateSkills() {
  const queryClient = useQueryClient()

  return useMutation({
    mutationFn: (skills: CreateSkillRequest[]) => skillApi.bulkCreate(skills),
    onSuccess: (newSkills) => {
      // Add all new skills to cache
      newSkills.forEach(skill => {
        queryClient.setQueryData(skillKeys.detail(skill.id), skill)
      })

      // Invalidate lists
      queryClient.invalidateQueries({ queryKey: skillKeys.lists() })

      // Invalidate categories to update counts
      queryClient.invalidateQueries({ queryKey: skillKeys.categories() })
    },
  })
}

// Bulk operations
export function useBulkDeleteSkills() {
  const queryClient = useQueryClient()

  return useMutation({
    mutationFn: async (ids: string[]) => {
      await Promise.all(ids.map(id => skillApi.delete(id)))
    },
    onSuccess: (_, deletedIds) => {
      // Remove all deleted skills from cache
      deletedIds.forEach(id => {
        queryClient.removeQueries({ queryKey: skillKeys.detail(id) })
      })

      // Invalidate lists
      queryClient.invalidateQueries({ queryKey: skillKeys.lists() })

      // Invalidate categories to update counts
      queryClient.invalidateQueries({ queryKey: skillKeys.categories() })
    },
  })
}
