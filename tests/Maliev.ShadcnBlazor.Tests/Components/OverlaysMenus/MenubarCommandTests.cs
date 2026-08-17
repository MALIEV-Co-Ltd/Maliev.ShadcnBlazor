using Bunit;
using Maliev.ShadcnBlazor.Components.Overlays;
using Microsoft.AspNetCore.Components;
using Microsoft.JSInterop;

namespace Maliev.ShadcnBlazor.Tests.Components.OverlaysMenus;

public sealed class MenubarCommandTests : BunitContext
{
    public MenubarCommandTests()
    {
        var module = JSInterop.SetupModule("./_content/Maliev.ShadcnBlazor/js/shadcn-overlays-menus.js");
        module.SetupVoid("attachMenubar", _ => true); module.SetupVoid("detachMenubar", _ => true);
        module.SetupVoid("attachMenu", _ => true); module.SetupVoid("detachMenu", _ => true);
        module.SetupVoid("attachPositioned", _ => true); module.SetupVoid("detachPositioned", _ => true);
        module.SetupVoid("attachCommand", _ => true); module.SetupVoid("detachCommand", _ => true);
        module.SetupVoid("refreshCommand", _ => true);
    }

    [Fact]
    public void MenubarComposesTopLevelMenusAndSharedSelectionSlots()
    {
        var cut = Render<ShadcnMenubar>(p => p.Add(x => x.Label, "เมนูแอปพลิเคชัน").AddChildContent(builder =>
        {
            AddMenu(builder, 0, "ไฟล์", "สร้างใหม่");
            AddMenu(builder, 10, "แก้ไข", "เลิกทำ");
        }));
        var root = cut.Find("[data-slot='menubar']");
        var triggers = cut.FindAll("[data-slot='menubar-trigger']");
        Assert.Equal("menubar", root.GetAttribute("role"));
        Assert.Equal("เมนูแอปพลิเคชัน", root.GetAttribute("aria-label"));
        Assert.Equal("menuitem", triggers[0].GetAttribute("role"));
        Assert.Equal("0", triggers[0].GetAttribute("tabindex"));
        Assert.Equal("-1", triggers[1].GetAttribute("tabindex"));
        triggers[0].Click();
        Assert.Equal("true", cut.FindAll("[data-slot='menubar-trigger']")[0].GetAttribute("aria-expanded"));
        Assert.Equal("menu", cut.Find("[data-slot='menubar-content']").GetAttribute("role"));
        Assert.Equal("menuitem", cut.Find("[data-slot='menubar-item']").GetAttribute("role"));
    }

    [Fact]
    public void MenubarRecomputesRovingTabIndexAfterDynamicMenuChanges()
    {
        var labels = new List<string> { "ไฟล์", "แก้ไข" };
        RenderFragment content = builder =>
        {
            for (var index = 0; index < labels.Count; index++)
            {
                builder.OpenComponent<ShadcnMenubarMenu>(index);
                builder.SetKey(labels[index]);
                var label = labels[index];
                builder.AddAttribute(index + 10, nameof(ShadcnMenubarMenu.ChildContent), (RenderFragment)(menu =>
                {
                    menu.OpenComponent<ShadcnMenubarTrigger>(0);
                    menu.AddAttribute(1, nameof(ShadcnMenubarTrigger.ChildContent), (RenderFragment)(text => text.AddContent(0, label)));
                    menu.CloseComponent();
                }));
                builder.CloseComponent();
            }
        };
        var cut = Render<ShadcnMenubar>(parameters => parameters.Add(component => component.Label, "เมนู").Add(component => component.ChildContent, content));

        labels.RemoveAt(0);
        cut.Render();
        Assert.Equal("0", cut.Find("[data-slot='menubar-trigger']").GetAttribute("tabindex"));

        labels.Insert(0, "มุมมอง");
        cut.Render();
        var triggers = cut.FindAll("[data-slot='menubar-trigger']");
        Assert.Equal("0", triggers[0].GetAttribute("tabindex"));
        Assert.Equal("-1", triggers[1].GetAttribute("tabindex"));
    }

    [Fact]
    public void MenubarPublishesOpenAndLoopStateForInteractionAndStyling()
    {
        RenderFragment content = builder =>
        {
            builder.OpenComponent<ShadcnMenubarMenu>(0);
            builder.AddAttribute(1, nameof(ShadcnMenubarMenu.Open), true);
            builder.AddAttribute(2, nameof(ShadcnMenubarMenu.ChildContent), (RenderFragment)(menu =>
            {
                menu.OpenComponent<ShadcnMenubarTrigger>(0);
                menu.AddAttribute(1, nameof(ShadcnMenubarTrigger.ChildContent), (RenderFragment)(text => text.AddContent(0, "File")));
                menu.CloseComponent();
                menu.OpenComponent<ShadcnMenubarContent>(2);
                menu.CloseComponent();
            }));
            builder.CloseComponent();
        };
        var cut = Render<ShadcnMenubar>(parameters => parameters
            .Add(component => component.Label, "Workspace menu")
            .Add(component => component.Loop, false)
            .Add(component => component.ChildContent, content));

        Assert.Equal("false", cut.Find("[data-slot='menubar']").GetAttribute("data-loop"));
        Assert.Equal("open", cut.Find("[data-slot='menubar-trigger']").GetAttribute("data-state"));
        var positioned = Assert.Single(JSInterop.Invocations, invocation => invocation.Identifier.EndsWith("attachPositioned", StringComparison.Ordinal));
        Assert.Equal(-4d, positioned.Arguments[5]);
        var contentSource = File.ReadAllText(Path.Combine(FindRoot(), "src", "Maliev.ShadcnBlazor", "Components", "Overlays", "ShadcnMenubarContent.razor"));
        Assert.Contains("Context.Owner.TriggerId,_ownerRef", contentSource, StringComparison.Ordinal);
        Assert.NotNull(typeof(ShadcnMenubarMenu).GetMethod(nameof(ShadcnMenubarMenu.RequestCloseAsync))?.GetCustomAttributes(typeof(JSInvokableAttribute), false).SingleOrDefault());

        cut.Render(parameters => parameters
            .Add(component => component.Label, "Workspace menu")
            .Add(component => component.Loop, true)
            .Add(component => component.ChildContent, content));

        Assert.Equal("true", cut.Find("[data-slot='menubar']").GetAttribute("data-loop"));
        var attachCalls = JSInterop.Invocations.Where(invocation => invocation.Identifier.EndsWith("attachMenubar", StringComparison.Ordinal)).ToArray();
        Assert.True(attachCalls.Length >= 2);
        Assert.Equal(true, attachCalls[^1].Arguments[1]);
    }

    [Fact]
    public void MenubarPointerContractDeduplicatesHoverSwitchesAndHonorsNonLoopingNavigation()
    {
        var script = File.ReadAllText(Path.Combine(FindRoot(), "src", "Maliev.ShadcnBlazor", "wwwroot", "js", "shadcn-overlays-menus.js"));

        Assert.Contains("event.pointerType === 'touch'", script, StringComparison.Ordinal);
        Assert.Contains("target.contains(event.relatedTarget)", script, StringComparison.Ordinal);
        Assert.Contains("state.hoveredTrigger === target", script, StringComparison.Ordinal);
        Assert.Contains("state.loop ?", script, StringComparison.Ordinal);
    }

    [Fact]
    public void MenubarSelectionItemsRemainInteractiveWithoutExternalStateBinding()
    {
        var cut = Render<ShadcnMenubar>(parameters => parameters.Add(component => component.Label, "View menu").AddChildContent(builder =>
        {
            builder.OpenComponent<ShadcnMenubarMenu>(0);
            builder.AddAttribute(1, nameof(ShadcnMenubarMenu.Open), true);
            builder.AddAttribute(2, nameof(ShadcnMenubarMenu.ChildContent), (RenderFragment)(menu =>
            {
                menu.OpenComponent<ShadcnMenubarTrigger>(0); menu.AddAttribute(1, nameof(ShadcnMenubarTrigger.ChildContent), (RenderFragment)(text => text.AddContent(0, "View"))); menu.CloseComponent();
                menu.OpenComponent<ShadcnMenubarContent>(2); menu.AddAttribute(3, nameof(ShadcnMenubarContent.ChildContent), (RenderFragment)(content =>
                {
                    content.OpenComponent<ShadcnMenubarCheckboxItem>(0); content.AddAttribute(1, nameof(ShadcnMenubarCheckboxItem.Checked), true); content.AddAttribute(2, nameof(ShadcnMenubarCheckboxItem.ChildContent), (RenderFragment)(text => text.AddContent(0, "Status bar"))); content.CloseComponent();
                    content.OpenComponent<ShadcnMenubarRadioGroup>(3); content.AddAttribute(4, nameof(ShadcnMenubarRadioGroup.Value), "comfortable"); content.AddAttribute(5, nameof(ShadcnMenubarRadioGroup.ChildContent), (RenderFragment)(group =>
                    {
                        group.OpenComponent<ShadcnMenubarRadioItem>(0); group.AddAttribute(1, nameof(ShadcnMenubarRadioItem.Value), "comfortable"); group.AddAttribute(2, nameof(ShadcnMenubarRadioItem.ChildContent), (RenderFragment)(text => text.AddContent(0, "Comfortable"))); group.CloseComponent();
                        group.OpenComponent<ShadcnMenubarRadioItem>(3); group.AddAttribute(4, nameof(ShadcnMenubarRadioItem.Value), "compact"); group.AddAttribute(5, nameof(ShadcnMenubarRadioItem.ChildContent), (RenderFragment)(text => text.AddContent(0, "Compact"))); group.CloseComponent();
                    })); content.CloseComponent();
                })); menu.CloseComponent();
            }));
            builder.CloseComponent();
        }));

        var checkbox = cut.Find("[data-slot='menubar-checkbox-item']");
        checkbox.Click();
        Assert.Equal("false", cut.Find("[data-slot='menubar-checkbox-item']").GetAttribute("aria-checked"));

        cut.FindAll("[data-slot='menubar-radio-item']")[1].Click();
        var radios = cut.FindAll("[data-slot='menubar-radio-item']");
        Assert.Equal("false", radios[0].GetAttribute("aria-checked"));
        Assert.Equal("true", radios[1].GetAttribute("aria-checked"));
    }

    [Fact]
    public void CommandFiltersNormalizedThaiKeywordsAndSelectsEnabledItem()
    {
        var selected = new List<string>();
        var cut = Render<ShadcnCommand>(p => p.Add(x => x.Label, "คำสั่งด่วน").AddChildContent(builder =>
        {
            builder.OpenComponent<ShadcnCommandInput>(0); builder.AddAttribute(1, nameof(ShadcnCommandInput.Placeholder), "ค้นหาคำสั่ง..."); builder.CloseComponent();
            builder.OpenComponent<ShadcnCommandList>(2); builder.AddAttribute(3, nameof(ShadcnCommandList.ChildContent), (RenderFragment)(list =>
            {
                list.OpenComponent<ShadcnCommandEmpty>(0); list.AddAttribute(1, nameof(ShadcnCommandEmpty.ChildContent), (RenderFragment)(text => text.AddContent(0, "ไม่พบผลลัพธ์"))); list.CloseComponent();
                list.OpenComponent<ShadcnCommandGroup>(2); list.AddAttribute(3, nameof(ShadcnCommandGroup.Heading), "การนำทาง"); list.AddAttribute(4, nameof(ShadcnCommandGroup.ChildContent), (RenderFragment)(group =>
                {
                    AddCommandItem(group, 0, "orders", "ใบสั่งซื้อ", ["งาน", "ผลิต"], selected);
                    AddCommandItem(group, 10, "customers", "ลูกค้า", ["crm"], selected);
                    group.OpenComponent<ShadcnCommandItem>(20); group.AddAttribute(21, nameof(ShadcnCommandItem.Value), "disabled"); group.AddAttribute(22, nameof(ShadcnCommandItem.Disabled), true); group.AddAttribute(23, nameof(ShadcnCommandItem.ChildContent), (RenderFragment)(text => text.AddContent(0, "ปิดใช้งาน"))); group.CloseComponent();
                })); list.CloseComponent();
            })); builder.CloseComponent();
        }));

        Assert.Equal("combobox", cut.Find("[data-slot='command-input']").GetAttribute("role"));
        Assert.Equal("listbox", cut.Find("[data-slot='command-list']").GetAttribute("role"));
        cut.Find("[data-slot='command-input']").Input("ผลิต");
        var visible = cut.FindAll("[data-slot='command-item']").Where(x => !x.HasAttribute("hidden")).ToArray();
        Assert.Single(visible);
        Assert.Contains("ใบสั่งซื้อ", visible[0].TextContent, StringComparison.Ordinal);
        visible[0].Click();
        Assert.Equal(["orders"], selected);
        cut.Find("[data-slot='command-input']").Input("nothing");
        Assert.False(cut.Find("[data-slot='command-empty']").HasAttribute("hidden"));
    }

    [Fact]
    public void CommandRejectsDuplicateOrEmptyValues()
    {
        Assert.ThrowsAny<Exception>(() => Render<ShadcnCommandItem>(p => p.Add(x => x.Value, " ")));
    }

    private static void AddMenu(Microsoft.AspNetCore.Components.Rendering.RenderTreeBuilder builder, int sequence, string trigger, string item)
    {
        builder.OpenComponent<ShadcnMenubarMenu>(sequence); builder.AddAttribute(sequence + 1, nameof(ShadcnMenubarMenu.ChildContent), (RenderFragment)(menu =>
        {
            menu.OpenComponent<ShadcnMenubarTrigger>(0); menu.AddAttribute(1, nameof(ShadcnMenubarTrigger.ChildContent), (RenderFragment)(text => text.AddContent(0, trigger))); menu.CloseComponent();
            menu.OpenComponent<ShadcnMenubarContent>(2); menu.AddAttribute(3, nameof(ShadcnMenubarContent.ChildContent), (RenderFragment)(content => { content.OpenComponent<ShadcnMenubarItem>(0); content.AddAttribute(1, nameof(ShadcnMenubarItem.ChildContent), (RenderFragment)(text => text.AddContent(0, item))); content.CloseComponent(); })); menu.CloseComponent();
        })); builder.CloseComponent();
    }

    private static void AddCommandItem(Microsoft.AspNetCore.Components.Rendering.RenderTreeBuilder builder, int sequence, string value, string textValue, IReadOnlyList<string> keywords, List<string> selected)
    {
        builder.OpenComponent<ShadcnCommandItem>(sequence); builder.AddAttribute(sequence + 1, nameof(ShadcnCommandItem.Value), value); builder.AddAttribute(sequence + 2, nameof(ShadcnCommandItem.Keywords), keywords); builder.AddAttribute(sequence + 3, nameof(ShadcnCommandItem.OnSelect), EventCallback.Factory.Create<string>(selected, selected.Add)); builder.AddAttribute(sequence + 4, nameof(ShadcnCommandItem.ChildContent), (RenderFragment)(text => text.AddContent(0, textValue))); builder.CloseComponent();
    }

    private static string FindRoot()
    {
        var directory = new DirectoryInfo(AppContext.BaseDirectory);
        while (directory is not null && !File.Exists(Path.Combine(directory.FullName, "Maliev.ShadcnBlazor.slnx"))) directory = directory.Parent;
        return directory?.FullName ?? throw new DirectoryNotFoundException();
    }
}
