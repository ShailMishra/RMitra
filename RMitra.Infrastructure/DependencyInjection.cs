using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using RMitra.Application.Abstractions;
using RMitra.Application.Catalog;
using RMitra.Application.Customer;
using RMitra.Application.Delivery;
using RMitra.Application.Identity;
using RMitra.Application.Kitchen;
using RMitra.Application.Ordering;
using RMitra.Application.Payment;
using RMitra.Application.Settlement;
using RMitra.Application.Subscription;
using RMitra.Infrastructure.Catalog;
using RMitra.Infrastructure.Customer;
using RMitra.Infrastructure.Delivery;
using RMitra.Infrastructure.Identity;
using RMitra.Infrastructure.Kitchen;
using RMitra.Infrastructure.Options;
using RMitra.Infrastructure.Ordering;
using RMitra.Infrastructure.Payment;
using RMitra.Infrastructure.Persistence;
using RMitra.Infrastructure.Security;
using RMitra.Infrastructure.Settlement;
using RMitra.Infrastructure.Subscription;

namespace RMitra.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddRMitraInfrastructure(this IServiceCollection services, IConfiguration configuration)
    {
        services.Configure<JwtOptions>(configuration.GetSection(JwtOptions.SectionName));
        services.Configure<OtpOptions>(configuration.GetSection(OtpOptions.SectionName));
        services.Configure<CommerceOptions>(configuration.GetSection(CommerceOptions.SectionName));

        services.AddSingleton<ISqlConnectionFactory, SqlConnectionFactory>();
        services.AddSingleton<ITokenService, JwtTokenService>();
        services.AddSingleton<IPasswordHasher, PasswordHasher>();
        services.AddSingleton<IGeoCalculator, GeoCalculator>();
        services.AddScoped<IPublicIdGenerator, PublicIdGenerator>();

        services.AddScoped<IdentityService>();
        services.AddScoped<IIdentityService>(sp => sp.GetRequiredService<IdentityService>());
        services.AddScoped<IKitchenService, KitchenService>();
        services.AddScoped<ICatalogService, CatalogService>();
        services.AddScoped<ICustomerService, CustomerService>();
        services.AddScoped<ISubscriptionService, SubscriptionService>();
        services.AddScoped<ISettlementService, SettlementService>();
        services.AddScoped<IRiderService, RiderService>();
        services.AddScoped<IOrderingService, OrderingService>();
        services.AddScoped<IPaymentService, PaymentService>();

        return services;
    }
}
