using FluentValidation;
using Microsoft.Extensions.DependencyInjection;
using Pharmacy.Application.Services;

namespace Pharmacy.Application;

public static class DependencyInjection
{
    public static IServiceCollection AddApplication(this IServiceCollection services)
    {
        services.AddValidatorsFromAssembly(typeof(DependencyInjection).Assembly);
        services.AddScoped<IMedicineService, MedicineService>();
        services.AddScoped<IAuthService, AuthService>();
        return services;
    }
}
