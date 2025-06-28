using Core.Constants;
using Core.Entities;
using Core.Interfaces;
using Core.Models;
using Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace Infrastructure.Services;

// Step 12: Authorization & Scopes - Consent Management Service
public class ConsentService : IConsentService
{
    private readonly ApplicationDbContext _context;
    private readonly IScopeService _scopeService;
    private readonly IOAuthClientService _clientService;
    private readonly ILogger<ConsentService> _logger;

    public ConsentService(
        ApplicationDbContext context,
        IScopeService scopeService,
        IOAuthClientService clientService,
        ILogger<ConsentService> logger)
    {
        _context = context;
        _scopeService = scopeService;
        _clientService = clientService;
        _logger = logger;
    }

    public async Task<ConsentModel> GetConsentModelAsync(string userId, string clientId, List<string> requestedScopes)
    {
        try
        {
            // Get client information
            var client = await _clientService.GetClientAsync(clientId);
            if (client == null)
            {
                throw new ArgumentException($"Client '{clientId}' not found");
            }

            // Validate and get scope details
            var scopeValidation = await _scopeService.ValidateScopesAsync(requestedScopes, clientId);
            if (!scopeValidation.IsValid)
            {
                throw new ArgumentException($"Invalid scopes: {string.Join(", ", scopeValidation.InvalidScopes)}");
            }

            // Get existing consent
            var existingConsent = await _context.UserConsents
                .FirstOrDefaultAsync(c => c.UserId == userId && c.ClientId == clientId && !c.IsRevoked);

            var consentedScopes = existingConsent?.GrantedScopes?.Split(' ', StringSplitOptions.RemoveEmptyEntries).ToList() ?? new List<string>();

            // Build consent model
            var scopeDetails = new List<ScopeConsentDetail>();
            var requestedScopeItems = new List<ScopeConsentItem>();

            foreach (var scope in requestedScopes)
            {
                var scopeDefinition = await _scopeService.GetScopeAsync(scope);
                if (scopeDefinition != null)
                {
                    scopeDetails.Add(new ScopeConsentDetail
                    {
                        Name = scope,
                        DisplayName = scopeDefinition.DisplayName,
                        Description = scopeDefinition.Description,
                        IsRequired = scopeDefinition.IsRequired,
                        IsAlreadyConsented = consentedScopes.Contains(scope)
                    });

                    requestedScopeItems.Add(new ScopeConsentItem
                    {
                        Name = scope,
                        DisplayName = scopeDefinition.DisplayName,
                        Description = scopeDefinition.Description,
                        IsRequired = scopeDefinition.IsRequired,
                        IsGranted = consentedScopes.Contains(scope),
                        Permissions = scopeDefinition.Permissions
                    });
                }
            }

            return new ConsentModel
            {
                ClientId = clientId,
                ClientName = client.ClientName,
                ClientDescription = client.Description,
                RequestedScopes = requestedScopeItems,
                ScopeDetails = scopeDetails,
                HasExistingConsent = existingConsent != null,
                RememberConsent = existingConsent?.RememberConsent ?? false
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting consent model for user {UserId}, client {ClientId}", userId, clientId);
            throw;
        }
    }

    public async Task<ConsentResponse> ProcessConsentAsync(string userId, string clientId, List<string> grantedScopes, bool rememberConsent)
    {
        try
        {
            // Validate client
            var client = await _clientService.GetClientAsync(clientId);
            if (client == null)
            {
                return new ConsentResponse
                {
                    IsSuccess = false,
                    Error = "invalid_client",
                    ErrorDescription = "Client not found"
                };
            }

            // Validate scopes
            var scopeValidation = await _scopeService.ValidateScopesAsync(grantedScopes, clientId);
            if (!scopeValidation.IsValid)
            {
                return new ConsentResponse
                {
                    IsSuccess = false,
                    Error = "invalid_scope",
                    ErrorDescription = $"Invalid scopes: {string.Join(", ", scopeValidation.InvalidScopes)}"
                };
            }

            // Check for required scopes
            var availableScopes = await _scopeService.GetAvailableScopesAsync();
            var requiredScopes = availableScopes.Where(s => s.IsRequired).Select(s => s.Name).ToList();
            var missingRequiredScopes = requiredScopes.Except(grantedScopes).ToList();

            if (missingRequiredScopes.Any())
            {
                return new ConsentResponse
                {
                    IsSuccess = false,
                    Error = "consent_required",
                    ErrorDescription = $"Required scopes not granted: {string.Join(", ", missingRequiredScopes)}"
                };
            }

            // Create or update consent record
            var existingConsent = await _context.UserConsents
                .FirstOrDefaultAsync(c => c.UserId == userId && c.ClientId == clientId);

            if (existingConsent != null)
            {
                existingConsent.GrantedScopes = string.Join(' ', grantedScopes);
                existingConsent.RememberConsent = rememberConsent;
                existingConsent.UpdatedAt = DateTime.UtcNow;
                existingConsent.IsRevoked = false;
            }
            else
            {
                var newConsent = new UserConsent
                {
                    Id = Guid.NewGuid().ToString(),
                    UserId = userId,
                    ClientId = clientId,
                    GrantedScopes = string.Join(' ', grantedScopes),
                    RememberConsent = rememberConsent,
                    ConsentedAt = DateTime.UtcNow,
                    CreatedAt = DateTime.UtcNow,
                    UpdatedAt = DateTime.UtcNow,
                    IsRevoked = false
                };

                _context.UserConsents.Add(newConsent);
            }

            await _context.SaveChangesAsync();

            _logger.LogInformation("Consent processed for user {UserId}, client {ClientId}, scopes: {Scopes}",
                userId, clientId, string.Join(", ", grantedScopes));

            return new ConsentResponse
            {
                IsSuccess = true,
                GrantedScopes = grantedScopes
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error processing consent for user {UserId}, client {ClientId}", userId, clientId);
            return new ConsentResponse
            {
                IsSuccess = false,
                Error = "server_error",
                ErrorDescription = "An error occurred while processing consent"
            };
        }
    }

    public async Task<bool> HasValidConsentAsync(string userId, string clientId, List<string> requestedScopes)
    {
        try
        {
            var consent = await _context.UserConsents
                .FirstOrDefaultAsync(c => c.UserId == userId && c.ClientId == clientId && !c.IsRevoked);

            if (consent == null || !consent.RememberConsent)
                return false;

            var grantedScopes = consent.GrantedScopes?.Split(' ', StringSplitOptions.RemoveEmptyEntries).ToList() ?? new List<string>();

            // Check if all requested scopes are granted
            return requestedScopes.All(scope => grantedScopes.Contains(scope));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error checking consent for user {UserId}, client {ClientId}", userId, clientId);
            return false;
        }
    }

    public async Task<List<UserConsent>> GetUserConsentsAsync(string userId)
    {
        try
        {
            return await _context.UserConsents
                .Where(c => c.UserId == userId && !c.IsRevoked)
                .OrderByDescending(c => c.ConsentedAt)
                .ToListAsync();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting consents for user {UserId}", userId);
            return new List<UserConsent>();
        }
    }

    public async Task<bool> RevokeConsentAsync(string userId, string clientId, string? reason = null)
    {
        try
        {
            var consent = await _context.UserConsents
                .FirstOrDefaultAsync(c => c.UserId == userId && c.ClientId == clientId && !c.IsRevoked);

            if (consent == null)
                return false;

            consent.IsRevoked = true;
            consent.RevokedAt = DateTime.UtcNow;
            consent.UpdatedAt = DateTime.UtcNow;

            await _context.SaveChangesAsync();

            _logger.LogInformation("Consent revoked for user {UserId}, client {ClientId}. Reason: {Reason}",
                userId, clientId, reason ?? "Not specified");

            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error revoking consent for user {UserId}, client {ClientId}", userId, clientId);
            return false;
        }
    }

    public async Task<bool> RevokeAllUserConsentsAsync(string userId)
    {
        try
        {
            var consents = await _context.UserConsents
                .Where(c => c.UserId == userId && !c.IsRevoked)
                .ToListAsync();

            if (!consents.Any())
                return true;

            foreach (var consent in consents)
            {
                consent.IsRevoked = true;
                consent.RevokedAt = DateTime.UtcNow;
                consent.UpdatedAt = DateTime.UtcNow;
            }

            await _context.SaveChangesAsync();

            _logger.LogInformation("All consents revoked for user {UserId}", userId);

            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error revoking all consents for user {UserId}", userId);
            return false;
        }
    }
}
