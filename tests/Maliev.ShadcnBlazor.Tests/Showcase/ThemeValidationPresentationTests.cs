using Maliev.ShadcnBlazor.Showcase.Theming;

namespace Maliev.ShadcnBlazor.Tests.Showcase;

public sealed class ThemeValidationPresentationTests
{
    [Theory]
    [InlineData(0, 0, "Ready to export")]
    [InlineData(0, 16, "Ready to export · 16 advisories")]
    [InlineData(2, 16, "Export blocked · 2 errors")]
    public void StatusLabelExplainsExportConsequence(int errors, int advisories, string expected)
    {
        Assert.Equal(expected, ThemeValidationPresentation.StatusLabel(errors, advisories));
    }
}
