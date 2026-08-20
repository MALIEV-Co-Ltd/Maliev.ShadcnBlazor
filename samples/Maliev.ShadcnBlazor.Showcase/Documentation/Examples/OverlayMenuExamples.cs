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
        "dropdown-menu" => [DropdownMenu()],
        "context-menu" => [ContextMenu()],
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
        var direction = ShadcnDrawerSwipeDirection.Down;
        var modalMode = ShadcnDrawerModalMode.Modal;
        var showHandle = true;
        var useSnapPoints = true;

        RenderFragment preview = builder =>
        {
            builder.OpenComponent<DrawerDossierPreview>(0);
            builder.AddAttribute(1, nameof(DrawerDossierPreview.Direction), direction);
            builder.AddAttribute(2, nameof(DrawerDossierPreview.ModalMode), modalMode);
            builder.AddAttribute(3, nameof(DrawerDossierPreview.ShowSwipeHandle), showHandle);
            builder.AddAttribute(4, nameof(DrawerDossierPreview.UseSnapPoints), useSnapPoints);
            builder.CloseComponent();
        };

        string Source()
        {
            var vertical = direction is ShadcnDrawerSwipeDirection.Up or ShadcnDrawerSwipeDirection.Down;
            var snapAttribute = useSnapPoints && vertical ? "\n              SnapPoints=\"SnapPoints\"\n              SnapPoint=\"SnapPoints[0]\"" : string.Empty;
            var snapField = useSnapPoints && vertical
                ? "\n    private static readonly IReadOnlyList<ShadcnDrawerSnapPoint> SnapPoints =\n        [ShadcnDrawerSnapPoint.Fraction(0.55), ShadcnDrawerSnapPoint.Fraction(0.9)];\n"
                : string.Empty;

            return $$"""
@using Maliev.ShadcnBlazor.Components.Overlays

<div class="showcase-drawer-dossier">
    <div class="showcase-drawer-dossier__summary">
        <span class="showcase-drawer-dossier__eyebrow">Dispatch review</span>
        <strong>Order QT-4189</strong>
        <span>Bangkok production hub · 3 packages ready</span>
        <ShadcnDrawer @bind-Open="Open"
              SwipeDirection="ShadcnDrawerSwipeDirection.{{direction}}"
              ModalMode="ShadcnDrawerModalMode.{{modalMode}}"
              ShowSwipeHandle="{{showHandle.ToString().ToLowerInvariant()}}"{{snapAttribute}}>
    <ShadcnDrawerTrigger Class="showcase-drawer-dossier__trigger">Review dispatch</ShadcnDrawerTrigger>
    <ShadcnDrawerContent Class="showcase-drawer-panel">
        <ShadcnDrawerHeader>
            <ShadcnDrawerTitle>Confirm dispatch</ShadcnDrawerTitle>
            <ShadcnDrawerDescription>
                Check the destination and production handoff before releasing this order.
            </ShadcnDrawerDescription>
        </ShadcnDrawerHeader>
        <div class="showcase-drawer-panel__body">
            <dl>
                <div><dt>Destination</dt><dd>Samut Prakan, Thailand</dd></div>
                <div><dt>Carrier</dt><dd>Kerry Express · Next day</dd></div>
                <div><dt>Handoff</dt><dd>Bangkok production hub</dd></div>
            </dl>
            <p>All three packages passed final inspection at 14:20.</p>
        </div>
        <ShadcnDrawerFooter>
            <ShadcnDrawerClose Class="showcase-drawer-panel__action showcase-drawer-panel__action--primary">Confirm dispatch</ShadcnDrawerClose>
            <ShadcnDrawerClose Class="showcase-drawer-panel__action">Cancel</ShadcnDrawerClose>
        </ShadcnDrawerFooter>
    </ShadcnDrawerContent>
        </ShadcnDrawer>
    </div>
</div>

@code {
    private bool Open { get; set; }
{{snapField}}}
""";
        }

        var controls = new ComponentParameterControl[]
        {
            new("drawer-direction", "Edge", ComponentParameterControlKind.Select, direction.ToString(), Enum.GetNames<ShadcnDrawerSwipeDirection>(), value => direction = Enum.Parse<ShadcnDrawerSwipeDirection>(value)),
            new("drawer-modal-mode", "Focus behavior", ComponentParameterControlKind.Select, modalMode.ToString(), Enum.GetNames<ShadcnDrawerModalMode>(), value => modalMode = Enum.Parse<ShadcnDrawerModalMode>(value)),
            Toggle("drawer-handle", "Swipe handle", value => showHandle = value, true),
            Toggle("drawer-snap-points", "Snap points", value => useSnapPoints = value, true)
        };
        var example = Example("drawer", "Dispatch drawer", preview, controls, ["trigger", "four-edges", "swipe", "snap-points", "focus", "escape", "outside-press", "rtl"], Source());
        return example with
        {
            Description = "Review a production dispatch with direct trigger, dismissal, focus, and gesture behavior.",
            RazorSourceProvider = Source
        };
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
        var instant = false;
        var disabled = false;
        RenderFragment preview = b =>
        {
            b.OpenElement(0, "div");
            b.AddAttribute(1, "class", "showcase-tooltip-dossier");
            b.AddAttribute(2, "data-testid", "tooltip-dossier-preview");
            b.AddAttribute(3, "data-side", bottom ? "bottom" : "top");
            b.AddAttribute(4, "data-disabled", disabled ? "true" : "false");
            b.OpenElement(5, "section");
            b.AddAttribute(6, "class", "showcase-tooltip-dossier__card");
            b.AddAttribute(7, "aria-label", "Quotation draft actions");
            b.OpenElement(8, "div");
            b.AddAttribute(9, "class", "showcase-tooltip-dossier__copy");
            b.OpenElement(10, "strong");
            b.AddContent(11, "QT-4189 · CNC enclosure");
            b.CloseElement();
            b.OpenElement(12, "span");
            b.AddContent(13, "Saved just now");
            b.CloseElement();
            b.CloseElement();
            b.OpenComponent<ShadcnTooltipProvider>(20);
            b.AddAttribute(21, "OpenDelay", instant ? TimeSpan.Zero : TimeSpan.FromMilliseconds(200));
            b.AddAttribute(22, "ChildContent", (RenderFragment)(p =>
            {
                p.OpenComponent<ShadcnTooltip>(0);
                p.AddAttribute(1, "ChildContent", (RenderFragment)(c =>
                {
                    c.OpenComponent<ShadcnTooltipTrigger>(0);
                    c.AddAttribute(1, "Class", "showcase-tooltip-dossier__action");
                    c.AddAttribute(2, "Disabled", disabled);
                    c.AddAttribute(3, "AccessibleLabel", "Save quotation draft");
                    c.AddAttribute(4, "ChildContent", Text("Save quotation draft"));
                    c.CloseComponent();
                    c.OpenComponent<ShadcnTooltipContent>(10);
                    c.AddAttribute(11, "Side", bottom ? ShadcnOverlaySide.Bottom : ShadcnOverlaySide.Top);
                    c.AddAttribute(12, "ChildContent", Text(disabled ? "Saving is unavailable while totals update" : "Save draft · Ctrl+S"));
                    c.CloseComponent();
                }));
                p.CloseComponent();
            }));
            b.CloseComponent();
            b.CloseElement();
            b.CloseElement();
        };
        string Source()
        {
            var delay = instant ? "TimeSpan.Zero" : "@(TimeSpan.FromMilliseconds(200))";
            var side = bottom ? "Bottom" : "Top";
            var disabledAttribute = disabled ? " Disabled=\"true\"" : string.Empty;
            var content = disabled ? "Saving is unavailable while totals update" : "Save draft · Ctrl+S";
            return $"""
                <section class="quotation-draft-actions" aria-label="Quotation draft actions">
                    <div>
                        <strong>QT-4189 · CNC enclosure</strong>
                        <span>Saved just now</span>
                    </div>
                    <ShadcnTooltipProvider OpenDelay="{delay}">
                        <ShadcnTooltip>
                            <ShadcnTooltipTrigger AccessibleLabel="Save quotation draft"{disabledAttribute}>Save quotation draft</ShadcnTooltipTrigger>
                            <ShadcnTooltipContent Side="ShadcnOverlaySide.{side}">{content}</ShadcnTooltipContent>
                        </ShadcnTooltip>
                    </ShadcnTooltipProvider>
                </section>
                """;
        }
        return Example(
            "tooltip",
            "Quotation draft tooltip",
            preview,
            [Toggle("tooltip-bottom", "Bottom placement", v => bottom = v), Toggle("tooltip-instant", "No open delay", v => instant = v), Toggle("tooltip-disabled", "Disabled action", v => disabled = v)],
            ["pointer", "focus", "escape", "provider-delay", "disabled", "collision", "rtl", "reduced-motion"],
            Source()) with
        {
            Description = "Hover or focus a real quotation action to inspect delayed, disabled, and collision-aware tooltip behavior.",
            RazorSourceProvider = Source
        };
    }

    private static ComponentExampleDefinition DropdownMenu()
    {
        var loop = true;
        var showDetails = true;
        var density = "comfortable";

        RenderFragment preview = b =>
        {
            b.OpenElement(0, "div");
            b.AddAttribute(1, "class", "showcase-dropdown-menu-dossier");
            b.AddAttribute(2, "data-testid", "dropdown-menu-dossier-preview");
            b.OpenElement(3, "section");
            b.AddAttribute(4, "class", "showcase-dropdown-menu-card");
            b.AddAttribute(5, "aria-label", "Quotation QT-4189 summary");
            b.OpenElement(6, "div");
            b.AddAttribute(7, "class", "showcase-dropdown-menu-card__identity");
            b.OpenElement(8, "span");
            b.AddAttribute(9, "class", "showcase-dropdown-menu-card__eyebrow");
            b.AddContent(10, "READY FOR REVIEW");
            b.CloseElement();
            b.OpenElement(11, "strong");
            b.AddContent(12, "Quotation QT-4189");
            b.CloseElement();
            b.OpenElement(13, "span");
            b.AddContent(14, "CNC enclosure · Revision C");
            b.CloseElement();
            b.CloseElement();
            b.OpenComponent<ShadcnDropdownMenu>(20);
            b.AddAttribute(21, nameof(ShadcnDropdownMenu.Loop), loop);
            b.AddAttribute(22, nameof(ShadcnDropdownMenu.ChildContent), (RenderFragment)(menu =>
            {
                menu.OpenComponent<ShadcnDropdownMenuTrigger>(0);
                menu.AddAttribute(1, nameof(ShadcnDropdownMenuTrigger.ChildContent), (RenderFragment)(trigger =>
                {
                    trigger.AddContent(0, "Actions");
                    trigger.OpenElement(1, "svg");
                    trigger.AddAttribute(2, "viewBox", "0 0 24 24");
                    trigger.AddAttribute(3, "aria-hidden", "true");
                    trigger.OpenElement(4, "circle"); trigger.AddAttribute(5, "cx", "5"); trigger.AddAttribute(6, "cy", "12"); trigger.AddAttribute(7, "r", "1"); trigger.CloseElement();
                    trigger.OpenElement(8, "circle"); trigger.AddAttribute(9, "cx", "12"); trigger.AddAttribute(10, "cy", "12"); trigger.AddAttribute(11, "r", "1"); trigger.CloseElement();
                    trigger.OpenElement(12, "circle"); trigger.AddAttribute(13, "cx", "19"); trigger.AddAttribute(14, "cy", "12"); trigger.AddAttribute(15, "r", "1"); trigger.CloseElement();
                    trigger.CloseElement();
                }));
                menu.CloseComponent();
                menu.OpenComponent<ShadcnDropdownMenuContent>(10);
                menu.AddAttribute(11, nameof(ShadcnDropdownMenuContent.ChildContent), (RenderFragment)(content => AddDropdownMenuContent(content, showDetails, density)));
                menu.CloseComponent();
            }));
            b.CloseComponent();
            b.OpenElement(30, "div");
            b.AddAttribute(31, "class", "showcase-dropdown-menu-card__meta");
            b.OpenElement(32, "span"); b.AddContent(33, "Customer"); b.OpenElement(34, "strong"); b.AddContent(35, "Siam Precision Co., Ltd."); b.CloseElement(); b.CloseElement();
            b.OpenElement(36, "span"); b.AddContent(37, "Total"); b.OpenElement(38, "strong"); b.AddContent(39, "฿128,400"); b.CloseElement(); b.CloseElement();
            b.OpenElement(40, "span"); b.AddContent(41, "Updated"); b.OpenElement(42, "strong"); b.AddContent(43, "Today, 10:42"); b.CloseElement(); b.CloseElement();
            b.CloseElement();
            b.CloseElement();
            b.CloseElement();
        };

        string Source() => $$"""
            @using Maliev.ShadcnBlazor.Components.Overlays

            <ShadcnDropdownMenu @bind-Open="Open" Loop="{{loop.ToString().ToLowerInvariant()}}">
                <ShadcnDropdownMenuTrigger>Actions</ShadcnDropdownMenuTrigger>
                <ShadcnDropdownMenuContent>
                    <ShadcnDropdownMenuLabel>Quotation actions</ShadcnDropdownMenuLabel>
                    <ShadcnDropdownMenuGroup>
                        <ShadcnDropdownMenuItem>
                            Open quotation <ShadcnDropdownMenuShortcut>Enter</ShadcnDropdownMenuShortcut>
                        </ShadcnDropdownMenuItem>
                        <ShadcnDropdownMenuItem>
                            Duplicate <ShadcnDropdownMenuShortcut>Ctrl+D</ShadcnDropdownMenuShortcut>
                        </ShadcnDropdownMenuItem>
                        <ShadcnDropdownMenuItem Disabled="true">Request approval</ShadcnDropdownMenuItem>
                    </ShadcnDropdownMenuGroup>
                    <ShadcnDropdownMenuSeparator />
                    <ShadcnDropdownMenuCheckboxItem Checked="{{showDetails.ToString().ToLowerInvariant()}}">
                        Show archived details
                    </ShadcnDropdownMenuCheckboxItem>
                    <ShadcnDropdownMenuLabel Inset="true">Interface density</ShadcnDropdownMenuLabel>
                    <ShadcnDropdownMenuRadioGroup Value="comfortable">
                        <ShadcnDropdownMenuRadioItem Value="comfortable">Comfortable</ShadcnDropdownMenuRadioItem>
                        <ShadcnDropdownMenuRadioItem Value="compact">Compact</ShadcnDropdownMenuRadioItem>
                    </ShadcnDropdownMenuRadioGroup>
                    <ShadcnDropdownMenuSub>
                        <ShadcnDropdownMenuSubTrigger>Export</ShadcnDropdownMenuSubTrigger>
                        <ShadcnDropdownMenuSubContent>
                            <ShadcnDropdownMenuItem>PDF package</ShadcnDropdownMenuItem>
                            <ShadcnDropdownMenuItem>CSV costing</ShadcnDropdownMenuItem>
                            <ShadcnDropdownMenuItem>STEP files</ShadcnDropdownMenuItem>
                        </ShadcnDropdownMenuSubContent>
                    </ShadcnDropdownMenuSub>
                    <ShadcnDropdownMenuSeparator />
                    <ShadcnDropdownMenuItem Variant="ShadcnMenuItemVariant.Destructive">Archive quotation</ShadcnDropdownMenuItem>
                </ShadcnDropdownMenuContent>
            </ShadcnDropdownMenu>

            @code {
                private bool Open { get; set; }
            }
            """;

        return Example(
            "dropdown-menu",
            "Quotation action menu",
            preview,
            [Toggle("dropdown-menu-loop", "Loop keyboard navigation", value => loop = value, true), Toggle("dropdown-menu-details", "Show archived details", value => showDetails = value, true)],
            ["trigger", "keyboard", "typeahead", "checkbox", "radio", "submenu", "disabled", "destructive", "rtl", "reduced-motion"],
            Source()) with
        {
            Description = "Open a complete quotation command menu with selection state, shortcuts, disabled actions, and a nested export workflow.",
            RazorSourceProvider = Source
        };
    }

    private static ComponentExampleDefinition ContextMenu()
    {
        var checkedValue = true;
        var compact = false;
        RenderFragment preview = b =>
        {
            b.OpenComponent<ContextMenuDossierPreview>(0);
            b.AddAttribute(1, nameof(ContextMenuDossierPreview.ShowArchived), checkedValue);
            b.AddAttribute(2, nameof(ContextMenuDossierPreview.Density), compact ? "compact" : "comfortable");
            b.CloseComponent();
        };

        string Source() => $$"""
            @using Maliev.ShadcnBlazor.Components.Overlays

            <ShadcnContextMenu>
                <ShadcnContextMenuTrigger Class="file-workspace">
                    <strong>Quotation files</strong>
                    <span>QT-4189 · CNC enclosure · 3 files</span>
                    <p>Right-click or press Shift+F10 for file actions.</p>
                </ShadcnContextMenuTrigger>
                <ShadcnContextMenuContent>
                    <ShadcnContextMenuLabel>File actions</ShadcnContextMenuLabel>
                    <ShadcnContextMenuGroup>
                        <ShadcnContextMenuItem OnSelect="@(_ => LastAction = "Opened enclosure.step")">
                            Open
                            <ShadcnContextMenuShortcut>Enter</ShadcnContextMenuShortcut>
                        </ShadcnContextMenuItem>
                        <ShadcnContextMenuItem Disabled="true">Publish revision</ShadcnContextMenuItem>
                    </ShadcnContextMenuGroup>
                    <ShadcnContextMenuSeparator />
                    <ShadcnContextMenuCheckboxItem @bind-Checked="ShowArchived">Show archived files</ShadcnContextMenuCheckboxItem>
                    <ShadcnContextMenuRadioGroup @bind-Value="Density">
                        <ShadcnContextMenuRadioItem Value="comfortable">Comfortable rows</ShadcnContextMenuRadioItem>
                        <ShadcnContextMenuRadioItem Value="compact">Compact rows</ShadcnContextMenuRadioItem>
                    </ShadcnContextMenuRadioGroup>
                    <ShadcnContextMenuSeparator />
                    <ShadcnContextMenuSub>
                        <ShadcnContextMenuSubTrigger>Export as</ShadcnContextMenuSubTrigger>
                        <ShadcnContextMenuSubContent>
                            <ShadcnContextMenuItem>PDF package</ShadcnContextMenuItem>
                            <ShadcnContextMenuItem>ZIP archive</ShadcnContextMenuItem>
                        </ShadcnContextMenuSubContent>
                    </ShadcnContextMenuSub>
                    <ShadcnContextMenuSeparator />
                    <ShadcnContextMenuItem Variant="ShadcnMenuItemVariant.Destructive">
                        Move to trash
                        <ShadcnContextMenuShortcut>Delete</ShadcnContextMenuShortcut>
                    </ShadcnContextMenuItem>
                </ShadcnContextMenuContent>
            </ShadcnContextMenu>
            <p role="status">@LastAction</p>

            @code {
                private bool ShowArchived { get; set; } = {{checkedValue.ToString().ToLowerInvariant()}};
                private string Density { get; set; } = "{{(compact ? "compact" : "comfortable")}}";
                private string LastAction { get; set; } = "No file action selected";
            }
            """;

        return Example(
            "context-menu",
            "File workspace context menu",
            preview,
            [Toggle("context-menu-archived", "Show archived files", value => checkedValue = value, true), Toggle("context-menu-compact", "Compact rows", value => compact = value)],
            ["pointer", "Shift+F10", "focus-restore", "typeahead", "checkbox", "radio", "submenu", "disabled", "shortcut", "collision", "rtl"],
            Source()) with
        {
            Description = "Right-click a realistic file workspace or use Shift+F10 to inspect selection, density, submenu, disabled, and destructive states.",
            RazorSourceProvider = Source
        };
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

    private static void AddDropdownMenuContent(RenderTreeBuilder content, bool showDetails, string density)
    {
        AddText<ShadcnDropdownMenuLabel>(content, 0, "Quotation actions");
        content.OpenComponent<ShadcnDropdownMenuGroup>(10);
        content.AddAttribute(11, nameof(ShadcnDropdownMenuGroup.ChildContent), (RenderFragment)(group =>
        {
            AddDropdownMenuItem(group, 0, "Open quotation", "Enter");
            AddDropdownMenuItem(group, 10, "Duplicate", "Ctrl+D");
            group.OpenComponent<ShadcnDropdownMenuItem>(20);
            group.AddAttribute(21, nameof(ShadcnDropdownMenuItem.Disabled), true);
            group.AddAttribute(22, nameof(ShadcnDropdownMenuItem.ChildContent), Text("Request approval"));
            group.CloseComponent();
        }));
        content.CloseComponent();
        content.OpenComponent<ShadcnDropdownMenuSeparator>(20); content.CloseComponent();
        content.OpenComponent<ShadcnDropdownMenuCheckboxItem>(30);
        content.AddAttribute(31, nameof(ShadcnDropdownMenuCheckboxItem.Checked), showDetails);
        content.AddAttribute(32, nameof(ShadcnDropdownMenuCheckboxItem.ChildContent), Text("Show archived details"));
        content.CloseComponent();
        content.OpenComponent<ShadcnDropdownMenuLabel>(40);
        content.AddAttribute(41, nameof(ShadcnDropdownMenuLabel.Inset), true);
        content.AddAttribute(42, nameof(ShadcnDropdownMenuLabel.ChildContent), Text("Interface density"));
        content.CloseComponent();
        content.OpenComponent<ShadcnDropdownMenuRadioGroup>(50);
        content.AddAttribute(51, nameof(ShadcnDropdownMenuRadioGroup.Value), density);
        content.AddAttribute(52, nameof(ShadcnDropdownMenuRadioGroup.ChildContent), (RenderFragment)(radio =>
        {
            AddDropdownRadioItem(radio, 0, "comfortable", "Comfortable");
            AddDropdownRadioItem(radio, 10, "compact", "Compact");
        }));
        content.CloseComponent();
        content.OpenComponent<ShadcnDropdownMenuSub>(60);
        content.AddAttribute(61, nameof(ShadcnDropdownMenuSub.ChildContent), (RenderFragment)(sub =>
        {
            AddText<ShadcnDropdownMenuSubTrigger>(sub, 0, "Export");
            sub.OpenComponent<ShadcnDropdownMenuSubContent>(10);
            sub.AddAttribute(11, nameof(ShadcnDropdownMenuSubContent.ChildContent), (RenderFragment)(export =>
            {
                AddText<ShadcnDropdownMenuItem>(export, 0, "PDF package");
                AddText<ShadcnDropdownMenuItem>(export, 10, "CSV costing");
                AddText<ShadcnDropdownMenuItem>(export, 20, "STEP files");
            }));
            sub.CloseComponent();
        }));
        content.CloseComponent();
        content.OpenComponent<ShadcnDropdownMenuSeparator>(70); content.CloseComponent();
        content.OpenComponent<ShadcnDropdownMenuItem>(80);
        content.AddAttribute(81, nameof(ShadcnDropdownMenuItem.Variant), ShadcnMenuItemVariant.Destructive);
        content.AddAttribute(82, nameof(ShadcnDropdownMenuItem.ChildContent), Text("Archive quotation"));
        content.CloseComponent();
    }

    private static void AddDropdownMenuItem(RenderTreeBuilder builder, int sequence, string text, string shortcut)
    {
        builder.OpenComponent<ShadcnDropdownMenuItem>(sequence);
        builder.AddAttribute(sequence + 1, nameof(ShadcnDropdownMenuItem.ChildContent), (RenderFragment)(child =>
        {
            child.AddContent(0, text);
            AddText<ShadcnDropdownMenuShortcut>(child, 1, shortcut);
        }));
        builder.CloseComponent();
    }

    private static void AddDropdownRadioItem(RenderTreeBuilder builder, int sequence, string value, string text)
    {
        builder.OpenComponent<ShadcnDropdownMenuRadioItem>(sequence);
        builder.AddAttribute(sequence + 1, nameof(ShadcnDropdownMenuRadioItem.Value), value);
        builder.AddAttribute(sequence + 2, nameof(ShadcnDropdownMenuRadioItem.ChildContent), Text(text));
        builder.CloseComponent();
    }

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
