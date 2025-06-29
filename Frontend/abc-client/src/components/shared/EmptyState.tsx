"use client";

import React from "react";

interface EmptyStateProps {
  title: string;
  description?: string;
  action?: {
    label: string;
    onClick: () => void;
  };
  icon?: React.ReactNode;
  className?: string;
}

export function EmptyState({
  title,
  description,
  action,
  icon,
  className = "",
}: Readonly<EmptyStateProps>) {
  return (
    <div className={`text-center py-12 ${className}`}>
      {icon && <div className="flex justify-center mb-6">{icon}</div>}

      <h3 className="text-lg font-medium text-gray-900 mb-2">{title}</h3>

      {description && (
        <p className="text-gray-600 mb-6 max-w-md mx-auto">{description}</p>
      )}

      {action && (
        <button
          onClick={action.onClick}
          className="inline-flex items-center px-4 py-2 bg-blue-600 text-white rounded-lg hover:bg-blue-700 transition-colors"
        >
          {action.label}
        </button>
      )}
    </div>
  );
}
