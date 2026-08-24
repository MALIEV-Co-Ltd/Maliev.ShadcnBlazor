using Bunit;
using Maliev.ShadcnBlazor.Components.Styling;

namespace Maliev.ShadcnBlazor.Tests.Components.Styling;

public sealed class VisualStyleScopeTests : BunitContext
{
    [Fact]
    public void DefaultsExposeAnExplicitLayerInheritanceContract()
    {
        var cut = Render<ShadcnVisualStyleScope>(parameters => parameters
            .AddChildContent("Fixture"));

        var root = cut.Find("[data-slot='visual-style-scope']");
        Assert.Equal("inherit", root.GetAttribute("data-visual-style"));
        Assert.Equal("inherit", root.GetAttribute("data-color-treatment"));
        Assert.Equal("inherit", root.GetAttribute("data-depth"));
        Assert.Equal("inherit", root.GetAttribute("data-motion"));
        Assert.Equal("default", root.GetAttribute("data-intensity"));
        Assert.Equal("Fixture", root.TextContent);
    }

    [Theory]
    [InlineData(ShadcnVisualStyle.Minimal, "minimal")]
    [InlineData(ShadcnVisualStyle.Glass, "glass")]
    [InlineData(ShadcnVisualStyle.NeoBrutalist, "neo-brutalist")]
    [InlineData(ShadcnVisualStyle.LiquidGlass, "liquid-glass")]
    public void VisualStylesEmitStableKebabCaseValues(ShadcnVisualStyle style, string expected)
    {
        var cut = Render<ShadcnVisualStyleScope>(parameters => parameters
            .Add(component => component.VisualStyle, style));

        Assert.Equal(expected, cut.Find("[data-slot='visual-style-scope']").GetAttribute("data-visual-style"));
    }

    [Fact]
    public void ExplicitLayersAndCallerAttributesCoexistWithoutOverridingOwnedAttributes()
    {
        var cut = Render<ShadcnVisualStyleScope>(parameters => parameters
            .Add(component => component.VisualStyle, ShadcnVisualStyle.LiquidGlass)
            .Add(component => component.ColorTreatment, ShadcnColorTreatment.VibrantDark)
            .Add(component => component.Depth, ShadcnDepthTreatment.Spatial)
            .Add(component => component.Motion, ShadcnMotionTreatment.Expressive)
            .Add(component => component.Intensity, ShadcnStyleIntensity.Strong)
            .Add(component => component.Class, "production-surface")
            .Add(component => component.AdditionalAttributes, new Dictionary<string, object>
            {
                ["data-slot"] = "wrong",
                ["data-depth"] = "flat",
                ["aria-label"] = "Styled production workspace"
            }));

        var root = cut.Find("[data-slot='visual-style-scope']");
        Assert.Equal("visual-style-scope", root.GetAttribute("data-slot"));
        Assert.Equal("liquid-glass", root.GetAttribute("data-visual-style"));
        Assert.Equal("vibrant-dark", root.GetAttribute("data-color-treatment"));
        Assert.Equal("spatial", root.GetAttribute("data-depth"));
        Assert.Equal("expressive", root.GetAttribute("data-motion"));
        Assert.Equal("strong", root.GetAttribute("data-intensity"));
        Assert.Equal("Styled production workspace", root.GetAttribute("aria-label"));
        Assert.Contains("shadcn-visual-style-scope", root.ClassList);
        Assert.Contains("production-surface", root.ClassList);
    }

    [Fact]
    public void NestedScopesKeepTheirOwnLayerValues()
    {
        var cut = Render<ShadcnVisualStyleScope>(parameters => parameters
            .Add(component => component.VisualStyle, ShadcnVisualStyle.Glass)
            .Add(component => component.ColorTreatment, ShadcnColorTreatment.VibrantDark)
            .AddChildContent<ShadcnVisualStyleScope>(nested => nested
                .Add(component => component.VisualStyle, ShadcnVisualStyle.Minimal)
                .Add(component => component.Depth, ShadcnDepthTreatment.Flat)
                .AddChildContent("Nested")));

        var scopes = cut.FindAll("[data-slot='visual-style-scope']");
        Assert.Equal("glass", scopes[0].GetAttribute("data-visual-style"));
        Assert.Equal("vibrant-dark", scopes[0].GetAttribute("data-color-treatment"));
        Assert.Equal("minimal", scopes[1].GetAttribute("data-visual-style"));
        Assert.Equal("inherit", scopes[1].GetAttribute("data-color-treatment"));
        Assert.Equal("flat", scopes[1].GetAttribute("data-depth"));
    }
}
