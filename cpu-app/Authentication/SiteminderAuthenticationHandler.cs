using Gov.Cscp.Victims.Public.Models;
using Gov.Cscp.Victims.Public.Utils;
using Gov.Cscp.Victims.Public.Services;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http.Extensions;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Serilog;
using System.Security.Claims;
using System.Text.Encodings.Web;
using System.Threading.Tasks;
using System;
using Microsoft.Extensions.DependencyInjection;

namespace Gov.Cscp.Victims.Public.Authentication
{
    #region SiteMinder Authentication Options
    /// <summary>
    /// Options required for setting up SiteMinder Authentication
    /// </summary>
    public class SiteMinderAuthOptions : AuthenticationSchemeOptions
    {
        // note that header keys are case insensitive, thus the reason why the keys are all lower case.

        private const string ConstDevAuthenticationTokenKey = "DEV-USER";
        private const string ConstDevBCSCAuthenticationTokenKey = "DEV-BCSC-USER";
        private const string ConstDevDefaultUserId = "TMcTesterson";
        private const string ConstSiteMinderUserGuidKey = "smgov_userguid"; //deprecated -- smgov_useridentifier
        private const string ConstSiteMinderUserIdentifierKey = "smgov_useridentifier";
        private const string ConstSiteMinderUniversalIdKey = "sm_universalid";
        private const string ConstSiteMinderUserNameKey = "sm_user";

        //BCeId Values
        private const string ConstSiteMinderBusinessGuidKey = "smgov_businessguid";
        private const string ConstSiteMinderBusinessLegalNameKey = "smgov_businesslegalname";

        //BC Services Card
        private const string ConstSiteMinderBirthDate = "smgov_birthdate";

        //BCeID or BC Services Card
        private const string ConstSiteMinderUserType = "smgov_usertype"; // get the type values from siteminder header this will be bceid or bcservices

        private const string ConstSiteMinderUserDisplayNameKey = "smgov_userdisplayname";

        private const string ConstMissingSiteMinderUserIdError = "Missing SiteMinder UserId";
        private const string ConstMissingSiteMinderGuidError = "Missing SiteMinder Guid";
        private const string ConstMissingSiteMinderUserTypeError = "Missing SiteMinder User Type";
        private const string ConstMissingDbUserIdError = "Could not find UserId in the database";
        private const string ConstUnderageError = "You must be 19 years of age or older to access this website.";

        private const string ConstInactivegDbUserIdError = "Database UserId is inactive";
        private const string ConstInvalidPermissions = "UserId does not have valid permissions";

        /// <summary>
        /// DEfault Constructor
        /// </summary>
        public SiteMinderAuthOptions()
        {
            SiteMinderBusinessGuidKey = ConstSiteMinderBusinessGuidKey;
            SiteMinderUserGuidKey = ConstSiteMinderUserGuidKey;
            SiteMinderUserIdentifierKey = ConstSiteMinderUserIdentifierKey;
            SiteMinderUniversalIdKey = ConstSiteMinderUniversalIdKey;
            SiteMinderUserNameKey = ConstSiteMinderUserNameKey;
            SiteMinderUserDisplayNameKey = ConstSiteMinderUserDisplayNameKey;
            SiteMinderUserTypeKey = ConstSiteMinderUserType;
            SiteMinderBirthDate = ConstSiteMinderUserType;
            MissingSiteMinderUserIdError = ConstMissingSiteMinderUserIdError;
            MissingSiteMinderUserTypeError = ConstMissingSiteMinderUserIdError;
            MissingSiteMinderGuidError = ConstMissingSiteMinderGuidError;
            MissingDbUserIdError = ConstMissingDbUserIdError;
            InactivegDbUserIdError = ConstInactivegDbUserIdError;
            InvalidPermissions = ConstInvalidPermissions;
            DevAuthenticationTokenKey = ConstDevAuthenticationTokenKey;
            DevBCSCAuthenticationTokenKey = ConstDevBCSCAuthenticationTokenKey;
            DevDefaultUserId = ConstDevDefaultUserId;
            UnderageError = ConstUnderageError;
        }

        /// <summary>
        /// Default Scheme Name
        /// </summary>
        public static string AuthenticationSchemeName => "site-minder-auth";

        /// <summary>
        /// SiteMinder Authentication Scheme Name
        /// </summary>
        public string Scheme => AuthenticationSchemeName;

        public string SiteMinderBusinessGuidKey { get; set; }

        /// <summary>
        /// User GUID
        /// </summary>
        public string SiteMinderUserGuidKey { get; set; }

        /// <summary>
        /// User Identifier
        /// </summary>
        public string SiteMinderUserIdentifierKey { get; set; }

        /// <summary>
        /// User Id
        /// </summary>
        public string SiteMinderUniversalIdKey { get; set; }

        /// <summary>
        /// User Name
        /// </summary>
        public string SiteMinderUserNameKey { get; set; }

        /// <summary>
        /// User's Display Name
        /// </summary>
        public string SiteMinderUserDisplayNameKey { get; set; }

        ///<summary>
        ///User's Type (BCeID or BC services card)
        /// </summary>
        public string SiteMinderUserTypeKey { get; set; }

        /// <summary>
        /// BC Service Card - Birth Date field.
        /// </summary>
        public string SiteMinderBirthDate { get; set; }

        /// <summary>
        /// Missing SiteMinder User Type Error
        /// </summary>
        public string MissingSiteMinderUserTypeError { get; set; }

        /// <summary>
        /// Missing SiteMinder UserId Error
        /// </summary>
        public string MissingSiteMinderUserIdError { get; set; }

        /// <summary>
        /// Missing SiteMinder Guid Error
        /// </summary>
        public string MissingSiteMinderGuidError { get; set; }

        /// <summary>
        /// Missing Database UserId Error
        /// </summary>
        public string MissingDbUserIdError { get; set; }

        /// <summary>
        /// Inactive Database UserId Error
        /// </summary>
        public string InactivegDbUserIdError { get; set; }

        /// <summary>
        /// Inactive Database UserId Error
        /// </summary>
        public string UnderageError { get; set; }

        /// <summary>
        /// User does not jave active / valid permissions
        /// </summary>
        public string InvalidPermissions { get; set; }

        /// <summary>
        /// Development Environment Authentication Key
        /// </summary>
        public string DevAuthenticationTokenKey { get; set; }

        /// <summary>
        /// Development Environment Authentication Key
        /// </summary>
        public string DevBCSCAuthenticationTokenKey { get; set; }

        /// <summary>
        /// Development Environment efault UserId
        /// </summary>
        public string DevDefaultUserId { get; set; }
    }
    #endregion    

    /// <summary>
    /// Setup Siteminder Authentication Handler
    /// </summary>
    public static class SiteminderAuthenticationExtensions
    {
        public static IServiceCollection AddSiteminderAuth(this IServiceCollection services)
        {
            // siteminder authentication (core 2.0)
            services
                .AddAuthentication(options =>
                {
                    options.DefaultAuthenticateScheme = SiteMinderAuthOptions.AuthenticationSchemeName;
                    options.DefaultChallengeScheme = SiteMinderAuthOptions.AuthenticationSchemeName;
                })
                .AddSiteminderAuth(options => { });
            services.AddAuthorization(options =>
            {
                options.AddPolicy("Business-User", policy => policy.RequireClaim(User.UserTypeClaim, "Business"));
            });
            return services;
        }

        /// <summary>
        /// Add Authentication Handler
        /// </summary>
        /// <param name="builder"></param>
        /// <param name="configureOptions"></param>
        /// <returns></returns>
        public static AuthenticationBuilder AddSiteminderAuth(this AuthenticationBuilder builder, Action<SiteMinderAuthOptions> configureOptions)
        {
            return builder.AddScheme<SiteMinderAuthOptions, SiteminderAuthenticationHandler>(SiteMinderAuthOptions.AuthenticationSchemeName, configureOptions);
        }
    }

    /// <summary>
    /// Siteminder Authentication Handler
    /// </summary>
    public class SiteminderAuthenticationHandler : AuthenticationHandler<SiteMinderAuthOptions>
    {
        private readonly Microsoft.Extensions.Logging.ILogger _logger;
        private readonly Serilog.ILogger _splunkLogger;
        private IDynamicsResultService _dynamicsResultService;

        /// <summary>
        /// Siteminder Authentication Constructir
        /// </summary>
        /// <param name="configureOptions"></param>
        /// <param name="loggerFactory"></param>
        /// <param name="encoder"></param>
        /// <param name="clock"></param>
        public SiteminderAuthenticationHandler(
            IOptionsMonitor<SiteMinderAuthOptions> configureOptions,
            ILoggerFactory loggerFactory,
            UrlEncoder encoder,
            ISystemClock clock,
            IDynamicsResultService dynamicsResultService
            ) : base(configureOptions, loggerFactory, encoder, clock)
        {
            _logger = loggerFactory.CreateLogger(typeof(SiteminderAuthenticationHandler));
            _dynamicsResultService = dynamicsResultService;
            _splunkLogger = Log.Logger;
        }

        /// <summary>
        /// Process Authentication Request
        /// </summary>
        /// <returns></returns>
        protected override async Task<AuthenticateResult> HandleAuthenticateAsync()
        {
            //         // get siteminder headers
            _logger.LogDebug("Parsing the HTTP headers for SiteMinder authentication credential");
            _logger.LogInformation("Test information");
            _logger.LogTrace("Test Trace");

            // Console.WriteLine("Parsing the HTTP headers for SiteMinder authentication credential");

            SiteMinderAuthOptions options = new SiteMinderAuthOptions();
            bool isDeveloperLogin = false;
            bool isBCSCDeveloperLogin = false;
            try
            {
                ClaimsPrincipal principal;
                HttpContext context = Request.HttpContext;
                IWebHostEnvironment hostingEnv = (IWebHostEnvironment)context.RequestServices.GetService(typeof(IWebHostEnvironment));

                UserSettings userSettings = new UserSettings();

                string userId = null;
                string devCompanyId = null;
                string siteMinderGuid = "";
                string siteMinderBusinessGuid = "";
                string siteMinderUserType = "";

                // **************************************************
                // If this is an Error or Authentiation API - Ignore
                // **************************************************
                string url = context.Request.GetDisplayUrl().ToLower();
                if (url.Contains(".js"))
                {
                    _splunkLogger.Error(new Exception("url contains .js"), "Unexpected error while in siteminder auth handler - url contains .js - returning NoResult(). Source = CPU");
                    return AuthenticateResult.NoResult();
                }


                // **************************************************
                // Check if the user session is already created
                // **************************************************
                try
                {
                    _logger.LogInformation("Checking user session");
                    userSettings = UserSettings.ReadUserSettings(context);
                    _logger.LogDebug("UserSettings found: " + userSettings.GetJson());
                    // Console.WriteLine("UserSettings found: " + userSettings.GetJson());
                }
                catch
                {
                    //do nothing
                    // Console.WriteLine("No UserSettings found");
                    _logger.LogDebug("No UserSettings found");
                }

                // is user authenticated - if so we're done
                if ((userSettings.UserAuthenticated && string.IsNullOrEmpty(userId)) ||
                        (userSettings.UserAuthenticated && !string.IsNullOrEmpty(userId) &&
                         !string.IsNullOrEmpty(userSettings.UserId) && userSettings.UserId == userId))
                {
                    _logger.LogDebug("User already authenticated with active session: " + userSettings.UserId);
                    // Console.WriteLine("User already authenticated with active session: " + userSettings.GetJson());
                    principal = userSettings.AuthenticatedUser.ToClaimsPrincipal(options.Scheme, userSettings.UserType);
                    return AuthenticateResult.Success(new AuthenticationTicket(principal, null, Options.Scheme));
                }

                string smgov_userdisplayname = context.Request.Headers["smgov_userdisplayname"];
                if (!string.IsNullOrEmpty(smgov_userdisplayname))
                {
                    userSettings.UserDisplayName = smgov_userdisplayname;
                }

                string smgov_businesslegalname = context.Request.Headers["smgov_businesslegalname"];
                if (!string.IsNullOrEmpty(smgov_businesslegalname))
                {
                    userSettings.BusinessLegalName = smgov_businesslegalname;
                }

                // **************************************************
                // Authenticate based on SiteMinder Headers
                // **************************************************

                // TODO userId is always null
                if (string.IsNullOrEmpty(userId))
                {
                    _logger.LogDebug("Getting user data from headers");
                    // Console.WriteLine("Getting user data from headers");

                    userId = context.Request.Headers[options.SiteMinderUserNameKey];
                    if (string.IsNullOrEmpty(userId))
                    {
                        userId = context.Request.Headers[options.SiteMinderUniversalIdKey];
                    }

                    siteMinderGuid = context.Request.Headers[options.SiteMinderUserGuidKey];
                    siteMinderBusinessGuid = context.Request.Headers[options.SiteMinderBusinessGuidKey];
                    siteMinderUserType = context.Request.Headers[options.SiteMinderUserTypeKey];


                    // **************************************************
                    // Validate credentials
                    // **************************************************
                    if (string.IsNullOrEmpty(userId))
                    {
                        _logger.LogDebug(options.MissingSiteMinderUserIdError);
                        return AuthenticateResult.Fail(options.MissingSiteMinderGuidError);
                    }

                    if (string.IsNullOrEmpty(siteMinderGuid))
                    {
                        _logger.LogDebug(options.MissingSiteMinderGuidError);
                        return AuthenticateResult.Fail(options.MissingSiteMinderGuidError);
                    }
                    if (string.IsNullOrEmpty(siteMinderUserType))
                    {
                        _logger.LogDebug(options.MissingSiteMinderUserTypeError);
                        return AuthenticateResult.Fail(options.MissingSiteMinderUserTypeError);
                    }
                }
                else // DEV user, setup a fake session and SiteMinder headers.
                {
                    if (isDeveloperLogin && _dynamicsResultService != null)
                    {
                        _logger.LogError("Generating a Development user");
                        userSettings.BusinessLegalName = devCompanyId + " BusinessProfileName";
                        userSettings.UserDisplayName = userId + " BCeIDContactType";

                        siteMinderGuid = GuidUtility.CreateIdForDynamics("contact", userSettings.UserDisplayName).ToString();
                        siteMinderBusinessGuid = GuidUtility.CreateIdForDynamics("account", userSettings.BusinessLegalName).ToString();
                        siteMinderUserType = "Business";
                    }
                    else if (isBCSCDeveloperLogin)
                    {
                        _logger.LogError("Generating a Development BC Services user");
                        userSettings.BusinessLegalName = null;
                        userSettings.UserDisplayName = userId + " Associate";
                        siteMinderGuid = GuidUtility.CreateIdForDynamics("bcsc", userSettings.UserDisplayName).ToString();
                        siteMinderBusinessGuid = null;
                        siteMinderUserType = "VerifiedIndividual";
                    }
                }

                if (_dynamicsResultService != null)
                {
                    // Console.WriteLine("We're \"Logged in\", businessBCeID: " + siteMinderBusinessGuid + ", UserBCeID: " + siteMinderGuid);
                    _logger.LogDebug("We're \"Logged in\", businessBCeID: " + siteMinderBusinessGuid + ", UserBCeID: " + siteMinderGuid);

                    userSettings.AuthenticatedUser = new User(new Guid(siteMinderGuid), "Bill", "Octoroc", true, "BO", "octoroc@foo.gov", siteMinderGuid, siteMinderBusinessGuid, siteMinderUserType, null);
                    userSettings.UserAuthenticated = true;

                    string requestJson = "{\"UserBCeID\":\"" + siteMinderGuid + "\",\"BusinessBCeID\":\"" + siteMinderBusinessGuid + "\"}";
                    // set the endpoint action
                    string endpointUrl = "vsd_GetCPUOrgContracts";
                    HttpClientResult result = await _dynamicsResultService.Post(endpointUrl, requestJson);
                    // Console.WriteLine("Dynamics result info:");
                    string resultString = result.ToString();
                    string resultResult = result.result.ToString();
                    string messageString = result.responseMessage.ToString();
                    int code = (int)result.statusCode;

                    // Console.WriteLine("resultResult");
                    // Console.WriteLine(resultResult);
                    // Console.WriteLine("messageString");
                    // Console.WriteLine(messageString);

                    userSettings.UserType = siteMinderUserType;
                    userSettings.UserId = siteMinderGuid;
                    userSettings.AccountId = siteMinderBusinessGuid;

                    //Error: No contact found with the supplied BCeID

                    string NEW_USER = "No contact found with the supplied BCeID";
                    string NEW_USER_AND_NEW_ORGANIZATION = "No organization and contact found with the supplied BCeID";
                    string CONTACT_NOT_APPROVED = "Contact is not approved for portal access";
                    string CONTACT_NOT_CPU = "Contact doesn't belong to CPU";
                    string NO_ROLES_ASSIGNED = "No roles assigned to the contact";

                    if (resultResult.Contains(NEW_USER))
                    {
                        // Console.WriteLine("New User Registration");

                        userSettings.IsNewUserRegistration = true;
                        userSettings.IsNewUserAndNewOrganizationRegistration = false;
                        principal = userSettings.AuthenticatedUser.ToClaimsPrincipal(options.Scheme, userSettings.UserType);
                        UserSettings.SaveUserSettings(userSettings, context);
                        return AuthenticateResult.Success(new AuthenticationTicket(principal, null, Options.Scheme));
                    }
                    else if (resultResult.Contains(NEW_USER_AND_NEW_ORGANIZATION))
                    {
                        // Console.WriteLine("New User and New Organization Registration");

                        userSettings.IsNewUserRegistration = false;
                        userSettings.IsNewUserAndNewOrganizationRegistration = true;
                        principal = userSettings.AuthenticatedUser.ToClaimsPrincipal(options.Scheme, userSettings.UserType);
                        UserSettings.SaveUserSettings(userSettings, context);
                        return AuthenticateResult.Success(new AuthenticationTicket(principal, null, Options.Scheme));
                    }
                    else if (resultResult.Contains(CONTACT_NOT_APPROVED))
                    {
                        //error state - should hopefully never happen
                        // Console.WriteLine("Error, contact already exists but is not approved");

                        userSettings.ContactExistsButNotApproved = true;
                        principal = userSettings.AuthenticatedUser.ToClaimsPrincipal(options.Scheme, userSettings.UserType);
                        UserSettings.SaveUserSettings(userSettings, context);
                        return AuthenticateResult.Success(new AuthenticationTicket(principal, null, Options.Scheme));
                    }
                    else if (resultResult.Contains(NO_ROLES_ASSIGNED))
                    {
                        // Console.WriteLine("New User and New Organization Registration");

                        userSettings.NoRolesAssigned = true;
                        principal = userSettings.AuthenticatedUser.ToClaimsPrincipal(options.Scheme, userSettings.UserType);
                        UserSettings.SaveUserSettings(userSettings, context);
                        return AuthenticateResult.Success(new AuthenticationTicket(principal, null, Options.Scheme));
                    }
                    else if (resultResult.Contains(CONTACT_NOT_CPU))
                    {
                        //error state - should hopefully never happen
                        // Console.WriteLine("Error, contact does not belong to CPU...");

                        userSettings.ContactExistsButNotApproved = true;
                        principal = userSettings.AuthenticatedUser.ToClaimsPrincipal(options.Scheme, userSettings.UserType);
                        UserSettings.SaveUserSettings(userSettings, context);
                        return AuthenticateResult.Success(new AuthenticationTicket(principal, null, Options.Scheme));
                    }
                    else
                    {
                        //TODO - should verify we did in fact get a success response

                        // Console.WriteLine("Found User Data");
                        // Console.WriteLine("We're \"Logged in\", businessBCeID: " + siteMinderBusinessGuid + ", UserBCeID: " + siteMinderGuid);
                        userSettings.IsNewUserRegistration = false;
                        userSettings.IsNewUserAndNewOrganizationRegistration = false;
                        // Console.WriteLine("UserSettings: " + userSettings.GetJson());
                        principal = userSettings.AuthenticatedUser.ToClaimsPrincipal(options.Scheme, userSettings.UserType);
                        UserSettings.SaveUserSettings(userSettings, context);

                        return AuthenticateResult.Success(new AuthenticationTicket(principal, null, Options.Scheme));
                    }
                }
                else {
                    // Console.WriteLine("No DynamicsResultService configured.");
                    _splunkLogger.Error(new Exception("No DynamicsResultService configured."), "Unexpected error while in siteminder auth handler - No DynamicsResultService configured. Source = CPU");
                    return AuthenticateResult.Fail("No DynamicsResultService configured");
                }
            }
            catch (Exception exception)
            {
                _splunkLogger.Error(exception, "Unexpected error while in siteminder auth handler. Source = CPU");
                _logger.LogError(exception.Message);
                Console.WriteLine("Hit exception in siteminder auth handler.");
                Console.WriteLine(exception);
                throw;
            }
            // return AuthenticateResult.NoResult();
        }
    }
}
