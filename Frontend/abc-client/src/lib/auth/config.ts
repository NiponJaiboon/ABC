import { env } from '@/lib/utils/env'
import { NextAuthOptions } from 'next-auth'
import { JWT } from 'next-auth/jwt'

export const authOptions: NextAuthOptions = {
  providers: [
    {
      id: "abc-oauth",
      name: "ABC Portfolio System",
      type: "oauth",
      authorization: {
        url: `${env.NEXT_PUBLIC_API_URL}/connect/authorize`,
        params: {
          scope: env.NEXT_PUBLIC_OAUTH_SCOPE,
          response_type: "code",
          client_id: env.NEXT_PUBLIC_OAUTH_CLIENT_ID,
        }
      },
      token: `${env.NEXT_PUBLIC_API_URL}/connect/token`,
      userinfo: `${env.NEXT_PUBLIC_API_URL}/connect/userinfo`,
      clientId: env.NEXT_PUBLIC_OAUTH_CLIENT_ID,
      clientSecret: env.NEXTAUTH_SECRET, // Using NextAuth secret as client secret for now
      checks: ["pkce"],
      profile(profile) {
        return {
          id: profile.sub,
          email: profile.email,
          name: profile.name,
          image: profile.picture,
        }
      },
    }
  ],

  pages: {
    signIn: '/auth/login',
    signOut: '/auth/logout',
    error: '/auth/error',
  },

  callbacks: {
    async jwt({ token, account, profile }): Promise<JWT> {
      // Initial sign in
      if (account && profile) {
        return {
          ...token,
          accessToken: account.access_token,
          refreshToken: account.refresh_token,
          expiresAt: account.expires_at ? account.expires_at * 1000 : Date.now() + 60 * 60 * 1000, // 1 hour default
        }
      }

      // Return previous token if the access token has not expired yet
      if (Date.now() < (token.expiresAt as number)) {
        return token
      }

      // Access token has expired, try to refresh it
      return await refreshAccessToken(token)
    },

    async session({ session, token }) {
      if (token) {
        session.accessToken = token.accessToken as string
        session.error = token.error as string
      }
      return session
    },
  },

  events: {
    async signOut() {
      // Clear any client-side storage
      if (typeof window !== 'undefined') {
        localStorage.removeItem('auth-storage')
      }
    },
  },

  debug: process.env.NODE_ENV === 'development',
}

async function refreshAccessToken(token: JWT): Promise<JWT> {
  try {
    const response = await fetch(`${env.NEXT_PUBLIC_API_URL}/connect/token`, {
      headers: {
        "Content-Type": "application/x-www-form-urlencoded",
      },
      body: new URLSearchParams({
        client_id: env.NEXT_PUBLIC_OAUTH_CLIENT_ID,
        client_secret: env.NEXTAUTH_SECRET,
        grant_type: "refresh_token",
        refresh_token: token.refreshToken as string,
      }),
      method: "POST",
    })

    const tokens = await response.json()

    if (!response.ok) {
      throw tokens
    }

    return {
      ...token,
      accessToken: tokens.access_token,
      expiresAt: Date.now() + tokens.expires_in * 1000,
      refreshToken: tokens.refresh_token ?? token.refreshToken, // Fall back to old refresh token
    }
  } catch (error) {
    console.error("Error refreshing access token", error)

    return {
      ...token,
      error: "RefreshAccessTokenError",
    }
  }
}
