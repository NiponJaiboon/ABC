"use client";

import { ConfirmDialog } from "@/components/shared/ConfirmDialog";
import { EmptyState } from "@/components/shared/EmptyState";
import { LoadingSpinner } from "@/components/shared/LoadingSpinner";
import {
  useBulkDeletePortfolios,
  useDeletePortfolio,
  usePortfolios,
} from "@/lib/hooks/usePortfolio";
import {
  usePortfolioFilters,
  usePortfolioStore,
  usePortfolioViewMode,
  useSelectedPortfolios,
} from "@/stores/portfolioStore";
import type { Portfolio } from "@/types/portfolio";
import {
  ListBulletIcon,
  PlusIcon,
  Squares2X2Icon,
} from "@heroicons/react/24/outline";
import { useState } from "react";
import { PortfolioCard } from "./PortfolioCard";
import { PortfolioFilters } from "./PortfolioFilters";

interface PortfolioListProps {
  onCreateNew?: () => void;
  onEdit?: (portfolio: Portfolio) => void;
  showFilters?: boolean;
  showBulkActions?: boolean;
}

export function PortfolioList({
  onCreateNew,
  onEdit,
  showFilters = true,
  showBulkActions = true,
}: Readonly<PortfolioListProps>) {
  const filters = usePortfolioFilters();
  const selectedPortfolios = useSelectedPortfolios();
  const viewMode = usePortfolioViewMode();
  const { clearSelection, setViewMode } = usePortfolioStore();

  const [deleteConfirm, setDeleteConfirm] = useState<Portfolio | null>(null);
  const [bulkDeleteConfirm, setBulkDeleteConfirm] = useState(false);

  // Fetch portfolios with current filters
  const {
    data: portfoliosResponse,
    isLoading,
    isError,
    error,
    refetch,
  } = usePortfolios(filters);

  const deleteMutation = useDeletePortfolio();
  const bulkDeleteMutation = useBulkDeletePortfolios();

  const portfolios = portfoliosResponse?.data ?? [];
  const totalCount = portfoliosResponse?.total ?? 0;

  const handleDelete = async (portfolio: Portfolio) => {
    try {
      await deleteMutation.mutateAsync(portfolio.id);
      setDeleteConfirm(null);
    } catch (error) {
      console.error("Failed to delete portfolio:", error);
    }
  };

  const handleBulkDelete = async () => {
    try {
      await bulkDeleteMutation.mutateAsync(selectedPortfolios);
      setBulkDeleteConfirm(false);
      clearSelection();
    } catch (error) {
      console.error("Failed to delete portfolios:", error);
    }
  };

  const handleEdit = (portfolio: Portfolio) => {
    onEdit?.(portfolio);
  };

  const isDeleting = deleteMutation.isPending || bulkDeleteMutation.isPending;

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
          Failed to load portfolios: {error?.message}
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
    <div className="space-y-6">
      {/* Header */}
      <div className="flex items-center justify-between">
        <div>
          <h1 className="text-2xl font-bold text-gray-900">Portfolios</h1>
          <p className="text-gray-600 mt-1">
            {totalCount} {totalCount === 1 ? "portfolio" : "portfolios"} total
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
          </div>

          {/* Create Button */}
          {onCreateNew && (
            <button
              onClick={onCreateNew}
              className="flex items-center px-4 py-2 bg-blue-600 text-white rounded-lg hover:bg-blue-700 transition-colors"
            >
              <PlusIcon className="w-5 h-5 mr-2" />
              New Portfolio
            </button>
          )}
        </div>
      </div>

      {/* Filters */}
      {showFilters && <PortfolioFilters />}

      {/* Bulk Actions */}
      {showBulkActions && selectedPortfolios.length > 0 && (
        <div className="flex items-center justify-between p-4 bg-blue-50 border border-blue-200 rounded-lg">
          <div className="text-blue-800">
            {selectedPortfolios.length}{" "}
            {selectedPortfolios.length === 1 ? "portfolio" : "portfolios"}{" "}
            selected
          </div>
          <div className="flex items-center space-x-3">
            <button
              onClick={() => clearSelection()}
              className="text-blue-600 hover:text-blue-800 transition-colors"
            >
              Clear selection
            </button>
            <button
              onClick={() => setBulkDeleteConfirm(true)}
              disabled={isDeleting}
              className="px-3 py-1.5 bg-red-600 text-white rounded-md hover:bg-red-700 disabled:opacity-50 transition-colors"
            >
              Delete Selected
            </button>
          </div>
        </div>
      )}

      {/* Portfolio Grid/List */}
      {portfolios.length === 0 ? (
        <EmptyState
          title="No portfolios found"
          description={
            filters.search
              ? "No portfolios match your search criteria"
              : "Create your first portfolio to get started"
          }
          action={
            onCreateNew && !filters.search
              ? {
                  label: "Create Portfolio",
                  onClick: onCreateNew,
                }
              : undefined
          }
        />
      ) : (
        <div
          className={
            viewMode === "grid"
              ? "grid grid-cols-1 md:grid-cols-2 lg:grid-cols-3 gap-6"
              : "space-y-4"
          }
        >
          {portfolios.map((portfolio) => (
            <PortfolioCard
              key={portfolio.id}
              portfolio={portfolio}
              onEdit={handleEdit}
              onDelete={(p) => setDeleteConfirm(p)}
              selectable={showBulkActions}
              showActions={true}
            />
          ))}
        </div>
      )}

      {/* Delete Confirmation Dialog */}
      <ConfirmDialog
        open={!!deleteConfirm}
        onClose={() => setDeleteConfirm(null)}
        onConfirm={() => deleteConfirm && handleDelete(deleteConfirm)}
        title="Delete Portfolio"
        description={`Are you sure you want to delete "${deleteConfirm?.name}"? This action cannot be undone.`}
        confirmLabel="Delete"
        confirmVariant="danger"
        isLoading={deleteMutation.isPending}
      />

      {/* Bulk Delete Confirmation Dialog */}
      <ConfirmDialog
        open={bulkDeleteConfirm}
        onClose={() => setBulkDeleteConfirm(false)}
        onConfirm={handleBulkDelete}
        title="Delete Portfolios"
        description={`Are you sure you want to delete ${
          selectedPortfolios.length
        } ${
          selectedPortfolios.length === 1 ? "portfolio" : "portfolios"
        }? This action cannot be undone.`}
        confirmLabel="Delete All"
        confirmVariant="danger"
        isLoading={bulkDeleteMutation.isPending}
      />
    </div>
  );
}
