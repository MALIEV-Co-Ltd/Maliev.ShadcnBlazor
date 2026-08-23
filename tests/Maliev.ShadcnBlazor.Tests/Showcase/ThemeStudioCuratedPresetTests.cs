using Maliev.ShadcnBlazor.Showcase.Theming;
using Maliev.ShadcnBlazor.Showcase.Theming.Presets;
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
    }

    [Fact]
    public void ShuffleSelectsOnlyAnotherCuratedPresetAndCreatesOneUndoEntry()
    {
        var catalog = new ThemeStudioPresetCatalog();
        var state = new ThemeStudioState(new NoOpStorage(), new ThemeStudioWorkbenchState(), catalog);
        var initial = state.SelectedPresetId;

        var shuffled = state.ShufflePreset();

        Assert.NotEqual(initial, shuffled);
        Assert.Contains(catalog.All, item => item.Id == shuffled);
        Assert.True(state.CanUndo);
        state.Undo();
        Assert.Equal(initial, state.SelectedPresetId);
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
