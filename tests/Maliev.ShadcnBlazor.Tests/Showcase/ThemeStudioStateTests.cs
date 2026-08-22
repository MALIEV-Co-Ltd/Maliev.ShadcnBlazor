using Bunit;
using Maliev.ShadcnBlazor.Showcase.Components.Theming;
using Maliev.ShadcnBlazor.Showcase.Pages;
using Maliev.ShadcnBlazor.Showcase.MockSites;
using Maliev.ShadcnBlazor.Showcase.Theming;
using Maliev.ShadcnBlazor.Theming;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.JSInterop;

namespace Maliev.ShadcnBlazor.Tests.Showcase;

public sealed class ThemeStudioStateTests
{
    [Fact]
    public void WorkbenchStateOwnsShellPreferencesWithoutCreatingThemeHistory()
    {
        var workbench = new ThemeStudioWorkbenchState();
        var changes = 0;
        workbench.Changed += (_, _) => changes++;

        Assert.False(workbench.SidebarOpen);
        Assert.Equal("colors", workbench.ActiveSection);
        Assert.Equal(ThemeStudioViewport.Desktop, workbench.Viewport);
        Assert.Equal(ThemeStudioMode.Light, workbench.Mode);
        Assert.Equal(ShadcnDirection.LeftToRight, workbench.Direction);
        Assert.Equal(ThemeStudioLocale.English, workbench.Locale);
        Assert.False(workbench.ReducedMotion);
        Assert.False(workbench.HighContrastPreview);

        workbench.SetViewport(ThemeStudioViewport.Mobile);
        workbench.SetMode(ThemeStudioMode.Dark);
        workbench.SetDirection(ShadcnDirection.RightToLeft);
        workbench.SetLocale(ThemeStudioLocale.Thai);
        workbench.SetReducedMotion(true);
        workbench.SetHighContrastPreview(true);
        workbench.SetActiveSection("typography");
        workbench.OpenSidebar();
        workbench.CloseSidebar();

        Assert.Equal(9, changes);
        Assert.False(workbench.SidebarOpen);
        Assert.Equal("typography", workbench.ActiveSection);
        Assert.Throws<ArgumentOutOfRangeException>(() => workbench.SetMode((ThemeStudioMode)999));
        Assert.Throws<ArgumentOutOfRangeException>(() => workbench.SetViewport(new("unknown", "Unknown", 1)));
    }

    [Fact]
    public void ThemeStateForwardsWorkbenchChangesWithoutPollutingThemeHistory()
    {
        var workbench = new ThemeStudioWorkbenchState();
        var state = new ThemeStudioState(new RecordingStorage(), workbench);
        var changes = 0;
        state.Changed += (_, _) => changes++;

        state.SetViewport(ThemeStudioViewport.Tablet);
        state.SetMode(ThemeStudioMode.Dark);
        state.SetDirection(ShadcnDirection.RightToLeft);
        state.SetLocale(ThemeStudioLocale.Thai);
        workbench.SetReducedMotion(true);

        Assert.Same(workbench, state.Workbench);
        Assert.Equal(5, changes);
        Assert.Equal(768, state.Viewport.Width);
        Assert.True(state.EffectiveDarkMode);
        Assert.False(state.CanUndo);
    }

    [Fact]
    public void InvalidTokenEditorTextStaysLocalWhileTypedThemesRemainValid()
    {
        var state = CreateState();

        state.SetToken(ThemeStudioScheme.Light, "primary", "#123456");
        Assert.Equal("#123456", state.Draft.Light.Primary);
        Assert.Equal("#123456", state.Applied.Light.Primary);

        state.SetToken(ThemeStudioScheme.Light, "primary", "red; background:url(https://bad.example)");

        Assert.Equal("red; background:url(https://bad.example)", state.GetTokenValue(ThemeStudioScheme.Light, "primary"));
        Assert.Equal("#123456", state.Draft.Light.Primary);
        Assert.Equal("#123456", state.Applied.Light.Primary);
        Assert.True(ShadcnThemeValidator.Validate(state.Draft).IsValid);
        Assert.True(ShadcnThemeValidator.Validate(state.Applied).IsValid);
        Assert.False(state.Validation.IsValid);
        Assert.Contains(state.Validation.Errors, error => error.Path == "light.primary");
    }

    [Fact]
    public void InvalidValidUndoRedoRestoresTypedAppliedAndEditorStateTransactionally()
    {
        var state = CreateState();
        var original = state.Draft.Light.Primary;

        state.SetToken(ThemeStudioScheme.Light, "primary", "red; background:url(https://bad.example)");
        state.SetToken(ThemeStudioScheme.Light, "primary", "#123456");

        Assert.True(state.Undo());
        Assert.Equal(original, state.Draft.Light.Primary);
        Assert.Equal(original, state.Applied.Light.Primary);
        Assert.Equal("red; background:url(https://bad.example)", state.GetTokenValue(ThemeStudioScheme.Light, "primary"));
        Assert.False(state.Validation.IsValid);

        Assert.True(state.Undo());
        Assert.Equal(original, state.Draft.Light.Primary);
        Assert.Equal(original, state.Applied.Light.Primary);
        Assert.Equal(original, state.GetTokenValue(ThemeStudioScheme.Light, "primary"));
        Assert.True(state.Validation.IsValid);

        Assert.True(state.Redo());
        Assert.Equal(original, state.Draft.Light.Primary);
        Assert.Equal(original, state.Applied.Light.Primary);
        Assert.Equal("red; background:url(https://bad.example)", state.GetTokenValue(ThemeStudioScheme.Light, "primary"));
        Assert.False(state.Validation.IsValid);

        Assert.True(state.Redo());
        Assert.Equal("#123456", state.Draft.Light.Primary);
        Assert.Equal("#123456", state.Applied.Light.Primary);
        Assert.Equal("#123456", state.GetTokenValue(ThemeStudioScheme.Light, "primary"));
        Assert.True(state.Validation.IsValid);
    }

    [Fact]
    public void PointerInteractionCoalescesInvalidAndValidEditorChangesToOneTransaction()
    {
        var state = CreateState();
        var original = state.Draft.Light.Primary;

        state.BeginPointerInteraction("light.primary");
        state.SetToken(ThemeStudioScheme.Light, "primary", "invalid");
        state.SetToken(ThemeStudioScheme.Light, "primary", "#111111");
        state.SetToken(ThemeStudioScheme.Light, "primary", "#222222");
        state.EndPointerInteraction();

        Assert.True(state.Undo());
        Assert.Equal(original, state.Draft.Light.Primary);
        Assert.Equal(original, state.Applied.Light.Primary);
        Assert.Equal(original, state.GetTokenValue(ThemeStudioScheme.Light, "primary"));
        Assert.True(state.Validation.IsValid);
        Assert.False(state.CanUndo);
    }

    [Fact]
    public void InvalidMetricEditorTextRemainsVisibleWithoutReplacingTypedDraftOrAppliedTheme()
    {
        var state = CreateState();
        var original = state.Draft.Metrics.RadiusRem;

        state.SetMetric("radiusRem", "not-a-number");

        Assert.Equal("not-a-number", state.GetMetricEditorValue("radiusRem"));
        Assert.Equal(original, state.Draft.Metrics.RadiusRem);
        Assert.Equal(original, state.Applied.Metrics.RadiusRem);
        Assert.Contains(state.Validation.Errors, error => error.Path == "metrics.radiusRem");
    }

    [Fact]
    public void UndoRedoAreBoundedAndNewMutationsClearRedo()
    {
        var state = CreateState();
        var original = state.Draft.Light.Primary;
        for (var index = 0; index < 55; index++)
            state.SetToken(ThemeStudioScheme.Light, "primary", $"#{index:x2}{index:x2}{index:x2}");

        var undoCount = 0;
        while (state.Undo()) undoCount++;

        Assert.Equal(50, undoCount);
        Assert.NotEqual(original, state.Draft.Light.Primary);
        Assert.True(state.Redo());
        state.SetToken(ThemeStudioScheme.Light, "primary", "#abcdef");
        Assert.False(state.CanRedo);
    }

    [Fact]
    public void PointerInteractionCoalescesRepeatedChangesToOneUndoEntry()
    {
        var state = CreateState();
        var original = state.Draft.Light.Primary;

        state.BeginPointerInteraction("light.primary");
        state.SetToken(ThemeStudioScheme.Light, "primary", "#111111");
        state.SetToken(ThemeStudioScheme.Light, "primary", "#222222");
        state.SetToken(ThemeStudioScheme.Light, "primary", "#333333");
        state.EndPointerInteraction();

        Assert.True(state.Undo());
        Assert.Equal(original, state.Draft.Light.Primary);
        Assert.False(state.CanUndo);
    }

    [Fact]
    public void PresetsAreIsolatedAndResetScopesRestoreOnlyTheirOwnedValues()
    {
        var state = CreateState();
        var defaults = ShadcnThemePresets.BaseVegaNeutral.CreateTheme();
        state.SetToken(ThemeStudioScheme.Light, "primary", "#123456");
        state.SetToken(ThemeStudioScheme.Light, "secondary", "#abcdef");
        state.SetMetric("radiusRem", "1.25");

        state.ResetToken(ThemeStudioScheme.Light, "primary");
        Assert.Equal(defaults.Light.Primary, state.Draft.Light.Primary);
        Assert.Equal("#abcdef", state.Draft.Light.Secondary);
        Assert.Equal(1.25, state.Draft.Metrics.RadiusRem);

        state.ResetGroup(ThemeStudioGroup.Colors, ThemeStudioScheme.Light);
        Assert.Equal(defaults.Light.Secondary, state.Draft.Light.Secondary);
        Assert.Equal(1.25, state.Draft.Metrics.RadiusRem);

        state.ApplyPreset("base-vega-neutral");
        state.SetToken(ThemeStudioScheme.Light, "primary", "#010203");
        Assert.NotEqual("#010203", ShadcnThemePresets.BaseVegaNeutral.CreateTheme().Light.Primary);
        state.ResetAll();
        Assert.Equal(defaults, state.Draft);
        Assert.False(state.IsDirty);
    }

    [Fact]
    public void ViewportModeLocaleDirectionAndMockupNeverEnterThemeHistory()
    {
        var state = CreateState();

        state.SetMode(ThemeStudioMode.Dark);
        state.SetViewport(ThemeStudioViewport.Mobile);
        state.SetDirection(ShadcnDirection.RightToLeft);
        state.SetLocale(ThemeStudioLocale.Thai);
        state.SetSelectedMockup(ThemeStudioMockup.CustomerWorkspace);
        state.SetSystemDarkMode(true);

        Assert.Equal(ThemeStudioMode.Dark, state.Mode);
        Assert.Equal(390, state.Viewport.Width);
        Assert.Equal(ShadcnDirection.RightToLeft, state.Direction);
        Assert.Equal(ThemeStudioLocale.Thai, state.Locale);
        Assert.Equal(ThemeStudioMockup.CustomerWorkspace, state.SelectedMockup);
        Assert.True(state.SystemDarkMode);
        Assert.False(state.CanUndo);
        Assert.False(state.IsDirty);
    }

    [Fact]
    public void GeneratorConfigRoundTripsThemeAndApplicationMetadata()
    {
        var state = CreateState();
        state.SetStyle("base");
        state.SetBaseColor("stone");
        state.SetIconLibrary(ThemeStudioIconLibrary.Tabler);
        state.SetMenuAccent(ThemeStudioMenuAccent.Bold);
        state.SetMenuColor(ThemeStudioMenuColor.Translucent);
        state.SetRadiusPreset(ThemeStudioRadiusPreset.Relaxed);
        state.SetToken(ThemeStudioScheme.Light, "primary", "#123456");

        var json = state.SerializeGeneratorConfig();
        var config = ThemeStudioGeneratorConfigSerializer.Deserialize(json);

        Assert.Equal("base", config.Style);
        Assert.Equal("stone", config.BaseColor);
        Assert.Equal(ThemeStudioIconLibrary.Tabler, config.IconLibrary);
        Assert.Equal(ThemeStudioMenuAccent.Bold, config.MenuAccent);
        Assert.Equal(ThemeStudioMenuColor.Translucent, config.MenuColor);
        Assert.Equal(ThemeStudioRadiusPreset.Relaxed, config.RadiusPreset);
        Assert.Equal("#123456", config.Theme.Light.Primary);

        var restored = CreateState();
        Assert.True(restored.ImportGeneratorConfig(json));
        Assert.Equal(config.Theme, restored.Applied);
        Assert.Equal(config.Style, restored.StyleId);
        Assert.Equal(config.BaseColor, restored.BaseColorId);
        Assert.Equal(config.IconLibrary, restored.IconLibrary);
        Assert.Equal(config.MenuAccent, restored.MenuAccent);
        Assert.Equal(config.MenuColor, restored.MenuColor);
        Assert.Equal(config.RadiusPreset, restored.RadiusPreset);
        Assert.False(restored.IsDirty);
    }

    [Fact]
    public void GeneratorConfigRejectsUnknownMetadataWithoutChangingTheCurrentTheme()
    {
        var state = CreateState();
        var before = state.Applied;

        Assert.False(state.ImportGeneratorConfig(
            "{\"schemaVersion\":1,\"preset\":\"base-vega-neutral\",\"style\":\"not-a-style\",\"baseColor\":\"neutral\",\"iconLibrary\":\"lucide\",\"menuAccent\":\"default\",\"menuColor\":\"default\",\"radiusPreset\":\"default\",\"fontFamily\":\"Geist\",\"monospaceFontFamily\":\"JetBrains Mono\",\"theme\":{}}"));

        Assert.Equal(before, state.Applied);
        Assert.False(string.IsNullOrWhiteSpace(state.ImportDiagnostic));
    }

    [Fact]
    public void ImportIsTransactionalAndSuccessfulImportCreatesOneUndoEntry()
    {
        var state = CreateState();
        var before = state.Draft;
        var invalid = before with { Light = before.Light with { Primary = "not-a-color" } };

        Assert.False(state.Import(invalid));
        Assert.Equal(before, state.Draft);
        Assert.False(state.CanUndo);

        var imported = before with { Name = "Imported", Light = before.Light with { Primary = "#445566" } };
        Assert.True(state.Import(imported));
        Assert.Equal("#445566", state.Applied.Light.Primary);
        Assert.True(state.Undo());
        Assert.Equal(before, state.Draft);
        Assert.False(state.CanUndo);
    }

    [Fact]
    public async Task PersistenceRestoresValidThemesAndReturnsDiagnosticsWithoutThrowing()
    {
        var storage = new RecordingStorage();
        var state = new ThemeStudioState(storage);
        state.SetToken(ThemeStudioScheme.Dark, "background", "#101010");

        await state.PersistAsync();
        var restored = new ThemeStudioState(storage);
        await restored.InitializeAsync();

        Assert.Equal("#101010", restored.Applied.Dark.Background);
        Assert.Null(restored.StorageDiagnostic);

        storage.LoadResult = ThemeStudioStorageResult.Failure("Stored theme is corrupted.");
        var fallback = new ThemeStudioState(storage);
        await fallback.InitializeAsync();
        Assert.Equal(ShadcnThemePresets.BaseVegaNeutral.CreateTheme(), fallback.Applied);
        Assert.Equal("Stored theme is corrupted.", fallback.StorageDiagnostic);
    }

    [Fact]
    public async Task StorageFailureLeavesTheSessionFunctionalAndReportsTheDiagnostic()
    {
        var storage = new RecordingStorage { SaveDiagnostic = "Storage quota exceeded." };
        var state = new ThemeStudioState(storage);
        state.SetToken(ThemeStudioScheme.Light, "primary", "#123456");

        await state.PersistAsync();

        Assert.Equal("#123456", state.Applied.Light.Primary);
        Assert.Equal("Storage quota exceeded.", state.StorageDiagnostic);
    }

    [Theory]
    [InlineData("{not-json", "could not be restored")]
    [InlineData("{\"schemaVersion\": 999}", "schema version 999")]
    public async Task BrowserStorageReportsCorruptionAndUnsupportedVersions(string storedJson, string diagnostic)
    {
        var storage = new ThemeStudioStorage(new StaticStorageJsRuntime(storedJson));

        var result = await storage.LoadAsync();

        Assert.False(result.Succeeded);
        Assert.Null(result.Theme);
        Assert.Contains(diagnostic, result.Diagnostic, StringComparison.OrdinalIgnoreCase);
    }

    private static ThemeStudioState CreateState() => new(new RecordingStorage());

    private sealed class RecordingStorage : IThemeStudioStorage
    {
        public ThemeStudioStorageResult? LoadResult { get; set; }
        public string? SaveDiagnostic { get; set; }
        private ShadcnTheme? _theme;

        public ValueTask<ThemeStudioStorageResult> LoadAsync() =>
            ValueTask.FromResult(LoadResult ?? ThemeStudioStorageResult.Success(_theme));

        public ValueTask<ThemeStudioStorageResult> SaveAsync(ShadcnTheme theme)
        {
            if (SaveDiagnostic is not null)
                return ValueTask.FromResult(ThemeStudioStorageResult.Failure(SaveDiagnostic));
            _theme = theme;
            return ValueTask.FromResult(ThemeStudioStorageResult.Success(theme));
        }
    }

    private sealed class StaticStorageJsRuntime(string? storedJson) : IJSRuntime
    {
        public ValueTask<TValue> InvokeAsync<TValue>(string identifier, object?[]? args) =>
            InvokeAsync<TValue>(identifier, CancellationToken.None, args);

        public ValueTask<TValue> InvokeAsync<TValue>(string identifier, CancellationToken cancellationToken, object?[]? args)
        {
            Assert.Equal("localStorage.getItem", identifier);
            return ValueTask.FromResult((TValue)(object?)storedJson!);
        }
    }
}

public sealed class ThemeStudioComponentTests : BunitContext, IAsyncLifetime
{
    public ThemeStudioComponentTests()
    {
        JSInterop.Mode = JSRuntimeMode.Loose;
        Services.AddMalievShadcn();
        Services.AddSingleton<IThemeStudioStorage>(new NoOpStorage());
        Services.AddSingleton<ThemeStudioState>();
        Services.AddSingleton<MockSiteState>();
    }

    [Fact]
    public void InspectorExposesEveryTokenAndMetricWithStableAccessibleLabelsAndTestIds()
    {
        var state = Services.GetRequiredService<ThemeStudioState>();
        var cut = Render<ThemeInspector>(parameters => parameters.Add(component => component.State, state));

        Assert.Equal(70, cut.FindAll("[data-theme-token]").Count);
        Assert.Equal(19, cut.FindAll("[data-theme-metric]")
            .Select(element => element.GetAttribute("data-theme-metric"))
            .Distinct(StringComparer.Ordinal)
            .Count());
        Assert.NotEmpty(cut.FindAll("[data-testid='theme-undo']"));
        Assert.NotEmpty(cut.FindAll("[data-testid='theme-reset-all']"));
        Assert.NotEmpty(cut.FindAll("[data-testid='theme-validation-summary']"));
        Assert.All(cut.FindAll("[data-theme-token], [data-theme-metric]"), element =>
            Assert.False(string.IsNullOrWhiteSpace(element.GetAttribute("aria-label"))));
    }

    [Fact]
    public void PreviewToolbarUsesExactViewportWidthsAndDoesNotCreateThemeHistory()
    {
        var state = Services.GetRequiredService<ThemeStudioState>();
        var cut = Render<PreviewToolbar>(parameters => parameters.Add(component => component.State, state));

        cut.Find("[data-testid='viewport-mobile']").Click();
        Assert.Equal(390, state.Viewport.Width);
        cut.Find("[data-testid='locale-thai']").Click();
        cut.Find("[data-testid='direction-rtl']").Click();
        Assert.Equal(ThemeStudioLocale.Thai, state.Locale);
        Assert.Equal(ShadcnDirection.RightToLeft, state.Direction);
        Assert.False(state.CanUndo);
    }

    [Fact]
    public void PreviewToolbarOffersWhitelistedGoogleFontPresetsAndAppliesTheSelectedMetric()
    {
        var state = Services.GetRequiredService<ThemeStudioState>();
        var cut = Render<PreviewToolbar>(parameters => parameters.Add(component => component.State, state));

        Assert.Equal(4, ThemeStudioFontPreset.All.Count);
        Assert.NotEmpty(cut.FindAll("[data-testid='font-family-select']"));
        Assert.NotEmpty(cut.FindAll("[data-testid='monospace-font-family-select']"));

        state.SetFontFamily(ThemeStudioFontPreset.NotoSansThai.Id);

        Assert.Equal(ThemeStudioFontPreset.NotoSansThai.CssStack, state.GetMetricEditorValue("fontFamily"));

        state.SetMonospaceFontFamily(ThemeStudioFontPreset.JetBrainsMono.Id);
        Assert.Equal(ThemeStudioFontPreset.JetBrainsMono.CssStack, state.GetMetricEditorValue("monospaceFontFamily"));
    }

    [Fact]
    public void GeneratorControlsAndCodeActionAreExposedWithStableAccessibleHooks()
    {
        var state = Services.GetRequiredService<ThemeStudioState>();
        var inspector = Render<ThemeInspector>(parameters => parameters.Add(component => component.State, state));

        Assert.NotEmpty(inspector.FindAll("[data-testid='theme-generator-options']"));
        Assert.NotEmpty(inspector.FindAll("[data-testid='theme-style-select']"));
        Assert.NotEmpty(inspector.FindAll("[data-testid='theme-base-color-select']"));
        Assert.NotEmpty(inspector.FindAll("[data-testid='theme-icon-library-select']"));
        Assert.NotEmpty(inspector.FindAll("[data-testid='theme-radius-select']"));
        Assert.NotEmpty(inspector.FindAll("[data-testid='theme-menu-accent-select']"));
        Assert.NotEmpty(inspector.FindAll("[data-testid='theme-menu-color-select']"));
        Assert.NotEmpty(inspector.FindAll("[data-testid='theme-code-open']"));
    }

    [Fact]
    public void CodeDialogShowsBothPortableJsonAndReadyToPasteCSharp()
    {
        var state = Services.GetRequiredService<ThemeStudioState>();
        var cut = Render<ThemeCodeDialog>(parameters => parameters
            .Add(component => component.State, state)
            .Add(component => component.Open, true));

        Assert.Contains("Generated by Maliev.ShadcnBlazor Theme Studio", cut.Find("[data-testid='theme-code-content']").TextContent, StringComparison.Ordinal);
        Assert.NotEmpty(cut.FindAll("[data-testid='theme-code-tab-csharp']"));
        Assert.NotEmpty(cut.FindAll("[data-testid='theme-code-tab-json']"));
        Assert.NotEmpty(cut.FindAll("[data-testid='theme-json-download']"));
        Assert.NotEmpty(cut.FindAll("[data-testid='theme-generator-import-file']"));
    }

    [Fact]
    public async Task ThemeStudioSwitchingMockupsResetsTheDestinationFixtureBeforeRender()
    {
        var theme = Services.GetRequiredService<ThemeStudioState>();
        var mockSites = Services.GetRequiredService<MockSiteState>();
        var cut = Render<ThemeStudio>();
        mockSites.SetCustomerQuery("วริศรา");
        await cut.InvokeAsync(() => theme.SetSelectedMockup(ThemeStudioMockup.CustomerWorkspace));

        cut.WaitForAssertion(() => Assert.Equal(string.Empty, mockSites.Customers.Query));
        mockSites.SetCustomerQuery("กานต์ชนก");
        await cut.InvokeAsync(() => theme.SetSelectedMockup(ThemeStudioMockup.OperationsDashboard));
        await cut.InvokeAsync(() => theme.SetSelectedMockup(ThemeStudioMockup.CustomerWorkspace));

        cut.WaitForAssertion(() => Assert.Equal(string.Empty, mockSites.Customers.Query));
    }

    [Fact]
    public async Task PersistenceCoalescesRapidChangesAndEventuallySavesTheNewestValidTheme()
    {
        var storage = new DelayedStorage();
        var state = new ThemeStudioState(storage);
        Services.AddSingleton<IThemeStudioStorage>(storage);
        Services.AddSingleton(state);
        var cut = Render<ThemeStudio>();

        await cut.InvokeAsync(() => state.SetToken(ThemeStudioScheme.Light, "primary", "#111111"));
        cut.WaitForAssertion(() => Assert.Equal(1, storage.SaveCount));
        await cut.InvokeAsync(() => state.SetToken(ThemeStudioScheme.Light, "primary", "#222222"));
        storage.ReleaseFirstSave();

        cut.WaitForAssertion(() =>
        {
            Assert.Equal(2, storage.SaveCount);
            Assert.Equal("#222222", storage.LastSaved?.Light.Primary);
        });
    }

    [Fact]
    public async Task PersistenceCoalescesABurstDuringAnActiveSaveToOneNewestCatchUpSave()
    {
        var storage = new DelayedStorage();
        var state = new ThemeStudioState(storage);
        Services.AddSingleton<IThemeStudioStorage>(storage);
        Services.AddSingleton(state);
        var cut = Render<ThemeStudio>();

        await cut.InvokeAsync(() => state.SetToken(ThemeStudioScheme.Light, "primary", "#111111"));
        cut.WaitForAssertion(() => Assert.Equal(1, storage.SaveCount));
        await cut.InvokeAsync(() => state.SetToken(ThemeStudioScheme.Light, "primary", "#222222"));
        await cut.InvokeAsync(() => state.SetToken(ThemeStudioScheme.Light, "primary", "#333333"));
        await cut.InvokeAsync(() => state.SetToken(ThemeStudioScheme.Light, "primary", "#444444"));
        storage.ReleaseFirstSave();

        cut.WaitForAssertion(() =>
        {
            Assert.Equal(2, storage.SaveCount);
            Assert.Equal("#444444", storage.LastSaved?.Light.Primary);
        });
    }

    [Fact]
    public async Task DisposedThemeStudioDoesNotScheduleAnotherSave()
    {
        var storage = new DelayedStorage();
        var state = new ThemeStudioState(storage);
        Services.AddSingleton<IThemeStudioStorage>(storage);
        Services.AddSingleton(state);
        var cut = Render<ThemeStudio>();

        await cut.InvokeAsync(() => state.SetToken(ThemeStudioScheme.Light, "primary", "#111111"));
        await storage.FirstSaveStarted.WaitAsync(TimeSpan.FromSeconds(10));
        Assert.Equal(1, storage.SaveCount);
        cut.Instance.Dispose();
        cut.Dispose();
        storage.ReleaseFirstSave();
        await storage.FirstSaveCompleted;

        var countAfterDisposal = storage.SaveCount;
        state.SetToken(ThemeStudioScheme.Light, "primary", "#222222");
        await Task.Yield();
        Assert.Equal(countAfterDisposal, storage.SaveCount);
    }

    [Fact]
    public async Task DisposalDuringABlockedSavePreventsCatchUpSaveAndPostDisposeWork()
    {
        var storage = new DelayedStorage();
        var state = new ThemeStudioState(storage);
        Services.AddSingleton<IThemeStudioStorage>(storage);
        Services.AddSingleton(state);
        var cut = Render<ThemeStudio>();

        await cut.InvokeAsync(() => state.SetToken(ThemeStudioScheme.Light, "primary", "#111111"));
        cut.WaitForAssertion(() => Assert.Equal(1, storage.SaveCount));
        await cut.InvokeAsync(() => state.SetToken(ThemeStudioScheme.Light, "primary", "#222222"));
        cut.Dispose();
        storage.ReleaseFirstSave();
        await storage.FirstSaveCompleted;

        Assert.Equal(1, storage.SaveCount);
    }

    [Fact]
    public async Task AChangeAfterACompletedSynchronousSaveSchedulesNormally()
    {
        var storage = new DelayedStorage(releaseFirstImmediately: true);
        var state = new ThemeStudioState(storage);
        Services.AddSingleton<IThemeStudioStorage>(storage);
        Services.AddSingleton(state);
        var cut = Render<ThemeStudio>();

        await cut.InvokeAsync(() => state.SetToken(ThemeStudioScheme.Light, "primary", "#111111"));
        cut.WaitForAssertion(() => Assert.Equal(1, storage.SaveCount));
        await cut.InvokeAsync(() => state.SetToken(ThemeStudioScheme.Light, "primary", "#222222"));

        cut.WaitForAssertion(() =>
        {
            Assert.Equal(2, storage.SaveCount);
            Assert.Equal("#222222", storage.LastSaved?.Light.Primary);
        });
    }

    [Fact]
    public async Task UnexpectedPersistenceFaultIsObservedDiagnosedAndDoesNotBlockALaterSave()
    {
        var storage = new FaultingStorage();
        var state = new ThemeStudioState(storage);
        Services.AddSingleton<IThemeStudioStorage>(storage);
        Services.AddSingleton(state);
        var cut = Render<ThemeStudio>();

        await cut.InvokeAsync(() => state.SetToken(ThemeStudioScheme.Light, "primary", "#111111"));
        storage.FailFirstSave(new InvalidOperationException("storage transport failed"));

        cut.WaitForAssertion(() => Assert.Contains("storage transport failed", cut.Markup, StringComparison.Ordinal));
        Assert.Equal(1, storage.SaveCount);

        await cut.InvokeAsync(() => state.SetToken(ThemeStudioScheme.Light, "primary", "#222222"));
        cut.WaitForAssertion(() =>
        {
            Assert.Equal(2, storage.SaveCount);
            Assert.Equal("#222222", storage.LastSaved?.Light.Primary);
        });
    }

    [Fact]
    public async Task PersistenceFaultCompletingAfterDisposalHasNoDiagnosticRenderOrCatchUp()
    {
        var storage = new FaultingStorage();
        var state = new ThemeStudioState(storage);
        Services.AddSingleton<IThemeStudioStorage>(storage);
        Services.AddSingleton(state);
        var cut = Render<ThemeStudio>();

        await cut.InvokeAsync(() => state.SetToken(ThemeStudioScheme.Light, "primary", "#111111"));
        cut.WaitForAssertion(() => Assert.Equal(1, storage.SaveCount));
        await cut.InvokeAsync(() => state.SetToken(ThemeStudioScheme.Light, "primary", "#222222"));
        var rendersBeforeDisposal = cut.RenderCount;
        cut.Instance.Dispose();
        cut.Dispose();

        storage.FailFirstSave(new InvalidOperationException("late storage failure"));
        await storage.FirstSaveCompleted;

        Assert.Equal(1, storage.SaveCount);
        Assert.Equal(rendersBeforeDisposal, cut.RenderCount);
        Assert.Null(state.StorageDiagnostic);
    }

    private sealed class NoOpStorage : IThemeStudioStorage
    {
        public ValueTask<ThemeStudioStorageResult> LoadAsync() => ValueTask.FromResult(ThemeStudioStorageResult.Success(null));
        public ValueTask<ThemeStudioStorageResult> SaveAsync(ShadcnTheme theme) => ValueTask.FromResult(ThemeStudioStorageResult.Success(theme));
    }

    private sealed class DelayedStorage(bool releaseFirstImmediately = false) : IThemeStudioStorage
    {
        private readonly TaskCompletionSource _firstSaveRelease = new(TaskCreationOptions.RunContinuationsAsynchronously);
        private readonly TaskCompletionSource _firstSaveStarted = new(TaskCreationOptions.RunContinuationsAsynchronously);
        private readonly TaskCompletionSource _firstSaveCompleted = new(TaskCreationOptions.RunContinuationsAsynchronously);
        public int SaveCount { get; private set; }
        public ShadcnTheme? LastSaved { get; private set; }
        public Task FirstSaveStarted => _firstSaveStarted.Task;
        public Task FirstSaveCompleted => _firstSaveCompleted.Task;

        public ValueTask<ThemeStudioStorageResult> LoadAsync() => LoadLaterAsync();

        private async ValueTask<ThemeStudioStorageResult> LoadLaterAsync()
        {
            await Task.Yield();
            return ThemeStudioStorageResult.Success(null);
        }

        public async ValueTask<ThemeStudioStorageResult> SaveAsync(ShadcnTheme theme)
        {
            SaveCount++;
            LastSaved = theme;
            _firstSaveStarted.TrySetResult();
            if (SaveCount == 1 && !releaseFirstImmediately)
                await _firstSaveRelease.Task;
            if (SaveCount == 1)
                _firstSaveCompleted.TrySetResult();
            return ThemeStudioStorageResult.Success(theme);
        }

        public void ReleaseFirstSave() => _firstSaveRelease.TrySetResult();
    }

    private sealed class FaultingStorage : IThemeStudioStorage
    {
        private readonly TaskCompletionSource<ThemeStudioStorageResult> _firstSave = new(TaskCreationOptions.RunContinuationsAsynchronously);
        private readonly TaskCompletionSource _firstSaveCompleted = new(TaskCreationOptions.RunContinuationsAsynchronously);
        public int SaveCount { get; private set; }
        public ShadcnTheme? LastSaved { get; private set; }
        public Task FirstSaveCompleted => _firstSaveCompleted.Task;

        public ValueTask<ThemeStudioStorageResult> LoadAsync() =>
            ValueTask.FromResult(ThemeStudioStorageResult.Success(null));

        public async ValueTask<ThemeStudioStorageResult> SaveAsync(ShadcnTheme theme)
        {
            SaveCount++;
            LastSaved = theme;
            if (SaveCount > 1)
                return ThemeStudioStorageResult.Success(theme);
            try
            {
                return await _firstSave.Task;
            }
            finally
            {
                _firstSaveCompleted.TrySetResult();
            }
        }

        public void FailFirstSave(Exception exception) => _firstSave.TrySetException(exception);
    }

    Task IAsyncLifetime.InitializeAsync() => Task.CompletedTask;

    async Task IAsyncLifetime.DisposeAsync() => await DisposeAsync();
}
