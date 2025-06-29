"use client";

import { useProjectStore } from "@/stores/projectStore";
import type { Project } from "@/types/projects";
import {
  ArchiveBoxIcon,
  CalendarIcon,
  CheckCircleIcon,
  ClockIcon,
  DocumentTextIcon,
  ExclamationCircleIcon,
  PencilIcon,
  TagIcon,
  TrashIcon,
} from "@heroicons/react/24/outline";
import { formatDistanceToNow } from "date-fns";
import Link from "next/link";
import React from "react";

interface ProjectCardProps {
  project: Project;
  onEdit?: (project: Project) => void;
  onDelete?: (project: Project) => void;
  onStatusChange?: (project: Project, status: Project["status"]) => void;
  showActions?: boolean;
  selectable?: boolean;
}

export function ProjectCard({
  project,
  onEdit,
  onDelete,
  onStatusChange,
  showActions = true,
  selectable = false,
}: Readonly<ProjectCardProps>) {
  const { selectedProjects, toggleProjectSelection } = useProjectStore();

  const isSelected = selectedProjects.includes(project.id);

  const handleToggleSelection = (e: React.ChangeEvent<HTMLInputElement>) => {
    e.stopPropagation();
    if (selectable) {
      toggleProjectSelection(project.id);
    }
  };

  const handleEdit = (e: React.MouseEvent) => {
    e.preventDefault();
    e.stopPropagation();
    onEdit?.(project);
  };

  const handleDelete = (e: React.MouseEvent) => {
    e.preventDefault();
    e.stopPropagation();
    onDelete?.(project);
  };

  const getStatusIcon = (status: Project["status"]) => {
    switch (status) {
      case "completed":
        return <CheckCircleIcon className="w-4 h-4 text-green-500" />;
      case "in-progress":
        return <ClockIcon className="w-4 h-4 text-blue-500" />;
      case "archived":
        return <ArchiveBoxIcon className="w-4 h-4 text-gray-500" />;
      default:
        return <ExclamationCircleIcon className="w-4 h-4 text-yellow-500" />;
    }
  };

  const getStatusColor = (status: Project["status"]) => {
    switch (status) {
      case "completed":
        return "bg-green-100 text-green-800";
      case "in-progress":
        return "bg-blue-100 text-blue-800";
      case "archived":
        return "bg-gray-100 text-gray-800";
      default:
        return "bg-yellow-100 text-yellow-800";
    }
  };

  const getStatusLabel = (status: Project["status"]) => {
    switch (status) {
      case "completed":
        return "Completed";
      case "in-progress":
        return "In Progress";
      case "archived":
        return "Archived";
      default:
        return "Draft";
    }
  };

  return (
    <div
      className={`
      bg-white rounded-xl shadow-sm border border-gray-200
      hover:shadow-md hover:border-gray-300
      transition-all duration-200 group
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

      {/* Project Link */}
      <Link href={`/projects/${project.id}`} className="block p-6">
        {/* Header */}
        <div className="flex items-start justify-between mb-4">
          <div className="flex items-center space-x-3 min-w-0 flex-1">
            <div className="flex-shrink-0">
              <DocumentTextIcon className="w-8 h-8 text-blue-500" />
            </div>
            <div className="min-w-0 flex-1">
              <h3 className="text-lg font-semibold text-gray-900 truncate group-hover:text-blue-600 transition-colors">
                {project.title}
              </h3>
              <div className="flex items-center space-x-2 mt-1">
                <span
                  className={`inline-flex items-center px-2 py-1 rounded-md text-xs font-medium ${getStatusColor(
                    project.status
                  )}`}
                >
                  {getStatusIcon(project.status)}
                  <span className="ml-1">{getStatusLabel(project.status)}</span>
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
                  title="Edit project"
                >
                  <PencilIcon className="w-4 h-4" />
                </button>
              )}
              {onDelete && (
                <button
                  onClick={handleDelete}
                  className="p-2 text-gray-400 hover:text-red-500 hover:bg-red-50 rounded-lg transition-colors"
                  title="Delete project"
                >
                  <TrashIcon className="w-4 h-4" />
                </button>
              )}
            </div>
          )}
        </div>

        {/* Description */}
        <p className="text-gray-600 text-sm mb-4 line-clamp-2">
          {project.description || "No description provided"}
        </p>

        {/* Technologies */}
        {project.technologies && project.technologies.length > 0 && (
          <div className="flex flex-wrap gap-2 mb-4">
            {project.technologies.slice(0, 3).map((tech) => (
              <span
                key={tech}
                className="inline-flex items-center px-2 py-1 rounded-md text-xs font-medium bg-gray-100 text-gray-800"
              >
                <TagIcon className="w-3 h-3 mr-1" />
                {tech}
              </span>
            ))}
            {project.technologies.length > 3 && (
              <span className="inline-flex items-center px-2 py-1 rounded-md text-xs font-medium bg-gray-100 text-gray-600">
                +{project.technologies.length - 3} more
              </span>
            )}
          </div>
        )}

        {/* Timeline */}
        <div className="flex items-center justify-between text-sm text-gray-500 mb-4">
          <div className="flex items-center">
            <CalendarIcon className="w-4 h-4 mr-1" />
            Started{" "}
            {formatDistanceToNow(new Date(project.startDate), {
              addSuffix: true,
            })}
          </div>
          {project.endDate && (
            <div className="flex items-center">
              <CalendarIcon className="w-4 h-4 mr-1" />
              Ended{" "}
              {formatDistanceToNow(new Date(project.endDate), {
                addSuffix: true,
              })}
            </div>
          )}
        </div>

        {/* Footer */}
        <div className="flex items-center justify-between pt-3 border-t border-gray-100">
          <div className="text-xs text-gray-400">
            Updated{" "}
            {formatDistanceToNow(new Date(project.updatedAt), {
              addSuffix: true,
            })}
          </div>

          {/* Status Change Buttons */}
          {onStatusChange && (
            <div className="flex items-center space-x-1">
              {project.status !== "in-progress" && (
                <button
                  onClick={(e) => {
                    e.preventDefault();
                    e.stopPropagation();
                    onStatusChange(project, "in-progress");
                  }}
                  className="px-2 py-1 text-xs bg-blue-100 text-blue-700 rounded hover:bg-blue-200 transition-colors"
                  title="Mark as in progress"
                >
                  Start
                </button>
              )}
              {project.status !== "completed" && (
                <button
                  onClick={(e) => {
                    e.preventDefault();
                    e.stopPropagation();
                    onStatusChange(project, "completed");
                  }}
                  className="px-2 py-1 text-xs bg-green-100 text-green-700 rounded hover:bg-green-200 transition-colors"
                  title="Mark as completed"
                >
                  Complete
                </button>
              )}
            </div>
          )}
        </div>
      </Link>
    </div>
  );
}
