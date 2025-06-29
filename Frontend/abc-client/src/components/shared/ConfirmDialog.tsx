"use client";

import { Dialog, DialogPanel, DialogTitle } from "@headlessui/react";
import { ExclamationTriangleIcon } from "@heroicons/react/24/outline";
import { LoadingSpinner } from "./LoadingSpinner";

interface ConfirmDialogProps {
  open: boolean;
  onClose: () => void;
  onConfirm: () => void;
  title: string;
  description: string;
  confirmLabel?: string;
  cancelLabel?: string;
  confirmVariant?: "default" | "danger";
  isLoading?: boolean;
}

export function ConfirmDialog({
  open,
  onClose,
  onConfirm,
  title,
  description,
  confirmLabel = "Confirm",
  cancelLabel = "Cancel",
  confirmVariant = "default",
  isLoading = false,
}: Readonly<ConfirmDialogProps>) {
  const confirmButtonClasses =
    confirmVariant === "danger"
      ? "bg-red-600 hover:bg-red-700 focus:ring-red-500"
      : "bg-blue-600 hover:bg-blue-700 focus:ring-blue-500";

  return (
    <Dialog open={open} onClose={onClose} className="relative z-50">
      {/* Backdrop */}
      <div className="fixed inset-0 bg-black/25" aria-hidden="true" />

      {/* Full-screen container */}
      <div className="fixed inset-0 flex items-center justify-center p-4">
        <DialogPanel className="mx-auto max-w-md bg-white rounded-lg shadow-xl">
          <div className="p-6">
            {/* Icon */}
            <div className="flex items-center mb-4">
              <div
                className={`flex-shrink-0 w-10 h-10 rounded-full flex items-center justify-center ${
                  confirmVariant === "danger" ? "bg-red-100" : "bg-blue-100"
                }`}
              >
                <ExclamationTriangleIcon
                  className={`w-6 h-6 ${
                    confirmVariant === "danger"
                      ? "text-red-600"
                      : "text-blue-600"
                  }`}
                />
              </div>
              <div className="ml-4">
                <DialogTitle className="text-lg font-medium text-gray-900">
                  {title}
                </DialogTitle>
              </div>
            </div>

            {/* Description */}
            <div className="mb-6">
              <p className="text-gray-600">{description}</p>
            </div>

            {/* Actions */}
            <div className="flex justify-end space-x-3">
              <button
                type="button"
                onClick={onClose}
                disabled={isLoading}
                className="px-4 py-2 text-gray-700 bg-gray-100 hover:bg-gray-200 rounded-lg transition-colors disabled:opacity-50"
              >
                {cancelLabel}
              </button>
              <button
                type="button"
                onClick={onConfirm}
                disabled={isLoading}
                className={`px-4 py-2 text-white rounded-lg transition-colors disabled:opacity-50 flex items-center ${confirmButtonClasses}`}
              >
                {isLoading && <LoadingSpinner size="sm" className="mr-2" />}
                {confirmLabel}
              </button>
            </div>
          </div>
        </DialogPanel>
      </div>
    </Dialog>
  );
}
