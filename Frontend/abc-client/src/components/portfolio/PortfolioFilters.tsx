"use client";

import {
  usePortfolioFilters,
  usePortfolioStore,
} from "@/stores/portfolioStore";
import {
  AdjustmentsHorizontalIcon,
  MagnifyingGlassIcon,
} from "@heroicons/react/24/outline";

export function PortfolioFilters() {
  const filters = usePortfolioFilters();
  const {
    setSearch,
    setSortBy,
    setSortOrder,
    setIsPublicFilter,
    resetFilters,
  } = usePortfolioStore();

  const hasActiveFilters = filters.search || filters.isPublic !== undefined;

  // Helper function for visibility filter value
  const getVisibilityValue = () => {
    if (filters.isPublic === undefined) return "all";
    return filters.isPublic ? "public" : "private";
  };

  return (
    <div className="bg-white border border-gray-200 rounded-lg p-4">
      <div className="flex flex-col sm:flex-row sm:items-center sm:justify-between space-y-4 sm:space-y-0 sm:space-x-4">
        {/* Search */}
        <div className="relative flex-1 max-w-md">
          <MagnifyingGlassIcon className="absolute left-3 top-1/2 transform -translate-y-1/2 w-5 h-5 text-gray-400" />
          <input
            type="text"
            placeholder="Search portfolios..."
            value={filters.search}
            onChange={(e) => setSearch(e.target.value)}
            className="w-full pl-10 pr-4 py-2 border border-gray-300 rounded-lg focus:ring-2 focus:ring-blue-500 focus:border-blue-500"
          />
        </div>

        {/* Filters */}
        <div className="flex items-center space-x-3">
          {/* Sort By */}
          <select
            value={filters.sortBy}
            onChange={(e) => setSortBy(e.target.value as typeof filters.sortBy)}
            className="px-3 py-2 border border-gray-300 rounded-lg focus:ring-2 focus:ring-blue-500 focus:border-blue-500"
          >
            <option value="updatedAt">Recently Updated</option>
            <option value="createdAt">Recently Created</option>
            <option value="name">Name</option>
          </select>

          {/* Sort Order */}
          <select
            value={filters.sortOrder}
            onChange={(e) =>
              setSortOrder(e.target.value as typeof filters.sortOrder)
            }
            className="px-3 py-2 border border-gray-300 rounded-lg focus:ring-2 focus:ring-blue-500 focus:border-blue-500"
          >
            <option value="desc">Descending</option>
            <option value="asc">Ascending</option>
          </select>

          {/* Visibility Filter */}
          <select
            value={getVisibilityValue()}
            onChange={(e) => {
              const value = e.target.value;
              setIsPublicFilter(
                value === "all" ? undefined : value === "public"
              );
            }}
            className="px-3 py-2 border border-gray-300 rounded-lg focus:ring-2 focus:ring-blue-500 focus:border-blue-500"
          >
            <option value="all">All Portfolios</option>
            <option value="public">Public Only</option>
            <option value="private">Private Only</option>
          </select>

          {/* Reset Filters */}
          {hasActiveFilters && (
            <button
              onClick={resetFilters}
              className="flex items-center px-3 py-2 text-gray-600 hover:text-gray-800 transition-colors"
              title="Reset filters"
            >
              <AdjustmentsHorizontalIcon className="w-5 h-5 mr-1" />
              Reset
            </button>
          )}
        </div>
      </div>

      {/* Active Filter Indicators */}
      {hasActiveFilters && (
        <div className="mt-3 pt-3 border-t border-gray-100">
          <div className="flex flex-wrap items-center gap-2">
            <span className="text-sm text-gray-600">Active filters:</span>

            {filters.search && (
              <span className="inline-flex items-center px-2 py-1 bg-blue-100 text-blue-800 rounded-md text-sm">
                Search: {filters.search}
                <button
                  onClick={() => setSearch("")}
                  className="ml-1 text-blue-600 hover:text-blue-800"
                >
                  ×
                </button>
              </span>
            )}

            {filters.isPublic !== undefined && (
              <span className="inline-flex items-center px-2 py-1 bg-green-100 text-green-800 rounded-md text-sm">
                {filters.isPublic ? "Public" : "Private"}
                <button
                  onClick={() => setIsPublicFilter(undefined)}
                  className="ml-1 text-green-600 hover:text-green-800"
                >
                  ×
                </button>
              </span>
            )}
          </div>
        </div>
      )}
    </div>
  );
}
