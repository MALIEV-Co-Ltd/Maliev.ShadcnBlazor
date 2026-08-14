using Bunit;
using Maliev.ShadcnBlazor.Components.Navigation;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Web;

namespace Maliev.ShadcnBlazor.Tests.Components.DisclosureNavigation;

public sealed class NavigationCompositionTests : BunitContext
{
    [Fact]
    public void BreadcrumbUsesNativeOrderedNavigationAndCurrentPageSemantics()
    {
        var cut = Render<ShadcnBreadcrumb>(p => p
            .Add(x => x.Label, "เส้นทางนำทาง")
            .AddUnmatched("data-consumer", "integration")
            .AddChildContent(builder =>
            {
                builder.OpenComponent<ShadcnBreadcrumbList>(0);
                builder.AddAttribute(1, nameof(ShadcnBreadcrumbList.ChildContent), (RenderFragment)(list =>
                {
                    AddBreadcrumbLink(list, 0, "/", "หน้าแรก");
                    list.OpenComponent<ShadcnBreadcrumbSeparator>(10); list.CloseComponent();
                    list.OpenComponent<ShadcnBreadcrumbItem>(11);
                    list.AddAttribute(12, nameof(ShadcnBreadcrumbItem.ChildContent), (RenderFragment)(item =>
                    {
                        item.OpenComponent<ShadcnBreadcrumbPage>(0);
                        item.AddAttribute(1, nameof(ShadcnBreadcrumbPage.ChildContent), (RenderFragment)(page => page.AddContent(0, "ใบเสนอราคา")));
                        item.CloseComponent();
                    }));
                    list.CloseComponent();
                }));
                builder.CloseComponent();
            }));

        var nav = cut.Find("nav[data-slot='breadcrumb']");
        Assert.Equal("เส้นทางนำทาง", nav.GetAttribute("aria-label"));
        Assert.Equal("integration", nav.GetAttribute("data-consumer"));
        Assert.NotNull(cut.Find("ol[data-slot='breadcrumb-list']"));
        Assert.Equal("page", cut.Find("[data-slot='breadcrumb-page']").GetAttribute("aria-current"));
        Assert.Equal("true", cut.Find("[data-slot='breadcrumb-separator']").GetAttribute("aria-hidden"));
    }

    [Fact]
    public void BreadcrumbEllipsisIsDecorativeWithCustomScreenReaderText()
    {
        var cut = Render<ShadcnBreadcrumbEllipsis>(p => p.Add(x => x.Label, "รายการเพิ่มเติม"));
        var root = cut.Find("[data-slot='breadcrumb-ellipsis']");
        Assert.Equal("presentation", root.GetAttribute("role"));
        Assert.Equal("true", root.QuerySelector("svg")?.GetAttribute("aria-hidden"));
        Assert.Equal("รายการเพิ่มเติม", root.QuerySelector(".shadcn-sr-only")?.TextContent);
    }

    [Fact]
    public void PaginationExposesCurrentLinkAndSuppressesDisabledNavigation()
    {
        var calls = 0;
        var cut = Render<ShadcnPagination>(p => p
            .Add(x => x.Label, "หน้าผลลัพธ์")
            .AddChildContent(builder =>
            {
                builder.OpenComponent<ShadcnPaginationContent>(0);
                builder.AddAttribute(1, nameof(ShadcnPaginationContent.ChildContent), (RenderFragment)(list =>
                {
                    list.OpenComponent<ShadcnPaginationItem>(0);
                    list.AddAttribute(1, nameof(ShadcnPaginationItem.ChildContent), (RenderFragment)(item =>
                    {
                        item.OpenComponent<ShadcnPaginationLink>(0);
                        item.AddAttribute(1, nameof(ShadcnPaginationLink.Href), "/orders?page=2");
                        item.AddAttribute(2, nameof(ShadcnPaginationLink.Current), true);
                        item.AddAttribute(3, nameof(ShadcnPaginationLink.OnClick), EventCallback.Factory.Create<MouseEventArgs>(this, () => calls++));
                        item.AddAttribute(4, nameof(ShadcnPaginationLink.ChildContent), (RenderFragment)(text => text.AddContent(0, "2")));
                        item.CloseComponent();
                    }));
                    list.CloseComponent();
                    list.OpenComponent<ShadcnPaginationItem>(10);
                    list.AddAttribute(11, nameof(ShadcnPaginationItem.ChildContent), (RenderFragment)(item =>
                    {
                        item.OpenComponent<ShadcnPaginationLink>(0);
                        item.AddAttribute(1, nameof(ShadcnPaginationLink.Disabled), true);
                        item.AddAttribute(2, nameof(ShadcnPaginationLink.OnClick), EventCallback.Factory.Create<MouseEventArgs>(this, () => calls++));
                        item.AddAttribute(3, nameof(ShadcnPaginationLink.ChildContent), (RenderFragment)(text => text.AddContent(0, "3")));
                        item.CloseComponent();
                    }));
                    list.CloseComponent();
                }));
                builder.CloseComponent();
            }));

        Assert.Equal("หน้าผลลัพธ์", cut.Find("nav").GetAttribute("aria-label"));
        var current = cut.Find("a[data-current='true']");
        Assert.Equal("page", current.GetAttribute("aria-current"));
        current.Click();
        cut.Find("button[disabled]").Click();
        Assert.Equal(1, calls);
    }

    [Fact]
    public void PaginationPreviousNextAndEllipsisHaveLocalizedAccessibleContracts()
    {
        var cut = Render<ShadcnPagination>(p => p.AddChildContent(builder =>
        {
            builder.OpenComponent<ShadcnPaginationContent>(0);
            builder.AddAttribute(1, nameof(ShadcnPaginationContent.ChildContent), (RenderFragment)(list =>
            {
                list.OpenComponent<ShadcnPaginationItem>(0);
                list.AddAttribute(1, nameof(ShadcnPaginationItem.ChildContent), (RenderFragment)(item =>
                {
                    item.OpenComponent<ShadcnPaginationPrevious>(0); item.AddAttribute(1, nameof(ShadcnPaginationPrevious.Label), "ก่อนหน้า"); item.CloseComponent();
                })); list.CloseComponent();
                list.OpenComponent<ShadcnPaginationItem>(2); list.AddAttribute(3, nameof(ShadcnPaginationItem.ChildContent), (RenderFragment)(item => { item.OpenComponent<ShadcnPaginationEllipsis>(0); item.AddAttribute(1, nameof(ShadcnPaginationEllipsis.Label), "หน้าเพิ่มเติม"); item.CloseComponent(); })); list.CloseComponent();
                list.OpenComponent<ShadcnPaginationItem>(4); list.AddAttribute(5, nameof(ShadcnPaginationItem.ChildContent), (RenderFragment)(item => { item.OpenComponent<ShadcnPaginationNext>(0); item.AddAttribute(1, nameof(ShadcnPaginationNext.Label), "ถัดไป"); item.CloseComponent(); })); list.CloseComponent();
            })); builder.CloseComponent();
        }));

        Assert.Equal("ก่อนหน้า", cut.Find("[data-slot='pagination-previous']").GetAttribute("aria-label"));
        Assert.Equal("ถัดไป", cut.Find("[data-slot='pagination-next']").GetAttribute("aria-label"));
        Assert.Equal("หน้าเพิ่มเติม", cut.Find("[data-slot='pagination-ellipsis'] .shadcn-sr-only").TextContent);
    }

    [Fact]
    public void PaginationRejectsUnknownSize()
    {
        Assert.ThrowsAny<Exception>(() => Render<ShadcnPaginationLink>(p => p.Add(x => x.Size, (ShadcnPaginationLinkSize)999)));
    }

    private static void AddBreadcrumbLink(Microsoft.AspNetCore.Components.Rendering.RenderTreeBuilder builder, int sequence, string href, string text)
    {
        builder.OpenComponent<ShadcnBreadcrumbItem>(sequence);
        builder.AddAttribute(sequence + 1, nameof(ShadcnBreadcrumbItem.ChildContent), (RenderFragment)(item =>
        {
            item.OpenComponent<ShadcnBreadcrumbLink>(0);
            item.AddAttribute(1, nameof(ShadcnBreadcrumbLink.Href), href);
            item.AddAttribute(2, nameof(ShadcnBreadcrumbLink.ChildContent), (RenderFragment)(content => content.AddContent(0, text)));
            item.CloseComponent();
        }));
        builder.CloseComponent();
    }
}
