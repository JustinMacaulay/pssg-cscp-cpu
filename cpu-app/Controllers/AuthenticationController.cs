using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Microsoft.IdentityModel.Tokens;
using System;
using System.IdentityModel.Tokens.Jwt;
using System.Text;
using SymmetricSecurityKey = Microsoft.IdentityModel.Tokens.SymmetricSecurityKey;

namespace Gov.Cscp.Victims.Public.Controllers;

[Route("api/[controller]")]
public class AuthenticationController : Controller
{
    private readonly IConfiguration _configuration;
    private readonly ILogger<AuthenticationController> _logger;

    public AuthenticationController(IConfiguration configuration, ILoggerFactory loggerFactory)
    {
        _configuration = configuration;
        _logger = loggerFactory.CreateLogger<AuthenticationController>();
    }

    // returns a security token used for authenticating subsequent requests using the returned token as a bearer token
    [HttpPost("token")]
    [AllowAnonymous]
    public string GetToken(string secret)
    {
        // check if the JWT token is configured
        string configuredSecret = _configuration["JWT_TOKEN_KEY"];
        string validIssuer = _configuration["JWT_VALID_ISSUER"];
        if (string.IsNullOrEmpty(configuredSecret) || string.IsNullOrEmpty(validIssuer))
        {
            throw new ArgumentNullException("JWT token is not configured.");
        }

        // invalid secret
        if (!configuredSecret.Equals(secret))
        {
            _logger.LogError("Invalid secret passed.");
            throw new Exception("Invalid secret.");
        }

        // valid secret, create secure token
        var secretBytes = Encoding.ASCII.GetBytes(configuredSecret);
        var key = new SymmetricSecurityKey(secretBytes);
        var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);
        var jwtSecurityToken = new JwtSecurityToken(validIssuer, validIssuer, expires: DateTime.UtcNow.AddDays(1), signingCredentials: creds);
        return new JwtSecurityTokenHandler().WriteToken(jwtSecurityToken);
    }
}