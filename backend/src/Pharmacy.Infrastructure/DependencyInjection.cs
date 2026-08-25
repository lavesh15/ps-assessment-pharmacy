using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Pharmacy.Application.Options;
using Pharmacy.Domain.Repositories;
using Pharmacy.Infrastructure.Persistence;

namespace Pharmacy.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(this IServiceCollection services, IConfiguration configuration)
    {
        services.Configure<JsonStoreOptions>(configuration.GetSection(JsonStoreOptions.SectionName));
        services.Configure<CorsPolicyOptions>(configuration.GetSection(CorsPolicyOptions.SectionName));
        services.Configure<ApiKeyOptions>(configuration.GetSection(ApiKeyOptions.SectionName));
        services.Configure<DemoAuthOptions>(configuration.GetSection(DemoAuthOptions.SectionName));
        services.Configure<FrontendOptions>(configuration.GetSection(FrontendOptions.SectionName));

        services.AddSingleton<JsonPharmacyStore>();
        services.AddSingleton<IMedicineRepository>(sp => sp.GetRequiredService<JsonPharmacyStore>());
        services.AddSingleton<ISaleRepository>(sp => sp.GetRequiredService<JsonPharmacyStore>());
        services.AddSingleton<IIdempotencyStore>(sp => sp.GetRequiredService<JsonPharmacyStore>());

        return services;
    }
}
