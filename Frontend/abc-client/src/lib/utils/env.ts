// Environment variables validation and configuration
export const env = {
  // API Configuration - defaults to local development
  NEXT_PUBLIC_API_URL: process.env.NEXT_PUBLIC_API_URL || 'http://localhost:5011',
  NEXT_PUBLIC_API_VERSION: process.env.NEXT_PUBLIC_API_VERSION || 'v1',

  // OAuth 2.0 Configuration
  NEXT_PUBLIC_OAUTH_CLIENT_ID: process.env.NEXT_PUBLIC_OAUTH_CLIENT_ID || 'ABC_NextJS_Client',
  NEXT_PUBLIC_OAUTH_REDIRECT_URI: process.env.NEXT_PUBLIC_OAUTH_REDIRECT_URI || 'http://localhost:3000/auth/callback',
  NEXT_PUBLIC_OAUTH_SCOPE: process.env.NEXT_PUBLIC_OAUTH_SCOPE || 'openid profile email api_access',

  // NextAuth Configuration
  NEXTAUTH_URL: process.env.NEXTAUTH_URL || 'http://localhost:3000',
  NEXTAUTH_SECRET: process.env.NEXTAUTH_SECRET || 'abc-portfolio-nextauth-secret-key-2025',

  // Feature Flags
  NEXT_PUBLIC_ENABLE_ANALYTICS: process.env.NEXT_PUBLIC_ENABLE_ANALYTICS === 'true',
  NEXT_PUBLIC_ENABLE_LOGGING: process.env.NEXT_PUBLIC_ENABLE_LOGGING === 'true',
} as const

// Validate required environment variables
const requiredEnvVars = [
  'NEXT_PUBLIC_API_URL',
  'NEXT_PUBLIC_OAUTH_CLIENT_ID',
  'NEXTAUTH_SECRET'
] as const

export function validateEnv() {
  const missing = requiredEnvVars.filter(
    (envVar) => !process.env[envVar]
  )

  if (missing.length > 0) {
    throw new Error(
      `Missing required environment variables: ${missing.join(', ')}`
    )
  }
}

export default env
