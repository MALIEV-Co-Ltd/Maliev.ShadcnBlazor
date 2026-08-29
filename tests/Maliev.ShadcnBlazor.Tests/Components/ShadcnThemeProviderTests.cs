using System.Reflection;
using Bunit;
using Maliev.ShadcnBlazor.Components;
using Maliev.ShadcnBlazor.Theming;
using Microsoft.AspNetCore.Components;
using Microsoft.Extensions.DependencyInjection;

namespace Maliev.ShadcnBlazor.Tests.Components;

public sealed class ShadcnThemeProviderTests : BunitContext
{
    public ShadcnThemeProviderTests()
    {
        JSInterop.Mode = JSRuntimeMode.Loose;
        Services.AddMalievShadcn();
    }

    [Fact]
    public void DoesNotExposeAOneOffSystemPreferenceSamplingMethod()
    {
        Assert.DoesNotContain(
            typeof(ShadcnThemeProvider).GetMethods(BindingFlags.Instance | BindingFlags.Public | BindingFlags.DeclaredOnly),
            method => method.Name == "GetSystemDarkModeAsync");
    }

    [Fact]
    public void RendersScopedDarkRtlRootWithoutExternalProviders()
    {
        var cut = Render<ShadcnThemeProvider>(parameters => parameters
            .Add(x => x.IsDarkMode, true)
            .Add(x => x.Direction, ShadcnDirection.RightToLeft)
            .Add(x => x.Class, "consumer-shell")
            .AddChildContent("content"));

        var root = cut.Find("[data-shadcn-scope]");
        Assert.Contains(ShadcnCss.ScopeClass, root.ClassList);
        Assert.Contains("consumer-shell", root.ClassList);
        Assert.Equal("dark", root.GetAttribute("data-shadcn-theme"));
        Assert.Equal("rtl", root.GetAttribute("dir"));
        Assert.Equal("content", root.TextContent.Trim());
        Assert.DoesNotContain("mud-", cut.Markup, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public void SystemPreferenceObservationIsOptInAndDefaultsDoNotChange()
    {
        var cut = Render<ShadcnThemeProvider>();
        Assert.False(cut.Instance.ObserveSystemDarkModeChange);
        Assert.Equal("light", cut.Find("[data-shadcn-scope]").GetAttribute("data-shadcn-theme"));
    }

    [Fact]
    public void OptedInSystemPreferenceIsSampledThroughThePackageModule()
    {
        bool? observed = null;
        var cut = Render<ShadcnThemeProvider>(parameters => parameters
            .Add(component => component.ObserveSystemDarkModeChange, true)
            .Add(component => component.SystemDarkModeChanged, value => observed = value));
        Assert.True(cut.Instance.ObserveSystemDarkModeChange);
        Assert.False(observed);
        Assert.Contains(JSInterop.Invocations, invocation => invocation.Identifier == "import");
        Assert.Contains(JSInterop.Invocations, invocation => invocation.Identifier == "observeSystemDarkMode");
    }

    [Fact]
    public void CascadesTheCurrentThemeAndDirection()
    {
        ShadcnContext? observed = null;
        var cut = Render<ShadcnThemeProvider>(parameters => parameters
            .Add(x => x.Direction, ShadcnDirection.LeftToRight)
            .AddChildContent<CaptureContext>(child => child.Add(x => x.OnCaptured, value => observed = value)));
        Assert.Equal(new ShadcnContext(false, ShadcnDirection.LeftToRight), observed);
    }

    [Fact]
    public void UsesConfiguredDefaultsWhenParametersAreOmitted()
    {
        Services.Configure<ShadcnOptions>(options =>
        {
            options.DefaultDarkMode = true;
            options.DefaultDirection = ShadcnDirection.RightToLeft;
            options.FontFamily = "Noto Sans Thai, sans-serif";
        });

        var cut = Render<ShadcnThemeProvider>();

        var root = cut.Find("[data-shadcn-scope]");
        Assert.Equal("dark", root.GetAttribute("data-shadcn-theme"));
        Assert.Equal("rtl", root.GetAttribute("dir"));
        Assert.Equal("--shadcn-font-sans: Noto Sans Thai, sans-serif", root.GetAttribute("style"));
    }

    [Fact]
    public void ExplicitParametersOverrideConfiguredDefaults()
    {
        Services.Configure<ShadcnOptions>(options =>
        {
            options.DefaultDarkMode = true;
            options.DefaultDirection = ShadcnDirection.RightToLeft;
        });

        var cut = Render<ShadcnThemeProvider>(parameters => parameters
            .Add(x => x.IsDarkMode, false)
            .Add(x => x.Direction, ShadcnDirection.LeftToRight));

        var root = cut.Find("[data-shadcn-scope]");
        Assert.Equal("light", root.GetAttribute("data-shadcn-theme"));
        Assert.Equal("ltr", root.GetAttribute("dir"));
    }

    [Fact]
    public void ConfiguredThemeIsUsedWhenTheThemeParameterIsOmitted()
    {
        var configuredTheme = ShadcnThemePresets.BaseVegaNeutral.CreateTheme() with
        {
            Light = ShadcnThemePresets.BaseVegaNeutral.Theme.Light with
            {
                Primary = "oklch(0.51 0.12 244)"
            }
        };
        Services.Configure<ShadcnOptions>(options => options.Theme = configuredTheme);

        var cut = Render<ShadcnThemeProvider>();

        Assert.Contains("--shadcn-primary: oklch(0.51 0.12 244)",
            cut.Find("[data-shadcn-scope]").GetAttribute("style"), StringComparison.Ordinal);
    }

    [Fact]
    public void ExplicitThemeParameterWinsOverTheConfiguredTheme()
    {
        var configuredTheme = ShadcnThemePresets.BaseVegaNeutral.CreateTheme() with
        {
            Light = ShadcnThemePresets.BaseVegaNeutral.Theme.Light with
            {
                Primary = "oklch(0.51 0.12 244)"
            }
        };
        var explicitTheme = configuredTheme with
        {
            Light = configuredTheme.Light with
            {
                Primary = "oklch(0.43 0.16 32)"
            }
        };
        Services.Configure<ShadcnOptions>(options => options.Theme = configuredTheme);

        var cut = Render<ShadcnThemeProvider>(parameters => parameters
            .Add(component => component.Theme, explicitTheme));

        var style = cut.Find("[data-shadcn-scope]").GetAttribute("style");
        Assert.Contains("--shadcn-primary: oklch(0.43 0.16 32)", style, StringComparison.Ordinal);
        Assert.DoesNotContain("--shadcn-primary: oklch(0.51 0.12 244)", style, StringComparison.Ordinal);
    }

    [Fact]
    public void PreservesConsumerClassAndStyleWhileContractAttributesWin()
    {
        var cut = Render<ShadcnThemeProvider>(parameters => parameters
            .Add(x => x.Class, "consumer-shell")
            .Add(x => x.Style, "background: black")
            .Add(x => x.AdditionalAttributes, new Dictionary<string, object>
            {
                ["class"] = "dictionary-shell",
                ["style"] = "color: rebeccapurple; --shadcn-font-sans: defeated",
                ["data-shadcn-scope"] = "false",
                ["data-shadcn-theme"] = "defeated",
                ["dir"] = "auto",
                ["aria-label"] = "consumer label"
            }));

        var root = cut.Find("[data-shadcn-scope]");
        Assert.Contains(ShadcnCss.ScopeClass, root.ClassList);
        Assert.Contains("consumer-shell", root.ClassList);
        Assert.Contains("dictionary-shell", root.ClassList);
        Assert.Equal(string.Empty, root.GetAttribute("data-shadcn-scope"));
        Assert.Equal("light", root.GetAttribute("data-shadcn-theme"));
        Assert.Equal("ltr", root.GetAttribute("dir"));
        Assert.Equal("consumer label", root.GetAttribute("aria-label"));
        Assert.Equal(
            "background: black; color: rebeccapurple; --shadcn-font-sans: defeated; --shadcn-font-sans: 'Geist', 'Noto Sans Thai', ui-sans-serif, system-ui, sans-serif",
            root.GetAttribute("style"));
    }

    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public void RejectsUnknownEffectiveDirection(bool explicitParameter)
    {
        var unknown = (ShadcnDirection)999;
        if (!explicitParameter)
            Services.Configure<ShadcnOptions>(options => options.DefaultDirection = unknown);

        var exception = Assert.ThrowsAny<Exception>(() => Render<ShadcnThemeProvider>(parameters =>
        {
            if (explicitParameter)
                parameters.Add(x => x.Direction, unknown);
        }));

        Assert.Contains("direction", exception.ToString(), StringComparison.OrdinalIgnoreCase);
    }

    private sealed class CaptureContext : ComponentBase
    {
        [CascadingParameter] public ShadcnContext Context { get; set; }
        [Parameter] public Action<ShadcnContext>? OnCaptured { get; set; }
        protected override void OnParametersSet() => OnCaptured?.Invoke(Context);
    }

    protected override void Dispose(bool disposing)
    {
        if (disposing)
            DisposeAsyncCore().AsTask().GetAwaiter().GetResult();

        base.Dispose(disposing);
    }
}
