"use client";

import { useSession } from "next-auth/react";
import { useRouter, useSearchParams } from "next/navigation";
import { useEffect } from "react";

export default function CallbackPage() {
  const router = useRouter();
  const searchParams = useSearchParams();
  const { data: session, status } = useSession();

  useEffect(() => {
    if (status === "loading") return;

    const error = searchParams.get("error");
    const callbackUrl = searchParams.get("callbackUrl") || "/dashboard";

    if (error) {
      // Handle OAuth error
      console.error("OAuth callback error:", error);
      router.push(`/auth/login?error=${encodeURIComponent(error)}`);
      return;
    }

    if (session) {
      // Successful authentication, redirect to intended page
      router.push(callbackUrl);
    } else {
      // No session but no error, redirect to login
      router.push("/auth/login");
    }
  }, [session, status, router, searchParams]);

  return (
    <div className="min-h-screen flex items-center justify-center bg-gray-50">
      <div className="text-center">
        <div className="animate-spin rounded-full h-12 w-12 border-b-2 border-blue-600 mx-auto"></div>
        <h2 className="mt-4 text-lg font-medium text-gray-900">
          Signing you in...
        </h2>
        <p className="mt-2 text-gray-600">
          Please wait while we complete your authentication.
        </p>
      </div>
    </div>
  );
}
