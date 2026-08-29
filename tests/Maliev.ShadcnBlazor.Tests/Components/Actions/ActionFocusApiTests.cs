using Bunit;
using Maliev.ShadcnBlazor.Components.Actions;
using Maliev.ShadcnBlazor.Components.Content;
using Maliev.ShadcnBlazor.Components.Conversation;
using Maliev.ShadcnBlazor.Components.Disclosure;
using Maliev.ShadcnBlazor.Components.Navigation;
using Maliev.ShadcnBlazor.Components.Navigation.Sidebar;
using Maliev.ShadcnBlazor.Components.Overlays;
using Maliev.ShadcnBlazor.Components.Primitives;
using Microsoft.AspNetCore.Components;

namespace Maliev.ShadcnBlazor.Tests.Components.Actions;

public sealed class ActionFocusApiTests : BunitContext
{
    public ActionFocusApiTests()
    {
        JSInterop.Mode = JSRuntimeMode.Loose;
    }

    public static TheoryData<Type> AuditedLeafTypes => new()
    {
        typeof(ShadcnToggle),
        typeof(ShadcnToggleGroupItem<string>),
        typeof(ShadcnCarouselPrevious),
        typeof(ShadcnCarouselNext),
        typeof(ShadcnAccordionTrigger),
        typeof(ShadcnCollapsibleTrigger),
        typeof(ShadcnBreadcrumbLink),
        typeof(ShadcnPaginationLink),
        typeof(ShadcnPaginationPrevious),
        typeof(ShadcnPaginationNext),
        typeof(ShadcnNavigationMenuLink),
        typeof(ShadcnNavigationMenuTrigger),
        typeof(ShadcnTabsTrigger)
        ,typeof(ShadcnSidebarGroupAction), typeof(ShadcnSidebarMenuAction), typeof(ShadcnSidebarMenuButton), typeof(ShadcnSidebarMenuSubButton), typeof(ShadcnSidebarTrigger)
        ,typeof(ShadcnAlertDialogAction), typeof(ShadcnAlertDialogCancel), typeof(ShadcnAlertDialogTrigger), typeof(ShadcnDialogClose), typeof(ShadcnDialogTrigger)
        ,typeof(ShadcnDrawerClose), typeof(ShadcnDrawerTrigger), typeof(ShadcnDropdownMenuTrigger), typeof(ShadcnHoverCardTrigger), typeof(ShadcnMenubarTrigger), typeof(ShadcnPopoverTrigger), typeof(ShadcnSheetClose), typeof(ShadcnSheetTrigger), typeof(ShadcnTooltipTrigger), typeof(ShadcnContextMenuTrigger)
        ,typeof(ShadcnDropdownMenuItem), typeof(ShadcnDropdownMenuCheckboxItem), typeof(ShadcnDropdownMenuRadioItem), typeof(ShadcnDropdownMenuSubTrigger)
        ,typeof(ShadcnContextMenuItem), typeof(ShadcnContextMenuCheckboxItem), typeof(ShadcnContextMenuRadioItem), typeof(ShadcnContextMenuSubTrigger)
        ,typeof(ShadcnMenubarItem), typeof(ShadcnMenubarCheckboxItem), typeof(ShadcnMenubarRadioItem), typeof(ShadcnMenubarSubTrigger), typeof(ShadcnCommandItem)
        ,typeof(ShadcnAttachmentAction), typeof(ShadcnAttachmentTrigger), typeof(ShadcnMessageCopyAction), typeof(ShadcnMessageReplyAction), typeof(ShadcnMessageScrollerButton)
        ,typeof(ShadcnQuestionnaireNext), typeof(ShadcnQuestionnairePrevious), typeof(ShadcnQuestionnaireSkip), typeof(ShadcnQuestionnaireSubmit)
    };

    [Theory]
    [MemberData(nameof(AuditedLeafTypes))]
    public void AuditedActionAndNavigationLeavesImplementTheSharedFocusContract(Type componentType)
    {
        Assert.True(typeof(IShadcnFocusable).IsAssignableFrom(componentType), componentType.FullName);
    }

    [Fact]
    public async Task ToggleFocusAsyncDispatchesToItsNativeButton()
    {
        var cut = Render<ShadcnToggle>(parameters => parameters.AddChildContent("Pin"));
        Assert.Equal("BUTTON", cut.Find("[data-slot='toggle']").TagName);

        await AssertFocusAsync((IShadcnFocusable)cut.Instance, preventScroll: true);
    }

    [Fact]
    public async Task PaginationLinkFocusAsyncSupportsAnchorRendering()
    {
        var cut = Render<ShadcnPaginationLink>(parameters => parameters
            .Add(component => component.Href, "/orders")
            .AddChildContent("Orders"));
        Assert.Equal("A", cut.Find("[data-slot='pagination-link']").TagName);

        await AssertFocusAsync((IShadcnFocusable)cut.Instance, preventScroll: false);
    }

    private async Task AssertFocusAsync(IShadcnFocusable component, bool preventScroll)
    {
        await component.FocusAsync(preventScroll);

        var invocation = Assert.Single(
            JSInterop.Invocations,
            candidate => candidate.Identifier == "Blazor._internal.domWrapper.focus");
        var element = Assert.IsType<ElementReference>(invocation.Arguments[0]);
        Assert.False(string.IsNullOrWhiteSpace(element.Id));
        Assert.Equal(preventScroll, invocation.Arguments[1]);
    }
}
