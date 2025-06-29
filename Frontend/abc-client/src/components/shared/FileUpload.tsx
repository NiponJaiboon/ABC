"use client";

import { LoadingSpinner } from "@/components/shared/LoadingSpinner";
import {
  CheckCircleIcon,
  CloudArrowUpIcon,
  DocumentIcon,
  PhotoIcon,
  XMarkIcon,
} from "@heroicons/react/24/outline";
import { useCallback, useState } from "react";
import { useDropzone } from "react-dropzone";

interface FileUploadProps {
  onUpload: (files: File[]) => Promise<string[]>;
  accept?: Record<string, string[]>;
  maxFiles?: number;
  maxSize?: number;
  multiple?: boolean;
  disabled?: boolean;
  className?: string;
}

interface UploadedFile {
  file: File;
  url?: string;
  error?: string;
  uploading: boolean;
  uploaded: boolean;
}

export function FileUpload({
  onUpload,
  accept = {
    "image/*": [".png", ".jpg", ".jpeg", ".gif", ".webp"],
    "application/pdf": [".pdf"],
    "text/*": [".txt", ".md"],
    "application/vnd.openxmlformats-officedocument.wordprocessingml.document": [
      ".docx",
    ],
    "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet": [
      ".xlsx",
    ],
  },
  maxFiles = 5,
  maxSize = 10 * 1024 * 1024, // 10MB
  multiple = true,
  disabled = false,
  className = "",
}: FileUploadProps) {
  const [uploadedFiles, setUploadedFiles] = useState<UploadedFile[]>([]);
  const [isUploading, setIsUploading] = useState(false);

  const onDrop = useCallback(
    async (acceptedFiles: File[]) => {
      if (disabled) return;

      // Add files to state with uploading status
      const newFiles: UploadedFile[] = acceptedFiles.map((file) => ({
        file,
        uploading: true,
        uploaded: false,
      }));

      setUploadedFiles((prev) => [...prev, ...newFiles]);
      setIsUploading(true);

      try {
        const urls = await onUpload(acceptedFiles);

        // Update files with success status and URLs
        setUploadedFiles((prev) =>
          prev.map((uploadedFile, index) => {
            const fileIndex = prev.length - acceptedFiles.length + index;
            if (fileIndex >= prev.length - acceptedFiles.length) {
              return {
                ...uploadedFile,
                uploading: false,
                uploaded: true,
                url: urls[index - (prev.length - acceptedFiles.length)],
              };
            }
            return uploadedFile;
          })
        );
      } catch (error) {
        // Update files with error status
        setUploadedFiles((prev) =>
          prev.map((uploadedFile, index) => {
            if (index >= prev.length - acceptedFiles.length) {
              return {
                ...uploadedFile,
                uploading: false,
                uploaded: false,
                error: error instanceof Error ? error.message : "Upload failed",
              };
            }
            return uploadedFile;
          })
        );
      } finally {
        setIsUploading(false);
      }
    },
    [onUpload, disabled]
  );

  const { getRootProps, getInputProps, isDragActive, fileRejections } =
    useDropzone({
      onDrop,
      accept,
      maxFiles: multiple ? maxFiles : 1,
      maxSize,
      multiple,
      disabled: disabled || isUploading,
    });

  const removeFile = (index: number) => {
    setUploadedFiles((prev) => prev.filter((_, i) => i !== index));
  };

  const retryUpload = async (index: number) => {
    const fileToRetry = uploadedFiles[index];
    if (!fileToRetry || fileToRetry.uploading) return;

    setUploadedFiles((prev) =>
      prev.map((file, i) =>
        i === index
          ? { ...file, uploading: true, uploaded: false, error: undefined }
          : file
      )
    );

    try {
      const urls = await onUpload([fileToRetry.file]);
      setUploadedFiles((prev) =>
        prev.map((file, i) =>
          i === index
            ? { ...file, uploading: false, uploaded: true, url: urls[0] }
            : file
        )
      );
    } catch (error) {
      setUploadedFiles((prev) =>
        prev.map((file, i) =>
          i === index
            ? {
                ...file,
                uploading: false,
                uploaded: false,
                error: error instanceof Error ? error.message : "Upload failed",
              }
            : file
        )
      );
    }
  };

  const getFileIcon = (file: File) => {
    if (file.type.startsWith("image/")) return PhotoIcon;
    return DocumentIcon;
  };

  const formatFileSize = (bytes: number) => {
    if (bytes === 0) return "0 Bytes";
    const k = 1024;
    const sizes = ["Bytes", "KB", "MB", "GB"];
    const i = Math.floor(Math.log(bytes) / Math.log(k));
    return parseFloat((bytes / Math.pow(k, i)).toFixed(2)) + " " + sizes[i];
  };

  return (
    <div className={`space-y-4 ${className}`}>
      {/* Drop Zone */}
      <div
        {...getRootProps()}
        className={`
          border-2 border-dashed rounded-lg p-6 text-center cursor-pointer transition-colors
          ${
            isDragActive
              ? "border-blue-500 bg-blue-50"
              : "border-gray-300 hover:border-gray-400"
          }
          ${disabled || isUploading ? "cursor-not-allowed opacity-50" : ""}
        `}
      >
        <input {...getInputProps()} />
        <CloudArrowUpIcon className="mx-auto h-12 w-12 text-gray-400" />
        <div className="mt-4">
          <p className="text-sm text-gray-600">
            {isDragActive
              ? "Drop files here..."
              : "Drag and drop files here, or click to select"}
          </p>
          <p className="text-xs text-gray-500 mt-1">
            Max {maxFiles} files, up to {formatFileSize(maxSize)} each
          </p>
        </div>
      </div>

      {/* File Rejections */}
      {fileRejections.length > 0 && (
        <div className="bg-red-50 border border-red-200 rounded-lg p-4">
          <h4 className="text-sm font-medium text-red-800 mb-2">
            Some files were rejected:
          </h4>
          <ul className="text-sm text-red-700 space-y-1">
            {fileRejections.map(({ file, errors }: any) => (
              <li key={file.name}>
                {file.name}: {errors.map((e: any) => e.message).join(", ")}
              </li>
            ))}
          </ul>
        </div>
      )}

      {/* Uploaded Files */}
      {uploadedFiles.length > 0 && (
        <div className="space-y-3">
          <h4 className="text-sm font-medium text-gray-900">
            {multiple ? "Uploaded Files" : "Uploaded File"}
          </h4>
          <div className="space-y-2">
            {uploadedFiles.map((uploadedFile, index) => {
              const IconComponent = getFileIcon(uploadedFile.file);
              return (
                <div
                  key={`${uploadedFile.file.name}-${index}`}
                  className="flex items-center justify-between p-3 bg-gray-50 rounded-lg"
                >
                  <div className="flex items-center space-x-3">
                    <IconComponent className="h-8 w-8 text-gray-400" />
                    <div className="min-w-0 flex-1">
                      <p className="text-sm font-medium text-gray-900 truncate">
                        {uploadedFile.file.name}
                      </p>
                      <p className="text-xs text-gray-500">
                        {formatFileSize(uploadedFile.file.size)}
                      </p>
                    </div>
                  </div>

                  <div className="flex items-center space-x-2">
                    {uploadedFile.uploading && <LoadingSpinner size="sm" />}

                    {uploadedFile.uploaded && (
                      <CheckCircleIcon className="h-5 w-5 text-green-500" />
                    )}

                    {uploadedFile.error && (
                      <div className="flex items-center space-x-2">
                        <span
                          className="text-xs text-red-600"
                          title={uploadedFile.error}
                        >
                          Failed
                        </span>
                        <button
                          onClick={() => retryUpload(index)}
                          className="text-xs text-blue-600 hover:text-blue-800"
                        >
                          Retry
                        </button>
                      </div>
                    )}

                    <button
                      onClick={() => removeFile(index)}
                      className="text-gray-400 hover:text-gray-600"
                      disabled={uploadedFile.uploading}
                    >
                      <XMarkIcon className="h-5 w-5" />
                    </button>
                  </div>
                </div>
              );
            })}
          </div>
        </div>
      )}
    </div>
  );
}
