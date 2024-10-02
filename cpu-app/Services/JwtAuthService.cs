using Microsoft.Extensions.DependencyInjection;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using System.Threading.Tasks;
using System.Text;
using Microsoft.IdentityModel.Tokens;
using Serilog;

namespace Gov.Cscp.Victims.Public.Services;

public static class JwtBearerDefaults
{
    public const string AuthenticationScheme = "Bearer";
}

public class JwtAuthorizeAttribute : AuthorizeAttribute
{
    public JwtAuthorizeAttribute() : base()
    {
        AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme;
    }
}

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddJwtAuth(this IServiceCollection services, string secret, string issuer)
    {
        // JWT token authentication for Dynamics to authenticate
        services.AddAuthentication()
            .AddJwtBearer(JwtBearerDefaults.AuthenticationScheme, options =>
            {
                options.IncludeErrorDetails = true;
                options.TokenValidationParameters = new TokenValidationParameters
                {
                    ValidateIssuer = true,
                    ValidateAudience = false,
                    ValidateLifetime = false,
                    ValidateIssuerSigningKey = false,
                    ValidIssuer = issuer,
                    ValidAudience = issuer,
                    IssuerSigningKey = new SymmetricSecurityKey(Encoding.ASCII.GetBytes(secret))
                };
                options.Events = new JwtBearerEvents
                {
                    OnAuthenticationFailed = context =>
                    {
                        Log.Error(context.Exception, "Error validating bearer token");
                        context.Response.StatusCode = StatusCodes.Status401Unauthorized;
                        return Task.CompletedTask;
                    }
                };
            });
        return services;
    }
}
