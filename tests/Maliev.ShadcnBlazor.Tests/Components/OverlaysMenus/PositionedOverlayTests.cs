using Bunit;
using Maliev.ShadcnBlazor.Components.Overlays;
using Microsoft.AspNetCore.Components;

namespace Maliev.ShadcnBlazor.Tests.Components.OverlaysMenus;

public sealed class PositionedOverlayTests : BunitContext
{
    public PositionedOverlayTests()
    {
        var module = JSInterop.SetupModule("./_content/Maliev.ShadcnBlazor/js/shadcn-overlays-menus.js");
        module.SetupVoid("attachPositioned", _ => true);
        module.SetupVoid("detachPositioned", _ => true);
        module.SetupVoid("attachDelayedTrigger", _ => true);
        module.SetupVoid("detachDelayedTrigger", _ => true);
        var tooltipModule = JSInterop.SetupModule("./_content/Maliev.ShadcnBlazor/js/shadcn-tooltip.js");
        tooltipModule.SetupVoid("attachDelayedTrigger", _ => true);
        tooltipModule.SetupVoid("detachDelayedTrigger", _ => true);
    }

    [Fact]
    public void PopoverRendersControlledAccessibleCompositionAndPlacement()
    {
        var cut = Render<ShadcnPopover>(p => p.Add(x => x.Open, true).AddChildContent(builder =>
        {
            builder.OpenComponent<ShadcnPopoverTrigger>(0);
            builder.AddAttribute(1, nameof(ShadcnPopoverTrigger.ChildContent), (RenderFragment)(text => text.AddContent(0, "เปิดตัวเลือก")));
            builder.CloseComponent();
            builder.OpenComponent<ShadcnPopoverContent>(2);
            builder.AddAttribute(3, nameof(ShadcnPopoverContent.Side), ShadcnOverlaySide.Top);
            builder.AddAttribute(4, nameof(ShadcnPopoverContent.Align), ShadcnOverlayAlign.Start);
            builder.AddAttribute(5, nameof(ShadcnPopoverContent.SideOffset), 8d);
            builder.AddAttribute(6, nameof(ShadcnPopoverContent.ChildContent), (RenderFragment)(content =>
            {
                content.OpenComponent<ShadcnPopoverTitle>(0);
                content.AddAttribute(1, nameof(ShadcnPopoverTitle.ChildContent), (RenderFragment)(title => title.AddContent(0, "ตั้งค่ามิติ")));
                content.CloseComponent();
                content.OpenComponent<ShadcnPopoverDescription>(2);
                content.AddAttribute(3, nameof(ShadcnPopoverDescription.ChildContent), (RenderFragment)(description => description.AddContent(0, "กำหนดค่าที่ต้องการ")));
                content.CloseComponent();
            }));
            builder.CloseComponent();
        }));

        var trigger = cut.Find("[data-slot='popover-trigger']");
        var content = cut.Find("[data-slot='popover-content']");
        Assert.Equal("dialog", content.GetAttribute("role"));
        Assert.Equal("top", content.GetAttribute("data-side"));
        Assert.Equal("start", content.GetAttribute("data-align"));
        Assert.Equal("8", content.GetAttribute("data-side-offset"));
        Assert.Equal(content.Id, trigger.GetAttribute("aria-controls"));
        Assert.Equal(cut.Find("[data-slot='popover-title']").Id, content.GetAttribute("aria-labelledby"));
    }

    [Fact]
    public void HoverCardExposesDelayPlacementAndNonModalSemantics()
    {
        var cut = Render<ShadcnHoverCard>(p => p
            .Add(x => x.Open, true)
            .Add(x => x.OpenDelay, TimeSpan.FromMilliseconds(100))
            .Add(x => x.CloseDelay, TimeSpan.FromMilliseconds(200))
            .AddChildContent(builder =>
            {
                builder.OpenComponent<ShadcnHoverCardTrigger>(0);
                builder.AddAttribute(1, nameof(ShadcnHoverCardTrigger.ChildContent), (RenderFragment)(text => text.AddContent(0, "@maliev")));
                builder.CloseComponent();
                builder.OpenComponent<ShadcnHoverCardContent>(2);
                builder.AddAttribute(3, nameof(ShadcnHoverCardContent.Side), ShadcnOverlaySide.Right);
                builder.AddAttribute(4, nameof(ShadcnHoverCardContent.ChildContent), (RenderFragment)(content => content.AddContent(0, "โรงงานดิจิทัล")));
                builder.CloseComponent();
            }));

        var root = cut.Find("[data-slot='hover-card']");
        var content = cut.Find("[data-slot='hover-card-content']");
        Assert.Equal("100", root.GetAttribute("data-open-delay"));
        Assert.Equal("200", root.GetAttribute("data-close-delay"));
        Assert.Equal("right", content.GetAttribute("data-side"));
        Assert.False(content.HasAttribute("aria-modal"));
        Assert.Null(content.GetAttribute("role"));
    }

    [Fact]
    public void TooltipProviderControlsDelaysAndTooltipUsesDescriptionSemantics()
    {
        var cut = Render<ShadcnTooltipProvider>(p => p
            .Add(x => x.OpenDelay, TimeSpan.FromMilliseconds(300))
            .Add(x => x.CloseDelay, TimeSpan.FromMilliseconds(50))
            .AddChildContent(builder =>
            {
                builder.OpenComponent<ShadcnTooltip>(0);
                builder.AddAttribute(1, nameof(ShadcnTooltip.Open), true);
                builder.AddAttribute(2, nameof(ShadcnTooltip.ChildContent), (RenderFragment)(tooltip =>
                {
                    tooltip.OpenComponent<ShadcnTooltipTrigger>(0);
                    tooltip.AddAttribute(1, nameof(ShadcnTooltipTrigger.ChildContent), (RenderFragment)(text => text.AddContent(0, "บันทึก")));
                    tooltip.CloseComponent();
                    tooltip.OpenComponent<ShadcnTooltipContent>(2);
                    tooltip.AddAttribute(3, nameof(ShadcnTooltipContent.Side), ShadcnOverlaySide.Bottom);
                    tooltip.AddAttribute(4, nameof(ShadcnTooltipContent.ChildContent), (RenderFragment)(text => text.AddContent(0, "บันทึก Ctrl+S")));
                    tooltip.CloseComponent();
                }));
                builder.CloseComponent();
            }));

        var trigger = cut.Find("[data-slot='tooltip-trigger']");
        var content = cut.Find("[data-slot='tooltip-content']");
        Assert.Equal("tooltip", content.GetAttribute("role"));
        Assert.Equal(content.Id, trigger.GetAttribute("aria-describedby"));
        Assert.Equal("bottom", content.GetAttribute("data-side"));
        Assert.NotEmpty(cut.FindAll("[data-slot='tooltip-arrow']"));
        Assert.Equal("300", cut.Find("[data-slot='tooltip']").GetAttribute("data-open-delay"));
    }

    [Fact]
    public void DisabledTooltipTriggerNamesAndDescribesItsFocusableWrapper()
    {
        var cut = Render<ShadcnTooltipProvider>(parameters => parameters.Add(component => component.ChildContent, (RenderFragment)(builder =>
        {
            builder.OpenComponent<ShadcnTooltip>(0);
            builder.AddAttribute(1, nameof(ShadcnTooltip.Open), true);
            builder.AddAttribute(2, nameof(ShadcnTooltip.ChildContent), (RenderFragment)(builder =>
            {
                builder.OpenComponent<ShadcnTooltipTrigger>(0);
                builder.AddAttribute(1, nameof(ShadcnTooltipTrigger.Disabled), true);
                builder.AddAttribute(2, nameof(ShadcnTooltipTrigger.AccessibleLabel), "บันทึก");
                builder.AddAttribute(3, nameof(ShadcnTooltipTrigger.ChildContent), (RenderFragment)(text => text.AddContent(0, "บันทึก")));
                builder.CloseComponent();
                builder.OpenComponent<ShadcnTooltipContent>(4);
                builder.AddAttribute(5, nameof(ShadcnTooltipContent.ChildContent), (RenderFragment)(text => text.AddContent(0, "บันทึกใบเสนอราคา")));
                builder.CloseComponent();
            }));
            builder.CloseComponent();
        })));

        var wrapper = cut.Find("[data-slot='tooltip-trigger-wrapper']");
        var tooltipContent = cut.Find("[data-slot='tooltip-content']");
        Assert.Equal("button", wrapper.GetAttribute("role"));
        Assert.Equal("true", wrapper.GetAttribute("aria-disabled"));
        Assert.Equal("บันทึก", wrapper.GetAttribute("aria-label"));
        Assert.Equal(tooltipContent.Id, wrapper.GetAttribute("aria-describedby"));
        Assert.Equal("true", cut.Find("[data-slot='tooltip-trigger']").GetAttribute("aria-hidden"));
    }

    [Fact]
    public async Task UncontrolledTooltipRespondsToOpenAndCloseRequests()
    {
        var cut = Render<ShadcnTooltip>(p => p.AddChildContent(builder =>
        {
            builder.OpenComponent<ShadcnTooltipTrigger>(0);
            builder.AddAttribute(1, nameof(ShadcnTooltipTrigger.ChildContent), (RenderFragment)(text => text.AddContent(0, "Save")));
            builder.CloseComponent();
            builder.OpenComponent<ShadcnTooltipContent>(2);
            builder.AddAttribute(3, nameof(ShadcnTooltipContent.ChildContent), (RenderFragment)(text => text.AddContent(0, "Save quotation")));
            builder.CloseComponent();
        }));

        Assert.Equal("closed", cut.Find("[data-slot='tooltip']").GetAttribute("data-state"));

        await cut.Instance.RequestOpenAsync(true);
        Assert.Equal("open", cut.Find("[data-slot='tooltip']").GetAttribute("data-state"));
        Assert.Equal("Save quotation", cut.Find("[data-slot='tooltip-content']").TextContent.Trim());

        await cut.Instance.RequestCloseAsync();
        Assert.Equal("closed", cut.Find("[data-slot='tooltip']").GetAttribute("data-state"));
        Assert.Empty(cut.FindAll("[data-slot='tooltip-content']"));
    }

    [Fact]
    public void PositionedOverlaysRejectInvalidGeometryAndDelays()
    {
        Assert.ThrowsAny<Exception>(() => Render<ShadcnPopoverContent>(p => p.Add(x => x.Side, (ShadcnOverlaySide)999)));
        Assert.ThrowsAny<Exception>(() => Render<ShadcnHoverCard>(p => p.Add(x => x.OpenDelay, TimeSpan.FromMilliseconds(-1))));
        Assert.ThrowsAny<Exception>(() => Render<ShadcnTooltipProvider>(p => p.Add(x => x.CloseDelay, TimeSpan.FromDays(1))));
    }
}
