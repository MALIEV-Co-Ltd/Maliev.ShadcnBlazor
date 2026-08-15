using Maliev.ShadcnBlazor.Components.Disclosure;
using Maliev.ShadcnBlazor.Components.Layout;
using Maliev.ShadcnBlazor.Components.Navigation;
using Maliev.ShadcnBlazor.Components.Navigation.Sidebar;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Rendering;
using Microsoft.AspNetCore.Components.Web;

namespace Maliev.ShadcnBlazor.Showcase.Documentation.Examples;

internal static class DisclosureNavigationExamples
{
    public static IReadOnlyList<ComponentExampleDefinition> Create(string slug) => slug switch
    {
        "accordion" => [Accordion()],
        "breadcrumb" => [Breadcrumb()],
        "collapsible" => [Collapsible()],
        "navigation-menu" => [NavigationMenu()],
        "pagination" => [Pagination()],
        "resizable" => [Resizable()],
        "scroll-area" => [ScrollArea()],
        "sidebar" => [Sidebar()],
        "tabs" => [Tabs()],
        _ => []
    };

    private static ComponentExampleDefinition Accordion()
    {
        var multiple = false; var horizontal = false; var disabled = false;
        RenderFragment preview = b => { b.OpenComponent<ShadcnAccordion>(0); b.AddAttribute(1, "Values", multiple ? new[] { "shipping", "returns" } : new[] { "shipping" }); b.AddAttribute(2, "Multiple", multiple); b.AddAttribute(3, "Orientation", horizontal ? ShadcnAccordionOrientation.Horizontal : ShadcnAccordionOrientation.Vertical); b.AddAttribute(4, "Disabled", disabled); b.AddAttribute(5, "Label", "Quotation questions"); b.AddAttribute(6, "ChildContent", (RenderFragment)(c => { AddAccordionItem(c, 0, "shipping", "What are the delivery options?", "Standard delivery is prepared from our Thailand workshop. The quote includes a dispatch estimate before checkout."); AddAccordionItem(c, 10, "returns", "What is the return policy?", "Returns are accepted within 30 days when the item is unused and in its original packaging."); AddAccordionItem(c, 20, "support", "How can I contact support?", "Send a message from the quotation workspace and the production team will reply with the next available step."); })); b.CloseComponent(); };
        return Example("accordion", "Accordion states", "A realistic quotation FAQ with single or multiple disclosure, orientation, keyboard roving, RTL, and disabled state.", "<ShadcnAccordion Values=\"new[] { \"shipping\" }\" Multiple=\"false\">\n    <ShadcnAccordionItem Value=\"shipping\">\n        <ShadcnAccordionTrigger>What are the delivery options?</ShadcnAccordionTrigger>\n        <ShadcnAccordionContent>Standard delivery is prepared from our Thailand workshop.</ShadcnAccordionContent>\n    </ShadcnAccordionItem>\n    <ShadcnAccordionItem Value=\"returns\">\n        <ShadcnAccordionTrigger>What is the return policy?</ShadcnAccordionTrigger>\n        <ShadcnAccordionContent>Returns are accepted within 30 days when the item is unused and in its original packaging.</ShadcnAccordionContent>\n    </ShadcnAccordionItem>\n    <ShadcnAccordionItem Value=\"support\">\n        <ShadcnAccordionTrigger>How can I contact support?</ShadcnAccordionTrigger>\n        <ShadcnAccordionContent>Send a message from the quotation workspace and the production team will reply with the next available step.</ShadcnAccordionContent>\n    </ShadcnAccordionItem>\n</ShadcnAccordion>", preview, [Toggle("accordion-multiple", "Multiple", v => multiple = v), Toggle("accordion-horizontal", "Horizontal", v => horizontal = v), Toggle("accordion-disabled", "Disabled", v => disabled = v)], ["single", "multiple", "horizontal", "vertical", "keyboard", "rtl", "disabled"]);
    }

    private static ComponentExampleDefinition Breadcrumb()
    {
        var ellipsis = true;
        RenderFragment preview = b => { b.OpenComponent<ShadcnBreadcrumb>(0); b.AddAttribute(1, "Label", "Quotation breadcrumb"); b.AddAttribute(2, "ChildContent", (RenderFragment)(c => { c.OpenComponent<ShadcnBreadcrumbList>(0); c.AddAttribute(1, "ChildContent", (RenderFragment)(l => { AddBreadcrumbLink(l, 0, "Home", "/"); Add<ShadcnBreadcrumbSeparator>(l, 10); AddBreadcrumbLink(l, 20, "Projects", "/projects"); Add<ShadcnBreadcrumbSeparator>(l, 30); if (ellipsis) { Add<ShadcnBreadcrumbEllipsis>(l, 40); Add<ShadcnBreadcrumbSeparator>(l, 50); } AddBreadcrumbLink(l, 60, "Quotations", "/projects/quotations"); Add<ShadcnBreadcrumbSeparator>(l, 70); l.OpenComponent<ShadcnBreadcrumbItem>(80); l.AddAttribute(81, "ChildContent", (RenderFragment)(i => AddText<ShadcnBreadcrumbPage>(i, 0, "Quotation #4189"))); l.CloseComponent(); })); c.CloseComponent(); })); b.CloseComponent(); };
        return Example("breadcrumb", "Breadcrumb composition", "Navigate a deep quotation workspace with multiple levels, links, separators, optional ellipsis, and current-page semantics.", "<ShadcnBreadcrumb Label=\"Quotation breadcrumb\">\n    <ShadcnBreadcrumbList>\n        <ShadcnBreadcrumbItem><ShadcnBreadcrumbLink Href=\"/\">Home</ShadcnBreadcrumbLink></ShadcnBreadcrumbItem>\n        <ShadcnBreadcrumbSeparator />\n        <ShadcnBreadcrumbItem><ShadcnBreadcrumbLink Href=\"/projects\">Projects</ShadcnBreadcrumbLink></ShadcnBreadcrumbItem>\n        <ShadcnBreadcrumbSeparator />\n        <ShadcnBreadcrumbEllipsis />\n        <ShadcnBreadcrumbSeparator />\n        <ShadcnBreadcrumbItem><ShadcnBreadcrumbPage>Quotation #4189</ShadcnBreadcrumbPage></ShadcnBreadcrumbItem>\n    </ShadcnBreadcrumbList>\n</ShadcnBreadcrumb>", preview, [Toggle("breadcrumb-ellipsis", "Ellipsis", v => ellipsis = v, true)], ["links", "separator", "ellipsis", "current-page", "rtl"]);
    }

    private static ComponentExampleDefinition Collapsible()
    {
        var open = false; var disabled = false;
        RenderFragment preview = b => { b.OpenComponent<ShadcnCollapsible>(0); b.AddAttribute(1, "Open", open); b.AddAttribute(2, "Disabled", disabled); b.AddAttribute(3, "ChildContent", (RenderFragment)(c => { AddText<ShadcnCollapsibleTrigger>(c, 0, "Project files"); c.OpenComponent<ShadcnCollapsibleContent>(10); c.AddAttribute(11, "ChildContent", FilesContent()); c.CloseComponent(); })); b.CloseComponent(); };
        return Example("collapsible", "Collapsible disclosure", "Reveal a project file list on demand with a real disclosure trigger, keyboard focus, and disabled state.", "<ShadcnCollapsible Open=\"false\">\n    <ShadcnCollapsibleTrigger>Project files</ShadcnCollapsibleTrigger>\n    <ShadcnCollapsibleContent>\n        <ul><li>Drawing.step</li><li>Inspection-report.pdf</li><li>Revision-notes.md</li></ul>\n    </ShadcnCollapsibleContent>\n</ShadcnCollapsible>", preview, [Toggle("collapsible-open", "Open", v => open = v), Toggle("collapsible-disabled", "Disabled", v => disabled = v)], ["open", "closed", "disabled", "controlled"]);
    }

    private static ComponentExampleDefinition NavigationMenu()
    {
        string? value = null; var vertical = false; var disabled = false;
        RenderFragment preview = b => { b.OpenComponent<ShadcnNavigationMenu>(0); b.AddAttribute(1, "Value", value); b.AddAttribute(2, "Orientation", vertical ? ShadcnNavigationMenuOrientation.Vertical : ShadcnNavigationMenuOrientation.Horizontal); b.AddAttribute(3, "Disabled", disabled); b.AddAttribute(4, "Label", "Workspace navigation"); b.AddAttribute(5, "ChildContent", (RenderFragment)(c => { c.OpenComponent<ShadcnNavigationMenuList>(0); c.AddAttribute(1, "ChildContent", (RenderFragment)(l => { AddNavigationItem(l, 0, "services", "Services", [("CNC machining", "#cnc"), ("Finishing", "#finishing"), ("Quality inspection", "#quality")]); AddNavigationItem(l, 10, "resources", "Resources", [("Material library", "#materials"), ("Guides", "#guides")]); AddNavigationItem(l, 20, "account", "Account", [("Team settings", "#team"), ("Billing", "#billing")]); })); c.CloseComponent(); Add<ShadcnNavigationMenuIndicator>(c, 10); Add<ShadcnNavigationMenuViewport>(c, 20); })); b.CloseComponent(); };
        return Example("navigation-menu", "Navigation menu portal", "Explore a realistic workspace navigation with delayed hover, click, keyboard focus, collision-aware viewport, RTL, and disabled behavior.", "<ShadcnNavigationMenu Label=\"Workspace navigation\">\n    <ShadcnNavigationMenuList>\n        <ShadcnNavigationMenuItem Value=\"services\">\n            <ShadcnNavigationMenuTrigger>Services</ShadcnNavigationMenuTrigger>\n            <ShadcnNavigationMenuContent>...</ShadcnNavigationMenuContent>\n        </ShadcnNavigationMenuItem>\n    </ShadcnNavigationMenuList>\n    <ShadcnNavigationMenuViewport />\n</ShadcnNavigationMenu>", preview, [Toggle("navigation-open", "Open Services", v => value = v ? "services" : null), Toggle("navigation-vertical", "Vertical", v => vertical = v), Toggle("navigation-disabled", "Disabled", v => disabled = v)], ["open", "closed", "hover", "keyboard", "portal", "collision", "rtl"]);
    }

    private static ComponentExampleDefinition Pagination()
    {
        var current = 2d; var disabled = false;
        RenderFragment preview = b => { b.OpenComponent<ShadcnPagination>(0); b.AddAttribute(1, "Label", "Quotation pages"); b.AddAttribute(2, "ChildContent", (RenderFragment)(c => { c.OpenComponent<ShadcnPaginationContent>(0); c.AddAttribute(1, "ChildContent", (RenderFragment)(l => { AddPaginationDirection<ShadcnPaginationPrevious>(l, 0, disabled, () => current = Math.Max(1, current - 1)); AddPage(l, 10, 1, 1 == (int)current, () => current = 1); AddPage(l, 20, 2, 2 == (int)current, () => current = 2); AddPage(l, 30, 3, 3 == (int)current, () => current = 3); Add<ShadcnPaginationEllipsis>(l, 40); AddPage(l, 50, 8, 8 == (int)current, () => current = 8); AddPaginationDirection<ShadcnPaginationNext>(l, 60, disabled, () => current = Math.Min(8, current + 1)); })); c.CloseComponent(); })); b.CloseComponent(); };
        return Example("pagination", "Pagination navigation", "Browse a multi-page quotation list with current-page state, previous/next buttons, ellipsis, and disabled navigation.", "<ShadcnPagination Label=\"Quotation pages\">\n    <ShadcnPaginationContent>\n        <ShadcnPaginationPrevious />\n        <ShadcnPaginationLink Current=\"true\">1</ShadcnPaginationLink>\n        <ShadcnPaginationLink>2</ShadcnPaginationLink>\n        <ShadcnPaginationLink>3</ShadcnPaginationLink>\n        <ShadcnPaginationEllipsis />\n        <ShadcnPaginationLink>8</ShadcnPaginationLink>\n        <ShadcnPaginationNext />\n    </ShadcnPaginationContent>\n</ShadcnPagination>", preview, [Number("pagination-current", "Current page", current, v => current = Math.Clamp(v, 1, 8)), Toggle("pagination-disabled", "Disable navigation", v => disabled = v)], ["current", "previous", "next", "ellipsis", "disabled", "link", "button"]);
    }

    private static ComponentExampleDefinition Resizable()
    {
        var vertical = false; var disabled = false; var collapsible = false;
        RenderFragment preview = b => { b.OpenComponent<ShadcnResizableGroup>(0); b.AddAttribute(1, "Sizes", new[] { 40d, 60d }); b.AddAttribute(2, "Direction", vertical ? ShadcnResizableDirection.Vertical : ShadcnResizableDirection.Horizontal); b.AddAttribute(3, "Disabled", disabled); b.AddAttribute(4, "ChildContent", (RenderFragment)(c => { AddPanel(c, 0, "queue", "Incoming queue", collapsible, "3 quotations waiting"); c.OpenComponent<ShadcnResizableHandle>(10); c.AddAttribute(11, "WithHandle", true); c.AddAttribute(12, "Label", "Resize queue and detail panels"); c.CloseComponent(); AddPanel(c, 20, "detail", "Quotation detail", false, "Select an item to review its production timeline."); })); b.CloseComponent(); };
        return Example("resizable", "Resizable panels", "Pointer and keyboard resizing, live panel IDs, constraints, collapsible panels, persistence, vertical layout, and RTL deltas.", "<ShadcnResizableGroup Sizes=\"Sizes\"><ShadcnResizablePanel Id=\"queue\">...</ShadcnResizablePanel><ShadcnResizableHandle /></ShadcnResizableGroup>", preview, [Toggle("resizable-vertical", "Vertical", v => vertical = v), Toggle("resizable-collapsible", "Collapsible first panel", v => collapsible = v), Toggle("resizable-disabled", "Disabled", v => disabled = v)], ["horizontal", "vertical", "pointer", "keyboard", "constraints", "collapse", "persistence", "rtl"]);
    }

    private static ComponentExampleDefinition ScrollArea()
    {
        var always = true; var horizontal = false;
        RenderFragment preview = b => { b.OpenComponent<ShadcnScrollArea>(0); b.AddAttribute(1, "Type", always ? ShadcnScrollAreaType.Always : ShadcnScrollAreaType.Auto); b.AddAttribute(2, "Style", "height:12rem;width:min(100%,24rem)"); b.AddAttribute(3, "ChildContent", (RenderFragment)(c => { c.OpenComponent<ShadcnScrollAreaViewport>(0); c.AddAttribute(1, "Label", "Material catalog"); c.AddAttribute(2, "ChildContent", MaterialCatalog(horizontal)); c.CloseComponent(); c.OpenComponent<ShadcnScrollAreaScrollbar>(10); c.AddAttribute(11, "Orientation", horizontal ? ShadcnScrollAreaOrientation.Horizontal : ShadcnScrollAreaOrientation.Vertical); c.AddAttribute(12, "ChildContent", (RenderFragment)(s => Add<ShadcnScrollAreaThumb>(s, 0))); c.CloseComponent(); Add<ShadcnScrollAreaCorner>(c, 20); })); b.CloseComponent(); };
        return Example("scroll-area", "Native scroll area", "Native focusable scrolling with auto/always visibility, vertical/horizontal thumbs, track click, drag grab offset, content observation, and RTL normalization.", "<ShadcnScrollArea><ShadcnScrollAreaViewport>...</ShadcnScrollAreaViewport><ShadcnScrollAreaScrollbar>...</ShadcnScrollAreaScrollbar></ShadcnScrollArea>", preview, [Toggle("scroll-always", "Always visible", v => always = v, true), Toggle("scroll-horizontal", "Horizontal", v => horizontal = v)], ["auto", "always", "vertical", "horizontal", "drag", "track", "rtl", "keyboard"]);
    }

    private static ComponentExampleDefinition Sidebar()
    {
        var open = true; var right = false; var none = false;
        RenderFragment preview = b => { b.OpenComponent<ShadcnSidebarProvider>(0); b.AddAttribute(1, "Open", open); b.AddAttribute(2, "ChildContent", (RenderFragment)(c => { c.OpenComponent<ShadcnSidebar>(0); c.AddAttribute(1, "Id", "dossier"); c.AddAttribute(2, "Side", right ? ShadcnSidebarSide.Right : ShadcnSidebarSide.Left); c.AddAttribute(3, "Collapsible", none ? ShadcnSidebarCollapsible.None : ShadcnSidebarCollapsible.Icon); c.AddAttribute(4, "Label", "Workspace"); c.AddAttribute(5, "ChildContent", SidebarContent()); c.CloseComponent(); c.OpenComponent<ShadcnSidebarInset>(10); c.AddAttribute(11, "ChildContent", (RenderFragment)(inset => { inset.OpenComponent<ShadcnSidebarTrigger>(0); inset.AddAttribute(1, "TargetId", "dossier"); inset.CloseComponent(); inset.OpenElement(10, "div"); inset.AddAttribute(11, "class", "showcase-sidebar-main"); inset.OpenElement(12, "h3"); inset.AddContent(13, "Quotation workspace"); inset.CloseElement(); inset.OpenElement(14, "p"); inset.AddContent(15, "Review active quotations and production handoffs."); inset.CloseElement(); inset.CloseElement(); })); c.CloseComponent(); })); b.CloseComponent(); };
        return Example("sidebar", "Responsive sidebar shell", "A realistic quotation workspace with navigation groups, active state, responsive collapse, physical sides, and mobile modal behavior.", "<ShadcnSidebarProvider Open=\"true\">\n    <ShadcnSidebar Id=\"workspace\" Collapsible=\"ShadcnSidebarCollapsible.Icon\">...</ShadcnSidebar>\n    <ShadcnSidebarInset><ShadcnSidebarTrigger TargetId=\"workspace\" />...</ShadcnSidebarInset>\n</ShadcnSidebarProvider>", preview, [Toggle("sidebar-open", "Expanded", v => open = v, true), Toggle("sidebar-right", "Right side", v => right = v), Toggle("sidebar-none", "Non-collapsible", v => none = v)], ["expanded", "collapsed", "offcanvas", "icon", "none", "mobile-modal", "persistence", "tooltip", "rtl"]);
    }

    private static ComponentExampleDefinition Tabs()
    {
        var value = "overview"; var vertical = false; var manual = false; var force = true;
        RenderFragment preview = b => { b.OpenComponent<ShadcnTabs>(0); b.AddAttribute(1, "Value", value); b.AddAttribute(2, "Orientation", vertical ? ShadcnTabsOrientation.Vertical : ShadcnTabsOrientation.Horizontal); b.AddAttribute(3, "ActivationMode", manual ? ShadcnTabsActivationMode.Manual : ShadcnTabsActivationMode.Automatic); b.AddAttribute(4, "ChildContent", (RenderFragment)(c => { c.OpenComponent<ShadcnTabsList>(0); c.AddAttribute(1, "Label", "Quotation views"); c.AddAttribute(2, "ChildContent", (RenderFragment)(l => { AddTab(l, 0, "overview", "Overview"); AddTab(l, 10, "history", "History"); AddTab(l, 20, "files", "Files"); AddTab(l, 30, "activity", "Activity"); })); c.CloseComponent(); AddTabContent(c, 40, "overview", "Current quotation · 3 parts ready for review.", force); AddTabContent(c, 50, "history", "Project history · Revision B was approved yesterday.", force); AddTabContent(c, 60, "files", "Files · Drawing.step, inspection-report.pdf, revision-notes.md.", force); AddTabContent(c, 70, "activity", "Activity · The production team left two updates.", force); })); b.CloseComponent(); };
        return Example("tabs", "Tabs state machine", "Switch between realistic quotation views with automatic/manual activation, horizontal/vertical roving focus, controlled value, keep-mounted panels, RTL, and native activation.", "<ShadcnTabs Value=\"overview\">\n    <ShadcnTabsList Label=\"Quotation views\">\n        <ShadcnTabsTrigger Value=\"overview\">Overview</ShadcnTabsTrigger>\n        <ShadcnTabsTrigger Value=\"history\">History</ShadcnTabsTrigger>\n        <ShadcnTabsTrigger Value=\"files\">Files</ShadcnTabsTrigger>\n        <ShadcnTabsTrigger Value=\"activity\">Activity</ShadcnTabsTrigger>\n    </ShadcnTabsList>\n    <ShadcnTabsContent Value=\"overview\">Current quotation</ShadcnTabsContent>\n</ShadcnTabs>", preview, [Toggle("tabs-history", "Select history", v => value = v ? "history" : "overview"), Toggle("tabs-vertical", "Vertical", v => vertical = v), Toggle("tabs-manual", "Manual activation", v => manual = v), Toggle("tabs-force", "Keep panels mounted", v => force = v, true)], ["automatic", "manual", "horizontal", "vertical", "force-mount", "disabled-recovery", "rtl", "keyboard"]);
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
    private static void AddPaginationDirection<T>(RenderTreeBuilder b, int s, bool disabled, Action apply) where T : IComponent { b.OpenComponent<ShadcnPaginationItem>(s); b.AddAttribute(s + 1, "ChildContent", (RenderFragment)(c => { c.OpenComponent<T>(0); c.AddAttribute(1, "Disabled", disabled); c.AddAttribute(2, "OnClick", EventCallback.Factory.Create<MouseEventArgs>(new object(), _ => { apply(); return Task.CompletedTask; })); c.CloseComponent(); })); b.CloseComponent(); }
    private static void AddPage(RenderTreeBuilder b, int s, int page, bool current, Action apply) { b.OpenComponent<ShadcnPaginationItem>(s); b.AddAttribute(s + 1, "ChildContent", (RenderFragment)(c => { c.OpenComponent<ShadcnPaginationLink>(0); c.AddAttribute(1, "Current", current); c.AddAttribute(2, "Href", $"#quotation-page-{page}"); c.AddAttribute(3, "OnClick", EventCallback.Factory.Create<MouseEventArgs>(new object(), _ => { apply(); return Task.CompletedTask; })); c.AddAttribute(4, "ChildContent", Text(page.ToString())); c.CloseComponent(); })); b.CloseComponent(); }
    private static void AddPanel(RenderTreeBuilder b, int s, string id, string title, bool collapsible, string summary) { b.OpenComponent<ShadcnResizablePanel>(s); b.AddAttribute(s + 1, "Id", id); b.AddAttribute(s + 2, "MinimumSize", 20d); b.AddAttribute(s + 3, "MaximumSize", 80d); b.AddAttribute(s + 4, "Collapsible", collapsible); b.AddAttribute(s + 5, "ChildContent", PanelContent(title, summary)); b.CloseComponent(); }
    private static void AddTab(RenderTreeBuilder b, int s, string value, string text) { b.OpenComponent<ShadcnTabsTrigger>(s); b.AddAttribute(s + 1, "Value", value); b.AddAttribute(s + 2, "ChildContent", Text(text)); b.CloseComponent(); }
    private static void AddTabContent(RenderTreeBuilder b, int s, string value, string text, bool force) { b.OpenComponent<ShadcnTabsContent>(s); b.AddAttribute(s + 1, "Value", value); b.AddAttribute(s + 2, "ForceMount", force); b.AddAttribute(s + 3, "ChildContent", Text(text)); b.CloseComponent(); }

    private static RenderFragment FilesContent() => builder =>
    {
        builder.OpenElement(0, "ul"); builder.AddAttribute(1, "class", "showcase-disclosure-file-list");
        foreach (var file in new[] { ("Drawing.step", "STEP · 2.4 MB"), ("Inspection-report.pdf", "PDF · 860 KB"), ("Revision-notes.md", "Markdown · 4 KB") })
        {
            builder.OpenElement(10, "li"); builder.OpenElement(11, "strong"); builder.AddContent(12, file.Item1); builder.CloseElement(); builder.OpenElement(13, "span"); builder.AddContent(14, file.Item2); builder.CloseElement(); builder.CloseElement();
        }
        builder.CloseElement();
    };

    private static void AddNavigationItem(RenderTreeBuilder b, int s, string value, string title, IReadOnlyList<(string Label, string Href)> links)
    {
        b.OpenComponent<ShadcnNavigationMenuItem>(s); b.AddAttribute(s + 1, "Value", value); b.AddAttribute(s + 2, "ChildContent", (RenderFragment)(item =>
        {
            AddText<ShadcnNavigationMenuTrigger>(item, 0, title);
            item.OpenComponent<ShadcnNavigationMenuContent>(10); item.AddAttribute(11, "ChildContent", (RenderFragment)(content =>
            {
                content.OpenElement(0, "div"); content.AddAttribute(1, "class", "showcase-navigation-menu-grid");
                foreach (var link in links)
                {
                    content.OpenComponent<ShadcnNavigationMenuLink>(10); content.AddAttribute(11, "Href", link.Href); content.AddAttribute(12, "ChildContent", Text(link.Label)); content.CloseComponent();
                }
                content.CloseElement();
            })); item.CloseComponent();
        })); b.CloseComponent();
    }

    private static RenderFragment MaterialCatalog(bool horizontal) => builder =>
    {
        builder.OpenElement(0, "div"); builder.AddAttribute(1, "class", horizontal ? "showcase-material-list showcase-material-list--wide" : "showcase-material-list");
        foreach (var material in new[] { ("Aluminum 6061", "Lightweight · CNC"), ("Stainless 316L", "Corrosion resistant · CNC"), ("PEEK", "High temperature · Polymer"), ("ABS", "Impact resistant · Polymer"), ("Titanium Grade 5", "High strength · CNC"), ("Brass C360", "Machinable · CNC") })
        {
            builder.OpenElement(10, "div"); builder.AddAttribute(11, "class", "showcase-material-row"); builder.OpenElement(12, "strong"); builder.AddContent(13, material.Item1); builder.CloseElement(); builder.OpenElement(14, "span"); builder.AddContent(15, material.Item2); builder.CloseElement(); builder.CloseElement();
        }
        builder.CloseElement();
    };

    private static RenderFragment SidebarContent() => builder =>
    {
        builder.OpenComponent<ShadcnSidebarHeader>(0); builder.AddAttribute(1, "ChildContent", (RenderFragment)(header =>
        {
            header.OpenElement(0, "div"); header.AddAttribute(1, "class", "showcase-sidebar-brand"); header.OpenElement(2, "strong"); header.AddContent(3, "MALIEV"); header.CloseElement(); header.OpenElement(4, "span"); header.AddContent(5, "Quotation workspace"); header.CloseElement(); header.CloseElement();
        })); builder.CloseComponent();
        builder.OpenComponent<ShadcnSidebarContent>(10); builder.AddAttribute(11, "ChildContent", (RenderFragment)(content =>
        {
            AddSidebarGroup(content, 0, "Workspace", [("Overview", true), ("Quotations", false), ("Files", false)]);
            AddSidebarGroup(content, 10, "Manage", [("Materials", false), ("Team members", false), ("Settings", false)]);
        })); builder.CloseComponent();
        builder.OpenComponent<ShadcnSidebarFooter>(20); builder.AddAttribute(21, "ChildContent", (RenderFragment)(footer =>
        {
            footer.OpenElement(0, "div"); footer.AddAttribute(1, "class", "showcase-sidebar-user"); footer.OpenElement(2, "strong"); footer.AddContent(3, "Natth"); footer.CloseElement(); footer.OpenElement(4, "span"); footer.AddContent(5, "natth@example.com"); footer.CloseElement(); footer.CloseElement();
        })); builder.CloseComponent();
    };

    private static void AddSidebarGroup(RenderTreeBuilder b, int s, string label, IReadOnlyList<(string Text, bool Active)> entries)
    {
        b.OpenComponent<ShadcnSidebarGroup>(s); b.AddAttribute(s + 1, "ChildContent", (RenderFragment)(group =>
        {
            AddText<ShadcnSidebarGroupLabel>(group, 0, label);
            group.OpenComponent<ShadcnSidebarGroupContent>(10); group.AddAttribute(11, "ChildContent", (RenderFragment)(content =>
            {
                content.OpenComponent<ShadcnSidebarMenu>(0); content.AddAttribute(1, "ChildContent", (RenderFragment)(menu =>
                {
                    var sequence = 0;
                    foreach (var entry in entries)
                    {
                        menu.OpenComponent<ShadcnSidebarMenuItem>(sequence); menu.AddAttribute(sequence + 1, "ChildContent", (RenderFragment)(item => AddText(item, 0, entry.Text, entry.Active))); menu.CloseComponent(); sequence += 10;
                    }
                })); content.CloseComponent();
            })); group.CloseComponent();
        })); b.CloseComponent();
    }

    private static void AddText(RenderTreeBuilder b, int sequence, string text, bool active)
    {
        b.OpenComponent<ShadcnSidebarMenuButton>(sequence); b.AddAttribute(sequence + 1, "Active", active); b.AddAttribute(sequence + 2, "Tooltip", text); b.AddAttribute(sequence + 3, "ChildContent", Text(text)); b.CloseComponent();
    }

    private static RenderFragment PanelContent(string title, string summary) => builder =>
    {
        builder.OpenElement(0, "div"); builder.AddAttribute(1, "class", "showcase-resizable-panel-content"); builder.OpenElement(2, "strong"); builder.AddContent(3, title); builder.CloseElement(); builder.OpenElement(4, "p"); builder.AddContent(5, summary); builder.CloseElement();
        builder.OpenElement(6, "span"); builder.AddContent(7, "Drag the handle or use arrow keys"); builder.CloseElement(); builder.CloseElement();
    };
}
