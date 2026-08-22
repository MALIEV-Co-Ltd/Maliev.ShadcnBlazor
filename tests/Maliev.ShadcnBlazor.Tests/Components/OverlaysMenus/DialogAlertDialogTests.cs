using Bunit;
using Maliev.ShadcnBlazor.Components.Overlays;
using Microsoft.AspNetCore.Components;

namespace Maliev.ShadcnBlazor.Tests.Components.OverlaysMenus;

public sealed class DialogAlertDialogTests : BunitContext
{
    public DialogAlertDialogTests()
    {
        var module = JSInterop.SetupModule("./_content/Maliev.ShadcnBlazor/js/shadcn-overlays-menus.js");
        module.SetupVoid("attachDialog", _ => true);
        module.SetupVoid("detachDialog", _ => true);
    }

    [Fact]
    public void DialogRendersControlledModalCompositionAndStableAccessibleRelationships()
    {
        var cut = RenderDialog(open: true);
        var trigger = cut.Find("button[data-slot='dialog-trigger']");
        var content = cut.Find("[data-slot='dialog-content']");
        var title = cut.Find("[data-slot='dialog-title']");
        var description = cut.Find("[data-slot='dialog-description']");

        Assert.Equal("dialog", content.GetAttribute("role"));
        Assert.Equal("true", content.GetAttribute("aria-modal"));
        Assert.Equal(title.Id, content.GetAttribute("aria-labelledby"));
        Assert.Equal(description.Id, content.GetAttribute("aria-describedby"));
        Assert.Equal(content.Id, trigger.GetAttribute("aria-controls"));
        Assert.Equal("true", trigger.GetAttribute("aria-expanded"));
        Assert.Equal("open", content.GetAttribute("data-state"));
        Assert.Contains("ยืนยันการแก้ไข", title.TextContent, StringComparison.Ordinal);
    }

    [Fact]
    public void DialogRequestsControlledStateAndSuppressesDisabledTrigger()
    {
        var changes = new List<bool>();
        var cut = RenderDialog(open: false, changed: changes.Add);
        cut.Find("[data-slot='dialog-trigger']").Click();
        Assert.Equal([true], changes);
        Assert.Empty(cut.FindAll("[data-slot='dialog-content']"));

        cut.Render(p => p
            .Add(x => x.Open, false)
            .Add(x => x.OpenChanged, changes.Add)
            .AddChildContent(BuildDialogContent(disabledTrigger: true)));
        cut.Find("[data-slot='dialog-trigger']").Click();
        Assert.Single(changes);
    }

    [Fact]
    public void DialogForwardsCustomizationOnlyToOwningSlotsAndCanHideCloseButton()
    {
        var cut = Render<ShadcnDialog>(p => p
            .Add(x => x.Open, true)
            .Add(x => x.Class, "root-layout")
            .AddUnmatched("data-consumer", "dialog")
            .AddChildContent(builder =>
            {
                builder.OpenComponent<ShadcnDialogContent>(0);
                builder.AddAttribute(1, nameof(ShadcnDialogContent.ShowCloseButton), false);
                builder.AddAttribute(2, nameof(ShadcnDialogContent.Class), "content-layout");
                builder.AddAttribute(3, nameof(ShadcnDialogContent.ChildContent), (RenderFragment)(content =>
                {
                    content.OpenComponent<ShadcnDialogTitle>(0);
                    content.AddAttribute(1, nameof(ShadcnDialogTitle.ChildContent), (RenderFragment)(title => title.AddContent(0, "Title")));
                    content.CloseComponent();
                }));
                builder.CloseComponent();
            }));

        Assert.Contains("root-layout", cut.Find("[data-slot='dialog']").ClassList);
        Assert.Equal("dialog", cut.Find("[data-slot='dialog']").GetAttribute("data-consumer"));
        Assert.Contains("content-layout", cut.Find("[data-slot='dialog-content']").ClassList);
        Assert.Empty(cut.FindAll("[data-slot='dialog-close']"));
        Assert.Null(cut.Find("[data-slot='dialog-content']").GetAttribute("data-consumer"));
    }

    [Fact]
    public void DialogDistinguishesTheIconDismissalFromFooterCloseActions()
    {
        var cut = Render<ShadcnDialog>(parameters => parameters
            .Add(component => component.Open, true)
            .AddChildContent(builder =>
            {
                builder.OpenComponent<ShadcnDialogContent>(0);
                builder.AddAttribute(1, nameof(ShadcnDialogContent.ChildContent), (RenderFragment)(content =>
                {
                    content.OpenComponent<ShadcnDialogTitle>(0);
                    content.AddAttribute(1, nameof(ShadcnDialogTitle.ChildContent), (RenderFragment)(title => title.AddContent(0, "Edit profile")));
                    content.CloseComponent();
                    content.OpenComponent<ShadcnDialogFooter>(2);
                    content.AddAttribute(3, nameof(ShadcnDialogFooter.ChildContent), (RenderFragment)(footer =>
                    {
                        footer.OpenComponent<ShadcnDialogClose>(0);
                        footer.AddAttribute(1, nameof(ShadcnDialogClose.ChildContent), (RenderFragment)(text => text.AddContent(0, "Save changes")));
                        footer.CloseComponent();
                    }));
                    content.CloseComponent();
                }));
                builder.CloseComponent();
            }));

        var closeButtons = cut.FindAll("[data-slot='dialog-close']");
        Assert.Equal(2, closeButtons.Count);
        var iconClose = Assert.Single(closeButtons, button => button.GetAttribute("data-icon-only") == "true");
        var footerClose = Assert.Single(closeButtons, button => button.GetAttribute("data-icon-only") == "false");
        Assert.Empty(iconClose.TextContent);
        Assert.Equal("Close", iconClose.GetAttribute("aria-label"));
        Assert.Null(footerClose.GetAttribute("aria-label"));
        Assert.Contains("Save changes", footerClose.TextContent, StringComparison.Ordinal);

        footerClose.Click();
        Assert.Empty(cut.FindAll("[data-slot='dialog-content']"));
    }

    [Fact]
    public void AlertDialogUsesAlertSemanticsSmallMediaAndCancelActionContracts()
    {
        var cancelCalls = 0;
        var actionCalls = 0;
        var cut = Render<ShadcnAlertDialog>(p => p
            .Add(x => x.Open, true)
            .Add(x => x.OpenChanged, _ => { })
            .AddChildContent(builder =>
            {
                builder.OpenComponent<ShadcnAlertDialogContent>(0);
                builder.AddAttribute(1, nameof(ShadcnAlertDialogContent.Size), ShadcnAlertDialogSize.Small);
                builder.AddAttribute(2, nameof(ShadcnAlertDialogContent.ChildContent), (RenderFragment)(content =>
                {
                    content.OpenComponent<ShadcnAlertDialogMedia>(0);
                    content.AddAttribute(1, nameof(ShadcnAlertDialogMedia.ChildContent), (RenderFragment)(media => media.AddContent(0, "!")));
                    content.CloseComponent();
                    content.OpenComponent<ShadcnAlertDialogTitle>(2);
                    content.AddAttribute(3, nameof(ShadcnAlertDialogTitle.ChildContent), (RenderFragment)(title => title.AddContent(0, "ลบรายการนี้หรือไม่")));
                    content.CloseComponent();
                    content.OpenComponent<ShadcnAlertDialogDescription>(4);
                    content.AddAttribute(5, nameof(ShadcnAlertDialogDescription.ChildContent), (RenderFragment)(description => description.AddContent(0, "ไม่สามารถย้อนกลับได้")));
                    content.CloseComponent();
                    content.OpenComponent<ShadcnAlertDialogFooter>(6);
                    content.AddAttribute(7, nameof(ShadcnAlertDialogFooter.ChildContent), (RenderFragment)(footer =>
                    {
                        footer.OpenComponent<ShadcnAlertDialogCancel>(0);
                        footer.AddAttribute(1, nameof(ShadcnAlertDialogCancel.OnClick), EventCallback.Factory.Create<Microsoft.AspNetCore.Components.Web.MouseEventArgs>(this, _ => cancelCalls++));
                        footer.AddAttribute(2, nameof(ShadcnAlertDialogCancel.ChildContent), (RenderFragment)(text => text.AddContent(0, "ยกเลิก")));
                        footer.CloseComponent();
                        footer.OpenComponent<ShadcnAlertDialogAction>(3);
                        footer.AddAttribute(4, nameof(ShadcnAlertDialogAction.OnClick), EventCallback.Factory.Create<Microsoft.AspNetCore.Components.Web.MouseEventArgs>(this, _ => actionCalls++));
                        footer.AddAttribute(5, nameof(ShadcnAlertDialogAction.ChildContent), (RenderFragment)(text => text.AddContent(0, "ดำเนินการ")));
                        footer.CloseComponent();
                    }));
                    content.CloseComponent();
                }));
                builder.CloseComponent();
            }));

        var content = cut.Find("[data-slot='alert-dialog-content']");
        Assert.Equal("alertdialog", content.GetAttribute("role"));
        Assert.Equal("sm", content.GetAttribute("data-size"));
        Assert.NotEmpty(cut.FindAll("[data-slot='alert-dialog-media']"));
        cut.Find("[data-slot='alert-dialog-cancel']").Click();
        cut.Find("[data-slot='alert-dialog-action']").Click();
        Assert.Equal(1, cancelCalls);
        Assert.Equal(1, actionCalls);

        var css = File.ReadAllText(Path.Combine(FindRepositoryRoot(), "src", "Maliev.ShadcnBlazor", "wwwroot", "css", "shadcn-overlays-menus.css"));
        Assert.Contains(".shadcn-alert-dialog-header {", css, StringComparison.Ordinal);
        Assert.Contains("place-items: start", css, StringComparison.Ordinal);
        Assert.DoesNotContain(".shadcn-alert-dialog-content[data-size=\"sm\"] { width: min(calc(100vw - 2rem), 24rem); text-align: center; }", css, StringComparison.Ordinal);
    }

    [Fact]
    public void DialogAndAlertDialogRejectInvalidContracts()
    {
        Assert.ThrowsAny<Exception>(() => Render<ShadcnDialog>(p => p.Add(x => x.Open, true).AddChildContent(builder =>
        {
            builder.OpenComponent<ShadcnDialogContent>(0);
            builder.CloseComponent();
        })));
        Assert.ThrowsAny<Exception>(() => Render<ShadcnAlertDialogContent>(p => p.Add(x => x.Size, (ShadcnAlertDialogSize)999)));
    }

    [Fact]
    public void DialogInteropKeepsAttachmentIdempotentAndRestoresFocusAfterTheCloseRender()
    {
        var script = File.ReadAllText(Path.Combine(
            FindRepositoryRoot(),
            "src",
            "Maliev.ShadcnBlazor",
            "wwwroot",
            "js",
            "shadcn-overlays-menus.js"));

        Assert.Contains("if (dialogs.has(content)) return;", script, StringComparison.Ordinal);
        Assert.Contains("requestAnimationFrame(restoreFocus)", script, StringComparison.Ordinal);
        Assert.Contains("focusOwner?.querySelector?.('[data-slot=\"dialog-trigger\"]')", script, StringComparison.Ordinal);
    }

    private IRenderedComponent<ShadcnDialog> RenderDialog(bool open, Action<bool>? changed = null) => Render<ShadcnDialog>(p => p
        .Add(x => x.Open, open)
        .Add(x => x.OpenChanged, changed is null ? default : EventCallback.Factory.Create(this, changed))
        .AddChildContent(BuildDialogContent()));

    private static RenderFragment BuildDialogContent(bool disabledTrigger = false) => builder =>
    {
        builder.OpenComponent<ShadcnDialogTrigger>(0);
        builder.AddAttribute(1, nameof(ShadcnDialogTrigger.Disabled), disabledTrigger);
        builder.AddAttribute(2, nameof(ShadcnDialogTrigger.ChildContent), (RenderFragment)(text => text.AddContent(0, "เปิด")));
        builder.CloseComponent();
        builder.OpenComponent<ShadcnDialogContent>(3);
        builder.AddAttribute(4, nameof(ShadcnDialogContent.ChildContent), (RenderFragment)(content =>
        {
            content.OpenComponent<ShadcnDialogHeader>(0);
            content.AddAttribute(1, nameof(ShadcnDialogHeader.ChildContent), (RenderFragment)(header =>
            {
                header.OpenComponent<ShadcnDialogTitle>(0);
                header.AddAttribute(1, nameof(ShadcnDialogTitle.ChildContent), (RenderFragment)(title => title.AddContent(0, "ยืนยันการแก้ไข")));
                header.CloseComponent();
                header.OpenComponent<ShadcnDialogDescription>(2);
                header.AddAttribute(3, nameof(ShadcnDialogDescription.ChildContent), (RenderFragment)(description => description.AddContent(0, "ตรวจสอบข้อมูลก่อนดำเนินการ")));
                header.CloseComponent();
            }));
            content.CloseComponent();
        }));
        builder.CloseComponent();
    };

    private static string FindRepositoryRoot()
    {
        var directory = new DirectoryInfo(AppContext.BaseDirectory);
        while (directory is not null && !File.Exists(Path.Combine(directory.FullName, "Maliev.ShadcnBlazor.slnx")))
            directory = directory.Parent;

        return directory?.FullName ?? throw new DirectoryNotFoundException("Repository root was not found.");
    }
}
