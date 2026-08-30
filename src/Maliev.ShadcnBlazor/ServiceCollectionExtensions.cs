using Maliev.ShadcnBlazor.Theming;
using Maliev.ShadcnBlazor.Components.Feedback.Toast;
using Maliev.ShadcnBlazor.Components.DataDisplay;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Options;

namespace Maliev.ShadcnBlazor;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddMalievShadcn(
        this IServiceCollection services,
        Action<ShadcnOptions>? configure = null)
    {
        ArgumentNullException.ThrowIfNull(services);
        services.AddOptions<ShadcnOptions>();
        if (configure is not null)
            services.Configure(configure);
        services.TryAddSingleton(TimeProvider.System);
        services.TryAddScoped<IShadcnIdAllocator, ShadcnIdAllocator>();
        services.AddScoped<IShadcnToastService>(provider =>
        {
            var options = provider.GetRequiredService<IOptions<ShadcnOptions>>().Value;
            return new ShadcnToastService(provider.GetRequiredService<TimeProvider>(), options.ToastDuration, options.ToastExitDuration);
        });
        return services;
    }
}
