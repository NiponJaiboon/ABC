"use client";

import { AuthGuard } from "@/components/auth/AuthGuard";
import { DashboardAnalytics } from "@/components/dashboard/DashboardAnalytics";
import {
  AcademicCapIcon,
  BriefcaseIcon,
  ChartBarIcon,
} from "@heroicons/react/24/outline";
import Link from "next/link";

export default function DashboardPage() {
  const quickActions = [
    {
      title: "Portfolio Management",
      description: "Create and manage your portfolios",
      href: "/portfolio",
      icon: BriefcaseIcon,
      color: "bg-blue-500",
    },
    {
      title: "Project Management",
      description: "Track and organize your projects",
      href: "/projects",
      icon: ChartBarIcon,
      color: "bg-green-500",
    },
    {
      title: "Skills Management",
      description: "Develop and track your skills",
      href: "/skills",
      icon: AcademicCapIcon,
      color: "bg-purple-500",
    },
  ];

  return (
    <AuthGuard>
      <div className="max-w-7xl mx-auto px-4 sm:px-6 lg:px-8 py-8">
        {/* Header */}
        <div className="mb-8">
          <h1 className="text-2xl font-bold text-gray-900">Dashboard</h1>
          <p className="text-gray-600 mt-1">
            Welcome back! Here's an overview of your portfolio management
            system.
          </p>
        </div>

        {/* Analytics Section */}
        <div className="mb-8">
          <DashboardAnalytics />
        </div>

        {/* Quick Actions */}
        <div className="mb-8">
          <h2 className="text-lg font-semibold text-gray-900 mb-4">
            Quick Actions
          </h2>
          <div className="grid grid-cols-1 md:grid-cols-2 lg:grid-cols-3 gap-6">
            {quickActions.map((action) => {
              const IconComponent = action.icon;
              return (
                <Link
                  key={action.title}
                  href={action.href}
                  className="group relative bg-white p-6 rounded-lg shadow hover:shadow-md transition-shadow duration-200"
                >
                  <div className="flex items-center">
                    <div className={`p-3 rounded-lg ${action.color}`}>
                      <IconComponent className="w-6 h-6 text-white" />
                    </div>
                    <div className="ml-4">
                      <h3 className="text-sm font-medium text-gray-900 group-hover:text-blue-600 transition-colors">
                        {action.title}
                      </h3>
                      <p className="text-xs text-gray-500 mt-1">
                        {action.description}
                      </p>
                    </div>
                  </div>
                  <div className="absolute inset-0 rounded-lg border-2 border-transparent group-hover:border-blue-500 transition-colors duration-200" />
                </Link>
              );
            })}
          </div>
        </div>

        {/* Getting Started Section */}
        <div className="bg-blue-50 border border-blue-200 rounded-lg p-6">
          <h2 className="text-lg font-semibold text-blue-900 mb-2">
            Getting Started
          </h2>
          <p className="text-blue-700 mb-4">
            New to the ABC Portfolio Management System? Here are some steps to
            help you get started:
          </p>
          <div className="space-y-2">
            <div className="flex items-center text-sm text-blue-700">
              <span className="w-2 h-2 bg-blue-500 rounded-full mr-3"></span>
              Create your first portfolio to organize your work
            </div>
            <div className="flex items-center text-sm text-blue-700">
              <span className="w-2 h-2 bg-blue-500 rounded-full mr-3"></span>
              Add projects to showcase your achievements
            </div>
            <div className="flex items-center text-sm text-blue-700">
              <span className="w-2 h-2 bg-blue-500 rounded-full mr-3"></span>
              Track your skills and proficiency levels
            </div>
            <div className="flex items-center text-sm text-blue-700">
              <span className="w-2 h-2 bg-blue-500 rounded-full mr-3"></span>
              Monitor your progress through the analytics dashboard
            </div>
          </div>
        </div>
      </div>
    </AuthGuard>
  );
}
