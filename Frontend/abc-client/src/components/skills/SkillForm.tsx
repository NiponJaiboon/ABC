"use client";

import { LoadingSpinner } from "@/components/shared/LoadingSpinner";
import { ProficiencyLevel, Skill, SkillCategory } from "@/types/skills";
import { XMarkIcon } from "@heroicons/react/24/outline";
import { useState } from "react";
import { useForm } from "react-hook-form";

interface SkillFormData {
  name: string;
  category: SkillCategory;
  proficiencyLevel: ProficiencyLevel;
  yearsOfExperience: number;
  description?: string;
  tags: string[];
  certifications: string[];
  lastUsed?: string;
  isActive: boolean;
}

interface SkillFormProps {
  skill?: Skill;
  isOpen: boolean;
  onClose: () => void;
  onSubmit: (data: SkillFormData) => Promise<void>;
  isLoading?: boolean;
}

const SKILL_CATEGORIES: { value: SkillCategory; label: string }[] = [
  { value: "Technical", label: "Technical" },
  { value: "Programming", label: "Programming" },
  { value: "Framework", label: "Framework" },
  { value: "Database", label: "Database" },
  { value: "DevOps", label: "DevOps" },
  { value: "Design", label: "Design" },
  { value: "Management", label: "Management" },
  { value: "Language", label: "Language" },
  { value: "Other", label: "Other" },
];

const PROFICIENCY_LEVELS: {
  value: ProficiencyLevel;
  label: string;
  description: string;
}[] = [
  { value: "Beginner", label: "Beginner", description: "Basic knowledge" },
  {
    value: "Intermediate",
    label: "Intermediate",
    description: "Can work with supervision",
  },
  {
    value: "Advanced",
    label: "Advanced",
    description: "Can work independently",
  },
  {
    value: "Expert",
    label: "Expert",
    description: "Can teach and mentor others",
  },
];

export function SkillForm({
  skill,
  isOpen,
  onClose,
  onSubmit,
  isLoading = false,
}: SkillFormProps) {
  const [tags, setTags] = useState<string[]>(skill?.tags || []);
  const [certifications, setCertifications] = useState<string[]>(
    skill?.certifications || []
  );
  const [newTag, setNewTag] = useState("");
  const [newCertification, setNewCertification] = useState("");

  const {
    register,
    handleSubmit,
    formState: { errors },
    reset,
    watch,
  } = useForm<SkillFormData>({
    defaultValues: {
      name: skill?.name || "",
      category: skill?.category || "Technical",
      proficiencyLevel: skill?.proficiencyLevel || "Beginner",
      yearsOfExperience: skill?.yearsOfExperience || 0,
      description: skill?.description || "",
      tags: skill?.tags || [],
      certifications: skill?.certifications || [],
      lastUsed: skill?.lastUsed
        ? new Date(skill.lastUsed).toISOString().split("T")[0]
        : "",
      isActive: skill?.isActive !== false,
    },
  });

  const proficiencyLevel = watch("proficiencyLevel");

  const handleFormSubmit = async (data: SkillFormData) => {
    try {
      await onSubmit({
        ...data,
        tags,
        certifications,
        lastUsed: data.lastUsed || undefined,
      });
      reset();
      setTags([]);
      setCertifications([]);
      setNewTag("");
      setNewCertification("");
      onClose();
    } catch (error) {
      console.error("Failed to save skill:", error);
    }
  };

  const addTag = () => {
    if (newTag.trim() && !tags.includes(newTag.trim())) {
      setTags([...tags, newTag.trim()]);
      setNewTag("");
    }
  };

  const removeTag = (tagToRemove: string) => {
    setTags(tags.filter((tag) => tag !== tagToRemove));
  };

  const addCertification = () => {
    if (
      newCertification.trim() &&
      !certifications.includes(newCertification.trim())
    ) {
      setCertifications([...certifications, newCertification.trim()]);
      setNewCertification("");
    }
  };

  const removeCertification = (certToRemove: string) => {
    setCertifications(certifications.filter((cert) => cert !== certToRemove));
  };

  if (!isOpen) return null;

  return (
    <div className="fixed inset-0 bg-gray-600 bg-opacity-50 overflow-y-auto h-full w-full z-50">
      <div className="relative top-20 mx-auto p-5 border w-full max-w-2xl shadow-lg rounded-md bg-white">
        <div className="flex items-center justify-between mb-4">
          <h3 className="text-lg font-medium text-gray-900">
            {skill ? "Edit Skill" : "Add New Skill"}
          </h3>
          <button
            onClick={onClose}
            className="text-gray-400 hover:text-gray-600"
            disabled={isLoading}
          >
            <XMarkIcon className="w-6 h-6" />
          </button>
        </div>

        <form onSubmit={handleSubmit(handleFormSubmit)} className="space-y-6">
          {/* Basic Information */}
          <div className="grid grid-cols-1 md:grid-cols-2 gap-4">
            <div>
              <label className="block text-sm font-medium text-gray-700 mb-1">
                Skill Name *
              </label>
              <input
                {...register("name", { required: "Skill name is required" })}
                className="w-full px-3 py-2 border border-gray-300 rounded-md focus:outline-none focus:ring-2 focus:ring-blue-500"
                placeholder="e.g., React, Python, Project Management"
                disabled={isLoading}
              />
              {errors.name && (
                <p className="text-red-600 text-sm mt-1">
                  {errors.name.message}
                </p>
              )}
            </div>

            <div>
              <label className="block text-sm font-medium text-gray-700 mb-1">
                Category *
              </label>
              <select
                {...register("category", { required: "Category is required" })}
                className="w-full px-3 py-2 border border-gray-300 rounded-md focus:outline-none focus:ring-2 focus:ring-blue-500"
                disabled={isLoading}
              >
                {SKILL_CATEGORIES.map((category) => (
                  <option key={category.value} value={category.value}>
                    {category.label}
                  </option>
                ))}
              </select>
              {errors.category && (
                <p className="text-red-600 text-sm mt-1">
                  {errors.category.message}
                </p>
              )}
            </div>
          </div>

          {/* Proficiency and Experience */}
          <div className="grid grid-cols-1 md:grid-cols-2 gap-4">
            <div>
              <label className="block text-sm font-medium text-gray-700 mb-1">
                Proficiency Level *
              </label>
              <select
                {...register("proficiencyLevel", {
                  required: "Proficiency level is required",
                })}
                className="w-full px-3 py-2 border border-gray-300 rounded-md focus:outline-none focus:ring-2 focus:ring-blue-500"
                disabled={isLoading}
              >
                {PROFICIENCY_LEVELS.map((level) => (
                  <option key={level.value} value={level.value}>
                    {level.label}
                  </option>
                ))}
              </select>
              {proficiencyLevel && (
                <p className="text-sm text-gray-500 mt-1">
                  {
                    PROFICIENCY_LEVELS.find((l) => l.value === proficiencyLevel)
                      ?.description
                  }
                </p>
              )}
              {errors.proficiencyLevel && (
                <p className="text-red-600 text-sm mt-1">
                  {errors.proficiencyLevel.message}
                </p>
              )}
            </div>

            <div>
              <label className="block text-sm font-medium text-gray-700 mb-1">
                Years of Experience
              </label>
              <input
                {...register("yearsOfExperience", {
                  valueAsNumber: true,
                  min: { value: 0, message: "Experience cannot be negative" },
                })}
                type="number"
                min="0"
                step="0.5"
                className="w-full px-3 py-2 border border-gray-300 rounded-md focus:outline-none focus:ring-2 focus:ring-blue-500"
                placeholder="0"
                disabled={isLoading}
              />
              {errors.yearsOfExperience && (
                <p className="text-red-600 text-sm mt-1">
                  {errors.yearsOfExperience.message}
                </p>
              )}
            </div>
          </div>

          {/* Description */}
          <div>
            <label className="block text-sm font-medium text-gray-700 mb-1">
              Description
            </label>
            <textarea
              {...register("description")}
              rows={3}
              className="w-full px-3 py-2 border border-gray-300 rounded-md focus:outline-none focus:ring-2 focus:ring-blue-500"
              placeholder="Describe your experience and expertise with this skill..."
              disabled={isLoading}
            />
          </div>

          {/* Tags */}
          <div>
            <label className="block text-sm font-medium text-gray-700 mb-1">
              Tags
            </label>
            <div className="flex flex-wrap gap-2 mb-2">
              {tags.map((tag) => (
                <span
                  key={tag}
                  className="inline-flex items-center px-2 py-1 rounded-full text-xs font-medium bg-blue-100 text-blue-800"
                >
                  {tag}
                  <button
                    type="button"
                    onClick={() => removeTag(tag)}
                    className="ml-1 text-blue-600 hover:text-blue-800"
                    disabled={isLoading}
                  >
                    <XMarkIcon className="w-3 h-3" />
                  </button>
                </span>
              ))}
            </div>
            <div className="flex gap-2">
              <input
                value={newTag}
                onChange={(e) => setNewTag(e.target.value)}
                onKeyPress={(e) => {
                  if (e.key === "Enter") {
                    e.preventDefault();
                    addTag();
                  }
                }}
                className="flex-1 px-3 py-2 border border-gray-300 rounded-md focus:outline-none focus:ring-2 focus:ring-blue-500"
                placeholder="Add a tag..."
                disabled={isLoading}
              />
              <button
                type="button"
                onClick={addTag}
                className="px-3 py-2 bg-gray-100 text-gray-700 rounded-md hover:bg-gray-200 transition-colors"
                disabled={isLoading}
              >
                Add
              </button>
            </div>
          </div>

          {/* Certifications */}
          <div>
            <label className="block text-sm font-medium text-gray-700 mb-1">
              Certifications
            </label>
            <div className="space-y-1 mb-2">
              {certifications.map((cert) => (
                <div
                  key={cert}
                  className="flex items-center justify-between px-3 py-2 bg-green-50 border border-green-200 rounded-md"
                >
                  <span className="text-sm text-green-800">{cert}</span>
                  <button
                    type="button"
                    onClick={() => removeCertification(cert)}
                    className="text-green-600 hover:text-green-800"
                    disabled={isLoading}
                  >
                    <XMarkIcon className="w-4 h-4" />
                  </button>
                </div>
              ))}
            </div>
            <div className="flex gap-2">
              <input
                value={newCertification}
                onChange={(e) => setNewCertification(e.target.value)}
                onKeyPress={(e) => {
                  if (e.key === "Enter") {
                    e.preventDefault();
                    addCertification();
                  }
                }}
                className="flex-1 px-3 py-2 border border-gray-300 rounded-md focus:outline-none focus:ring-2 focus:ring-blue-500"
                placeholder="Add a certification..."
                disabled={isLoading}
              />
              <button
                type="button"
                onClick={addCertification}
                className="px-3 py-2 bg-gray-100 text-gray-700 rounded-md hover:bg-gray-200 transition-colors"
                disabled={isLoading}
              >
                Add
              </button>
            </div>
          </div>

          {/* Additional Fields */}
          <div className="grid grid-cols-1 md:grid-cols-2 gap-4">
            <div>
              <label
                htmlFor="lastUsed"
                className="block text-sm font-medium text-gray-700 mb-1"
              >
                Last Used
              </label>
              <input
                {...register("lastUsed")}
                id="lastUsed"
                type="date"
                className="w-full px-3 py-2 border border-gray-300 rounded-md focus:outline-none focus:ring-2 focus:ring-blue-500"
                disabled={isLoading}
              />
            </div>

            <div className="flex items-center">
              <input
                {...register("isActive")}
                id="isActive"
                type="checkbox"
                className="h-4 w-4 text-blue-600 focus:ring-blue-500 border-gray-300 rounded"
                disabled={isLoading}
              />
              <label
                htmlFor="isActive"
                className="ml-2 block text-sm text-gray-900"
              >
                Currently active skill
              </label>
            </div>
          </div>

          {/* Form Actions */}
          <div className="flex justify-end space-x-3 pt-6 border-t">
            <button
              type="button"
              onClick={onClose}
              className="px-4 py-2 border border-gray-300 rounded-md text-gray-700 hover:bg-gray-50 transition-colors"
              disabled={isLoading}
            >
              Cancel
            </button>
            <button
              type="submit"
              className="px-4 py-2 bg-blue-600 text-white rounded-md hover:bg-blue-700 transition-colors disabled:opacity-50 flex items-center"
              disabled={isLoading}
            >
              {isLoading && <LoadingSpinner size="sm" className="mr-2" />}
              {skill ? "Update Skill" : "Create Skill"}
            </button>
          </div>
        </form>
      </div>
    </div>
  );
}
