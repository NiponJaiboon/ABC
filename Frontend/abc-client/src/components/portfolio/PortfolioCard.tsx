"use client";

import { usePortfolioStore } from "@/stores/portfolioStore";
import type { Portfolio } from "@/types/portfolio";
import {
  DocumentTextIcon,
  EyeIcon,
  EyeSlashIcon,
  FolderIcon,
  PencilIcon,
  TrashIcon,
} from "@heroicons/react/24/outline";
import { formatDistanceToNow } from "date-fns";
import Link from "next/link";
import React from "react";

interface PortfolioCardProps {
  portfolio: Portfolio;
  onEdit?: (portfolio: Portfolio) => void;
  onDelete?: (portfolio: Portfolio) => void;
  showActions?: boolean;
  selectable?: boolean;
}

export function PortfolioCard({
  portfolio,
  onEdit,
  onDelete,
  showActions = true,
  selectable = false,
}: PortfolioCardProps) {
  const { selectedPortfolios, togglePortfolioSelection } = usePortfolioStore();

  const isSelected = selectedPortfolios.includes(portfolio.id);

  const handleToggleSelection = (e: React.ChangeEvent<HTMLInputElement>) => {
    e.stopPropagation();
    if (selectable) {
      togglePortfolioSelection(portfolio.id);
    }
  };

  const handleEdit = (e: React.MouseEvent) => {
    e.preventDefault();
    e.stopPropagation();
    onEdit?.(portfolio);
  };

  const handleDelete = (e: React.MouseEvent) => {
    e.preventDefault();
    e.stopPropagation();
    onDelete?.(portfolio);
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

      {/* Portfolio Link */}
      <Link href={`/portfolio/${portfolio.id}`} className="block p-6">
        {/* Header */}
        <div className="flex items-start justify-between mb-4">
          <div className="flex items-center space-x-3 min-w-0 flex-1">
            <div className="flex-shrink-0">
              <FolderIcon className="w-8 h-8 text-blue-500" />
            </div>
            <div className="min-w-0 flex-1">
              <h3 className="text-lg font-semibold text-gray-900 truncate group-hover:text-blue-600 transition-colors">
                {portfolio.name}
              </h3>
              <div className="flex items-center space-x-2 mt-1">
                {portfolio.isPublic ? (
                  <div className="flex items-center text-green-600 text-sm">
                    <EyeIcon className="w-4 h-4 mr-1" />
                    Public
                  </div>
                ) : (
                  <div className="flex items-center text-gray-500 text-sm">
                    <EyeSlashIcon className="w-4 h-4 mr-1" />
                    Private
                  </div>
                )}
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
                  title="Edit portfolio"
                >
                  <PencilIcon className="w-4 h-4" />
                </button>
              )}
              {onDelete && (
                <button
                  onClick={handleDelete}
                  className="p-2 text-gray-400 hover:text-red-500 hover:bg-red-50 rounded-lg transition-colors"
                  title="Delete portfolio"
                >
                  <TrashIcon className="w-4 h-4" />
                </button>
              )}
            </div>
          )}
        </div>

        {/* Description */}
        <p className="text-gray-600 text-sm mb-4 line-clamp-2">
          {portfolio.description || "No description provided"}
        </p>

        {/* Stats */}
        <div className="flex items-center justify-between">
          <div className="flex items-center space-x-4 text-sm text-gray-500">
            <div className="flex items-center">
              <DocumentTextIcon className="w-4 h-4 mr-1" />
              {portfolio.projectCount || 0} projects
            </div>
            <div className="flex items-center">
              <svg
                className="w-4 h-4 mr-1"
                fill="currentColor"
                viewBox="0 0 20 20"
              >
                <path d="M9 12l2 2 4-4m6 2a9 9 0 11-18 0 9 9 0 0118 0z" />
              </svg>
              {portfolio.skillCount || 0} skills
            </div>
          </div>

          <div className="text-xs text-gray-400">
            Updated{" "}
            {formatDistanceToNow(new Date(portfolio.updatedAt), {
              addSuffix: true,
            })}
          </div>
        </div>

        {/* Tags/Categories if available */}
        {portfolio.isPublic && (
          <div className="mt-3 pt-3 border-t border-gray-100">
            <span className="inline-flex items-center px-2 py-1 rounded-md text-xs font-medium bg-green-100 text-green-800">
              Featured
            </span>
          </div>
        )}
      </Link>
    </div>
  );
}
