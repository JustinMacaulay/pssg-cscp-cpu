using Microsoft.Extensions.DependencyInjection;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authentication;
using System.Text;
using Microsoft.IdentityModel.Tokens;
using System.Configuration;

namespace Gov.Cscp.Victims.Public.Services;

public static class JwtBearerDefaults
{
    public const string AuthenticationScheme = "Bearer";
}

//public static class Policies
//{
//    public const string Dynamics = "dynamics";
//}

//public class DynamicsAuthorizeAttribute : AuthorizeAttribute
//{
//    public DynamicsAuthorizeAttribute() : base(Policies.Dynamics) 
//    {
//        AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme; 
//    }
//}

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddJwtAuth(this IServiceCollection services, string secret)
    {
        // JWT token authentication for Dynamics to authenticate
        var jwtIssuer = "https://localhost:62198";
        services.AddAuthentication(o => o.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme)
            .AddJwtBearer(JwtBearerDefaults.AuthenticationScheme, options =>
            {
                options.IncludeErrorDetails = true;
                options.TokenValidationParameters = new TokenValidationParameters
                {
                    ValidateIssuer = false,
                    ValidateAudience = false,
                    ValidateLifetime = false,
                    ValidateIssuerSigningKey = false,
                    ValidIssuer = jwtIssuer,
                    ValidAudience = jwtIssuer,
                    IssuerSigningKey = new SymmetricSecurityKey(Encoding.ASCII.GetBytes(secret))
                };
                options.Events = new JwtBearerEvents
                {
                    OnTokenValidated = async context => await OnTokenValidatedAsync(context),
                    OnAuthenticationFailed = context =>
                    {
                        //_logger.Error(context.Exception, "Error validating bearer token");
                        context.Response.StatusCode = StatusCodes.Status401Unauthorized;
                        return Task.CompletedTask;
                    }
                };
            });
        //services.AddAuthorization(options => options.AddPolicy(Policies.Dynamics, policy => policy.RequireAuthenticatedUser().AddAuthenticationSchemes(Schemes.Jwt)));       
        services.AddAuthorization(options =>
        {
            options.DefaultPolicy = new AuthorizationPolicyBuilder(JwtBearerDefaults.AuthenticationScheme)
            .RequireAuthenticatedUser().Build();
        });
        return services;
    }

    private static async Task OnTokenValidatedAsync(TokenValidatedContext context)
    {
        // validate the token
        //var token = context.SecurityToken as JwtSecurityToken;
        //if (token == null)
        //{
        //    context.Fail("Unauthorized");
        //    return;
        //}

        //var claims = context.Principal.Claims;
        //var user = claims.FirstOrDefault(c => c.Type == "user");
        //if (user == null)
        //{
        //    context.Fail("Unauthorized");
        //    return;
        //}

        //var userGuid = user.Value;
        //if (string.IsNullOrEmpty(userGuid))
        //{
        //    context.Fail("Unauthorized");
        //    return;
        //}

        //var userService = context.HttpContext.RequestServices.GetRequiredService<IUserService>();
        //var userExists = await userService.UserExists(userGuid);
        //if (!userExists)
        //{
        //    context.Fail("Unauthorized");
        //    return;
        //}
    }
}
