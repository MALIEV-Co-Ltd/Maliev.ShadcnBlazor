using Maliev.ShadcnBlazor.Showcase.Theming;
using Maliev.ShadcnBlazor.Showcase.Theming.Presets;
using Maliev.ShadcnBlazor.Components.Styling;
using Maliev.ShadcnBlazor.Theming;

namespace Maliev.ShadcnBlazor.Tests.Showcase;

public sealed class ThemeStudioCuratedPresetTests
{
    [Fact]
    public void CatalogContainsReviewedUniqueMaterializedDocuments()
    {
        var catalog = new ThemeStudioPresetCatalog();

        Assert.Equal(12, catalog.All.Count);
        Assert.Equal(catalog.All.Count, catalog.All.Select(item => item.Id).Distinct(StringComparer.Ordinal).Count());
        Assert.All(catalog.All, preset =>
        {
            Assert.False(string.IsNullOrWhiteSpace(preset.Accent));
            Assert.False(string.IsNullOrWhiteSpace(preset.Density));
            Assert.False(string.IsNullOrWhiteSpace(preset.BorderTreatment));
            Assert.False(string.IsNullOrWhiteSpace(preset.SurfaceTreatment));
            Assert.False(string.IsNullOrWhiteSpace(preset.ControlTreatment));
            Assert.False(string.IsNullOrWhiteSpace(preset.MotionProfile));
            Assert.True(ShadcnThemeDocumentValidator.Validate(preset.Document).IsValid);
            Assert.Equal(preset.Id, preset.Document.Application.Preset);
        });

        Assert.Equal(
            Enum.GetValues<ShadcnVisualStyle>().Where(value => value != ShadcnVisualStyle.Inherit).Order(),
            catalog.All.Select(item => item.VisualStyle).Distinct().Order());
        var graphite = Assert.Single(catalog.All, item => item.Id == "graphite-control");
        Assert.Equal(ShadcnColorTreatment.Inherit, graphite.ColorTreatment);
        Assert.All(catalog.All, item => Assert.Equal(ShadcnColorTreatment.Inherit, item.ColorTreatment));
        Assert.All(catalog.All, preset =>
        {
            Assert.NotEqual(ShadcnVisualStyle.Inherit, preset.VisualStyle);
            Assert.True(Enum.IsDefined(preset.DepthTreatment));
            Assert.True(Enum.IsDefined(preset.MotionTreatment));
            Assert.True(Enum.IsDefined(preset.StyleIntensity));
        });
    }

    [Fact]
    public void ShuffleSelectsAnotherPresetAndAllFontFamiliesAsOneUndoEntry()
    {
        var catalog = new ThemeStudioPresetCatalog();
        var state = new ThemeStudioState(new NoOpStorage(), new ThemeStudioWorkbenchState(), catalog);
        var initial = state.SelectedPresetId;
        var initialTypography = state.Typography;

        var shuffled = state.ShufflePreset();

        Assert.NotEqual(initial, shuffled);
        Assert.Contains(catalog.All, item => item.Id == shuffled);
        Assert.NotEqual(initialTypography.Body.Family, state.Typography.Body.Family);
        Assert.NotEqual(initialTypography.ThaiFallback.Family, state.Typography.ThaiFallback.Family);
        Assert.NotEqual(initialTypography.Code.Family, state.Typography.Code.Family);
        Assert.True(state.CanUndo);
        state.Undo();
        Assert.Equal(initial, state.SelectedPresetId);
        Assert.Equal(initialTypography, state.Typography);
        Assert.False(state.CanUndo);
    }

    [Fact]
    public void ManualVisualTreatmentsAreTransactionalAndUndoable()
    {
        var state = new ThemeStudioState(new NoOpStorage());

        state.SetVisualStyle(ShadcnVisualStyle.Glass);
        state.SetColorTreatment(ShadcnColorTreatment.VibrantDark);
        state.SetDepthTreatment(ShadcnDepthTreatment.Spatial);
        state.SetMotionTreatment(ShadcnMotionTreatment.Expressive);
        state.SetStyleIntensity(ShadcnStyleIntensity.Strong);

        Assert.Equal(ShadcnVisualStyle.Glass, state.VisualStyle);
        Assert.Equal(ShadcnColorTreatment.VibrantDark, state.ColorTreatment);
        Assert.Equal(ShadcnDepthTreatment.Spatial, state.DepthTreatment);
        Assert.Equal(ShadcnMotionTreatment.Expressive, state.MotionTreatment);
        Assert.Equal(ShadcnStyleIntensity.Strong, state.StyleIntensity);

        Assert.True(state.Undo());
        Assert.Equal(ShadcnStyleIntensity.Default, state.StyleIntensity);
        Assert.True(state.Redo());
        Assert.Equal(ShadcnStyleIntensity.Strong, state.StyleIntensity);
    }

    [Fact]
    public void PresetCopiesAreDefensiveAndByteStable()
    {
        var preset = new ThemeStudioPresetCatalog().All[0];
        var first = preset.CreateDocument();
        var second = preset.CreateDocument();

        Assert.NotSame(first, second);
        Assert.Equal(ShadcnThemeDocumentSerializer.Serialize(first), ShadcnThemeDocumentSerializer.Serialize(second));
    }

    private sealed class NoOpStorage : IThemeStudioStorage
    {
        public ValueTask<ThemeStudioStorageResult> LoadAsync() => ValueTask.FromResult(ThemeStudioStorageResult.Success(null));
        public ValueTask<ThemeStudioStorageResult> SaveAsync(ShadcnThemeDocument document) => ValueTask.FromResult(ThemeStudioStorageResult.Success(document));
    }
}
