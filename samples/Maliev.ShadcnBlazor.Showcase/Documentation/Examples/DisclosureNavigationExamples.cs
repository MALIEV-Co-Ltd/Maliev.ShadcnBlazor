using Maliev.ShadcnBlazor.Components.Disclosure;
using Maliev.ShadcnBlazor.Components.Layout;
using Maliev.ShadcnBlazor.Components.Navigation;
using Maliev.ShadcnBlazor.Components.Navigation.Sidebar;
using Maliev.ShadcnBlazor.Showcase.Components.Documentation;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Rendering;
using Microsoft.AspNetCore.Components.Web;

namespace Maliev.ShadcnBlazor.Showcase.Documentation.Examples;

internal static class DisclosureNavigationExamples
{
    public static IReadOnlyList<ComponentExampleDefinition> Create(string slug) => slug switch
    {
        "accordion" => [AccordionPolished()],
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

    private static ComponentExampleDefinition AccordionPolished()
    {
        var multiple = false; var horizontal = false; var disabled = false;
        RenderFragment preview = b =>
        {
            b.OpenComponent<ShadcnAccordion>(0);
            b.AddAttribute(1, "Values", multiple ? new[] { "shipping", "returns" } : new[] { "shipping" });
            b.AddAttribute(2, "Multiple", multiple);
            b.AddAttribute(3, "Orientation", horizontal ? ShadcnAccordionOrientation.Horizontal : ShadcnAccordionOrientation.Vertical);
            b.AddAttribute(4, "Disabled", disabled);
            b.AddAttribute(5, "Label", "Quotation questions");
            b.AddAttribute(6, "ChildContent", (RenderFragment)(c =>
            {
                AddAccordionItem(c, 0, "shipping", "What are the delivery options?", ShippingAccordionContent());
                AddAccordionItem(c, 10, "returns", "What is the return policy?", ReturnsAccordionContent());
                AddAccordionItem(c, 20, "support", "How can I contact support?", SupportAccordionContent());
            }));
            b.CloseComponent();
        };
        const string source = """
<ShadcnAccordion Values='new[] { "shipping" }' Multiple="false" Label="Quotation questions">
    <ShadcnAccordionItem Value="shipping">
        <ShadcnAccordionTrigger>What are the delivery options?</ShadcnAccordionTrigger>
        <ShadcnAccordionContent>
            <p>Choose the handoff that fits your production deadline.</p>
            <ul>
                <li>Standard delivery — dispatch estimate included before checkout.</li>
                <li>Express delivery — priority workshop queue when available.</li>
            </ul>
        </ShadcnAccordionContent>
    </ShadcnAccordionItem>
    <ShadcnAccordionItem Value="returns">
        <ShadcnAccordionTrigger>What is the return policy?</ShadcnAccordionTrigger>
        <ShadcnAccordionContent>
            <p>Returns are accepted within 30 days when the item is unused and in its original packaging.</p>
            <p>Include the quotation number so the team can route the request quickly.</p>
            <p>Revision notes help production review the requested change.</p>
        </ShadcnAccordionContent>
    </ShadcnAccordionItem>
    <ShadcnAccordionItem Value="support">
        <ShadcnAccordionTrigger>How can I contact support?</ShadcnAccordionTrigger>
        <ShadcnAccordionContent>
            <p>Send a message from the quotation workspace and the production team will reply with the next available step.</p>
            <ul>
                <li>Ask about materials, finishing, or delivery timing.</li>
                <li>Attach a drawing when the requested revision needs review.</li>
            </ul>
        </ShadcnAccordionContent>
    </ShadcnAccordionItem>
</ShadcnAccordion>
""";
        return Example("accordion", "Accordion states", "A realistic quotation FAQ with single or multiple disclosure, orientation, keyboard roving, RTL, and disabled state.", source, preview, [Toggle("accordion-multiple", "Multiple", v => multiple = v), Toggle("accordion-horizontal", "Horizontal", v => horizontal = v), Toggle("accordion-disabled", "Disabled", v => disabled = v)], ["single", "multiple", "horizontal", "vertical", "keyboard", "rtl", "disabled"]);
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
        RenderFragment preview = b =>
        {
            b.OpenComponent<NavigationMenuDossierPreview>(0);
            b.AddAttribute(1, nameof(NavigationMenuDossierPreview.OpenValue), value);
            b.AddAttribute(2, nameof(NavigationMenuDossierPreview.Orientation), vertical ? ShadcnNavigationMenuOrientation.Vertical : ShadcnNavigationMenuOrientation.Horizontal);
            b.AddAttribute(3, nameof(NavigationMenuDossierPreview.Disabled), disabled);
            b.CloseComponent();
        };
        string Source()
        {
            var state = value is null ? string.Empty : " Value=\"getting-started\"";
            var orientation = vertical ? " Orientation=\"ShadcnNavigationMenuOrientation.Vertical\"" : string.Empty;
            var disabledAttribute = disabled ? " Disabled=\"true\"" : string.Empty;
            return $"""
<div class="showcase-navigation-menu">
<ShadcnNavigationMenu Label="Documentation navigation"{state}{orientation}{disabledAttribute}>
    <ShadcnNavigationMenuList>
        <ShadcnNavigationMenuItem Value="getting-started">
            <ShadcnNavigationMenuTrigger>Getting started</ShadcnNavigationMenuTrigger>
            <ShadcnNavigationMenuContent>
                <ul class="showcase-navigation-menu__list">
                    <li><ShadcnNavigationMenuLink Href="#overview"><span class="showcase-navigation-menu__entry"><strong>Overview</strong><span>Understand the component library and its accessibility defaults.</span></span></ShadcnNavigationMenuLink></li>
                    <li><ShadcnNavigationMenuLink Href="#installation"><span class="showcase-navigation-menu__entry"><strong>Installation</strong><span>Install the package and register its theme services.</span></span></ShadcnNavigationMenuLink></li>
                    <li><ShadcnNavigationMenuLink Href="#theming"><span class="showcase-navigation-menu__entry"><strong>Theme setup</strong><span>Apply semantic tokens for light, dark, and RTL layouts.</span></span></ShadcnNavigationMenuLink></li>
                </ul>
            </ShadcnNavigationMenuContent>
        </ShadcnNavigationMenuItem>
        <ShadcnNavigationMenuItem Value="components" Class="showcase-navigation-menu__wide-item">
            <ShadcnNavigationMenuTrigger>Components</ShadcnNavigationMenuTrigger>
            <ShadcnNavigationMenuContent>
                <ul class="showcase-navigation-menu__component-grid">
                    <li><ShadcnNavigationMenuLink Href="#alert-dialog"><span class="showcase-navigation-menu__entry"><strong>Alert Dialog</strong><span>Confirm a destructive or sensitive action.</span></span></ShadcnNavigationMenuLink></li>
                    <li><ShadcnNavigationMenuLink Href="#hover-card"><span class="showcase-navigation-menu__entry"><strong>Hover Card</strong><span>Preview supporting information behind a link.</span></span></ShadcnNavigationMenuLink></li>
                    <li><ShadcnNavigationMenuLink Href="#progress"><span class="showcase-navigation-menu__entry"><strong>Progress</strong><span>Communicate determinate and indeterminate work.</span></span></ShadcnNavigationMenuLink></li>
                    <li><ShadcnNavigationMenuLink Href="#scroll-area"><span class="showcase-navigation-menu__entry"><strong>Scroll Area</strong><span>Keep long content within a stable viewport.</span></span></ShadcnNavigationMenuLink></li>
                    <li><ShadcnNavigationMenuLink Href="#tabs"><span class="showcase-navigation-menu__entry"><strong>Tabs</strong><span>Switch between related views without leaving the page.</span></span></ShadcnNavigationMenuLink></li>
                    <li><ShadcnNavigationMenuLink Href="#tooltip"><span class="showcase-navigation-menu__entry"><strong>Tooltip</strong><span>Explain compact controls on hover and focus.</span></span></ShadcnNavigationMenuLink></li>
                </ul>
            </ShadcnNavigationMenuContent>
        </ShadcnNavigationMenuItem>
        <ShadcnNavigationMenuItem Value="status">
            <ShadcnNavigationMenuTrigger>Project status</ShadcnNavigationMenuTrigger>
            <ShadcnNavigationMenuContent>
                <ul class="showcase-navigation-menu__status-list">
                    <li><ShadcnNavigationMenuLink Href="#backlog" Class="showcase-navigation-menu__status-link"><svg viewBox="0 0 24 24" aria-hidden="true"><circle cx="12" cy="12" r="9" /><path d="M12 8v5m0 3h.01" /></svg><span>Backlog</span></ShadcnNavigationMenuLink></li>
                    <li><ShadcnNavigationMenuLink Href="#review" Class="showcase-navigation-menu__status-link"><svg viewBox="0 0 24 24" aria-hidden="true"><circle cx="12" cy="12" r="9" stroke-dasharray="4 3" /></svg><span>In review</span></ShadcnNavigationMenuLink></li>
                    <li><ShadcnNavigationMenuLink Href="#ready" Class="showcase-navigation-menu__status-link"><svg viewBox="0 0 24 24" aria-hidden="true"><circle cx="12" cy="12" r="9" /><path d="m8 12 2.5 2.5L16 9" /></svg><span>Ready</span></ShadcnNavigationMenuLink></li>
                </ul>
            </ShadcnNavigationMenuContent>
        </ShadcnNavigationMenuItem>
        <ShadcnNavigationMenuItem Value="documentation">
            <ShadcnNavigationMenuLink Href="#usage" Class="shadcn-navigation-menu-link--trigger">Documentation</ShadcnNavigationMenuLink>
        </ShadcnNavigationMenuItem>
    </ShadcnNavigationMenuList>
    <ShadcnNavigationMenuIndicator />
    <ShadcnNavigationMenuViewport />
</ShadcnNavigationMenu>
</div>
""";
        }
        var example = Example("navigation-menu", "Responsive application navigation", "Explore a realistic component workspace with descriptive menus, project-status icons, direct links, delayed pointer disclosure, keyboard focus, collision-aware viewport, RTL, and mobile adaptation.", Source(), preview, [Toggle("navigation-open", "Open getting started", v => value = v ? "getting-started" : null), Toggle("navigation-vertical", "Vertical", v => vertical = v), Toggle("navigation-disabled", "Disabled", v => disabled = v)], ["pointer", "keyboard", "responsive", "outside-press", "portal", "collision", "rtl", "reduced-motion"]);
        return example with { RazorSourceProvider = Source };
    }

    private static ComponentExampleDefinition Pagination()
    {
        const int totalPages = 12;
        var current = 2d;
        var visible = 5d;
        var disabled = false;

        string Source() => $$"""
<section class="quotation-pagination" aria-labelledby="quotation-pagination-title">
    <div>
        <h3 id="quotation-pagination-title">Production quotations</h3>
        <p>Page {{(int)current}} of {{totalPages}} · 96 quotations</p>
    </div>
    <ShadcnPagination Label="Production quotation pages">
        <ShadcnPaginationPages TotalPages="{{totalPages}}"
                               @bind-CurrentPage="currentPage"
                               VisiblePageCount="{{(int)visible}}"
                               Disabled="{{disabled.ToString().ToLowerInvariant()}}" />
    </ShadcnPagination>
</section>

@code {
    private int currentPage = {{(int)current}};
}
""";

        RenderFragment preview = builder =>
        {
            builder.OpenComponent<PaginationDossierPreview>(0);
            builder.AddAttribute(1, nameof(PaginationDossierPreview.CurrentPage), (int)current);
            builder.AddAttribute(2, nameof(PaginationDossierPreview.CurrentPageChanged), EventCallback.Factory.Create<int>(new object(), page => current = page));
            builder.AddAttribute(3, nameof(PaginationDossierPreview.TotalPages), totalPages);
            builder.AddAttribute(4, nameof(PaginationDossierPreview.TotalItems), 96);
            builder.AddAttribute(5, nameof(PaginationDossierPreview.VisiblePageCount), (int)visible);
            builder.AddAttribute(6, nameof(PaginationDossierPreview.Disabled), disabled);
            builder.CloseComponent();
        };

        var example = Example("pagination", "Production quotation pages", "Move through a realistic quotation queue with a configurable numeric window, stable boundary pages, automatic ellipses, and accessible previous and next actions.", Source(), preview, [Number("pagination-current", "Current page", current, v => current = Math.Clamp(Math.Round(v), 1, totalPages)), Number("pagination-visible", "Visible pages", visible, v => visible = Math.Clamp(Math.Round(v), 3, 9)), Toggle("pagination-disabled", "Disable navigation", v => disabled = v)], ["current", "previous", "next", "ellipsis", "visible-count", "disabled", "keyboard", "rtl"]);
        return example with { RazorSourceProvider = Source };
    }

    private static ComponentExampleDefinition Resizable()
    {
        var vertical = false; var disabled = false; var collapsible = false;
        RenderFragment preview = b =>
        {
            b.OpenElement(0, "div"); b.AddAttribute(1, "class", "showcase-resizable-dossier");
            b.OpenComponent<ShadcnResizableGroup>(2);
            b.AddAttribute(3, "Class", "showcase-resizable-group");
            b.AddAttribute(4, "Sizes", new[] { 44d, 56d });
            b.AddAttribute(5, "Direction", vertical ? ShadcnResizableDirection.Vertical : ShadcnResizableDirection.Horizontal);
            b.AddAttribute(6, "Disabled", disabled);
            b.AddAttribute(7, "ChildContent", (RenderFragment)(c =>
            {
                AddPanel(c, 0, "queue", "Production queue", collapsible, "3 jobs require review");
                c.OpenComponent<ShadcnResizableHandle>(10); c.AddAttribute(11, "WithHandle", true); c.AddAttribute(12, "Label", "Resize production queue and job detail panels"); c.CloseComponent();
                AddPanel(c, 20, "detail", "Job Q-1842", false, "Aluminum 6061 enclosure · Revision C");
            }));
            b.CloseComponent();
            b.CloseElement();
        };

        string Source() => $$"""
<div class="showcase-resizable-dossier">
    <ShadcnResizableGroup Class="showcase-resizable-group"
                           Sizes="new[] { 44d, 56d }"
                           Direction="ShadcnResizableDirection.{{(vertical ? "Vertical" : "Horizontal")}}"
                           Disabled="{{disabled.ToString().ToLowerInvariant()}}">
        <ShadcnResizablePanel Id="queue" MinimumSize="20" MaximumSize="80" Collapsible="{{collapsible.ToString().ToLowerInvariant()}}">
            <div class="showcase-resizable-panel-content">
                <span>Production queue</span>
                <strong>3 jobs require review</strong>
                <small>Q-1842 · CNC milling · Due today</small>
            </div>
        </ShadcnResizablePanel>
        <ShadcnResizableHandle WithHandle="true" Label="Resize production queue and job detail panels" />
        <ShadcnResizablePanel Id="detail" MinimumSize="20" MaximumSize="80" Collapsible="false">
            <div class="showcase-resizable-panel-content">
                <span>Selected job</span>
                <strong>Job Q-1842</strong>
                <small>Aluminum 6061 enclosure · Revision C</small>
            </div>
        </ShadcnResizablePanel>
    </ShadcnResizableGroup>
</div>
""";

        return Example("resizable", "Production workspace", "Resize a production queue and job inspector directly with pointer or keyboard, then switch orientation to verify both axes.", Source(), preview, [Toggle("resizable-vertical", "Vertical", v => vertical = v), Toggle("resizable-collapsible", "Collapsible queue", v => collapsible = v), Toggle("resizable-disabled", "Disabled", v => disabled = v)], ["horizontal", "vertical", "pointer", "keyboard", "constraints", "collapse", "persistence", "rtl"]) with { RazorSourceProvider = Source };
    }

    private static ComponentExampleDefinition ScrollArea()
    {
        var always = true; var horizontal = false;
        RenderFragment preview = b => { b.OpenComponent<ShadcnScrollArea>(0); b.AddAttribute(1, "Type", always ? ShadcnScrollAreaType.Always : ShadcnScrollAreaType.Auto); b.AddAttribute(2, "Style", "height:12rem;width:min(100%,24rem)"); b.AddAttribute(3, "ChildContent", (RenderFragment)(c => { c.OpenComponent<ShadcnScrollAreaViewport>(0); c.AddAttribute(1, "Label", "Material catalog"); c.AddAttribute(2, "ChildContent", MaterialCatalog(horizontal)); c.CloseComponent(); c.OpenComponent<ShadcnScrollAreaScrollbar>(10); c.AddAttribute(11, "Orientation", horizontal ? ShadcnScrollAreaOrientation.Horizontal : ShadcnScrollAreaOrientation.Vertical); c.AddAttribute(12, "ChildContent", (RenderFragment)(s => Add<ShadcnScrollAreaThumb>(s, 0))); c.CloseComponent(); Add<ShadcnScrollAreaCorner>(c, 20); })); b.CloseComponent(); };
        var source = """
<ShadcnScrollArea Type="ShadcnScrollAreaType.Always" Style="height:12rem;width:min(100%,24rem)">
    <ShadcnScrollAreaViewport Label="Material catalog">
        <p>Aluminum 6061 · CNC-ready stock</p>
        <p>Stainless 316L · Corrosion resistant</p>
        <p>PEEK · High-temperature polymer</p>
    </ShadcnScrollAreaViewport>
    <ShadcnScrollAreaScrollbar Orientation="ShadcnScrollAreaOrientation.Vertical">
        <ShadcnScrollAreaThumb />
    </ShadcnScrollAreaScrollbar>
    <ShadcnScrollAreaCorner />
</ShadcnScrollArea>
""";
        return Example("scroll-area", "Native scroll area", "Native focusable scrolling with auto/always visibility, vertical/horizontal thumbs, track click, drag grab offset, content observation, and RTL normalization.", source, preview, [Toggle("scroll-always", "Always visible", v => always = v, true), Toggle("scroll-horizontal", "Horizontal", v => horizontal = v)], ["auto", "always", "vertical", "horizontal", "drag", "track", "rtl", "keyboard"]);
    }

    private static ComponentExampleDefinition Sidebar()
    {
        var open = true; var right = false; var none = false;
        RenderFragment preview = b => { b.OpenComponent<ShadcnSidebarProvider>(0); b.AddAttribute(1, "Open", open); b.AddAttribute(2, "ChildContent", (RenderFragment)(c => { c.OpenComponent<ShadcnSidebar>(0); c.AddAttribute(1, "Id", "dossier"); c.AddAttribute(2, "Side", right ? ShadcnSidebarSide.Right : ShadcnSidebarSide.Left); c.AddAttribute(3, "Collapsible", none ? ShadcnSidebarCollapsible.None : ShadcnSidebarCollapsible.Icon); c.AddAttribute(4, "Label", "Workspace"); c.AddAttribute(5, "ChildContent", SidebarContent()); c.CloseComponent(); c.OpenComponent<ShadcnSidebarInset>(10); c.AddAttribute(11, "ChildContent", (RenderFragment)(inset => { inset.OpenComponent<ShadcnSidebarTrigger>(0); inset.AddAttribute(1, "TargetId", "dossier"); inset.CloseComponent(); inset.OpenElement(10, "div"); inset.AddAttribute(11, "class", "showcase-sidebar-main"); inset.OpenElement(12, "h3"); inset.AddContent(13, "Quotation workspace"); inset.CloseElement(); inset.OpenElement(14, "p"); inset.AddContent(15, "Review active quotations and production handoffs."); inset.CloseElement(); inset.CloseElement(); })); c.CloseComponent(); })); b.CloseComponent(); };
        var source = """
<ShadcnSidebarProvider Open="true">
    <ShadcnSidebar Id="dossier" Collapsible="ShadcnSidebarCollapsible.Icon" Label="Workspace">
        <ShadcnSidebarHeader>MALIEV</ShadcnSidebarHeader>
        <ShadcnSidebarContent>
            <ShadcnSidebarMenuItem>Quotations</ShadcnSidebarMenuItem>
            <ShadcnSidebarMenuItem>Materials</ShadcnSidebarMenuItem>
            <ShadcnSidebarMenuItem>Team settings</ShadcnSidebarMenuItem>
        </ShadcnSidebarContent>
    </ShadcnSidebar>
    <ShadcnSidebarInset>
        <ShadcnSidebarTrigger TargetId="dossier" />
        <h3>Quotation workspace</h3>
        <p>Review active quotations and production handoffs.</p>
    </ShadcnSidebarInset>
</ShadcnSidebarProvider>
""";
        return Example("sidebar", "Responsive sidebar shell", "A realistic quotation workspace with navigation groups, active state, responsive collapse, physical sides, and mobile modal behavior.", source, preview, [Toggle("sidebar-open", "Expanded", v => open = v, true), Toggle("sidebar-right", "Right side", v => right = v), Toggle("sidebar-none", "Non-collapsible", v => none = v)], ["expanded", "collapsed", "offcanvas", "icon", "none", "mobile-modal", "persistence", "tooltip", "rtl"]);
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
    private static void AddAccordionItem(RenderTreeBuilder b, int s, string value, string title, string content) => AddAccordionItem(b, s, value, title, Text(content));
    private static void AddAccordionItem(RenderTreeBuilder b, int s, string value, string title, RenderFragment content)
    {
        b.OpenComponent<ShadcnAccordionItem>(s);
        b.AddAttribute(s + 1, "Value", value);
        b.AddAttribute(s + 2, "ChildContent", (RenderFragment)(c =>
        {
            AddText<ShadcnAccordionTrigger>(c, 0, title);
            c.OpenComponent<ShadcnAccordionContent>(10);
            c.AddAttribute(11, "ChildContent", content);
            c.CloseComponent();
        }));
        b.CloseComponent();
    }
    private static RenderFragment ShippingAccordionContent() => builder =>
    {
        builder.OpenElement(0, "p"); builder.AddContent(1, "Choose the handoff that fits your production deadline."); builder.CloseElement();
        builder.OpenElement(10, "ul");
        AddAccordionDetail(builder, 20, "Standard delivery", "dispatch estimate included before checkout");
        AddAccordionDetail(builder, 30, "Express delivery", "priority workshop queue when available");
        builder.CloseElement();
    };
    private static RenderFragment ReturnsAccordionContent() => builder =>
    {
        builder.OpenElement(0, "p"); builder.AddContent(1, "Returns are accepted within 30 days when the item is unused and in its original packaging."); builder.CloseElement();
        builder.OpenElement(10, "p"); builder.AddContent(11, "Include the quotation number so the team can route the request quickly."); builder.CloseElement();
        builder.OpenElement(20, "p"); builder.AddContent(21, "Revision notes help production review the requested change."); builder.CloseElement();
    };
    private static RenderFragment SupportAccordionContent() => builder =>
    {
        builder.OpenElement(0, "p"); builder.AddContent(1, "Send a message from the quotation workspace and the production team will reply with the next available step."); builder.CloseElement();
        builder.OpenElement(10, "ul");
        AddAccordionDetail(builder, 20, "Materials and finishing", "ask for a recommendation");
        AddAccordionDetail(builder, 30, "Drawing review", "attach the requested revision");
        builder.CloseElement();
    };
    private static void AddAccordionDetail(RenderTreeBuilder builder, int sequence, string title, string detail)
    {
        builder.OpenElement(sequence, "li");
        builder.OpenElement(sequence + 1, "strong"); builder.AddContent(sequence + 2, title); builder.CloseElement();
        builder.AddContent(sequence + 3, $" — {detail}.");
        builder.CloseElement();
    }
    private static void AddBreadcrumbLink(RenderTreeBuilder b, int s, string text, string href) { b.OpenComponent<ShadcnBreadcrumbItem>(s); b.AddAttribute(s + 1, "ChildContent", (RenderFragment)(c => { c.OpenComponent<ShadcnBreadcrumbLink>(0); c.AddAttribute(1, "Href", href); c.AddAttribute(2, "ChildContent", Text(text)); c.CloseComponent(); })); b.CloseComponent(); }
    private static void AddPaginationItem<T>(RenderTreeBuilder b, int s, bool disabled) where T : IComponent { b.OpenComponent<ShadcnPaginationItem>(s); b.AddAttribute(s + 1, "ChildContent", (RenderFragment)(c => { c.OpenComponent<T>(0); c.AddAttribute(1, "Disabled", disabled); c.CloseComponent(); })); b.CloseComponent(); }
    private static void AddPaginationDirection<T>(RenderTreeBuilder b, int s, bool disabled, Action apply) where T : IComponent { b.OpenComponent<ShadcnPaginationItem>(s); b.AddAttribute(s + 1, "ChildContent", (RenderFragment)(c => { c.OpenComponent<T>(0); c.AddAttribute(1, "Disabled", disabled); c.AddAttribute(2, "OnClick", EventCallback.Factory.Create<MouseEventArgs>(new object(), _ => { apply(); return Task.CompletedTask; })); c.CloseComponent(); })); b.CloseComponent(); }
    private static void AddPage(RenderTreeBuilder b, int s, int page, bool current, Action apply) { b.OpenComponent<ShadcnPaginationItem>(s); b.AddAttribute(s + 1, "ChildContent", (RenderFragment)(c => { c.OpenComponent<ShadcnPaginationLink>(0); c.AddAttribute(1, "Current", current); c.AddAttribute(2, "Href", $"#quotation-page-{page}"); c.AddAttribute(3, "OnClick", EventCallback.Factory.Create<MouseEventArgs>(new object(), _ => { apply(); return Task.CompletedTask; })); c.AddAttribute(4, "ChildContent", Text(page.ToString())); c.CloseComponent(); })); b.CloseComponent(); }
    private static void AddPanel(RenderTreeBuilder b, int s, string id, string title, bool collapsible, string summary) { b.OpenComponent<ShadcnResizablePanel>(s); b.AddAttribute(s + 1, "Id", id); b.AddAttribute(s + 2, "MinimumSize", 20d); b.AddAttribute(s + 3, "MaximumSize", 80d); b.AddAttribute(s + 4, "Collapsible", collapsible); b.AddAttribute(s + 5, "ChildContent", PanelContent(id, title, summary)); b.CloseComponent(); }
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

    private static RenderFragment PanelContent(string id, string title, string summary) => builder =>
    {
        builder.OpenElement(0, "div"); builder.AddAttribute(1, "class", "showcase-resizable-panel-content");
        builder.OpenElement(2, "span"); builder.AddContent(3, id == "queue" ? title : "Selected job"); builder.CloseElement();
        builder.OpenElement(4, "strong"); builder.AddContent(5, id == "queue" ? summary : title); builder.CloseElement();
        builder.OpenElement(6, "small"); builder.AddContent(7, id == "queue" ? "Q-1842 · CNC milling · Due today" : summary); builder.CloseElement();
        builder.CloseElement();
    };
}
