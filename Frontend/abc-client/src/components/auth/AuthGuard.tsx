"use client";

import { useAuth } from "@/stores/authStore";
import { useSession } from "next-auth/react";
import { useRouter } from "next/navigation";
import { useEffect } from "react";

interface AuthGuardProps {
  children: React.ReactNode;
  redirectTo?: string;
  requiredRole?: string;
  requiredPermission?: string;
}

export function AuthGuard({
  children,
  redirectTo = "/auth/login",
  requiredRole,
  requiredPermission,
}: AuthGuardProps) {
  const router = useRouter();
  const { data: session, status } = useSession();
  const { isLoggedIn, hasRole, hasPermission } = useAuth();

  useEffect(() => {
    // Wait for session to load
    if (status === "loading") return;

    // Redirect if not authenticated
    if (!session && !isLoggedIn) {
      router.push(redirectTo);
      return;
    }

    // Check role requirement
    if (requiredRole && !hasRole(requiredRole)) {
      router.push("/auth/unauthorized");
      return;
    }

    // Check permission requirement
    if (requiredPermission && !hasPermission(requiredPermission)) {
      router.push("/auth/unauthorized");
      return;
    }
  }, [
    session,
    status,
    isLoggedIn,
    requiredRole,
    requiredPermission,
    hasRole,
    hasPermission,
    router,
    redirectTo,
  ]);

  // Show loading while checking authentication
  if (status === "loading") {
    return (
      <div className="flex items-center justify-center min-h-screen">
        <div className="animate-spin rounded-full h-12 w-12 border-b-2 border-blue-600"></div>
      </div>
    );
  }

  // Don't render children if not authenticated
  if (!session && !isLoggedIn) {
    return null;
  }

  // Don't render if role/permission check fails
  if (requiredRole && !hasRole(requiredRole)) {
    return null;
  }

  if (requiredPermission && !hasPermission(requiredPermission)) {
    return null;
  }

  return <>{children}</>;
}

// Higher-order component version
export function withAuthGuard<P extends object>(
  Component: React.ComponentType<P>,
  options?: {
    redirectTo?: string;
    requiredRole?: string;
    requiredPermission?: string;
  }
) {
  return function AuthGuardedComponent(props: P) {
    return (
      <AuthGuard {...options}>
        <Component {...props} />
      </AuthGuard>
    );
  };
}
