using System.Security.Claims;
using Core.Constants;
using Core.Entities;
using Core.Interfaces;
using Core.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;

namespace API.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize] // Require authentication for all endpoints
[EnableRateLimiting("ApiPolicy")] // Apply API rate limiting
public class OAuthManagementController : ControllerBase
{
    private readonly IOAuthClientService _clientService;
    private readonly IScopeService _scopeService;
    private readonly IConsentService _consentService;
    private readonly IPermissionService _permissionService;
    private readonly IOAuthAuthorizationService _authorizationService;
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly ILogger<OAuthManagementController> _logger;

    public OAuthManagementController(
        IOAuthClientService clientService,
        IScopeService scopeService,
        IConsentService consentService,
        IPermissionService permissionService,
        IOAuthAuthorizationService authorizationService,
        UserManager<ApplicationUser> userManager,
        ILogger<OAuthManagementController> logger)
    {
        _clientService = clientService;
        _scopeService = scopeService;
        _consentService = consentService;
        _permissionService = permissionService;
        _authorizationService = authorizationService;
        _userManager = userManager;
        _logger = logger;
    }

    #region OAuth Client Management

    /// <summary>
    /// Register a new OAuth client
    /// </summary>
    [HttpPost("clients")]
    [Authorize(Roles = Roles.Admin)]
    public async Task<ActionResult<ClientRegistrationResult>> RegisterClient([FromBody] ClientRegistrationModel model)
    {
        try
        {
            var currentUser = await _userManager.GetUserAsync(User);
            if (currentUser == null)
            {
                return Unauthorized();
            }

            var result = await _clientService.RegisterClientAsync(model, currentUser.Id);

            if (result.Success)
            {
                _logger.LogInformation("OAuth client registered: {ClientId} by user {UserId}", result.ClientId, currentUser.Id);
                return Ok(result);
            }

            return BadRequest(result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error registering OAuth client");
            return StatusCode(500, new { message = "An error occurred while registering the client" });
        }
    }

    /// <summary>
    /// Get OAuth client information
    /// </summary>
    [HttpGet("clients/{clientId}")]
    [Authorize(Roles = Roles.Admin)]
    public async Task<ActionResult<ClientInfo>> GetClient(string clientId)
    {
        try
        {
            var client = await _clientService.GetClientAsync(clientId);
            if (client == null)
            {
                return NotFound(new { message = "Client not found" });
            }

            return Ok(client);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting OAuth client {ClientId}", clientId);
            return StatusCode(500, new { message = "An error occurred while retrieving the client" });
        }
    }

    /// <summary>
    /// Get list of OAuth clients
    /// </summary>
    [HttpGet("clients")]
    [Authorize(Roles = Roles.Admin)]
    public async Task<ActionResult<ClientListResponse>> GetClients(
        [FromQuery] int pageNumber = 1,
        [FromQuery] int pageSize = 10,
        [FromQuery] string? createdBy = null)
    {
        try
        {
            var result = await _clientService.GetClientsAsync(pageNumber, pageSize, createdBy);
            return Ok(result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting OAuth clients");
            return StatusCode(500, new { message = "An error occurred while retrieving clients" });
        }
    }

    /// <summary>
    /// Update OAuth client
    /// </summary>
    [HttpPut("clients/{clientId}")]
    [Authorize(Roles = Roles.Admin)]
    public async Task<IActionResult> UpdateClient(string clientId, [FromBody] ClientUpdateModel model)
    {
        try
        {
            var currentUser = await _userManager.GetUserAsync(User);
            if (currentUser == null)
            {
                return Unauthorized();
            }

            model.ClientId = clientId;
            var success = await _clientService.UpdateClientAsync(model, currentUser.Id);

            if (success)
            {
                _logger.LogInformation("OAuth client updated: {ClientId} by user {UserId}", clientId, currentUser.Id);
                return NoContent();
            }

            return NotFound(new { message = "Client not found" });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating OAuth client {ClientId}", clientId);
            return StatusCode(500, new { message = "An error occurred while updating the client" });
        }
    }

    /// <summary>
    /// Delete OAuth client
    /// </summary>
    [HttpDelete("clients/{clientId}")]
    [Authorize(Roles = Roles.Admin)]
    public async Task<IActionResult> DeleteClient(string clientId)
    {
        try
        {
            var currentUser = await _userManager.GetUserAsync(User);
            if (currentUser == null)
            {
                return Unauthorized();
            }

            var success = await _clientService.DeleteClientAsync(clientId, currentUser.Id);

            if (success)
            {
                _logger.LogInformation("OAuth client deleted: {ClientId} by user {UserId}", clientId, currentUser.Id);
                return NoContent();
            }

            return NotFound(new { message = "Client not found" });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting OAuth client {ClientId}", clientId);
            return StatusCode(500, new { message = "An error occurred while deleting the client" });
        }
    }

    #endregion

    #region Scope Management

    /// <summary>
    /// Get available OAuth scopes
    /// </summary>
    [HttpGet("scopes")]
    [Authorize(Roles = Roles.Admin)]
    public async Task<ActionResult<List<ScopeDefinition>>> GetScopes()
    {
        try
        {
            var scopes = await _scopeService.GetAvailableScopesAsync();
            return Ok(scopes);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting OAuth scopes");
            return StatusCode(500, new { message = "An error occurred while retrieving scopes" });
        }
    }

    /// <summary>
    /// Get specific scope information
    /// </summary>
    [HttpGet("scopes/{scopeName}")]
    [Authorize(Roles = Roles.Admin)]
    public async Task<ActionResult<ScopeDefinition>> GetScope(string scopeName)
    {
        try
        {
            var scope = await _scopeService.GetScopeAsync(scopeName);
            if (scope == null)
            {
                return NotFound(new { message = "Scope not found" });
            }

            return Ok(scope);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting OAuth scope {ScopeName}", scopeName);
            return StatusCode(500, new { message = "An error occurred while retrieving the scope" });
        }
    }

    /// <summary>
    /// Create new scope
    /// </summary>
    [HttpPost("scopes")]
    [Authorize(Roles = Roles.Admin)]
    public async Task<IActionResult> CreateScope([FromBody] ScopeDefinition scope)
    {
        try
        {
            var currentUser = await _userManager.GetUserAsync(User);
            if (currentUser == null)
            {
                return Unauthorized();
            }

            var success = await _scopeService.CreateScopeAsync(scope, currentUser.Id);

            if (success)
            {
                _logger.LogInformation("OAuth scope created: {ScopeName} by user {UserId}", scope.Name, currentUser.Id);
                return CreatedAtAction(nameof(GetScope), new { scopeName = scope.Name }, scope);
            }

            return BadRequest(new { message = "Failed to create scope" });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating OAuth scope {ScopeName}", scope.Name);
            return StatusCode(500, new { message = "An error occurred while creating the scope" });
        }
    }

    #endregion

    #region User Consent Management

    /// <summary>
    /// Get user's consent information for a client
    /// </summary>
    [HttpGet("consent")]
    public async Task<ActionResult<ConsentModel>> GetConsent(
        [FromQuery] string clientId,
        [FromQuery] string scopes)
    {
        try
        {
            var currentUser = await _userManager.GetUserAsync(User);
            if (currentUser == null)
            {
                return Unauthorized();
            }

            var requestedScopes = scopes.Split(' ', StringSplitOptions.RemoveEmptyEntries).ToList();
            var consentModel = await _consentService.GetConsentModelAsync(currentUser.Id, clientId, requestedScopes);

            return Ok(consentModel);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting consent for user {UserId}, client {ClientId}", User.Identity?.Name, clientId);
            return StatusCode(500, new { message = "An error occurred while retrieving consent information" });
        }
    }

    /// <summary>
    /// Process user consent
    /// </summary>
    [HttpPost("consent")]
    public async Task<ActionResult<ConsentResponse>> ProcessConsent([FromBody] ConsentRequest request)
    {
        try
        {
            var currentUser = await _userManager.GetUserAsync(User);
            if (currentUser == null)
            {
                return Unauthorized();
            }

            var response = await _consentService.ProcessConsentAsync(
                currentUser.Id,
                request.ClientId,
                request.GrantedScopes,
                request.RememberConsent);

            if (response.IsSuccess)
            {
                _logger.LogInformation("Consent processed for user {UserId}, client {ClientId}", currentUser.Id, request.ClientId);
                return Ok(response);
            }

            return BadRequest(response);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error processing consent for user {UserId}, client {ClientId}", User.Identity?.Name, request.ClientId);
            return StatusCode(500, new { message = "An error occurred while processing consent" });
        }
    }

    /// <summary>
    /// Get user's consent history
    /// </summary>
    [HttpGet("consents")]
    public async Task<ActionResult<List<UserConsent>>> GetUserConsents()
    {
        try
        {
            var currentUser = await _userManager.GetUserAsync(User);
            if (currentUser == null)
            {
                return Unauthorized();
            }

            var consents = await _consentService.GetUserConsentsAsync(currentUser.Id);
            return Ok(consents);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting consents for user {UserId}", User.Identity?.Name);
            return StatusCode(500, new { message = "An error occurred while retrieving consents" });
        }
    }

    /// <summary>
    /// Revoke consent for a specific client
    /// </summary>
    [HttpDelete("consent/{clientId}")]
    public async Task<IActionResult> RevokeConsent(string clientId)
    {
        try
        {
            var currentUser = await _userManager.GetUserAsync(User);
            if (currentUser == null)
            {
                return Unauthorized();
            }

            var success = await _consentService.RevokeConsentAsync(currentUser.Id, clientId, "User requested revocation");

            if (success)
            {
                _logger.LogInformation("Consent revoked for user {UserId}, client {ClientId}", currentUser.Id, clientId);
                return NoContent();
            }

            return NotFound(new { message = "Consent not found" });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error revoking consent for user {UserId}, client {ClientId}", User.Identity?.Name, clientId);
            return StatusCode(500, new { message = "An error occurred while revoking consent" });
        }
    }

    #endregion

    #region Permission Management

    /// <summary>
    /// Get user's permissions
    /// </summary>
    [HttpGet("permissions")]
    public async Task<ActionResult<UserPermissionModel>> GetUserPermissions([FromQuery] string? userId = null)
    {
        try
        {
            var currentUser = await _userManager.GetUserAsync(User);
            if (currentUser == null)
            {
                return Unauthorized();
            }

            // If userId is specified and user is not admin, only allow viewing own permissions
            var targetUserId = userId ?? currentUser.Id;
            if (userId != null && userId != currentUser.Id)
            {
                var isAdmin = await _userManager.IsInRoleAsync(currentUser, Roles.Admin);
                if (!isAdmin)
                {
                    return Forbid();
                }
            }

            var permissions = await _permissionService.GetUserPermissionsAsync(targetUserId);
            return Ok(permissions);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting permissions for user {UserId}", userId ?? User.Identity?.Name);
            return StatusCode(500, new { message = "An error occurred while retrieving permissions" });
        }
    }

    /// <summary>
    /// Grant permissions to a user (Admin only)
    /// </summary>
    [HttpPost("permissions/grant")]
    [Authorize(Roles = Roles.Admin)]
    public async Task<IActionResult> GrantPermissions([FromBody] PermissionAssignmentModel model)
    {
        try
        {
            var currentUser = await _userManager.GetUserAsync(User);
            if (currentUser == null)
            {
                return Unauthorized();
            }

            var success = await _permissionService.GrantPermissionsAsync(model, currentUser.Id);

            if (success)
            {
                _logger.LogInformation("Permissions granted to user {UserId} by admin {AdminId}", model.UserId, currentUser.Id);
                return NoContent();
            }

            return BadRequest(new { message = "Failed to grant permissions" });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error granting permissions to user {UserId}", model.UserId);
            return StatusCode(500, new { message = "An error occurred while granting permissions" });
        }
    }

    /// <summary>
    /// Revoke specific permission from a user (Admin only)
    /// </summary>
    [HttpDelete("permissions/{userId}/{permission}")]
    [Authorize(Roles = Roles.Admin)]
    public async Task<IActionResult> RevokePermission(string userId, string permission)
    {
        try
        {
            var currentUser = await _userManager.GetUserAsync(User);
            if (currentUser == null)
            {
                return Unauthorized();
            }

            var success = await _permissionService.RevokePermissionAsync(userId, permission, currentUser.Id, "Admin revocation");

            if (success)
            {
                _logger.LogInformation("Permission {Permission} revoked from user {UserId} by admin {AdminId}", permission, userId, currentUser.Id);
                return NoContent();
            }

            return NotFound(new { message = "Permission not found" });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error revoking permission {Permission} from user {UserId}", permission, userId);
            return StatusCode(500, new { message = "An error occurred while revoking permission" });
        }
    }

    #endregion

    #region Authorization Endpoints

    /// <summary>
    /// Validate authorization request
    /// </summary>
    [HttpPost("validate")]
    [AllowAnonymous]
    public async Task<ActionResult<AuthorizeRequest>> ValidateAuthorizeRequest([FromBody] AuthorizeRequest request)
    {
        try
        {
            var validatedRequest = await _authorizationService.ValidateAuthorizeRequestAsync(request);
            return Ok(validatedRequest);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error validating authorize request for client {ClientId}", request.ClientId);
            return StatusCode(500, new { message = "An error occurred while validating the request" });
        }
    }

    /// <summary>
    /// Get effective permissions for user with given scopes
    /// </summary>
    [HttpGet("effective-permissions")]
    public async Task<ActionResult<List<string>>> GetEffectivePermissions([FromQuery] string scopes)
    {
        try
        {
            var currentUser = await _userManager.GetUserAsync(User);
            if (currentUser == null)
            {
                return Unauthorized();
            }

            var scopeList = scopes.Split(' ', StringSplitOptions.RemoveEmptyEntries).ToList();
            var permissions = await _authorizationService.GetEffectivePermissionsAsync(currentUser.Id, scopeList);

            return Ok(permissions);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting effective permissions for user {UserId}", User.Identity?.Name);
            return StatusCode(500, new { message = "An error occurred while retrieving permissions" });
        }
    }

    #endregion
}

// Request/Response models for this controller
public class ConsentRequest
{
    public string ClientId { get; set; } = string.Empty;
    public List<string> GrantedScopes { get; set; } = new();
    public bool RememberConsent { get; set; }
}
