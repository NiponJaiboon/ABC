using System.Text.Json;
using Core.Constants;
using Core.Entities;
using Core.Interfaces;
using Core.Models;
using Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace Infrastructure.Services;

public class OAuthClientService : IOAuthClientService
{
    private readonly ApplicationDbContext _context;
    private readonly ILogger<OAuthClientService> _logger;

    public OAuthClientService(
        ApplicationDbContext context,
        ILogger<OAuthClientService> logger)
    {
        _context = context;
        _logger = logger;
    }

    public async Task<ClientRegistrationResult> RegisterClientAsync(ClientRegistrationModel model, string createdBy)
    {
        try
        {
            // Validate the model
            var validationErrors = ValidateClientRegistration(model);
            if (validationErrors.Count > 0)
            {
                return new ClientRegistrationResult
                {
                    Success = false,
                    Errors = validationErrors
                };
            }

            // Generate client credentials
            var clientId = GenerateClientId();
            var clientSecret = model.RequireClientSecret ? GenerateClientSecret() : null;

            var client = new OAuthClient
            {
                ClientId = clientId,
                ClientName = model.ClientName,
                ClientType = model.ClientType,
                Description = model.Description,
                ClientSecret = clientSecret,
                RedirectUris = JsonSerializer.Serialize(model.RedirectUris),
                PostLogoutRedirectUris = JsonSerializer.Serialize(model.PostLogoutRedirectUris),
                Scopes = JsonSerializer.Serialize(model.Scopes),
                GrantTypes = JsonSerializer.Serialize(model.GrantTypes),
                RequirePkce = model.RequirePkce,
                RequireClientSecret = model.RequireClientSecret,
                ClientUri = model.ClientUri,
                LogoUri = model.LogoUri,
                ContactEmail = model.ContactEmail,
                CreatedBy = createdBy,
                CreatedAt = DateTime.UtcNow
            };

            _context.OAuthClients.Add(client);
            await _context.SaveChangesAsync();

            _logger.LogInformation("OAuth client registered: {ClientId} by {CreatedBy}", clientId, createdBy);

            return new ClientRegistrationResult
            {
                Success = true,
                ClientId = clientId,
                ClientSecret = clientSecret,
                CreatedAt = client.CreatedAt,
                Client = MapToClientInfo(client)
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error registering OAuth client");
            return new ClientRegistrationResult
            {
                Success = false,
                Errors = new List<string> { "An error occurred while registering the client" }
            };
        }
    }

    public async Task<ClientInfo?> GetClientAsync(string clientId)
    {
        try
        {
            var client = await _context.OAuthClients
                .FirstOrDefaultAsync(c => c.ClientId == clientId);

            return client != null ? MapToClientInfo(client) : null;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting OAuth client {ClientId}", clientId);
            return null;
        }
    }

    public async Task<ClientListResponse> GetClientsAsync(int pageNumber = 1, int pageSize = 10, string? createdBy = null)
    {
        try
        {
            var query = _context.OAuthClients.AsQueryable();

            if (!string.IsNullOrEmpty(createdBy))
            {
                query = query.Where(c => c.CreatedBy == createdBy);
            }

            var totalCount = await query.CountAsync();
            var clients = await query
                .OrderByDescending(c => c.CreatedAt)
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            return new ClientListResponse
            {
                Clients = clients.Select(MapToClientInfo).ToList(),
                TotalCount = totalCount,
                PageNumber = pageNumber,
                PageSize = pageSize,
                HasNextPage = pageNumber * pageSize < totalCount,
                HasPreviousPage = pageNumber > 1
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting OAuth clients");
            return new ClientListResponse();
        }
    }

    public async Task<bool> UpdateClientAsync(ClientUpdateModel model, string updatedBy)
    {
        try
        {
            var client = await _context.OAuthClients
                .FirstOrDefaultAsync(c => c.ClientId == model.ClientId);

            if (client == null)
            {
                return false;
            }

            // Update only provided fields
            if (!string.IsNullOrEmpty(model.ClientName))
                client.ClientName = model.ClientName;

            if (!string.IsNullOrEmpty(model.Description))
                client.Description = model.Description;

            if (model.RedirectUris != null)
                client.RedirectUris = JsonSerializer.Serialize(model.RedirectUris);

            if (model.PostLogoutRedirectUris != null)
                client.PostLogoutRedirectUris = JsonSerializer.Serialize(model.PostLogoutRedirectUris);

            if (model.Scopes != null)
                client.Scopes = JsonSerializer.Serialize(model.Scopes);

            if (model.RequirePkce.HasValue)
                client.RequirePkce = model.RequirePkce.Value;

            if (!string.IsNullOrEmpty(model.ClientUri))
                client.ClientUri = model.ClientUri;

            if (!string.IsNullOrEmpty(model.LogoUri))
                client.LogoUri = model.LogoUri;

            if (model.IsActive.HasValue)
                client.IsActive = model.IsActive.Value;

            await _context.SaveChangesAsync();

            _logger.LogInformation("OAuth client updated: {ClientId} by {UpdatedBy}", model.ClientId, updatedBy);
            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating OAuth client {ClientId}", model.ClientId);
            return false;
        }
    }

    public async Task<bool> DeleteClientAsync(string clientId, string deletedBy)
    {
        try
        {
            var client = await _context.OAuthClients
                .FirstOrDefaultAsync(c => c.ClientId == clientId);

            if (client == null)
            {
                return false;
            }

            // Soft delete by deactivating
            client.IsActive = false;
            await _context.SaveChangesAsync();

            _logger.LogInformation("OAuth client deleted: {ClientId} by {DeletedBy}", clientId, deletedBy);
            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting OAuth client {ClientId}", clientId);
            return false;
        }
    }

    public async Task<bool> ValidateClientAsync(string clientId, string? clientSecret = null)
    {
        try
        {
            var client = await _context.OAuthClients
                .FirstOrDefaultAsync(c => c.ClientId == clientId && c.IsActive);

            if (client == null)
            {
                return false;
            }

            // If client requires secret, validate it
            if (client.RequireClientSecret)
            {
                if (string.IsNullOrEmpty(clientSecret) || client.ClientSecret != clientSecret)
                {
                    return false;
                }
            }

            // Update usage statistics
            client.LastUsed = DateTime.UtcNow;
            client.UsageCount++;
            await _context.SaveChangesAsync();

            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error validating OAuth client {ClientId}", clientId);
            return false;
        }
    }

    public async Task<bool> ValidateRedirectUriAsync(string clientId, string redirectUri)
    {
        try
        {
            var client = await _context.OAuthClients
                .FirstOrDefaultAsync(c => c.ClientId == clientId && c.IsActive);

            if (client == null)
            {
                return false;
            }

            var redirectUris = JsonSerializer.Deserialize<List<string>>(client.RedirectUris) ?? new List<string>();
            return redirectUris.Contains(redirectUri, StringComparer.OrdinalIgnoreCase);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error validating redirect URI for client {ClientId}", clientId);
            return false;
        }
    }

    public async Task<List<string>> GetClientScopesAsync(string clientId)
    {
        try
        {
            var client = await _context.OAuthClients
                .FirstOrDefaultAsync(c => c.ClientId == clientId && c.IsActive);

            if (client == null)
            {
                return new List<string>();
            }

            return JsonSerializer.Deserialize<List<string>>(client.Scopes) ?? new List<string>();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting client scopes for {ClientId}", clientId);
            return new List<string>();
        }
    }

    private static List<string> ValidateClientRegistration(ClientRegistrationModel model)
    {
        var errors = new List<string>();

        if (string.IsNullOrWhiteSpace(model.ClientName))
            errors.Add("Client name is required");

        if (!IsValidClientType(model.ClientType))
            errors.Add("Invalid client type");

        if (model.RedirectUris == null || model.RedirectUris.Count == 0)
            errors.Add("At least one redirect URI is required");

        if (model.Scopes == null || model.Scopes.Count == 0)
            errors.Add("At least one scope is required");

        // Validate redirect URIs
        foreach (var uri in model.RedirectUris ?? new List<string>())
        {
            if (!Uri.TryCreate(uri, UriKind.Absolute, out _))
                errors.Add($"Invalid redirect URI: {uri}");
        }

        return errors;
    }

    private static bool IsValidClientType(string clientType)
    {
        return clientType switch
        {
            ClientTypes.Web => true,
            ClientTypes.Mobile => true,
            ClientTypes.Desktop => true,
            ClientTypes.Api => true,
            ClientTypes.Service => true,
            _ => false
        };
    }

    private static string GenerateClientId()
    {
        return $"client_{Guid.NewGuid():N}";
    }

    private static string GenerateClientSecret()
    {
        var bytes = new byte[32];
        using var rng = System.Security.Cryptography.RandomNumberGenerator.Create();
        rng.GetBytes(bytes);
        return Convert.ToBase64String(bytes);
    }

    private static ClientInfo MapToClientInfo(OAuthClient client)
    {
        return new ClientInfo
        {
            ClientId = client.ClientId,
            ClientName = client.ClientName,
            ClientType = client.ClientType,
            Description = client.Description,
            RedirectUris = JsonSerializer.Deserialize<List<string>>(client.RedirectUris) ?? new List<string>(),
            PostLogoutRedirectUris = JsonSerializer.Deserialize<List<string>>(client.PostLogoutRedirectUris) ?? new List<string>(),
            Scopes = JsonSerializer.Deserialize<List<string>>(client.Scopes) ?? new List<string>(),
            GrantTypes = JsonSerializer.Deserialize<List<string>>(client.GrantTypes) ?? new List<string>(),
            RequirePkce = client.RequirePkce,
            RequireClientSecret = client.RequireClientSecret,
            ClientUri = client.ClientUri,
            LogoUri = client.LogoUri,
            CreatedAt = client.CreatedAt,
            ExpiresAt = client.ExpiresAt,
            IsActive = client.IsActive,
            CreatedBy = client.CreatedBy
        };
    }
}
