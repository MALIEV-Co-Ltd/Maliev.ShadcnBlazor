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
    public void SpinnerDossierUsesThePackageStateTransitionForStableStateChanges()
    {
        var source = File.ReadAllText(Path.Combine(FindRoot(), "samples", "Maliev.ShadcnBlazor.Showcase", "Components", "Documentation", "SpinnerDossierPreview.razor"));

        Assert.Contains("<ShadcnStateTransition", source, StringComparison.Ordinal);
        Assert.Contains("Active=\"_processing\"", source, StringComparison.Ordinal);
        Assert.Contains("ReducedMotion=\"ReducedMotion\"", source, StringComparison.Ordinal);
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
        Assert.Equal(66, catalog.All.Count(entry => entry.Status == ComponentDocumentationStatus.Complete));
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
        Assert.Contains("บันทึกใบงานแล้ว", cut.Markup, StringComparison.Ordinal);
        cut.Find("[data-slot='toast-action']").Click();
        cut.WaitForAssertion(() => Assert.Contains("ยกเลิกการบันทึกแล้ว", cut.Markup, StringComparison.Ordinal));
    }

    [Fact]
    public void FeedbackSourcesDescribeTheRenderedCompositionsInsteadOfPlaceholders()
    {
        var registry = new ComponentExampleRegistry(new ComponentDocumentationCatalog());

        var card = registry.GetBySlug("card").Single().RazorSource;
        Assert.Contains("Laser cell 04", card, StringComparison.Ordinal);
        Assert.Contains("ShadcnCardDescription", card, StringComparison.Ordinal);
        Assert.Contains("ShadcnCardFooter", card, StringComparison.Ordinal);
        Assert.Contains("private void ToggleProduction()", card, StringComparison.Ordinal);
        Assert.Contains("OnClick=\"ToggleProduction\"", card, StringComparison.Ordinal);
        Assert.DoesNotContain("...</", card, StringComparison.Ordinal);

        var progress = registry.GetBySlug("progress").Single().RazorSource;
        Assert.Contains("Indeterminate ? null : Value", progress, StringComparison.Ordinal);
        Assert.Contains("Label=\"@(Indeterminate ? \"Preparing upload\" : \"Upload progress\")\"", progress, StringComparison.Ordinal);
        Assert.Contains("class=\"showcase-progress-demo\"", progress, StringComparison.Ordinal);
        Assert.Contains("private string UploadDetail", progress, StringComparison.Ordinal);

        var toast = registry.GetBySlug("toast").Single().RazorSource;
        Assert.Contains("private void Show()", toast, StringComparison.Ordinal);
        Assert.Contains("MaximumVisible=\"3\"", toast, StringComparison.Ordinal);
        Assert.Contains("ActionLabel: \"เลิกทำ\"", toast, StringComparison.Ordinal);
        Assert.Contains("Description: \"ใบงาน WO-2048 พร้อมส่งให้ฝ่ายผลิต\"", toast, StringComparison.Ordinal);
        Assert.Contains("private Task UndoAsync()", toast, StringComparison.Ordinal);
        Assert.Contains("ยกเลิกการบันทึกแล้ว — Save undone", toast, StringComparison.Ordinal);
    }

    [Fact]
    public void ToastDossierCentersItsRealTriggerWithoutRelocatingThePageLevelViewport()
    {
        var root = FindRoot();
        var css = File.ReadAllText(Path.Combine(root, "samples", "Maliev.ShadcnBlazor.Showcase", "wwwroot", "css", "showcase.css"));

        Assert.Contains(".showcase-toast-demo {", css, StringComparison.Ordinal);
        Assert.Contains("place-items: center", css, StringComparison.Ordinal);
        Assert.Contains(".showcase-toast-demo .shadcn-toast-viewport", css, StringComparison.Ordinal);
    }

    [Fact]
    public void FeedbackSourcesFollowTheCurrentPreviewControls()
    {
        var registry = new ComponentExampleRegistry(new ComponentDocumentationCatalog());

        var card = registry.GetBySlug("card").Single();
        card.Controls.Single(control => control.Id == "card-size").Apply("Small");
        card.Controls.Single(control => control.Id == "card-spacing").Apply("true");
        card.Controls.Single(control => control.Id == "card-action").Apply("false");
        Assert.Contains("Size=\"ShadcnCardSize.Small\"", card.RazorSource, StringComparison.Ordinal);
        Assert.Contains("Spacing=\"0.75rem\"", card.RazorSource, StringComparison.Ordinal);
        Assert.DoesNotContain("ShadcnCardAction", card.RazorSource, StringComparison.Ordinal);
        Assert.DoesNotContain("OnClick=\"ToggleProduction\"", card.RazorSource, StringComparison.Ordinal);

        var progress = registry.GetBySlug("progress").Single();
        progress.Controls.Single(control => control.Id == "progress-indeterminate").Apply("true");
        progress.Controls.Single(control => control.Id == "progress-value").Apply("32");
        progress.Controls.Single(control => control.Id == "progress-show-value").Apply("false");
        Assert.Contains("private bool Indeterminate = true", progress.RazorSource, StringComparison.Ordinal);
        Assert.Contains("private double Value = 32", progress.RazorSource, StringComparison.Ordinal);
        Assert.Contains("private bool ShowValue = false", progress.RazorSource, StringComparison.Ordinal);
        Assert.Contains("Value=\"@(Indeterminate ? null : Value)\"", progress.RazorSource, StringComparison.Ordinal);
        Assert.Contains("Label=\"@(Indeterminate ? \"Preparing upload\" : \"Upload progress\")\"", progress.RazorSource, StringComparison.Ordinal);
        Assert.Contains("ShowValue=\"@ShowValue\"", progress.RazorSource, StringComparison.Ordinal);

        var carousel = registry.GetBySlug("carousel").Single();
        carousel.Controls.Single(control => control.Id == "carousel-vertical").Apply("true");
        carousel.Controls.Single(control => control.Id == "carousel-loop").Apply("true");
        carousel.Controls.Single(control => control.Id == "carousel-rtl").Apply("true");
        carousel.Controls.Single(control => control.Id == "carousel-reduced").Apply("true");
        Assert.Contains("Orientation=\"ShadcnCarouselOrientation.Vertical\"", carousel.RazorSource, StringComparison.Ordinal);
        Assert.Contains("Loop = true", carousel.RazorSource, StringComparison.Ordinal);
        Assert.Contains("RightToLeft = true", carousel.RazorSource, StringComparison.Ordinal);
        Assert.Contains("ReducedMotion = true", carousel.RazorSource, StringComparison.Ordinal);
        Assert.Contains("ViewportBlockSize=\"256\"", carousel.RazorSource, StringComparison.Ordinal);
        Assert.Contains("--shadcn-carousel-viewport-block-size: 256px", Render(carousel.Preview).Find("[data-slot='carousel-content']").GetAttribute("style"), StringComparison.Ordinal);
        Assert.Contains("GoToAsync(slide.Index)", carousel.RazorSource, StringComparison.Ordinal);

        var toast = registry.GetBySlug("toast").Single();
        toast.Controls.Single(control => control.Id == "toast-limit").Apply("1");
        toast.Controls.Single(control => control.Id == "toast-start").Apply("true");
        toast.Controls.Single(control => control.Id == "toast-type").Apply("Error");
        Assert.Contains("MaximumVisible=\"1\"", toast.RazorSource, StringComparison.Ordinal);
        Assert.Contains("Placement=\"ShadcnToastPlacement.BottomStart\"", toast.RazorSource, StringComparison.Ordinal);
        Assert.Contains("Type: ShadcnToastType.Error", toast.RazorSource, StringComparison.Ordinal);
    }

    [Fact]
    public void CardPreviewUsesARealInteractiveProductionOrderComposition()
    {
        var example = Assert.Single(new ComponentExampleRegistry(new ComponentDocumentationCatalog()).GetBySlug("card"));
        var cut = Render(example.Preview);

        var preview = cut.Find("[data-testid='card-dossier-preview']");
        Assert.Contains("Production order #MO-2418", preview.TextContent, StringComparison.Ordinal);
        Assert.Contains("12 of 18 parts complete", preview.TextContent, StringComparison.Ordinal);
        Assert.Equal("In progress", cut.Find("[data-testid='card-production-status']").TextContent);

        cut.Find("[data-testid='card-toggle-production']").Click();

        Assert.Equal("Paused", cut.Find("[data-testid='card-production-status']").TextContent);
        Assert.Equal("Resume production", cut.Find("[data-testid='card-toggle-production']").TextContent);
    }

    [Fact]
    public void SkeletonDossierShowsACompleteInteractiveLoadingStateAndTracksItsSource()
    {
        var skeleton = new ComponentExampleRegistry(new ComponentDocumentationCatalog()).GetBySlug("skeleton").Single();
        var loading = Render(skeleton.Preview);

        Assert.Equal("true", loading.Find("[data-testid='skeleton-dossier-preview']").GetAttribute("aria-busy"));
        Assert.Equal(12, loading.FindAll("[data-testid='skeleton-loading-list'] [data-slot='skeleton']").Count);
        Assert.Equal("Show loaded queue", loading.Find("[data-testid='skeleton-state-toggle']").TextContent.Trim());

        loading.Find("[data-testid='skeleton-state-toggle']").Click();
        Assert.Equal("false", loading.Find("[data-testid='skeleton-dossier-preview']").GetAttribute("aria-busy"));
        Assert.Empty(loading.FindAll("[data-testid='skeleton-loading-list']"));
        Assert.Equal(3, loading.FindAll("[data-testid='skeleton-loaded-list'] > li").Count);
        Assert.Contains("WO-2486", loading.Markup, StringComparison.Ordinal);
        Assert.Equal("Reset loading preview", loading.Find("[data-testid='skeleton-state-toggle']").TextContent.Trim());

        skeleton.Controls.Single(control => control.Id == "skeleton-circle").Apply("true");
        skeleton.Controls.Single(control => control.Id == "skeleton-motion").Apply("false");
        var configured = Render(skeleton.Preview);
        Assert.All(configured.FindAll("[data-testid='skeleton-loading-list'] [data-testid='skeleton-media']"), media =>
        {
            Assert.Equal("circle", media.GetAttribute("data-shape"));
            Assert.Equal("none", media.GetAttribute("data-animation"));
        });
        Assert.All(configured.FindAll("[data-testid='skeleton-loading-list'] [data-slot='skeleton']"), placeholder =>
            Assert.Equal("none", placeholder.GetAttribute("data-animation")));
        Assert.Contains("private bool RoundMedia = true;", skeleton.RazorSource, StringComparison.Ordinal);
        Assert.Contains("private ShadcnSkeletonAnimation Motion = ShadcnSkeletonAnimation.None;", skeleton.RazorSource, StringComparison.Ordinal);
        Assert.Contains("private bool Loading = true;", skeleton.RazorSource, StringComparison.Ordinal);
        Assert.Contains("Show loaded queue", skeleton.RazorSource, StringComparison.Ordinal);
        Assert.Contains("WO-2486", skeleton.RazorSource, StringComparison.Ordinal);
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
            ["spinner"] = ["spinner-decorative", "spinner-large", "spinner-reduced-motion"],
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
        var presence = avatarCut.Find("[data-slot='avatar-group'] > [data-slot='avatar'] > [data-slot='avatar-badge']");
        Assert.Equal("Online", presence.GetAttribute("aria-label"));
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
    public void AvatarDossierSourceTracksSizeFailurePresenceAndGroupControls()
    {
        var avatar = new ComponentExampleRegistry(new ComponentDocumentationCatalog()).GetBySlug("avatar").Single();

        avatar.Controls.Single(control => control.Id == "avatar-size").Apply("Large");
        avatar.Controls.Single(control => control.Id == "avatar-failed").Apply("true");
        avatar.Controls.Single(control => control.Id == "avatar-badge").Apply("false");
        avatar.Controls.Single(control => control.Id == "avatar-group").Apply("false");

        Assert.Contains("Size=\"ShadcnAvatarSize.Large\"", avatar.RazorSource, StringComparison.Ordinal);
        Assert.Contains("Source=\"images/avatars/missing-avatar.png\"", avatar.RazorSource, StringComparison.Ordinal);
        Assert.DoesNotContain("<ShadcnAvatarBadge", avatar.RazorSource, StringComparison.Ordinal);
        Assert.DoesNotContain("<ShadcnAvatarGroup", avatar.RazorSource, StringComparison.Ordinal);

        avatar.Controls.Single(control => control.Id == "avatar-badge").Apply("true");
        avatar.Controls.Single(control => control.Id == "avatar-group").Apply("true");
        Assert.Contains("<ShadcnAvatarBadge aria-label=\"Online\" />", avatar.RazorSource, StringComparison.Ordinal);
        Assert.Contains("<ShadcnAvatarGroup Size=\"ShadcnAvatarSize.Large\"", avatar.RazorSource, StringComparison.Ordinal);
    }

    [Fact]
    public void BadgeDossierShowsEveryPinnedVariantAndKeepsSourceInSyncWithControls()
    {
        var example = new ComponentExampleRegistry(new ComponentDocumentationCatalog()).GetBySlug("badge").Single();
        var cut = Render(example.Preview);

        Assert.NotNull(cut.Find("[data-testid='badge-dossier-preview']"));
        Assert.Equal(
            ["default", "secondary", "destructive", "outline", "ghost", "link"],
            cut.FindAll("[data-testid='badge-variant-gallery'] [data-slot='badge']")
                .Select(element => element.GetAttribute("data-variant")));
        Assert.Contains("<ShadcnBadge Variant=\"ShadcnBadgeVariant.Default\">", example.RazorSource, StringComparison.Ordinal);
        Assert.DoesNotContain("aria-invalid", example.RazorSource, StringComparison.Ordinal);

        example.Controls.Single(control => control.Id == "badge-variant").Apply("Outline");
        example.Controls.Single(control => control.Id == "badge-link").Apply("true");
        example.Controls.Single(control => control.Id == "badge-invalid").Apply("true");
        cut = Render(example.Preview);

        var selected = cut.Find("[data-testid='badge-current'] [data-slot='badge']");
        Assert.Equal("outline", selected.GetAttribute("data-variant"));
        Assert.Equal("a", selected.NodeName.ToLowerInvariant());
        Assert.Equal("true", selected.GetAttribute("aria-invalid"));
        Assert.Contains("<ShadcnBadge Variant=\"ShadcnBadgeVariant.Outline\" Href=\"docs/components/badge\" aria-invalid=\"true\">", example.RazorSource, StringComparison.Ordinal);
        Assert.Contains("aria-invalid=\"true\"", example.RazorSource, StringComparison.Ordinal);
    }

    [Fact]
    public void BadgeDossierKeepsTheSameVisibleLabelForSpanAndLinkRenderModes()
    {
        var example = new ComponentExampleRegistry(new ComponentDocumentationCatalog()).GetBySlug("badge").Single();
        var span = Render(example.Preview).Find("[data-testid='badge-current'] [data-slot='badge']");
        Assert.Equal("span", span.NodeName.ToLowerInvariant());
        Assert.Contains("Ready for inspection", span.TextContent, StringComparison.Ordinal);

        example.Controls.Single(control => control.Id == "badge-link").Apply("true");
        var link = Render(example.Preview).Find("[data-testid='badge-current'] [data-slot='badge']");
        Assert.Equal("a", link.NodeName.ToLowerInvariant());
        Assert.Equal(span.TextContent.Trim(), link.TextContent.Trim());

        var css = File.ReadAllText(Path.Combine(FindRoot(), "samples", "Maliev.ShadcnBlazor.Showcase", "wwwroot", "css", "showcase.css"));
        Assert.DoesNotContain(".showcase-badge-dossier__current > span {", css, StringComparison.Ordinal);
        Assert.Contains(".showcase-badge-dossier__summary {", css, StringComparison.Ordinal);
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
            ["spinner-reduced-motion"] = "true",
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
            case "avatar-failed":
                Assert.Equal("images/avatars/missing-avatar.png", cut.Find("[data-slot='avatar-image']").GetAttribute("src"));
                break;
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
            case "spinner-reduced-motion": Assert.Equal("true", cut.Find("[data-slot='spinner']").GetAttribute("data-reduced-motion")); break;
            case "toast-limit": Assert.Equal("1", cut.Find("[data-slot='toast-viewport']").GetAttribute("data-maximum-visible")); break;
            case "toast-start": Assert.Equal("bottom-start", cut.Find("[data-slot='toast-viewport']").GetAttribute("data-placement")); break;
            case "toast-reduced": Assert.Equal("true", cut.Find("[data-slot='toast-viewport']").GetAttribute("data-reduced-motion")); break;
            case "toast-type": Assert.NotEmpty(cut.FindAll("[data-slot='toast'][data-type='error']")); break;
            case "toast-priority": Assert.NotEmpty(cut.FindAll("[data-slot='toast'][data-priority='high']")); break;
            default: throw new InvalidOperationException($"No exact dossier assertion exists for {controlId}.");
        }
    }

    [Fact]
    public void SpinnerDossierShowsAnInteractiveReportExportAndExactDynamicSource()
    {
        var example = new ComponentExampleRegistry(new ComponentDocumentationCatalog()).GetBySlug("spinner").Single();
        var cut = Render(example.Preview);

        Assert.Equal("true", cut.Find("[data-testid='spinner-export']").GetAttribute("aria-busy"));
        Assert.Contains("Preparing production report", cut.Markup, StringComparison.Ordinal);
        cut.Find("button").Click();
        Assert.Equal("false", cut.Find("[data-testid='spinner-export']").GetAttribute("aria-busy"));
        Assert.Contains("Export paused", cut.Markup, StringComparison.Ordinal);

        example.Controls.Single(control => control.Id == "spinner-decorative").Apply("true");
        example.Controls.Single(control => control.Id == "spinner-large").Apply("true");
        example.Controls.Single(control => control.Id == "spinner-reduced-motion").Apply("true");
        Assert.Contains("SpinnerRole=\"@(Decorative ? ShadcnSpinnerRole.None : ShadcnSpinnerRole.Status)\"", example.RazorSource, StringComparison.Ordinal);
        Assert.Contains("Label=\"@(Decorative ? null : \"Generating production report\")\"", example.RazorSource, StringComparison.Ordinal);
        Assert.Contains("private bool Decorative = true;", example.RazorSource, StringComparison.Ordinal);
        Assert.Contains("private bool Large = true;", example.RazorSource, StringComparison.Ordinal);
        Assert.Contains("private bool ReducedMotion = true;", example.RazorSource, StringComparison.Ordinal);
        Assert.Contains("private void ToggleExport()", example.RazorSource, StringComparison.Ordinal);
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
