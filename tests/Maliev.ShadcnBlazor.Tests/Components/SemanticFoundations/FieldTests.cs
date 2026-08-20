using Bunit;
using Maliev.ShadcnBlazor.Components.Forms;
using Microsoft.AspNetCore.Components;

namespace Maliev.ShadcnBlazor.Tests.Components.SemanticFoundations;

public sealed class FieldTests : BunitContext
{
    [Fact]
    public void LabelStylesMatchPinnedDisabledAndPlatformBehavior()
    {
        var css = File.ReadAllText(Path.Combine(FindRoot(), "src", "Maliev.ShadcnBlazor", "wwwroot", "css", "shadcn-semantic-foundations.css"));

        Assert.Contains("line-height: 1", css, StringComparison.Ordinal);
        Assert.Contains(".shadcn-label:has(+ :disabled)", css, StringComparison.Ordinal);
        Assert.Contains("[data-disabled=\"true\"] .shadcn-label", css, StringComparison.Ordinal);
        Assert.Contains("cursor: not-allowed", css, StringComparison.Ordinal);
        Assert.Contains("@media (forced-colors: active)", css, StringComparison.Ordinal);
        Assert.Contains("color: GrayText", css, StringComparison.Ordinal);
    }

    [Theory]
    [InlineData(false, "help", "error", "help")]
    [InlineData(true, "help", "error", "help error")]
    [InlineData(true, null, "error", "error")]
    [InlineData(false, null, "error", null)]
    public void FieldContextBuildsAccessibleDescriptionIds(
        bool invalid,
        string? descriptionId,
        string? errorId,
        string? expected)
    {
        var context = new ShadcnFieldContext(invalid, false, descriptionId, errorId);

        Assert.Equal(expected, context.AriaDescribedBy);
    }

    [Fact]
    public void LabelRendersNativeAssociationAndProtectsOwnedAttributes()
    {
        var cut = Render<ShadcnLabel>(parameters => parameters
            .Add(x => x.For, "email")
            .Add(x => x.Class, "consumer-label")
            .Add(x => x.AdditionalAttributes, new Dictionary<string, object>
            {
                ["for"] = "wrong",
                ["data-slot"] = "wrong",
                ["aria-label"] = "Email address"
            })
            .AddChildContent("Email"));

        var label = cut.Find("label[data-slot='label']");
        Assert.Equal("email", label.GetAttribute("for"));
        Assert.Equal("Email address", label.GetAttribute("aria-label"));
        Assert.Contains("shadcn-label", label.ClassList);
        Assert.Contains("consumer-label", label.ClassList);
        Assert.Equal("Email", label.TextContent);
    }

    [Fact]
    public void FieldSetAndLegendUseNativeSemantics()
    {
        var cut = Render<ShadcnFieldSet>(parameters => parameters
            .Add(x => x.Disabled, true)
            .AddChildContent<ShadcnFieldLegend>(legend => legend
                .Add(x => x.Variant, ShadcnFieldLegendVariant.Label)
                .AddChildContent("Preferences")));

        var fieldset = cut.Find("fieldset[data-slot='field-set']");
        Assert.True(fieldset.HasAttribute("disabled"));
        var legend = fieldset.QuerySelector("legend[data-slot='field-legend']")!;
        Assert.Equal("label", legend.GetAttribute("data-variant"));
        Assert.Equal("Preferences", legend.TextContent);
    }

    [Theory]
    [InlineData(ShadcnFieldOrientation.Vertical, "vertical")]
    [InlineData(ShadcnFieldOrientation.Horizontal, "horizontal")]
    [InlineData(ShadcnFieldOrientation.Responsive, "responsive")]
    public void FieldRendersOrientationAndState(ShadcnFieldOrientation orientation, string value)
    {
        var cut = Render<ShadcnField>(parameters => parameters
            .Add(x => x.Orientation, orientation)
            .Add(x => x.Invalid, true)
            .Add(x => x.Disabled, true)
            .Add(x => x.DescriptionId, "email-help")
            .Add(x => x.ErrorId, "email-error")
            .AddChildContent("content"));

        var field = cut.Find("[data-slot='field']");
        Assert.Equal("group", field.GetAttribute("role"));
        Assert.Equal(value, field.GetAttribute("data-orientation"));
        Assert.Equal("true", field.GetAttribute("data-invalid"));
        Assert.Equal("true", field.GetAttribute("data-disabled"));
        Assert.Equal("email-help", field.GetAttribute("data-description-id"));
        Assert.Equal("email-error", field.GetAttribute("data-error-id"));
    }

    [Fact]
    public void FieldCascadesStateAndIdsToDescriptionAndError()
    {
        ShadcnFieldContext? observed = null;
        var cut = Render<ShadcnField>(parameters => parameters
            .Add(x => x.Invalid, true)
            .Add(x => x.Disabled, true)
            .Add(x => x.DescriptionId, "name-help")
            .Add(x => x.ErrorId, "name-error")
            .AddChildContent(builder =>
            {
                builder.OpenComponent<ShadcnFieldDescription>(0);
                builder.AddAttribute(1, nameof(ShadcnFieldDescription.ChildContent), (RenderFragment)(content => content.AddContent(0, "Use the legal name.")));
                builder.CloseComponent();
                builder.OpenComponent<ShadcnFieldError>(2);
                builder.AddAttribute(3, nameof(ShadcnFieldError.Errors), new[] { "Required", "Required", "Too short", " " });
                builder.CloseComponent();
                builder.OpenComponent<CaptureFieldContext>(4);
                builder.AddAttribute(5, nameof(CaptureFieldContext.OnCaptured), (Action<ShadcnFieldContext>)(value => observed = value));
                builder.CloseComponent();
            }));

        Assert.Equal("name-help", cut.Find("[data-slot='field-description']").Id);
        var error = cut.Find("[data-slot='field-error']");
        Assert.Equal("name-error", error.Id);
        Assert.Equal("alert", error.GetAttribute("role"));
        Assert.Equal(new[] { "Required", "Too short" }, error.QuerySelectorAll("li").Select(x => x.TextContent));
        Assert.Equal(new ShadcnFieldContext(true, true, "name-help", "name-error"), observed);
    }

    [Fact]
    public void FieldErrorRendersOneMessageWithoutListAndSuppressesEmptyOutput()
    {
        var single = Render<ShadcnFieldError>(parameters => parameters
            .Add(x => x.Errors, new[] { "Required", "Required" }));
        Assert.Equal("Required", single.Find("[data-slot='field-error']").TextContent);
        Assert.Empty(single.FindAll("li"));

        var empty = Render<ShadcnFieldError>(parameters => parameters
            .Add(x => x.Errors, new string?[] { null, "", " " }));
        Assert.Equal(string.Empty, empty.Markup);
    }

    [Fact]
    public void FieldErrorChildContentTakesPrecedenceOverErrors()
    {
        var cut = Render<ShadcnFieldError>(parameters => parameters
            .Add(x => x.Errors, new[] { "Ignored" })
            .AddChildContent("Caller-provided error"));

        Assert.Equal("Caller-provided error", cut.Find("[data-slot='field-error']").TextContent);
    }

    [Fact]
    public void FieldCompositionPrimitivesExposeExpectedSlots()
    {
        var content = Render<ShadcnFieldContent>(parameters => parameters.AddChildContent("content"));
        var title = Render<ShadcnFieldTitle>(parameters => parameters.AddChildContent("title"));
        var label = Render<ShadcnFieldLabel>(parameters => parameters.Add(x => x.For, "control").AddChildContent("label"));
        var group = Render<ShadcnFieldGroup>(parameters => parameters.AddChildContent("group"));

        Assert.Equal("content", content.Find("[data-slot='field-content']").TextContent);
        Assert.Equal("title", title.Find("[data-slot='field-label']").TextContent);
        Assert.Equal("control", label.Find("label[data-slot='field-label']").GetAttribute("for"));
        Assert.Equal("group", group.Find("[data-slot='field-group']").TextContent);
    }

    [Fact]
    public void FieldSeparatorSupportsPlainAndLabelledForms()
    {
        var plain = Render<ShadcnFieldSeparator>();
        Assert.Equal("false", plain.Find("[data-slot='field-separator']").GetAttribute("data-content"));
        Assert.Empty(plain.FindAll("[data-slot='field-separator-content']"));

        var labelled = Render<ShadcnFieldSeparator>(parameters => parameters.AddChildContent("Or continue with"));
        Assert.Equal("true", labelled.Find("[data-slot='field-separator']").GetAttribute("data-content"));
        Assert.Equal("Or continue with", labelled.Find("[data-slot='field-separator-content']").TextContent);
    }

    [Fact]
    public void FieldRejectsUnknownEnumValues()
    {
        var orientation = Assert.ThrowsAny<Exception>(() => Render<ShadcnField>(parameters =>
            parameters.Add(x => x.Orientation, (ShadcnFieldOrientation)999)));
        var legend = Assert.ThrowsAny<Exception>(() => Render<ShadcnFieldLegend>(parameters =>
            parameters.Add(x => x.Variant, (ShadcnFieldLegendVariant)999)));

        Assert.Contains("orientation", orientation.ToString(), StringComparison.OrdinalIgnoreCase);
        Assert.Contains("variant", legend.ToString(), StringComparison.OrdinalIgnoreCase);
    }

    private sealed class CaptureFieldContext : ComponentBase
    {
        [CascadingParameter]
        public ShadcnFieldContext Context { get; set; }

        [Parameter]
        public Action<ShadcnFieldContext>? OnCaptured { get; set; }

        protected override void OnParametersSet() => OnCaptured?.Invoke(Context);
    }

    private static string FindRoot()
    {
        var directory = new DirectoryInfo(AppContext.BaseDirectory);
        while (directory is not null && !File.Exists(Path.Combine(directory.FullName, "Maliev.ShadcnBlazor.slnx")))
            directory = directory.Parent;
        return directory?.FullName ?? throw new DirectoryNotFoundException("Could not locate the repository root.");
    }
}
