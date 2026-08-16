using Bunit;
using Maliev.ShadcnBlazor.Components.Conversation;
using Microsoft.AspNetCore.Components;

namespace Maliev.ShadcnBlazor.Tests.Components.Conversation;

public sealed class AttachmentTests : BunitContext
{
    [Theory]
    [InlineData(ShadcnAttachmentState.Idle, "idle")]
    [InlineData(ShadcnAttachmentState.Uploading, "uploading")]
    [InlineData(ShadcnAttachmentState.Processing, "processing")]
    [InlineData(ShadcnAttachmentState.Error, "error")]
    [InlineData(ShadcnAttachmentState.Done, "done")]
    public void AttachmentOwnsPinnedStateSizeOrientationAndProgress(ShadcnAttachmentState state, string expected)
    {
        var cut = Render<ShadcnAttachment>(parameters => parameters
            .Add(component => component.State, state)
            .Add(component => component.Size, ShadcnAttachmentSize.Small)
            .Add(component => component.Orientation, ShadcnAttachmentOrientation.Vertical)
            .Add(component => component.Progress, state is ShadcnAttachmentState.Uploading ? 64 : null)
            .Add(component => component.ErrorReason, "Upload failed")
            .Add(component => component.AdditionalAttributes, new Dictionary<string, object>
            {
                ["data-state"] = "wrong",
                ["data-testid"] = "attachment"
            })
            .AddChildContent("drawing.step"));

        var root = cut.Find("[data-slot='attachment']");
        Assert.Equal(expected, root.GetAttribute("data-state"));
        Assert.Equal("sm", root.GetAttribute("data-size"));
        Assert.Equal("vertical", root.GetAttribute("data-orientation"));
        Assert.Equal("attachment", root.GetAttribute("data-testid"));
        if (state is ShadcnAttachmentState.Uploading)
        {
            var progress = cut.Find("[data-slot='attachment-progress']");
            Assert.Equal("64", progress.GetAttribute("aria-valuenow"));
            Assert.Equal("progressbar", progress.GetAttribute("role"));
            Assert.Equal("Attachment upload progress", progress.GetAttribute("aria-label"));
            Assert.Null(root.GetAttribute("role"));
        }
    }

    [Fact]
    public void AttachmentRendersExactCompositionAndIndependentActions()
    {
        var action = ShadcnAttachmentActionKind.None;
        var activated = false;
        var cut = Render<ShadcnAttachment>(parameters => parameters.AddChildContent(builder =>
        {
            builder.OpenComponent<ShadcnAttachmentMedia>(0);
            builder.AddAttribute(1, nameof(ShadcnAttachmentMedia.Variant), ShadcnAttachmentMediaVariant.Image);
            builder.AddAttribute(2, nameof(ShadcnAttachmentMedia.ImageAlt), "Preview of ใบเสนอราคา.pdf");
            builder.AddAttribute(3, nameof(ShadcnAttachmentMedia.ChildContent), Text("preview"));
            builder.CloseComponent();
            builder.OpenComponent<ShadcnAttachmentContent>(3);
            builder.AddAttribute(4, nameof(ShadcnAttachmentContent.ChildContent), (RenderFragment)(content =>
            {
                content.OpenComponent<ShadcnAttachmentTitle>(0);
                content.AddAttribute(1, nameof(ShadcnAttachmentTitle.ChildContent), Text("ใบเสนอราคา.pdf"));
                content.CloseComponent();
                content.OpenComponent<ShadcnAttachmentDescription>(2);
                content.AddAttribute(3, nameof(ShadcnAttachmentDescription.ChildContent), Text("PDF · 2.4 MB"));
                content.CloseComponent();
            }));
            builder.CloseComponent();
            builder.OpenComponent<ShadcnAttachmentActions>(5);
            builder.AddAttribute(6, nameof(ShadcnAttachmentActions.ChildContent), (RenderFragment)(actions =>
            {
                actions.OpenComponent<ShadcnAttachmentAction>(0);
                actions.AddAttribute(1, nameof(ShadcnAttachmentAction.Action), ShadcnAttachmentActionKind.Remove);
                actions.AddAttribute(2, nameof(ShadcnAttachmentAction.AccessibleName), "Remove ใบเสนอราคา.pdf");
                actions.AddAttribute(3, nameof(ShadcnAttachmentAction.OnAction), EventCallback.Factory.Create<ShadcnAttachmentActionKind>(this, value => action = value));
                actions.AddAttribute(4, nameof(ShadcnAttachmentAction.ChildContent), Text("×"));
                actions.CloseComponent();
            }));
            builder.CloseComponent();
            builder.OpenComponent<ShadcnAttachmentTrigger>(7);
            builder.AddAttribute(8, nameof(ShadcnAttachmentTrigger.AccessibleName), "Preview ใบเสนอราคา.pdf");
            builder.AddAttribute(9, nameof(ShadcnAttachmentTrigger.OnActivate), EventCallback.Factory.Create(this, () => activated = true));
            builder.CloseComponent();
        }));

        Assert.Equal("image", cut.Find("[data-slot='attachment-media']").GetAttribute("data-variant"));
        Assert.Equal("ใบเสนอราคา.pdf", cut.Find("[data-slot='attachment-title']").TextContent);
        Assert.Equal("PDF · 2.4 MB", cut.Find("[data-slot='attachment-description']").TextContent);
        cut.Find("[data-slot='attachment-action']").Click();
        Assert.Equal(ShadcnAttachmentActionKind.Remove, action);
        Assert.False(activated);
        cut.Find("[data-slot='attachment-trigger']").Click();
        Assert.True(activated);
    }

    [Fact]
    public void AttachmentGroupAndCssExposeScrollableAccessibleLifecycleTreatment()
    {
        var group = Render<ShadcnAttachmentGroup>(parameters => parameters
            .Add(component => component.AccessibleName, "Uploaded drawings")
            .AddChildContent<ShadcnAttachment>(attachment => attachment.AddChildContent("drawing.step")));

        var root = group.Find("[data-slot='attachment-group']");
        Assert.Equal("group", root.GetAttribute("role"));
        Assert.Equal("Uploaded drawings", root.GetAttribute("aria-label"));
        Assert.Equal("0", root.GetAttribute("tabindex"));

        var css = File.ReadAllText(Path.Combine(AppContext.BaseDirectory, "..", "..", "..", "..", "..", "src", "Maliev.ShadcnBlazor", "wwwroot", "css", "shadcn-conversation.css"));
        Assert.Contains("scroll-snap-type: inline mandatory", css, StringComparison.Ordinal);
        Assert.Contains("[data-state=\"error\"]", css, StringComparison.Ordinal);
        Assert.Contains("background: color-mix(in srgb, var(--shadcn-destructive)", css, StringComparison.Ordinal);
        Assert.Contains("prefers-reduced-motion", css, StringComparison.Ordinal);
        Assert.Contains("forced-colors", css, StringComparison.Ordinal);
    }

    [Fact]
    public void AttachmentRejectsInvalidEnumsProgressAndUnnamedInteractiveParts()
    {
        Assert.ThrowsAny<Exception>(() => Render<ShadcnAttachment>());
        Assert.ThrowsAny<Exception>(() => Render<ShadcnAttachment>(p => p.Add(c => c.State, (ShadcnAttachmentState)99).AddChildContent("file")));
        Assert.ThrowsAny<Exception>(() => Render<ShadcnAttachment>(p => p.Add(c => c.Progress, 101).AddChildContent("file")));
        Assert.ThrowsAny<Exception>(() => Render<ShadcnAttachmentAction>(p => p.AddChildContent("×")));
        Assert.ThrowsAny<Exception>(() => Render<ShadcnAttachmentTrigger>());
    }

    [Fact]
    public void TypedFileValidationEnforcesTypeExtensionSizeCountAndCallerRules()
    {
        var files = new[] { new ShadcnAttachmentFile("safe.step", 400, "application/step"), new ShadcnAttachmentFile("large.pdf", 2_000, "application/pdf") };
        var result = ShadcnAttachmentValidator.Validate(files, new()
        {
            AcceptedExtensions = new HashSet<string>([".step", ".pdf"], StringComparer.OrdinalIgnoreCase),
            MaximumFileSize = 1_000,
            MaximumFileCount = 1,
            ValidateFile = file => file.Name == "safe.step" ? "Caller rejected this drawing." : null
        });
        Assert.False(result.IsValid); Assert.Empty(result.Accepted);
        Assert.Contains(result.Errors, error => error.Code == "count");
        Assert.Contains(result.Errors, error => error.Code == "size" && error.FileName == "large.pdf");
        Assert.Contains(result.Errors, error => error.Code == "custom" && error.FileName == "safe.step");
        Assert.False(ShadcnAttachmentValidator.Validate([new("bad.exe", 1, "application/octet-stream")], new() { AcceptedTypes = new HashSet<string>(["application/pdf"]) }).IsValid);
    }

    [Fact]
    public void AttachmentEnforcesControlledOwnershipAndAccessibleErrorImageContracts()
    {
        Assert.ThrowsAny<Exception>(() => Render<ShadcnAttachment>(p => p.Add(c => c.State, ShadcnAttachmentState.Error).AddChildContent("file")));
        Assert.ThrowsAny<Exception>(() => Render<ShadcnAttachment>(p => p.Add(c => c.State, ShadcnAttachmentState.Done).Add(c => c.StateIsControlled, true).Add(c => c.File, new("file.pdf", 2, "application/pdf")).Add(c => c.Title, "file.pdf").AddChildContent("file")));
        Assert.ThrowsAny<Exception>(() => Render<ShadcnAttachment>(p => p.AddChildContent<ShadcnAttachmentMedia>(media => media.Add(c => c.Variant, ShadcnAttachmentMediaVariant.Image).AddChildContent("preview"))));
    }

    [Fact]
    public void IndeterminateProcessingAndPolymorphicTriggerRemainSemantic()
    {
        var processing = Render<ShadcnAttachment>(p => p.Add(c => c.State, ShadcnAttachmentState.Processing).AddChildContent("processing"));
        Assert.Equal("status", processing.Find("[data-slot='attachment']").GetAttribute("role"));
        Assert.Equal("polite", processing.Find("[data-slot='attachment']").GetAttribute("aria-live"));
        var link = Render<ShadcnAttachment>(p => p.AddChildContent<ShadcnAttachmentTrigger>(trigger => trigger.Add(c => c.AccessibleName, "Open drawing").Add(c => c.Href, "/drawing/1").AddChildContent("Open")));
        Assert.Equal("/drawing/1", link.Find("a[data-slot='attachment-trigger']").GetAttribute("href"));
        var dialog = Render<ShadcnAttachment>(p => p.AddChildContent<ShadcnAttachmentTrigger>(trigger => trigger.Add(c => c.AccessibleName, "Preview drawing").Add(c => c.DialogTarget, "drawing-dialog").AddChildContent("Preview")));
        Assert.Equal("dialog", dialog.Find("button[data-slot='attachment-trigger']").GetAttribute("aria-haspopup"));
        Assert.Equal("drawing-dialog", dialog.Find("button[data-slot='attachment-trigger']").GetAttribute("aria-controls"));
    }

    [Fact]
    public void AttachmentActionsRespectUncontrolledAndControlledStateOwnership()
    {
        var uncontrolled = Render<ShadcnAttachment>(p => p.Add(c => c.State, ShadcnAttachmentState.Done).AddChildContent<ShadcnAttachmentAction>(action => action.Add(c => c.Action, ShadcnAttachmentActionKind.Retry).Add(c => c.AccessibleName, "Retry")));
        uncontrolled.Find("[data-slot='attachment-action']").Click();
        Assert.Equal("uploading", uncontrolled.Find("[data-slot='attachment']").GetAttribute("data-state"));

        var requested = ShadcnAttachmentState.Done;
        var controlled = Render<ShadcnAttachment>(p => p.Add(c => c.State, ShadcnAttachmentState.Done).Add(c => c.StateIsControlled, true).Add(c => c.StateChanged, value => requested = value).AddChildContent<ShadcnAttachmentAction>(action => action.Add(c => c.Action, ShadcnAttachmentActionKind.Retry).Add(c => c.AccessibleName, "Retry")));
        controlled.Find("[data-slot='attachment-action']").Click();
        Assert.Equal(ShadcnAttachmentState.Uploading, requested);
        Assert.Equal("done", controlled.Find("[data-slot='attachment']").GetAttribute("data-state"));
    }

    private static RenderFragment Text(string value) => builder => builder.AddContent(0, value);
}
