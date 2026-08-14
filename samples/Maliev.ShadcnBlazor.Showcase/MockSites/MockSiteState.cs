using Maliev.ShadcnBlazor.Showcase.Theming;

namespace Maliev.ShadcnBlazor.Showcase.MockSites;

public enum MockDisplayState { Ready, Loading, Empty, Error }
public enum ManufacturingStage { Details, Review, Success }
public enum CustomerWorkspaceTab { Overview, Activity, Messages }
public enum OperationStatus { InProgress, Queued, QualityCheck, Completed }
public enum CustomerStatus { Review, Active, Waiting }
public enum CustomerActivity { DrawingApproved, QuoteShared, RevisionRequested }
public enum ManufacturingValidationError { ProjectRequired, PartRequired, AttachmentRequired }
public enum MockAnnouncement { None, OperationCompleted, AttachmentAdded, ValidationFailed, ReviewReady, ManufacturingConfirmed, CustomerMessageSent }
public enum CustomerMessageAuthor { Engineering, Customer }
public enum CustomerMessageKind { DfmReviewReady, PreserveMountingPattern, Custom }

public sealed record OperationJob(
    string Id,
    string Part,
    string Process,
    string Material,
    DateOnly DueDate,
    OperationStatus Status,
    int Progress)
{
    public string SearchText => $"{Id} {Part} {Process} {Material} {Status}";
}

public sealed record OperationsMockState(
    string Query,
    bool SortDescending,
    int Page,
    MockDisplayState DisplayState,
    IReadOnlyList<OperationJob> Jobs,
    MockAnnouncement Announcement,
    string? AnnouncementSubject);

public sealed record ManufacturingMockState(
    string Project,
    string Part,
    string Material,
    string Process,
    string Finish,
    string RequiredDate,
    string? Attachment,
    bool InspectionReport,
    ManufacturingStage Stage,
    MockDisplayState DisplayState,
    IReadOnlyList<ManufacturingValidationError> Errors,
    bool ConfirmationOpen,
    string? RequestId,
    MockAnnouncement Announcement,
    string? AnnouncementSubject);

public sealed record CustomerRecord(
    string Id,
    string Name,
    string Initials,
    string Company,
    string Project,
    CustomerStatus Status,
    CustomerActivity LastActivity)
{
    public string SearchText => $"{Id} {Name} {Company} {Project} {Status}";
}

public sealed record CustomerMessage(string Id, CustomerMessageAuthor Author, CustomerMessageKind Kind, string? Text, string DisplayTime);

public sealed record CustomerWorkspaceMockState(
    string Query,
    CustomerWorkspaceTab ActiveTab,
    MockDisplayState DisplayState,
    IReadOnlyList<CustomerRecord> Records,
    string? SelectedCustomerId,
    bool DetailOpen,
    string MessageDraft,
    IReadOnlyList<CustomerMessage> Messages,
    MockAnnouncement Announcement,
    string? AnnouncementSubject);

public sealed class MockSiteState
{
    private const int OperationsPageSize = 3;

    private static readonly IReadOnlyList<OperationJob> OperationFixtures =
    [
        new("MO-24018", "Valve manifold", "CNC milling", "Aluminum 6061", new DateOnly(2026, 8, 28), OperationStatus.InProgress, 72),
        new("MO-24019", "Sensor housing", "CNC turning", "Aluminum 7075", new DateOnly(2026, 8, 29), OperationStatus.Queued, 18),
        new("MO-24020", "Conveyor guide", "Laser cutting", "Stainless 304", new DateOnly(2026, 8, 30), OperationStatus.QualityCheck, 92),
        new("MO-24021", "Pump bracket", "FDM printing", "PA-CF", new DateOnly(2026, 9, 2), OperationStatus.InProgress, 48),
        new("MO-24022", "Drive cover", "Sheet forming", "Mild steel", new DateOnly(2026, 9, 4), OperationStatus.Queued, 8)
    ];

    private static readonly IReadOnlyList<CustomerRecord> CustomerFixtures =
    [
        new("CUS-101", "กานต์ชนก ศรีสุข", "กศ", "Siam Motion Labs", "Robot gripper pilot", CustomerStatus.Review, CustomerActivity.DrawingApproved),
        new("CUS-102", "ปริญญา จิตมั่น", "ปจ", "Chao Phraya Systems", "Packaging line retrofit", CustomerStatus.Active, CustomerActivity.QuoteShared),
        new("CUS-103", "วริศรา ตั้งใจ", "วต", "Northern Precision", "Pump bracket tooling", CustomerStatus.Waiting, CustomerActivity.RevisionRequested)
    ];

    private static readonly IReadOnlyList<CustomerMessage> CustomerMessageFixtures =
    [
        new("MSG-01", CustomerMessageAuthor.Engineering, CustomerMessageKind.DfmReviewReady, null, "09:30"),
        new("MSG-02", CustomerMessageAuthor.Customer, CustomerMessageKind.PreserveMountingPattern, null, "10:15")
    ];

    private static readonly IReadOnlyList<ManufacturingValidationError> NoErrors = Array.Empty<ManufacturingValidationError>();

    private static readonly OperationsMockState OperationsBaseline = new(
        string.Empty,
        false,
        1,
        MockDisplayState.Ready,
        OperationFixtures,
        MockAnnouncement.None,
        null);

    private static readonly ManufacturingMockState ManufacturingBaseline = new(
        string.Empty,
        string.Empty,
        "Aluminum 6061",
        "CNC milling",
        "As machined",
        "2026-08-28",
        null,
        false,
        ManufacturingStage.Details,
        MockDisplayState.Ready,
        NoErrors,
        false,
        null,
        MockAnnouncement.None,
        null);

    private static readonly CustomerWorkspaceMockState CustomersBaseline = new(
        string.Empty,
        CustomerWorkspaceTab.Overview,
        MockDisplayState.Ready,
        CustomerFixtures,
        null,
        false,
        string.Empty,
        CustomerMessageFixtures,
        MockAnnouncement.None,
        null);

    public OperationsMockState Operations { get; private set; } = OperationsBaseline;
    public ManufacturingMockState Manufacturing { get; private set; } = ManufacturingBaseline;
    public CustomerWorkspaceMockState Customers { get; private set; } = CustomersBaseline;

    public IReadOnlyList<OperationJob> FilteredOperations
    {
        get
        {
            var filtered = string.IsNullOrWhiteSpace(Operations.Query)
                ? Operations.Jobs
                : Operations.Jobs.Where(job => job.SearchText.Contains(Operations.Query.Trim(), StringComparison.OrdinalIgnoreCase)).ToArray();
            var sorted = Operations.SortDescending
                ? filtered.OrderByDescending(job => job.DueDate)
                : filtered.OrderBy(job => job.DueDate);
            return sorted.Skip((Operations.Page - 1) * OperationsPageSize).Take(OperationsPageSize).ToArray();
        }
    }

    public int OperationsPageCount => Math.Max(1, (int)Math.Ceiling(FilteredOperationsCount / (double)OperationsPageSize));
    private int FilteredOperationsCount => string.IsNullOrWhiteSpace(Operations.Query)
        ? Operations.Jobs.Count
        : Operations.Jobs.Count(job => job.SearchText.Contains(Operations.Query.Trim(), StringComparison.OrdinalIgnoreCase));

    public IReadOnlyList<CustomerRecord> FilteredCustomers => string.IsNullOrWhiteSpace(Customers.Query)
        ? Customers.Records
        : Customers.Records.Where(customer => customer.SearchText.Contains(Customers.Query.Trim(), StringComparison.OrdinalIgnoreCase)).ToArray();

    public CustomerRecord? SelectedCustomer => Customers.Records.FirstOrDefault(customer => customer.Id == Customers.SelectedCustomerId);

    public void Reset(ThemeStudioMockup? mockup = null)
    {
        if (mockup is null or ThemeStudioMockup.OperationsDashboard)
            Operations = OperationsBaseline;
        if (mockup is null or ThemeStudioMockup.ManufacturingRequest)
            Manufacturing = ManufacturingBaseline;
        if (mockup is null or ThemeStudioMockup.CustomerWorkspace)
            Customers = CustomersBaseline;
    }

    public void SetOperationsQuery(string? query) => Operations = Operations with { Query = query?.TrimStart() ?? string.Empty, Page = 1 };
    public void ToggleOperationsSort() => Operations = Operations with { SortDescending = !Operations.SortDescending, Page = 1 };
    public void NextOperationsPage() => Operations = Operations with { Page = Math.Min(Operations.Page + 1, OperationsPageCount) };
    public void PreviousOperationsPage() => Operations = Operations with { Page = Math.Max(1, Operations.Page - 1) };
    public void SetOperationsDisplayState(MockDisplayState displayState) => Operations = Operations with { DisplayState = displayState };

    public void CompleteOperation(string id)
    {
        var job = Operations.Jobs.FirstOrDefault(item => item.Id == id)
            ?? throw new ArgumentException($"Unknown operation '{id}'.", nameof(id));
        var jobs = Operations.Jobs.Select(item => item.Id == job.Id ? item with { Status = OperationStatus.Completed, Progress = 100 } : item).ToArray();
        Operations = Operations with { Jobs = jobs, Announcement = MockAnnouncement.OperationCompleted, AnnouncementSubject = id, DisplayState = MockDisplayState.Ready };
    }

    public void SetManufacturingProject(string? value) => Manufacturing = Manufacturing with { Project = value ?? string.Empty, Errors = Manufacturing.Errors.Where(error => error != ManufacturingValidationError.ProjectRequired).ToArray() };
    public void SetManufacturingPart(string? value) => Manufacturing = Manufacturing with { Part = value ?? string.Empty, Errors = Manufacturing.Errors.Where(error => error != ManufacturingValidationError.PartRequired).ToArray() };
    public void SetManufacturingMaterial(string? value) => Manufacturing = Manufacturing with { Material = value ?? string.Empty };
    public void SetManufacturingProcess(string? value) => Manufacturing = Manufacturing with { Process = value ?? string.Empty };
    public void SetManufacturingFinish(string? value) => Manufacturing = Manufacturing with { Finish = value ?? string.Empty };
    public void SetManufacturingRequiredDate(string? value) => Manufacturing = Manufacturing with { RequiredDate = value ?? string.Empty };
    public void SetManufacturingInspectionReport(bool value) => Manufacturing = Manufacturing with { InspectionReport = value };
    public void SetManufacturingDisplayState(MockDisplayState value) => Manufacturing = Manufacturing with { DisplayState = value };
    public void SetManufacturingStage(ManufacturingStage value)
    {
        if (!Enum.IsDefined(value))
            throw new ArgumentOutOfRangeException(nameof(value), value, "Unknown manufacturing stage.");
        Manufacturing = Manufacturing with { Stage = value, ConfirmationOpen = false };
    }
    public void AttachManufacturingFile() => Manufacturing = Manufacturing with { Attachment = "pump-bracket-r3.step", Errors = Manufacturing.Errors.Where(error => error != ManufacturingValidationError.AttachmentRequired).ToArray(), Announcement = MockAnnouncement.AttachmentAdded, AnnouncementSubject = "pump-bracket-r3.step" };

    public bool ReviewManufacturingRequest()
    {
        var errors = new List<ManufacturingValidationError>();
        if (string.IsNullOrWhiteSpace(Manufacturing.Project)) errors.Add(ManufacturingValidationError.ProjectRequired);
        if (string.IsNullOrWhiteSpace(Manufacturing.Part)) errors.Add(ManufacturingValidationError.PartRequired);
        if (string.IsNullOrWhiteSpace(Manufacturing.Attachment)) errors.Add(ManufacturingValidationError.AttachmentRequired);
        if (errors.Count > 0)
        {
            Manufacturing = Manufacturing with { Errors = errors, Announcement = MockAnnouncement.ValidationFailed, AnnouncementSubject = null };
            return false;
        }

        Manufacturing = Manufacturing with { Errors = NoErrors, Stage = ManufacturingStage.Review, Announcement = MockAnnouncement.ReviewReady, AnnouncementSubject = null };
        return true;
    }

    public bool OpenManufacturingConfirmation()
    {
        if (Manufacturing.Stage != ManufacturingStage.Review)
            return false;
        Manufacturing = Manufacturing with { ConfirmationOpen = true };
        return true;
    }

    public void CloseManufacturingConfirmation() => Manufacturing = Manufacturing with { ConfirmationOpen = false };

    public void ConfirmManufacturingRequest()
    {
        if (!Manufacturing.ConfirmationOpen)
            throw new InvalidOperationException("The manufacturing confirmation is not open.");
        Manufacturing = Manufacturing with
        {
            Stage = ManufacturingStage.Success,
            ConfirmationOpen = false,
            RequestId = "MR-240812",
            Announcement = MockAnnouncement.ManufacturingConfirmed,
            AnnouncementSubject = "MR-240812"
        };
    }

    public void SetCustomerQuery(string? value) => Customers = Customers with { Query = value?.TrimStart() ?? string.Empty };
    public void SetCustomerTab(CustomerWorkspaceTab tab) => Customers = Customers with { ActiveTab = tab };
    public void SetCustomerDisplayState(MockDisplayState state) => Customers = Customers with { DisplayState = state };

    public void OpenCustomer(string id)
    {
        if (!Customers.Records.Any(customer => customer.Id == id))
            throw new ArgumentException($"Unknown customer '{id}'.", nameof(id));
        Customers = Customers with { SelectedCustomerId = id, DetailOpen = true, ActiveTab = CustomerWorkspaceTab.Overview };
    }

    public void CloseCustomer() => Customers = Customers with { DetailOpen = false, SelectedCustomerId = null, MessageDraft = string.Empty };
    public void SetCustomerMessage(string? value) => Customers = Customers with { MessageDraft = value ?? string.Empty };

    public bool SendCustomerMessage()
    {
        var customer = SelectedCustomer;
        if (customer is null || string.IsNullOrWhiteSpace(Customers.MessageDraft))
            return false;
        var messages = Customers.Messages.Concat([
            new CustomerMessage("MSG-03", CustomerMessageAuthor.Engineering, CustomerMessageKind.Custom, Customers.MessageDraft.Trim(), "11:00")
        ]).ToArray();
        Customers = Customers with
        {
            Messages = messages,
            MessageDraft = string.Empty,
            Announcement = MockAnnouncement.CustomerMessageSent,
            AnnouncementSubject = customer.Name
        };
        return true;
    }
}
