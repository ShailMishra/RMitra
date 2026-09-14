using AspNetCoreRateLimit;
using System.Text;
using RM.DataRepository.CommonRequests.Helper;
using RM.DataRepository.DBDapper;
using RM.DataRepository.Health;
using RM.DataRepository.Kitchen;
using RM.DataRepository.MobileVerification;
using RM.DataRepository.Sms;
using RM.Infrastructure.CommonClass;
using RM_Backend_API.Swagger;
using RM_Backend_API.ActionFilters;
using RM_Backend_API.ExceptionFilter;
using RM_Backend_API.Health;
using RM_Backend_API.Middleware;
using RMitra.Api.ActionFilters;
using RMitra.Api.Security;
using RMitra.Application.Abstractions;
using RMitra.BuildingBlocks.Security;
using RMitra.Infrastructure;
using RMitra.Infrastructure.Options;
using RMitra.Infrastructure.Persistence;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.DataProtection;
using Microsoft.AspNetCore.Diagnostics.HealthChecks;
using Microsoft.AspNetCore.Http.Features;
using Microsoft.AspNetCore.HttpOverrides;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Server.Kestrel.Core;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using System.Reflection;
using System.Runtime.InteropServices;

namespace RM_Backend_API
{
    public class Startup
    {
        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        public IConfiguration Configuration { get; }

        public void ConfigureServices(IServiceCollection services)
        {
            services.AddCors(options =>
            {
                options.AddPolicy("AllowAllOrigins", builder =>
                {
                    builder.AllowAnyOrigin()
                           .AllowAnyHeader()
                           .AllowAnyMethod();
                });
            });

            services.Configure<ForwardedHeadersOptions>(options =>
            {
                options.ForwardedHeaders = ForwardedHeaders.XForwardedFor | ForwardedHeaders.XForwardedProto;
                options.KnownNetworks.Clear();
                options.KnownProxies.Clear();
            });

            var keysDirectory = Path.Combine(Directory.GetCurrentDirectory(), "DataProtection-Keys");
            Directory.CreateDirectory(keysDirectory);

            var dataProtectionBuilder = services.AddDataProtection()
                .SetApplicationName("RM-Backend-API")
                .PersistKeysToFileSystem(new DirectoryInfo(keysDirectory))
                .SetDefaultKeyLifetime(TimeSpan.FromDays(90));

            if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
            {
                dataProtectionBuilder.ProtectKeysWithDpapi();
            }

            services.AddMemoryCache();
            services.Configure<IpRateLimitOptions>(Configuration.GetSection("IpRateLimiting"));
            services.AddSingleton<IIpPolicyStore, MemoryCacheIpPolicyStore>();
            services.AddSingleton<IRateLimitCounterStore, MemoryCacheRateLimitCounterStore>();
            services.AddSingleton<IRateLimitConfiguration, RateLimitConfiguration>();
            services.AddSingleton<IProcessingStrategy, AsyncKeyLockProcessingStrategy>();

            services.AddHttpContextAccessor();
            services.AddScoped<ICurrentUser, CurrentUser>();
            services.AddRMitraInfrastructure(Configuration);
            ConfigurationHelper.Initialize(Configuration);

            services.AddTransient<DapperContext>();
            services.AddTransient<DapperContextViewOnly>();
            services.AddScoped<IHealthRepository, HealthRepository>();

            var otpSettings = Configuration.GetSection("OtpSettings").Get<OtpSettings>() ?? new OtpSettings();
            services.AddSingleton(otpSettings);

            var smsSettings = Configuration.GetSection("SmsSettings").Get<SmsSettings>() ?? new SmsSettings();
            services.AddSingleton(smsSettings);

            var tokenSettings = Configuration.GetSection("TokenSettings").Get<TokenSettings>() ?? new TokenSettings();
            services.AddSingleton(tokenSettings);

            var jwt = Configuration.GetSection(JwtOptions.SectionName).Get<JwtOptions>() ?? new JwtOptions();
            if (string.IsNullOrWhiteSpace(jwt.Secret))
            {
                jwt.Secret = tokenSettings.Secret_Key;
            }

            services.PostConfigure<JwtOptions>(options =>
            {
                if (string.IsNullOrWhiteSpace(options.Secret))
                {
                    options.Secret = tokenSettings.Secret_Key;
                }
            });

            services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
                .AddJwtBearer(options =>
                {
                    var keyBytes = Encoding.UTF8.GetBytes(jwt.Secret);
                    if (keyBytes.Length < 32)
                    {
                        Array.Resize(ref keyBytes, 32);
                    }

                    options.TokenValidationParameters = new TokenValidationParameters
                    {
                        ValidateIssuer = true,
                        ValidateAudience = true,
                        ValidateIssuerSigningKey = true,
                        ValidateLifetime = true,
                        ValidIssuer = jwt.Issuer,
                        ValidAudience = jwt.Audience,
                        IssuerSigningKey = new SymmetricSecurityKey(keyBytes)
                    };
                });

            services.AddAuthorization();

            services.AddHttpClient<HttpSmsService>();
            services.AddScoped<LogSmsService>();
            services.AddScoped<ISmsService>(sp =>
            {
                if (string.Equals(smsSettings.Provider, "Http", StringComparison.OrdinalIgnoreCase))
                    return sp.GetRequiredService<HttpSmsService>();

                return sp.GetRequiredService<LogSmsService>();
            });

            services.AddScoped<IOtpRepository, OtpRepository>();
            services.AddScoped<IKitchenRepository, KitchenRepository>();

            services.AddHealthChecks()
                .AddCheck("self", () => HealthCheckResult.Healthy("API is running"), tags: new[] { "live" })
                .AddCheck<DatabaseHealthCheck>("database", failureStatus: HealthStatus.Degraded, tags: new[] { "db", "ready" });

            services.Configure<ApiBehaviorOptions>(options =>
            {
                options.SuppressModelStateInvalidFilter = true;
            });

            services.AddControllers(options =>
            {
                options.SuppressImplicitRequiredAttributeForNonNullableReferenceTypes = true;
                options.Filters.Add(typeof(CustomExceptionFilter));
                options.Filters.Add(typeof(ValidateModelAttribute));
                options.Filters.Add(typeof(CustomAuthenticationFilter));
            })
            .AddNewtonsoftJson();

            services.AddSwaggerGen(options =>
            {
                options.CustomSchemaIds(type => type.FullName?.Replace("+", ".") ?? type.Name);
                options.OperationFilter<AuthorizationHeaderParameterOperationFilter>();
                options.SwaggerDoc("RasoiMitraAPI", new OpenApiInfo
                {
                    Title = "RasoiMitra Backend API",
                    Version = "v1.0.0"
                });
                options.SwaggerDoc("HomelyAPI", new OpenApiInfo
                {
                    Title = "HOMELY API",
                    Version = "v1.0.0",
                    Description = "Homely homemade food APIs. Base path /api/homely"
                });
                options.DocInclusionPredicate((docName, apiDesc) =>
                {
                    var relativePath = apiDesc.RelativePath ?? string.Empty;
                    var isHomely = relativePath.StartsWith("api/homely", StringComparison.OrdinalIgnoreCase);
                    return docName == "HomelyAPI" ? isHomely : !isHomely;
                });
                options.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
                {
                    Description = "JWT from Homely login / verify_otp. Example: Bearer {token}",
                    Name = "Authorization",
                    In = ParameterLocation.Header,
                    Type = SecuritySchemeType.Http,
                    Scheme = "bearer",
                    BearerFormat = "JWT"
                });
                options.AddSecurityRequirement(new OpenApiSecurityRequirement
                {
                    {
                        new OpenApiSecurityScheme
                        {
                            Reference = new OpenApiReference
                            {
                                Type = ReferenceType.SecurityScheme,
                                Id = "Bearer"
                            }
                        },
                        Array.Empty<string>()
                    }
                });

                var xmlFile = $"{Assembly.GetExecutingAssembly().GetName().Name}.xml";
                var xmlPath = Path.Combine(AppContext.BaseDirectory, xmlFile);
                if (File.Exists(xmlPath))
                {
                    options.IncludeXmlComments(xmlPath);
                }
            });

            services.Configure<KestrelServerOptions>(options =>
            {
                options.AllowSynchronousIO = true;
                options.Limits.MaxRequestBodySize = int.MaxValue;
                options.Limits.KeepAliveTimeout = TimeSpan.FromMinutes(30);
                options.Limits.RequestHeadersTimeout = TimeSpan.FromMinutes(30);
            });

            services.Configure<FormOptions>(options =>
            {
                options.ValueLengthLimit = int.MaxValue;
                options.MultipartBodyLengthLimit = int.MaxValue;
                options.MultipartHeadersLengthLimit = int.MaxValue;
            });
        }

        public void Configure(IApplicationBuilder app, IWebHostEnvironment env, ILoggerFactory loggerFactory)
        {
            var path = Directory.GetCurrentDirectory();
            app.UseForwardedHeaders();

            if (env.IsDevelopment())
            {
                app.UseHttpsRedirection();
            }
            else
            {
                app.UseHsts();
            }

            app.UseSecurityHeaders();
            var logsDirectory = Path.Combine(path, "Logs");
            Directory.CreateDirectory(logsDirectory);
            loggerFactory.AddFile(Path.Combine(logsDirectory, "Log.txt"));

            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }

            app.UseIpRateLimiting();
            app.UseStaticFiles();
            app.UseRouting();
            app.UseCors("AllowAllOrigins");
            app.UseAuthentication();
            app.UseAuthorization();

            var swaggerEnabled = Configuration.GetSection("Swagger:Switch").Value == "Y";

            if (swaggerEnabled)
            {
                app.UseSwagger();
                app.UseSwaggerUI(options =>
                {
                    options.SwaggerEndpoint("/swagger/HomelyAPI/swagger.json", "HOMELY API");
                    options.SwaggerEndpoint("/swagger/RasoiMitraAPI/swagger.json", "RasoiMitra Backend API");
                });
            }

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapGet("/", context =>
                {
                    context.Response.Redirect(swaggerEnabled ? "/swagger" : "/health");
                    return Task.CompletedTask;
                });
                endpoints.MapControllers();
                endpoints.MapHealthChecks("/health", new HealthCheckOptions
                {
                    Predicate = check => check.Tags.Contains("live")
                });
                endpoints.MapHealthChecks("/ready", new HealthCheckOptions
                {
                    Predicate = check => check.Tags.Contains("ready")
                });
            });

            using var scope = app.ApplicationServices.CreateScope();
            AdminSeeder.TrySeedAsync(
                    scope.ServiceProvider.GetRequiredService<ISqlConnectionFactory>(),
                    scope.ServiceProvider.GetRequiredService<IPasswordHasher>(),
                    Configuration,
                    loggerFactory.CreateLogger("AdminSeeder"))
                .GetAwaiter()
                .GetResult();
        }
    }
}
