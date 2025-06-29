"use client";

import { ProjectCard } from "@/components/projects/ProjectCard";
import { ProjectForm } from "@/components/projects/ProjectForm";
import { ConfirmDialog } from "@/components/shared/ConfirmDialog";
import { EmptyState } from "@/components/shared/EmptyState";
import { LoadingSpinner } from "@/components/shared/LoadingSpinner";
import {
  useCreateProject,
  useDeleteProject,
  useProjects,
  useUpdateProject,
  useUpdateProjectStatus,
} from "@/lib/hooks/useProjects";
import { useProjectFilters } from "@/stores/projectStore";
import type { Project } from "@/types/projects";
import { PlusIcon } from "@heroicons/react/24/outline";
import { useState } from "react";

export default function ProjectsPage() {
  const filters = useProjectFilters();

  const [showCreateForm, setShowCreateForm] = useState(false);
  const [editingProject, setEditingProject] = useState<Project | null>(null);
  const [deleteConfirm, setDeleteConfirm] = useState<Project | null>(null);

  // Fetch projects with current filters
  const {
    data: projectsResponse,
    isLoading,
    isError,
    error,
    refetch,
  } = useProjects(filters);

  const createMutation = useCreateProject();
  const updateMutation = useUpdateProject();
  const deleteMutation = useDeleteProject();
  const updateStatusMutation = useUpdateProjectStatus();

  const projects = projectsResponse?.data ?? [];
  const totalCount = projectsResponse?.total ?? 0;

  const handleCreateNew = () => {
    setShowCreateForm(true);
  };

  const handleEdit = (project: Project) => {
    setEditingProject(project);
  };

  const handleDelete = async (project: Project) => {
    try {
      await deleteMutation.mutateAsync(project.id);
      setDeleteConfirm(null);
    } catch (error) {
      console.error("Failed to delete project:", error);
    }
  };

  const handleStatusChange = async (
    project: Project,
    status: Project["status"]
  ) => {
    try {
      await updateStatusMutation.mutateAsync({ id: project.id, status });
    } catch (error) {
      console.error("Failed to update project status:", error);
    }
  };

  const handleCreateSubmit = async (data: {
    title: string;
    description: string;
    content?: string;
    status: Project["status"];
    technologies: string[];
    startDate: string;
    endDate?: string;
    portfolioId: string;
  }) => {
    await createMutation.mutateAsync({
      title: data.title,
      description: data.description,
      content: data.content ?? "",
      status: data.status,
      technologies: data.technologies,
      startDate: data.startDate,
      endDate: data.endDate || undefined,
      portfolioId: data.portfolioId,
    });
  };

  const handleUpdateSubmit = async (data: {
    title: string;
    description: string;
    content?: string;
    status: Project["status"];
    technologies: string[];
    startDate: string;
    endDate?: string;
    portfolioId: string;
  }) => {
    if (editingProject) {
      await updateMutation.mutateAsync({
        id: editingProject.id,
        data: {
          title: data.title,
          description: data.description,
          content: data.content,
          status: data.status,
          technologies: data.technologies,
          startDate: data.startDate,
          endDate: data.endDate || undefined,
          portfolioId: data.portfolioId,
        },
      });
    }
  };

  const handleCloseCreateForm = () => {
    setShowCreateForm(false);
  };

  const handleCloseEditForm = () => {
    setEditingProject(null);
  };

  const isDeleting = deleteMutation.isPending;

  if (isLoading) {
    return (
      <div className="flex items-center justify-center py-12">
        <LoadingSpinner size="lg" />
      </div>
    );
  }

  if (isError) {
    return (
      <div className="text-center py-12">
        <div className="text-red-600 mb-4">
          Failed to load projects: {error?.message}
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
          <h1 className="text-2xl font-bold text-gray-900">Projects</h1>
          <p className="text-gray-600 mt-1">
            {totalCount} {totalCount === 1 ? "project" : "projects"} total
          </p>
        </div>

        <button
          onClick={handleCreateNew}
          className="flex items-center px-4 py-2 bg-blue-600 text-white rounded-lg hover:bg-blue-700 transition-colors"
        >
          <PlusIcon className="w-5 h-5 mr-2" />
          New Project
        </button>
      </div>

      {/* Projects Grid */}
      {projects.length === 0 ? (
        <EmptyState
          title="No projects found"
          description="Create your first project to get started"
          action={{
            label: "Create Project",
            onClick: handleCreateNew,
          }}
        />
      ) : (
        <div className="grid grid-cols-1 md:grid-cols-2 lg:grid-cols-3 gap-6">
          {projects.map((project) => (
            <ProjectCard
              key={project.id}
              project={project}
              onEdit={handleEdit}
              onDelete={(p) => setDeleteConfirm(p)}
              onStatusChange={handleStatusChange}
              showActions={true}
              selectable={false}
            />
          ))}
        </div>
      )}

      {/* Create Project Form */}
      <ProjectForm
        open={showCreateForm}
        onClose={handleCloseCreateForm}
        onSubmit={handleCreateSubmit}
        isLoading={createMutation.isPending}
      />

      {/* Edit Project Form */}
      <ProjectForm
        open={!!editingProject}
        onClose={handleCloseEditForm}
        onSubmit={handleUpdateSubmit}
        project={editingProject ?? undefined}
        isLoading={updateMutation.isPending}
      />

      {/* Delete Confirmation Dialog */}
      <ConfirmDialog
        open={!!deleteConfirm}
        onClose={() => setDeleteConfirm(null)}
        onConfirm={() => deleteConfirm && handleDelete(deleteConfirm)}
        title="Delete Project"
        description={`Are you sure you want to delete "${deleteConfirm?.title}"? This action cannot be undone.`}
        confirmLabel="Delete"
        confirmVariant="danger"
        isLoading={isDeleting}
      />
    </div>
  );
}
