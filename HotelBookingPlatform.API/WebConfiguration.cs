using Asp.Versioning;
using HotelBookingPlatform.API.Filters;
using HotelBookingPlatform.API.Middlewares;
using HotelBookingPlatform.API.Services;
using HotelBookingPlatform.Application.AuthorizationOptions;
using HotelBookingPlatform.Application.Interfaces;
using Microsoft.OpenApi.Models;
using System.Reflection;
using System.Security.Claims;
using System.Text.Json.Serialization;

namespace HotelBookingPlatform.API;

/// <summary>
/// Register services in the DI container
/// </summary>
public static class WebConfiguration
{
    /// <summary>
    /// Register services
    /// </summary>
    /// <param name="services"></param>
    /// <param name="configuration"></param>
    /// <returns></returns>
    public static IServiceCollection AddWebComponents(
               this IServiceCollection services, IConfiguration configuration)
    {
        services.AddApiVersioning(setup =>
        {
            setup.DefaultApiVersion = new ApiVersion(1, 0);
            setup.AssumeDefaultVersionWhenUnspecified = true;
            setup.ReportApiVersions = true;
        }).AddMvc();

        services.AddControllers(option =>
            {
                option.Filters.Add<LogActivityFilter>();
            })
        .AddJsonOptions(options =>
            {
                options.JsonSerializerOptions.Converters.Add(new JsonStringEnumConverter());
            })
        .AddJsonOptions(options =>
            {
                options.JsonSerializerOptions.Encoder = System.Text.Encodings.Web.JavaScriptEncoder.UnsafeRelaxedJsonEscaping;
            });
        services.AddEndpointsApiExplorer();
        services.AddProblemDetails()
                .AddExceptionHandler<GlobalExceptionHandler>();
        services.AddSwagger();
        services.AddSwaggerGen(c =>
        {
            string xmlFile = $"{Assembly.GetExecutingAssembly().GetName().Name}.xml";
            string xmlPath = Path.Combine(AppContext.BaseDirectory, xmlFile);
            c.IncludeXmlComments(xmlPath);
        });
        services.AddDateOnlyTimeOnlyStringConverters();

        services.AddAuthorization(options =>
        {
            options.AddPolicy(Policies.GuestOnly, policy => policy
                                              .RequireRole(UserRoles.Guest.ToString())
                                              .RequireClaim(ClaimTypes.NameIdentifier)
                                              .RequireClaim(ClaimTypes.Email));

            options.AddPolicy(Policies.AdminOnly, policy => policy
                                              .RequireRole(UserRoles.Admin.ToString())
                                              .RequireClaim(ClaimTypes.NameIdentifier)
                                              .RequireClaim(ClaimTypes.Email));
        });
        services.AddHttpContextAccessor();
        services.AddScoped<ICurrentUser, CurrentUser>();

        return services;
    }

    private static IServiceCollection AddSwagger(
              this IServiceCollection services)
        => services.AddSwaggerGen(setup =>
        {
            setup.SwaggerDoc("v1", new OpenApiInfo
            {
                Title = "Hotel Booking Blatform API",
                Version = "v1",
                Description = "API endpoints for managing hotel bookings",
            });
            setup.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
            {
                In = ParameterLocation.Header,
                Description = "Please enter a valid token",
                Name = "Authorization",
                Type = SecuritySchemeType.Http,
                BearerFormat = "JWT",
                Scheme = "Bearer"
            });
            setup.AddSecurityRequirement(new OpenApiSecurityRequirement
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
            setup.UseDateOnlyTimeOnlyStringConverters();
        });
}
