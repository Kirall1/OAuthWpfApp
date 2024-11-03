using System.Security.Claims;
using Api.Models;
using Microsoft.AspNetCore;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using OpenIddict.Abstractions;
using OpenIddict.Server.AspNetCore;
using static OpenIddict.Abstractions.OpenIddictConstants;

namespace Api.Controllers;
[Route("connect")]
public class AccountController : ControllerBase
{
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly SignInManager<ApplicationUser> _signInManager;

    public AccountController(UserManager<ApplicationUser> userManager, SignInManager<ApplicationUser> signInManager)
    {
        _userManager = userManager;
        _signInManager = signInManager;
    }

    [HttpPost("register")]
    public async Task<IActionResult> Register([FromBody] RegisterRequest request)
    {
        if (await _userManager.FindByNameAsync(request.Username) != null)
            return BadRequest("User with this username already exists.");

        var user = new ApplicationUser { UserName = request.Username };
        var result = await _userManager.CreateAsync(user, request.Password);

        return result.Succeeded ? Ok("User registered successfully.") : BadRequest("User registration failed.");
    }
    
    [HttpPost("token"), IgnoreAntiforgeryToken, Produces("application/json")]
    public async Task<IActionResult> Login()
    {
        var request = HttpContext.GetOpenIddictServerRequest();
        if (request == null || !request.IsPasswordGrantType())
            return BadRequest(new OpenIddictResponse
            {
                Error = Errors.InvalidRequest,
                ErrorDescription = "The specified grant type is not supported."
            });

        var user = await _userManager.FindByNameAsync(request.Username);
        if (user == null || !await _userManager.CheckPasswordAsync(user, request.Password))
            return Unauthorized(new OpenIddictResponse
            {
                Error = Errors.InvalidGrant,
                ErrorDescription = "The username/password couple is invalid."
            });

        var principal = await _signInManager.CreateUserPrincipalAsync(user);
        
        var identity = principal.Identity as ClaimsIdentity;
        identity.AddClaim(new Claim(Claims.Subject, user.Id));
        
        principal.SetScopes(new[] { Scopes.OpenId, Scopes.Email, Scopes.Profile, Scopes.OfflineAccess });

        return SignIn(principal, OpenIddictServerAspNetCoreDefaults.AuthenticationScheme);
    }

    [HttpPost("refresh"), IgnoreAntiforgeryToken, Produces("application/json")]
    public async Task<IActionResult> RefreshToken()
    {
        var request = HttpContext.GetOpenIddictServerRequest();

        if (request == null || !request.IsRefreshTokenGrantType())
            return BadRequest(new OpenIddictResponse
            {
                Error = Errors.InvalidRequest,
                ErrorDescription = "The specified grant type is not supported."
            });

        var result = await HttpContext.AuthenticateAsync(OpenIddictServerAspNetCoreDefaults.AuthenticationScheme);
        if (!result.Succeeded || result.Principal == null)
        {
            return Unauthorized(new OpenIddictResponse
            {
                Error = Errors.InvalidGrant,
                ErrorDescription = "The refresh token is invalid or has expired."
            });
        }
        
        var principal = result.Principal;
        principal.SetScopes(new[] { Scopes.OpenId, Scopes.Email, Scopes.Profile, Scopes.OfflineAccess });

        return SignIn(principal, OpenIddictServerAspNetCoreDefaults.AuthenticationScheme);
    }
}
