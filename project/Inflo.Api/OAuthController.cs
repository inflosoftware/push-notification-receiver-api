using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;

namespace Inflo.Api;

[ApiController]
[Route("connect")]
public class OAuthController(IConfiguration configuration) : ControllerBase
{
    /// <summary>
    /// Controller is designed to not care about credentials that are provided as to not require too much setup.
    /// In a real world this would have a proper implementation but for example, this demonstrares authentication required endpoints.
    /// </summary>
    /// <param name="request"></param>
    /// <returns></returns>
    [HttpPost("token")]
    public IActionResult Token([FromForm] OAuthTokenRequest request)
    {
        if (!string.Equals(request.GrantType, "client_credentials", StringComparison.Ordinal))
        {
            return BadRequest(new { error = "unsupported_grant_type" });
        }

        var oauth = configuration.GetSection("OAuth");
        var issuer = oauth["Issuer"]!;
        var audience = oauth["Audience"]!;
        var signingKey = oauth["SigningKey"]!;
        var expiry = DateTime.UtcNow.AddHours(1);
        var credentials = new SigningCredentials(
            new SymmetricSecurityKey(Encoding.UTF8.GetBytes(signingKey)),
            SecurityAlgorithms.HmacSha256);
        var descriptor = new SecurityTokenDescriptor
        {
            Issuer = issuer,
            Audience = audience,
            Subject = new ClaimsIdentity([new Claim(ClaimTypes.Name, request.ClientId ?? "development-client")]),
            Expires = expiry,
            SigningCredentials = credentials
        };
        var accessToken = new JwtSecurityTokenHandler().CreateEncodedJwt(descriptor);

        return Ok(new
        {
            access_token = accessToken,
            token_type = "Bearer",
            expires_in = (int)(expiry - DateTime.UtcNow).TotalSeconds
        });
    }
}

public sealed class OAuthTokenRequest
{
    [FromForm(Name = "grant_type")]
    public string? GrantType { get; init; }

    [FromForm(Name = "client_id")]
    public string? ClientId { get; init; }

    [FromForm(Name = "client_secret")]
    public string? ClientSecret { get; init; }
}