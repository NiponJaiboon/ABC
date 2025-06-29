"use client";

import { ProficiencyLevel, SkillCategory } from "@/types/skills";
import {
  FunnelIcon,
  MagnifyingGlassIcon,
  XMarkIcon,
} from "@heroicons/react/24/outline";
import { useEffect, useState } from "react";

interface SkillFiltersProps {
  onFiltersChange: (filters: SkillFilters) => void;
  initialFilters?: SkillFilters;
}

export interface SkillFilters {
  search: string;
  category: SkillCategory | "";
  proficiencyLevel: ProficiencyLevel | "";
  isActive?: boolean;
  hasLastUsed?: boolean;
  sortBy:
    | "name"
    | "category"
    | "proficiencyLevel"
    | "lastUsed"
    | "yearsOfExperience";
  sortOrder: "asc" | "desc";
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

const PROFICIENCY_LEVELS: { value: ProficiencyLevel; label: string }[] = [
  { value: "Beginner", label: "Beginner" },
  { value: "Intermediate", label: "Intermediate" },
  { value: "Advanced", label: "Advanced" },
  { value: "Expert", label: "Expert" },
];

const SORT_OPTIONS = [
  { value: "name", label: "Name" },
  { value: "category", label: "Category" },
  { value: "proficiencyLevel", label: "Proficiency" },
  { value: "yearsOfExperience", label: "Experience" },
  { value: "lastUsed", label: "Last Used" },
];

export function SkillFilters({
  onFiltersChange,
  initialFilters,
}: SkillFiltersProps) {
  const [filters, setFilters] = useState<SkillFilters>({
    search: "",
    category: "",
    proficiencyLevel: "",
    isActive: undefined,
    hasLastUsed: undefined,
    sortBy: "name",
    sortOrder: "asc",
    ...initialFilters,
  });

  const [showAdvanced, setShowAdvanced] = useState(false);

  useEffect(() => {
    onFiltersChange(filters);
  }, [filters, onFiltersChange]);

  const handleFilterChange = (key: keyof SkillFilters, value: any) => {
    setFilters((prev) => ({
      ...prev,
      [key]: value,
    }));
  };

  const clearFilters = () => {
    const clearedFilters: SkillFilters = {
      search: "",
      category: "",
      proficiencyLevel: "",
      isActive: undefined,
      hasLastUsed: undefined,
      sortBy: "name",
      sortOrder: "asc",
    };
    setFilters(clearedFilters);
  };

  const hasActiveFilters =
    filters.search ||
    filters.category ||
    filters.proficiencyLevel ||
    filters.isActive !== undefined ||
    filters.hasLastUsed !== undefined;

  return (
    <div className="bg-white border border-gray-200 rounded-lg p-4 space-y-4">
      {/* Search and Main Controls */}
      <div className="flex flex-col sm:flex-row gap-4">
        {/* Search */}
        <div className="flex-1">
          <div className="relative">
            <MagnifyingGlassIcon className="absolute left-3 top-1/2 transform -translate-y-1/2 text-gray-400 w-5 h-5" />
            <input
              type="text"
              placeholder="Search skills..."
              value={filters.search}
              onChange={(e) => handleFilterChange("search", e.target.value)}
              className="w-full pl-10 pr-3 py-2 border border-gray-300 rounded-md focus:outline-none focus:ring-2 focus:ring-blue-500"
            />
          </div>
        </div>

        {/* Category Filter */}
        <div className="sm:w-48">
          <select
            value={filters.category}
            onChange={(e) => handleFilterChange("category", e.target.value)}
            className="w-full px-3 py-2 border border-gray-300 rounded-md focus:outline-none focus:ring-2 focus:ring-blue-500"
          >
            <option value="">All Categories</option>
            {SKILL_CATEGORIES.map((category) => (
              <option key={category.value} value={category.value}>
                {category.label}
              </option>
            ))}
          </select>
        </div>

        {/* Proficiency Filter */}
        <div className="sm:w-48">
          <select
            value={filters.proficiencyLevel}
            onChange={(e) =>
              handleFilterChange("proficiencyLevel", e.target.value)
            }
            className="w-full px-3 py-2 border border-gray-300 rounded-md focus:outline-none focus:ring-2 focus:ring-blue-500"
          >
            <option value="">All Levels</option>
            {PROFICIENCY_LEVELS.map((level) => (
              <option key={level.value} value={level.value}>
                {level.label}
              </option>
            ))}
          </select>
        </div>

        {/* Advanced Filters Toggle */}
        <button
          onClick={() => setShowAdvanced(!showAdvanced)}
          className="flex items-center px-3 py-2 text-gray-600 hover:text-gray-800 border border-gray-300 rounded-md hover:bg-gray-50 transition-colors"
        >
          <FunnelIcon className="w-5 h-5 mr-1" />
          Advanced
        </button>

        {/* Clear Filters */}
        {hasActiveFilters && (
          <button
            onClick={clearFilters}
            className="flex items-center px-3 py-2 text-red-600 hover:text-red-800 border border-red-300 rounded-md hover:bg-red-50 transition-colors"
          >
            <XMarkIcon className="w-5 h-5 mr-1" />
            Clear
          </button>
        )}
      </div>

      {/* Advanced Filters */}
      {showAdvanced && (
        <div className="border-t pt-4 space-y-4">
          <div className="grid grid-cols-1 sm:grid-cols-2 lg:grid-cols-4 gap-4">
            {/* Sort By */}
            <div>
              <label
                htmlFor="sortBy"
                className="block text-sm font-medium text-gray-700 mb-1"
              >
                Sort By
              </label>
              <select
                id="sortBy"
                value={filters.sortBy}
                onChange={(e) => handleFilterChange("sortBy", e.target.value)}
                className="w-full px-3 py-2 border border-gray-300 rounded-md focus:outline-none focus:ring-2 focus:ring-blue-500"
              >
                {SORT_OPTIONS.map((option) => (
                  <option key={option.value} value={option.value}>
                    {option.label}
                  </option>
                ))}
              </select>
            </div>

            {/* Sort Order */}
            <div>
              <label
                htmlFor="sortOrder"
                className="block text-sm font-medium text-gray-700 mb-1"
              >
                Sort Order
              </label>
              <select
                id="sortOrder"
                value={filters.sortOrder}
                onChange={(e) =>
                  handleFilterChange("sortOrder", e.target.value)
                }
                className="w-full px-3 py-2 border border-gray-300 rounded-md focus:outline-none focus:ring-2 focus:ring-blue-500"
              >
                <option value="asc">Ascending</option>
                <option value="desc">Descending</option>
              </select>
            </div>

            {/* Status Filter */}
            <div>
              <label
                htmlFor="statusFilter"
                className="block text-sm font-medium text-gray-700 mb-1"
              >
                Status
              </label>
              <select
                id="statusFilter"
                value={
                  filters.isActive === undefined
                    ? ""
                    : filters.isActive.toString()
                }
                onChange={(e) => {
                  const value = e.target.value;
                  handleFilterChange(
                    "isActive",
                    value === "" ? undefined : value === "true"
                  );
                }}
                className="w-full px-3 py-2 border border-gray-300 rounded-md focus:outline-none focus:ring-2 focus:ring-blue-500"
              >
                <option value="">All Skills</option>
                <option value="true">Active Only</option>
                <option value="false">Inactive Only</option>
              </select>
            </div>

            {/* Usage Filter */}
            <div>
              <label
                htmlFor="usageFilter"
                className="block text-sm font-medium text-gray-700 mb-1"
              >
                Usage
              </label>
              <select
                id="usageFilter"
                value={
                  filters.hasLastUsed === undefined
                    ? ""
                    : filters.hasLastUsed.toString()
                }
                onChange={(e) => {
                  const value = e.target.value;
                  handleFilterChange(
                    "hasLastUsed",
                    value === "" ? undefined : value === "true"
                  );
                }}
                className="w-full px-3 py-2 border border-gray-300 rounded-md focus:outline-none focus:ring-2 focus:ring-blue-500"
              >
                <option value="">All Skills</option>
                <option value="true">Recently Used</option>
                <option value="false">Never Used</option>
              </select>
            </div>
          </div>
        </div>
      )}

      {/* Active Filters Summary */}
      {hasActiveFilters && (
        <div className="border-t pt-3">
          <div className="flex flex-wrap gap-2">
            <span className="text-sm text-gray-600">Active filters:</span>
            {filters.search && (
              <span className="inline-flex items-center px-2 py-1 rounded-full text-xs font-medium bg-blue-100 text-blue-800">
                Search: {filters.search}
                <button
                  onClick={() => handleFilterChange("search", "")}
                  className="ml-1 text-blue-600 hover:text-blue-800"
                >
                  <XMarkIcon className="w-3 h-3" />
                </button>
              </span>
            )}
            {filters.category && (
              <span className="inline-flex items-center px-2 py-1 rounded-full text-xs font-medium bg-green-100 text-green-800">
                Category: {filters.category}
                <button
                  onClick={() => handleFilterChange("category", "")}
                  className="ml-1 text-green-600 hover:text-green-800"
                >
                  <XMarkIcon className="w-3 h-3" />
                </button>
              </span>
            )}
            {filters.proficiencyLevel && (
              <span className="inline-flex items-center px-2 py-1 rounded-full text-xs font-medium bg-purple-100 text-purple-800">
                Level: {filters.proficiencyLevel}
                <button
                  onClick={() => handleFilterChange("proficiencyLevel", "")}
                  className="ml-1 text-purple-600 hover:text-purple-800"
                >
                  <XMarkIcon className="w-3 h-3" />
                </button>
              </span>
            )}
            {filters.isActive !== undefined && (
              <span className="inline-flex items-center px-2 py-1 rounded-full text-xs font-medium bg-yellow-100 text-yellow-800">
                Status: {filters.isActive ? "Active" : "Inactive"}
                <button
                  onClick={() => handleFilterChange("isActive", undefined)}
                  className="ml-1 text-yellow-600 hover:text-yellow-800"
                >
                  <XMarkIcon className="w-3 h-3" />
                </button>
              </span>
            )}
            {filters.hasLastUsed !== undefined && (
              <span className="inline-flex items-center px-2 py-1 rounded-full text-xs font-medium bg-orange-100 text-orange-800">
                Usage: {filters.hasLastUsed ? "Recently Used" : "Never Used"}
                <button
                  onClick={() => handleFilterChange("hasLastUsed", undefined)}
                  className="ml-1 text-orange-600 hover:text-orange-800"
                >
                  <XMarkIcon className="w-3 h-3" />
                </button>
              </span>
            )}
          </div>
        </div>
      )}
    </div>
  );
}
