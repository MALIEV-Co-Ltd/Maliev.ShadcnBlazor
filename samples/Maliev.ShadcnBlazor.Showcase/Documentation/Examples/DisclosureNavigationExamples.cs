using Maliev.ShadcnBlazor.Components.Disclosure;
using Maliev.ShadcnBlazor.Components.Layout;
using Maliev.ShadcnBlazor.Components.Navigation;
using Maliev.ShadcnBlazor.Components.Navigation.Sidebar;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Rendering;

namespace Maliev.ShadcnBlazor.Showcase.Documentation.Examples;

internal static class DisclosureNavigationExamples
{
    public static IReadOnlyList<ComponentExampleDefinition> Create(string slug) => slug switch
    {
        "accordion" => [Accordion()], "breadcrumb" => [Breadcrumb()], "collapsible" => [Collapsible()],
        "navigation-menu" => [NavigationMenu()], "pagination" => [Pagination()], "resizable" => [Resizable()],
        "scroll-area" => [ScrollArea()], "sidebar" => [Sidebar()], "tabs" => [Tabs()], _ => []
    };

    private static ComponentExampleDefinition Accordion()
    {
        var multiple = false; var horizontal = false; var disabled = false;
        RenderFragment preview = b => { b.OpenComponent<ShadcnAccordion>(0); b.AddAttribute(1, "Values", new[] { "shipping" }); b.AddAttribute(2, "Multiple", multiple); b.AddAttribute(3, "Orientation", horizontal ? ShadcnAccordionOrientation.Horizontal : ShadcnAccordionOrientation.Vertical); b.AddAttribute(4, "Disabled", disabled); b.AddAttribute(5, "Label", "Manufacturing questions"); b.AddAttribute(6, "ChildContent", (RenderFragment)(c => { AddAccordionItem(c, 0, "shipping", "Shipping", "Ships from Thailand."); AddAccordionItem(c, 10, "quality", "Quality ภาษาไทย", "Inspection included."); })); b.CloseComponent(); };
        return Example("accordion", "Accordion states", "Controlled disclosure with single or multiple values, orientation, disabled state, keyboard roving, RTL, and Thai content.", "<ShadcnAccordion Values=\"Values\"><ShadcnAccordionItem Value=\"shipping\">...</ShadcnAccordionItem></ShadcnAccordion>", preview, [Toggle("accordion-multiple", "Multiple", v => multiple = v), Toggle("accordion-horizontal", "Horizontal", v => horizontal = v), Toggle("accordion-disabled", "Disabled", v => disabled = v)], ["single", "multiple", "horizontal", "vertical", "keyboard", "rtl", "disabled"]);
    }

    private static ComponentExampleDefinition Breadcrumb()
    {
        var ellipsis = true;
        RenderFragment preview = b => { b.OpenComponent<ShadcnBreadcrumb>(0); b.AddAttribute(1, "Label", "Project breadcrumb"); b.AddAttribute(2, "ChildContent", (RenderFragment)(c => { c.OpenComponent<ShadcnBreadcrumbList>(0); c.AddAttribute(1, "ChildContent", (RenderFragment)(l => { AddBreadcrumbLink(l, 0, "Home", "/"); Add<ShadcnBreadcrumbSeparator>(l, 10); if (ellipsis) { Add<ShadcnBreadcrumbEllipsis>(l, 20); Add<ShadcnBreadcrumbSeparator>(l, 30); } l.OpenComponent<ShadcnBreadcrumbItem>(40); l.AddAttribute(41, "ChildContent", (RenderFragment)(i => AddText<ShadcnBreadcrumbPage>(i, 0, "Quotation"))); l.CloseComponent(); })); c.CloseComponent(); })); b.CloseComponent(); };
        return Example("breadcrumb", "Breadcrumb composition", "Landmark, ordered list, links, separators, ellipsis, and current-page semantics.", "<ShadcnBreadcrumb><ShadcnBreadcrumbList>...</ShadcnBreadcrumbList></ShadcnBreadcrumb>", preview, [Toggle("breadcrumb-ellipsis", "Ellipsis", v => ellipsis = v, true)], ["links", "separator", "ellipsis", "current-page", "rtl"]);
    }

    private static ComponentExampleDefinition Collapsible()
    {
        var open = true; var disabled = false;
        RenderFragment preview = b => { b.OpenComponent<ShadcnCollapsible>(0); b.AddAttribute(1, "Open", open); b.AddAttribute(2, "Disabled", disabled); b.AddAttribute(3, "ChildContent", (RenderFragment)(c => { AddText<ShadcnCollapsibleTrigger>(c, 0, "Project files"); AddText<ShadcnCollapsibleContent>(c, 10, "Drawing.step"); })); b.CloseComponent(); };
        return Example("collapsible", "Collapsible disclosure", "Open, closed, disabled, controlled, and native button activation states.", "<ShadcnCollapsible Open=\"true\"><ShadcnCollapsibleTrigger>Files</ShadcnCollapsibleTrigger><ShadcnCollapsibleContent>...</ShadcnCollapsibleContent></ShadcnCollapsible>", preview, [Toggle("collapsible-open", "Open", v => open = v, true), Toggle("collapsible-disabled", "Disabled", v => disabled = v)], ["open", "closed", "disabled", "controlled"]);
    }

    private static ComponentExampleDefinition NavigationMenu()
    {
        string? value = "services"; var vertical = false; var disabled = false;
        RenderFragment preview = b => { b.OpenComponent<ShadcnNavigationMenu>(0); b.AddAttribute(1, "Value", value); b.AddAttribute(2, "Orientation", vertical ? ShadcnNavigationMenuOrientation.Vertical : ShadcnNavigationMenuOrientation.Horizontal); b.AddAttribute(3, "Disabled", disabled); b.AddAttribute(4, "Label", "Services"); b.AddAttribute(5, "ChildContent", (RenderFragment)(c => { c.OpenComponent<ShadcnNavigationMenuList>(0); c.AddAttribute(1, "ChildContent", (RenderFragment)(l => { l.OpenComponent<ShadcnNavigationMenuItem>(0); l.AddAttribute(1, "Value", "services"); l.AddAttribute(2, "ChildContent", (RenderFragment)(i => { AddText<ShadcnNavigationMenuTrigger>(i, 0, "Services"); i.OpenComponent<ShadcnNavigationMenuContent>(10); i.AddAttribute(11, "ChildContent", (RenderFragment)(p => { p.OpenComponent<ShadcnNavigationMenuLink>(0); p.AddAttribute(1, "Href", "#cnc"); p.AddAttribute(2, "ChildContent", Text("CNC machining")); p.CloseComponent(); })); i.CloseComponent(); })); l.CloseComponent(); })); c.CloseComponent(); Add<ShadcnNavigationMenuIndicator>(c, 10); Add<ShadcnNavigationMenuViewport>(c, 20); })); b.CloseComponent(); };
        return Example("navigation-menu", "Navigation menu portal", "Delayed hover, click, keyboard focus, controlled open state, collision-aware top-layer viewport, RTL, and disabled behavior.", "<ShadcnNavigationMenu><ShadcnNavigationMenuList>...</ShadcnNavigationMenuList><ShadcnNavigationMenuViewport /></ShadcnNavigationMenu>", preview, [Toggle("navigation-open", "Open", v => value = v ? "services" : null, true), Toggle("navigation-vertical", "Vertical", v => vertical = v), Toggle("navigation-disabled", "Disabled", v => disabled = v)], ["open", "closed", "hover", "keyboard", "portal", "collision", "rtl"]);
    }

    private static ComponentExampleDefinition Pagination()
    {
        var current = 2d; var disabled = false;
        RenderFragment preview = b => { b.OpenComponent<ShadcnPagination>(0); b.AddAttribute(1, "Label", "Quotation pages"); b.AddAttribute(2, "ChildContent", (RenderFragment)(c => { c.OpenComponent<ShadcnPaginationContent>(0); c.AddAttribute(1, "ChildContent", (RenderFragment)(l => { AddPaginationItem<ShadcnPaginationPrevious>(l, 0, disabled); for (var page = 1; page <= 3; page++) AddPage(l, 10 * page, page, page == (int)current); AddPaginationItem<ShadcnPaginationNext>(l, 50, disabled); })); c.CloseComponent(); })); b.CloseComponent(); };
        return Example("pagination", "Pagination navigation", "Current-page semantics, previous/next labels, links or callbacks, ellipsis, and disabled navigation.", "<ShadcnPagination><ShadcnPaginationContent>...</ShadcnPaginationContent></ShadcnPagination>", preview, [Number("pagination-current", "Current page", current, v => current = Math.Clamp(v, 1, 3)), Toggle("pagination-disabled", "Disable navigation", v => disabled = v)], ["current", "previous", "next", "disabled", "link", "button"]);
    }

    private static ComponentExampleDefinition Resizable()
    {
        var vertical = false; var disabled = false; var collapsible = false;
        RenderFragment preview = b => { b.OpenComponent<ShadcnResizableGroup>(0); b.AddAttribute(1, "Sizes", new[] { 40d, 60d }); b.AddAttribute(2, "Direction", vertical ? ShadcnResizableDirection.Vertical : ShadcnResizableDirection.Horizontal); b.AddAttribute(3, "Disabled", disabled); b.AddAttribute(4, "ChildContent", (RenderFragment)(c => { AddPanel(c, 0, "queue", "Queue", collapsible); c.OpenComponent<ShadcnResizableHandle>(10); c.AddAttribute(11, "WithHandle", true); c.AddAttribute(12, "Label", "Resize queue"); c.CloseComponent(); AddPanel(c, 20, "detail", "Detail", false); })); b.CloseComponent(); };
        return Example("resizable", "Resizable panels", "Pointer and keyboard resizing, live panel IDs, constraints, collapsible panels, persistence, vertical layout, and RTL deltas.", "<ShadcnResizableGroup Sizes=\"Sizes\"><ShadcnResizablePanel Id=\"queue\">...</ShadcnResizablePanel><ShadcnResizableHandle /></ShadcnResizableGroup>", preview, [Toggle("resizable-vertical", "Vertical", v => vertical = v), Toggle("resizable-collapsible", "Collapsible first panel", v => collapsible = v), Toggle("resizable-disabled", "Disabled", v => disabled = v)], ["horizontal", "vertical", "pointer", "keyboard", "constraints", "collapse", "persistence", "rtl"]);
    }

    private static ComponentExampleDefinition ScrollArea()
    {
        var always = true; var horizontal = false;
        RenderFragment preview = b => { b.OpenComponent<ShadcnScrollArea>(0); b.AddAttribute(1, "Type", always ? ShadcnScrollAreaType.Always : ShadcnScrollAreaType.Auto); b.AddAttribute(2, "Style", "height:8rem;width:18rem"); b.AddAttribute(3, "ChildContent", (RenderFragment)(c => { c.OpenComponent<ShadcnScrollAreaViewport>(0); c.AddAttribute(1, "Label", "Material catalog"); c.AddAttribute(2, "ChildContent", Text(string.Join(' ', Enumerable.Repeat("Material ภาษาไทย", 30)))); c.CloseComponent(); c.OpenComponent<ShadcnScrollAreaScrollbar>(10); c.AddAttribute(11, "Orientation", horizontal ? ShadcnScrollAreaOrientation.Horizontal : ShadcnScrollAreaOrientation.Vertical); c.AddAttribute(12, "ChildContent", (RenderFragment)(s => Add<ShadcnScrollAreaThumb>(s, 0))); c.CloseComponent(); Add<ShadcnScrollAreaCorner>(c, 20); })); b.CloseComponent(); };
        return Example("scroll-area", "Native scroll area", "Native focusable scrolling with auto/always visibility, vertical/horizontal thumbs, track click, drag grab offset, content observation, and RTL normalization.", "<ShadcnScrollArea><ShadcnScrollAreaViewport>...</ShadcnScrollAreaViewport><ShadcnScrollAreaScrollbar>...</ShadcnScrollAreaScrollbar></ShadcnScrollArea>", preview, [Toggle("scroll-always", "Always visible", v => always = v, true), Toggle("scroll-horizontal", "Horizontal", v => horizontal = v)], ["auto", "always", "vertical", "horizontal", "drag", "track", "rtl", "keyboard"]);
    }

    private static ComponentExampleDefinition Sidebar()
    {
        var open = true; var right = false; var none = false;
        RenderFragment preview = b => { b.OpenComponent<ShadcnSidebarProvider>(0); b.AddAttribute(1, "Open", open); b.AddAttribute(2, "ChildContent", (RenderFragment)(c => { c.OpenComponent<ShadcnSidebar>(0); c.AddAttribute(1, "Id", "dossier"); c.AddAttribute(2, "Side", right ? ShadcnSidebarSide.Right : ShadcnSidebarSide.Left); c.AddAttribute(3, "Collapsible", none ? ShadcnSidebarCollapsible.None : ShadcnSidebarCollapsible.Icon); c.AddAttribute(4, "Label", "Workspace"); c.AddAttribute(5, "ChildContent", (RenderFragment)(s => { AddText<ShadcnSidebarHeader>(s, 0, "MALIEV"); AddText<ShadcnSidebarContent>(s, 10, "Quotations ภาษาไทย"); })); c.CloseComponent(); c.OpenComponent<ShadcnSidebarTrigger>(10); c.AddAttribute(11, "TargetId", "dossier"); c.CloseComponent(); AddText<ShadcnSidebarInset>(c, 20, "Workspace content"); })); b.CloseComponent(); };
        return Example("sidebar", "Responsive sidebar shell", "Desktop icon/off-canvas/none modes, physical sides, typed multi-sidebar targets, persisted controlled state, mobile modal focus/inert lifecycle, menu variants, and tooltips.", "<ShadcnSidebarProvider><ShadcnSidebar Id=\"workspace\">...</ShadcnSidebar><ShadcnSidebarTrigger TargetId=\"workspace\" /><ShadcnSidebarInset>...</ShadcnSidebarInset></ShadcnSidebarProvider>", preview, [Toggle("sidebar-open", "Expanded", v => open = v, true), Toggle("sidebar-right", "Right side", v => right = v), Toggle("sidebar-none", "Non-collapsible", v => none = v)], ["expanded", "collapsed", "offcanvas", "icon", "none", "mobile-modal", "persistence", "tooltip", "rtl"]);
    }

    private static ComponentExampleDefinition Tabs()
    {
        var value = "overview"; var vertical = false; var manual = false; var force = true;
        RenderFragment preview = b => { b.OpenComponent<ShadcnTabs>(0); b.AddAttribute(1, "Value", value); b.AddAttribute(2, "Orientation", vertical ? ShadcnTabsOrientation.Vertical : ShadcnTabsOrientation.Horizontal); b.AddAttribute(3, "ActivationMode", manual ? ShadcnTabsActivationMode.Manual : ShadcnTabsActivationMode.Automatic); b.AddAttribute(4, "ChildContent", (RenderFragment)(c => { c.OpenComponent<ShadcnTabsList>(0); c.AddAttribute(1, "Label", "Project views"); c.AddAttribute(2, "ChildContent", (RenderFragment)(l => { AddTab(l, 0, "overview", "Overview"); AddTab(l, 10, "history", "History"); })); c.CloseComponent(); AddTabContent(c, 20, "overview", "Current project", force); AddTabContent(c, 30, "history", "Project history", force); })); b.CloseComponent(); };
        return Example("tabs", "Tabs state machine", "Automatic/manual activation, horizontal/vertical roving focus, disabled recovery, controlled value, keep-mounted panels, RTL, and native activation.", "<ShadcnTabs Value=\"overview\"><ShadcnTabsList>...</ShadcnTabsList><ShadcnTabsContent Value=\"overview\">...</ShadcnTabsContent></ShadcnTabs>", preview, [Toggle("tabs-history", "Select history", v => value = v ? "history" : "overview"), Toggle("tabs-vertical", "Vertical", v => vertical = v), Toggle("tabs-manual", "Manual activation", v => manual = v), Toggle("tabs-force", "Keep panels mounted", v => force = v, true)], ["automatic", "manual", "horizontal", "vertical", "force-mount", "disabled-recovery", "rtl", "keyboard"]);
    }

    private static ComponentExampleDefinition Example(string id, string title, string description, string source, RenderFragment preview, IReadOnlyList<ComponentParameterControl> controls, IReadOnlyList<string> tags) => new($"{id}-primary", title, description, source, preview, controls, tags);
    private static ComponentParameterControl Toggle(string id, string label, Action<bool> apply, bool initial = false) => new(id, label, ComponentParameterControlKind.Toggle, initial.ToString(), [], text => apply(bool.Parse(text)));
    private static ComponentParameterControl Number(string id, string label, double value, Action<double> apply) => new(id, label, ComponentParameterControlKind.Number, value.ToString(System.Globalization.CultureInfo.InvariantCulture), [], text => apply(double.Parse(text, System.Globalization.CultureInfo.InvariantCulture)));
    private static RenderFragment Text(string text) => b => b.AddContent(0, text);
    private static void Add<T>(RenderTreeBuilder b, int sequence) where T : IComponent { b.OpenComponent<T>(sequence); b.CloseComponent(); }
    private static void AddText<T>(RenderTreeBuilder b, int sequence, string text) where T : IComponent { b.OpenComponent<T>(sequence); b.AddAttribute(sequence + 1, "ChildContent", Text(text)); b.CloseComponent(); }
    private static void AddAccordionItem(RenderTreeBuilder b, int s, string value, string title, string content) { b.OpenComponent<ShadcnAccordionItem>(s); b.AddAttribute(s + 1, "Value", value); b.AddAttribute(s + 2, "ChildContent", (RenderFragment)(c => { AddText<ShadcnAccordionTrigger>(c, 0, title); AddText<ShadcnAccordionContent>(c, 10, content); })); b.CloseComponent(); }
    private static void AddBreadcrumbLink(RenderTreeBuilder b, int s, string text, string href) { b.OpenComponent<ShadcnBreadcrumbItem>(s); b.AddAttribute(s + 1, "ChildContent", (RenderFragment)(c => { c.OpenComponent<ShadcnBreadcrumbLink>(0); c.AddAttribute(1, "Href", href); c.AddAttribute(2, "ChildContent", Text(text)); c.CloseComponent(); })); b.CloseComponent(); }
    private static void AddPaginationItem<T>(RenderTreeBuilder b, int s, bool disabled) where T : IComponent { b.OpenComponent<ShadcnPaginationItem>(s); b.AddAttribute(s + 1, "ChildContent", (RenderFragment)(c => { c.OpenComponent<T>(0); c.AddAttribute(1, "Disabled", disabled); c.CloseComponent(); })); b.CloseComponent(); }
    private static void AddPage(RenderTreeBuilder b, int s, int page, bool current) { b.OpenComponent<ShadcnPaginationItem>(s); b.AddAttribute(s + 1, "ChildContent", (RenderFragment)(c => { c.OpenComponent<ShadcnPaginationLink>(0); c.AddAttribute(1, "Current", current); c.AddAttribute(2, "ChildContent", Text(page.ToString())); c.CloseComponent(); })); b.CloseComponent(); }
    private static void AddPanel(RenderTreeBuilder b, int s, string id, string text, bool collapsible) { b.OpenComponent<ShadcnResizablePanel>(s); b.AddAttribute(s + 1, "Id", id); b.AddAttribute(s + 2, "MinimumSize", 20d); b.AddAttribute(s + 3, "MaximumSize", 80d); b.AddAttribute(s + 4, "Collapsible", collapsible); b.AddAttribute(s + 5, "ChildContent", Text(text)); b.CloseComponent(); }
    private static void AddTab(RenderTreeBuilder b, int s, string value, string text) { b.OpenComponent<ShadcnTabsTrigger>(s); b.AddAttribute(s + 1, "Value", value); b.AddAttribute(s + 2, "ChildContent", Text(text)); b.CloseComponent(); }
    private static void AddTabContent(RenderTreeBuilder b, int s, string value, string text, bool force) { b.OpenComponent<ShadcnTabsContent>(s); b.AddAttribute(s + 1, "Value", value); b.AddAttribute(s + 2, "ForceMount", force); b.AddAttribute(s + 3, "ChildContent", Text(text)); b.CloseComponent(); }
}
