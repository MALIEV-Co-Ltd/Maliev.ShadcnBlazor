using Maliev.ShadcnBlazor.Components.Primitives;

namespace Maliev.ShadcnBlazor.Tests.Components.SemanticFoundations;

public sealed class ShadcnComponentBaseTests
{
    [Fact]
    public void MergeClassCombinesFrameworkExplicitAndUnmatchedClassesWithoutDuplicates()
    {
        var component = new TestComponent(
            @class: "consumer compact",
            attributes: new Dictionary<string, object>
            {
                ["CLASS"] = "compact external"
            });

        Assert.Equal("framework compact consumer external", component.Classes("framework compact"));
    }

    [Fact]
    public void MergeStyleCombinesFrameworkExplicitAndUnmatchedStyles()
    {
        var component = new TestComponent(
            style: "color: red;",
            attributes: new Dictionary<string, object>
            {
                ["style"] = "inline-size: 10rem;"
            });

        Assert.Equal(
            "display: flex; color: red; inline-size: 10rem",
            component.Styles("display: flex;"));
    }

    [Fact]
    public void AttributesExceptFiltersClassStyleAndProtectedNamesCaseInsensitively()
    {
        var component = new TestComponent(
            attributes: new Dictionary<string, object>
            {
                ["class"] = "external",
                ["STYLE"] = "color: red",
                ["ROLE"] = "presentation",
                ["data-slot"] = "wrong",
                ["id"] = "customer-name",
                ["aria-describedby"] = "customer-help",
                ["data-test-id"] = "customer-field"
            });

        var attributes = component.Filtered("role", "data-slot");

        Assert.Equal(3, attributes.Count);
        Assert.Equal("customer-name", attributes["id"]);
        Assert.Equal("customer-help", attributes["aria-describedby"]);
        Assert.Equal("customer-field", attributes["data-test-id"]);
    }

    [Fact]
    public void HelpersAcceptNullValues()
    {
        var component = new TestComponent();

        Assert.Equal("framework", component.Classes("framework"));
        Assert.Equal("display: block", component.Styles("display: block"));
        Assert.Empty(component.Filtered("role"));
    }

    private sealed class TestComponent : ShadcnComponentBase
    {
        public TestComponent(
            string? @class = null,
            string? style = null,
            IReadOnlyDictionary<string, object>? attributes = null)
        {
            Class = @class;
            Style = style;
            AdditionalAttributes = attributes;
        }

        public string Classes(string frameworkClass) => MergeClass(frameworkClass);

        public string? Styles(string? frameworkStyle) => MergeStyle(frameworkStyle);

        public IReadOnlyDictionary<string, object> Filtered(params string[] protectedNames) =>
            AttributesExcept(protectedNames);
    }
}
