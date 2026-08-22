using Bunit;
using Maliev.ShadcnBlazor.Components.Forms;

namespace Maliev.ShadcnBlazor.Tests.Components.Forms;

public sealed class SecretInputTests : BunitContext
{
    public SecretInputTests() => JSInterop.Mode = JSRuntimeMode.Loose;

    [Fact]
    public void MasksOnlyTheSuffixUntilTheFieldIsEditedOrRevealed()
    {
        var cut = Render<ShadcnSecretInput>(parameters => parameters
            .Add(component => component.Value, "sk-abcdef")
            .Add(component => component.MaskStart, 3));

        var input = cut.Find("[data-slot='input-group-control']");
        Assert.Equal("sk-••••••", input.GetAttribute("value"));
        Assert.Equal("text", input.GetAttribute("type"));
        Assert.True(input.HasAttribute("readonly"));

        input.Focus();
        Assert.Equal("sk-abcdef", input.GetAttribute("value"));
        Assert.Equal("password", input.GetAttribute("type"));
        Assert.False(input.HasAttribute("readonly"));

        cut.Find("[data-secret-input-toggle]").Click();
        Assert.Equal("true", cut.Find("[data-secret-input-toggle]").GetAttribute("aria-pressed"));
        Assert.Equal("text", input.GetAttribute("type"));
        Assert.Equal("sk-abcdef", input.GetAttribute("value"));
    }

    [Fact]
    public void HidesAnnouncementsAndKeepsSecretValuesLeftToRightByDefault()
    {
        var cut = Render<ShadcnSecretInput>(parameters => parameters
            .Add(component => component.Value, "sk-abcdef")
            .Add(component => component.MaskStart, 3));

        Assert.Equal("ltr", cut.Find("[data-slot='input-group-control']").GetAttribute("dir"));
        Assert.Contains("shadcn-sr-only", cut.Find("[data-slot='secret-input-status']").ClassList);
    }

    [Fact]
    public void AllowsCallersToOverrideSecretValueDirection()
    {
        var cut = Render<ShadcnSecretInput>(parameters => parameters
            .Add(component => component.AdditionalAttributes, new Dictionary<string, object> { ["dir"] = "rtl" }));

        Assert.Equal("rtl", cut.Find("[data-slot='input-group-control']").GetAttribute("dir"));
    }

    [Fact]
    public void UncontrolledEditingPreservesTheNewSecretWithoutAnnouncingIt()
    {
        var cut = Render<ShadcnSecretInput>(parameters => parameters
            .Add(component => component.MaskStart, 3));

        var input = cut.Find("[data-slot='input-group-control']");
        input.Focus();
        input.Input("sk-new-value");
        input.Blur();

        Assert.Equal("sk-•••••••••", input.GetAttribute("value"));
        Assert.DoesNotContain("sk-new-value", cut.Find("[data-slot='secret-input-status']").TextContent, StringComparison.Ordinal);

        cut.Render(parameters => parameters
            .Add(component => component.Value, null)
            .Add(component => component.MaskStart, 3));

        Assert.Equal("sk-•••••••••", cut.Find("[data-slot='input-group-control']").GetAttribute("value"));
    }

    [Fact]
    public void ReadOnlySecretsCanBeRevealedButCannotBeEdited()
    {
        var cut = Render<ShadcnSecretInput>(parameters => parameters
            .Add(component => component.Value, "sk-abcdef")
            .Add(component => component.MaskStart, 3)
            .Add(component => component.ReadOnly, true));

        var input = cut.Find("[data-slot='input-group-control']");
        input.Focus();
        Assert.True(input.HasAttribute("readonly"));
        cut.Find("[data-secret-input-toggle]").Click();
        Assert.Equal("sk-abcdef", input.GetAttribute("value"));
        Assert.True(input.HasAttribute("readonly"));
    }

    [Fact]
    public void RejectsInvalidMaskConfiguration()
    {
        Assert.ThrowsAny<Exception>(() => Render<ShadcnSecretInput>(parameters => parameters
            .Add(component => component.MaskStart, -1)));
        Assert.ThrowsAny<Exception>(() => Render<ShadcnSecretInput>(parameters => parameters
            .Add(component => component.MaskCharacter, "ab")));
    }
}
