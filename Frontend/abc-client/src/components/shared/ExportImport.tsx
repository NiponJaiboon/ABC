"use client";

import { FileUpload } from "@/components/shared/FileUpload";
import { LoadingSpinner } from "@/components/shared/LoadingSpinner";
import {
  ArrowDownTrayIcon,
  ArrowUpTrayIcon,
  DocumentArrowDownIcon,
  DocumentTextIcon,
  TableCellsIcon,
} from "@heroicons/react/24/outline";
import { useState } from "react";

type ExportFormat = "json" | "csv" | "pdf";
type ImportFormat = "json" | "csv";

interface ExportImportProps {
  onExport: (format: ExportFormat) => Promise<void>;
  onImport: (files: File[]) => Promise<void>;
  exportFormats?: ExportFormat[];
  importFormats?: ImportFormat[];
  disabled?: boolean;
  className?: string;
}

const FORMAT_CONFIG = {
  json: {
    label: "JSON",
    description: "Complete data with all fields",
    icon: DocumentTextIcon,
    color: "bg-blue-500",
    accept: { "application/json": [".json"] },
  },
  csv: {
    label: "CSV",
    description: "Spreadsheet format (basic fields only)",
    icon: TableCellsIcon,
    color: "bg-green-500",
    accept: { "text/csv": [".csv"] },
  },
  pdf: {
    label: "PDF",
    description: "Formatted report for viewing/printing",
    icon: DocumentArrowDownIcon,
    color: "bg-red-500",
    accept: { "application/pdf": [".pdf"] },
  },
};

export function ExportImport({
  onExport,
  onImport,
  exportFormats = ["json", "csv", "pdf"],
  importFormats = ["json", "csv"],
  disabled = false,
  className = "",
}: ExportImportProps) {
  const [isExporting, setIsExporting] = useState(false);
  const [exportingFormat, setExportingFormat] = useState<string | null>(null);
  const [showImport, setShowImport] = useState(false);

  const handleExport = async (format: ExportFormat) => {
    if (disabled || isExporting) return;

    setIsExporting(true);
    setExportingFormat(format);

    try {
      await onExport(format);
    } catch (error) {
      console.error(`Export failed:`, error);
      // You might want to show a toast notification here
    } finally {
      setIsExporting(false);
      setExportingFormat(null);
    }
  };

  const handleImport = async (files: File[]): Promise<string[]> => {
    try {
      await onImport(files);
      setShowImport(false);
      return files.map((f) => f.name); // Return filenames as URLs/identifiers
    } catch (error) {
      console.error("Import failed:", error);
      throw error; // Re-throw to let FileUpload handle the error display
    }
  };

  const getImportAcceptTypes = () => {
    return importFormats.reduce((acc, format) => {
      return { ...acc, ...FORMAT_CONFIG[format].accept };
    }, {});
  };

  return (
    <div className={`space-y-6 ${className}`}>
      {/* Export Section */}
      <div className="bg-white rounded-lg border border-gray-200 p-6">
        <div className="flex items-center space-x-2 mb-4">
          <ArrowDownTrayIcon className="w-5 h-5 text-gray-500" />
          <h3 className="text-lg font-medium text-gray-900">Export Data</h3>
        </div>

        <p className="text-sm text-gray-600 mb-6">
          Export your data in different formats for backup, sharing, or
          analysis.
        </p>

        <div className="grid grid-cols-1 md:grid-cols-3 gap-4">
          {exportFormats.map((format) => {
            const config = FORMAT_CONFIG[format];
            const IconComponent = config.icon;
            const isCurrentlyExporting = exportingFormat === format;

            return (
              <button
                key={format}
                onClick={() => handleExport(format)}
                disabled={disabled || isExporting}
                className={`
                  relative p-4 rounded-lg border-2 border-gray-200 hover:border-gray-300
                  transition-colors text-left group
                  ${
                    disabled || isExporting
                      ? "opacity-50 cursor-not-allowed"
                      : "hover:shadow-sm"
                  }
                `}
              >
                <div className="flex items-start space-x-3">
                  <div className={`p-2 rounded-lg ${config.color}`}>
                    <IconComponent className="w-5 h-5 text-white" />
                  </div>
                  <div className="flex-1 min-w-0">
                    <h4 className="text-sm font-medium text-gray-900 group-hover:text-blue-600">
                      Export as {config.label}
                    </h4>
                    <p className="text-xs text-gray-500 mt-1">
                      {config.description}
                    </p>
                  </div>
                </div>

                {isCurrentlyExporting && (
                  <div className="absolute inset-0 bg-white bg-opacity-75 rounded-lg flex items-center justify-center">
                    <div className="flex items-center space-x-2">
                      <LoadingSpinner size="sm" />
                      <span className="text-sm text-gray-600">
                        Exporting...
                      </span>
                    </div>
                  </div>
                )}
              </button>
            );
          })}
        </div>
      </div>

      {/* Import Section */}
      <div className="bg-white rounded-lg border border-gray-200 p-6">
        <div className="flex items-center justify-between mb-4">
          <div className="flex items-center space-x-2">
            <ArrowUpTrayIcon className="w-5 h-5 text-gray-500" />
            <h3 className="text-lg font-medium text-gray-900">Import Data</h3>
          </div>

          {!showImport && (
            <button
              onClick={() => setShowImport(true)}
              disabled={disabled}
              className="px-4 py-2 bg-blue-600 text-white rounded-lg hover:bg-blue-700 transition-colors disabled:opacity-50"
            >
              Start Import
            </button>
          )}
        </div>

        <p className="text-sm text-gray-600 mb-4">
          Import data from previously exported files or external sources.
        </p>

        {showImport ? (
          <div className="space-y-4">
            <div className="bg-yellow-50 border border-yellow-200 rounded-lg p-4">
              <div className="flex items-start space-x-2">
                <div className="w-5 h-5 text-yellow-600 mt-0.5">⚠️</div>
                <div>
                  <h4 className="text-sm font-medium text-yellow-800">
                    Important Notes
                  </h4>
                  <ul className="text-sm text-yellow-700 mt-1 space-y-1">
                    <li>
                      • Importing will add new data to your existing records
                    </li>
                    <li>• Duplicate entries may be created if IDs conflict</li>
                    <li>• Consider backing up your data before importing</li>
                    <li>
                      • Supported formats:{" "}
                      {importFormats.join(", ").toUpperCase()}
                    </li>
                  </ul>
                </div>
              </div>
            </div>

            <FileUpload
              onUpload={handleImport}
              accept={getImportAcceptTypes()}
              maxFiles={1}
              multiple={false}
              disabled={disabled}
            />

            <div className="flex justify-end space-x-3">
              <button
                onClick={() => setShowImport(false)}
                className="px-4 py-2 border border-gray-300 rounded-lg text-gray-700 hover:bg-gray-50 transition-colors"
              >
                Cancel
              </button>
            </div>
          </div>
        ) : (
          <div className="grid grid-cols-1 md:grid-cols-2 gap-4">
            {importFormats.map((format) => {
              const config = FORMAT_CONFIG[format];
              const IconComponent = config.icon;

              return (
                <div
                  key={format}
                  className="p-4 rounded-lg border border-gray-200 bg-gray-50"
                >
                  <div className="flex items-start space-x-3">
                    <div className={`p-2 rounded-lg ${config.color}`}>
                      <IconComponent className="w-4 h-4 text-white" />
                    </div>
                    <div className="flex-1 min-w-0">
                      <h4 className="text-sm font-medium text-gray-900">
                        {config.label} Format
                      </h4>
                      <p className="text-xs text-gray-500 mt-1">
                        {config.description}
                      </p>
                    </div>
                  </div>
                </div>
              );
            })}
          </div>
        )}
      </div>

      {/* Usage Tips */}
      <div className="bg-blue-50 border border-blue-200 rounded-lg p-4">
        <h4 className="text-sm font-medium text-blue-900 mb-2">Tips</h4>
        <ul className="text-sm text-blue-800 space-y-1">
          <li>• Export your data regularly to create backups</li>
          <li>• Use JSON format for complete data preservation</li>
          <li>• CSV format is ideal for spreadsheet analysis</li>
          <li>• PDF format creates printable reports</li>
          <li>• Always validate imported data before proceeding</li>
        </ul>
      </div>
    </div>
  );
}
