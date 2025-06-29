"use client";

import { LoadingSpinner } from "@/components/shared/LoadingSpinner";
import { usePortfolios } from "@/lib/hooks/usePortfolio";
import { useProjects } from "@/lib/hooks/useProjects";
import { useSkills } from "@/lib/hooks/useSkills";
import {
  AcademicCapIcon,
  BriefcaseIcon,
  ChartBarIcon,
  ClockIcon,
  StarIcon,
} from "@heroicons/react/24/outline";
import { useEffect, useState } from "react";

interface DashboardStats {
  totalPortfolios: number;
  totalProjects: number;
  totalSkills: number;
  activeProjects: number;
  completedProjects: number;
  expertSkills: number;
  recentActivity: number;
}

interface ActivityItem {
  id: string;
  type: "portfolio" | "project" | "skill";
  action: "created" | "updated" | "completed";
  title: string;
  timestamp: Date;
  description?: string;
}

export function DashboardAnalytics() {
  const [stats, setStats] = useState<DashboardStats>({
    totalPortfolios: 0,
    totalProjects: 0,
    totalSkills: 0,
    activeProjects: 0,
    completedProjects: 0,
    expertSkills: 0,
    recentActivity: 0,
  });

  const [recentActivity, setRecentActivity] = useState<ActivityItem[]>([]);

  // Fetch data
  const { data: portfoliosData, isLoading: portfoliosLoading } = usePortfolios({
    page: 1,
    limit: 100,
  });

  const { data: projectsData, isLoading: projectsLoading } = useProjects({
    page: 1,
    limit: 100,
  });

  const { data: skillsData, isLoading: skillsLoading } = useSkills({
    page: 1,
    limit: 100,
  });

  const isLoading = portfoliosLoading || projectsLoading || skillsLoading;

  // Calculate stats
  useEffect(() => {
    const portfolios = portfoliosData?.data || [];
    const projects = projectsData?.data || [];
    const skills = skillsData?.data || [];

    const activeProjects = projects.filter(
      (p) => p.status === "in-progress"
    ).length;
    const completedProjects = projects.filter(
      (p) => p.status === "completed"
    ).length;
    const expertSkills = skills.filter(
      (s) => s.proficiencyLevel === "Expert"
    ).length;

    // Create recent activity (mock data for now)
    const activity: ActivityItem[] = [
      ...portfolios.slice(0, 3).map((p) => ({
        id: p.id,
        type: "portfolio" as const,
        action: "updated" as const,
        title: p.name,
        timestamp: new Date(p.updatedAt),
        description: `Portfolio updated`,
      })),
      ...projects.slice(0, 5).map((p) => ({
        id: p.id,
        type: "project" as const,
        action:
          p.status === "completed"
            ? ("completed" as const)
            : ("updated" as const),
        title: p.title,
        timestamp: new Date(p.updatedAt),
        description: `Project ${p.status}`,
      })),
      ...skills.slice(0, 4).map((s) => ({
        id: s.id,
        type: "skill" as const,
        action: "updated" as const,
        title: s.name,
        timestamp: new Date(s.updatedAt),
        description: `Skill proficiency: ${s.proficiencyLevel}`,
      })),
    ]
      .sort((a, b) => b.timestamp.getTime() - a.timestamp.getTime())
      .slice(0, 10);

    setStats({
      totalPortfolios: portfolios.length,
      totalProjects: projects.length,
      totalSkills: skills.length,
      activeProjects,
      completedProjects,
      expertSkills,
      recentActivity: activity.length,
    });

    setRecentActivity(activity);
  }, [portfoliosData, projectsData, skillsData]);

  // Stats cards configuration
  const statsCards = [
    {
      title: "Total Portfolios",
      value: stats.totalPortfolios,
      icon: BriefcaseIcon,
      color: "bg-blue-500",
      change: "+2 this month",
      changeType: "positive" as const,
    },
    {
      title: "Total Projects",
      value: stats.totalProjects,
      icon: ChartBarIcon,
      color: "bg-green-500",
      change: `${stats.activeProjects} active`,
      changeType: "neutral" as const,
    },
    {
      title: "Total Skills",
      value: stats.totalSkills,
      icon: AcademicCapIcon,
      color: "bg-purple-500",
      change: `${stats.expertSkills} expert level`,
      changeType: "positive" as const,
    },
    {
      title: "Completed Projects",
      value: stats.completedProjects,
      icon: StarIcon,
      color: "bg-orange-500",
      change: "75% completion rate",
      changeType: "positive" as const,
    },
  ];

  // Chart data for skills by proficiency
  const skillsProficiencyData =
    skillsData?.data.reduce((acc, skill) => {
      acc[skill.proficiencyLevel] = (acc[skill.proficiencyLevel] || 0) + 1;
      return acc;
    }, {} as Record<string, number>) || {};

  // Chart data for projects by status
  const projectsStatusData =
    projectsData?.data.reduce((acc, project) => {
      acc[project.status] = (acc[project.status] || 0) + 1;
      return acc;
    }, {} as Record<string, number>) || {};

  const getActivityIcon = (type: string) => {
    switch (type) {
      case "portfolio":
        return BriefcaseIcon;
      case "project":
        return ChartBarIcon;
      case "skill":
        return AcademicCapIcon;
      default:
        return ClockIcon;
    }
  };

  const getActivityColor = (action: string) => {
    switch (action) {
      case "created":
        return "text-green-600";
      case "updated":
        return "text-blue-600";
      case "completed":
        return "text-purple-600";
      default:
        return "text-gray-600";
    }
  };

  const getChangeTextColor = (
    changeType: "positive" | "negative" | "neutral"
  ) => {
    switch (changeType) {
      case "positive":
        return "text-green-600";
      case "negative":
        return "text-red-600";
      default:
        return "text-gray-600";
    }
  };

  const getProficiencyColor = (level: string) => {
    switch (level) {
      case "Expert":
        return "bg-purple-500";
      case "Advanced":
        return "bg-blue-500";
      case "Intermediate":
        return "bg-green-500";
      default:
        return "bg-yellow-500";
    }
  };

  const getStatusColor = (status: string) => {
    switch (status) {
      case "completed":
        return "bg-green-500";
      case "in-progress":
        return "bg-blue-500";
      case "draft":
        return "bg-yellow-500";
      default:
        return "bg-gray-500";
    }
  };

  const formatTimeAgo = (date: Date) => {
    const now = new Date();
    const diffInHours = (now.getTime() - date.getTime()) / (1000 * 60 * 60);

    if (diffInHours < 1) return "Just now";
    if (diffInHours < 24) return `${Math.floor(diffInHours)}h ago`;
    return `${Math.floor(diffInHours / 24)}d ago`;
  };

  if (isLoading) {
    return (
      <div className="flex items-center justify-center py-12">
        <LoadingSpinner size="lg" />
      </div>
    );
  }

  return (
    <div className="space-y-6">
      {/* Stats Cards */}
      <div className="grid grid-cols-1 md:grid-cols-2 lg:grid-cols-4 gap-6">
        {statsCards.map((card) => {
          const IconComponent = card.icon;
          return (
            <div key={card.title} className="bg-white rounded-lg shadow p-6">
              <div className="flex items-center">
                <div className={`p-3 rounded-lg ${card.color}`}>
                  <IconComponent className="w-6 h-6 text-white" />
                </div>
                <div className="ml-4">
                  <h3 className="text-lg font-semibold text-gray-900">
                    {card.value}
                  </h3>
                  <p className="text-sm text-gray-600">{card.title}</p>
                </div>
              </div>
              <div className="mt-4">
                <span
                  className={`text-sm ${getChangeTextColor(card.changeType)}`}
                >
                  {card.change}
                </span>
              </div>
            </div>
          );
        })}
      </div>

      {/* Charts Row */}
      <div className="grid grid-cols-1 lg:grid-cols-2 gap-6">
        {/* Skills Proficiency Chart */}
        <div className="bg-white rounded-lg shadow p-6">
          <h3 className="text-lg font-semibold text-gray-900 mb-4">
            Skills by Proficiency Level
          </h3>
          <div className="space-y-3">
            {Object.entries(skillsProficiencyData).map(([level, count]) => (
              <div key={level} className="flex items-center">
                <div className="w-20 text-sm text-gray-600">{level}</div>
                <div className="flex-1 mx-3">
                  <div className="bg-gray-200 rounded-full h-3">
                    <div
                      className={`h-3 rounded-full ${getProficiencyColor(
                        level
                      )}`}
                      style={{
                        width: `${
                          stats.totalSkills
                            ? (count / stats.totalSkills) * 100
                            : 0
                        }%`,
                      }}
                    />
                  </div>
                </div>
                <div className="w-8 text-sm text-gray-900 text-right">
                  {count}
                </div>
              </div>
            ))}
          </div>
        </div>

        {/* Projects Status Chart */}
        <div className="bg-white rounded-lg shadow p-6">
          <h3 className="text-lg font-semibold text-gray-900 mb-4">
            Projects by Status
          </h3>
          <div className="space-y-3">
            {Object.entries(projectsStatusData).map(([status, count]) => (
              <div key={status} className="flex items-center">
                <div className="w-20 text-sm text-gray-600 capitalize">
                  {status.replace("-", " ")}
                </div>
                <div className="flex-1 mx-3">
                  <div className="bg-gray-200 rounded-full h-3">
                    <div
                      className={`h-3 rounded-full ${getStatusColor(status)}`}
                      style={{
                        width: `${
                          stats.totalProjects
                            ? (count / stats.totalProjects) * 100
                            : 0
                        }%`,
                      }}
                    />
                  </div>
                </div>
                <div className="w-8 text-sm text-gray-900 text-right">
                  {count}
                </div>
              </div>
            ))}
          </div>
        </div>
      </div>

      {/* Recent Activity */}
      <div className="bg-white rounded-lg shadow">
        <div className="p-6 border-b border-gray-200">
          <h3 className="text-lg font-semibold text-gray-900">
            Recent Activity
          </h3>
        </div>
        <div className="p-6">
          {recentActivity.length === 0 ? (
            <p className="text-gray-500 text-center py-8">No recent activity</p>
          ) : (
            <div className="space-y-4">
              {recentActivity.map((activity) => {
                const IconComponent = getActivityIcon(activity.type);
                return (
                  <div key={activity.id} className="flex items-start space-x-3">
                    <div className={`p-2 rounded-lg bg-gray-100`}>
                      <IconComponent className="w-4 h-4 text-gray-600" />
                    </div>
                    <div className="flex-1 min-w-0">
                      <p className="text-sm">
                        <span className="font-medium text-gray-900">
                          {activity.title}
                        </span>
                        <span
                          className={`ml-2 ${getActivityColor(
                            activity.action
                          )}`}
                        >
                          {activity.action}
                        </span>
                      </p>
                      {activity.description && (
                        <p className="text-xs text-gray-500 mt-1">
                          {activity.description}
                        </p>
                      )}
                    </div>
                    <div className="text-xs text-gray-500">
                      {formatTimeAgo(activity.timestamp)}
                    </div>
                  </div>
                );
              })}
            </div>
          )}
        </div>
      </div>
    </div>
  );
}
