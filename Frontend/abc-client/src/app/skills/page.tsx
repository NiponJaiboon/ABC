"use client";

import { EmptyState } from "@/components/shared/EmptyState";
import { LoadingSpinner } from "@/components/shared/LoadingSpinner";
import {
  SkillFilters,
  type SkillFilters as SkillFiltersType,
} from "@/components/skills/SkillFilters";
import { SkillForm } from "@/components/skills/SkillForm";
import { SkillList } from "@/components/skills/SkillList";
import {
  useCreateSkill,
  useDeleteSkill,
  useSkills,
  useUpdateSkill,
} from "@/lib/hooks/useSkills";
import { Skill } from "@/types/skills";
import {
  ChartBarIcon,
  ListBulletIcon,
  PlusIcon,
  Squares2X2Icon,
} from "@heroicons/react/24/outline";
import { useState } from "react";

export default function SkillsPage() {
  const [viewMode, setViewMode] = useState<"grid" | "list" | "chart">("grid");
  const [filters, setFilters] = useState<SkillFiltersType>({
    search: "",
    category: "",
    proficiencyLevel: "",
    sortBy: "name",
    sortOrder: "asc",
  });
  const [formState, setFormState] = useState<{
    isOpen: boolean;
    skill?: Skill;
  }>({ isOpen: false });

  // Fetch skills with current filters
  const {
    data: skillsResponse,
    isLoading,
    isError,
    error,
    refetch,
  } = useSkills(filters);

  const createSkillMutation = useCreateSkill();
  const updateSkillMutation = useUpdateSkill();
  const deleteSkillMutation = useDeleteSkill();

  const skills = skillsResponse?.data ?? [];
  const totalCount = skillsResponse?.total ?? 0;

  const handleCreateNew = () => {
    setFormState({ isOpen: true, skill: undefined });
  };

  const handleEdit = (skill: Skill) => {
    setFormState({ isOpen: true, skill });
  };

  const handleDelete = async (skillId: string) => {
    try {
      await deleteSkillMutation.mutateAsync(skillId);
    } catch (error) {
      console.error("Failed to delete skill:", error);
    }
  };

  const handleBulkDelete = async (skillIds: string[]) => {
    try {
      await Promise.all(
        skillIds.map((id) => deleteSkillMutation.mutateAsync(id))
      );
    } catch (error) {
      console.error("Failed to delete skills:", error);
    }
  };

  const handleFormSubmit = async (data: any) => {
    try {
      if (formState.skill) {
        await updateSkillMutation.mutateAsync({
          id: formState.skill.id,
          data,
        });
      } else {
        await createSkillMutation.mutateAsync(data);
      }
    } catch (error) {
      console.error("Failed to save skill:", error);
      throw error;
    }
  };

  const handleCloseForm = () => {
    setFormState({ isOpen: false, skill: undefined });
  };

  if (isError) {
    return (
      <div className="text-center py-12">
        <div className="text-red-600 mb-4">
          Failed to load skills: {error?.message}
        </div>
        <button
          onClick={() => refetch()}
          className="px-4 py-2 bg-blue-600 text-white rounded-lg hover:bg-blue-700 transition-colors"
        >
          Try Again
        </button>
      </div>
    );
  }

  return (
    <div className="max-w-7xl mx-auto px-4 sm:px-6 lg:px-8 py-8">
      {/* Header */}
      <div className="flex items-center justify-between mb-8">
        <div>
          <h1 className="text-2xl font-bold text-gray-900">Skills</h1>
          <p className="text-gray-600 mt-1">
            {totalCount} {totalCount === 1 ? "skill" : "skills"} total
          </p>
        </div>

        <div className="flex items-center space-x-3">
          {/* View Toggle */}
          <div className="flex items-center border border-gray-200 rounded-lg">
            <button
              onClick={() => setViewMode("grid")}
              className={`p-2 ${
                viewMode === "grid"
                  ? "bg-blue-500 text-white"
                  : "text-gray-400 hover:text-gray-600"
              } transition-colors`}
              title="Grid view"
            >
              <Squares2X2Icon className="w-5 h-5" />
            </button>
            <button
              onClick={() => setViewMode("list")}
              className={`p-2 ${
                viewMode === "list"
                  ? "bg-blue-500 text-white"
                  : "text-gray-400 hover:text-gray-600"
              } transition-colors`}
              title="List view"
            >
              <ListBulletIcon className="w-5 h-5" />
            </button>
            <button
              onClick={() => setViewMode("chart")}
              className={`p-2 ${
                viewMode === "chart"
                  ? "bg-blue-500 text-white"
                  : "text-gray-400 hover:text-gray-600"
              } transition-colors`}
              title="Chart view"
            >
              <ChartBarIcon className="w-5 h-5" />
            </button>
          </div>

          <button
            onClick={handleCreateNew}
            className="flex items-center px-4 py-2 bg-blue-600 text-white rounded-lg hover:bg-blue-700 transition-colors"
          >
            <PlusIcon className="w-5 h-5 mr-2" />
            Add Skill
          </button>
        </div>
      </div>

      {/* Filters */}
      <div className="mb-6">
        <SkillFilters onFiltersChange={setFilters} initialFilters={filters} />
      </div>

      {/* Content */}
      {isLoading ? (
        <div className="flex items-center justify-center py-12">
          <LoadingSpinner size="lg" />
        </div>
      ) : skills.length === 0 ? (
        <EmptyState
          title="No skills found"
          description="Add your first skill to track your expertise"
          action={{
            label: "Add Skill",
            onClick: handleCreateNew,
          }}
        />
      ) : (
        <SkillList
          skills={skills}
          isLoading={isLoading}
          viewMode={viewMode}
          onEdit={handleEdit}
          onDelete={handleDelete}
          onBulkDelete={handleBulkDelete}
        />
      )}

      {/* Skill Form Modal */}
      <SkillForm
        skill={formState.skill}
        isOpen={formState.isOpen}
        onClose={handleCloseForm}
        onSubmit={handleFormSubmit}
        isLoading={
          createSkillMutation.isPending || updateSkillMutation.isPending
        }
      />
    </div>
  );
}
