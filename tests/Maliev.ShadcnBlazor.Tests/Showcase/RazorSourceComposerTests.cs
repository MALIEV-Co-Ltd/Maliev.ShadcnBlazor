using Maliev.ShadcnBlazor.Showcase.Documentation.Examples;

namespace Maliev.ShadcnBlazor.Tests.Showcase;

public sealed class RazorSourceComposerTests
{
    [Fact]
    public void ComposeDeduplicatesOnlyLeadingUsingDirectivesAndPreservesOrderAndBody()
    {
        const string example = """
            @using Maliev.ShadcnBlazor.Components.Conversation
            @using Maliev.ShadcnBlazor.Components.Content
            @using Maliev.ShadcnBlazor.Components.Conversation

            <ShadcnMessage>@using remains body text</ShadcnMessage>
            """;

        var source = RazorSourceComposer.Compose("Maliev.ShadcnBlazor.Components.Conversation", example);

        Assert.Equal(1, source.Split("@using Maliev.ShadcnBlazor.Components.Conversation", StringSplitOptions.None).Length - 1);
        Assert.StartsWith("@using Maliev.ShadcnBlazor.Components.Conversation\n@using Maliev.ShadcnBlazor.Components.Content\n\n", source, StringComparison.Ordinal);
        Assert.EndsWith("<ShadcnMessage>@using remains body text</ShadcnMessage>", source, StringComparison.Ordinal);
    }
}
