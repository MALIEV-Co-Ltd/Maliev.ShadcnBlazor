using Maliev.ShadcnBlazor.Components.Overlays;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Rendering;

namespace Maliev.ShadcnBlazor.Showcase.Documentation.Examples;

internal static class OverlayMenuExamples
{
    public static IReadOnlyList<ComponentExampleDefinition> Create(string slug) => slug switch
    {
        "dialog" => [Dialog(false)],
        "alert-dialog" => [Dialog(true)],
        "sheet" => [Sheet()],
        "drawer" => [Drawer()],
        "popover" => [Popover()],
        "hover-card" => [HoverCard()],
        "tooltip" => [Tooltip()],
        "dropdown-menu" => [Menu(false)],
        "context-menu" => [Menu(true)],
        "menubar" => [Menubar()],
        "command" => [Command()],
        _ => []
    };

    private static ComponentExampleDefinition Dialog(bool alert)
    {
        var open = false; var compact = false;
        RenderFragment preview = b =>
        {
            if (alert)
            {
                b.OpenComponent<ShadcnAlertDialog>(0); b.AddAttribute(1, "Open", open); b.AddAttribute(2, "ChildContent", (RenderFragment)(c =>
                {
                    AddText<ShadcnAlertDialogTrigger>(c, 0, "Delete quotation");
                    c.OpenComponent<ShadcnAlertDialogContent>(10); c.AddAttribute(11, "Size", compact ? ShadcnAlertDialogSize.Small : ShadcnAlertDialogSize.Default); c.AddAttribute(12, "ChildContent", (RenderFragment)(x =>
                    {
                        x.OpenComponent<ShadcnAlertDialogHeader>(0); x.AddAttribute(1, "ChildContent", (RenderFragment)(header => { AddText<ShadcnAlertDialogTitle>(header, 0, "Delete quotation?"); AddText<ShadcnAlertDialogDescription>(header, 10, "This action cannot be undone. The saved quotation will be permanently removed."); })); x.CloseComponent();
                        x.OpenComponent<ShadcnAlertDialogFooter>(20); x.AddAttribute(21, "ChildContent", (RenderFragment)(footer => { AddText<ShadcnAlertDialogCancel>(footer, 0, "Cancel"); AddText<ShadcnAlertDialogAction>(footer, 10, "Delete quotation"); })); x.CloseComponent();
                    })); c.CloseComponent();
                })); b.CloseComponent();
            }
            else
            {
                b.OpenComponent<ShadcnDialog>(0); b.AddAttribute(1, "Open", open); b.AddAttribute(2, "Modal", !compact); b.AddAttribute(3, "ChildContent", (RenderFragment)(c =>
                {
                    AddText<ShadcnDialogTrigger>(c, 0, "Edit profile");
                    c.OpenComponent<ShadcnDialogContent>(10); c.AddAttribute(11, "ShowCloseButton", false); c.AddAttribute(12, "ChildContent", (RenderFragment)(x =>
                    {
                        x.OpenComponent<ShadcnDialogHeader>(0); x.AddAttribute(1, "ChildContent", (RenderFragment)(header => { AddText<ShadcnDialogTitle>(header, 0, "Edit profile"); AddText<ShadcnDialogDescription>(header, 10, "Update the customer details, then save when you are done."); })); x.CloseComponent();
                        x.OpenElement(20, "div"); x.AddAttribute(21, "class", "shadcn-overlay-form-preview"); x.AddContent(22, "Name · Pedro Duarte"); x.AddContent(23, "Username · @peduarte"); x.CloseElement();
                        x.OpenComponent<ShadcnDialogFooter>(30); x.AddAttribute(31, "ChildContent", (RenderFragment)(footer => AddText<ShadcnDialogClose>(footer, 0, "Save changes"))); x.CloseComponent();
                    })); c.CloseComponent();
                })); b.CloseComponent();
            }
        };
        var slug = alert ? "alert-dialog" : "dialog";
        return Example(slug, alert ? "Confirmation dialog" : "Modal and non-modal dialog", preview,
            [Toggle($"{slug}-open", "Open", v => open = v), Toggle($"{slug}-variant", alert ? "Small layout" : "Non-modal", v => compact = v)],
            alert ? ["open", "cancel-focus", "destructive", "small", "localized"] : ["open", "modal", "non-modal", "focus-trap", "restore-focus"]);
    }

    private static ComponentExampleDefinition Sheet()
    {
        var open = false; var left = false;
        RenderFragment preview = b => { b.OpenComponent<ShadcnSheet>(0); b.AddAttribute(1, "Open", open); b.AddAttribute(2, "ChildContent", (RenderFragment)(c => { AddText<ShadcnSheetTrigger>(c, 0, "Open settings"); c.OpenComponent<ShadcnSheetContent>(10); c.AddAttribute(11, "Side", left ? ShadcnSheetSide.Left : ShadcnSheetSide.Right); c.AddAttribute(12, "ShowCloseButton", false); c.AddAttribute(13, "CloseLabel", "Close settings"); c.AddAttribute(14, "ChildContent", (RenderFragment)(x => { x.OpenComponent<ShadcnSheetHeader>(0); x.AddAttribute(1, "ChildContent", (RenderFragment)(header => { AddText<ShadcnSheetTitle>(header, 0, "Workspace settings"); AddText<ShadcnSheetDescription>(header, 10, "Manage notifications and quotation defaults."); })); x.CloseComponent(); x.OpenElement(20, "div"); x.AddAttribute(21, "class", "shadcn-overlay-form-preview"); x.AddContent(22, "Email notifications · Enabled"); x.AddContent(23, "Default currency · THB"); x.CloseElement(); x.OpenComponent<ShadcnSheetFooter>(30); x.AddAttribute(31, "ChildContent", (RenderFragment)(footer => AddText<ShadcnSheetClose>(footer, 0, "Done"))); x.CloseComponent(); })); c.CloseComponent(); })); b.CloseComponent(); };
        return Example("sheet", "Edge sheet", preview, [Toggle("sheet-open", "Open", v => open = v), Toggle("sheet-left", "Left side", v => left = v)], ["top", "right", "bottom", "left", "modal", "localized-close"]);
    }

    private static ComponentExampleDefinition Drawer()
    {
        var open = false; var nonmodal = false; var up = false;
        RenderFragment preview = b => { b.OpenComponent<ShadcnDrawer>(0); b.AddAttribute(1, "Open", open); b.AddAttribute(2, "ModalMode", nonmodal ? ShadcnDrawerModalMode.NonModal : ShadcnDrawerModalMode.Modal); b.AddAttribute(3, "SwipeDirection", up ? ShadcnDrawerSwipeDirection.Up : ShadcnDrawerSwipeDirection.Down); b.AddAttribute(4, "ShowSwipeHandle", true); b.AddAttribute(5, "SnapPoints", new[] { ShadcnDrawerSnapPoint.Fraction(.4), ShadcnDrawerSnapPoint.Fraction(1) }); b.AddAttribute(6, "ChildContent", (RenderFragment)(c => { AddText<ShadcnDrawerTrigger>(c, 0, "Open drawer"); c.OpenComponent<ShadcnDrawerContent>(10); c.AddAttribute(11, "ChildContent", (RenderFragment)(x => { x.OpenComponent<ShadcnDrawerHeader>(0); x.AddAttribute(1, "ChildContent", (RenderFragment)(header => { AddText<ShadcnDrawerTitle>(header, 0, "Order #4189"); AddText<ShadcnDrawerDescription>(header, 10, "Review delivery details before confirming."); })); x.CloseComponent(); x.OpenElement(20, "div"); x.AddAttribute(21, "class", "shadcn-overlay-form-preview"); x.AddContent(22, "Status · Ready to ship"); x.AddContent(23, "Delivery · Friday, 4:30 PM"); x.CloseElement(); x.OpenComponent<ShadcnDrawerFooter>(30); x.AddAttribute(31, "ChildContent", (RenderFragment)(footer => AddText<ShadcnDrawerClose>(footer, 0, "Confirm order"))); x.CloseComponent(); })); c.CloseComponent(); })); b.CloseComponent(); };
        return Example("drawer", "Gesture drawer", preview, [Toggle("drawer-open", "Open", v => open = v), Toggle("drawer-up", "Open from top", v => up = v), Toggle("drawer-nonmodal", "Non-modal", v => nonmodal = v)], ["swipe", "snap-points", "modal", "non-modal", "trap-focus", "safe-area"]);
    }

    private static ComponentExampleDefinition Popover()
    {
        var open = false; var top = false;
        RenderFragment preview = b => { b.OpenComponent<ShadcnPopover>(0); b.AddAttribute(1, "Open", open); b.AddAttribute(2, "ChildContent", (RenderFragment)(c => { AddText<ShadcnPopoverTrigger>(c, 0, "Open popover"); c.OpenComponent<ShadcnPopoverContent>(10); c.AddAttribute(11, "Side", top ? ShadcnOverlaySide.Top : ShadcnOverlaySide.Bottom); c.AddAttribute(12, "ChildContent", (RenderFragment)(x => { AddText<ShadcnPopoverTitle>(x, 0, "Dimensions"); AddText<ShadcnPopoverDescription>(x, 10, "Set part dimensions."); })); c.CloseComponent(); })); b.CloseComponent(); };
        return Example("popover", "Collision-aware popover", preview, [Toggle("popover-open", "Open", v => open = v), Toggle("popover-top", "Top placement", v => top = v)], ["controlled", "placement", "collision", "outside-press", "restore-focus"]);
    }

    private static ComponentExampleDefinition HoverCard()
    {
        var open = false; var fast = false;
        RenderFragment preview = b => { b.OpenComponent<ShadcnHoverCard>(0); b.AddAttribute(1, "Open", open); b.AddAttribute(2, "OpenDelay", TimeSpan.FromMilliseconds(fast ? 0 : 700)); b.AddAttribute(3, "ChildContent", (RenderFragment)(c => { AddText<ShadcnHoverCardTrigger>(c, 0, "@maliev"); AddText<ShadcnHoverCardContent>(c, 10, "Thai digital manufacturing platform."); })); b.CloseComponent(); };
        return Example("hover-card", "Delayed hover card", preview, [Toggle("hover-card-open", "Open", v => open = v), Toggle("hover-card-fast", "No open delay", v => fast = v)], ["hover", "focus", "delays", "pointer-bridge", "collision"]);
    }

    private static ComponentExampleDefinition Tooltip()
    {
        var bottom = false;
        RenderFragment preview = b =>
        {
            b.OpenElement(0, "div");
            b.AddAttribute(1, "class", "showcase-tooltip-preview");
            b.AddAttribute(2, "style", "display:grid; place-items:center; min-block-size:8rem; padding:2rem;");
            b.OpenComponent<ShadcnTooltipProvider>(3);
            b.AddAttribute(4, "OpenDelay", TimeSpan.FromMilliseconds(200));
            b.AddAttribute(5, "ChildContent", (RenderFragment)(p =>
            {
                p.OpenComponent<ShadcnTooltip>(0);
                p.AddAttribute(1, "ChildContent", (RenderFragment)(c =>
                {
                    AddText<ShadcnTooltipTrigger>(c, 0, "Save");
                    c.OpenComponent<ShadcnTooltipContent>(10);
                    c.AddAttribute(11, "Side", bottom ? ShadcnOverlaySide.Bottom : ShadcnOverlaySide.Top);
                    c.AddAttribute(12, "ChildContent", Text("Save quotation"));
                    c.CloseComponent();
                }));
                p.CloseComponent();
            }));
            b.CloseComponent();
            b.CloseElement();
        };
        var side = bottom ? "Bottom" : "Top";
        var source = $"<ShadcnTooltipProvider OpenDelay=\"@(TimeSpan.FromMilliseconds(200))\">{Environment.NewLine}" +
            $"    <ShadcnTooltip>{Environment.NewLine}" +
            $"        <ShadcnTooltipTrigger>Save</ShadcnTooltipTrigger>{Environment.NewLine}" +
            $"        <ShadcnTooltipContent Side=\"{side}\">Save quotation</ShadcnTooltipContent>{Environment.NewLine}" +
            $"    </ShadcnTooltip>{Environment.NewLine}</ShadcnTooltipProvider>";
        return Example("tooltip", "Hover and focus tooltip", preview, [Toggle("tooltip-bottom", "Bottom placement", v => bottom = v)], ["hover", "focus", "provider-delay", "arrow", "noninteractive"], source);
    }

    private static ComponentExampleDefinition Menu(bool context)
    {
        var open = false; var checkedValue = true;
        RenderFragment preview = b =>
        {
            if (context) { b.OpenComponent<ShadcnContextMenu>(0); b.AddAttribute(1, "Open", open); b.AddAttribute(2, "ChildContent", (RenderFragment)(c => { AddText<ShadcnContextMenuTrigger>(c, 0, "Right-click this surface"); c.OpenComponent<ShadcnContextMenuContent>(10); c.AddAttribute(11, "ChildContent", (RenderFragment)(x => { AddText<ShadcnContextMenuItem>(x, 0, "New quotation"); AddChecked<ShadcnContextMenuCheckboxItem>(x, 10, checkedValue, "Show archived"); })); c.CloseComponent(); })); b.CloseComponent(); }
            else { b.OpenComponent<ShadcnDropdownMenu>(0); b.AddAttribute(1, "Open", open); b.AddAttribute(2, "ChildContent", (RenderFragment)(c => { AddText<ShadcnDropdownMenuTrigger>(c, 0, "Actions"); c.OpenComponent<ShadcnDropdownMenuContent>(10); c.AddAttribute(11, "ChildContent", (RenderFragment)(x => { AddText<ShadcnDropdownMenuItem>(x, 0, "Duplicate"); AddChecked<ShadcnDropdownMenuCheckboxItem>(x, 10, checkedValue, "Show details"); })); c.CloseComponent(); })); b.CloseComponent(); }
        };
        var slug = context ? "context-menu" : "dropdown-menu";
        return Example(slug, context ? "Pointer and keyboard context menu" : "Dropdown menu states", preview, [Toggle($"{slug}-open", "Open", v => open = v), Toggle($"{slug}-checked", "Checked item", v => checkedValue = v, true)], ["keyboard", "typeahead", "checkbox", "radio", "submenu", "rtl", context ? "Shift+F10" : "trigger"]);
    }

    private static ComponentExampleDefinition Menubar()
    {
        var second = false;
        RenderFragment preview = b => { b.OpenComponent<ShadcnMenubar>(0); b.AddAttribute(1, "Label", "Application menu"); b.AddAttribute(2, "ChildContent", (RenderFragment)(c => { AddMenubarMenu(c, 0, "File", "New quotation", false); AddMenubarMenu(c, 10, "Edit", "Undo", second); })); b.CloseComponent(); };
        return Example("menubar", "Application menubar", preview, [Toggle("menubar-second", "Open Edit menu", v => second = v)], ["roving-focus", "open-switching", "submenu", "checkbox", "radio", "rtl"]);
    }

    private static ComponentExampleDefinition Command()
    {
        var empty = false; var disabled = false;
        RenderFragment preview = b => { b.OpenComponent<ShadcnCommand>(0); b.AddAttribute(1, "Label", "Quick commands"); b.AddAttribute(2, "ChildContent", (RenderFragment)(c => { c.OpenComponent<ShadcnCommandInput>(0); c.AddAttribute(1, "Placeholder", empty ? "No matching commands" : "Search commands..."); c.CloseComponent(); c.OpenComponent<ShadcnCommandList>(10); c.AddAttribute(11, "ChildContent", (RenderFragment)(x => { AddText<ShadcnCommandEmpty>(x, 0, "No results"); x.OpenComponent<ShadcnCommandGroup>(10); x.AddAttribute(11, "Heading", "Navigation"); x.AddAttribute(12, "ChildContent", (RenderFragment)(g => { AddCommandItem(g, 0, empty ? "hidden-orders" : "orders", "Orders", disabled); AddCommandItem(g, 10, empty ? "hidden-customers" : "customers", "Customers", false); })); x.CloseComponent(); })); c.CloseComponent(); })); b.CloseComponent(); };
        return Example("command", "Searchable command palette", preview, [Toggle("command-empty", "Alternate values", v => empty = v), Toggle("command-disabled", "Disable first item", v => disabled = v)], ["filtering", "Thai-keywords", "groups", "empty", "disabled", "keyboard", "dialog"]);
    }

    private static ComponentExampleDefinition Example(string slug, string title, RenderFragment preview, IReadOnlyList<ComponentParameterControl> controls, IReadOnlyList<string> tags, string? razorSource = null) =>
        new($"{slug}-primary", title, "Live package component with controlled state and the complete composition surface.", razorSource ?? $"<{Primary(slug)}>...</{Primary(slug)}>", preview, controls, tags);
    private static string Primary(string slug) => "Shadcn" + string.Concat(slug.Split('-').Select(part => char.ToUpperInvariant(part[0]) + part[1..]));
    private static ComponentParameterControl Toggle(string id, string label, Action<bool> apply, bool initial = false) => new(id, label, ComponentParameterControlKind.Toggle, initial.ToString(), [], value => apply(bool.Parse(value)));
    private static RenderFragment Text(string value) => b => b.AddContent(0, value);
    private static void AddText<T>(RenderTreeBuilder b, int sequence, string text) where T : IComponent { b.OpenComponent<T>(sequence); b.AddAttribute(sequence + 1, "ChildContent", Text(text)); b.CloseComponent(); }
    private static void AddChecked<T>(RenderTreeBuilder b, int sequence, bool value, string text) where T : IComponent { b.OpenComponent<T>(sequence); b.AddAttribute(sequence + 1, "Checked", value); b.AddAttribute(sequence + 2, "ChildContent", Text(text)); b.CloseComponent(); }
    private static void AddMenubarMenu(RenderTreeBuilder b, int sequence, string trigger, string item, bool open) { b.OpenComponent<ShadcnMenubarMenu>(sequence); b.AddAttribute(sequence + 1, "Open", open); b.AddAttribute(sequence + 2, "ChildContent", (RenderFragment)(c => { AddText<ShadcnMenubarTrigger>(c, 0, trigger); c.OpenComponent<ShadcnMenubarContent>(10); c.AddAttribute(11, "ChildContent", (RenderFragment)(x => AddText<ShadcnMenubarItem>(x, 0, item))); c.CloseComponent(); })); b.CloseComponent(); }
    private static void AddCommandItem(RenderTreeBuilder b, int sequence, string value, string text, bool disabled) { b.OpenComponent<ShadcnCommandItem>(sequence); b.AddAttribute(sequence + 1, "Value", value); b.AddAttribute(sequence + 2, "TextValue", text); b.AddAttribute(sequence + 3, "Disabled", disabled); b.AddAttribute(sequence + 4, "ChildContent", Text(text)); b.CloseComponent(); }
}
