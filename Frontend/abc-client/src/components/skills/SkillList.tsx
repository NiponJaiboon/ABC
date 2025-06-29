"use client";

import { ConfirmDialog } from "@/components/shared/ConfirmDialog";
import { EmptyState } from "@/components/shared/EmptyState";
import { LoadingSpinner } from "@/components/shared/LoadingSpinner";
import { Skill } from "@/types/skills";
import { PencilIcon, TrashIcon } from "@heroicons/react/24/outline";
import { useState } from "react";
import { SkillCard } from "./SkillCard";

interface SkillListProps {
  skills: Skill[];
  isLoading?: boolean;
  viewMode: "grid" | "list" | "chart";
  onEdit: (skill: Skill) => void;
  onDelete: (skillId: string) => void;
  onBulkDelete?: (skillIds: string[]) => void;
  onProficiencyUpdate?: (skillId: string, newLevel: string) => void;
}

export function SkillList({
  skills,
  isLoading = false,
  viewMode,
  onEdit,
  onDelete,
  onBulkDelete,
  onProficiencyUpdate,
}: SkillListProps) {
  const [selectedSkills, setSelectedSkills] = useState<string[]>([]);
  const [deleteConfirm, setDeleteConfirm] = useState<{
    isOpen: boolean;
    skillId?: string;
    skillName?: string;
    isBulk?: boolean;
  }>({ isOpen: false });

  const handleSelectSkill = (skillId: string, selected: boolean) => {
    if (selected) {
      setSelectedSkills([...selectedSkills, skillId]);
    } else {
      setSelectedSkills(selectedSkills.filter((id) => id !== skillId));
    }
  };

  const handleSelectAll = (selected: boolean) => {
    if (selected) {
      setSelectedSkills(skills.map((skill) => skill.id));
    } else {
      setSelectedSkills([]);
    }
  };

  const handleDeleteClick = (skill: Skill) => {
    setDeleteConfirm({
      isOpen: true,
      skillId: skill.id,
      skillName: skill.name,
      isBulk: false,
    });
  };

  const handleBulkDeleteClick = () => {
    if (selectedSkills.length > 0) {
      setDeleteConfirm({
        isOpen: true,
        isBulk: true,
      });
    }
  };

  const handleDeleteConfirm = () => {
    if (deleteConfirm.isBulk && onBulkDelete) {
      onBulkDelete(selectedSkills);
      setSelectedSkills([]);
    } else if (deleteConfirm.skillId) {
      onDelete(deleteConfirm.skillId);
    }
    setDeleteConfirm({ isOpen: false });
  };

  const getProficiencyColor = (level: string) => {
    switch (level) {
      case "Expert":
        return "bg-purple-100 text-purple-800";
      case "Advanced":
        return "bg-blue-100 text-blue-800";
      case "Intermediate":
        return "bg-green-100 text-green-800";
      case "Beginner":
        return "bg-yellow-100 text-yellow-800";
      default:
        return "bg-gray-100 text-gray-800";
    }
  };

  const getCategoryColor = (category: string) => {
    const colors = {
      Technical: "bg-blue-100 text-blue-800",
      Programming: "bg-purple-100 text-purple-800",
      Framework: "bg-green-100 text-green-800",
      Database: "bg-red-100 text-red-800",
      DevOps: "bg-orange-100 text-orange-800",
      Design: "bg-pink-100 text-pink-800",
      Management: "bg-indigo-100 text-indigo-800",
      Language: "bg-teal-100 text-teal-800",
      Other: "bg-gray-100 text-gray-800",
    };
    return colors[category as keyof typeof colors] || colors.Other;
  };

  if (isLoading) {
    return (
      <div className="flex items-center justify-center py-12">
        <LoadingSpinner size="lg" />
      </div>
    );
  }

  if (skills.length === 0) {
    return (
      <EmptyState
        title="No skills found"
        description="No skills match your current filters"
      />
    );
  }

  // Chart View
  if (viewMode === "chart") {
    const proficiencyStats = skills.reduce((acc, skill) => {
      acc[skill.proficiencyLevel] = (acc[skill.proficiencyLevel] || 0) + 1;
      return acc;
    }, {} as Record<string, number>);

    const categoryStats = skills.reduce((acc, skill) => {
      acc[skill.category] = (acc[skill.category] || 0) + 1;
      return acc;
    }, {} as Record<string, number>);

    return (
      <div className="space-y-8">
        {/* Proficiency Chart */}
        <div className="bg-white p-6 rounded-lg border">
          <h3 className="text-lg font-medium text-gray-900 mb-4">
            Skills by Proficiency Level
          </h3>
          <div className="space-y-3">
            {Object.entries(proficiencyStats).map(([level, count]) => (
              <div key={level} className="flex items-center">
                <div className="w-24 text-sm text-gray-600">{level}</div>
                <div className="flex-1 mx-4">
                  <div className="bg-gray-200 rounded-full h-4">
                    <div
                      className={`h-4 rounded-full ${
                        getProficiencyColor(level).split(" ")[0]
                      }`}
                      style={{
                        width: `${(count / skills.length) * 100}%`,
                      }}
                    />
                  </div>
                </div>
                <div className="w-12 text-sm text-gray-900 text-right">
                  {count}
                </div>
              </div>
            ))}
          </div>
        </div>

        {/* Category Chart */}
        <div className="bg-white p-6 rounded-lg border">
          <h3 className="text-lg font-medium text-gray-900 mb-4">
            Skills by Category
          </h3>
          <div className="space-y-3">
            {Object.entries(categoryStats).map(([category, count]) => (
              <div key={category} className="flex items-center">
                <div className="w-24 text-sm text-gray-600">{category}</div>
                <div className="flex-1 mx-4">
                  <div className="bg-gray-200 rounded-full h-4">
                    <div
                      className={`h-4 rounded-full ${
                        getCategoryColor(category).split(" ")[0]
                      }`}
                      style={{
                        width: `${(count / skills.length) * 100}%`,
                      }}
                    />
                  </div>
                </div>
                <div className="w-12 text-sm text-gray-900 text-right">
                  {count}
                </div>
              </div>
            ))}
          </div>
        </div>
      </div>
    );
  }

  // List View
  if (viewMode === "list") {
    return (
      <div className="space-y-4">
        {/* Bulk Actions */}
        {selectedSkills.length > 0 && (
          <div className="bg-blue-50 border border-blue-200 rounded-lg p-4">
            <div className="flex items-center justify-between">
              <span className="text-sm text-blue-700">
                {selectedSkills.length} skill
                {selectedSkills.length > 1 ? "s" : ""} selected
              </span>
              <div className="flex space-x-2">
                <button
                  onClick={() => setSelectedSkills([])}
                  className="text-sm text-blue-600 hover:text-blue-800"
                >
                  Clear
                </button>
                {onBulkDelete && (
                  <button
                    onClick={handleBulkDeleteClick}
                    className="flex items-center text-sm text-red-600 hover:text-red-800"
                  >
                    <TrashIcon className="w-4 h-4 mr-1" />
                    Delete Selected
                  </button>
                )}
              </div>
            </div>
          </div>
        )}

        {/* Header */}
        <div className="bg-gray-50 rounded-lg p-4">
          <div className="flex items-center">
            <input
              type="checkbox"
              checked={selectedSkills.length === skills.length}
              onChange={(e) => handleSelectAll(e.target.checked)}
              className="h-4 w-4 text-blue-600 focus:ring-blue-500 border-gray-300 rounded"
            />
            <div className="ml-4 grid grid-cols-6 gap-4 flex-1">
              <div className="text-xs font-medium text-gray-500 uppercase tracking-wider">
                Name
              </div>
              <div className="text-xs font-medium text-gray-500 uppercase tracking-wider">
                Category
              </div>
              <div className="text-xs font-medium text-gray-500 uppercase tracking-wider">
                Proficiency
              </div>
              <div className="text-xs font-medium text-gray-500 uppercase tracking-wider">
                Experience
              </div>
              <div className="text-xs font-medium text-gray-500 uppercase tracking-wider">
                Last Used
              </div>
              <div className="text-xs font-medium text-gray-500 uppercase tracking-wider">
                Actions
              </div>
            </div>
          </div>
        </div>

        {/* Skills List */}
        <div className="space-y-2">
          {skills.map((skill) => (
            <div
              key={skill.id}
              className="bg-white border rounded-lg p-4 hover:shadow-md transition-shadow"
            >
              <div className="flex items-center">
                <input
                  type="checkbox"
                  checked={selectedSkills.includes(skill.id)}
                  onChange={(e) =>
                    handleSelectSkill(skill.id, e.target.checked)
                  }
                  className="h-4 w-4 text-blue-600 focus:ring-blue-500 border-gray-300 rounded"
                />
                <div className="ml-4 grid grid-cols-6 gap-4 flex-1">
                  <div>
                    <div className="font-medium text-gray-900">
                      {skill.name}
                    </div>
                    {skill.description && (
                      <div className="text-sm text-gray-500 truncate">
                        {skill.description}
                      </div>
                    )}
                  </div>
                  <div>
                    <span
                      className={`inline-flex px-2 py-1 text-xs font-medium rounded-full ${getCategoryColor(
                        skill.category
                      )}`}
                    >
                      {skill.category}
                    </span>
                  </div>
                  <div>
                    <span
                      className={`inline-flex px-2 py-1 text-xs font-medium rounded-full ${getProficiencyColor(
                        skill.proficiencyLevel
                      )}`}
                    >
                      {skill.proficiencyLevel}
                    </span>
                  </div>
                  <div className="text-sm text-gray-900">
                    {skill.yearsOfExperience} year
                    {skill.yearsOfExperience !== 1 ? "s" : ""}
                  </div>
                  <div className="text-sm text-gray-900">
                    {skill.lastUsed
                      ? new Date(skill.lastUsed).toLocaleDateString()
                      : "Never"}
                  </div>
                  <div className="flex space-x-2">
                    <button
                      onClick={() => onEdit(skill)}
                      className="text-blue-600 hover:text-blue-800"
                      title="Edit skill"
                    >
                      <PencilIcon className="w-4 h-4" />
                    </button>
                    <button
                      onClick={() => handleDeleteClick(skill)}
                      className="text-red-600 hover:text-red-800"
                      title="Delete skill"
                    >
                      <TrashIcon className="w-4 h-4" />
                    </button>
                  </div>
                </div>
              </div>
            </div>
          ))}
        </div>
      </div>
    );
  }

  // Grid View (default)
  return (
    <div className="grid grid-cols-1 md:grid-cols-2 lg:grid-cols-3 gap-6">
      {skills.map((skill) => (
        <SkillCard
          key={skill.id}
          skill={skill}
          onEdit={onEdit}
          onDelete={() => handleDeleteClick(skill)}
        />
      ))}

      {/* Delete Confirmation Dialog */}
      <ConfirmDialog
        open={deleteConfirm.isOpen}
        onClose={() => setDeleteConfirm({ isOpen: false })}
        onConfirm={handleDeleteConfirm}
        title={deleteConfirm.isBulk ? "Delete Selected Skills" : "Delete Skill"}
        description={
          deleteConfirm.isBulk
            ? `Are you sure you want to delete ${selectedSkills.length} selected skills? This action cannot be undone.`
            : `Are you sure you want to delete "${deleteConfirm.skillName}"? This action cannot be undone.`
        }
        confirmLabel="Delete"
        cancelLabel="Cancel"
        confirmVariant="danger"
      />
    </div>
  );
}
