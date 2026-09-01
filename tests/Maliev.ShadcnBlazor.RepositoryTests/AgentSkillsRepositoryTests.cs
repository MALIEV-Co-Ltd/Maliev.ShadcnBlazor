using System.Text.RegularExpressions;

namespace Maliev.ShadcnBlazor.RepositoryTests;

public sealed partial class AgentSkillsRepositoryTests
{
    private static readonly string[] SkillNames =
    [
        "maliev-shadcnblazor",
        "maliev-shadcnblazor-maintainer",
    ];

    [Fact]
    public void SkillPackagesAreCompleteAndPortable()
    {
        var root = RepositoryRoot.Find();

        foreach (var skillName in SkillNames)
        {
            var skillRoot = Path.Combine(root, ".agents", "skills", skillName);
            var instructionsPath = Path.Combine(skillRoot, "SKILL.md");
            var metadataPath = Path.Combine(skillRoot, "agents", "openai.yaml");
            var referencesPath = Path.Combine(skillRoot, "references");

            Assert.True(File.Exists(instructionsPath), $"Missing instructions for {skillName}.");
            Assert.True(File.Exists(metadataPath), $"Missing agent metadata for {skillName}.");
            Assert.True(Directory.Exists(referencesPath), $"Missing references for {skillName}.");
            Assert.NotEmpty(Directory.EnumerateFiles(referencesPath, "*.md", SearchOption.TopDirectoryOnly));

            var instructions = File.ReadAllText(instructionsPath);
            Assert.StartsWith("---", instructions, StringComparison.Ordinal);
            Assert.Contains($"name: {skillName}", instructions, StringComparison.Ordinal);
            Assert.Contains("description:", instructions, StringComparison.Ordinal);
            Assert.DoesNotContain("TODO", instructions, StringComparison.OrdinalIgnoreCase);

            var metadata = File.ReadAllText(metadataPath);
            Assert.Contains("display_name:", metadata, StringComparison.Ordinal);
            Assert.Contains("$" + skillName, metadata, StringComparison.Ordinal);
            Assert.Contains("allow_implicit_invocation: true", metadata, StringComparison.Ordinal);

            foreach (var file in Directory.EnumerateFiles(skillRoot, "*", SearchOption.AllDirectories))
            {
                var content = File.ReadAllText(file);
                Assert.DoesNotMatch(WindowsAbsolutePath(), content);
            }
        }
    }

    [Fact]
    public void PublicDocumentationRoutesConsumerAndMaintainerWorkflows()
    {
        var root = RepositoryRoot.Find();
        var readme = File.ReadAllText(Path.Combine(root, "README.md"));
        var guide = File.ReadAllText(Path.Combine(root, "AGENTS.md"));
        var skillGuide = File.ReadAllText(Path.Combine(root, "docs", "agent-skills.md"));

        foreach (var skillName in SkillNames)
        {
            Assert.Contains(skillName, readme, StringComparison.Ordinal);
            Assert.Contains(skillName, guide, StringComparison.Ordinal);
            Assert.Contains(skillName, skillGuide, StringComparison.Ordinal);
        }

        Assert.Contains("npx skills add MALIEV-Co-Ltd/Maliev.ShadcnBlazor", readme, StringComparison.Ordinal);
        Assert.Contains("A skill is reusable guidance, not authority", guide, StringComparison.Ordinal);
    }

    [Fact]
    public void AgenticIntegrationGuidanceStaysAlignedWithTheAdvertisedWorkflow()
    {
        var root = RepositoryRoot.Find();
        var guide = File.ReadAllText(Path.Combine(root, "AGENTS.md"));
        var publicGuide = File.ReadAllText(Path.Combine(root, "docs", "agent-skills.md"));
        var consumerSkillRoot = Path.Combine(root, ".agents", "skills", "maliev-shadcnblazor");
        var consumerSkill = File.ReadAllText(Path.Combine(consumerSkillRoot, "SKILL.md"));
        var integrationReferencePath = Path.Combine(consumerSkillRoot, "references", "agentic-integration.md");
        var maintainerSkill = File.ReadAllText(Path.Combine(
            root,
            ".agents",
            "skills",
            "maliev-shadcnblazor-maintainer",
            "SKILL.md"));

        Assert.Contains("Agent guidance synchronization", guide, StringComparison.Ordinal);
        Assert.Contains("references/agentic-integration.md", consumerSkill, StringComparison.Ordinal);
        Assert.True(File.Exists(integrationReferencePath), "The consumer skill is missing its agentic-integration reference.");

        var integrationReference = File.ReadAllText(integrationReferencePath);
        foreach (var required in new[]
        {
            "AGENTS.md",
            "ShadcnSidebar",
            "ShadcnChart",
            "ShadcnChartTooltipContent",
            "ShadcnChartLegendContent",
            "ShadcnInput",
            "installed package version",
        })
        {
            Assert.Contains(required, integrationReference, StringComparison.OrdinalIgnoreCase);
        }

        Assert.Contains("agent guidance", maintainerSkill, StringComparison.OrdinalIgnoreCase);
        Assert.Contains("docs/agent-skills.md", maintainerSkill, StringComparison.Ordinal);
        Assert.Contains("Application AGENTS.md", publicGuide, StringComparison.Ordinal);
    }

    [GeneratedRegex(@"(?im)\b[A-Z]:\\")]
    private static partial Regex WindowsAbsolutePath();
}
