import { withAuth } from "next-auth/middleware"
import { NextResponse } from "next/server"

export default withAuth(
  function middleware(req) {
    const token = req.nextauth.token
    const isAuth = !!token
    const isAuthPage = req.nextUrl.pathname.startsWith('/auth')
    const isPublicPage = req.nextUrl.pathname === '/' || req.nextUrl.pathname.startsWith('/public')
    const isProtectedPage = req.nextUrl.pathname.startsWith('/dashboard') ||
                           req.nextUrl.pathname.startsWith('/portfolio') ||
                           req.nextUrl.pathname.startsWith('/projects')

    // Redirect authenticated users away from auth pages
    if (isAuthPage && isAuth) {
      return NextResponse.redirect(new URL('/dashboard', req.url))
    }

    // Allow access to public pages
    if (isPublicPage) {
      return NextResponse.next()
    }

    // Redirect unauthenticated users to login
    if (isProtectedPage && !isAuth) {
      const callbackUrl = encodeURIComponent(req.nextUrl.pathname + req.nextUrl.search)
      return NextResponse.redirect(new URL(`/auth/login?callbackUrl=${callbackUrl}`, req.url))
    }

    return NextResponse.next()
  },
  {
    callbacks: {
      authorized: ({ token, req }) => {
        // Allow API routes and auth pages without token
        if (req.nextUrl.pathname.startsWith('/api') ||
            req.nextUrl.pathname.startsWith('/auth') ||
            req.nextUrl.pathname === '/') {
          return true
        }

        // Require token for protected routes
        return !!token
      },
    },
  }
)

export const config = {
  matcher: [
    /*
     * Match all request paths except for the ones starting with:
     * - _next/static (static files)
     * - _next/image (image optimization files)
     * - favicon.ico (favicon file)
     * - public folder
     */
    '/((?!_next/static|_next/image|favicon.ico|public/).*)',
  ],
}
