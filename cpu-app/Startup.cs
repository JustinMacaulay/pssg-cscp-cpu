using Gov.Cscp.Victims.Public.Authentication;
using Gov.Cscp.Victims.Public.Authorization;
using Gov.Cscp.Victims.Public.Services;
using Gov.Cscp.Victims.Public.Models;

using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.CookiePolicy;
using Microsoft.AspNetCore.DataProtection;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.Features;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Authorization;
using Microsoft.AspNetCore.SpaServices.AngularCli;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.HealthChecks;
using Microsoft.Extensions.Logging;
using Microsoft.Net.Http.Headers;
using NWebsec.AspNetCore.Mvc;
using NWebsec.AspNetCore.Mvc.Csp;
using Serilog;
using Serilog.Exceptions;
using System;
using System.IO;
using System.Threading.Tasks;
using System.Net.Http;
using Microsoft.Extensions.Hosting;
using Database;
using Gov.Cscp.Victims.Public.Background;
using Manager;

namespace Gov.Cscp.Victims.Public
{
    public class Startup
    {
        public IConfiguration Configuration { get; }
        private IWebHostEnvironment CurrentEnvironment { get; set; }
        public Startup(IConfiguration configuration, IWebHostEnvironment env)
        {
            Configuration = configuration;
            CurrentEnvironment = env;
        }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            services.AddSingleton<IHttpContextAccessor, HttpContextAccessor>();

            services.AddTransient<TokenHandler>();
            services.AddTransient<KeycloakHandler>();

            services.AddHandlers();
            services.AddMediatR(cfg => cfg.RegisterServicesFromAssemblyContaining<InvoiceHandlers>());

            services.AddHttpClient<ICOASTAuthService, COASTAuthService>();
            services.AddHttpClient<IKeycloakAuthService, KeycloakAuthService>();

            services.AddHttpClient<IDynamicsResultService, DynamicsResultService>().AddHttpMessageHandler<TokenHandler>();
            services.AddHttpClient<IDocumentMergeService, DocumentMergeService>().AddHttpMessageHandler<KeycloakHandler>();

            services.AddMemoryCache();

            // for security reasons, the following headers are set.
            services.AddMvc(opts =>
            {
                opts.EnableEndpointRouting = false;
                // default deny
                var policy = new AuthorizationPolicyBuilder()
                 .RequireAuthenticatedUser()
                 .Build();
                opts.Filters.Add(new AuthorizeFilter(policy));

                opts.Filters.Add(typeof(NoCacheHttpHeadersAttribute));
                opts.Filters.Add(new XRobotsTagAttribute() { NoIndex = true, NoFollow = true });
                opts.Filters.Add(typeof(XContentTypeOptionsAttribute));
                opts.Filters.Add(typeof(XDownloadOptionsAttribute));
                opts.Filters.Add(typeof(XFrameOptionsAttribute));
                opts.Filters.Add(typeof(XXssProtectionAttribute));
                //CSPReportOnly
                opts.Filters.Add(typeof(CspReportOnlyAttribute));
                opts.Filters.Add(new CspScriptSrcReportOnlyAttribute { None = true });

                if (CurrentEnvironment.IsDevelopment())
                {
                    opts.Filters.Add(new AllowAnonymousFilter()); // Allow anonymous for dev
                }

            })
            .SetCompatibilityVersion(CompatibilityVersion.Version_3_0)
            .AddNewtonsoftJson(
                opts =>
                {
                    opts.SerializerSettings.Formatting = Newtonsoft.Json.Formatting.Indented;
                    opts.SerializerSettings.DateFormatHandling = Newtonsoft.Json.DateFormatHandling.IsoDateFormat;
                    opts.SerializerSettings.DateTimeZoneHandling = Newtonsoft.Json.DateTimeZoneHandling.Utc;

                    // ReferenceLoopHandling is set to Ignore to prevent JSON parser issues with the user / roles model.
                    opts.SerializerSettings.ReferenceLoopHandling = Newtonsoft.Json.ReferenceLoopHandling.Ignore;

                    //opts.PayloadSerializerOptions.WriteIndented = true;
                });

            services.AddJwtAuth(Configuration["JWT_TOKEN_KEY"], Configuration["JWT_VALID_ISSUER"]);
            services.AddSiteminderAuth();

            services.RegisterPermissionHandler();

            // setup key ring to persist in storage.
            if (!string.IsNullOrEmpty(Configuration["KEY_RING_DIRECTORY"]))
            {
                services.AddDataProtection().PersistKeysToFileSystem(new DirectoryInfo(Configuration["KEY_RING_DIRECTORY"]));
            }

            // In production, the Angular files will be served from this directory
            services.AddSpaStaticFiles(configuration =>
            {
                configuration.RootPath = "ClientApp/dist";
            });

            // add dynamics database adapter
            services.AddDatabase(Configuration);

            services.AddAutoMapper();

            services.AddBackgroundTask();

            // allow for large files to be uploaded
            services.Configure<FormOptions>(options =>
            {
                options.MultipartBodyLengthLimit = 1073741824; // 1 GB
            });

            // health checks
            services.AddHealthChecks(checks =>
            {
                checks.AddValueTaskCheck("HTTP Endpoint", () => new ValueTask<IHealthCheckResult>(HealthCheckResult.Healthy("Ok")));

            });

            services.AddSession(x =>
            {
                x.IdleTimeout = TimeSpan.FromHours(1.0);
            });
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env, ILoggerFactory loggerFactory)
        {
            var log = loggerFactory.CreateLogger("Startup");

            string pathBase = Configuration["BASE_PATH"];

            if (!string.IsNullOrEmpty(pathBase))
            {
                app.UsePathBase(pathBase);
            }
            if (!env.IsProduction())
            {
                app.UseDeveloperExceptionPage();
            }
            else
            {
                app.UseExceptionHandler("/Home/Error");
            }

            app.Use(async (ctx, next) =>
            {
                ctx.Response.Headers.Add("Strict-Transport-Security", "max-age=31536000; includeSubDomains; preload");
                await next();
            });

            app.UseXContentTypeOptions();
            app.UseReferrerPolicy(opts => opts.NoReferrer());
            app.UseXXssProtection(options => options.EnabledWithBlockMode());
            app.UseXfo(options => options.Deny());

            if (!env.IsDevelopment())  // when running locally we can't have a strict CSP
            {
                // Content-Security-Policy header
                app.UseCsp(opts =>
                {
                    opts
                        .BlockAllMixedContent()
                        .StyleSources(s => s.Self().UnsafeInline().CustomSources("https://use.fontawesome.com",
                        "https://stackpath.bootstrapcdn.com",
                        "https://fonts.googleapis.com"))
                        .FontSources(s => s.Self().CustomSources("https://use.fontawesome.com", "https://fonts.gstatic.com"))
                        .FormActions(s => s.Self())
                        .FrameAncestors(s => s.Self())
                        .ImageSources(s => s.Self().CustomSources("data:"))
                        .DefaultSources(s => s.Self())
                        .ObjectSources(s => s.Self().CustomSources("data:"))
                        .FrameSources(s => s.Self().CustomSources("data:"))
                        .ScriptSources(s => s.Self().CustomSources("https://apis.google.com",
                        "https://maxcdn.bootstrapcdn.com",
                        "https://cdnjs.cloudflare.com",
                        "https://code.jquery.com",
                        "https://stackpath.bootstrapcdn.com",
                        "https://fonts.googleapis.com"));

                });
            }

            StaticFileOptions staticFileOptions = new StaticFileOptions
            {
                OnPrepareResponse = ctx =>
                {
                    ctx.Context.Response.Headers[HeaderNames.CacheControl] = "no-cache, no-store, must-revalidate, private";
                    ctx.Context.Response.Headers[HeaderNames.Pragma] = "no-cache";
                    ctx.Context.Response.Headers["X-Frame-Options"] = "SAMEORIGIN";
                    ctx.Context.Response.Headers["X-XSS-Protection"] = "1; mode=block";
                    ctx.Context.Response.Headers["X-Content-Type-Options"] = "nosniff";
                }
            };

            app.UseStaticFiles(staticFileOptions);
            app.UseSpaStaticFiles(staticFileOptions);

            app.UseNoCacheHttpHeaders();
            // IMPORTANT: This session call MUST go before UseMvc()
            app.UseSession();
            app.UseAuthentication();

            app.UseCookiePolicy(new CookiePolicyOptions
            {
                HttpOnly = HttpOnlyPolicy.Always,
                Secure = CookieSecurePolicy.Always,
                MinimumSameSitePolicy = Microsoft.AspNetCore.Http.SameSiteMode.None
            });
            app.UseMvc(routes =>
            {
                routes.MapRoute(
                    name: "default",
                    template: "{controller}/{action=Index}/{id?}");
            });

            if (!string.IsNullOrEmpty(Configuration["SPLUNK_COLLECTOR_URL"]) &&
                !string.IsNullOrEmpty(Configuration["SPLUNK_TOKEN"])
                )
            {

                Serilog.Sinks.Splunk.CustomFields fields = new Serilog.Sinks.Splunk.CustomFields();
                if (!string.IsNullOrEmpty(Configuration["SPLUNK_CHANNEL"]))
                {
                    fields.CustomFieldList.Add(new Serilog.Sinks.Splunk.CustomField("channel", Configuration["SPLUNK_CHANNEL"]));
                }
                var splunkUri = new Uri(Configuration["SPLUNK_COLLECTOR_URL"]);
                var upperSplunkHost = splunkUri.Host?.ToUpperInvariant() ?? string.Empty;

                // Fix for bad SSL issues 


                Log.Logger = new LoggerConfiguration()
                    .Enrich.FromLogContext()
                    .Enrich.WithExceptionDetails()
                    .WriteTo.Console()
                    .WriteTo.EventCollector(splunkHost: Configuration["SPLUNK_COLLECTOR_URL"],
                       sourceType: "portal", eventCollectorToken: Configuration["SPLUNK_TOKEN"],
                       restrictedToMinimumLevel: Serilog.Events.LogEventLevel.Information,
#pragma warning disable CA2000 // Dispose objects before losing scope
                                    messageHandler: new HttpClientHandler()
                                    {
                                        ServerCertificateCustomValidationCallback = (message, cert, chain, errors) => { return true; }
                                    }
#pragma warning restore CA2000 // Dispose objects before losing scope
                                  )
                    .CreateLogger();

                Serilog.Debugging.SelfLog.Enable(Console.Error);

                Log.Logger.Information("CPU Portal Container Started");

            }
            else
            {
                Log.Logger = new LoggerConfiguration()
                    .Enrich.FromLogContext()
                    .Enrich.WithExceptionDetails()
                    .WriteTo.Console()
                    .CreateLogger();
            }

            app.UseSpa(spa =>
            {
                // To learn more about options for serving an Angular SPA from ASP.NET Core, see https://go.microsoft.com/fwlink/?linkid=864501
                spa.Options.SourcePath = "ClientApp";

                // Only run the angular CLI Server in Development mode (not staging or test.)
                if (env.IsDevelopment())
                {
                    spa.UseAngularCliServer(npmScript: "start");
                    spa.Options.StartupTimeout = TimeSpan.FromSeconds(200);
                }
            });
        }
    }
}
