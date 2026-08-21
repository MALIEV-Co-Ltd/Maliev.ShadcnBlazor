using Maliev.ShadcnBlazor.Components.Forms;
using Maliev.ShadcnBlazor.Components.Overlays;
using Maliev.ShadcnBlazor.Showcase.Components.Documentation;
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
        if (alert)
        {
            RenderFragment alertPreview = builder =>
            {
                builder.OpenComponent<AlertDialogDossierPreview>(0);
                builder.AddAttribute(1, nameof(AlertDialogDossierPreview.Compact), compact);
                builder.CloseComponent();
            };
            string AlertSource() => $$"""
                @using Maliev.ShadcnBlazor.Components.Overlays

                <ShadcnAlertDialog Open="_open" OpenChanged="SetOpenAsync">
                    <ShadcnAlertDialogTrigger>Delete saved quotation</ShadcnAlertDialogTrigger>
                    <ShadcnAlertDialogContent Size="ShadcnAlertDialogSize.{{(compact ? "Small" : "Default")}}">
                        <ShadcnAlertDialogMedia>
                            <svg viewBox="0 0 24 24" aria-hidden="true">
                                <path d="M12 3 2.8 19h18.4z" />
                                <path d="M12 9v4M12 17h.01" />
                            </svg>
                        </ShadcnAlertDialogMedia>
                        <ShadcnAlertDialogHeader>
                            <ShadcnAlertDialogTitle>Delete quotation?</ShadcnAlertDialogTitle>
                            <ShadcnAlertDialogDescription>
                                This permanently removes Q-2847 and its saved pricing notes. This action cannot be undone.
                            </ShadcnAlertDialogDescription>
                        </ShadcnAlertDialogHeader>
                        <ShadcnAlertDialogFooter>
                            <ShadcnAlertDialogCancel>Cancel</ShadcnAlertDialogCancel>
                            <ShadcnAlertDialogAction OnClick="DeleteAsync">Delete quotation</ShadcnAlertDialogAction>
                        </ShadcnAlertDialogFooter>
                    </ShadcnAlertDialogContent>
                </ShadcnAlertDialog>

                @code {
                    private bool _open;

                    private Task SetOpenAsync(bool value)
                    {
                        _open = value;
                        return Task.CompletedTask;
                    }

                    private Task DeleteAsync(MouseEventArgs _)
                    {
                        _open = false;
                        return Task.CompletedTask;
                    }
                }
                """;
            return Example("alert-dialog", "Delete a saved quotation", alertPreview,
                [Toggle("alert-dialog-variant", "Small layout", value => compact = value)],
                ["closed-by-default", "cancel-focus", "destructive", "focus-trap", "restore-focus", "rtl", "reduced-motion"], AlertSource())
                with
            { RazorSourceProvider = AlertSource };
        }
        RenderFragment preview = b =>
        {
            b.OpenComponent<DialogDossierPreview>(0);
            b.AddAttribute(1, nameof(DialogDossierPreview.Open), open);
            b.AddAttribute(2, nameof(DialogDossierPreview.Modal), !compact);
            b.CloseComponent();
        };

        string Source() => $$"""
<ShadcnDialog Open="{{open.ToString().ToLowerInvariant()}}" Modal="{{(!compact).ToString().ToLowerInvariant()}}">
    <ShadcnDialogTrigger>Edit profile</ShadcnDialogTrigger>
    <ShadcnDialogContent ShowCloseButton="true" CloseLabel="Close profile editor">
        <ShadcnDialogHeader>
            <ShadcnDialogTitle>Edit profile</ShadcnDialogTitle>
            <ShadcnDialogDescription>
                Keep the contact details your project team sees up to date.
            </ShadcnDialogDescription>
        </ShadcnDialogHeader>

        <ShadcnFieldGroup>
            <ShadcnField>
                <ShadcnFieldLabel For="dialog-profile-name">Name</ShadcnFieldLabel>
                <ShadcnInput TValue="string" id="dialog-profile-name" @bind-Value="_name" AutoComplete="name" />
            </ShadcnField>
            <ShadcnField>
                <ShadcnFieldLabel For="dialog-profile-username">Username</ShadcnFieldLabel>
                <ShadcnInput TValue="string" id="dialog-profile-username" @bind-Value="_username" AutoComplete="username" />
            </ShadcnField>
        </ShadcnFieldGroup>

        <ShadcnDialogFooter>
            <ShadcnDialogClose>Cancel</ShadcnDialogClose>
            <ShadcnDialogClose Variant="ShadcnButtonVariant.Default">Save changes</ShadcnDialogClose>
        </ShadcnDialogFooter>
    </ShadcnDialogContent>
</ShadcnDialog>

@code {
    private string _name = "Narin Chaiyasit";
    private string _username = "narin.c";
}
""";

        var example = new ComponentExampleDefinition(
            "dialog-primary",
            "Editable profile dialog",
            "Open a focused profile editor with labeled inputs, explicit actions, modal focus containment, and trigger restoration.",
            Source(),
            preview,
            [Toggle("dialog-open", "Open", value => open = value), Toggle("dialog-variant", "Non-modal", value => compact = value)],
            ["trigger", "editable-form", "modal", "non-modal", "focus-trap", "restore-focus", "escape", "rtl"]);
        return example with { RazorSourceProvider = Source };
    }

    private static ComponentExampleDefinition Sheet()
    {
        var open = false;
        var side = ShadcnSheetSide.Right;
        RenderFragment preview = builder =>
        {
            builder.OpenComponent<SheetDossierPreview>(0);
            builder.AddAttribute(1, nameof(SheetDossierPreview.Open), open);
            builder.AddAttribute(2, nameof(SheetDossierPreview.Side), side);
            builder.CloseComponent();
        };
        string Source() => $$"""
@using Maliev.ShadcnBlazor.Components.Actions
@using Maliev.ShadcnBlazor.Components.Forms
@using Maliev.ShadcnBlazor.Components.Overlays
@using Maliev.ShadcnBlazor.Components.Selection

<div class="showcase-sheet-dossier">
    <ShadcnSheet @bind-Open="open">
        <ShadcnSheetTrigger Class="showcase-sheet-dossier__trigger">Review delivery schedule</ShadcnSheetTrigger>
        <ShadcnSheetContent Side="ShadcnSheetSide.{{side}}" CloseLabel="Close delivery schedule">
        <ShadcnSheetHeader Class="showcase-sheet-dossier__header">
            <ShadcnSheetTitle>Delivery schedule</ShadcnSheetTitle>
            <ShadcnSheetDescription>
                Confirm the production contact and release notifications for quotation Q-4189.
            </ShadcnSheetDescription>
        </ShadcnSheetHeader>

        <div class="showcase-sheet-dossier__body">
            <div class="showcase-sheet-dossier__summary" aria-label="Quotation summary">
                <span>Quotation</span>
                <strong>Q-4189 · CNC enclosure</strong>
                <span>Target dispatch</span>
                <strong>Friday, 21 Aug · 16:30</strong>
            </div>

            <ShadcnField>
                <ShadcnFieldLabel For="sheet-contact">Production contact</ShadcnFieldLabel>
                <ShadcnInput TValue="string" id="sheet-contact" @bind-Value="contact" AutoComplete="name" />
                <ShadcnFieldDescription>Shown to the dispatch team.</ShadcnFieldDescription>
            </ShadcnField>

            <ShadcnField Orientation="ShadcnFieldOrientation.Horizontal">
                <ShadcnFieldContent>
                    <ShadcnFieldLabel For="sheet-notifications">Release notifications</ShadcnFieldLabel>
                    <ShadcnFieldDescription>Email the project team when dispatch is confirmed.</ShadcnFieldDescription>
                </ShadcnFieldContent>
                <ShadcnSwitch id="sheet-notifications" @bind-Value="notificationsEnabled" Name="release-notifications" />
            </ShadcnField>
        </div>

        <ShadcnSheetFooter Class="showcase-sheet-dossier__footer">
            <ShadcnSheetClose Class="showcase-sheet-dossier__cancel" Label="Cancel delivery schedule">Cancel</ShadcnSheetClose>
            <ShadcnButton OnClick="SaveAsync">Save schedule</ShadcnButton>
        </ShadcnSheetFooter>
        </ShadcnSheetContent>
    </ShadcnSheet>
    <p class="showcase-sheet-dossier__status" role="status" aria-live="polite" dir="auto">@status</p>
</div>

@code {
    private bool open;
    private string contact = "Narin S.";
    private bool notificationsEnabled = true;
    private string status = "Open the schedule to review delivery settings.";

    private Task SaveAsync()
    {
        status = $"Schedule saved for {(string.IsNullOrWhiteSpace(contact) ? "the production team" : contact)}. Notifications {(notificationsEnabled ? "enabled" : "disabled")}.";
        open = false;
        return Task.CompletedTask;
    }
}
""";
        return Example("sheet", "Delivery schedule sheet", preview,
            [Toggle("sheet-open", "Open", value => open = value), Select("sheet-side", "Edge", side, value => side = value)],
            ["top", "right", "bottom", "left", "modal", "focus-trap", "outside-press", "escape", "responsive"]) with
        { RazorSourceProvider = Source };
    }

    private static ComponentExampleDefinition Drawer()
    {
        var open = false; var nonmodal = false; var up = false;
        RenderFragment preview = b => { b.OpenComponent<ShadcnDrawer>(0); b.AddAttribute(1, "Open", open); b.AddAttribute(2, "ModalMode", nonmodal ? ShadcnDrawerModalMode.NonModal : ShadcnDrawerModalMode.Modal); b.AddAttribute(3, "SwipeDirection", up ? ShadcnDrawerSwipeDirection.Up : ShadcnDrawerSwipeDirection.Down); b.AddAttribute(4, "ShowSwipeHandle", true); b.AddAttribute(5, "SnapPoints", new[] { ShadcnDrawerSnapPoint.Fraction(.4), ShadcnDrawerSnapPoint.Fraction(1) }); b.AddAttribute(6, "ChildContent", (RenderFragment)(c => { AddText<ShadcnDrawerTrigger>(c, 0, "Open drawer"); c.OpenComponent<ShadcnDrawerContent>(10); c.AddAttribute(11, "ChildContent", (RenderFragment)(x => { x.OpenComponent<ShadcnDrawerHeader>(0); x.AddAttribute(1, "ChildContent", (RenderFragment)(header => { AddText<ShadcnDrawerTitle>(header, 0, "Order #4189"); AddText<ShadcnDrawerDescription>(header, 10, "Review delivery details before confirming."); })); x.CloseComponent(); x.OpenElement(20, "div"); x.AddAttribute(21, "class", "shadcn-overlay-form-preview"); x.AddContent(22, "Status · Ready to ship"); x.AddContent(23, "Delivery · Friday, 4:30 PM"); x.CloseElement(); x.OpenComponent<ShadcnDrawerFooter>(30); x.AddAttribute(31, "ChildContent", (RenderFragment)(footer => AddText<ShadcnDrawerClose>(footer, 0, "Confirm order"))); x.CloseComponent(); })); c.CloseComponent(); })); b.CloseComponent(); };
        return Example("drawer", "Gesture drawer", preview, [Toggle("drawer-open", "Open", v => open = v), Toggle("drawer-up", "Open from top", v => up = v), Toggle("drawer-nonmodal", "Non-modal", v => nonmodal = v)], ["swipe", "snap-points", "modal", "non-modal", "trap-focus", "safe-area"]);
    }

    private static ComponentExampleDefinition Popover()
    {
        var open = false;
        var top = false;
        var closeOnOutsidePress = true;
        decimal width = 120;
        decimal depth = 80;
        decimal height = 24;

        RenderFragment preview = builder =>
        {
            builder.OpenElement(0, "div");
            builder.AddAttribute(1, "class", "showcase-popover-dossier");
            builder.AddAttribute(2, "data-placement", top ? "top" : "bottom");
            builder.AddAttribute(3, "data-close-on-outside-press", closeOnOutsidePress.ToString().ToLowerInvariant());
            builder.OpenComponent<ShadcnPopover>(4);
            builder.AddAttribute(5, nameof(ShadcnPopover.Open), open);
            builder.AddAttribute(6, nameof(ShadcnPopover.OpenChanged), EventCallback.Factory.Create<bool>(new object(), next => open = next));
            builder.AddAttribute(7, nameof(ShadcnPopover.CloseOnOutsidePress), closeOnOutsidePress);
            builder.AddAttribute(8, nameof(ShadcnPopover.ChildContent), (RenderFragment)(popover =>
            {
                AddText<ShadcnPopoverTrigger>(popover, 0, "Edit part dimensions");
                popover.OpenComponent<ShadcnPopoverContent>(10);
                popover.AddAttribute(11, nameof(ShadcnPopoverContent.Side), top ? ShadcnOverlaySide.Top : ShadcnOverlaySide.Bottom);
                popover.AddAttribute(12, nameof(ShadcnPopoverContent.Align), ShadcnOverlayAlign.Start);
                popover.AddAttribute(13, nameof(ShadcnPopoverContent.ChildContent), (RenderFragment)(content =>
                {
                    content.OpenComponent<ShadcnPopoverHeader>(0);
                    content.AddAttribute(1, nameof(ShadcnPopoverHeader.ChildContent), (RenderFragment)(header =>
                    {
                        AddText<ShadcnPopoverTitle>(header, 0, "Part dimensions");
                        AddText<ShadcnPopoverDescription>(header, 10, "Set the finished size in millimetres.");
                    }));
                    content.CloseComponent();
                    content.OpenElement(10, "div");
                    content.AddAttribute(11, "class", "showcase-popover-form");
                    AddPopoverField(content, 20, "part-width", "Width", width, next => width = next);
                    AddPopoverField(content, 40, "part-depth", "Depth", depth, next => depth = next);
                    AddPopoverField(content, 60, "part-height", "Height", height, next => height = next);
                    content.CloseElement();
                }));
                popover.CloseComponent();
            }));
            builder.CloseComponent();
            builder.CloseElement();
        };

        string Source()
        {
            var side = top ? "Top" : "Bottom";
            var outside = closeOnOutsidePress.ToString().ToLowerInvariant();
            return $$"""
                @using Maliev.ShadcnBlazor.Components.Forms
                @using Maliev.ShadcnBlazor.Components.Overlays

                <div class="showcase-popover-dossier" data-placement="{{side.ToLowerInvariant()}}" data-close-on-outside-press="{{outside}}">
                    <ShadcnPopover @bind-Open="Open" CloseOnOutsidePress="{{outside}}">
                        <ShadcnPopoverTrigger>Edit part dimensions</ShadcnPopoverTrigger>
                        <ShadcnPopoverContent Side="ShadcnOverlaySide.{{side}}" Align="ShadcnOverlayAlign.Start">
                            <ShadcnPopoverHeader>
                                <ShadcnPopoverTitle>Part dimensions</ShadcnPopoverTitle>
                                <ShadcnPopoverDescription>Set the finished size in millimetres.</ShadcnPopoverDescription>
                            </ShadcnPopoverHeader>
                            <div class="showcase-popover-form">
                                <ShadcnLabel For="part-width">Width</ShadcnLabel>
                                <ShadcnInput TValue="decimal" @bind-Value="Width" Type="number" id="part-width" />
                                <ShadcnLabel For="part-depth">Depth</ShadcnLabel>
                                <ShadcnInput TValue="decimal" @bind-Value="Depth" Type="number" id="part-depth" />
                                <ShadcnLabel For="part-height">Height</ShadcnLabel>
                                <ShadcnInput TValue="decimal" @bind-Value="Height" Type="number" id="part-height" />
                            </div>
                        </ShadcnPopoverContent>
                    </ShadcnPopover>
                </div>

                @code {
                    private bool Open { get; set; }
                    private decimal Width { get; set; } = 120;
                    private decimal Depth { get; set; } = 80;
                    private decimal Height { get; set; } = 24;
                }
                """;
        }

        return Example(
            "popover",
            "Part dimensions popover",
            preview,
            [
                Toggle("popover-top", "Top placement", value => top = value),
                Toggle("popover-outside", "Close on outside press", value => closeOnOutsidePress = value, true)
            ],
            ["controlled", "placement", "collision", "outside-press", "restore-focus", "rtl"],
            Source()) with
        { RazorSourceProvider = Source };
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
        var loop = true;
        var showStatus = true;

        RenderFragment preview = b =>
        {
            b.OpenElement(0, "div");
            b.AddAttribute(1, "class", "showcase-menubar-dossier");
            b.AddAttribute(2, "data-testid", "menubar-dossier-preview");
            b.OpenElement(3, "section");
            b.AddAttribute(4, "class", "showcase-menubar-workspace");
            b.AddAttribute(5, "aria-label", "Quotation editor preview");
            b.OpenElement(6, "header");
            b.AddAttribute(7, "class", "showcase-menubar-workspace__header");
            b.OpenElement(8, "div");
            b.AddAttribute(9, "class", "showcase-menubar-workspace__identity");
            b.OpenElement(10, "span");
            b.AddAttribute(11, "class", "showcase-menubar-workspace__mark");
            b.AddAttribute(12, "aria-hidden", "true");
            b.AddContent(13, "M");
            b.CloseElement();
            b.OpenElement(14, "div");
            b.OpenElement(15, "strong");
            b.AddContent(16, "Quotation editor");
            b.CloseElement();
            b.OpenElement(17, "span");
            b.AddContent(18, "QT-4189");
            b.CloseElement();
            b.CloseElement();
            b.CloseElement();
            b.OpenElement(19, "span");
            b.AddAttribute(20, "class", "showcase-menubar-workspace__save-state");
            b.AddContent(21, "Saved just now");
            b.CloseElement();
            b.CloseElement();

            b.OpenComponent<ShadcnMenubar>(30);
            b.AddAttribute(31, "Label", "Quotation editor commands");
            b.AddAttribute(32, "Loop", loop);
            b.AddAttribute(33, "ChildContent", (RenderFragment)(c =>
            {
                AddMenubarFileMenu(c, 0);
                AddMenubarEditMenu(c, 100);
                AddMenubarViewMenu(c, 200, showStatus);
                AddMenubarHelpMenu(c, 300);
            }));
            b.CloseComponent();

            b.OpenElement(40, "div");
            b.AddAttribute(41, "class", "showcase-menubar-workspace__canvas");
            b.OpenElement(42, "div");
            b.AddAttribute(43, "class", "showcase-menubar-document");
            b.OpenElement(44, "span");
            b.AddAttribute(45, "class", "showcase-menubar-document__eyebrow");
            b.AddContent(46, "PRODUCTION DRAWING");
            b.CloseElement();
            b.OpenElement(47, "strong");
            b.AddContent(48, "CNC enclosure · Revision C");
            b.CloseElement();
            b.OpenElement(49, "span");
            b.AddContent(50, "3 files ready for engineering review");
            b.CloseElement();
            b.CloseElement();
            b.CloseElement();

            if (showStatus)
            {
                b.OpenElement(60, "footer");
                b.AddAttribute(61, "class", "showcase-menubar-workspace__status");
                b.AddContent(62, "Ready");
                b.OpenElement(63, "span");
                b.AddContent(64, "Page 1 of 3 · 100%");
                b.CloseElement();
                b.CloseElement();
            }

            b.CloseElement();
            b.CloseElement();
        };

        string Source() => $$"""
            <ShadcnMenubar Label="Quotation editor commands" Loop="{{loop.ToString().ToLowerInvariant()}}">
                <ShadcnMenubarMenu>
                    <ShadcnMenubarTrigger>File</ShadcnMenubarTrigger>
                    <ShadcnMenubarContent>
                        <ShadcnMenubarItem>New quotation <ShadcnMenubarShortcut>Ctrl+N</ShadcnMenubarShortcut></ShadcnMenubarItem>
                        <ShadcnMenubarSub>
                            <ShadcnMenubarSubTrigger>Open recent</ShadcnMenubarSubTrigger>
                            <ShadcnMenubarSubContent>
                                <ShadcnMenubarItem>QT-4189 · CNC enclosure</ShadcnMenubarItem>
                                <ShadcnMenubarItem>QT-4176 · Fixture plate</ShadcnMenubarItem>
                            </ShadcnMenubarSubContent>
                        </ShadcnMenubarSub>
                        <ShadcnMenubarSeparator />
                        <ShadcnMenubarItem>Save draft <ShadcnMenubarShortcut>Ctrl+S</ShadcnMenubarShortcut></ShadcnMenubarItem>
                    </ShadcnMenubarContent>
                </ShadcnMenubarMenu>
                <ShadcnMenubarMenu>
                    <ShadcnMenubarTrigger>Edit</ShadcnMenubarTrigger>
                    <ShadcnMenubarContent>
                        <ShadcnMenubarItem>Undo <ShadcnMenubarShortcut>Ctrl+Z</ShadcnMenubarShortcut></ShadcnMenubarItem>
                        <ShadcnMenubarItem Disabled="true">Redo <ShadcnMenubarShortcut>Ctrl+Shift+Z</ShadcnMenubarShortcut></ShadcnMenubarItem>
                        <ShadcnMenubarSeparator />
                        <ShadcnMenubarItem>Copy <ShadcnMenubarShortcut>Ctrl+C</ShadcnMenubarShortcut></ShadcnMenubarItem>
                        <ShadcnMenubarItem>Paste <ShadcnMenubarShortcut>Ctrl+V</ShadcnMenubarShortcut></ShadcnMenubarItem>
                    </ShadcnMenubarContent>
                </ShadcnMenubarMenu>
                <ShadcnMenubarMenu>
                    <ShadcnMenubarTrigger>View</ShadcnMenubarTrigger>
                    <ShadcnMenubarContent>
                        <ShadcnMenubarCheckboxItem Checked="{{showStatus.ToString().ToLowerInvariant()}}">Show status bar</ShadcnMenubarCheckboxItem>
                        <ShadcnMenubarSeparator />
                        <ShadcnMenubarLabel>Interface density</ShadcnMenubarLabel>
                        <ShadcnMenubarRadioGroup Value="comfortable">
                            <ShadcnMenubarRadioItem Value="comfortable">Comfortable</ShadcnMenubarRadioItem>
                            <ShadcnMenubarRadioItem Value="compact">Compact</ShadcnMenubarRadioItem>
                        </ShadcnMenubarRadioGroup>
                    </ShadcnMenubarContent>
                </ShadcnMenubarMenu>
                <ShadcnMenubarMenu>
                    <ShadcnMenubarTrigger>Help</ShadcnMenubarTrigger>
                    <ShadcnMenubarContent>
                        <ShadcnMenubarItem>Documentation</ShadcnMenubarItem>
                        <ShadcnMenubarItem>Keyboard shortcuts <ShadcnMenubarShortcut>Ctrl+/</ShadcnMenubarShortcut></ShadcnMenubarItem>
                        <ShadcnMenubarSeparator />
                        <ShadcnMenubarItem>About Shadcn Blazor</ShadcnMenubarItem>
                    </ShadcnMenubarContent>
                </ShadcnMenubarMenu>
            </ShadcnMenubar>
            """;

        return Example(
            "menubar",
            "Quotation editor menubar",
            preview,
            [Toggle("menubar-loop", "Loop keyboard navigation", value => loop = value, true), Toggle("menubar-status", "Show status bar", value => showStatus = value, true)],
            ["roving-focus", "open-switching", "submenu", "checkbox", "radio", "rtl", "reduced-motion"],
            Source()) with
        {
            Description = "Explore realistic file, edit, view, and help commands with stable pointer switching and complete keyboard navigation.",
            RazorSourceProvider = Source
        };
    }

    private static ComponentExampleDefinition Command()
    {
        var empty = false; var disabled = false;
        RenderFragment preview = builder =>
        {
            builder.OpenComponent<CommandDossierPreview>(0);
            builder.AddAttribute(1, nameof(CommandDossierPreview.EmptyState), empty);
            builder.AddAttribute(2, nameof(CommandDossierPreview.DisableFirstAction), disabled);
            builder.CloseComponent();
        };
        string Source()
        {
            var query = empty ? "no matching command" : string.Empty;
            var disabledAttribute = disabled ? " Disabled=\"true\"" : string.Empty;
            return $$"""
                @using Maliev.ShadcnBlazor.Components.Overlays

                <ShadcnCommand Label="Workspace commands" Class="workspace-command">
                    <ShadcnCommandInput Value="{{query}}"
                                        ValueChanged="SearchChanged"
                                        Placeholder="Type a command or search..." />
                    <ShadcnCommandList>
                        <ShadcnCommandEmpty>No commands match the search.</ShadcnCommandEmpty>
                        <ShadcnCommandGroup Heading="Workspace">
                            <ShadcnCommandItem Value="overview" TextValue="Overview" OnSelect="SelectAsync">
                                <span>Overview</span>
                                <ShadcnCommandShortcut>G O</ShadcnCommandShortcut>
                            </ShadcnCommandItem>
                            <ShadcnCommandItem Value="orders" TextValue="Orders" OnSelect="SelectAsync">
                                <span>Orders</span>
                                <ShadcnCommandShortcut>G R</ShadcnCommandShortcut>
                            </ShadcnCommandItem>
                            <ShadcnCommandItem Value="customers" TextValue="Customers" OnSelect="SelectAsync">
                                <span>Customers</span>
                                <ShadcnCommandShortcut>G C</ShadcnCommandShortcut>
                            </ShadcnCommandItem>
                        </ShadcnCommandGroup>
                        <ShadcnCommandSeparator />
                        <ShadcnCommandGroup Heading="Actions">
                            <ShadcnCommandItem Value="create-quotation" TextValue="Create quotation"{{disabledAttribute}} OnSelect="SelectAsync">
                                <span>Create quotation</span>
                                <ShadcnCommandShortcut>Q</ShadcnCommandShortcut>
                            </ShadcnCommandItem>
                            <ShadcnCommandItem Value="upload-drawing" TextValue="Upload drawing" OnSelect="SelectAsync">
                                <span>Upload drawing</span>
                                <ShadcnCommandShortcut>U</ShadcnCommandShortcut>
                            </ShadcnCommandItem>
                        </ShadcnCommandGroup>
                    </ShadcnCommandList>
                </ShadcnCommand>

                @code {
                    private string _query = "{{query}}";
                    private string _selection = "None yet";

                    private Task SearchChanged(string value)
                    {
                        _query = value;
                        return Task.CompletedTask;
                    }

                    private Task SelectAsync(string value)
                    {
                        _selection = value;
                        return Task.CompletedTask;
                    }
                }
                """;
        }

        var example = Example(
            "command",
            "Workspace command palette",
            preview,
            [Toggle("command-empty", "Show empty state", value => empty = value), Toggle("command-disabled", "Disable create action", value => disabled = value)],
            ["filtering", "Thai-keywords", "groups", "empty", "disabled", "keyboard", "pointer", "rtl"]);
        return example with { RazorSourceProvider = Source };
    }

    private static ComponentExampleDefinition Example(string slug, string title, RenderFragment preview, IReadOnlyList<ComponentParameterControl> controls, IReadOnlyList<string> tags, string? razorSource = null) =>
        new($"{slug}-primary", title, "Live package component with controlled state and the complete composition surface.", razorSource ?? $"<{Primary(slug)} />", preview, controls, tags);
    private static string Primary(string slug) => "Shadcn" + string.Concat(slug.Split('-').Select(part => char.ToUpperInvariant(part[0]) + part[1..]));
    private static ComponentParameterControl Toggle(string id, string label, Action<bool> apply, bool initial = false) => new(id, label, ComponentParameterControlKind.Toggle, initial.ToString(), [], value => apply(bool.Parse(value)));
    private static ComponentParameterControl Select<T>(string id, string label, T initial, Action<T> apply) where T : struct, Enum =>
        new(id, label, ComponentParameterControlKind.Select, initial.ToString(), Enum.GetNames<T>(), value => apply(Enum.Parse<T>(value)));
    private static RenderFragment Text(string value) => b => b.AddContent(0, value);
    private static void AddText<T>(RenderTreeBuilder b, int sequence, string text) where T : IComponent { b.OpenComponent<T>(sequence); b.AddAttribute(sequence + 1, "ChildContent", Text(text)); b.CloseComponent(); }
    private static void AddChecked<T>(RenderTreeBuilder b, int sequence, bool value, string text) where T : IComponent { b.OpenComponent<T>(sequence); b.AddAttribute(sequence + 1, "Checked", value); b.AddAttribute(sequence + 2, "ChildContent", Text(text)); b.CloseComponent(); }
    private static void AddMenubarFileMenu(RenderTreeBuilder b, int sequence) => AddMenubarMenu(b, sequence, "File", content =>
    {
        AddMenubarItem(content, 0, "New quotation", "Ctrl+N");
        content.OpenComponent<ShadcnMenubarSub>(10);
        content.AddAttribute(11, "ChildContent", (RenderFragment)(sub =>
        {
            AddText<ShadcnMenubarSubTrigger>(sub, 0, "Open recent");
            sub.OpenComponent<ShadcnMenubarSubContent>(10);
            sub.AddAttribute(11, "ChildContent", (RenderFragment)(recent =>
            {
                AddText<ShadcnMenubarItem>(recent, 0, "QT-4189 · CNC enclosure");
                AddText<ShadcnMenubarItem>(recent, 10, "QT-4176 · Fixture plate");
            }));
            sub.CloseComponent();
        }));
        content.CloseComponent();
        content.OpenComponent<ShadcnMenubarSeparator>(20); content.CloseComponent();
        AddMenubarItem(content, 30, "Save draft", "Ctrl+S");
    });

    private static void AddMenubarEditMenu(RenderTreeBuilder b, int sequence) => AddMenubarMenu(b, sequence, "Edit", content =>
    {
        AddMenubarItem(content, 0, "Undo", "Ctrl+Z");
        AddMenubarItem(content, 10, "Redo", "Ctrl+Shift+Z", true);
        content.OpenComponent<ShadcnMenubarSeparator>(20); content.CloseComponent();
        AddMenubarItem(content, 30, "Copy", "Ctrl+C");
        AddMenubarItem(content, 40, "Paste", "Ctrl+V");
    });

    private static void AddMenubarViewMenu(RenderTreeBuilder b, int sequence, bool showStatus) => AddMenubarMenu(b, sequence, "View", content =>
    {
        content.OpenComponent<ShadcnMenubarCheckboxItem>(0);
        content.AddAttribute(1, "Checked", showStatus);
        content.AddAttribute(2, "ChildContent", Text("Show status bar"));
        content.CloseComponent();
        content.OpenComponent<ShadcnMenubarSeparator>(10); content.CloseComponent();
        AddText<ShadcnMenubarLabel>(content, 20, "Interface density");
        content.OpenComponent<ShadcnMenubarRadioGroup>(30);
        content.AddAttribute(31, "Value", "comfortable");
        content.AddAttribute(32, "ChildContent", (RenderFragment)(radio =>
        {
            AddMenubarRadioItem(radio, 0, "comfortable", "Comfortable");
            AddMenubarRadioItem(radio, 10, "compact", "Compact");
        }));
        content.CloseComponent();
    });

    private static void AddMenubarHelpMenu(RenderTreeBuilder b, int sequence) => AddMenubarMenu(b, sequence, "Help", content =>
    {
        AddText<ShadcnMenubarItem>(content, 0, "Documentation");
        AddMenubarItem(content, 10, "Keyboard shortcuts", "Ctrl+/");
        content.OpenComponent<ShadcnMenubarSeparator>(20); content.CloseComponent();
        AddText<ShadcnMenubarItem>(content, 30, "About Shadcn Blazor");
    });

    private static void AddMenubarMenu(RenderTreeBuilder b, int sequence, string trigger, RenderFragment content)
    {
        b.OpenComponent<ShadcnMenubarMenu>(sequence);
        b.AddAttribute(sequence + 1, "ChildContent", (RenderFragment)(menu =>
        {
            AddText<ShadcnMenubarTrigger>(menu, 0, trigger);
            menu.OpenComponent<ShadcnMenubarContent>(10);
            menu.AddAttribute(11, "ChildContent", content);
            menu.CloseComponent();
        }));
        b.CloseComponent();
    }

    private static void AddMenubarItem(RenderTreeBuilder b, int sequence, string text, string shortcut, bool disabled = false)
    {
        b.OpenComponent<ShadcnMenubarItem>(sequence);
        b.AddAttribute(sequence + 1, "Disabled", disabled);
        b.AddAttribute(sequence + 2, "ChildContent", (RenderFragment)(content =>
        {
            content.AddContent(0, text);
            AddText<ShadcnMenubarShortcut>(content, 1, shortcut);
        }));
        b.CloseComponent();
    }

    private static void AddMenubarRadioItem(RenderTreeBuilder b, int sequence, string value, string text)
    {
        b.OpenComponent<ShadcnMenubarRadioItem>(sequence);
        b.AddAttribute(sequence + 1, "Value", value);
        b.AddAttribute(sequence + 2, "ChildContent", Text(text));
        b.CloseComponent();
    }

    private static void AddPopoverField(RenderTreeBuilder builder, int sequence, string id, string label, decimal value, Action<decimal> changed)
    {
        builder.OpenComponent<ShadcnLabel>(sequence);
        builder.AddAttribute(sequence + 1, nameof(ShadcnLabel.For), id);
        builder.AddAttribute(sequence + 2, nameof(ShadcnLabel.ChildContent), Text(label));
        builder.CloseComponent();
        builder.OpenComponent<ShadcnInput<decimal>>(sequence + 10);
        builder.AddAttribute(sequence + 11, nameof(ShadcnInput<decimal>.Value), value);
        builder.AddAttribute(sequence + 12, nameof(ShadcnInput<decimal>.ValueChanged), EventCallback.Factory.Create<decimal>(new object(), changed));
        builder.AddAttribute(sequence + 13, nameof(ShadcnInput<decimal>.Type), "number");
        builder.AddAttribute(sequence + 14, nameof(ShadcnInput<decimal>.AdditionalAttributes), new Dictionary<string, object> { ["id"] = id });
        builder.CloseComponent();
    }
}
