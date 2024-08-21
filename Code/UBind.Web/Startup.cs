// <copyright file="Startup.cs" company="uBind">
// Copyright (c) uBind. All rights reserved.
// </copyright>

// If you edit this file, you must remove this line and then do proper null checking
#pragma warning disable CS8600, CS8625, CS8629, CS8618, CS8605, CS8604, CS8601, CS8602, CS8603, CS8622, CS8619, CS8767, CS8620, CS8765

namespace UBind.Web;

using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Globalization;
using System.IO;
using System.IO.Abstractions;
using System.Linq;
using System.Net;
using System.Runtime;
using System.Security.Cryptography.X509Certificates;
using ComponentSpace.Saml2.Configuration.Resolver;
using Dapper;
using DotLiquid;
using DotLiquid.NamingConventions;
using GemBox.Document;
using Hangfire;
using Hangfire.Console;
using Hangfire.Console.Extensions;
using Hangfire.Console.Extensions.Serilog;
using Hangfire.Dashboard;
using Hangfire.Pro.Redis;
using Hangfire.Storage;
using Hellang.Middleware.ProblemDetails;
using Humanizer;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Diagnostics.HealthChecks;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Localization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Infrastructure;
using Microsoft.AspNetCore.Mvc.Routing;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using Microsoft.Extensions.FileProviders;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using Microsoft.Net.Http.Headers;
using Microsoft.OpenApi.Models;
using NetTools;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using NodaTime;
using NodaTime.Serialization.JsonNet;
using Quartz;
using Quartz.Impl;
using Quartz.Spi;
using RazorEngine.Templating;
using Sentry.AspNetCore;
using Serilog;
using Serilog.Events;
using StackExchange.Profiling.EntityFramework6;
using StackExchange.Profiling.Storage;
using StackExchange.Redis;
using Stripe;
using UBind.Application;
using UBind.Application.Authentication;
using UBind.Application.Authorisation;
using UBind.Application.Automation;
using UBind.Application.Automation.Actions;
using UBind.Application.Automation.DocumentAttacher;
using UBind.Application.Automation.Providers.Entity;
using UBind.Application.Automation.Providers.Liquid;
using UBind.Application.Automation.Providers.List;
using UBind.Application.Automation.Triggers;
using UBind.Application.Configuration;
using UBind.Application.CustomPipelines;
using UBind.Application.Dashboard;
using UBind.Application.Dashboard.Model;
using UBind.Application.DataDownloader;
using UBind.Application.Export;
using UBind.Application.ExtensionMethods;
using UBind.Application.FileHandling;
using UBind.Application.FileHandling.GemBoxServices;
using UBind.Application.FlexCel;
using UBind.Application.Funding;
using UBind.Application.Funding.EFundExpress;
using UBind.Application.Funding.Iqumulate;
using UBind.Application.Funding.PremiumFunding;
using UBind.Application.Helpers;
using UBind.Application.Infrastructure;
using UBind.Application.MicrosoftGraph;
using UBind.Application.Payment;
using UBind.Application.Payment.Deft;
using UBind.Application.Person;
using UBind.Application.Product.Component.Configuration.Parsers;
using UBind.Application.Queries.Services;
using UBind.Application.Quote;
using UBind.Application.Releases;
using UBind.Application.Report;
using UBind.Application.Scheduler;
using UBind.Application.Scheduler.Jobs;
using UBind.Application.Services;
using UBind.Application.Services.DelimiterSeparatedValues;
using UBind.Application.Services.Email;
using UBind.Application.Services.Encryption;
using UBind.Application.Services.HangfireCqrs;
using UBind.Application.Services.Import;
using UBind.Application.Services.Imports;
using UBind.Application.Services.MachineInformation;
using UBind.Application.Services.Maintenance;
using UBind.Application.Services.Messaging;
using UBind.Application.Services.PolicyDataPatcher;
using UBind.Application.Services.Search;
using UBind.Application.Services.SystemEmail;
using UBind.Application.Sms;
using UBind.Application.Sms.Clickatell;
using UBind.Application.StartupJobs;
using UBind.Application.SystemEvents;
using UBind.Application.ThirdPartyDataSets;
using UBind.Application.ThirdPartyDataSets.Query.QueryFactories.Gnaf;
using UBind.Application.User;
using UBind.Domain;
using UBind.Domain.AbnLookup;
using UBind.Domain.Accounting;
using UBind.Domain.Aggregates;
using UBind.Domain.Aggregates.Accounting;
using UBind.Domain.Aggregates.AdditionalPropertyDefinition;
using UBind.Domain.Aggregates.AdditionalPropertyValue;
using UBind.Domain.Aggregates.Claim;
using UBind.Domain.Aggregates.Claim.Workflow;
using UBind.Domain.Aggregates.Customer;
using UBind.Domain.Aggregates.Organisation;
using UBind.Domain.Aggregates.Person;
using UBind.Domain.Aggregates.Portal;
using UBind.Domain.Aggregates.Quote;
using UBind.Domain.Aggregates.Quote.Workflow;
using UBind.Domain.Aggregates.Report;
using UBind.Domain.Aggregates.User;
using UBind.Domain.Attributes;
using UBind.Domain.Authentication;
using UBind.Domain.Configuration;
using UBind.Domain.Entities;
using UBind.Domain.Enums;
using UBind.Domain.Events;
using UBind.Domain.Exceptions;
using UBind.Domain.Extensions;
using UBind.Domain.Helpers;
using UBind.Domain.Loggers;
using UBind.Domain.Models.DataTable;
using UBind.Domain.NumberGenerators;
using UBind.Domain.Patterns.Cqrs;
using UBind.Domain.Permissions;
using UBind.Domain.Processing;
using UBind.Domain.Product;
using UBind.Domain.Product.Component;
using UBind.Domain.ReadModel;
using UBind.Domain.ReadModel.Accounting;
using UBind.Domain.ReadModel.Claim;
using UBind.Domain.ReadModel.Customer;
using UBind.Domain.ReadModel.Organisation;
using UBind.Domain.ReadModel.Person.Fields;
using UBind.Domain.ReadModel.Policy;
using UBind.Domain.ReadModel.Portal;
using UBind.Domain.ReadModel.User;
using UBind.Domain.Reduction;
using UBind.Domain.ReferenceNumbers;
using UBind.Domain.Repositories;
using UBind.Domain.Repositories.FileSystem;
using UBind.Domain.Repositories.Redis;
using UBind.Domain.Search;
using UBind.Domain.Search.ThirdPartyDataSets;
using UBind.Domain.Services;
using UBind.Domain.Services.AdditionalPropertyDefinition;
using UBind.Domain.Services.AdditionalPropertyValue;
using UBind.Domain.Services.Maintenance;
using UBind.Domain.Services.Migration;
using UBind.Domain.Services.Pricing;
using UBind.Domain.Services.QuoteExpiry;
using UBind.Domain.Services.SystemEvents;
using UBind.Domain.ThirdPartyDataSets;
using UBind.Persistence;
using UBind.Persistence.Aggregates;
using UBind.Persistence.Clients.DVA.Migrations;
using UBind.Persistence.Configuration;
using UBind.Persistence.DataTable;
using UBind.Persistence.Entities;
using UBind.Persistence.Filesystem;
using UBind.Persistence.Helpers;
using UBind.Persistence.Infrastructure;
using UBind.Persistence.Migrations;
using UBind.Persistence.ReadModels;
using UBind.Persistence.ReadModels.Claim;
using UBind.Persistence.ReadModels.Organisation;
using UBind.Persistence.ReadModels.Portal;
using UBind.Persistence.ReadModels.Quote;
using UBind.Persistence.ReadModels.Report;
using UBind.Persistence.ReadModels.User;
using UBind.Persistence.Redis.Repositories;
using UBind.Persistence.Reduction;
using UBind.Persistence.Repositories;
using UBind.Persistence.Search;
using UBind.Persistence.Search.ThirdPartyDataSets;
using UBind.Persistence.Services;
using UBind.Persistence.Services.Filesystem;
using UBind.Persistence.ThirdPartyDataSets;
using UBind.Web.Clients.DVA;
using UBind.Web.Configuration;
using UBind.Web.Extensions;
using UBind.Web.Filters;
using UBind.Web.HealthChecks;
using UBind.Web.Helpers;
using UBind.Web.Infrastructure;
using UBind.Web.InputFormatters;
using UBind.Web.Loggers;
using UBind.Web.Mapping;
using UBind.Web.Middleware;
using UBind.Web.Swagger;
using CustomerService = Domain.Services.CustomerService;
using FileService = Persistence.Services.Filesystem.FileService;
using PersonService = Application.Person.PersonService;
using QuoteService = Application.Services.QuoteService;
using SubscriptionService = Application.SubscriptionService;

/// <summary>
/// ASP.NET Core startup class.
/// </summary>
public class Startup
{
    private IApplicationBuilder app;
    private RedisConfiguration redisGlobalConfig;
    private ConnectionMultiplexer redisConnectionMultiplexer;
    private MiniProfilerConfiguration miniProfilerConfiguration;

    /// <summary>
    /// Initializes a new instance of the <see cref="Startup"/> class.
    /// </summary>
    /// <param name="env">The hosting environment.</param>
    public Startup(IHostEnvironment env)
    {
        var builder = new ConfigurationBuilder()
            .SetBasePath(env.ContentRootPath)
            .AddJsonFile("appsettings.json", optional: true, reloadOnChange: true)
            .AddJsonFile($"appsettings.{env.EnvironmentName}.json", optional: true)
            .AddEnvironmentVariables();
        this.Configuration = builder.Build();

        _ = ApplicationLifetimeManager.ApplicationShutdownToken;

        Log.Logger = new LoggerConfiguration()
            .ReadFrom.Configuration(this.Configuration)
            .WriteTo.Logger(lc => lc // Sub-logger for application log
                .Filter.ByExcluding(le => le.Properties.ContainsKey("LogType") && le.Properties["LogType"].ToString().Contains("\"AccessLog\""))
                .Filter.ByExcluding(le =>
                {
                    if (!le.Properties.ContainsKey("SourceContext"))
                    {
                        return false;
                    }

                    var sourceContext = le.Properties["SourceContext"].ToString().Trim('"');
                    return sourceContext.StartsWith("Microsoft.AspNetCore.Mvc")
                        || sourceContext.StartsWith("Microsoft.AspNetCore.Routing")
                        || sourceContext.StartsWith("Microsoft.AspNetCore.Hosting")
                        || sourceContext.StartsWith("Serilog.AspNetCore.RequestLoggingMiddleware")
                        || sourceContext.StartsWith("Microsoft.AspNetCore.StaticFiles.StaticFileMiddleware")
                        || sourceContext.StartsWith("Microsoft.AspNetCore.ResponseCaching.ResponseCachingMiddleware");
                })
                .WriteTo.File(
                    "logs/application-log-.txt",
                    buffered: true,
                    flushToDiskInterval: TimeSpan.FromSeconds(10), // Flush every 10 seconds
                    rollingInterval: RollingInterval.Day,
                    rollOnFileSizeLimit: true,
                    retainedFileCountLimit: 30,
                    outputTemplate: "{Timestamp:yyyy-MM-dd HH:mm:ss.fff zzz} [{Level:u3}] {SourceContext}: {Message:lj}{NewLine}{Exception}"))
            .WriteTo.Logger(lc => lc // Sub-logger for access log
                .Filter.ByIncludingOnly(le => le.Properties.ContainsKey("LogType") && le.Properties["LogType"].ToString().Contains("\"AccessLog\""))
                .WriteTo.File(
                    "logs/access-log-.txt",
                    buffered: true,
                    flushToDiskInterval: TimeSpan.FromSeconds(10), // Flush every 10 seconds
                    rollingInterval: RollingInterval.Day,
                    retainedFileCountLimit: 60,

                        // Log the access log in the combined log format, which is fairly standard for web servers
                        outputTemplate: "{RemoteIpAddress} - {UserId} [{Timestamp:dd/MMM/yyyy:HH:mm:ss zzz}] \"{Method} {Path} HTTP/{HttpRequestProtocol}\" {StatusCode} {ResponseLength} \"{Referrer}\" \"{UserAgent}\" {Duration}ms\r\n"))
                .WriteTo.Logger(lc => lc // Sub-logger for Hangfire log
                    .Enrich.FromLogContext()
                    .Enrich.WithHangfireContext()
                    .MinimumLevel.Override("Microsoft", LogEventLevel.Warning)
                    .MinimumLevel.Override("System", LogEventLevel.Warning)
                    .MinimumLevel.Override("Microsoft.Hosting.Lifetime", LogEventLevel.Error)
                    .MinimumLevel.Override("Hangfire", LogEventLevel.Warning)
                    .WriteTo.Hangfire())
                .CreateLogger();

        Log.Logger.Information("{0:l} {1:l} started.", env.ApplicationName, env.EnvironmentName);

        if (!GCSettings.IsServerGC)
        {
            throw new ArgumentException("You must set the application to run using Server Garbage Collection.");
        }

        // setup the connection to redis
        this.redisGlobalConfig = new RedisConfiguration();
        this.Configuration.GetSection("Redis").Bind(this.redisGlobalConfig);
        this.redisConnectionMultiplexer = ConnectionMultiplexer.Connect(this.redisGlobalConfig.ConnectionString);

        if (this.redisGlobalConfig.DebuggingLog)
        {
            this.HookRedisEvents();
        }

        // store the miniprofiler configuration
        this.miniProfilerConfiguration = this.Configuration
            .GetSection("MiniProfiler")
            .Get<MiniProfilerConfiguration>();
    }

    /// <summary>
    /// Gets the configuration.
    /// </summary>
    public IConfigurationRoot Configuration { get; }

    /// <summary>
    /// This method gets called by the runtime. Use this method to add services to the container.
    /// </summary>
    /// <param name="services">Collection to add services to.</param>
    public void ConfigureServices(IServiceCollection services)
    {
        var ubindSqlServerConnectionString = this.Configuration.GetConnectionString("UBind");

        services.AddLogging(loggingBuilder =>
        {
            loggingBuilder.AddConfiguration(this.Configuration.GetSection("Logging"));
            loggingBuilder.AddConsole();
            loggingBuilder.AddDebug();
            loggingBuilder.AddSerilog();
        });

        // Redis configuration
        var redisGlobalConfig = new RedisConfiguration();
        this.Configuration.GetSection("Redis").Bind(redisGlobalConfig);
        services.AddSingleton<IRedisConfiguration>(redisGlobalConfig);
        services.AddSingleton<IConnectionMultiplexer>(sp => this.redisConnectionMultiplexer);

        // Configure mini profiler
        this.ConfigureMiniProfiler(services, ubindSqlServerConnectionString);

        services.AddMemoryCache();
        services.AddHttpContextAccessor();
        services.Configure<EnvironmentSetting<ProductSetting>>(this.Configuration.GetSection("ProductConfiguration"));
        services.Configure<ConnectionStrings>(this.Configuration.GetSection(nameof(ConnectionStrings)));
        services.Configure<InternalUrlConfiguration>(this.Configuration.GetSection(nameof(InternalUrlConfiguration)));
        services.Configure<MicrosoftGraphConfiguration>(this.Configuration.GetSection(nameof(MicrosoftGraphConfiguration)));
        services.Configure<LocalFilesystemStorageConfiguration>(this.Configuration.GetSection(nameof(LocalFilesystemStorageConfiguration)));
        services.Configure<FilesystemStorageConfiguration>(this.Configuration.GetSection(nameof(FilesystemStorageConfiguration)));
        services.Configure<SmtpClientConfiguration>(this.Configuration.GetSection(nameof(SmtpClientConfiguration)));
        services.Configure<IpWhitelistConfiguration>(this.Configuration.GetSection(nameof(IpWhitelistConfiguration)));
        services.Configure<EmailInvitationConfiguration>(this.Configuration.GetSection(nameof(EmailInvitationConfiguration)));
        services.Configure<LuceneDirectoryConfiguration>(this.Configuration.GetSection(nameof(LuceneDirectoryConfiguration)));
        services.Configure<SystemAlertConfiguration>(this.Configuration.GetSection(nameof(SystemAlertConfiguration)));
        services.Configure<AuthConfiguration>(this.Configuration.GetSection(nameof(AuthConfiguration)));
        services.Configure<ErrorNotificationConfiguration>(this.Configuration.GetSection(nameof(ErrorNotificationConfiguration)));
        services.Configure<CustomHeaderConfiguration>(this.Configuration.GetSection(nameof(CustomHeaderConfiguration)));
        services.Configure<DatabaseConfiguration>(this.Configuration.GetSection(nameof(DatabaseConfiguration)));
        services.Configure<ThirdPartyDataSetsConfiguration>(this.Configuration.GetSection(nameof(ThirdPartyDataSetsConfiguration)));
        services.Configure<FtpConfiguration>(this.Configuration.GetSection(nameof(FtpConfiguration)));
        services.Configure<SpreadsheetPoolConfiguration>(this.Configuration.GetSection(nameof(SpreadsheetPoolConfiguration)));
        services.Configure<AbnLookupConfiguration>(this.Configuration.GetSection(nameof(AbnLookupConfiguration)));
        services.Configure<StartupJobEnvironmentConfiguration>(this.Configuration.GetSection(nameof(StartupJobEnvironmentConfiguration)));
        services.Configure<EncryptionConfiguration>(this.Configuration.GetSection(nameof(EncryptionConfiguration)));
        services.Configure<ClickatellConfiguration>(this.Configuration.GetSection(nameof(ClickatellConfiguration)));
        services.Configure<UBind.Application.ThirdPartyDataSets.NfidUpdaterJob.NfidConfiguration>(this.Configuration.GetSection(nameof(UBind.Application.ThirdPartyDataSets.NfidUpdaterJob.NfidConfiguration)));
        services.Configure<AdditionalPropertyBackgroundJobSetting>(this.Configuration.GetSection(nameof(AdditionalPropertyBackgroundJobSetting)));
        services.Configure<RateLimitConfiguration>(this.Configuration.GetSection(nameof(RateLimitConfiguration)));
        services.Configure<SentryAspNetCoreOptions>(this.Configuration.GetSection("Sentry"));
        services.Configure<SentryExtrasConfiguration>(this.Configuration.GetSection("SentryExtrasConfiguration"));
        services.Configure<SchemaConfiguration>(this.Configuration.GetSection(nameof(SchemaConfiguration)));
        services.Configure<HangfireConfiguration>(this.Configuration.GetSection("Hangfire"));
        services.Configure<SamlConfiguration>(this.Configuration.GetSection("Saml"));
        services.Configure<RedisConfiguration>(this.Configuration.GetSection("Redis"));
        services.Configure<DbMonitoringConfiguration>(this.Configuration.GetSection("DbMonitoringConfiguration"));
        services.Configure<ContentSecurityPolicyConfiguration>(this.Configuration.GetSection("ContentSecurityPolicyConfiguration"));

        services.AddHttpsRedirection(options =>
        {
            options.RedirectStatusCode = (int)HttpStatusCode.TemporaryRedirect;
            options.HttpsPort = 443;
        });

        // Add JWT validation
        var authConfiguration = this.Configuration.GetSection(nameof(AuthConfiguration)).Get<AuthConfiguration>();
        services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
            .AddJwtBearer(options =>
            {
                options.TokenValidationParameters = new TokenValidationParameters
                {
                    ValidateIssuer = true,
                    ValidateAudience = true,
                    ValidateLifetime = true,
                    ValidateIssuerSigningKey = true,
                    ValidIssuer = authConfiguration.TokenIssuer,
                    ValidAudience = authConfiguration.TokenAudience,
                    IssuerSigningKeyResolver = (s, securityToken, identifier, parameters) =>
                    {
                        // Resolve the service within a new scope since we can't access the existing scope.
                        using (IServiceScope serviceScope
                            = this.app.ApplicationServices.GetRequiredService<IServiceScopeFactory>().CreateScope())
                        {
                            // Get the active keys
                            var jwtKeyService = serviceScope.ServiceProvider.GetRequiredService<ICachedJwtKeyService>();
                            var keys = jwtKeyService.GetActiveKeys();

                            // Convert them to SymmetricSecurityKeys
                            var securityKeys = keys.Select(k => new SymmetricSecurityKey(Convert.FromBase64String(k.KeyBase64)));
                            return securityKeys;
                        }
                    },
                };
            });

        services.AddCors();

        services.AddResponseCaching();
        services
            .AddControllersWithViews((options) =>
            {
                options.SuppressImplicitRequiredAttributeForNonNullableReferenceTypes = true;
            })
            .AddMvcOptions(options =>
            {
                options.EnableEndpointRouting = true;
                options.Filters.Add(new ProducesResponseTypeAttribute(typeof(ProblemDetails), StatusCodes.Status500InternalServerError));
                options.Filters.Add(new ProducesResponseTypeAttribute(typeof(ProblemDetails), StatusCodes.Status400BadRequest));
                options.Filters.Add<RequiresFeatureFilter>();
                options.CacheProfiles.Add(CacheProfileNames.MaxStoreDuration, new CacheProfile
                {
                    Duration = int.MaxValue,
                    Location = ResponseCacheLocation.Any,
                    NoStore = false,
                });
            })
            .AddTypedRouting()
            .AddNewtonsoftJson(options =>
            {
                options.SerializerSettings.Converters.Add(new StringEnumConverter());
                options.SerializerSettings.Converters.Add(new IsoDateTimeConverter());
                options.SerializerSettings.Converters.Add(new UnixDateTimeConverter());
                options.SerializerSettings.ConfigureForNodaTime(DateTimeZoneProviders.Tzdb);
                options.SerializerSettings.DateParseHandling = DateParseHandling.None;
            })
            .AddXmlSerializerFormatters() // this is needed to support the application/xml content type for SAML
            .AddMvcOptions(options =>
                {
                    // Add the text/plain formatter here in the bottom so it gets added at the bottom of the list instead of preselected
                    options.InputFormatters.Add(new TextPlainInputFormatter());
                });

        services.AddProblemDetails(options =>
        {
            Infrastructure.ProblemDetailsFactory.RegisterMappings(options);
            options.IncludeExceptionDetails = (httpContext, exception) =>
            {
                var ipAddressWhitelistHelper = httpContext.RequestServices.GetRequiredService<IIpAddressWhitelistHelper>();
                var headerConfiguration = httpContext.RequestServices.GetRequiredService<ICustomHeaderConfiguration>();
                var clientIpAddress = httpContext.GetClientIPAddress(headerConfiguration.ClientIpCode);
                return ipAddressWhitelistHelper.IsWhitelisted(clientIpAddress);
            };

            options.OnBeforeWriteDetails = (httpContext, mvcProblemDetails) =>
            {
                if (mvcProblemDetails.Title == "Internal Server Error")
                {
                    mvcProblemDetails.Title = "We ran into a problem";
                    if (!mvcProblemDetails.Detail.EndsWith("."))
                    {
                        mvcProblemDetails.Detail += ".";
                    }

                    mvcProblemDetails.Detail += " We apologise for the inconvenience. Our technical staff "
                        + "have been notified automatically, however we would still appreciate it if you "
                        + "would get in touch with us and let us know what happened from your end.";
                }
            };
        });

        services.Configure<ApiBehaviorOptions>(options =>
        {
            options.InvalidModelStateResponseFactory = context =>
            {
                var problemDetails = new ValidationProblemDetails(context.ModelState)
                {
                    Status = (int)HttpStatusCode.BadRequest,
                };

                var result = new BadRequestObjectResult(problemDetails);
                result.ContentTypes.Add(ContentTypes.Json);
                return result;
            };
        });

        var databaseConfiguration = this.Configuration.GetSection(nameof(DatabaseConfiguration)).Get<DatabaseConfiguration>();
        var interceptor = new LoggingDbCommandInterceptor(
            warning => Log.Logger.Warning("{Warning}", warning),
            error => Log.Logger.Error("{Error}", error),
            databaseConfiguration);
        var dbConfiguration = new UBindDbConfiguration(interceptor);
        DbConfiguration.SetConfiguration(dbConfiguration);

        services.AddSingleton<IClock>(SystemClock.Instance);

        services.AddScoped<IUBindDbContext>(serviceProvider =>
        {
            ICqrsRequestContext cqrsContext = serviceProvider.GetService<ICqrsRequestContext>();
            var httpContextAccessor = serviceProvider.GetRequiredService<IHttpContextAccessor>();
            var httpContext = httpContextAccessor.HttpContext;
            if (httpContext != null)
            {
                RequestIntent? requestIntent = (RequestIntent?)httpContext.Items[nameof(RequestIntent)];
                cqrsContext.RequestIntent = requestIntent;
            }

            cqrsContext.DbContext = new UBindDbContext(ubindSqlServerConnectionString, cqrsContext.RequestIntent == RequestIntent.ReadOnly);
            return cqrsContext.DbContext;
        });

        services.AddScoped<IUnitOfWork, UnitOfWork>();
        services.AddScoped<ICqrsRequestContext, CqrsRequestContext>();
        services.AddScoped<IUniqueNumberSequenceGenerator>(_ => new UniqueNumberSequenceGenerator(ubindSqlServerConnectionString));
        services.AddScoped<IQuoteService, QuoteService>();
        services.AddScoped<ISearchableEntityService<IQuoteSearchResultItemReadModel, QuoteReadModelFilters>, QuoteLuceneIndexService>();
        services.AddScoped<ISearchableEntityService<IPolicySearchResultItemReadModel, PolicyReadModelFilters>, PolicyLuceneIndexService>();
        services.AddScoped<IDocumentService, DocumentService>();
        services.AddScoped<IDocumentFileRepository, DocumentFileRepository>();
        services.AddScoped<IProductRepository, ProductRepository>();
        services.AddScoped<IReleaseRepository, ReleaseRepository>();
        services.AddScoped<IDevReleaseRepository, DevReleaseRepository>();
        services.AddScoped<UBind.Application.IProductService, UBind.Application.ProductService>();
        services.AddScoped<INumberPoolService, NumberPoolService>();
        services.AddScoped<IReleaseQueryService, ReleaseQueryService>();
        services.AddSingleton<IGlobalReleaseCache, GlobalReleaseCache>();
        services.AddSingleton<IRequestRateLimitCache, RequestRateLimitCache>();
        services.AddSingleton<ILuceneIndexCache, LuceneIndexCache>();
        services.AddScoped<IReleaseService, ReleaseService>();
        services.AddScoped<IReleaseValidator, ReleaseValidator>();
        services.AddScoped<IDeploymentService, DeploymentService>();
        services.AddScoped<IDeploymentRepository, DeploymentRepository>();
        services.AddScoped<IConfigurationService, ConfigurationService>();
        services.AddScoped<IFormConfigurationGenerator, FormConfigurationGenerator>();
        services.AddScoped<IPasswordHashingService, PasswordHashingService>();
        services.AddScoped<IProductCompatibilityService, ProductCompatibilityService>();
        services.AddScoped<IPasswordComplexityValidator>(_ => PasswordComplexityValidator.Default);
        services.AddScoped<IPasswordReuseValidator>(_ => PasswordReuseValidator.Default);
        services.AddScoped<IDifferentialPriceCalculationService, DifferentialPriceCalculationService>();
        services.AddScoped<IEmailRequestRecordRepository<PasswordResetRecord>, EmailRequestRecordRepository<PasswordResetRecord>>();
        services.AddScoped<IEmailRequestRecordRepository<LoginAttemptResult>, EmailRequestRecordRepository<LoginAttemptResult>>();
        services.AddScoped<IEmailAddressBlockingEventRepository, EmailAddressBlockingEventRepository>();
        services.AddScoped<IPasswordResetTrackingService, PasswordResetTrackingService>();
        services.AddScoped<IPasswordResetTrackingService, PasswordResetTrackingService>();
        services.AddScoped<ILoginAttemptTrackingService, LoginAttemptTrackingService>();
        services.AddScoped<GuidIdMigration, GuidIdMigration>();
        services.AddScoped<ISetTenantIdOfRelationshipMigration, SetTenantIdOfRelationshipMigration>();
        services.AddScoped<ISetMissingTenantIdOfRelationshipMigration, SetMissingTenantIdOfRelationshipMigration>();
        services.AddScoped<IMissingCustomerRelationshipMigration, MissingCustomerRelationshipMigration>();
        services.AddScoped<IReportService, ReportService>();
        services.AddScoped<IDropGenerationService, DropGenerationService>();
        services.AddScoped<ISystemEventObservable, SystemEventObservable>();
        services.AddScoped<ISystemEventService, SystemEventService>();
        services.AddScoped<ISystemEventRepository, SystemEventRepository>();
        services.AddTransient<ISystemEventPersistenceService, SystemEventPersistenceService>();
        services.AddScoped<IRelationshipRepository, RelationshipRepository>();
        services.AddScoped<ITagRepository, TagRepository>();
        services.AddScoped<IEventPayloadFactory, EventPayloadFactory>();
        services.AddScoped<IQuoteSystemEventEmitter, QuoteSystemEventEmitter>();
        services.AddScoped<IClaimSystemEventEmitter, ClaimSystemEventEmitter>();
        services.AddScoped<IPortalSystemEventEmitter, PortalSystemEventEmitter>();
        services.AddScoped<IOrganisationSystemEventEmitter, OrganisationSystemEventEmitter>();
        services.AddScoped<ICustomerSystemEventEmitter, CustomerSystemEventEmitter>();
        services.AddScoped<IUserSystemEventEmitter, UserSystemEventEmitter>();
        services.AddScoped<ITenantSystemEventEmitter, TenantSystemEventEmitter>();
        services.AddScoped<IAutomationEventTriggerService, AutomationEventTriggerService>();
        services.AddScoped<IAutomationPortalPageTriggerService, AutomationPortalPageTriggerService>();
        services.AddScoped<ISystemEmailService, SystemEmailService>();
        services.AddScoped<IRoleRepository, RoleRepository>();
        services.AddScoped<IAbnLookupConfiguration, AbnLookupConfiguration>();
        services.AddScoped<IAdditionalPropertyValueService, AdditionalPropertyValueService>();
        services.AddScoped<SetEnvironmentAndHasAttachmentForEmailMigration>();
        services.AddScoped<IDataTableDefinitionService, DataTableDefinitionService>();
        services.AddScoped<IJwtKeyRotator, JwtKeyRotator>();
        services.AddScoped<ICachedJwtKeyService, CachedJwtKeyService>();
        services.AddScoped<IAuthenticationMethodService, AuthenticationMethodService>();
        services.AddScoped<IAdditionalPropertyTransformHelper, AdditionalPropertyTransformHelper>();
        services.AddScoped<IUserSessionDeletionService, UserSessionDeletionService>();
        services.AddScoped<IDbLogFileMaintenanceService, DbLogFileMaintenanceService>();
        services.AddScoped<
                ISummaryGeneratorFactory<QuoteDashboardSummaryModel, QuotePeriodicSummaryModel>, QuoteSummaryGeneratorFactory>();
        services.AddScoped<ISummaryGeneratorFactory<PolicyTransactionDashboardSummaryModel,
                PolicyTransactionPeriodicSummaryModel>, PolicyTransactionSummaryGeneratorFactory>();
        services.AddScoped<ISummaryGeneratorFactory<ClaimDashboardSummaryModel, ClaimPeriodicSummaryModel>, ClaimSummaryGeneratorFactory>();
        services.AddScoped<IAggregateSnapshotService<QuoteAggregate>, AggregateSnapshotService<QuoteAggregate>>();
        services.AddScoped<IAggregateSnapshotService<PersonAggregate>, AggregateSnapshotService<PersonAggregate>>();
        services.AddScoped<IAggregateSnapshotService<UserAggregate>, AggregateSnapshotService<UserAggregate>>();
        services.AddScoped<IAggregateSnapshotService<CustomerAggregate>, AggregateSnapshotService<CustomerAggregate>>();
        services.AddScoped<IAggregateSnapshotService<ClaimAggregate>, AggregateSnapshotService<ClaimAggregate>>();
        services.AddScoped<IAggregateSnapshotService<Organisation>, AggregateSnapshotService<Organisation>>();
        services.AddScoped<IAggregateSnapshotService<ReportAggregate>, AggregateSnapshotService<ReportAggregate>>();
        services.AddScoped<IAggregateSnapshotService<TextAdditionalPropertyValue>, AggregateSnapshotService<TextAdditionalPropertyValue>>();
        services.AddScoped<IAggregateSnapshotService<AdditionalPropertyDefinition>, AggregateSnapshotService<AdditionalPropertyDefinition>>();
        services.AddScoped<IAggregateSnapshotService<FinancialTransactionAggregate<Domain.Accounting.Invoice>>, AggregateSnapshotService<FinancialTransactionAggregate<Domain.Accounting.Invoice>>>();
        services.AddScoped<IAggregateSnapshotService<FinancialTransactionAggregate<Domain.Accounting.CreditNote>>, AggregateSnapshotService<FinancialTransactionAggregate<Domain.Accounting.CreditNote>>>();
        services.AddScoped<IAggregateSnapshotService<PortalAggregate>, AggregateSnapshotService<PortalAggregate>>();
        services.AddScoped<IAggregateSnapshotService<StructuredDataAdditionalPropertyValue>, AggregateSnapshotService<StructuredDataAdditionalPropertyValue>>();

        services.AddScoped<IAggregateLockingService, AggregateLockingService>();

        // Migrations
        services.AddScoped<IAddQuoteAggregateSnapshotMigration, AddQuoteAggregateSnapshotMigration>();
        services.AddScoped<IRemoveDuplicateFileContentsMigration, RemoveDuplicateFileContentsMigration>();
        services.AddScoped<IRegenerateAggregateSnapshotMigration, RegenerateAggregateSnapshotMigration>();

        // Aggregate repositories
        services.AddScoped<IPersonAggregateRepository, PersonAggregateRepository>();
        services.AddScoped<IUserAggregateRepository, UserAggregateRepository>();
        services.AddScoped<IPortalAggregateRepository, PortalAggregateRepository>();
        services.AddScoped<ICustomerAggregateRepository, CustomerAggregateRepository>();
        services.AddScoped<IQuoteAggregateRepository, QuoteAggregateRepository>();
        services.AddScoped<IClaimAggregateRepository, ClaimAggregateRepository>();
        services.AddScoped<IQuoteEventRepository, QuoteEventRepository>();
        services.AddScoped<IReportAggregateRepository, ReportAggregateRepository>();
        services.AddScoped<IProductFeatureSettingRepository, ProductFeatureSettingRepository>();
        services.AddScoped<IOrganisationAggregateRepository, OrganisationAggregateRepository>();
        services.AddScoped<IAdditionalPropertyDefinitionAggregateRepository, AdditionalPropertyDefinitionAggregateRepository>();
        services.AddScoped<IPaymentAggregateRepository, PaymentAggregateRepository>();
        services.AddScoped<IRefundAggregateRepository, RefundAggregateRepository>();
        services.AddScoped<ITextAdditionalPropertyValueAggregateRepository, TextAdditionalPropertyValueAggregateRepository>();
        services.AddScoped<IAggregateRepositoryResolver, AggregateRepositoryResolver>();
        services.AddScoped<IStructuredDataAdditionalPropertyValueAggregateRepository, StructuredDataAdditionalPropertyValueAggregateRepository>();
        services.AddScoped<IAggregateSnapshotRepository, AggregateSnapshotRepository>();

        // Writable read model repositories
        services.AddScoped<IWritableReadModelRepository<UserReadModel>, ReadModelUpdateRepository<UserReadModel>>();
        services.AddScoped<IWritableReadModelRepository<PortalReadModel>, ReadModelUpdateRepository<PortalReadModel>>();
        services.AddScoped<IWritableReadModelRepository<PortalSignInMethodReadModel>, ReadModelUpdateRepository<PortalSignInMethodReadModel>>();
        services.AddScoped<IWritableReadModelRepository<PolicyTransaction>, ReadModelUpdateRepository<PolicyTransaction>>();
        services.AddScoped<IWritableReadModelRepository<CustomerReadModel>, ReadModelUpdateRepository<CustomerReadModel>>();
        services.AddScoped<IWritableReadModelRepository<NewQuoteReadModel>, ReadModelDeferredUpdateRepository<NewQuoteReadModel>>();
        services.AddScoped<IWritableReadModelRepository<PolicyReadModel>, ReadModelDeferredUpdateRepository<PolicyReadModel>>();
        services.AddScoped<IWritableReadModelRepository<ClaimReadModel>, ReadModelUpdateRepository<ClaimReadModel>>();
        services.AddScoped<IWritableReadModelRepository<ClaimVersionReadModel>, ReadModelUpdateRepository<ClaimVersionReadModel>>();
        services.AddScoped<IWritableReadModelRepository<QuoteDocumentReadModel>, ReadModelUpdateRepository<QuoteDocumentReadModel>>();
        services.AddScoped<IWritableReadModelRepository<ClaimAttachmentReadModel>, ReadModelUpdateRepository<ClaimAttachmentReadModel>>();
        services.AddScoped<IWritableReadModelRepository<QuoteVersionReadModel>, ReadModelUpdateRepository<QuoteVersionReadModel>>();
        services.AddScoped<IWritableReadModelRepository<ReportReadModel>, ReadModelUpdateRepository<ReportReadModel>>();
        services.AddScoped<IWritableReadModelRepository<PaymentReadModel>, ReadModelUpdateRepository<PaymentReadModel>>();
        services.AddScoped<IWritableReadModelRepository<RefundReadModel>, ReadModelUpdateRepository<RefundReadModel>>();
        services.AddScoped<IWritableReadModelRepository<OrganisationReadModel>, ReadModelUpdateRepository<OrganisationReadModel>>();
        services.AddScoped<IWritableReadModelRepository<PersonReadModel>, ReadModelUpdateRepository<PersonReadModel>>();
        services.AddScoped<IWritableReadModelRepository<EmailAddressReadModel>, ReadModelUpdateRepository<EmailAddressReadModel>>();
        services.AddScoped<IWritableReadModelRepository<PhoneNumberReadModel>, ReadModelUpdateRepository<PhoneNumberReadModel>>();
        services.AddScoped<IWritableReadModelRepository<StreetAddressReadModel>, ReadModelUpdateRepository<StreetAddressReadModel>>();
        services.AddScoped<IWritableReadModelRepository<WebsiteAddressReadModel>, ReadModelUpdateRepository<WebsiteAddressReadModel>>();
        services.AddScoped<IWritableReadModelRepository<MessengerIdReadModel>, ReadModelUpdateRepository<MessengerIdReadModel>>();
        services.AddScoped<IWritableReadModelRepository<SocialMediaIdReadModel>, ReadModelUpdateRepository<SocialMediaIdReadModel>>();
        services.AddScoped<IWritableReadModelRepository<UserLoginEmail>, ReadModelUpdateRepository<UserLoginEmail>>();
        services.AddScoped<
            IWritableReadModelRepository<AuthenticationMethodReadModelSummary>,
            ReadModelUpdateRepository<AuthenticationMethodReadModelSummary>>();

        services.AddScoped<IWritableReadModelRepository<AdditionalPropertyDefinitionReadModel>, ReadModelUpdateRepository<AdditionalPropertyDefinitionReadModel>>();
        services.AddScoped<IWritableReadModelRepository<QuoteFileAttachmentReadModel>, ReadModelUpdateRepository<QuoteFileAttachmentReadModel>>();
        services.AddScoped<IPolicyTransactionRepository, PolicyTransactionRepository>();
        services.AddScoped<IEventRecordRepository, EventRecordRepository>();
        services.AddScoped<IReadModelRepository<CustomerReadModel>>(sp => sp.GetService<IWritableReadModelRepository<CustomerReadModel>>());
        services.AddScoped<IReadModelRepository<UserReadModel>>(sp => sp.GetService<IWritableReadModelRepository<UserReadModel>>());
        services.AddScoped<IReadModelRepository<PortalReadModel>>(sp => sp.GetService<IWritableReadModelRepository<PortalReadModel>>());
        services.AddScoped<IReadModelRepository<IClaimReadModel>>(sp => sp.GetService<IWritableReadModelRepository<IClaimReadModel>>());
        services.AddScoped<IReadModelRepository<OrganisationReadModel>>(sp => sp.GetService<IWritableReadModelRepository<OrganisationReadModel>>());
        services.AddScoped<IReadModelRepository<QuoteVersionReadModel>>(sp => sp.GetService<IWritableReadModelRepository<QuoteVersionReadModel>>());
        services.AddScoped<IReadModelRepository<ClaimVersionReadModel>>(sp => sp.GetService<IWritableReadModelRepository<ClaimVersionReadModel>>());
        services.AddScoped<IReadModelRepository<ClaimAttachmentReadModel>>(sp => sp.GetService<IWritableReadModelRepository<ClaimAttachmentReadModel>>());
        services.AddScoped<IReadModelRepository<PersonReadModel>>(sp => sp.GetService<IWritableReadModelRepository<PersonReadModel>>());
        services.AddScoped<IReadModelRepository<PaymentAllocationReadModel>>(sp => sp.GetService<IWritableReadModelRepository<PaymentAllocationReadModel>>());
        services.AddScoped<IReadModelRepository<RefundAllocationReadModel>>(sp => sp.GetService<IWritableReadModelRepository<RefundAllocationReadModel>>());
        services.AddScoped<IReadModelRepository<UBind.Domain.Accounting.Invoice>>(sp => sp.GetService<IWritableReadModelRepository<UBind.Domain.Accounting.Invoice>>());
        services.AddScoped<IReadModelRepository<UBind.Domain.Accounting.CreditNote>>(sp => sp.GetService<IWritableReadModelRepository<UBind.Domain.Accounting.CreditNote>>());
        services.AddScoped<IWritableReadModelRepository<TextAdditionalPropertyValueReadModel>, ReadModelUpdateRepository<TextAdditionalPropertyValueReadModel>>();
        services.AddScoped<ITextAdditionalPropertyValueReadModelWriter, TextAdditionalPropertyValueReadModelWriter>();
        services.AddScoped<IWritableReadModelRepository<TextAdditionalPropertyValueReadModel>, ReadModelUpdateRepository<TextAdditionalPropertyValueReadModel>>();
        services.AddScoped<ITextAdditionalPropertyValueReadModelWriter, TextAdditionalPropertyValueReadModelWriter>();
        services.AddScoped<IWritableReadModelRepository<StructuredDataAdditionalPropertyValueReadModel>, ReadModelUpdateRepository<StructuredDataAdditionalPropertyValueReadModel>>();
        services.AddScoped<IStructuredDataAdditionalPropertyValueReadModelWriter, StructuredDataAdditionalPropertyValueReadModelWriter>();
        services.AddScoped<ITinyUrlRepository, TinyUrlRepository>();

        // Query repositories
        services.AddScoped<IUserLoginEmailRepository, UserLoginEmailRepository>();
        services.AddScoped<IClaimAttachmentReadModelRepository, ClaimAttachmentReadModelRepository>();
        services.AddScoped<IClaimReadModelRepository, ClaimReadModelRepository>();
        services.AddScoped<IClaimVersionReadModelRepository, ClaimVersionReadModelRepository>();
        services.AddScoped<IClaimNumberRepository, ClaimNumberRepository>();
        services.AddScoped<ICustomerReadModelRepository, CustomerReadModelRepository>();
        services.AddScoped<IUserReadModelRepository, UserReadModelRepository>();
        services.AddScoped<IPortalReadModelRepository, PortalReadModelRepository>();
        services.AddScoped<IPortalSignInMethodReadModelRepository, PortalSignInMethodReadModelRepository>();
        services.AddScoped<IPolicyReadModelRepository, PolicyReadModelRepository>();
        services.AddScoped<IQuoteReadModelRepository, QuoteReadModelRepository>();
        services.AddScoped<IQuoteDocumentReadModelRepository, QuoteDocumentReadModelRepository>();
        services.AddScoped<IFileAttachmentRepository<ClaimFileAttachment>, ClaimFileAttachmentRepository>();
        services.AddScoped<IFileAttachmentRepository<QuoteFileAttachment>, QuoteFileAttachmentRepository>();
        services.AddScoped<IQuoteVersionReadModelRepository, QuoteVersionReadModelRepository>();
        services.AddScoped<IReportReadModelRepository, ReportReadModelRepository>();
        services.AddScoped<IReportFileRepository, ReportFileRepository>();
        services.AddScoped<IPolicyTransactionReadModelRepository, PolicyTransactionReadModelRepository>();
        services.AddScoped<IFinancialTransactionReadModelRepository<PaymentReadModel>, PaymentReadModelRepository>();
        services.AddScoped<IFinancialTransactionReadModelRepository<RefundReadModel>, RefundReadModelRepository>();
        services.AddScoped<IOrganisationReadModelRepository, OrganisationReadModelRepository>();
        services.AddScoped<IPersonReadModelRepository, PersonReadModelRepository>();
        services.AddScoped<IStartupJobRepository, StartupJobRepository>();
        services.AddScoped<IProductPortalSettingRepository, ProductPortalSettingRepository>();
        services.AddScoped<IAdditionalPropertyDefinitionRepository, AdditionalPropertyDefinitionRepository>();
        services.AddScoped<ITextAdditionalPropertyValueReadModelRepository, TextAdditionalPropertyValueReadModelRepository>();
        services.AddScoped<IProductOrganisationSettingRepository, ProductOrganisationSettingRepository>();
        services.AddScoped<IEntitySettingsRepository, EntitySettingsRepository>();
        services.AddScoped<ISavedPaymentMethodRepository, SavedPaymentMethodRepository>();
        services.AddScoped<IAuthenticationMethodReadModelRepository, AuthenticationMethodReadModelRepository>();
        services.AddScoped<IStructuredDataAdditionalPropertyValueReadModelRepository, StructuredDataAdditionalPropertyValueReadModelRepository>();

        // Other repositories
        services.AddScoped<IJwtKeyRepository, JwtKeyRepository>();

        // Redis repositories
        services.AddScoped<IUserSessionRepository, UserSessionRepository>();
        services.AddScoped<IProductReleaseSynchroniseRepository, ProductReleaseSynchroniseRepository>();
        services.AddScoped<INumberPoolCountLastCheckedTimestampRepository, NumberPoolCountLastCheckedTimestampRepository>();

        // Read model writers
        services.AddScoped<IHttpContextPropertiesResolver, HttpContextPropertiesResolver>();
        services.AddScoped<IUserReadModelWriter, UserReadModelWriter>();
        services.AddScoped<IPortalReadModelWriter, PortalReadModelWriter>();
        services.AddScoped<IPortalSignInMethodReadModelWriter, PortalSignInMethodReadModelWriter>();
        services.AddScoped<ICustomerReadModelWriter, CustomerReadModelWriter>();
        services.AddScoped<IQuoteReadModelWriter, QuoteReadModelWriter>();
        services.AddScoped<IPolicyReadModelWriter, PolicyReadModelWriter>();
        services.AddScoped<IQuoteDocumentReadModelWriter, QuoteDocumentReadModelWriter>();
        services.AddScoped<IClaimReadModelWriter, ClaimReadModelWriter>();
        services.AddScoped<IClaimAttachmentReadModelWriter, ClaimAttachmentReadModelWriter>();
        services.AddScoped<IQuoteVersionReadModelWriter, QuoteVersionReadModelWriter>();
        services.AddScoped<IReportReadModelWriter, ReportReadModelWriter>();
        services.AddScoped<IClaimVersionReadModelWriter, ClaimVersionReadModelWriter>();
        services.AddScoped<IOrganisationReadModelWriter, OrganisationReadModelWriter>();
        services.AddScoped<IPersonReadModelWriter, PersonReadModelWriter>();
        services.AddScoped<IAdditionalPropertyDefinitionReadModelWriter, AdditionalPropertyDefinitionReadModelWriter>();
        services.AddScoped<IFinancialTransactionReadModelWriter<Domain.Accounting.Invoice>, PaymentReadModelWriter>();
        services.AddScoped<IFinancialTransactionReadModelWriter<Domain.Accounting.CreditNote>, RefundReadModelWriter>();
        services.AddScoped<IAuthenticationMethodReadModelWriter, AuthenticationMethodReadModelWriter>();
        services.AddScoped<IUserLinkedIdentityReadModelWriter, UserLinkedIdentityReadModelWriter>();
        services.AddScoped<IOrganisationLinkedIdentityReadModelWriter, OrganisationLinkedIdentityReadModelWriter>();

        // Event observers
        services.AddScoped<IPortalEventObserver, PortalEventAggregator>();
        services.AddScoped<IPersonEventObserver, PersonEventAggregator>();
        services.AddScoped<IClaimEventObserver, ClaimEventAggregator>();
        services.AddScoped<IQuoteEventObserver, QuoteEventAggregator>();
        services.AddScoped<IUserEventObserver, UserEventAggregator>();
        services.AddScoped<ICustomerEventObserver, CustomerEventAggregator>();
        services.AddScoped<IReportEventObserver, ReportEventAggregator>();
        services.AddScoped<IOrganisationEventObserver, OrganisationEventAggregator>();
        services.AddScoped<IQuoteEventIntegrationScheduler, QuoteEventIntegrationScheduler>();
        services.AddScoped<IReportEventObserver>(sp => sp.GetService<IReportReadModelWriter>());
        services.AddScoped<IOrganisationEventObserver, OrganisationEventAggregator>();
        services.AddScoped<EventAggregator<AdditionalPropertyDefinition, Guid>, AdditionalPropertyDefinitionEventAggregator>();
        services.AddScoped<IFinancialTransactionEventObserver<Domain.Accounting.Invoice>, PaymentEventAggregator>();
        services.AddScoped<IFinancialTransactionEventObserver<Domain.Accounting.CreditNote>, RefundEventAggregator>();
        services.AddScoped<IAdditionalPropertyDefinitionEventObserver, AdditionalPropertyDefinitionReadModelWriter>();
        services.AddScoped<ITextAdditionalPropertyValueEventObserver, TextAdditionalPropertyValueReadModelWriter>();
        services.AddScoped<EventAggregator<TextAdditionalPropertyValue, Guid>,
            TextAdditionalPropertyValueEventAggregator>();
        services.AddScoped<IStructuredDataAdditionalPropertyValueEventObserver, StructuredDataAdditionalPropertyValueReadModelWriter>();
        services.AddScoped<EventAggregator<StructuredDataAdditionalPropertyValue, Guid>,
            StructuredDataAdditionalPropertyValueEventAggregator>();
        services.AddScoped<PropertyTypeEvaluatorService>((sp) =>
        {
            var dictionary = new Dictionary<AdditionalPropertyDefinitionType, IAdditionalPropertyValueProcessor>
                {
                    {
                        AdditionalPropertyDefinitionType.Text,
                        new TextAdditionalPropertyValueProcessor(
                            sp.GetService<ITextAdditionalPropertyValueReadModelRepository>(),
                            sp.GetService<IClock>(),
                            sp.GetService<ITextAdditionalPropertyValueAggregateRepository>(),
                            sp.GetService<IWritableReadModelRepository<TextAdditionalPropertyValueReadModel>>())
                    },
                    {
                        AdditionalPropertyDefinitionType.StructuredData,
                        new StructuredDataAdditionalPropertyValueProcessor(
                            sp.GetService<IStructuredDataAdditionalPropertyValueReadModelRepository>(),
                            sp.GetService<IClock>(),
                            sp.GetService<IStructuredDataAdditionalPropertyValueAggregateRepository>(),
                            sp.GetService<IWritableReadModelRepository<StructuredDataAdditionalPropertyValueReadModel>>())
                    },
                };
            return new PropertyTypeEvaluatorService(dictionary);
        });

        services.AddScoped<IDocumentAttacher, QuoteDocumentAttacher>();
        services.AddScoped<IDocumentAttacher, QuoteVersionDocumentAttacher>();
        services.AddScoped<IDocumentAttacher, ClaimDocumentAttacher>();
        services.AddScoped<IDocumentAttacher, ClaimVersionDocumentAttacher>();
        services.AddScoped<IDocumentAttacher, PolicyTransactionDocumentAttacher>();
        services.AddScoped<IDocumentAttacher, PolicyDocumentAttacher>();

        // Entities
        services.AddScoped<IUserProfilePictureRepository, UserProfilePictureRepository>();
        services.AddScoped<IMailClientFactory, MailClientFactory>();

        // Infrastructure
        services.AddSingleton<IActionContextAccessor, ActionContextAccessor>();
        services.AddScoped<IUrlHelper>(x =>
        {
            var actionContext = x.GetRequiredService<IActionContextAccessor>().ActionContext;
            var factory = x.GetRequiredService<IUrlHelperFactory>();
            return factory.GetUrlHelper(actionContext);
        });

        services.AddScoped<IPolicyTransactionTimeOfDayScheme, DefaultPolicyTransactionTimeOfDayScheme>();
        services.AddScoped<ICustomerService, CustomerService>();
        services.AddSingleton<IAuthenticationTokenProvider, AuthenticationTokenProvider>();
        services.AddSingleton<ICachingAuthenticationTokenProvider, CachingAuthenticationTokenProvider>();
        services.AddSingleton<IGraphUrlProvider, GraphUrlProvider>();
        services.AddScoped<FilesystemStoragePathService>();
        services.AddScoped<ISubscriptionService, SubscriptionService>();
        services.AddScoped<IFilesystemStoragePathService, FilesystemStoragePathService>();
        services.AddSingleton<IFileSystemService, FileService>(provider =>
        {
            var fileSystem = provider.GetRequiredService<IFileSystem>();
            var filesystemFileCompressionService = provider.GetRequiredService<IFileSystemFileCompressionService>();

            return new FileService(fileSystem, filesystemFileCompressionService);
        });
        services.AddSingleton<IFileSystem, FileSystem>();
        services.AddSingleton<IFileSystemFileCompressionService, FilesystemFileCompressionService>();
        services.AddScoped<IPolicyService, PolicyService>();
        services.AddScoped<IAccountingTransactionService, AccountingTransactionService>();
        services.AddScoped<IProductReleaseService, ProductReleaseService>();
        services.AddScoped<IApplicationFundingService, ApplicationFundingService>();
        services.AddScoped<IQuoteEndorsementService, QuoteEndorsementService>();
        services.AddScoped<IPolicyRenewalService, PolicyRenewalService>();
        services.AddScoped<IDomainFundingService, ApplicationFundingService>();
        services.AddScoped<IQuoteAggregateResolverService, QuoteAggregateResolverService>();
        services.AddScoped<IApplicationQuoteService, ApplicationQuoteService>();
        services.AddScoped<IFileContentRepository, FileContentRepository>();
        services.AddScoped<IDkimSettingRepository, DkimSettingRepository>();
        services.AddScoped<IProductConfigurationProvider, ProductConfigurationProvider>();
        services.AddScoped<IQuoteExpirySettingsProvider, QuoteExpirySettingsProvider>();
        services.AddScoped<IQuoteWorkflowProvider, ProductConfigurationProvider>();
        services.AddScoped<IClaimWorkflowProvider, ProductConfigurationProvider>();
        services.AddScoped<IUniqueIdentifierService, UniqueIdentifierService>();
        services.AddScoped<IQuoteReferenceNumberGenerator, QuoteReferenceNumberGenerator>();
        services.AddScoped<IClaimReferenceNumberGenerator, ClaimReferenceNumberGenerator>();
        services.AddScoped<IRefundReferenceNumberGenerator, RefundReferenceNumberGenerator>();
        services.AddScoped<IPaymentReferenceNumberGenerator, PaymentReferenceNumberGenerator>();
        services.AddScoped<IPaymentService, PaymentService>();
        services.AddScoped<IGuidIdMigration, GuidIdMigration>();
        services.AddScoped<IFileContentMigration, FileContentMigration>();
        services.AddScoped<IAggregateEventsMigration, AggregateEventsMigration>();
        services.AddScoped<IRecreateReadModelsOfEventsMigration, RecreateReadModelsOfEventsMigration>();
        services.AddScoped<IPolicyLatestRenewalEffectiveTimeMigration, PolicyLatestRenewalEffectiveTimeMigration>();
        services.AddScoped<IInitialDataSeeder, InitialDataSeeder>();
        services.AddScoped<IRolePermissionMigration, RolePermissionMigration>();
        services.AddScoped<IPolicyStateMigration, PolicyStateMigration>();
        services.AddScoped<ICustomerUserOwnerIdMigration, CustomerUserOwnerIdMigration>();
        services.AddScoped<IPolicyLatestExpiryDateTimeMigration, PolicyLatestExpiryDateTimeMigration>();
        services.AddScoped<CreditNoteService, CreditNoteService>();
        services.AddScoped<IUrlTokenGenerator, UrlTokenGenerator>();
        services.AddScoped<ICachingResolver, CachingResolver>();
        services.AddScoped<IApplicationDocumentService, ApplicationDocumentService>();
        services.AddScoped<PaymentGatewayFactory>();
        services.AddScoped<IPaymentConfigurationProvider, PaymentConfigurationProvider>();
        services.AddScoped<PaymentConfigurationParser>();
        services.AddScoped<IDeftCustomerReferenceNumberGenerator, DeftCustomerReferenceNumberGenerator>();
        services.AddScoped<IIqumulateService, IqumulateFundingService>();
        services.AddScoped<IFundingConfigurationProvider, FundingConfigurationProvider>();
        services.AddScoped<FundingServiceFactory>();
        services.AddScoped<IFundingServiceRedirectUrlHelper, FundingServiceRedirectUrlHelper>();
        services.AddSingleton<IAccessTokenProvider, AccessTokenProvider>();
        services.AddSingleton<ICachingAccessTokenProvider, CachingAccessTokenProvider>();
        services.AddScoped<IIntegrationConfigurationProvider, IntegrationConfigurationProvider>();
        services.AddSingleton<IBackgroundJobClient, BackgroundJobClient>();
        services.AddScoped<IJobClient>(s =>
            new JobClient(
                s.GetRequiredService<IBackgroundJobClient>(),
                s.GetRequiredService<ICachingResolver>()));
        services.AddScoped<IExporterDependencyProvider, ExporterDependencyProvider>();
        services.AddScoped<IPolicyNumberRepository, PolicyNumberRepository>();
        services.AddScoped<IFileContentsLoader, FileContentsLoader>();
        services.AddScoped<IInvoiceNumberRepository, InvoiceNumberRepository>();
        services.AddScoped<ICreditNoteNumberRepository, CreditNoteNumberRepository>();
        services.AddScoped<IUniqueIdentifierService, UniqueIdentifierService>();
        services.AddScoped<IClaimService, ClaimService>();
        services.AddScoped<IMsWordEngineService, MsWordEngineService>();
        services.AddScoped<IGemBoxMsWordEngineService, GemBoxMsWordEngineService>();
        services.AddScoped<IMsExcelEngineService, MsExcelEngineService>();
        services.AddScoped<IMessagingService, EmailMessagingService>();
        services.AddScoped<ClientIPAddressFilterAttribute>();
        services.AddScoped<RequestRateLimitAttribute>();
        services.AddScoped<IApplicationQuoteFileAttachmentService, ApplicationQuoteFileAttachmentService>();
        services.AddScoped<IApplicationClaimFileAttachmentService, ApplicationClaimFileAttachmentService>();
        services.AddScoped<IQuoteFileAttachmentRepository, QuoteFileAttachmentRepository>();
        services.AddScoped<IApplicationDocumentService, ApplicationDocumentService>();
        services.AddScoped<ISystemEventRepository, SystemEventRepository>();
        services.AddScoped<IProductFeatureSettingService, ProductFeatureSettingService>();
        services.AddScoped<ISmsClient, ClickatellClient>();
        services.AddScoped<IAdditionalPropertyDefinitionValidator>((prov) =>
        {
            var factoryValidatorForTenant = new AdditionalPropertyDefinitionValidatorForTenantFactory(
                prov.GetService<ITenantRepository>(),
                prov.GetService<IAdditionalPropertyDefinitionRepository>());

            var factoryValidatorForProduct = new AdditionalPropertyDefinitionValidatorForProductFactory(
                prov.GetService<ICachingResolver>(),
                prov.GetService<IAdditionalPropertyDefinitionRepository>());

            var factoryValidatorForOrganisation = new AdditionalPropertyDefinitionValidatorForOrganisationFactory(
                prov.GetService<IOrganisationReadModelRepository>(),
                prov.GetService<IAdditionalPropertyDefinitionRepository>(),
                prov.GetService<ICachingResolver>());

            var mappings = new Dictionary<AdditionalPropertyDefinitionContextType, IAdditionalPropertyDefinitionValidatorFactory>
            {
            { AdditionalPropertyDefinitionContextType.Tenant, factoryValidatorForTenant },
            { AdditionalPropertyDefinitionContextType.Product, factoryValidatorForProduct },
            { AdditionalPropertyDefinitionContextType.Organisation, factoryValidatorForOrganisation },
            };

            return new AdditionalPropertyDefinitionValidator(mappings);
        });
        services.AddScoped<IAdditionalPropertyDefinitionJsonValidator, AdditionalPropertyDefinitionJsonValidator>();
        services.AddScoped<IAdditionalPropertyDefinitionFilterResolver, AdditionalPropertyDefinitionFilterResolver>();
        services.AddScoped<IPdfEngineService, PdfEngineService>();
        services.AddScoped<IOrganisationTransferService, OrganisationTransferService>();
        services.AddScoped<IAdditionalPropertyContextResolver, AdditionalPropertyContextResolver>();
        services.AddScoped<IAdditionalPropertyModelResolverHelper, AdditionalPropertyModelResolverHelper>();
        services.AddScoped<IPdfEngineService, PdfEngineService>();
        services.AddScoped<ISerialisedEntityFactory, SerialisedEntityFactory>();

        services.AddSingleton<IConnectionConfiguration>(
           resolver => resolver.GetRequiredService<IOptions<ConnectionStrings>>().Value);
        services.AddSingleton<IRazorEngineService>(RazorEngineService.Create());
        services.AddSingleton<IMicrosoftGraphConfiguration>(
            resolver => resolver.GetRequiredService<IOptions<MicrosoftGraphConfiguration>>().Value);
        services.AddSingleton<ILocalFilesystemStorageConfiguration>(
            resolver => resolver.GetRequiredService<IOptions<LocalFilesystemStorageConfiguration>>().Value);
        services.AddSingleton<IFilesystemStorageConfiguration>(
            resolver => resolver.GetRequiredService<IOptions<FilesystemStorageConfiguration>>().Value);
        services.AddSingleton<IInternalUrlConfiguration>(
            resolver => resolver.GetRequiredService<IOptions<InternalUrlConfiguration>>().Value);
        services.AddSingleton<ISmtpClientConfiguration>(
            resolver => resolver.GetRequiredService<IOptions<SmtpClientConfiguration>>().Value);
        services.AddSingleton<IIpWhitelistConfiguration>(
            resolver => resolver.GetRequiredService<IOptions<IpWhitelistConfiguration>>().Value);
        services.AddSingleton<IEnvironmentSetting<ProductSetting>>(
            resolver => resolver.GetRequiredService<IOptions<EnvironmentSetting<ProductSetting>>>().Value);
        services.AddSingleton<IEmailInvitationConfiguration>(
            resolver => resolver.GetRequiredService<IOptions<EmailInvitationConfiguration>>().Value);
        services.AddSingleton<ILuceneDirectoryConfiguration>(
           resolver => resolver.GetRequiredService<IOptions<LuceneDirectoryConfiguration>>().Value);
        services.AddSingleton<IESystemAlertConfiguration>(
            resolver => resolver.GetRequiredService<IOptions<SystemAlertConfiguration>>().Value);
        services.AddSingleton<IIpAddressWhitelistHelper, IpAddressWhitelistHelper>();
        services.AddSingleton<ICustomHeaderConfiguration>(
            resolver => resolver.GetRequiredService<IOptions<CustomHeaderConfiguration>>().Value);
        services.AddSingleton<IRateLimitConfiguration>(
            resolver => resolver.GetRequiredService<IOptions<RateLimitConfiguration>>().Value);
        services.AddSingleton<ErrorNotificationConfiguration>(
            resolver => resolver.GetRequiredService<IOptions<ErrorNotificationConfiguration>>().Value);
        services.AddSingleton<IDatabaseConfiguration>(
            resolver => resolver.GetRequiredService<IOptions<DatabaseConfiguration>>().Value);
        services.AddSingleton<ISpreadsheetPoolConfiguration>(
            resolver => resolver.GetRequiredService<IOptions<SpreadsheetPoolConfiguration>>().Value);
        services.AddSingleton<IAbnLookupConfiguration>(
           resolver => resolver.GetRequiredService<IOptions<AbnLookupConfiguration>>().Value);
        services.AddSingleton<IThirdPartyDataSetsConfiguration>(
            resolver => resolver.GetRequiredService<IOptions<ThirdPartyDataSetsConfiguration>>().Value);
        services.AddSingleton<IStartupJobEnvironmentConfiguration>(
            resolver => resolver.GetRequiredService<IOptions<StartupJobEnvironmentConfiguration>>().Value);
        services.AddSingleton<IEncryptionConfiguration>(
            resolver => resolver.GetRequiredService<IOptions<EncryptionConfiguration>>().Value);
        services.AddSingleton<ISmsConfiguration>(
            resolver => resolver.GetRequiredService<IOptions<ClickatellConfiguration>>().Value);
        services.AddSingleton<UBind.Application.ThirdPartyDataSets.NfidUpdaterJob.INfidConfiguration>(
            resolver => resolver.GetRequiredService<IOptions<UBind.Application.ThirdPartyDataSets.NfidUpdaterJob.NfidConfiguration>>().Value);
        services.AddSingleton<DbMonitoringConfiguration>(
           resolver => resolver.GetRequiredService<IOptions<DbMonitoringConfiguration>>().Value);
        services.AddSingleton<IContentSecurityPolicyConfiguration>(
                resolver => resolver.GetRequiredService<IOptions<ContentSecurityPolicyConfiguration>>().Value);

        services.AddSingleton<IFtpConfiguration>(resolver => resolver.GetRequiredService<IOptions<FtpConfiguration>>().Value);

        services.AddSingleton<IAdditionalPropertyBackgroundJobSetting>(
            resolver => resolver.GetRequiredService<IOptions<AdditionalPropertyBackgroundJobSetting>>().Value);
        services.AddSingleton<HangfireDashboardAuthorizationFilter>();
        services.AddSingleton(resolver => new HangfireUserAuthorizationFilter(resolver.GetRequiredService<IServiceProvider>()));
        services.AddScoped<ISystemAlertRepository, SystemAlertRepository>();
        services.AddSingleton<IEmailComposer, EmailComposer>();
        services.AddScoped<IPortalSettingRepository, PortalSettingRepository>();
        services.AddScoped<ISystemAlertService, SystemAlertService>();
        services.AddScoped<IApplicationIntegrationRequestService, ApplicationIntegrationRequestService>();
        services.AddScoped<IIntegrationEventReplayService, IntegrationEventReplayService>();
        services.AddScoped<IRenewalInvitationService, RenewalInvitationService>();
        services.AddSingleton<ICalculationJsonSanitizer, CalculationJsonSanitizer>();
        services.AddSingleton<IAsymmetricEncryptionService, AsymmetricEncryptionService>();
        services.AddScoped<IDataTableContentDbFactory, DataTableContentDbFactory>();
        services.AddScoped<IDataTableDefinitionRepository, DataTableDefinitionRepository>();
        services.AddScoped<IDataTableContentRepository, DataTableContentRepository>();
        services.AddScoped<IDataTableContentDbConfiguration>(
            context => new DataTableContentDbConfiguration(this.Configuration.GetConnectionString("UBind")));

        // Command Injection
        services.AddTransient<ICqrsMediator, CqrsMediator>();
        services.AddSingleton(c => new QuoteCalculationRequestTracker());
        services.RegisterCustomPipelines();

        // HangFire and Mediator Service.
        services.AddScoped<IHangfireCqrsJobService, HangfireCqrsJobService>();
        services.AddScoped<IHangfireCqrsService, HangfireCqrsService>();
        services.AddSingleton<ITimeZoneResolver>(_ => new TimeZoneResolver());

        // State Machines and related repository injection.
        services.AddScoped<IUpdaterJob, UBind.Application.ThirdPartyDataSets.RedBookUpdaterJob.UpdaterJobStateMachine>();
        services.AddScoped<IUpdaterJob, UBind.Application.ThirdPartyDataSets.GlassGuideUpdaterJob.UpdaterJobStateMachine>();
        services.AddScoped<IUpdaterJob, UBind.Application.ThirdPartyDataSets.GnafUpdaterJob.UpdaterJobStateMachine>();
        services.AddScoped<IUpdaterJob, UBind.Application.ThirdPartyDataSets.NfidUpdaterJob.UpdaterJobStateMachine>();
        services.AddScoped<ThirdPartyDataSetsDbContext>(ctx => new ThirdPartyDataSetsDbContext(this.Configuration.GetConnectionString("ThirdPartyDataSets")));
        services.AddScoped<IThirdPartyDataSetsDbConfiguration>(ctx => new ThirdPartyDataSetsDbConfiguration(this.Configuration.GetConnectionString("ThirdPartyDataSets")));
        services.AddScoped<IThirdPartyDataSetsDbObjectFactory, ThirdPartyDataSetsDbObjectFactory>();
        services.AddSingleton<IThirdPartyDataSetsSearchService, ThirdPartyDataSetsSearchService>(provider =>
        {
            var thirdPartyDataSetsConfiguration = provider.GetRequiredService<IThirdPartyDataSetsConfiguration>();
            var fileSystemService = provider.GetRequiredService<IFileSystemService>();
            var logger = provider.GetRequiredService<ILogger<ThirdPartyDataSetsSearchService>>();

            return new ThirdPartyDataSetsSearchService(thirdPartyDataSetsConfiguration, fileSystemService, logger);
        });
        services.AddSingleton<IQueryFactory, QueryFactory>();
        services.AddScoped<IFtpClientFactory, FtpClientFactory>();

        services.AddScoped<IStateMachineJobsRepository, StateMachineJobsRepository>();
        services.AddScoped<IRedBookRepository, RedBookRepository>();
        services.AddScoped<IGlassGuideRepository, GlassGuideRepository>();
        services.AddScoped<IGnafRepository, GnafRepository>();
        services.AddScoped<INfidRepository, NfidRepository>();
        services.AddScoped<IUpdaterJobFactory, UpdaterJobFactory>();
        services.AddScoped<IDataDownloaderService, DataDownloaderService>();
        services.AddScoped<IDelimiterSeparatedValuesService, DelimiterSeparatedValuesService>();
        services.AddScoped<IDelimiterSeparatedValuesFileProvider, DelimiterSeparatedValuesFileProvider>();

        // Filesystem storage : use Local storage if configured, otherwise fallback to OneDrive/MsGraph
        services.AddScoped<IGraphClientFileRepository, GraphClient>();
        services.AddScoped<IFilesystemFileRepository>(x =>
        {
            var filesystemStorageConfiguration = x.GetRequiredService<IFilesystemStorageConfiguration>();
            if (filesystemStorageConfiguration != null
                && filesystemStorageConfiguration.StorageProvider == FilesystemStorageProvider.Local)
            {
                return new LocalFilesystemFileRepository(x.GetRequiredService<ILocalFilesystemStorageConfiguration>());
            }
            else
            {
                return x.GetRequiredService<IGraphClientFileRepository>();
            }
        });

        services.AddUbindApplication();
        services.AddUbindDomainMediators();

        // Service configuration for authentication
        services.AddSingleton<IAuthConfiguration>(
            resolver => resolver.GetRequiredService<IOptions<AuthConfiguration>>().Value);

        services.AddScoped<ISystemEmailTemplateRepository, SystemEmailTemplateRepository>();
        services.AddScoped<IEmailTemplateService, EmailTemplateService>();
        services.AddScoped<IAccessTokenService, AccessTokenService>();

        services.AddScoped<UBind.Application.User.IUserService, UBind.Application.User.UserService>();
        services.AddScoped<Domain.Services.IUserService, Domain.Services.UserService>();

        // Service for users
        services.AddSingleton<IErrorNotificationService, ErrorNotificationService>();
        services.AddScoped<IUserActivationInvitationService, UserActivationInvitationService>();
        services.AddScoped<IUserPasswordResetInvitationService, UserPasswordResetInvitationService>();
        services.AddSingleton<IAuthorizationHandler, UserTypeHandler>();
        services.AddScoped<IAuthorizationHandler, PermissionRequirementHandler>();
        services.AddScoped<ITenantRepository, TenantRepository>();
        services.AddScoped<ITenantService, TenantService>();
        services.AddScoped<IOrganisationService, OrganisationService>();
        services.AddScoped<Domain.Services.IProductService, Domain.Services.ProductService>();
        services.AddScoped<PortalAppContentSecurityPolicyService>();
        services.AddScoped<FormsAppContentSecurityPolicyService>();
        services.AddSingleton<IContentSecurityPolicyServiceFactory, ContentSecurityPolicyServiceFactory>();
#pragma warning disable CS0618 // Type or member is obsolete
        services.AddScoped<IPortalRepository, PortalRepository>();
#pragma warning restore CS0618 // Type or member is obsolete
        services.AddScoped<IPortalSettingsService, PortalSettingsService>();
        services.AddScoped<IPortalService, PortalService>();
        services.AddSingleton<IBaseUrlResolver, BaseUrlResolver>();
        services.AddScoped<IPortalSettingsService, PortalSettingsService>();
        services.AddScoped<IFeatureSettingRepository, FeatureSettingRepository>();
        services.AddScoped<IFeatureSettingService, FeatureSettingService>();
        services.AddScoped<IPersonService, PersonService>();
        services.AddSingleton<IMachineInformationService, MachineInformationService>();
        services.AddScoped<IUserSessionService, UserSessionService>();
        services.AddScoped<ITinyUrlService, TinyUrlService>();

        services.AddSwaggerGen(options =>
        {
            options.SwaggerDoc("v1", new OpenApiInfo
            {
                Version = "v1",
                Title = "UBind API Services",
                Description = "API services available in uBind that you can execute.",
            });
            var xmlFile = $"{System.Reflection.Assembly.GetExecutingAssembly().GetName().Name}.xml";
            var xmlPath = Path.Combine(AppContext.BaseDirectory, xmlFile);
            options.IncludeXmlComments(xmlPath);
            options.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
            {
                Description = "JWT Authorization header using the Bearer scheme. Example: \"Bearer {token}. You Can get the token in the Authorization request.\"",
                Name = "Authorization",
                In = ParameterLocation.Header,
                Type = SecuritySchemeType.ApiKey,
            });
            options.AddSecurityRequirement(new OpenApiSecurityRequirement
            {
                {
                    new OpenApiSecurityScheme
                    {
                        Reference = new OpenApiReference
                        {
                            Type = ReferenceType.SecurityScheme,
                            Id = "Bearer",
                        },
                        Scheme = "oauth2",
                        Name = "Bearer",
                        In = ParameterLocation.Header,
                    },
                    new List<string>()
                },
            });
            options.OperationFilter<FileUploadOperation>();
            options.OperationFilter<HideParameterFilter>();
            options.CustomSchemaIds(x => x.FullName);
            options.SchemaFilter<EnumSchemaFilter>();
            options.UseAllOfToExtendReferenceSchemas();
        });
        services.AddSwaggerGenNewtonsoftSupport();

        // DBA related objects
        services.AddScoped<IDbaRepository<UBindDbContext>, UBindDbaRepository>();
        services.AddScoped<IDbaScheduler<UBindDbContext>, UBindDbaScheduler>();
        services.AddScoped<IDbaRepository<ThirdPartyDataSetsDbContext>, ThirdPartyDataSetsDbaRepository>();
        services.AddScoped<IDbaScheduler<ThirdPartyDataSetsDbContext>, ThirdPartyDataSetsDbaScheduler>();
        services.AddScoped<IDbaRepository<DvaDbContext>, DvaDbaRepository>();
        services.AddScoped<IDbaScheduler<DvaDbContext>, DvaDbaScheduler>();

        // FlexCel related classes
        services.AddSingleton<ISpreadsheetPoolService, SpreadsheetPoolService>();
        services.AddScoped<ICalculationService, SpreadsheetCalculationService>();

        services.AddScoped<IEmailQueryService, EmailQueryService>();
        services.AddScoped<IEmailService, EmailService>();
        services.AddScoped<IEmailRepository, EmailRepository>();
        services.AddScoped<IClaimNumberRepository, ClaimNumberRepository>();
        services.AddScoped<ISmsRepository, SmsRepository>();
        services.AddScoped<IDkimHeaderVerifier, DkimHeaderVerifier>();

        // Roles and Permissions
        services.AddScoped<IRoleRepository, RoleRepository>();
        services.AddScoped<IRoleService, RoleService>();
        services.AddSingleton<IDefaultRolePermissionsRegistry, DefaultRolePermissionsRegistry>();
        services.AddSingleton<IRoleTypePermissionsRegistry, RoleTypePermissionsRegistry>();
        services.AddSingleton<IDefaultRoleNameRegistry, DefaultRoleNameRegistry>();

        // Events
        services.AddSingleton<ISystemEventTypePersistenceDurationRegistry, SystemEventTypePersistenceDurationRegistry>();

        // Authorisation and data restriction
        services.AddScoped<IAuthorisationService, AuthorisationService>();
        services.AddScoped<IUserAuthorisationService, UserAuthorisationService>();
        services.AddScoped<IRoleAuthorisationService, RoleAuthorisationService>();
        services.AddScoped<IOrganisationAuthorisationService, OrganisationAuthorisationService>();
        services.AddScoped<IAdditionalPropertyAuthorisationService, AdditionalPropertyAuthorisationService>();

        // Configure dependency injection for deletion service
        services.AddScoped<IDeletionService, DataDeletionService>();
        services.AddScoped<ISystemEventDeletionService, SystemEventDeletionService>();

        // Configure Hangfire Console context
        services.AddScoped<IStorageConnection>(c => JobStorage.Current.GetConnection());
        services.AddScoped<IProgressLogger, HangfireProgressLogger>();
        services.AddScoped<IProgressLoggerFactory, ProgressLoggerFactory>();
        services.AddScoped<IQuoteDeletionManager>(s => new QuoteDeletionManager(ubindSqlServerConnectionString));
        services.AddScoped<IMiniProfilerDeletionManager>(s => new MiniProfilerDeletionManager(ubindSqlServerConnectionString));

        // Configure import service
        services.AddScoped<IMappingTransactionService, MappingTransactionService>();
        services.AddScoped<IImportService, ImportService>();

        // Configure imported form data patcher
        services.AddScoped<IPatchService, PolicyDataPatchService>();
        services.AddSingleton<IFormDataFieldFormatterFactory, FormDataFieldFormatterFactory>();
        services.AddSingleton<IFormDataPrettifier, FormDataPrettifier>();

        // Automations
        services.AddScoped<IAutomationConfigurationProvider, AutomationConfigurationProvider>();
        services.AddScoped<IAutomationConfigurationModelProvider, AutomationConfigurationModelProvider>();
        services.AddScoped<IAutomationService, AutomationService>();
        services.AddScoped<IAutomationExtensionPointService, AutomationExtensionPointService>();
        services.AddScoped<IAsynchronousActionHandler, AsynchronousActionHandler>();
        services.AddScoped<IActionRunner, ActionRunner>();
        services.AddScoped<IEntityQueryService, AutomationEntityQueryService>();
        services.AddScoped<IAutomationConfigurationValidator, AutomationConfigurationValidator>();

        // Lucene Implementations
        services.AddScoped<ILuceneRepository<IQuoteSearchIndexWriteModel, IQuoteSearchResultItemReadModel, QuoteReadModelFilters>, LuceneQuoteRepository>();
        services.AddScoped<ILuceneRepository<IPolicySearchIndexWriteModel, IPolicySearchResultItemReadModel, PolicyReadModelFilters>, LucenePolicyRepository>();
        services.AddScoped<ILuceneDocumentBuilder<IQuoteSearchIndexWriteModel>, QuoteLuceneDocumentBuilder>();
        services.AddScoped<ILuceneDocumentBuilder<IPolicySearchIndexWriteModel>, PolicyLuceneDocumentBuilder>();

        // Job Scheduler
        // Add Quartz services
        services.AddSingleton<IJobFactory, SingletonJobFactory>();
        services.AddSingleton<ISchedulerFactory, StdSchedulerFactory>();
        services.AddScoped<IBackgroundJobService, BackgroundJobService>();
        services.AddScoped<IAutomationPeriodicTriggerScheduler, AutomationPeriodicTriggerScheduler>();
        services.AddScoped<IEntityScheduler<Policy>, PolicyScheduler>();
        services.AddScoped<IEntityScheduler<Domain.Aggregates.Quote.Quote>, QuoteScheduler>();
        services.AddScoped<IRedbookUpdaterScheduler, RedbookUpdaterScheduler>();
        services.AddScoped<IGlassGuideUpdaterScheduler, GlassGuideUpdaterScheduler>();
        services.AddScoped<IEntityScheduler<Domain.Events.SystemEvent>, SystemEventDeletionScheduler>();
        services.AddScoped<ISystemEventRelationshipTableMaintenanceScheduler, SystemEventRelationshipTableMaintenanceScheduler>();

        // Add our job
        services.AddSingleton<UpdateQuoteSearchIndexJob>();
        services.AddSingleton<UpdatePolicySearchIndexJob>();
        services.AddSingleton<RegenerateQuoteSearchIndexJob>();
        services.AddSingleton<RegeneratePolicySearchIndexJob>();

        // startup jobs
        services.AddScoped<IStartupJobRunner, StartupJobRunner>();
        services.AddScoped<IStartupJobRegistry, StartupJobRegistry>();

        // lucene search indexing
        services.AddSingleton<ILuceneDirectoryConfiguration>(
            resolver => resolver.GetRequiredService<IOptions<LuceneDirectoryConfiguration>>().Value);
        services.AddSingleton(s =>
        {
            var luceneQuotesConfig = s.GetRequiredService<ILuceneDirectoryConfiguration>();
            return new JobSchedule(
                jobType: typeof(UpdateQuoteSearchIndexJob),
                null,
                cronExpression: luceneQuotesConfig.Quote.IndexGenerationCronExpression,
                luceneQuotesConfig.Quote.IndexGenerationStartupDelayInSeconds); // Adding delay here in seconds, so we can run some manual scripts and some prep work before lucene indexing kicks in.
        });
        services.AddSingleton(s =>
        {
            var luceneQuotesConfig = s.GetRequiredService<ILuceneDirectoryConfiguration>();
            return new JobSchedule(
               jobType: typeof(UpdatePolicySearchIndexJob),
               null,
               cronExpression: luceneQuotesConfig.Policy.IndexGenerationCronExpression,
               luceneQuotesConfig.Policy.IndexGenerationStartupDelayInSeconds); // Adding delay here in seconds, so we can run some manual scripts and some prep work before lucene indexing kicks in.
        });
        services.AddSingleton(s =>
        {
            var luceneQuotesConfig = s.GetRequiredService<ILuceneDirectoryConfiguration>();
            return new JobSchedule(
               jobType: typeof(RegenerateQuoteSearchIndexJob),
               null,
               cronExpression: luceneQuotesConfig.Quote.IndexRegenerationCronExpression,
               luceneQuotesConfig.Quote.IndexRegenerationStartupDelayInSeconds);
        });
        services.AddSingleton(s =>
        {
            var luceneQuotesConfig = s.GetRequiredService<ILuceneDirectoryConfiguration>();
            return new JobSchedule(
               jobType: typeof(RegeneratePolicySearchIndexJob),
               null,
               cronExpression: luceneQuotesConfig.Policy.IndexRegenerationCronExpression,
               luceneQuotesConfig.Policy.IndexRegenerationStartupDelayInSeconds);
        });

        // Workbook field factory for parsing workbooks and creating the right field type
        services.AddSingleton<IWorkbookFieldFactory, WorkbookFieldFactory>();
        services.AddSingleton<
            IAttributeObjectPropertyMapRegistry<WorkbookTableColumnNameAttribute>,
            AttributeObjectPropertyMapRegistry<WorkbookTableColumnNameAttribute>>();
        services.AddSingleton<
            IAttributeObjectPropertyMapRegistry<WorkbookTableSectionPropertyNameAttribute>,
            AttributeObjectPropertyMapRegistry<WorkbookTableSectionPropertyNameAttribute>>();
        services.AddSingleton<
            IWorkbookProductComponentConfigurationReader,
            WorkbookProductComponentConfigurationReader>();
        services.AddSingleton<IFieldSerializationBinder, FieldSerializationBinder>();

        // Web helpers
        services.AddScoped<ICustomerHelper, CustomerHelper>();

        // Add authorization policy
        services.AddAuthorization(options =>
        {
            foreach (var map in RolePolicyMapping.Mapping)
            {
                options.AddPolicy(
                    map.Key,
                    policy =>
                    {
                        policy.Requirements.Add(
                         new RoleRequirement(map.Value.ToArray()));
                    });
            }

            var permissions = Enum<Permission>.GetValues();
            foreach (var permission in permissions)
            {
                options.AddPolicy(
                     permission.ToString(),
                     policy => policy.Requirements.Add(new PermissionRequirement(permission)));
            }
        });

        // DataTable
        services.AddSingleton<IDataTableDataTypeSqlSettingsRegistry, DataTableDataTypeSqlSettingsRegistry>();

        // Schema
        services.AddSingleton<ISchemaConfiguration>(
            resolver => resolver.GetRequiredService<IOptions<SchemaConfiguration>>().Value);

        // Dapper
        SqlMapper.AddTypeHandler(LocalDateHandler.Default);

        // Hangfire
        GlobalConfiguration.Configuration.UseConsole();
        GlobalConfiguration.Configuration.SetDataCompatibilityLevel(CompatibilityLevel.Version_180);
        var hangfireOptions = new HangfireConfiguration();
        this.Configuration.GetSection("HangFire").Bind(hangfireOptions);
        var hangfireStorageSelection = hangfireOptions.Storage.ToEnumOrThrow<ComponentStorage>();
        services.AddHangfire(config =>
        {
            switch (hangfireStorageSelection)
            {
                case ComponentStorage.SqlServer:
                    config.UseSqlServerStorage(ubindSqlServerConnectionString);
                    break;
                case ComponentStorage.Redis:
                    var redisConnectionString = hangfireOptions.Redis.ConnectionString ?? redisGlobalConfig.ConnectionString;
                    var redisStorageOptions = new RedisStorageOptions()
                    {
                        Prefix = string.IsNullOrEmpty(hangfireOptions.Redis.Prefix)
                            ? "ubind:{hangfire-1}:"
                            : hangfireOptions.Redis.Prefix,
                    };
                    int maxSucceededListLength = hangfireOptions.Redis.MaxSucceededListLength != 0
                            ? hangfireOptions.Redis.MaxSucceededListLength
                            : redisStorageOptions.MaxSucceededListLength;
                    redisStorageOptions.MaxSucceededListLength = maxSucceededListLength;
                    config.UseRedisStorage(
                        redisConnectionString,
                        redisStorageOptions);
                    break;
                default:
                    throw new ErrorException(
                        Errors.General.UnexpectedEnumValue(hangfireStorageSelection, typeof(ComponentStorage)));
            }
        });
        services.AddHangfireServer(x =>
        {
            x.ServerName = Environment.MachineName;
            x.ShutdownTimeout = TimeSpan.FromSeconds(35);
        });
        services.AddHangfireConsoleExtensions();

        services.ConfigureThirdPartyDataSets(this.Configuration);

        // initialize UBind clients' configuration services
        this.InitializeClientsConfigurationServices(ref services, this.Configuration);

        services.AddHostedService<QuartzHostedService>();
        services.AddHealthChecks()
            .AddCheck<LuceneHealthCheck>("lucene_index")
            .AddCheck<DatabaseConnectionHealthCheck>("ubind_database_connection");

        // Add SAML SSO services.
        this.LoadSamlServiceProviderCertificate(services);
        services.AddSaml();
        services.AddScoped<ISamlConfigurationResolver, AuthenticationMethodSamlConfigurationResolver>();

        // Add the Redis cache for IDistributedCache - this is used by ComponentSpace SAML
        services.AddStackExchangeRedisCache(options =>
        {
            options.Configuration = this.redisGlobalConfig.ConnectionString;
            options.InstanceName = this.redisGlobalConfig.Prefix;
        });

        this.ConfigureDotLiquid();
    }

    /// <summary>
    /// This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
    /// </summary>
    /// <param name="app">The application builder.</param>
    /// <param name="env">The hosting environment.</param>
    /// <param name="serviceProvider">The ASP.NET Core service provider.</param>
    public void Configure(IApplicationBuilder app, IWebHostEnvironment env, IServiceProvider serviceProvider, IHostApplicationLifetime appLifetime)
    {
        this.InstantiateStaticDependencies(app);
        this.InitDatabase();
        app.UseJwtKeyRotation();
        this.InitGemBox();

        if (this.miniProfilerConfiguration.Enabled)
        {
            app.UseMiniProfiler();
        }

        var dashboardOptions = new DashboardOptions
        {
            Authorization = new IDashboardAuthorizationFilter[]
            {
                    app.ApplicationServices.GetRequiredService<HangfireDashboardAuthorizationFilter>(),
                    app.ApplicationServices.GetRequiredService<HangfireUserAuthorizationFilter>(),
            },

            // Setting IgnoreAntiforgeryToken to true based on current security measures:
            // 1. JWT Authentication: Access to the Hangfire dashboard is controlled via JWT tokens,
            //    providing stateless authentication. Each request must carry a valid JWT, making
            //    CSRF attacks less feasible as there's no session state to exploit.
            // 2. HttpOnly Cookie: The JWT token is stored in an HttpOnly cookie, making it
            //    inaccessible to JavaScript and thus reducing the risk of XSS attacks.
            // 3. Secure Cookie Attribute: Cookies are flagged as Secure, ensuring they are transmitted
            //    only over HTTPS, protecting them from interception over unsecured connections.
            // 4. SameSite=Strict: The SameSite=Strict cookie attribute is used, ensuring the cookie is
            //    sent only in requests originating from the same domain, effectively mitigating CSRF risks.
            // Note: While these measures significantly reduce the necessity for CSRF protection in our
            // current setup, it might still be beneficial to consider enabling CSRF tokens in the future
            // as an added layer of defense, aligning with security best practices.
            IgnoreAntiforgeryToken = true,
        };
        app.UseHangfireDashboard("/hangfire", dashboardOptions);
        app.UseAuthentication();

        // Configure Australian culture (regardless of server locale).
        var cultureInfo = new CultureInfo("en-AU");
        app.UseRequestLocalization(new RequestLocalizationOptions
        {
            DefaultRequestCulture = new RequestCulture(cultureInfo),
            SupportedCultures = new List<CultureInfo> { cultureInfo },
            SupportedUICultures = new List<CultureInfo> { cultureInfo },
        });
        GlobalJobFilters.Filters.Add(new PreserveExistingExpirationAttribute());
        GlobalConfiguration.Configuration.UseActivator(new HangfireActivator(serviceProvider));
        var logger = app.ApplicationServices.GetRequiredService<ILogger<PeriodicAutomationJobStateFilterEventHandler>>();
        GlobalConfiguration.Configuration.UseFilter(new PeriodicAutomationJobStateFilterEventHandler(logger));
        var config = app.ApplicationServices.GetRequiredService<IESystemAlertConfiguration>();
        var smtp = app.ApplicationServices.GetRequiredService<ISmtpClientConfiguration>();
        var baseUrl = app.ApplicationServices.GetRequiredService<IInternalUrlConfiguration>();
        GlobalConfiguration.Configuration.UseFilter(new HangfireNotificationAttribute(config, smtp, baseUrl));
        var jsonSerializerSettings = new JsonSerializerSettings
        {
            TypeNameHandling = TypeNameHandling.All,
        };
        jsonSerializerSettings.ConfigureForNodaTime(DateTimeZoneProviders.Tzdb);
        GlobalConfiguration.Configuration.UseSerializerSettings(jsonSerializerSettings);

        if (!env.IsDevelopment())
        {
            app.UseHsts();
        }

        app.Use(async (context, next) =>
        {
            context.Response.Headers.Add("X-XSS-Protection", "1; mode=block");

            context.Response.Headers.Add(
             "X-Content-Type-Options", "nosniff");

            if (!context.Response.Headers.ContainsKey("Cache-Control"))
            {
                context.Response.Headers.Add(
                "Cache-control", "no-cache , no-store");
            }

            context.Response.Headers.Add(
               "Pragma", "no-cache");

            // Some automations create embeddings and iframes, as seen in quay/trades
            var excludePaths = new List<string> { "/index.html", "/automation", "/principalFinance" };
            if (!excludePaths.Any(str => context.Request.Path.ToString().Contains(str)))
            {
                context.Response.Headers.Add("Content-Security-Policy", "frame-ancestors 'none'");
                context.Response.Headers.Add("X-Frame-Options", "SAMEORIGIN");
                context.Response.Headers.Add("Permissions-Policy", "frame-ancestors 'none'");
            }

            context.Response.Headers.Add("Referrer-Policy", "no-referrer");
            if (context.Request.IsHttps
                || context.Request.Host.ToString().StartsWith("localhost", StringComparison.InvariantCultureIgnoreCase)) //// Ignore for localhost
            {
                await next();
            }
            else
            {
                var httpsUrl = Uri.UriSchemeHttps + Uri.SchemeDelimiter + context.Request.Host + context.Request.Path + context.Request.QueryString;
                context.Response.Redirect(httpsUrl);
            }
        });

        app.UseProblemDetails();
        app.UseSwagger(c => c.SerializeAsV2 = true);
        app.UseSwaggerUI(c =>
        {
            c.SwaggerEndpoint("../swagger/v1/swagger.json", "UBind API V1");
            c.RoutePrefix = "docs";
        });

        app.UseSentryTracing();

        // this call should come before app.UseMiddleware<SessionManagementMiddleware>()
        var authConfiguration = serviceProvider.GetService<IAuthConfiguration>();
        app.UseCors(builder => builder
           .WithOrigins(authConfiguration.PermittedCorsOrigins)
           .AllowAnyHeader()
           .AllowAnyMethod());

        app.UseHttpsRedirection();
        app.UseDefaultFiles();
        app.UseMiddleware<AccessLogMiddleware>();
        app.UseMiddleware<ContentSecurityPolicyMiddleware>();

        // Serve the static files in the wwwroot folder
        app.UseStaticFiles();
        app.UseResponseCaching();
        app.UseSerilogRequestLogging();

        // Configure output of ionic portal app build
        this.UseStaticFilesForInjection(app, env);
        this.UseStaticFilesForFormsApp(app, env);
        this.UseStaticFilesForPortalApp(app, env);

        // UseRouting and UseEndpoints must be called after UseStaticFiles
        app.UseRouting();
        app.UseAuthorization();
        app.UseMiddleware<RequestRateLimitMiddleware>();
        app.UseMiddleware<SessionManagementMiddleware>();
        app.UseMiddleware<RequestIntentMiddleware>();
        app.UseMiddleware<FallbackMiddleware>();
        app.UseEndpoints(endpoints =>
        {
            endpoints.MapControllerRoute(
                name: "default",
                pattern: "{controller=Home}/{action=Index}/{id?}");
            endpoints.MapHealthChecks("/api/v1/health/node", new HealthCheckOptions
            {
                ResponseWriter = this.WriteHealthCheckResponse,
            });
        });

        app.UseSentryTracing();
        var startedTimeInUtc = DateTime.UtcNow;
        appLifetime.ApplicationStopped.Register(() =>
        {
            var duration = DateTime.UtcNow - startedTimeInUtc;
            Log.Information($"Application shutdown complete after running for" +
                $" {TimeSpan.FromSeconds(duration.TotalSeconds).Humanize()}.");
            Log.Information("To determine if the shutdown is due to an app recycle, " +
                "check Windows Event Viewer (event source: WAS), for logged recycle events.");
            Log.CloseAndFlush();
        });
        this.app = app;

        this.InitializeBackgroundJobs(serviceProvider);

        Log.Logger.Information("Startup DONE!");
    }

    private void InitializeBackgroundJobs(IServiceProvider serviceProvider)
    {
        this.StartupJobInitialization(serviceProvider);
        this.ScheduleInitialization();

        var triggerService = serviceProvider.GetService<IAutomationPeriodicTriggerScheduler>();
        BackgroundJob.Schedule(() => triggerService.RegisterPeriodicTriggerJobs(), TimeSpan.FromMinutes(2));

        var policyScheduler = serviceProvider.GetService<IEntityScheduler<Policy>>();
        BackgroundJob.Schedule(() => policyScheduler.RegisterStateChangeJob(), TimeSpan.FromMinutes(2));

        var quoteScheduler = serviceProvider.GetService<IEntityScheduler<Domain.Aggregates.Quote.Quote>>();
        BackgroundJob.Schedule(() => quoteScheduler.RegisterStateChangeJob(), TimeSpan.FromMinutes(2));

        var redbookUpdater = serviceProvider.GetService<IRedbookUpdaterScheduler>();
        BackgroundJob.Schedule(() => redbookUpdater.RegisterStateChangeJob(), TimeSpan.FromMinutes(2));

        var glassGuideUpdater = serviceProvider.GetService<IGlassGuideUpdaterScheduler>();
        BackgroundJob.Schedule(() => glassGuideUpdater.RegisterStateChangeJob(), TimeSpan.FromMinutes(2));

        var systemEventScheduler = serviceProvider.GetService<IEntityScheduler<Domain.Events.SystemEvent>>();
        BackgroundJob.Schedule(() => systemEventScheduler.RegisterStateChangeJob(), TimeSpan.FromMinutes(2));

        var systemEventAndRelationshipScheduler = serviceProvider.GetService<ISystemEventRelationshipTableMaintenanceScheduler>();
        BackgroundJob.Schedule(() => systemEventAndRelationshipScheduler.RegisterStateChangeJob(), TimeSpan.FromMinutes(2));

        var dbMonitoringConfiguration = serviceProvider.GetService<DbMonitoringConfiguration>();
        if (dbMonitoringConfiguration != null)
        {
            serviceProvider.InitializeDbaSchedules(Log.Logger);
        }
    }

    private void StartupJobInitialization(IServiceProvider serviceProvider)
    {
        var startupJobRunner = (IStartupJobRunner)serviceProvider.GetService(typeof(IStartupJobRunner));
        startupJobRunner.RunJobs().GetAwaiter().GetResult();
    }

    private void UseStaticFilesForInjection(IApplicationBuilder app, IWebHostEnvironment env)
    {
        string injectionFilesPath = AppFilesHelper.GetInjectionStaticFilesPath(env);
        Directory.CreateDirectory(injectionFilesPath);
        var physicalFileProvider = new PhysicalFileProvider(injectionFilesPath);
        app.UseStaticFiles(new StaticFileOptions
        {
            FileProvider = physicalFileProvider,
            RequestPath = new PathString(string.Empty),
            OnPrepareResponse = (context) =>
            {
                context.Context.Response.Headers[HeaderNames.CacheControl] = CacheDurations.NoCache;
            },
        });
    }

    private void UseStaticFilesForFormsApp(IApplicationBuilder app, IWebHostEnvironment env)
    {
        string appFilesPath = AppFilesHelper.GetFormsAppStaticFilesPath(env);
        Directory.CreateDirectory(appFilesPath);
        var physicalFileProvider = new PhysicalFileProvider(appFilesPath);
        app.UseStaticFiles(new StaticFileOptions
        {
            FileProvider = physicalFileProvider,
            RequestPath = new PathString(string.Empty),
            OnPrepareResponse = (context) =>
            {
                context.Context.Response.Headers[HeaderNames.CacheControl] = CacheDurations.OneDayCache;
            },
        });
    }

    private void UseStaticFilesForPortalApp(IApplicationBuilder app, IWebHostEnvironment env)
    {
        string appFilesPath = AppFilesHelper.GetPortalAppStaticFilesPath(env);
        Directory.CreateDirectory(appFilesPath);
        var physicalFileProvider = new PhysicalFileProvider(appFilesPath);
        app.UseStaticFiles(new StaticFileOptions
        {
            FileProvider = physicalFileProvider,
            RequestPath = "/portal",
            OnPrepareResponse = (ctx) =>
            {
                var requestPath = ctx.Context.Request.Path;
                var staticFilePathSegmentsToCache = new string[] { "/svg/", "/imgs/", "/icon/" };
                if (requestPath.HasValue && staticFilePathSegmentsToCache.Any(requestPath.Value.Contains))
                {
                    ctx.Context.Response.Headers[HeaderNames.CacheControl] = CacheDurations.MaxDuration;
                }
            },
        });
    }

    private void ScheduleInitialization()
    {
        BackgroundJob.Enqueue<Initializer>(initializer => initializer.SetupFilesystemStorage());
    }

    private void ScheduleMicrosoftGraphAuthenticationTokenCaching()
    {
        RecurringJob.AddOrUpdate<ICachingAuthenticationTokenProvider>(
            $"{nameof(Startup.ScheduleMicrosoftGraphAuthenticationTokenCaching)} Job",
            authenticator => authenticator.CacheAuthenticationTokenAsync(),
            "*/7 * * * *" /* Every 30 minutes */);
    }

    private void InitializeClientsConfigurationServices(ref IServiceCollection services, IConfigurationRoot configuration)
    {
        // DVA startup
        DvaWebStartup.ConfigureServices(ref services, configuration);
    }

    private void InitDatabase()
    {
        var automaticMigrationConfiguration = this.Configuration
            .GetSection(nameof(AutomaticMigrationConfiguration))
            .Get<AutomaticMigrationConfiguration>();

        // Trigger database creation before hangfire tries to use it.
        var initializer = new MigrateDatabaseToLatestVersion<UBindDbContext, Persistence.Migrations.Configuration>(true);
        var allowUBindMigration = automaticMigrationConfiguration != null && automaticMigrationConfiguration.UBind;
        Database.SetInitializer(allowUBindMigration ? initializer : null);
        var connectionString = this.Configuration.GetConnectionString("UBind");
        using (var dbContext = new UBindDbContext(connectionString))
        {
#if DEBUG
            // Creating the database ourselves proves faster than relying on Database.Initialize() alone.
            DatabaseHelper.CreateDatabaseIfNotExists(connectionString);
#endif
            dbContext.Database.Initialize(false);
        }
    }

    /// <summary>
    /// Some services are depended upon by statically so we want them instantiated immediately, not on
    /// first use. This is so that we can set a static reference to the service on the class that depends
    /// on it. This is typically on domain entities which cannot have services injected, so we use
    /// static references instead.
    /// </summary>
    private void InstantiateStaticDependencies(IApplicationBuilder app)
    {
        app.ApplicationServices.GetService<IDefaultRolePermissionsRegistry>();
        app.ApplicationServices.GetService<IRoleTypePermissionsRegistry>();
        app.ApplicationServices.GetService<IDefaultRoleNameRegistry>();
        app.ApplicationServices.GetService<ISystemEventTypePersistenceDurationRegistry>();
        app.ApplicationServices.GetService<IDataTableDataTypeSqlSettingsRegistry>();
    }

    private async Task WriteHealthCheckResponse(HttpContext context, HealthReport report)
    {
        var ipAddressWhitelistHelper = context.RequestServices.GetRequiredService<IIpAddressWhitelistHelper>();
        var headerConfiguration = context.RequestServices.GetRequiredService<ICustomHeaderConfiguration>();
        var clientIpAddress = context.GetClientIPAddress(headerConfiguration.ClientIpCode);
        var ipIsWhitelisted = ipAddressWhitelistHelper.IsWhitelisted(clientIpAddress);
        if (!ipIsWhitelisted)
        {
            context.Response.StatusCode = StatusCodes.Status403Forbidden;
            return;
        }

        var result = JsonConvert.SerializeObject(new
        {
            status = report.Status.ToString(),
            node = context.Connection.LocalIpAddress?.ToString(),
            checks = report.Entries.Select(e => new
            {
                name = e.Key,
                status = e.Value.Status.ToString(),
                description = e.Value.Description,
            }),
        });
        context.Response.ContentType = "application/json";
        if (report.Status != HealthStatus.Healthy)
        {
            context.Response.StatusCode = StatusCodes.Status503ServiceUnavailable;
        }

        await context.Response.WriteAsync(result);
    }

    private void LoadSamlServiceProviderCertificate(IServiceCollection services)
    {
        var samlConfig = new SamlConfiguration();
        this.Configuration.GetSection("Saml").Bind(samlConfig);
        var subjectName = samlConfig.ServiceProviderCertificateSubjectName;
        if (subjectName.IsNullOrEmpty())
        {
            Log.Logger.Warning("No subject name was provided for the SAML service provider certificate. "
                + "SAML signing of authentication requets will not be available");

            // register the configuration as a Singleton
            services.AddSingleton<ISamlConfiguration>(samlConfig);
            return;
        }

        // as per convention, the subject name should start with "CN=", so let's add it if it doesn't exist.
        if (!subjectName.StartsWith("CN="))
        {
            subjectName = "CN=" + subjectName;
        }

        X509Store machineStore = new X509Store(StoreName.My, StoreLocation.LocalMachine);
        machineStore.Open(OpenFlags.ReadOnly);
        X509Certificate2? certificate = machineStore.Certificates.FindFirstWithSubjectName(subjectName);
        machineStore.Close();

        if (certificate == null)
        {
            // the certificate was not found in the LocalMachine store, try the CurrentUser store
            X509Store personalStore = new X509Store(StoreName.My, StoreLocation.CurrentUser);
            personalStore.Open(OpenFlags.ReadOnly);
            certificate = personalStore.Certificates.FindFirstWithSubjectName(subjectName);
            personalStore.Close();
        }

        if (certificate == null)
        {
            throw new Exception($"SAML signing certificate with subject name '{samlConfig.ServiceProviderCertificateSubjectName}' "
                + "was not found. We tried looking in both the LocalMachine (machine wide) store, and the "
                + "CurrentUser store. In each store, we looked in the Personal\\Certificates location, but "
                + "we couldn't find the certificate. If you don't wish to support SAML, you can set the value for "
                + "ServiceProviderCertificateSubjectName to null or an empty string in appsettings.json.");
        }

        // make sure the cert has a private key so we can actually sign requests:
        if (!certificate.HasPrivateKey)
        {
            throw new Exception($"SAML signing certificate with subject name '{samlConfig.ServiceProviderCertificateSubjectName}' "
                + "was found, but it does not have a private key. We need a private key to sign SAML requests.");
        }

        // add the certificate to the configuration
        samlConfig.ServiceProviderCertificate = certificate;

        // load the private key to ensure that we can export both the public and private key
        try
        {
            using (var rsaPrivateKey = certificate.GetRSAPrivateKey())
            {
                samlConfig.ServiceProviderCertificateBase64
                    = Convert.ToBase64String(certificate.Export(X509ContentType.Pkcs12));
            }
        }
        catch (Exception ex)
        {
            throw new Exception("We were unable to load the private key for the SAML service provider certificate. "
                + "The private key must be part of the certificate. When importing the certificate into the Windows "
                + "Certificate store, it must be imported as \"exportable\" otherwise the private key will not be "
                + "available. On production machines, the key might need to be available from a key store also, with "
                + "specific permissions. See the inner exception for more details. " + ex.Message.WithDot(),
                ex);
        }

        // register the configuration as a Singleton
        services.AddSingleton<ISamlConfiguration>(samlConfig);
    }

    private void ConfigureMiniProfiler(IServiceCollection services, string ubindSqlServerConnectionString)
    {
        var authorisedIpAddresses = this.Configuration
                    .GetSection(nameof(IpWhitelistConfiguration))
                    .Get<IpWhitelistConfiguration>()
                    ?.AuthorizedIpAddresses;
        if (this.miniProfilerConfiguration.Enabled)
        {
            var miniProfilerStorageSelection = this.miniProfilerConfiguration.Storage.ToEnumOrThrow<ComponentStorage>();
            services.AddMiniProfiler(options =>
            {
                options.RouteBasePath = "/profiler";
                switch (miniProfilerStorageSelection)
                {
                    case ComponentStorage.SqlServer:
                        options.Storage = new SqlServerStorage(ubindSqlServerConnectionString);
                        break;
                    case ComponentStorage.Redis:
                        string prefix = string.IsNullOrWhiteSpace(this.miniProfilerConfiguration.Redis?.Prefix)
                            ? "ubind:{miniProfiler-1}:"
                            : this.miniProfilerConfiguration.Redis.Prefix;
                        IConnectionMultiplexer connectionMultiplexer;
                        if (this.miniProfilerConfiguration.Redis?.ConnectionString != null)
                        {
                            var redisConnectionString = this.miniProfilerConfiguration.Redis?.ConnectionString;
                            connectionMultiplexer = ConnectionMultiplexer.Connect(redisConnectionString);
                        }
                        else
                        {
                            // we'll use the global redis config
                            connectionMultiplexer = this.redisConnectionMultiplexer;
                        }

                        var redisStorage = new StackExchange.Profiling.Storage.RedisStorage((ConnectionMultiplexer)connectionMultiplexer);
                        redisStorage.ProfilerResultKeyPrefix = redisStorage.ProfilerResultKeyPrefix.Prepend(prefix);
                        redisStorage.ProfilerResultSetKey = redisStorage.ProfilerResultSetKey.Prepend(prefix);
                        redisStorage.ProfilerResultUnviewedSetKeyPrefix = redisStorage.ProfilerResultUnviewedSetKeyPrefix.Prepend(prefix);
                        redisStorage.CacheDuration = TimeSpan.FromMinutes(this.miniProfilerConfiguration.CacheDurationMinutes);
                        redisStorage.ResultListMaxLength = this.miniProfilerConfiguration.ResultListMaxLength;
                        options.Storage = redisStorage;
                        break;
                    default:
                        throw new ErrorException(
                            Errors.General.UnexpectedEnumValue(miniProfilerStorageSelection, typeof(ComponentStorage)));
                }

                // only allow access to view results, where the request is on the authorized IP address list
                options.ResultsAuthorize = request =>
                {
                    // The latest miniprofiler library updated decided to implement something which retreived the unviewed
                    // miniprofiler results from storage as part of each request, and deleted the old ones.
                    // This is plain dumb, as it slows down every request.
                    // It only does this for "authorised" users, but that could be developers who are testing,
                    // and we don't want their experience to be slow.
                    // So to get around this, we tell miniprofiler that the current request is not "authorised"
                    // unless it's for its profiler pages.
                    if (request.Path.Value.StartsWith("/profiler/"))
                    {
                        var clientIpAddress = request.HttpContext.Connection.RemoteIpAddress;
                        var isWhitelisted = authorisedIpAddresses?.Select(address => IPAddressRange.Parse(address))
                                ?.Any(range => range.Contains(clientIpAddress)) ?? false;
                        return isWhitelisted;
                    }

                    return false;
                };

                // By default we only profile requests from whitelisted IP addresses, because otherwise it would create a lot of
                // data, which would be too much to store in Redis.
                if (this.miniProfilerConfiguration.OnlyProfileWhitelistedIpAddresses)
                {
                    options.ShouldProfile = request =>
                    {
                        var clientIpAddress = request.HttpContext.Connection.RemoteIpAddress;
                        var isWhitelisted = authorisedIpAddresses?.Select(address => IPAddressRange.Parse(address))
                                ?.Any(range => range.Contains(clientIpAddress)) ?? false;
                        return isWhitelisted;
                    };
                }
            });
            if (this.miniProfilerConfiguration.ProfileEntityFramework6)
            {
                MiniProfilerEF6.Initialize();
            }
        }
    }

    private void ConfigureDotLiquid()
    {
        // Set the naming convention to CSharpNamingConvention to match the conventions used in our Liquid templates.
        Template.NamingConvention = new CSharpNamingConvention();
        Template.RegisterTag<RenderTag>("render");
        SystemEmailService.RegisterTemplateSafeTypes();
    }

    private void InitGemBox()
    {
        var gemBoxConfig = this.Configuration.GetSection("GemBox").Get<GemBoxConfiguration>();
        ComponentInfo.SetLicense(gemBoxConfig?.Document.LicenseKey);
        ComponentInfo.FreeLimitReached += (sender, e) => e.FreeLimitReachedAction = FreeLimitReachedAction.ContinueAsTrial;
    }

    private void HookRedisEvents()
    {
        this.redisConnectionMultiplexer.ConfigurationChanged += (sender, e) =>
            Log.Information($"Redis configuration changed: {e.EndPoint}");
        this.redisConnectionMultiplexer.ConfigurationChangedBroadcast += (sender, e) =>
            Log.Information($"Redis configuration changed via broadcast: {e.EndPoint}");
        this.redisConnectionMultiplexer.ConnectionFailed += (sender, e) =>
            Log.Error($"Redis connection failed: {e.EndPoint}, Conn Type: {e.ConnectionType}");
        this.redisConnectionMultiplexer.ConnectionRestored += (sender, e) =>
            Log.Information($"Redis connection restored: {e.EndPoint}, Conn Type: {e.ConnectionType}");
        this.redisConnectionMultiplexer.ErrorMessage += (sender, e) =>
            Log.Error($"Redis error: {e.EndPoint}, Error: {e.Message}");
        this.redisConnectionMultiplexer.HashSlotMoved += (sender, e) =>
            Log.Information($"Redis hash-slot relocated. Hash-Slot: {e.HashSlot}, Old: {e.OldEndPoint}, New: {e.NewEndPoint}");
        this.redisConnectionMultiplexer.InternalError += (sender, e) =>
            Log.Error($"Redis internal error: {e.EndPoint}, Error: {e.Exception.Message}, Origin: {e.Origin}, Conn Type: {e.ConnectionType}");
        this.redisConnectionMultiplexer.ServerMaintenanceEvent += (sender, e) =>
            Log.Information($"Redis maintenance: {e.RawMessage}, Start: {e.StartTimeUtc}, Received: {e.ReceivedTimeUtc}");
    }
}