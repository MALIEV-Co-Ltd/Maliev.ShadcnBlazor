using Bunit;
using Maliev.ShadcnBlazor.Showcase.MockSites;
using Maliev.ShadcnBlazor.Showcase.Theming;
using System.Text.Json;
using System.Text.RegularExpressions;

namespace Maliev.ShadcnBlazor.Tests.Showcase;

public sealed class MockSiteStateTests
{
    [Fact]
    public void FixturesAreDeterministicAndContainNoRuntimeGeneratedValues()
    {
        var first = new MockSiteState();
        var second = new MockSiteState();

        Assert.Equal(first.Operations, second.Operations);
        Assert.Equal(first.Manufacturing, second.Manufacturing);
        Assert.Equal(first.Customers, second.Customers);
        Assert.Equal(
            ["MO-24018", "MO-24019", "MO-24020", "MO-24021", "MO-24022"],
            first.Operations.Jobs.Select(job => job.Id));
        Assert.Equal(
            ["กานต์ชนก ศรีสุข", "ปริญญา จิตมั่น", "วริศรา ตั้งใจ"],
            first.Customers.Records.Select(customer => customer.Name));
    }

    [Fact]
    public void ResetRestoresOnlyTheSelectedFixtureToItsExactBaseline()
    {
        var state = new MockSiteState();
        var operationsBaseline = state.Operations;
        var customersBaseline = state.Customers;

        state.SetOperationsQuery("laser");
        state.SetCustomerQuery("วริศรา");
        state.Reset(ThemeStudioMockup.OperationsDashboard);

        Assert.Equal(operationsBaseline, state.Operations);
        Assert.NotEqual(customersBaseline, state.Customers);
        Assert.Equal("วริศรา", state.Customers.Query);

        state.Reset();
        Assert.Equal(customersBaseline, state.Customers);
    }

    [Fact]
    public void OperationsWorkflowFiltersSortsPagesAndCompletesAJob()
    {
        var state = new MockSiteState();

        state.SetOperationsQuery("aluminum");
        Assert.All(state.FilteredOperations, job => Assert.Contains("aluminum", job.SearchText, StringComparison.OrdinalIgnoreCase));
        state.ToggleOperationsSort();
        Assert.True(state.Operations.SortDescending);
        state.SetOperationsQuery(string.Empty);
        state.NextOperationsPage();
        Assert.Equal(2, state.Operations.Page);

        state.CompleteOperation("MO-24018");

        Assert.Equal(OperationStatus.Completed, state.Operations.Jobs.Single(job => job.Id == "MO-24018").Status);
        Assert.Equal(MockAnnouncement.OperationCompleted, state.Operations.Announcement);
        Assert.Equal("MO-24018", state.Operations.AnnouncementSubject);
    }

    [Fact]
    public void OperationsFilteringAndSortingResetPaginationAndPageWithinBounds()
    {
        var state = new MockSiteState();

        state.NextOperationsPage();
        Assert.Equal(2, state.Operations.Page);
        state.NextOperationsPage();
        Assert.Equal(2, state.Operations.Page);
        state.ToggleOperationsSort();
        Assert.Equal(1, state.Operations.Page);
        state.SetOperationsQuery("no fixture matches this query");

        Assert.Empty(state.FilteredOperations);
        Assert.Equal(1, state.OperationsPageCount);
        state.PreviousOperationsPage();
        Assert.Equal(1, state.Operations.Page);
    }

    [Fact]
    public void OperationsSortsByTypedDueDateInsteadOfDisplayText()
    {
        var state = new MockSiteState();

        Assert.Equal(["MO-24018", "MO-24019", "MO-24020"], state.FilteredOperations.Select(job => job.Id));
        state.ToggleOperationsSort();
        Assert.Equal(["MO-24022", "MO-24021", "MO-24020"], state.FilteredOperations.Select(job => job.Id));
    }

    [Fact]
    public void ManufacturingWorkflowValidatesReviewsConfirmsAndCanReset()
    {
        var state = new MockSiteState();

        Assert.False(state.ReviewManufacturingRequest());
        Assert.Contains(ManufacturingValidationError.ProjectRequired, state.Manufacturing.Errors);

        state.SetManufacturingProject("Pump bracket pilot");
        state.SetManufacturingPart("mounting-bracket.step");
        state.AttachManufacturingFile();
        Assert.True(state.ReviewManufacturingRequest());
        Assert.Equal(ManufacturingStage.Review, state.Manufacturing.Stage);
        Assert.True(state.OpenManufacturingConfirmation());
        state.ConfirmManufacturingRequest();

        Assert.Equal(ManufacturingStage.Success, state.Manufacturing.Stage);
        Assert.Equal("MR-240812", state.Manufacturing.RequestId);

        state.Reset(ThemeStudioMockup.ManufacturingRequest);
        Assert.Equal(ManufacturingStage.Details, state.Manufacturing.Stage);
        Assert.Null(state.Manufacturing.RequestId);
    }

    [Fact]
    public void CorrectingOneManufacturingFieldRetainsOtherActiveErrors()
    {
        var state = new MockSiteState();

        Assert.False(state.ReviewManufacturingRequest());
        state.SetManufacturingProject("Pump bracket pilot");

        Assert.DoesNotContain(ManufacturingValidationError.ProjectRequired, state.Manufacturing.Errors);
        Assert.Contains(ManufacturingValidationError.PartRequired, state.Manufacturing.Errors);
        Assert.Contains(ManufacturingValidationError.AttachmentRequired, state.Manufacturing.Errors);
    }

    [Fact]
    public void CustomerWorkflowSearchesOpensDetailAndSendsAMessage()
    {
        var state = new MockSiteState();

        state.SetCustomerQuery("วริศรา");
        Assert.Single(state.FilteredCustomers);
        state.OpenCustomer("CUS-103");
        state.SetCustomerTab(CustomerWorkspaceTab.Messages);
        state.SetCustomerMessage("Your revised drawing is ready.");
        Assert.True(state.SendCustomerMessage());

        Assert.True(state.Customers.DetailOpen);
        Assert.Equal(CustomerWorkspaceTab.Messages, state.Customers.ActiveTab);
        Assert.Equal(MockAnnouncement.CustomerMessageSent, state.Customers.Announcement);
        Assert.Equal("วริศรา ตั้งใจ", state.Customers.AnnouncementSubject);
        Assert.Contains(state.Customers.Messages, message => message.Text == "Your revised drawing is ready.");
    }
}

public sealed class MockSiteComponentTests : BunitContext
{
    public MockSiteComponentTests() => JSInterop.Mode = JSRuntimeMode.Loose;

    [Theory]
    [InlineData(ThemeStudioMockup.OperationsDashboard, "operations-dashboard-mock")]
    [InlineData(ThemeStudioMockup.ManufacturingRequest, "manufacturing-request-mock")]
    [InlineData(ThemeStudioMockup.CustomerWorkspace, "customer-workspace-mock")]
    public void HostRendersExactlyOneDeterministicLocalMockWithoutHttpServices(
        ThemeStudioMockup mockup,
        string expectedTestId)
    {
        var state = new MockSiteState();

        var cut = Render<MockSiteHost>(parameters => parameters
            .Add(component => component.State, state)
            .Add(component => component.Mockup, mockup)
            .Add(component => component.Locale, ThemeStudioLocale.English));

        Assert.Single(cut.FindAll("[data-mock-site]"));
        Assert.NotEmpty(cut.FindAll($"[data-testid='{expectedTestId}']"));
        Assert.DoesNotContain(cut.Markup, "http://", StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain(cut.Markup, "https://", StringComparison.OrdinalIgnoreCase);
        Assert.Contains(cut.FindAll("[data-slot]"), element => element.GetAttribute("data-slot") == "typography");
    }

    [Fact]
    public void OperationsCompositionUsesCertifiedPackageContentAndACompleteStatefulWorkflow()
    {
        var state = new MockSiteState();
        var cut = Render<OperationsDashboardMock>(parameters => parameters
            .Add(component => component.State, state)
            .Add(component => component.Locale, ThemeStudioLocale.English));

        Assert.NotEmpty(cut.FindAll("nav[aria-label='Operations navigation']"));
        Assert.NotEmpty(cut.FindAll("[data-slot='item']"));
        Assert.NotEmpty(cut.FindAll("figure[aria-labelledby='operations-chart-title']"));
        Assert.NotEmpty(cut.FindAll("table"));
        Assert.NotEmpty(cut.FindAll("details[data-testid='operations-actions']"));
        Assert.NotEmpty(cut.FindAll("[role='status'][aria-live='polite']"));

        cut.Find("[data-testid='operations-query']").Input("aluminum");
        Assert.All(cut.FindAll("tbody tr"), row => Assert.Contains("Aluminum", row.TextContent, StringComparison.OrdinalIgnoreCase));
        cut.Find("[data-testid='operation-complete-MO-24018']").Click();
        Assert.Contains("MO-24018 moved to completed", cut.Find("[role='status']").TextContent, StringComparison.Ordinal);

        cut.Find("[data-testid='operations-state-loading']").Click();
        Assert.NotEmpty(cut.FindAll("[data-testid='operations-skeleton']"));
        cut.Find("[data-testid='operations-state-empty']").Click();
        Assert.NotEmpty(cut.FindAll("[data-slot='empty']"));
        cut.Find("[data-testid='operations-state-error']").Click();
        Assert.NotEmpty(cut.FindAll("[role='alert']"));
    }

    [Fact]
    public void ManufacturingCompositionAssociatesFieldsAndCompletesTheLocalRequest()
    {
        var state = new MockSiteState();
        var cut = Render<ManufacturingRequestMock>(parameters => parameters
            .Add(component => component.State, state)
            .Add(component => component.Locale, ThemeStudioLocale.English));

        Assert.True(cut.FindAll("[data-slot='field']").Count >= 5);
        Assert.NotEmpty(cut.FindAll("input[type='date']"));
        Assert.Empty(cut.FindAll("select"));
        Assert.Equal(3, cut.FindAll("[data-slot='select']").Count);
        Assert.NotEmpty(cut.FindAll("[role='progressbar']"));

        cut.Find("#manufacturing-material").Click();
        cut.Find("[role='option'][data-value='Stainless 304']").Click();
        Assert.Equal("Stainless 304", state.Manufacturing.Material);

        cut.Find("[data-testid='manufacturing-state-loading']").Click();
        Assert.NotEmpty(cut.FindAll("[data-testid='manufacturing-skeleton']"));
        cut.Find("[data-testid='manufacturing-state-empty']").Click();
        Assert.NotEmpty(cut.FindAll("[data-slot='empty']"));
        cut.Find("[data-testid='manufacturing-state-error']").Click();
        Assert.NotEmpty(cut.FindAll("[data-testid='manufacturing-state-error-alert'][role='alert']"));
        cut.Find("[data-testid='manufacturing-state-ready']").Click();

        cut.Find("[data-testid='manufacturing-review']").Click();
        Assert.NotEmpty(cut.FindAll("[role='alert']"));
        cut.Find("[data-testid='manufacturing-project']").Input("Pump bracket pilot");
        cut.Find("[data-testid='manufacturing-part']").Input("mounting-bracket.step");
        cut.Find("[data-testid='manufacturing-attach']").Click();
        cut.Find("[data-testid='manufacturing-review']").Click();
        cut.Find("[data-testid='manufacturing-open-confirmation']").Click();
        Assert.NotEmpty(cut.FindAll("[role='dialog'][aria-modal='true']"));
        cut.Find("[data-testid='manufacturing-confirm']").Click();

        Assert.Contains("MR-240812", cut.Markup, StringComparison.Ordinal);
        Assert.NotEmpty(cut.FindAll("[role='status']"));
    }

    [Fact]
    public void CustomerCompositionUsesThaiFixturesAndCompletesTheMessageWorkflow()
    {
        var state = new MockSiteState();
        var cut = Render<CustomerWorkspaceMock>(parameters => parameters
            .Add(component => component.State, state)
            .Add(component => component.Locale, ThemeStudioLocale.English));

        Assert.NotEmpty(cut.FindAll("nav[aria-label='Breadcrumb']"));
        Assert.NotEmpty(cut.FindAll("table"));
        Assert.Contains("กานต์ชนก ศรีสุข", cut.Markup, StringComparison.Ordinal);

        cut.Find("[data-testid='customer-query']").Input("วริศรา");
        Assert.Single(cut.FindAll("tbody tr"));
        cut.Find("[data-testid='customer-open-CUS-103']").Click();
        Assert.NotEmpty(cut.FindAll("[role='tablist']"));
        cut.Find("[data-testid='customer-tab-messages']").Click();
        cut.Find("[data-testid='customer-message']").Input("Your revised drawing is ready.");
        cut.Find("[data-testid='customer-send-message']").Click();

        Assert.Contains("Message sent to วริศรา ตั้งใจ", cut.Find("[role='status']").TextContent, StringComparison.Ordinal);
        Assert.NotEmpty(cut.FindAll("[data-testid='customer-detail-sheet']"));
    }

    [Fact]
    public void ThaiManufacturingValidationAndAnnouncementsAreLocalizedAtRenderBoundary()
    {
        var state = new MockSiteState();
        var cut = Render<ManufacturingRequestMock>(parameters => parameters
            .Add(component => component.State, state)
            .Add(component => component.Locale, ThemeStudioLocale.Thai));

        cut.Find("[data-testid='manufacturing-review']").Click();
        Assert.Contains("กรุณาระบุชื่อโครงการ", cut.Find("[data-testid='manufacturing-errors']").TextContent, StringComparison.Ordinal);
        Assert.DoesNotContain("required", cut.Find("[data-testid='manufacturing-errors']").TextContent, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain("Please resolve", cut.Find("[role='status']").TextContent, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public void ThaiOperationsAnnouncementIsLocalizedAtRenderBoundary()
    {
        var state = new MockSiteState();
        var cut = Render<OperationsDashboardMock>(parameters => parameters
            .Add(component => component.State, state)
            .Add(component => component.Locale, ThemeStudioLocale.Thai));

        cut.Find("[data-testid='operation-complete-MO-24018']").Click();

        Assert.Contains("ย้าย MO-24018 ไปยังสถานะเสร็จสมบูรณ์แล้ว", cut.Find("[data-testid='operations-announcement']").TextContent, StringComparison.Ordinal);
    }
}

public sealed class MockSiteLedgerContractTests
{
    [Fact]
    public void EveryRenderedShadcnTypeBelongsToACompleteLedgerFamily()
    {
        var root = FindRepositoryRoot();
        using var ledger = JsonDocument.Parse(File.ReadAllText(Path.Combine(root, "docs", "component-catalog.json")));
        var families = ledger.RootElement.GetProperty("components").EnumerateArray()
            .Select(component => new
            {
                Name = Regex.Replace(component.GetProperty("name").GetString()!, "[^A-Za-z0-9]", string.Empty),
                Status = component.GetProperty("status").GetString()!
            })
            .OrderByDescending(component => component.Name.Length)
            .ToArray();
        var sourceDirectory = Path.Combine(root, "samples", "Maliev.ShadcnBlazor.Showcase", "MockSites");
        var violations = new List<string>();

        foreach (var file in Directory.GetFiles(sourceDirectory, "*.razor"))
        {
            foreach (Match match in Regex.Matches(File.ReadAllText(file), @"<Shadcn(?<type>[A-Za-z0-9]+)"))
            {
                var type = match.Groups["type"].Value;
                var family = families.FirstOrDefault(candidate => type.StartsWith(candidate.Name, StringComparison.Ordinal));
                if (family is null || !string.Equals(family.Status, "complete", StringComparison.Ordinal))
                    violations.Add($"{Path.GetFileName(file)}: Shadcn{type} -> {family?.Status ?? "missing"}");
            }
        }

        Assert.Empty(violations);
    }

    private static string FindRepositoryRoot()
    {
        var directory = new DirectoryInfo(AppContext.BaseDirectory);
        while (directory is not null && !File.Exists(Path.Combine(directory.FullName, "Maliev.ShadcnBlazor.slnx")))
            directory = directory.Parent;
        return directory?.FullName ?? throw new DirectoryNotFoundException("Repository root not found.");
    }
}
