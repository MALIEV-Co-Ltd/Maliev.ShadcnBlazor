using Maliev.ShadcnBlazor.Theming;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Maliev.ShadcnBlazor.Components.Feedback.Toast;
using Maliev.ShadcnBlazor.Components.DataDisplay;

namespace Maliev.ShadcnBlazor.Tests.Theming;

public sealed class ServiceCollectionExtensionsTests
{
    [Fact]
    public void AddMalievShadcnRegistersOnlyPackageOwnedOptionsAndServices()
    {
        var services = new ServiceCollection();
        services.AddLogging();
        services.AddMalievShadcn(options => options.FontFamily = "Test Sans");
        using var provider = services.BuildServiceProvider();

        Assert.Equal("Test Sans", provider.GetRequiredService<IOptions<ShadcnOptions>>().Value.FontFamily);
        Assert.NotNull(provider.GetRequiredService<IShadcnIdAllocator>());
        Assert.NotNull(provider.GetRequiredService<IShadcnToastService>());
    }

    [Fact]
    public void ToastLifecycleDefaultsAreConfigurableThroughLibraryOptions()
    {
        var services = new ServiceCollection();
        services.AddLogging();
        services.AddMalievShadcn(options =>
        {
            options.ToastDuration = TimeSpan.FromSeconds(9);
            options.ToastExitDuration = TimeSpan.FromMilliseconds(250);
        });
        using var provider = services.BuildServiceProvider();
        using var scope = provider.CreateScope();
        var service = scope.ServiceProvider.GetRequiredService<IShadcnToastService>();

        var id = service.Show(new ShadcnToastOptions("Configured"));
        Assert.Equal(TimeSpan.FromSeconds(9), service.Items.Single().Duration);
        Assert.True(service.Dismiss(id));
        Assert.Equal("closing", service.Items.Single().State);
    }

    [Fact]
    public void ZeroDefaultToastDurationConfiguresPersistentToasts()
    {
        var services = new ServiceCollection();
        services.AddLogging();
        services.AddMalievShadcn(options => options.ToastDuration = TimeSpan.Zero);
        using var provider = services.BuildServiceProvider();
        using var scope = provider.CreateScope();
        var service = scope.ServiceProvider.GetRequiredService<IShadcnToastService>();

        service.Show(new ShadcnToastOptions("Persistent by default"));

        Assert.Null(service.Items.Single().Duration);
    }
}
