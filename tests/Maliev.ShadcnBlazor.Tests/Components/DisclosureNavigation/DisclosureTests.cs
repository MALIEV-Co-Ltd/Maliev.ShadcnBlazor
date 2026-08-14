using Bunit;
using Maliev.ShadcnBlazor.Components.Disclosure;
using Maliev.ShadcnBlazor.Components.Direction;
using Maliev.ShadcnBlazor.Theming;
using Microsoft.AspNetCore.Components;

namespace Maliev.ShadcnBlazor.Tests.Components.DisclosureNavigation;

public sealed class DisclosureTests : BunitContext
{
    public DisclosureTests()
    {
        var module = JSInterop.SetupModule("./_content/Maliev.ShadcnBlazor/js/shadcn-disclosure-navigation.js");
        module.SetupVoid("focusById", _ => true);
        module.SetupVoid("attachKeyGuard", _ => true);
        module.SetupVoid("detachKeyGuard", _ => true);
    }

    [Fact]
    public void AccordionRendersStableAccessibleRelationshipsAndPreservesClosedContent()
    {
        var cut = RenderAccordion(values: ["shipping"]);
        var triggers = cut.FindAll("button[data-slot='accordion-trigger']");
        var contents = cut.FindAll("[data-slot='accordion-content']");

        Assert.Equal("true", triggers[0].GetAttribute("aria-expanded"));
        Assert.Equal(contents[0].Id, triggers[0].GetAttribute("aria-controls"));
        Assert.Equal(triggers[0].Id, contents[0].GetAttribute("aria-labelledby"));
        Assert.False(contents[0].HasAttribute("hidden"));
        Assert.True(contents[1].HasAttribute("hidden"));
        Assert.Contains("รายละเอียดที่ยังอยู่ใน DOM", contents[1].TextContent, StringComparison.Ordinal);
    }

    [Fact]
    public void AccordionSingleAndMultipleModesRequestCorrectControlledValues()
    {
        IReadOnlyCollection<string>? requested = null;
        var single = RenderAccordion(values: ["shipping"], changed: values => requested = values);
        single.FindAll("button")[1].Click();
        Assert.Equal(["billing"], requested);
        Assert.Equal("false", single.FindAll("button")[1].GetAttribute("aria-expanded"));

        var multiple = RenderAccordion(values: ["shipping"], multiple: true, changed: values => requested = values);
        multiple.FindAll("button")[1].Click();
        Assert.Equal(["shipping", "billing"], requested);
    }

    [Fact]
    public void AccordionHonorsCollapsibleAndSuppressesDisabledCallbacks()
    {
        var calls = 0;
        var fixedOpen = RenderAccordion(values: ["shipping"], collapsible: false, changed: _ => calls++);
        fixedOpen.Find("button").Click();
        Assert.Equal(0, calls);

        var disabled = RenderAccordion(values: [], disabled: true, changed: _ => calls++);
        Assert.True(disabled.Find("button").HasAttribute("disabled"));
        disabled.Find("button").Click();
        Assert.Equal(0, calls);
    }

    [Fact]
    public void AccordionRejectsInvalidOrientationAndDuplicateValues()
    {
        Assert.ThrowsAny<Exception>(() => Render<ShadcnAccordion>(p => p.Add(x => x.Orientation, (ShadcnAccordionOrientation)999)));
        Assert.ThrowsAny<Exception>(() => Render<ShadcnAccordion>(p => p.AddChildContent(builder =>
        {
            AddAccordionItem(builder, 0, "same", "One", "One");
            AddAccordionItem(builder, 10, "same", "Two", "Two");
        })));
    }

    [Fact]
    public void HorizontalAccordionUsesRtlAwareRovingFocusAndKeyGuard()
    {
        var cut = Render<ShadcnDirectionProvider>(p => p
            .Add(x => x.Direction, ShadcnDirection.RightToLeft)
            .AddChildContent(builder =>
            {
                builder.OpenComponent<ShadcnAccordion>(0);
                builder.AddAttribute(1, nameof(ShadcnAccordion.Orientation), ShadcnAccordionOrientation.Horizontal);
                builder.AddAttribute(2, nameof(ShadcnAccordion.ChildContent), (RenderFragment)(items =>
                {
                    AddAccordionItem(items, 0, "first", "หนึ่ง", "First");
                    AddAccordionItem(items, 10, "second", "สอง", "Second");
                }));
                builder.CloseComponent();
            }));

        var accordion = cut.Find("[data-slot='accordion']");
        Assert.Equal("horizontal", accordion.GetAttribute("data-orientation"));
        Assert.Equal("rtl", accordion.GetAttribute("dir"));
        cut.FindAll("button")[0].KeyDown(new Microsoft.AspNetCore.Components.Web.KeyboardEventArgs { Key = "ArrowRight" });
        var focus = JSInterop.Invocations.Last(invocation => invocation.Identifier == "focusById");
        Assert.Equal(cut.FindAll("button")[1].Id, focus.Arguments[0]);

        var css = File.ReadAllText(Path.Combine(FindRoot(), "src", "Maliev.ShadcnBlazor", "wwwroot", "css", "shadcn-disclosure-navigation.css"));
        Assert.Contains(".shadcn-accordion[data-orientation=\"horizontal\"]", css, StringComparison.Ordinal);
    }

    [Fact]
    public void CollapsibleUsesControlledStateStableAriaAndDisabledSuppression()
    {
        var calls = new List<bool>();
        var cut = Render<ShadcnCollapsible>(p => p
            .Add(x => x.Open, true)
            .Add(x => x.OpenChanged, value => calls.Add(value))
            .AddChildContent(builder =>
            {
                builder.OpenComponent<ShadcnCollapsibleTrigger>(0);
                builder.AddAttribute(1, nameof(ShadcnCollapsibleTrigger.ChildContent), (RenderFragment)(content => content.AddContent(0, "เปิดรายละเอียด")));
                builder.CloseComponent();
                builder.OpenComponent<ShadcnCollapsibleContent>(2);
                builder.AddAttribute(3, nameof(ShadcnCollapsibleContent.ChildContent), (RenderFragment)(content => content.AddContent(0, "รายละเอียด")));
                builder.CloseComponent();
            }));

        var trigger = cut.Find("button");
        var content = cut.Find("[data-slot='collapsible-content']");
        Assert.Equal(content.Id, trigger.GetAttribute("aria-controls"));
        Assert.Equal(trigger.Id, content.GetAttribute("aria-labelledby"));
        trigger.Click();
        Assert.Equal([false], calls);
        Assert.False(content.HasAttribute("hidden"));

        cut.Render(p => p.Add(x => x.Open, true).Add(x => x.Disabled, true).Add(x => x.OpenChanged, value => calls.Add(value)).AddChildContent(builder =>
        {
            builder.OpenComponent<ShadcnCollapsibleTrigger>(0);
            builder.CloseComponent();
            builder.OpenComponent<ShadcnCollapsibleContent>(1);
            builder.CloseComponent();
        }));
        cut.Find("button").Click();
        Assert.Single(calls);
    }

    private IRenderedComponent<ShadcnAccordion> RenderAccordion(
        IReadOnlyCollection<string> values,
        bool multiple = false,
        bool collapsible = true,
        bool disabled = false,
        Action<IReadOnlyCollection<string>>? changed = null) => Render<ShadcnAccordion>(p => p
            .Add(x => x.Values, values)
            .Add(x => x.Multiple, multiple)
            .Add(x => x.Collapsible, collapsible)
            .Add(x => x.Disabled, disabled)
            .Add(x => x.ValuesChanged, changed is null ? default : EventCallback.Factory.Create(this, changed))
            .Add(x => x.Label, "หัวข้อช่วยเหลือ")
            .AddChildContent(builder =>
            {
                AddAccordionItem(builder, 0, "shipping", "การจัดส่ง", "รายละเอียดการจัดส่ง");
                AddAccordionItem(builder, 10, "billing", "การชำระเงิน", "รายละเอียดที่ยังอยู่ใน DOM");
            }));

    private static void AddAccordionItem(Microsoft.AspNetCore.Components.Rendering.RenderTreeBuilder builder, int sequence, string value, string title, string content)
    {
        builder.OpenComponent<ShadcnAccordionItem>(sequence);
        builder.AddAttribute(sequence + 1, nameof(ShadcnAccordionItem.Value), value);
        builder.AddAttribute(sequence + 2, nameof(ShadcnAccordionItem.ChildContent), (RenderFragment)(item =>
        {
            item.OpenComponent<ShadcnAccordionTrigger>(0);
            item.AddAttribute(1, nameof(ShadcnAccordionTrigger.ChildContent), (RenderFragment)(trigger => trigger.AddContent(0, title)));
            item.CloseComponent();
            item.OpenComponent<ShadcnAccordionContent>(2);
            item.AddAttribute(3, nameof(ShadcnAccordionContent.ChildContent), (RenderFragment)(body => body.AddContent(0, content)));
            item.CloseComponent();
        }));
        builder.CloseComponent();
    }

    private static string FindRoot()
    {
        var directory = new DirectoryInfo(AppContext.BaseDirectory);
        while (directory is not null && !File.Exists(Path.Combine(directory.FullName, "Maliev.ShadcnBlazor.slnx"))) directory = directory.Parent;
        return directory?.FullName ?? throw new DirectoryNotFoundException();
    }
}
