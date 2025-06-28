# Step 7: External Authentication Providers Setup

## Objective
Implement external authentication providers (Google and Microsoft) to allow users to register and login using their existing social accounts.

## Implementation Plan

### 7.1 External Provider Configuration
- [ ] Add Google OAuth 2.0 configuration to appsettings.json
- [ ] Add Microsoft OAuth 2.0 configuration to appsettings.json
- [ ] Update AuthenticationConfigurationService to configure external providers
- [ ] Add external provider constants to AuthConstants

### 7.2 External Login Models
- [ ] Create ExternalLoginModel for external authentication requests
- [ ] Create ExternalLoginCallbackModel for handling provider callbacks
- [ ] Add external provider info to AuthResult model
- [ ] Create LinkExternalLoginModel for account linking

### 7.3 External Authentication Endpoints
- [ ] Add GET /api/Account/external-login/{provider} - Initiate external login
- [ ] Add POST /api/Account/external-login-callback - Handle provider callback
- [ ] Add POST /api/Account/link-external-login - Link external account to existing user
- [ ] Add POST /api/Account/remove-external-login - Remove external account link
- [ ] Add GET /api/Account/external-logins - List user's external logins

### 7.4 External Login Logic
- [ ] Implement external login initiation in AccountController
- [ ] Handle external login callbacks and token exchange
- [ ] Auto-create user accounts for new external users
- [ ] Link external accounts to existing users
- [ ] Manage external login information storage

### 7.5 Database Updates
- [ ] Verify AspNetUserLogins table exists (handled by Identity)
- [ ] Add external provider tracking to ApplicationUser if needed
- [ ] Update DataSeeder for external login test data

### 7.6 Enhanced Security
- [ ] Add state parameter validation for external logins
- [ ] Implement CSRF protection for external auth flows
- [ ] Add external login rate limiting
- [ ] Validate external provider tokens

### 7.7 Testing
- [ ] Create test endpoints for external provider simulation
- [ ] Update test scripts for external login flows
- [ ] Test account linking and unlinking
- [ ] Verify external user creation and login

## Files to Create/Modify

### New Files:
- `Core/Models/ExternalAuthModels.cs` - External authentication models
- `Infrastructure/Services/ExternalAuthenticationService.cs` - External auth logic
- `API/Controllers/ExternalAuthController.cs` - External auth endpoints (or extend AccountController)

### Files to Modify:
- `Core/Models/AuthModels.cs` - Add external auth properties
- `Core/Constants/AuthConstants.cs` - Add external provider constants
- `Infrastructure/Services/AuthenticationConfigurationService.cs` - Configure external providers
- `API/Controllers/AccountController.cs` - Add external auth endpoints
- `Backend/src/API/appsettings.json` - External provider configurations
- `API/test-account-controller.sh` - Add external auth tests

## Google OAuth 2.0 Setup Requirements
1. Google Cloud Console project setup
2. OAuth 2.0 client ID and secret
3. Authorized redirect URIs configuration
4. Scope configuration (email, profile)

## Microsoft OAuth 2.0 Setup Requirements
1. Azure AD app registration
2. Client ID and client secret
3. Redirect URI configuration
4. API permissions (User.Read)

## Success Criteria
- [ ] Users can login with Google accounts
- [ ] Users can login with Microsoft accounts
- [ ] External accounts can be linked to existing users
- [ ] External login information is properly stored
- [ ] Security measures prevent external auth attacks
- [ ] All external auth flows are thoroughly tested

Ready to begin Step 7 implementation?
