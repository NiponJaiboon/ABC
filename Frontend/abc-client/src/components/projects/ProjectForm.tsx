"use client";

import { LoadingSpinner } from "@/components/shared/LoadingSpinner";
import { usePortfolios } from "@/lib/hooks/usePortfolio";
import type { Project } from "@/types/projects";
import { Dialog, DialogPanel, DialogTitle } from "@headlessui/react";
import { PlusIcon, XCircleIcon, XMarkIcon } from "@heroicons/react/24/outline";
import { zodResolver } from "@hookform/resolvers/zod";
import React, { useEffect, useState } from "react";
import { useForm } from "react-hook-form";
import { z } from "zod";

const projectSchema = z.object({
  title: z
    .string()
    .min(1, "Project title is required")
    .max(200, "Title must be less than 200 characters"),
  description: z
    .string()
    .min(1, "Description is required")
    .max(1000, "Description must be less than 1000 characters"),
  content: z
    .string()
    .max(10000, "Content must be less than 10000 characters")
    .optional(),
  status: z.enum(["draft", "in-progress", "completed", "archived"]),
  technologies: z.array(z.string()).default([]),
  startDate: z.string().min(1, "Start date is required"),
  endDate: z.string().optional(),
  portfolioId: z.string().min(1, "Portfolio is required"),
});

type ProjectFormData = z.infer<typeof projectSchema>;

interface ProjectFormProps {
  open: boolean;
  onClose: () => void;
  onSubmit: (data: ProjectFormData) => Promise<void>;
  project?: Project;
  isLoading?: boolean;
}

export function ProjectForm({
  open,
  onClose,
  onSubmit,
  project,
  isLoading = false,
}: Readonly<ProjectFormProps>) {
  const isEditing = !!project;
  const [newTechnology, setNewTechnology] = useState("");

  const {
    register,
    handleSubmit,
    formState: { errors, isSubmitting },
    reset,
    setValue,
    watch,
    getValues,
  } = useForm<ProjectFormData>({
    resolver: zodResolver(projectSchema),
    defaultValues: {
      title: "",
      description: "",
      content: "",
      status: "draft",
      technologies: [],
      startDate: new Date().toISOString().split("T")[0],
      endDate: "",
      portfolioId: "",
    },
  });

  const watchedTechnologies = watch("technologies");

  // Fetch portfolios for dropdown
  const { data: portfoliosResponse } = usePortfolios();
  const portfolios = portfoliosResponse?.data ?? [];

  // Reset form when project changes or dialog opens/closes
  useEffect(() => {
    if (project) {
      setValue("title", project.title);
      setValue("description", project.description);
      setValue("content", project.content ?? "");
      setValue("status", project.status);
      setValue("technologies", project.technologies ?? []);
      setValue("startDate", project.startDate.split("T")[0]);
      setValue("endDate", project.endDate ? project.endDate.split("T")[0] : "");
      setValue("portfolioId", project.portfolioId);
    } else {
      reset({
        title: "",
        description: "",
        content: "",
        status: "draft",
        technologies: [],
        startDate: new Date().toISOString().split("T")[0],
        endDate: "",
        portfolioId: "",
      });
    }
  }, [project, setValue, reset]);

  const onFormSubmit = async (data: ProjectFormData) => {
    try {
      await onSubmit(data);
      reset();
      onClose();
    } catch (error) {
      console.error("Failed to submit project:", error);
    }
  };

  const handleClose = () => {
    if (!isSubmitting && !isLoading) {
      reset();
      setNewTechnology("");
      onClose();
    }
  };

  const addTechnology = () => {
    if (
      newTechnology.trim() &&
      !watchedTechnologies.includes(newTechnology.trim())
    ) {
      const currentTechnologies = getValues("technologies");
      setValue("technologies", [...currentTechnologies, newTechnology.trim()]);
      setNewTechnology("");
    }
  };

  const removeTechnology = (tech: string) => {
    const currentTechnologies = getValues("technologies");
    setValue(
      "technologies",
      currentTechnologies.filter((t) => t !== tech)
    );
  };

  const handleKeyPress = (e: React.KeyboardEvent) => {
    if (e.key === "Enter") {
      e.preventDefault();
      addTechnology();
    }
  };

  return (
    <Dialog open={open} onClose={handleClose} className="relative z-50">
      {/* Backdrop */}
      <div className="fixed inset-0 bg-black/25" aria-hidden="true" />

      {/* Full-screen container */}
      <div className="fixed inset-0 flex items-center justify-center p-4">
        <DialogPanel className="mx-auto max-w-2xl w-full bg-white rounded-lg shadow-xl max-h-[90vh] overflow-y-auto">
          {/* Header */}
          <div className="flex items-center justify-between p-6 border-b border-gray-200">
            <DialogTitle className="text-lg font-semibold text-gray-900">
              {isEditing ? "Edit Project" : "Create New Project"}
            </DialogTitle>
            <button
              onClick={handleClose}
              disabled={isSubmitting || isLoading}
              className="text-gray-400 hover:text-gray-600 disabled:opacity-50"
            >
              <XMarkIcon className="w-6 h-6" />
            </button>
          </div>

          {/* Form */}
          <form onSubmit={handleSubmit(onFormSubmit)} className="p-6 space-y-6">
            {/* Basic Information */}
            <div className="grid grid-cols-1 md:grid-cols-2 gap-4">
              {/* Title */}
              <div className="md:col-span-2">
                <label
                  htmlFor="title"
                  className="block text-sm font-medium text-gray-700 mb-1"
                >
                  Project Title *
                </label>
                <input
                  {...register("title")}
                  type="text"
                  id="title"
                  placeholder="Enter project title"
                  className="w-full px-3 py-2 border border-gray-300 rounded-lg focus:ring-2 focus:ring-blue-500 focus:border-blue-500"
                  disabled={isSubmitting || isLoading}
                />
                {errors.title && (
                  <p className="mt-1 text-sm text-red-600">
                    {errors.title.message}
                  </p>
                )}
              </div>

              {/* Portfolio */}
              <div>
                <label
                  htmlFor="portfolioId"
                  className="block text-sm font-medium text-gray-700 mb-1"
                >
                  Portfolio *
                </label>
                <select
                  {...register("portfolioId")}
                  id="portfolioId"
                  className="w-full px-3 py-2 border border-gray-300 rounded-lg focus:ring-2 focus:ring-blue-500 focus:border-blue-500"
                  disabled={isSubmitting || isLoading}
                >
                  <option value="">Select a portfolio</option>
                  {portfolios.map((portfolio) => (
                    <option key={portfolio.id} value={portfolio.id}>
                      {portfolio.name}
                    </option>
                  ))}
                </select>
                {errors.portfolioId && (
                  <p className="mt-1 text-sm text-red-600">
                    {errors.portfolioId.message}
                  </p>
                )}
              </div>

              {/* Status */}
              <div>
                <label
                  htmlFor="status"
                  className="block text-sm font-medium text-gray-700 mb-1"
                >
                  Status
                </label>
                <select
                  {...register("status")}
                  id="status"
                  className="w-full px-3 py-2 border border-gray-300 rounded-lg focus:ring-2 focus:ring-blue-500 focus:border-blue-500"
                  disabled={isSubmitting || isLoading}
                >
                  <option value="draft">Draft</option>
                  <option value="in-progress">In Progress</option>
                  <option value="completed">Completed</option>
                  <option value="archived">Archived</option>
                </select>
              </div>

              {/* Start Date */}
              <div>
                <label
                  htmlFor="startDate"
                  className="block text-sm font-medium text-gray-700 mb-1"
                >
                  Start Date *
                </label>
                <input
                  {...register("startDate")}
                  type="date"
                  id="startDate"
                  className="w-full px-3 py-2 border border-gray-300 rounded-lg focus:ring-2 focus:ring-blue-500 focus:border-blue-500"
                  disabled={isSubmitting || isLoading}
                />
                {errors.startDate && (
                  <p className="mt-1 text-sm text-red-600">
                    {errors.startDate.message}
                  </p>
                )}
              </div>

              {/* End Date */}
              <div>
                <label
                  htmlFor="endDate"
                  className="block text-sm font-medium text-gray-700 mb-1"
                >
                  End Date
                </label>
                <input
                  {...register("endDate")}
                  type="date"
                  id="endDate"
                  className="w-full px-3 py-2 border border-gray-300 rounded-lg focus:ring-2 focus:ring-blue-500 focus:border-blue-500"
                  disabled={isSubmitting || isLoading}
                />
              </div>
            </div>

            {/* Description */}
            <div>
              <label
                htmlFor="description"
                className="block text-sm font-medium text-gray-700 mb-1"
              >
                Description *
              </label>
              <textarea
                {...register("description")}
                id="description"
                rows={3}
                placeholder="Describe your project..."
                className="w-full px-3 py-2 border border-gray-300 rounded-lg focus:ring-2 focus:ring-blue-500 focus:border-blue-500 resize-none"
                disabled={isSubmitting || isLoading}
              />
              {errors.description && (
                <p className="mt-1 text-sm text-red-600">
                  {errors.description.message}
                </p>
              )}
            </div>

            {/* Technologies */}
            <div>
              <label
                htmlFor="technology-input"
                className="block text-sm font-medium text-gray-700 mb-1"
              >
                Technologies
              </label>

              {/* Add Technology Input */}
              <div className="flex gap-2 mb-3">
                <input
                  id="technology-input"
                  type="text"
                  value={newTechnology}
                  onChange={(e) => setNewTechnology(e.target.value)}
                  onKeyPress={handleKeyPress}
                  placeholder="Add technology (e.g., React, Node.js)"
                  className="flex-1 px-3 py-2 border border-gray-300 rounded-lg focus:ring-2 focus:ring-blue-500 focus:border-blue-500"
                  disabled={isSubmitting || isLoading}
                />
                <button
                  type="button"
                  onClick={addTechnology}
                  disabled={!newTechnology.trim() || isSubmitting || isLoading}
                  className="px-3 py-2 bg-blue-600 text-white rounded-lg hover:bg-blue-700 disabled:opacity-50 transition-colors"
                >
                  <PlusIcon className="w-5 h-5" />
                </button>
              </div>

              {/* Technology Tags */}
              {watchedTechnologies.length > 0 && (
                <div className="flex flex-wrap gap-2">
                  {watchedTechnologies.map((tech) => (
                    <span
                      key={tech}
                      className="inline-flex items-center px-3 py-1 rounded-md text-sm font-medium bg-blue-100 text-blue-800"
                    >
                      {tech}
                      <button
                        type="button"
                        onClick={() => removeTechnology(tech)}
                        className="ml-2 text-blue-600 hover:text-blue-800"
                        disabled={isSubmitting || isLoading}
                      >
                        <XCircleIcon className="w-4 h-4" />
                      </button>
                    </span>
                  ))}
                </div>
              )}
            </div>

            {/* Content */}
            <div>
              <label
                htmlFor="content"
                className="block text-sm font-medium text-gray-700 mb-1"
              >
                Project Content
              </label>
              <textarea
                {...register("content")}
                id="content"
                rows={8}
                placeholder="Detailed project information, requirements, progress notes, etc..."
                className="w-full px-3 py-2 border border-gray-300 rounded-lg focus:ring-2 focus:ring-blue-500 focus:border-blue-500 resize-none"
                disabled={isSubmitting || isLoading}
              />
              {errors.content && (
                <p className="mt-1 text-sm text-red-600">
                  {errors.content.message}
                </p>
              )}
            </div>

            {/* Actions */}
            <div className="flex justify-end space-x-3 pt-4 border-t border-gray-200">
              <button
                type="button"
                onClick={handleClose}
                disabled={isSubmitting || isLoading}
                className="px-4 py-2 text-gray-700 bg-gray-100 hover:bg-gray-200 rounded-lg transition-colors disabled:opacity-50"
              >
                Cancel
              </button>
              <button
                type="submit"
                disabled={isSubmitting || isLoading}
                className="px-4 py-2 bg-blue-600 text-white rounded-lg hover:bg-blue-700 transition-colors disabled:opacity-50 flex items-center"
              >
                {(isSubmitting || isLoading) && (
                  <LoadingSpinner size="sm" className="mr-2" />
                )}
                {isEditing ? "Update Project" : "Create Project"}
              </button>
            </div>
          </form>
        </DialogPanel>
      </div>
    </Dialog>
  );
}
