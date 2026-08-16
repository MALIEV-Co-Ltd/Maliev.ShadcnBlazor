using Bunit;
using Maliev.ShadcnBlazor.Showcase.Documentation;
using Maliev.ShadcnBlazor.Showcase.Documentation.Examples;
using Maliev.ShadcnBlazor.Showcase.Pages;
using Microsoft.AspNetCore.Components;
using Microsoft.Extensions.DependencyInjection;

namespace Maliev.ShadcnBlazor.Tests.Showcase;

public sealed class FeedbackContentShowcaseContractTests : BunitContext
{
    private static readonly string[] Slugs = ["alert", "avatar", "badge", "card", "carousel", "progress", "skeleton", "spinner", "toast"];

    public FeedbackContentShowcaseContractTests()
    {
        JSInterop.Mode = JSRuntimeMode.Loose;
        Services.AddMalievShadcn();
    }

    [Fact]
    public void IndependentlyAcceptedPlanFiveComponentsHaveOneCertifiedInteractiveDossier()
    {
        var catalog = new ComponentDocumentationCatalog();
        var registry = new ComponentExampleRegistry(catalog);
        foreach (var slug in Slugs)
        {
            var entry = Assert.IsType<ComponentDocumentationEntry>(catalog.FindBySlug(slug));
            Assert.Equal(ComponentDocumentationStatus.Complete, entry.Status);
            Assert.True(entry.Evidence.Api && entry.Evidence.ComponentTests && entry.Evidence.Accessibility && entry.Evidence.Interaction && entry.Evidence.ComputedStyle && entry.Evidence.Visual && entry.Evidence.Integration);
            var example = Assert.Single(registry.GetBySlug(slug));
            Assert.Equal($"{slug}-primary", example.Id);
            Assert.NotEmpty(example.Controls);
            Assert.NotEmpty(example.StateTags);
            Assert.NotEmpty(Render(example.Preview).FindAll("[data-slot]"));
        }
        Assert.Equal(64, catalog.All.Count(entry => entry.Status == ComponentDocumentationStatus.Complete));
        Assert.DoesNotContain(catalog.All, entry => entry.Status == ComponentDocumentationStatus.Planned);
    }

    [Fact]
    public void FamilyPageRendersAllComponentsAndUsageGuidance()
    {
        var cut = Render<FeedbackAndContent>();
        Assert.Equal(9, cut.FindAll("[data-component]").Count);
        Assert.Contains("dotnet add package Maliev.ShadcnBlazor", cut.Markup);
        Assert.Contains("TimeProvider", cut.Markup);
        Assert.Contains("Blazor-native engine", cut.Markup);
        Assert.NotNull(cut.Find("[data-testid='feedback-content-fixture']"));
        Assert.Equal(Slugs, cut.FindAll("[data-pair-id]").Take(9).Select(element => element.GetAttribute("data-pair-id")));
        Assert.Equal(17, cut.FindAll("[data-pair-id]").Count);
    }

    [Fact]
    public void EveryDossierOwnsItsCompleteCompositionApiSurface()
    {
        var catalog = new ComponentDocumentationCatalog();
        var api = new Maliev.ShadcnBlazor.Showcase.Documentation.Api.ComponentApiCatalog();
        var expectedMinimum = new Dictionary<string, int>
        {
            ["alert"] = 5,
            ["avatar"] = 6,
            ["badge"] = 1,
            ["card"] = 7,
            ["carousel"] = 7,
            ["progress"] = 5,
            ["skeleton"] = 1,
            ["spinner"] = 1,
            ["toast"] = 5
        };

        foreach (var (slug, minimum) in expectedMinimum)
        {
            var entry = Assert.IsType<ComponentDocumentationEntry>(catalog.FindBySlug(slug));
            Assert.True(api.GetByEntry(entry).Count >= minimum, $"{slug} exposes an incomplete API dossier.");
        }
    }

    [Fact]
    public void ToastDossierIsLiveAndExposesQueueTypePlacementAndMotionControls()
    {
        var example = Assert.Single(new ComponentExampleRegistry(new ComponentDocumentationCatalog()).GetBySlug("toast"));
        Assert.Contains(example.Controls, control => control.Id == "toast-limit");
        Assert.Contains(example.Controls, control => control.Id == "toast-start");
        Assert.Contains(example.Controls, control => control.Id == "toast-reduced");
        Assert.Contains(example.Controls, control => control.Id == "toast-type");
        var cut = Render(example.Preview);
        cut.Find("button").Click();
        Assert.Contains("บันทึกแล้ว", cut.Markup, StringComparison.Ordinal);
    }

    [Fact]
    public void AlertDossierUsesTheComposedShadcnCalloutPattern()
    {
        var example = Assert.Single(new ComponentExampleRegistry(new ComponentDocumentationCatalog()).GetBySlug("alert"));
        var cut = Render(example.Preview);

        Assert.Single(cut.FindAll("[data-slot='alert']"));
        Assert.Single(cut.FindAll("[data-slot='alert-icon']"));
        Assert.Single(cut.FindAll("[data-slot='alert-title']"));
        Assert.Single(cut.FindAll("[data-slot='alert-description']"));
        Assert.Single(cut.FindAll("[data-slot='alert-action'] button"));
        Assert.Contains("Payment processed", cut.Markup, StringComparison.Ordinal);
    }

    [Fact]
    public void EveryFeedbackDossierExposesControlsForItsDocumentedCustomizationStates()
    {
        var registry = new ComponentExampleRegistry(new ComponentDocumentationCatalog());
        var expected = new Dictionary<string, string[]>(StringComparer.Ordinal)
        {
            ["alert"] = ["alert-variant", "alert-role", "alert-action"],
            ["avatar"] = ["avatar-size", "avatar-failed", "avatar-badge", "avatar-group"],
            ["badge"] = ["badge-variant", "badge-link", "badge-invalid"],
            ["card"] = ["card-size", "card-spacing", "card-action"],
            ["carousel"] = ["carousel-vertical", "carousel-loop", "carousel-rtl", "carousel-reduced"],
            ["progress"] = ["progress-indeterminate", "progress-value", "progress-show-value"],
            ["skeleton"] = ["skeleton-circle", "skeleton-motion"],
            ["spinner"] = ["spinner-decorative", "spinner-large"],
            ["toast"] = ["toast-limit", "toast-start", "toast-reduced", "toast-type", "toast-priority"]
        };

        foreach (var (slug, controlIds) in expected)
            Assert.Equal(controlIds, registry.GetBySlug(slug).Single().Controls.Select(control => control.Id));
    }

    [Fact]
    public void AvatarAndCardDossierControlsRenderValidCompositionSlots()
    {
        var registry = new ComponentExampleRegistry(new ComponentDocumentationCatalog());
        var avatar = registry.GetBySlug("avatar").Single();
        avatar.Controls.Single(control => control.Id == "avatar-badge").Apply("true");
        avatar.Controls.Single(control => control.Id == "avatar-group").Apply("true");
        var avatarCut = Render(avatar.Preview);
        Assert.Equal(4, avatarCut.FindAll("[data-testid='avatar-gallery'] > [data-testid='avatar-profile']").Count);
        Assert.NotNull(avatarCut.Find("[data-testid='avatar-gallery'] [data-slot='avatar-fallback'] svg"));
        Assert.Equal(3, avatarCut.FindAll("[data-testid='avatar-group-preview'] [data-slot='avatar']").Count);
        Assert.NotNull(avatarCut.Find("[data-slot='avatar-group'] > [data-slot='avatar'] > [data-slot='avatar-badge']"));
        Assert.NotNull(avatarCut.Find("[data-slot='avatar-group'] > [data-slot='avatar-group-count']"));

        var card = registry.GetBySlug("card").Single();
        card.Controls.Single(control => control.Id == "card-action").Apply("true");
        var cardCut = Render(card.Preview);
        Assert.NotNull(cardCut.Find("[data-slot='card-header'] > [data-slot='card-action']"));
        card.Controls.Single(control => control.Id == "card-action").Apply("false");
        var cardWithoutAction = Render(card.Preview);
        Assert.Empty(cardWithoutAction.FindAll("[data-slot='card-action']"));
    }

    [Fact]
    public void EveryFeedbackDossierControlChangesTheRenderedComponentCanvas()
    {
        var alternate = new Dictionary<string, string>(StringComparer.Ordinal)
        {
            ["alert-variant"] = "Destructive",
            ["alert-role"] = "Status",
            ["alert-action"] = "false",
            ["avatar-size"] = "Large",
            ["avatar-failed"] = "true",
            ["avatar-badge"] = "true",
            ["avatar-group"] = "true",
            ["badge-variant"] = "Outline",
            ["badge-link"] = "true",
            ["badge-invalid"] = "true",
            ["card-size"] = "Small",
            ["card-spacing"] = "true",
            ["card-action"] = "false",
            ["carousel-vertical"] = "true",
            ["carousel-loop"] = "true",
            ["carousel-rtl"] = "true",
            ["carousel-reduced"] = "false",
            ["progress-indeterminate"] = "true",
            ["progress-value"] = "33",
            ["progress-show-value"] = "false",
            ["skeleton-circle"] = "true",
            ["skeleton-motion"] = "false",
            ["spinner-decorative"] = "true",
            ["spinner-large"] = "true",
            ["toast-limit"] = "1",
            ["toast-start"] = "true",
            ["toast-reduced"] = "true",
            ["toast-type"] = "Error",
            ["toast-priority"] = "High"
        };

        foreach (var slug in Slugs.Where(slug => slug != "toast"))
        {
            var controlIds = new ComponentExampleRegistry(new ComponentDocumentationCatalog()).GetBySlug(slug).Single().Controls.Select(control => control.Id).ToArray();
            foreach (var controlId in controlIds)
            {
                var example = new ComponentExampleRegistry(new ComponentDocumentationCatalog()).GetBySlug(slug).Single();
                var control = example.Controls.Single(candidate => candidate.Id == controlId);
                var before = Render(example.Preview).Markup;
                control.Apply(alternate[control.Id]);
                var after = Render(example.Preview);
                Assert.NotEqual(before, after.Markup);
                AssertAppliedControl(control.Id, after);
            }
        }

        var toastControlIds = new ComponentExampleRegistry(new ComponentDocumentationCatalog()).GetBySlug("toast").Single().Controls.Select(control => control.Id).ToArray();
        foreach (var controlId in toastControlIds)
        {
            var toastExample = new ComponentExampleRegistry(new ComponentDocumentationCatalog()).GetBySlug("toast").Single();
            var control = toastExample.Controls.Single(candidate => candidate.Id == controlId);
            var before = Render(toastExample.Preview); before.Find("button").Click(); var beforeToast = before.Markup;
            control.Apply(alternate[control.Id]);
            var after = Render(toastExample.Preview); after.Find("button").Click();
            Assert.NotEqual(beforeToast, after.Markup);
            AssertAppliedControl(control.Id, after);
        }
    }

    private static void AssertAppliedControl(string controlId, IRenderedComponent<IComponent> cut)
    {
        switch (controlId)
        {
            case "alert-variant": Assert.Equal("destructive", cut.Find("[data-slot='alert']").GetAttribute("data-variant")); break;
            case "alert-role": Assert.Equal("status", cut.Find("[data-slot='alert']").GetAttribute("role")); break;
            case "alert-action": Assert.Empty(cut.FindAll("[data-slot='alert-action']")); break;
            case "avatar-size": Assert.Equal("lg", cut.Find("[data-slot='avatar']").GetAttribute("data-size")); break;
            case "avatar-failed": Assert.Equal("images/avatars/missing-avatar.webp", cut.Find("[data-slot='avatar-image']").GetAttribute("src")); break;
            case "avatar-badge": Assert.Single(cut.FindAll("[data-slot='avatar-badge']")); break;
            case "avatar-group": Assert.Single(cut.FindAll("[data-slot='avatar-group']")); break;
            case "badge-variant": Assert.Equal("outline", cut.Find("[data-slot='badge']").GetAttribute("data-variant")); break;
            case "badge-link": Assert.Equal("a", cut.Find("[data-slot='badge']").NodeName.ToLowerInvariant()); break;
            case "badge-invalid": Assert.Equal("true", cut.Find("[data-slot='badge']").GetAttribute("aria-invalid")); break;
            case "card-size": Assert.Equal("sm", cut.Find("[data-slot='card']").GetAttribute("data-size")); break;
            case "card-spacing": Assert.Contains("--shadcn-card-spacing: 0.75rem", cut.Find("[data-slot='card']").GetAttribute("style"), StringComparison.Ordinal); break;
            case "card-action": Assert.Empty(cut.FindAll("[data-slot='card-action']")); break;
            case "carousel-vertical": Assert.Equal("vertical", cut.Find("[data-slot='carousel']").GetAttribute("data-orientation")); break;
            case "carousel-loop": Assert.Equal("true", cut.Find("[data-slot='carousel']").GetAttribute("data-loop")); break;
            case "carousel-rtl": Assert.Equal("true", cut.Find("[data-slot='carousel']").GetAttribute("data-rtl")); break;
            case "carousel-reduced": Assert.Null(cut.Find("[data-slot='carousel']").GetAttribute("data-reduced-motion")); break;
            case "progress-indeterminate": Assert.Equal("indeterminate", cut.Find("[data-slot='progress']").GetAttribute("data-state")); break;
            case "progress-value": Assert.Equal("33", cut.Find("[data-slot='progress']").GetAttribute("aria-valuenow")); break;
            case "progress-show-value": Assert.Empty(cut.FindAll("[data-slot='progress-value']")); break;
            case "skeleton-circle": Assert.Equal("circle", cut.Find("[data-slot='skeleton']").GetAttribute("data-shape")); break;
            case "skeleton-motion": Assert.Equal("none", cut.Find("[data-slot='skeleton']").GetAttribute("data-animation")); break;
            case "spinner-decorative": Assert.Equal("true", cut.Find("[data-slot='spinner']").GetAttribute("aria-hidden")); break;
            case "spinner-large": Assert.Contains("--shadcn-spinner-size: 1.5rem", cut.Find("[data-slot='spinner']").GetAttribute("style"), StringComparison.Ordinal); break;
            case "toast-limit": Assert.Equal("1", cut.Find("[data-slot='toast-viewport']").GetAttribute("data-maximum-visible")); break;
            case "toast-start": Assert.Equal("bottom-start", cut.Find("[data-slot='toast-viewport']").GetAttribute("data-placement")); break;
            case "toast-reduced": Assert.Equal("true", cut.Find("[data-slot='toast-viewport']").GetAttribute("data-reduced-motion")); break;
            case "toast-type": Assert.NotEmpty(cut.FindAll("[data-slot='toast'][data-type='error']")); break;
            case "toast-priority": Assert.NotEmpty(cut.FindAll("[data-slot='toast'][data-priority='high']")); break;
            default: throw new InvalidOperationException($"No exact dossier assertion exists for {controlId}.");
        }
    }

    private static string FindRoot()
    {
        var directory = new DirectoryInfo(AppContext.BaseDirectory);
        while (directory is not null && !File.Exists(Path.Combine(directory.FullName, "Maliev.ShadcnBlazor.slnx")))
        {
            directory = directory.Parent;
        }

        return directory?.FullName ?? throw new DirectoryNotFoundException();
    }
}
