"use client";

import { LoadingSpinner } from "@/components/shared/LoadingSpinner";
import type { Portfolio } from "@/types/portfolio";
import { Dialog, DialogPanel, DialogTitle } from "@headlessui/react";
import { XMarkIcon } from "@heroicons/react/24/outline";
import { zodResolver } from "@hookform/resolvers/zod";
import { useEffect } from "react";
import { useForm } from "react-hook-form";
import { z } from "zod";

const portfolioSchema = z.object({
  name: z
    .string()
    .min(1, "Portfolio name is required")
    .max(100, "Name must be less than 100 characters"),
  description: z
    .string()
    .max(500, "Description must be less than 500 characters")
    .optional(),
  isPublic: z.boolean(),
});

type PortfolioFormData = z.infer<typeof portfolioSchema>;

interface PortfolioFormProps {
  open: boolean;
  onClose: () => void;
  onSubmit: (data: PortfolioFormData) => Promise<void>;
  portfolio?: Portfolio;
  isLoading?: boolean;
}

export function PortfolioForm({
  open,
  onClose,
  onSubmit,
  portfolio,
  isLoading = false,
}: Readonly<PortfolioFormProps>) {
  const isEditing = !!portfolio;

  const {
    register,
    handleSubmit,
    formState: { errors, isSubmitting },
    reset,
    setValue,
  } = useForm<PortfolioFormData>({
    resolver: zodResolver(portfolioSchema),
    defaultValues: {
      name: "",
      description: "",
      isPublic: false,
    },
  });

  // Reset form when portfolio changes or dialog opens/closes
  useEffect(() => {
    if (portfolio) {
      setValue("name", portfolio.name);
      setValue("description", portfolio.description ?? "");
      setValue("isPublic", portfolio.isPublic);
    } else {
      reset({
        name: "",
        description: "",
        isPublic: false,
      });
    }
  }, [portfolio, setValue, reset]);

  const onFormSubmit = async (data: PortfolioFormData) => {
    try {
      await onSubmit(data);
      reset();
      onClose();
    } catch (error) {
      console.error("Failed to submit portfolio:", error);
    }
  };

  const handleClose = () => {
    if (!isSubmitting && !isLoading) {
      reset();
      onClose();
    }
  };

  return (
    <Dialog open={open} onClose={handleClose} className="relative z-50">
      {/* Backdrop */}
      <div className="fixed inset-0 bg-black/25" aria-hidden="true" />

      {/* Full-screen container */}
      <div className="fixed inset-0 flex items-center justify-center p-4">
        <DialogPanel className="mx-auto max-w-md w-full bg-white rounded-lg shadow-xl">
          {/* Header */}
          <div className="flex items-center justify-between p-6 border-b border-gray-200">
            <DialogTitle className="text-lg font-semibold text-gray-900">
              {isEditing ? "Edit Portfolio" : "Create New Portfolio"}
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
          <form onSubmit={handleSubmit(onFormSubmit)} className="p-6 space-y-4">
            {/* Name */}
            <div>
              <label
                htmlFor="name"
                className="block text-sm font-medium text-gray-700 mb-1"
              >
                Portfolio Name *
              </label>
              <input
                {...register("name")}
                type="text"
                id="name"
                placeholder="Enter portfolio name"
                className="w-full px-3 py-2 border border-gray-300 rounded-lg focus:ring-2 focus:ring-blue-500 focus:border-blue-500"
                disabled={isSubmitting || isLoading}
              />
              {errors.name && (
                <p className="mt-1 text-sm text-red-600">
                  {errors.name.message}
                </p>
              )}
            </div>

            {/* Description */}
            <div>
              <label
                htmlFor="description"
                className="block text-sm font-medium text-gray-700 mb-1"
              >
                Description
              </label>
              <textarea
                {...register("description")}
                id="description"
                rows={4}
                placeholder="Describe your portfolio..."
                className="w-full px-3 py-2 border border-gray-300 rounded-lg focus:ring-2 focus:ring-blue-500 focus:border-blue-500 resize-none"
                disabled={isSubmitting || isLoading}
              />
              {errors.description && (
                <p className="mt-1 text-sm text-red-600">
                  {errors.description.message}
                </p>
              )}
            </div>

            {/* Visibility */}
            <div>
              <label className="flex items-center">
                <input
                  {...register("isPublic")}
                  type="checkbox"
                  className="w-4 h-4 text-blue-600 border-gray-300 rounded focus:ring-blue-500"
                  disabled={isSubmitting || isLoading}
                />
                <span className="ml-2 text-sm text-gray-700">
                  Make this portfolio public
                </span>
              </label>
              <p className="mt-1 text-xs text-gray-500">
                Public portfolios can be viewed by anyone
              </p>
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
                {isEditing ? "Update Portfolio" : "Create Portfolio"}
              </button>
            </div>
          </form>
        </DialogPanel>
      </div>
    </Dialog>
  );
}
