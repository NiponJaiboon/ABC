"use client";

import { PortfolioForm } from "@/components/portfolio/PortfolioForm";
import { PortfolioList } from "@/components/portfolio/PortfolioList";
import {
  useCreatePortfolio,
  useUpdatePortfolio,
} from "@/lib/hooks/usePortfolio";
import type { Portfolio } from "@/types/portfolio";
import { useState } from "react";

export default function PortfolioPage() {
  const [showCreateForm, setShowCreateForm] = useState(false);
  const [editingPortfolio, setEditingPortfolio] = useState<Portfolio | null>(
    null
  );

  const createMutation = useCreatePortfolio();
  const updateMutation = useUpdatePortfolio();

  const handleCreateNew = () => {
    setShowCreateForm(true);
  };

  const handleEdit = (portfolio: Portfolio) => {
    setEditingPortfolio(portfolio);
  };

  const handleCreateSubmit = async (data: {
    name: string;
    description?: string;
    isPublic: boolean;
  }) => {
    await createMutation.mutateAsync({
      name: data.name,
      description: data.description ?? "",
      isPublic: data.isPublic,
    });
  };

  const handleUpdateSubmit = async (data: {
    name: string;
    description?: string;
    isPublic: boolean;
  }) => {
    if (editingPortfolio) {
      await updateMutation.mutateAsync({
        id: editingPortfolio.id,
        data: {
          name: data.name,
          description: data.description,
          isPublic: data.isPublic,
        },
      });
    }
  };

  const handleCloseCreateForm = () => {
    setShowCreateForm(false);
  };

  const handleCloseEditForm = () => {
    setEditingPortfolio(null);
  };

  return (
    <div className="max-w-7xl mx-auto px-4 sm:px-6 lg:px-8 py-8">
      <PortfolioList
        onCreateNew={handleCreateNew}
        onEdit={handleEdit}
        showFilters={true}
        showBulkActions={true}
      />

      {/* Create Portfolio Form */}
      <PortfolioForm
        open={showCreateForm}
        onClose={handleCloseCreateForm}
        onSubmit={handleCreateSubmit}
        isLoading={createMutation.isPending}
      />

      {/* Edit Portfolio Form */}
      <PortfolioForm
        open={!!editingPortfolio}
        onClose={handleCloseEditForm}
        onSubmit={handleUpdateSubmit}
        portfolio={editingPortfolio ?? undefined}
        isLoading={updateMutation.isPending}
      />
    </div>
  );
}
