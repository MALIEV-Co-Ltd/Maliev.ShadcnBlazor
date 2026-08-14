using Bunit;
using Maliev.ShadcnBlazor.Components.Overlays;
using Microsoft.AspNetCore.Components;

namespace Maliev.ShadcnBlazor.Tests.Components.OverlaysMenus;

public sealed class SheetDrawerTests : BunitContext
{
    public SheetDrawerTests()
    {
        var module = JSInterop.SetupModule("./_content/Maliev.ShadcnBlazor/js/shadcn-overlays-menus.js");
        module.SetupVoid("attachDialog", _ => true);
        module.SetupVoid("detachDialog", _ => true);
        module.SetupVoid("attachDrawer", _ => true);
        module.SetupVoid("detachDrawer", _ => true);
    }

    [Fact]
    public void NonModalDrawerDoesNotRenderABlockingOverlay()
    {
        var cut = Render<ShadcnDrawer>(p => p.Add(x => x.Open, true).Add(x => x.ModalMode, ShadcnDrawerModalMode.NonModal).AddChildContent(builder =>
        {
            builder.OpenComponent<ShadcnDrawerContent>(0);
            builder.AddAttribute(1, nameof(ShadcnDrawerContent.ChildContent), (RenderFragment)(content =>
            {
                content.OpenComponent<ShadcnDrawerTitle>(0); content.AddAttribute(1, nameof(ShadcnDrawerTitle.ChildContent), (RenderFragment)(title => title.AddContent(0, "Non-modal"))); content.CloseComponent();
            }));
            builder.CloseComponent();
        }));
        Assert.Empty(cut.FindAll("[data-slot='drawer-overlay']"));
        Assert.Null(cut.Find("[data-slot='drawer-content']").GetAttribute("aria-modal"));
    }

    [Theory]
    [InlineData(ShadcnSheetSide.Top, "top")]
    [InlineData(ShadcnSheetSide.Right, "right")]
    [InlineData(ShadcnSheetSide.Bottom, "bottom")]
    [InlineData(ShadcnSheetSide.Left, "left")]
    public void SheetRendersEverySideWithDialogSemantics(ShadcnSheetSide side, string expected)
    {
        var cut = Render<ShadcnSheet>(p => p.Add(x => x.Open, true).AddChildContent(builder =>
        {
            builder.OpenComponent<ShadcnSheetContent>(0);
            builder.AddAttribute(1, nameof(ShadcnSheetContent.Side), side);
            builder.AddAttribute(2, nameof(ShadcnSheetContent.ChildContent), (RenderFragment)(content =>
            {
                content.OpenComponent<ShadcnSheetTitle>(0);
                content.AddAttribute(1, nameof(ShadcnSheetTitle.ChildContent), (RenderFragment)(title => title.AddContent(0, "รายละเอียดคำสั่งซื้อ")));
                content.CloseComponent();
                content.OpenComponent<ShadcnSheetDescription>(2);
                content.AddAttribute(3, nameof(ShadcnSheetDescription.ChildContent), (RenderFragment)(description => description.AddContent(0, "แก้ไขข้อมูลจากแผงด้านข้าง")));
                content.CloseComponent();
            }));
            builder.CloseComponent();
        }));

        var content = cut.Find("[data-slot='sheet-content']");
        Assert.Equal(expected, content.GetAttribute("data-side"));
        Assert.Equal("dialog", content.GetAttribute("role"));
        Assert.Equal(cut.Find("[data-slot='sheet-title']").Id, content.GetAttribute("aria-labelledby"));
    }

    [Fact]
    public void DrawerExposesTypedDirectionAxisSnapAndModalState()
    {
        var snapChanges = new List<ShadcnDrawerSnapPoint?>();
        var points = new[] { ShadcnDrawerSnapPoint.Fraction(0.25), ShadcnDrawerSnapPoint.Rem(24), ShadcnDrawerSnapPoint.Fraction(1) };
        var cut = Render<ShadcnDrawer>(p => p
            .Add(x => x.Open, true)
            .Add(x => x.SwipeDirection, ShadcnDrawerSwipeDirection.Down)
            .Add(x => x.ModalMode, ShadcnDrawerModalMode.TrapFocus)
            .Add(x => x.ShowSwipeHandle, true)
            .Add(x => x.SnapPoints, points)
            .Add(x => x.SnapPoint, points[1])
            .Add(x => x.SnapPointChanged, value => snapChanges.Add(value))
            .AddChildContent(builder =>
            {
                builder.OpenComponent<ShadcnDrawerContent>(0);
                builder.AddAttribute(1, nameof(ShadcnDrawerContent.ChildContent), (RenderFragment)(content =>
                {
                    content.OpenComponent<ShadcnDrawerTitle>(0);
                    content.AddAttribute(1, nameof(ShadcnDrawerTitle.ChildContent), (RenderFragment)(title => title.AddContent(0, "ตัวกรอง")));
                    content.CloseComponent();
                    content.OpenComponent<ShadcnDrawerDescription>(2);
                    content.AddAttribute(3, nameof(ShadcnDrawerDescription.ChildContent), (RenderFragment)(description => description.AddContent(0, "เลือกเงื่อนไข")));
                    content.CloseComponent();
                }));
                builder.CloseComponent();
            }));

        var root = cut.Find("[data-slot='drawer']");
        var content = cut.Find("[data-slot='drawer-content']");
        Assert.Equal("down", root.GetAttribute("data-swipe-direction"));
        Assert.Equal("y", root.GetAttribute("data-swipe-axis"));
        Assert.Equal("trap-focus", root.GetAttribute("data-modal"));
        Assert.True(root.HasAttribute("data-snap-points"));
        Assert.Equal("24rem", content.GetAttribute("data-snap-point"));
        Assert.NotEmpty(cut.FindAll("[data-slot='drawer-swipe-handle']"));
        Assert.Equal("dialog", content.GetAttribute("role"));
    }

    [Fact]
    public void DrawerValidatesSnapPointsDirectionAndCurrentValue()
    {
        Assert.Throws<ArgumentOutOfRangeException>(() => ShadcnDrawerSnapPoint.Fraction(0));
        Assert.Throws<ArgumentOutOfRangeException>(() => ShadcnDrawerSnapPoint.Rem(-1));
        Assert.ThrowsAny<Exception>(() => Render<ShadcnDrawer>(p => p.Add(x => x.SwipeDirection, (ShadcnDrawerSwipeDirection)999)));
        Assert.ThrowsAny<Exception>(() => Render<ShadcnDrawer>(p => p
            .Add(x => x.SnapPoints, new[] { ShadcnDrawerSnapPoint.Fraction(0.75), ShadcnDrawerSnapPoint.Fraction(0.25) })));
        Assert.ThrowsAny<Exception>(() => Render<ShadcnDrawer>(p => p
            .Add(x => x.SwipeDirection, ShadcnDrawerSwipeDirection.Left)
            .Add(x => x.SnapPoints, new[] { ShadcnDrawerSnapPoint.Fraction(0.5) })));
    }
}
