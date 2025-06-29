"use client";

import { useAuth } from "@/stores/authStore";
import { signOut } from "next-auth/react";
import { useRouter } from "next/navigation";

interface LogoutButtonProps {
  className?: string;
  children?: React.ReactNode;
  redirectTo?: string;
}

export function LogoutButton({
  className = "px-4 py-2 text-sm text-gray-700 hover:bg-gray-100",
  children = "Sign Out",
  redirectTo = "/",
}: LogoutButtonProps) {
  const { logout } = useAuth();
  const router = useRouter();

  const handleLogout = async () => {
    try {
      // Sign out from NextAuth
      await signOut({ redirect: false });

      // Clear Zustand store
      await logout();

      // Redirect to specified page
      router.push(redirectTo);
    } catch (error) {
      console.error("Logout error:", error);
      // Force redirect even if logout fails
      router.push(redirectTo);
    }
  };

  return (
    <button onClick={handleLogout} className={className} type="button">
      {children}
    </button>
  );
}
