import { AuthGuard } from "@/components/auth/AuthGuard";
import Link from "next/link";

export default function DashboardPage() {
  return (
    <AuthGuard>
      <div className="min-h-screen bg-gray-50">
        <div className="py-10">
          <header>
            <div className="max-w-7xl mx-auto px-4 sm:px-6 lg:px-8">
              <h1 className="text-3xl font-bold leading-tight text-gray-900">
                Dashboard
              </h1>
            </div>
          </header>
          <main>
            <div className="max-w-7xl mx-auto sm:px-6 lg:px-8">
              <div className="px-4 py-8 sm:px-0">
                <div className="border-4 border-dashed border-gray-200 rounded-lg p-8">
                  <div className="text-center">
                    <h2 className="text-2xl font-semibold text-gray-900 mb-4">
                      Welcome to ABC Portfolio Management System
                    </h2>
                    <p className="text-gray-600 mb-8">
                      You have successfully authenticated! This is a protected
                      dashboard area.
                    </p>

                    <div className="grid grid-cols-1 md:grid-cols-3 gap-6">
                      <Link
                        href="/portfolio"
                        className="bg-white p-6 rounded-lg shadow hover:shadow-lg transition-shadow block"
                      >
                        <h3 className="text-lg font-medium text-gray-900 mb-2">
                          Portfolios
                        </h3>
                        <p className="text-gray-600">
                          Manage your project portfolios
                        </p>
                      </Link>

                      <div className="bg-white p-6 rounded-lg shadow">
                        <h3 className="text-lg font-medium text-gray-900 mb-2">
                          Projects
                        </h3>
                        <p className="text-gray-600">
                          View and edit your projects
                        </p>
                      </div>

                      <div className="bg-white p-6 rounded-lg shadow">
                        <h3 className="text-lg font-medium text-gray-900 mb-2">
                          Skills
                        </h3>
                        <p className="text-gray-600">
                          Track your technical skills
                        </p>
                      </div>
                    </div>
                  </div>
                </div>
              </div>
            </div>
          </main>
        </div>
      </div>
    </AuthGuard>
  );
}
