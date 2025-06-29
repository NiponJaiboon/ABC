"use client";

import { useSkillStore } from "@/stores/skillStore";
import type { Skill } from "@/types/skills";
import {
  AcademicCapIcon,
  CalendarIcon,
  PencilIcon,
  StarIcon,
  TagIcon,
  TrashIcon,
} from "@heroicons/react/24/outline";
import { StarIcon as StarIconSolid } from "@heroicons/react/24/solid";
import { formatDistanceToNow } from "date-fns";
import React from "react";

interface SkillCardProps {
  skill: Skill;
  onEdit?: (skill: Skill) => void;
  onDelete?: (skill: Skill) => void;
  onProficiencyChange?: (
    skill: Skill,
    level: Skill["proficiencyLevel"]
  ) => void;
  showActions?: boolean;
  selectable?: boolean;
}

export function SkillCard({
  skill,
  onEdit,
  onDelete,
  onProficiencyChange,
  showActions = true,
  selectable = false,
}: Readonly<SkillCardProps>) {
  const { selectedSkills, toggleSkillSelection } = useSkillStore();

  const isSelected = selectedSkills.includes(skill.id);

  const handleToggleSelection = (e: React.ChangeEvent<HTMLInputElement>) => {
    e.stopPropagation();
    if (selectable) {
      toggleSkillSelection(skill.id);
    }
  };

  const handleEdit = (e: React.MouseEvent) => {
    e.preventDefault();
    e.stopPropagation();
    onEdit?.(skill);
  };

  const handleDelete = (e: React.MouseEvent) => {
    e.preventDefault();
    e.stopPropagation();
    onDelete?.(skill);
  };

  const renderStars = (level: number, interactive = false) => {
    return (
      <div className="flex items-center space-x-1">
        {[1, 2, 3, 4, 5].map((star) => {
          const filled = star <= level;
          return (
            <button
              key={star}
              onClick={(e) => {
                if (interactive && onProficiencyChange) {
                  e.preventDefault();
                  e.stopPropagation();
                  onProficiencyChange(skill, star as Skill["proficiencyLevel"]);
                }
              }}
              disabled={!interactive}
              className={`${
                interactive
                  ? "cursor-pointer hover:scale-110"
                  : "cursor-default"
              } transition-transform`}
              title={
                interactive
                  ? `Set proficiency to ${star}`
                  : `Proficiency level: ${star}`
              }
            >
              {filled ? (
                <StarIconSolid className="w-4 h-4 text-yellow-400" />
              ) : (
                <StarIcon className="w-4 h-4 text-gray-300" />
              )}
            </button>
          );
        })}
        <span className="ml-2 text-sm text-gray-600">({level}/5)</span>
      </div>
    );
  };

  const getCategoryColor = (category: string) => {
    const colors: Record<string, string> = {
      Frontend: "bg-blue-100 text-blue-800",
      Backend: "bg-green-100 text-green-800",
      Database: "bg-purple-100 text-purple-800",
      DevOps: "bg-orange-100 text-orange-800",
      Mobile: "bg-pink-100 text-pink-800",
      Design: "bg-indigo-100 text-indigo-800",
      Testing: "bg-red-100 text-red-800",
      Other: "bg-gray-100 text-gray-800",
    };
    return colors[category] || colors["Other"];
  };

  const getProficiencyLabel = (level: number) => {
    const labels = {
      1: "Beginner",
      2: "Basic",
      3: "Intermediate",
      4: "Advanced",
      5: "Expert",
    };
    return labels[level as keyof typeof labels] || "Unknown";
  };

  return (
    <div
      className={`
      bg-white rounded-xl shadow-sm border border-gray-200
      hover:shadow-md hover:border-gray-300
      transition-all duration-200 group p-6
      ${isSelected ? "ring-2 ring-blue-500 border-blue-500" : ""}
      ${selectable ? "cursor-pointer" : ""}
    `}
    >
      {/* Selection Checkbox */}
      {selectable && (
        <div className="absolute top-3 left-3 z-10">
          <input
            type="checkbox"
            checked={isSelected}
            onChange={handleToggleSelection}
            className="w-4 h-4 text-blue-600 border-gray-300 rounded focus:ring-blue-500"
          />
        </div>
      )}

      {/* Header */}
      <div className="flex items-start justify-between mb-4">
        <div className="flex items-center space-x-3 min-w-0 flex-1">
          <div className="flex-shrink-0">
            <AcademicCapIcon className="w-8 h-8 text-blue-500" />
          </div>
          <div className="min-w-0 flex-1">
            <h3 className="text-lg font-semibold text-gray-900 truncate group-hover:text-blue-600 transition-colors">
              {skill.name}
            </h3>
            <div className="flex items-center space-x-2 mt-1">
              <span
                className={`inline-flex items-center px-2 py-1 rounded-md text-xs font-medium ${getCategoryColor(
                  skill.category
                )}`}
              >
                <TagIcon className="w-3 h-3 mr-1" />
                {skill.category}
              </span>
            </div>
          </div>
        </div>

        {/* Actions */}
        {showActions && (
          <div className="flex items-center space-x-1 opacity-0 group-hover:opacity-100 transition-opacity">
            {onEdit && (
              <button
                onClick={handleEdit}
                className="p-2 text-gray-400 hover:text-blue-500 hover:bg-blue-50 rounded-lg transition-colors"
                title="Edit skill"
              >
                <PencilIcon className="w-4 h-4" />
              </button>
            )}
            {onDelete && (
              <button
                onClick={handleDelete}
                className="p-2 text-gray-400 hover:text-red-500 hover:bg-red-50 rounded-lg transition-colors"
                title="Delete skill"
              >
                <TrashIcon className="w-4 h-4" />
              </button>
            )}
          </div>
        )}
      </div>

      {/* Proficiency Level */}
      <div className="mb-4">
        <div className="flex items-center justify-between mb-2">
          <span className="text-sm font-medium text-gray-700">Proficiency</span>
          <span className="text-sm text-gray-600">
            {getProficiencyLabel(skill.proficiencyLevel)}
          </span>
        </div>
        {renderStars(skill.proficiencyLevel, !!onProficiencyChange)}
      </div>

      {/* Experience */}
      <div className="grid grid-cols-1 gap-3 mb-4">
        <div className="flex items-center justify-between text-sm">
          <span className="text-gray-600">Experience:</span>
          <span className="font-medium">
            {skill.experienceYears}{" "}
            {skill.experienceYears === 1 ? "year" : "years"}
          </span>
        </div>
        <div className="flex items-center justify-between text-sm">
          <span className="text-gray-600">Last used:</span>
          <span className="font-medium">
            {formatDistanceToNow(new Date(skill.lastUsed), { addSuffix: true })}
          </span>
        </div>
      </div>

      {/* Certifications */}
      {skill.certifications && skill.certifications.length > 0 && (
        <div className="mb-4">
          <span className="text-sm font-medium text-gray-700 mb-2 block">
            Certifications
          </span>
          <div className="flex flex-wrap gap-1">
            {skill.certifications.slice(0, 2).map((cert) => (
              <span
                key={cert}
                className="inline-flex items-center px-2 py-1 rounded-md text-xs bg-green-100 text-green-800"
              >
                {cert}
              </span>
            ))}
            {skill.certifications.length > 2 && (
              <span className="inline-flex items-center px-2 py-1 rounded-md text-xs bg-gray-100 text-gray-600">
                +{skill.certifications.length - 2} more
              </span>
            )}
          </div>
        </div>
      )}

      {/* Footer */}
      <div className="flex items-center justify-between pt-3 border-t border-gray-100">
        <div className="flex items-center text-xs text-gray-400">
          <CalendarIcon className="w-4 h-4 mr-1" />
          Added{" "}
          {formatDistanceToNow(new Date(skill.createdAt), { addSuffix: true })}
        </div>

        {/* Quick Proficiency Update */}
        {onProficiencyChange && skill.proficiencyLevel < 5 && (
          <button
            onClick={(e) => {
              e.preventDefault();
              e.stopPropagation();
              onProficiencyChange(
                skill,
                (skill.proficiencyLevel + 1) as Skill["proficiencyLevel"]
              );
            }}
            className="px-2 py-1 text-xs bg-blue-100 text-blue-700 rounded hover:bg-blue-200 transition-colors"
            title="Increase proficiency"
          >
            Level Up
          </button>
        )}
      </div>
    </div>
  );
}
