using System.Text;
using Bunit;
using Maliev.ShadcnBlazor.Components;
using Maliev.ShadcnBlazor.Showcase.Components.Theming;
using Maliev.ShadcnBlazor.Showcase.Export;
using Maliev.ShadcnBlazor.Showcase.Theming;
using Maliev.ShadcnBlazor.Theming;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.JSInterop;

namespace Maliev.ShadcnBlazor.Tests.Showcase;

public sealed class ThemeValidationPresentationTests
{
    [Theory]
    [InlineData(0, "0 errors")]
    [InlineData(1, "1 error")]
    [InlineData(2, "2 errors")]
    public void ErrorLabelUsesCountAwareGrammar(int errors, string expected)
    {
        Assert.Equal(expected, ThemeValidationPresentation.ErrorLabel(errors));
    }

    [Theory]
    [InlineData(0, 0, "Ready to export")]
    [InlineData(0, 1, "Ready to export · 1 advisory")]
    [InlineData(0, 16, "Ready to export · 16 advisories")]
    [InlineData(1, 0, "Export blocked · 1 error")]
    [InlineData(2, 16, "Export blocked · 2 errors")]
    public void StatusLabelExplainsExportConsequence(int errors, int advisories, string expected)
    {
        Assert.Equal(expected, ThemeValidationPresentation.StatusLabel(errors, advisories));
    }
}

public sealed class ThemeValidationPresentationComponentTests : BunitContext
{
    public ThemeValidationPresentationComponentTests()
    {
        JSInterop.Mode = JSRuntimeMode.Loose;
        Services.AddMalievShadcn();
        Services.AddSingleton<IThemeStudioStorage>(new NoOpStorage());
        Services.AddSingleton<ThemeStudioState>();
    }

    [Fact]
    public void CurrentValidationErrorBlocksDownloadEvenWhenTheAppliedThemeRemainsValid()
    {
        var state = Services.GetRequiredService<ThemeStudioState>();
        state.SetToken(ThemeStudioScheme.Light, "primary", "#123456");
        state.SetToken(ThemeStudioScheme.Light, "primary", "red; background:url(evil)");
        Assert.False(state.Validation.IsValid);
        Assert.Equal("#123456", state.Applied.Light.Primary);

        var cut = Render<ThemeExportDialog>(parameters => parameters
            .Add(component => component.State, state)
            .Add(component => component.Open, true));

        Assert.Contains("Export blocked · 1 error", cut.Markup, StringComparison.Ordinal);
        var download = cut.Find("button[data-testid='theme-download']");
        Assert.True(download.HasAttribute("disabled"));
        download.Click();
        Assert.DoesNotContain(JSInterop.Invocations, invocation => invocation.Identifier == "downloadBytes");
    }

    [Fact]
    public void ExportAcknowledgementUsesTheGeneratedReadmeWarningTerminology()
    {
        var state = Services.GetRequiredService<ThemeStudioState>();
        state.SetToken(ThemeStudioScheme.Light, "foreground", "#777777");
        Assert.True(state.Validation.IsValid);
        Assert.NotEmpty(state.Validation.Warnings);

        var cut = Render<ThemeExportDialog>(parameters => parameters
            .Add(component => component.State, state)
            .Add(component => component.Open, true));
        var acknowledgement = cut.Find("[data-testid='theme-export-warning-ack']").ParentElement!;
        var readme = Encoding.UTF8.GetString(ThemeBundleBuilder.Build(
            state.CreateDocument(),
            new ThemeBundleOptions(state.SelectedPresetId, "1.0.0"))
            .Files.Single(file => file.Path == "README.md").Bytes);

        Assert.Contains("contrast warnings recorded in README.md", acknowledgement.TextContent, StringComparison.Ordinal);
        Assert.Contains("Contrast warnings", readme, StringComparison.Ordinal);
    }

    [Fact]
    public void ValidationSummaryUsesSingularErrorAndAdvisoryGrammar()
    {
        var state = Services.GetRequiredService<ThemeStudioState>();
        var validation = new ShadcnThemeValidationResult(
            [new ShadcnThemeValidationMessage("invalid-token", "light.primary", "Token is invalid.")],
            [new ShadcnThemeValidationMessage("low-contrast", "light.foreground", "Contrast needs review.")],
            []);
        typeof(ThemeStudioState).GetProperty(nameof(ThemeStudioState.Validation))!.SetValue(state, validation);

        var cut = Render<ThemeValidationSummary>(parameters => parameters
            .Add(component => component.State, state));

        Assert.Contains("1 error · 1 advisory ·", cut.Markup, StringComparison.Ordinal);
        Assert.DoesNotContain("1 errors", cut.Markup, StringComparison.Ordinal);
        Assert.DoesNotContain("1 advisories", cut.Markup, StringComparison.Ordinal);
    }

    private sealed class NoOpStorage : IThemeStudioStorage
    {
        public ValueTask<ThemeStudioStorageResult> LoadAsync() => ValueTask.FromResult(ThemeStudioStorageResult.Success(null));
        public ValueTask<ThemeStudioStorageResult> SaveAsync(ShadcnThemeDocument document) => ValueTask.FromResult(ThemeStudioStorageResult.Success(document));
    }
}
