using System.Collections;
using System.Collections.ObjectModel;
using System.Globalization;
using System.Reflection;
using Maliev.ShadcnBlazor.Components.Primitives;
using Maliev.ShadcnBlazor.Components.Selection;
using Microsoft.AspNetCore.Components;

namespace Maliev.ShadcnBlazor.Showcase.Documentation.Api;

public sealed class ComponentApiCatalog
{
    private static readonly string[] OwnedNamespaces =
    [
        "Maliev.ShadcnBlazor.Components.Actions",
        "Maliev.ShadcnBlazor.Components.Content",
        "Maliev.ShadcnBlazor.Components.Conversation",
        "Maliev.ShadcnBlazor.Components.DataDisplay",
        "Maliev.ShadcnBlazor.Components.Disclosure",
        "Maliev.ShadcnBlazor.Components.Direction",
        "Maliev.ShadcnBlazor.Components.Forms",
        "Maliev.ShadcnBlazor.Components.Feedback",
        "Maliev.ShadcnBlazor.Components.Feedback.Toast",
        "Maliev.ShadcnBlazor.Components.Layout",
        "Maliev.ShadcnBlazor.Components.Navigation",
        "Maliev.ShadcnBlazor.Components.Navigation.Sidebar",
        "Maliev.ShadcnBlazor.Components.Overlays",
        "Maliev.ShadcnBlazor.Components.Primitives",
        "Maliev.ShadcnBlazor.Components.Selection",
        "Maliev.ShadcnBlazor.Components.Styling",
        "Maliev.ShadcnBlazor.Components.Typography"
    ];

    private static readonly IReadOnlyDictionary<string, string> CuratedDescriptions = new ReadOnlyDictionary<string, string>(
        new Dictionary<string, string>(StringComparer.Ordinal)
        {
            ["Maliev.ShadcnBlazor.Components.Layout.ShadcnAspectRatio.Ratio"] = "Width divided by height for the rendered content frame.",
            ["Maliev.ShadcnBlazor.Components.Layout.ShadcnBentoGrid.Columns"] = "Maximum track count used when the grid container is wide.",
            ["Maliev.ShadcnBlazor.Components.Layout.ShadcnBentoGrid.MediumColumns"] = "Track count used when the grid container reaches its intermediate size.",
            ["Maliev.ShadcnBlazor.Components.Layout.ShadcnBentoGrid.Gap"] = "Optional non-negative CSS length between Bento items.",
            ["Maliev.ShadcnBlazor.Components.Layout.ShadcnBentoItem.ColumnSpan"] = "Maximum number of responsive tracks occupied by this item.",
            ["Maliev.ShadcnBlazor.Components.Layout.ShadcnBentoItem.RowSpan"] = "Number of automatic grid rows occupied by this item.",
            ["Maliev.ShadcnBlazor.Components.Direction.ShadcnDirectionProvider.Direction"] = "Reading direction applied to this subtree; inherits the parent direction when omitted.",
            ["Maliev.ShadcnBlazor.Components.Content.ShadcnSeparator.Decorative"] = "When true, hides the separator from assistive technology.",
            ["Maliev.ShadcnBlazor.Components.Selection.ShadcnCheckbox.Value"] = "Controlled checked state; null renders the indeterminate state.",
            ["Maliev.ShadcnBlazor.Components.Selection.ShadcnSlider.Values"] = "Controlled one- or two-thumb slider values."
        });

    private static readonly IReadOnlyDictionary<string, string> CuratedConstraints = new ReadOnlyDictionary<string, string>(
        new Dictionary<string, string>(StringComparer.Ordinal)
        {
            ["Maliev.ShadcnBlazor.Components.Layout.ShadcnAspectRatio.Ratio"] = "Must be positive and finite.",
            ["Maliev.ShadcnBlazor.Components.Layout.ShadcnBentoGrid.Columns"] = "Allowed values: 1 through 4.",
            ["Maliev.ShadcnBlazor.Components.Layout.ShadcnBentoGrid.MediumColumns"] = "Allowed values: 1 through the configured maximum column count.",
            ["Maliev.ShadcnBlazor.Components.Layout.ShadcnBentoGrid.Gap"] = "Must be a non-negative CSS length without additional declarations.",
            ["Maliev.ShadcnBlazor.Components.Layout.ShadcnBentoItem.ColumnSpan"] = "Allowed values: 1 through 4.",
            ["Maliev.ShadcnBlazor.Components.Layout.ShadcnBentoItem.RowSpan"] = "Allowed values: 1 through 4.",
            ["Maliev.ShadcnBlazor.Components.Typography.ShadcnTypeset.Tag"] = "Allowed values: div, article, section.",
            ["Maliev.ShadcnBlazor.Components.Selection.ShadcnSlider.Step"] = "Must be positive and align values within the minimum and maximum range.",
            ["Maliev.ShadcnBlazor.Components.Selection.ShadcnSlider.Values"] = "One value creates a single thumb; two values create a range."
        });

    private static readonly IReadOnlyDictionary<string, IReadOnlySet<string>> ExplicitComponentTypes =
        new ReadOnlyDictionary<string, IReadOnlySet<string>>(new Dictionary<string, IReadOnlySet<string>>(StringComparer.Ordinal)
        {
            ["bento-grid"] = Types(
                "Maliev.ShadcnBlazor.Components.Layout.ShadcnBentoGrid",
                "Maliev.ShadcnBlazor.Components.Layout.ShadcnBentoItem"),
            ["visual-style-scope"] = Types(
                "Maliev.ShadcnBlazor.Components.Styling.ShadcnVisualStyleScope",
                "Maliev.ShadcnBlazor.Components.Styling.ShadcnVisualStyle",
                "Maliev.ShadcnBlazor.Components.Styling.ShadcnColorTreatment",
                "Maliev.ShadcnBlazor.Components.Styling.ShadcnDepthTreatment",
                "Maliev.ShadcnBlazor.Components.Styling.ShadcnMotionTreatment",
                "Maliev.ShadcnBlazor.Components.Styling.ShadcnStyleIntensity"),
            ["accordion"] = Types(
                "Maliev.ShadcnBlazor.Components.Disclosure.ShadcnAccordion",
                "Maliev.ShadcnBlazor.Components.Disclosure.ShadcnAccordionItem",
                "Maliev.ShadcnBlazor.Components.Disclosure.ShadcnAccordionTrigger",
                "Maliev.ShadcnBlazor.Components.Disclosure.ShadcnAccordionContent"),
            ["breadcrumb"] = Types(
                "Maliev.ShadcnBlazor.Components.Navigation.ShadcnBreadcrumb",
                "Maliev.ShadcnBlazor.Components.Navigation.ShadcnBreadcrumbList",
                "Maliev.ShadcnBlazor.Components.Navigation.ShadcnBreadcrumbItem",
                "Maliev.ShadcnBlazor.Components.Navigation.ShadcnBreadcrumbLink",
                "Maliev.ShadcnBlazor.Components.Navigation.ShadcnBreadcrumbPage",
                "Maliev.ShadcnBlazor.Components.Navigation.ShadcnBreadcrumbSeparator",
                "Maliev.ShadcnBlazor.Components.Navigation.ShadcnBreadcrumbEllipsis"),
            ["collapsible"] = Types(
                "Maliev.ShadcnBlazor.Components.Disclosure.ShadcnCollapsible",
                "Maliev.ShadcnBlazor.Components.Disclosure.ShadcnCollapsibleTrigger",
                "Maliev.ShadcnBlazor.Components.Disclosure.ShadcnCollapsibleContent"),
            ["navigation-menu"] = Types(
                "Maliev.ShadcnBlazor.Components.Navigation.ShadcnNavigationMenu",
                "Maliev.ShadcnBlazor.Components.Navigation.ShadcnNavigationMenuList",
                "Maliev.ShadcnBlazor.Components.Navigation.ShadcnNavigationMenuItem",
                "Maliev.ShadcnBlazor.Components.Navigation.ShadcnNavigationMenuTrigger",
                "Maliev.ShadcnBlazor.Components.Navigation.ShadcnNavigationMenuContent",
                "Maliev.ShadcnBlazor.Components.Navigation.ShadcnNavigationMenuLink",
                "Maliev.ShadcnBlazor.Components.Navigation.ShadcnNavigationMenuIndicator",
                "Maliev.ShadcnBlazor.Components.Navigation.ShadcnNavigationMenuViewport"),
            ["pagination"] = Types(
                "Maliev.ShadcnBlazor.Components.Navigation.ShadcnPagination",
                "Maliev.ShadcnBlazor.Components.Navigation.ShadcnPaginationContent",
                "Maliev.ShadcnBlazor.Components.Navigation.ShadcnPaginationItem",
                "Maliev.ShadcnBlazor.Components.Navigation.ShadcnPaginationLink",
                "Maliev.ShadcnBlazor.Components.Navigation.ShadcnPaginationPrevious",
                "Maliev.ShadcnBlazor.Components.Navigation.ShadcnPaginationNext",
                "Maliev.ShadcnBlazor.Components.Navigation.ShadcnPaginationEllipsis",
                "Maliev.ShadcnBlazor.Components.Navigation.ShadcnPaginationPages"),
            ["resizable"] = Types(
                "Maliev.ShadcnBlazor.Components.Layout.ShadcnResizableGroup",
                "Maliev.ShadcnBlazor.Components.Layout.ShadcnResizablePanel",
                "Maliev.ShadcnBlazor.Components.Layout.ShadcnResizableHandle",
                "Maliev.ShadcnBlazor.Components.Layout.IShadcnResizableStateStore"),
            ["scroll-area"] = Types(
                "Maliev.ShadcnBlazor.Components.Layout.ShadcnScrollArea",
                "Maliev.ShadcnBlazor.Components.Layout.ShadcnScrollAreaViewport",
                "Maliev.ShadcnBlazor.Components.Layout.ShadcnScrollAreaScrollbar",
                "Maliev.ShadcnBlazor.Components.Layout.ShadcnScrollAreaThumb",
                "Maliev.ShadcnBlazor.Components.Layout.ShadcnScrollAreaCorner"),
            ["sidebar"] = Types(
                "Maliev.ShadcnBlazor.Components.Navigation.Sidebar.ShadcnSidebarProvider",
                "Maliev.ShadcnBlazor.Components.Navigation.Sidebar.ShadcnSidebar",
                "Maliev.ShadcnBlazor.Components.Navigation.Sidebar.ShadcnSidebarTrigger",
                "Maliev.ShadcnBlazor.Components.Navigation.Sidebar.ShadcnSidebarRail",
                "Maliev.ShadcnBlazor.Components.Navigation.Sidebar.ShadcnSidebarInset",
                "Maliev.ShadcnBlazor.Components.Navigation.Sidebar.ShadcnSidebarInput",
                "Maliev.ShadcnBlazor.Components.Navigation.Sidebar.ShadcnSidebarHeader",
                "Maliev.ShadcnBlazor.Components.Navigation.Sidebar.ShadcnSidebarFooter",
                "Maliev.ShadcnBlazor.Components.Navigation.Sidebar.ShadcnSidebarContent",
                "Maliev.ShadcnBlazor.Components.Navigation.Sidebar.ShadcnSidebarSeparator",
                "Maliev.ShadcnBlazor.Components.Navigation.Sidebar.ShadcnSidebarGroup",
                "Maliev.ShadcnBlazor.Components.Navigation.Sidebar.ShadcnSidebarGroupLabel",
                "Maliev.ShadcnBlazor.Components.Navigation.Sidebar.ShadcnSidebarGroupAction",
                "Maliev.ShadcnBlazor.Components.Navigation.Sidebar.ShadcnSidebarGroupContent",
                "Maliev.ShadcnBlazor.Components.Navigation.Sidebar.ShadcnSidebarMenu",
                "Maliev.ShadcnBlazor.Components.Navigation.Sidebar.ShadcnSidebarMenuItem",
                "Maliev.ShadcnBlazor.Components.Navigation.Sidebar.ShadcnSidebarMenuButton",
                "Maliev.ShadcnBlazor.Components.Navigation.Sidebar.ShadcnSidebarMenuAction",
                "Maliev.ShadcnBlazor.Components.Navigation.Sidebar.ShadcnSidebarMenuBadge",
                "Maliev.ShadcnBlazor.Components.Navigation.Sidebar.ShadcnSidebarMenuSkeleton",
                "Maliev.ShadcnBlazor.Components.Navigation.Sidebar.ShadcnSidebarMenuSub",
                "Maliev.ShadcnBlazor.Components.Navigation.Sidebar.ShadcnSidebarMenuSubItem",
                "Maliev.ShadcnBlazor.Components.Navigation.Sidebar.ShadcnSidebarMenuSubButton",
                "Maliev.ShadcnBlazor.Components.Navigation.Sidebar.IShadcnSidebarStateStore"),
            ["tabs"] = Types(
                "Maliev.ShadcnBlazor.Components.Navigation.ShadcnTabs",
                "Maliev.ShadcnBlazor.Components.Navigation.ShadcnTabsList",
                "Maliev.ShadcnBlazor.Components.Navigation.ShadcnTabsTrigger",
                "Maliev.ShadcnBlazor.Components.Navigation.ShadcnTabsContent"),
            ["alert"] = Types(
                "Maliev.ShadcnBlazor.Components.Feedback.ShadcnAlert",
                "Maliev.ShadcnBlazor.Components.Feedback.ShadcnAlertIcon",
                "Maliev.ShadcnBlazor.Components.Feedback.ShadcnAlertTitle",
                "Maliev.ShadcnBlazor.Components.Feedback.ShadcnAlertDescription",
                "Maliev.ShadcnBlazor.Components.Feedback.ShadcnAlertAction"),
            ["avatar"] = Types(
                "Maliev.ShadcnBlazor.Components.Content.ShadcnAvatar",
                "Maliev.ShadcnBlazor.Components.Content.ShadcnAvatarImage",
                "Maliev.ShadcnBlazor.Components.Content.ShadcnAvatarFallback",
                "Maliev.ShadcnBlazor.Components.Content.ShadcnAvatarBadge",
                "Maliev.ShadcnBlazor.Components.Content.ShadcnAvatarGroup",
                "Maliev.ShadcnBlazor.Components.Content.ShadcnAvatarGroupCount"),
            ["badge"] = Types("Maliev.ShadcnBlazor.Components.Content.ShadcnBadge"),
            ["card"] = Types(
                "Maliev.ShadcnBlazor.Components.Content.ShadcnCard",
                "Maliev.ShadcnBlazor.Components.Content.ShadcnCardHeader",
                "Maliev.ShadcnBlazor.Components.Content.ShadcnCardTitle",
                "Maliev.ShadcnBlazor.Components.Content.ShadcnCardDescription",
                "Maliev.ShadcnBlazor.Components.Content.ShadcnCardAction",
                "Maliev.ShadcnBlazor.Components.Content.ShadcnCardContent",
                "Maliev.ShadcnBlazor.Components.Content.ShadcnCardFooter"),
            ["carousel"] = Types(
                "Maliev.ShadcnBlazor.Components.Content.ShadcnCarousel",
                "Maliev.ShadcnBlazor.Components.Content.ShadcnCarouselContent",
                "Maliev.ShadcnBlazor.Components.Content.ShadcnCarouselItem",
                "Maliev.ShadcnBlazor.Components.Content.ShadcnCarouselPrevious",
                "Maliev.ShadcnBlazor.Components.Content.ShadcnCarouselNext",
                "Maliev.ShadcnBlazor.Components.Content.ShadcnCarouselOptions",
                "Maliev.ShadcnBlazor.Components.Content.ShadcnCarouselAutoplayPlugin"),
            ["progress"] = Types(
                "Maliev.ShadcnBlazor.Components.Feedback.ShadcnProgress",
                "Maliev.ShadcnBlazor.Components.Feedback.ShadcnProgressLabel",
                "Maliev.ShadcnBlazor.Components.Feedback.ShadcnProgressValue",
                "Maliev.ShadcnBlazor.Components.Feedback.ShadcnProgressTrack",
                "Maliev.ShadcnBlazor.Components.Feedback.ShadcnProgressIndicator"),
            ["skeleton"] = Types("Maliev.ShadcnBlazor.Components.Feedback.ShadcnSkeleton"),
            ["spinner"] = Types("Maliev.ShadcnBlazor.Components.Feedback.ShadcnSpinner"),
            ["button"] = Types("Maliev.ShadcnBlazor.Components.Actions.ShadcnButton"),
            ["button-group"] = Types(
                "Maliev.ShadcnBlazor.Components.Actions.ShadcnButtonGroup",
                "Maliev.ShadcnBlazor.Components.Actions.ShadcnButtonGroupSeparator",
                "Maliev.ShadcnBlazor.Components.Actions.ShadcnButtonGroupText"),
            ["checkbox"] = Types("Maliev.ShadcnBlazor.Components.Selection.ShadcnCheckbox"),
            ["code-block"] = Types("Maliev.ShadcnBlazor.Components.Typography.ShadcnCodeBlock"),
            ["radio-group"] = Types(
                "Maliev.ShadcnBlazor.Components.Selection.ShadcnRadioGroup`1",
                "Maliev.ShadcnBlazor.Components.Selection.ShadcnRadioGroupItem`1"),
            ["slider"] = Types(
                "Maliev.ShadcnBlazor.Components.Selection.ShadcnSlider",
                "Maliev.ShadcnBlazor.Components.Selection.ShadcnSliderThumbAttributes"),
            ["switch"] = Types("Maliev.ShadcnBlazor.Components.Selection.ShadcnSwitch"),
            ["toggle"] = Types("Maliev.ShadcnBlazor.Components.Actions.ShadcnToggle"),
            ["toggle-group"] = Types(
                "Maliev.ShadcnBlazor.Components.Actions.ShadcnToggleGroup`1",
                "Maliev.ShadcnBlazor.Components.Actions.ShadcnToggleGroupItem`1"),
            ["calendar"] = Types(
                "Maliev.ShadcnBlazor.Components.Forms.ShadcnCalendar",
                "Maliev.ShadcnBlazor.Components.Forms.ShadcnDateRange"),
            ["combobox"] = Types(
                "Maliev.ShadcnBlazor.Components.Forms.ShadcnCombobox`1",
                "Maliev.ShadcnBlazor.Components.Forms.ShadcnComboboxOption`1"),
            ["date-picker"] = Types(
                "Maliev.ShadcnBlazor.Components.Forms.ShadcnDatePicker",
                "Maliev.ShadcnBlazor.Components.Forms.ShadcnDateRange"),
            ["dropzone"] = Types(
                "Maliev.ShadcnBlazor.Components.Forms.ShadcnDropzone",
                "Maliev.ShadcnBlazor.Components.Forms.ShadcnDropzoneSelection",
                "Maliev.ShadcnBlazor.Components.Forms.ShadcnDropzoneError",
                "Maliev.ShadcnBlazor.Components.Forms.ShadcnDropzoneValidation"),
            ["input"] = Types("Maliev.ShadcnBlazor.Components.Forms.ShadcnInput`1"),
            ["input-group"] = Types(
                "Maliev.ShadcnBlazor.Components.Forms.ShadcnInputGroup",
                "Maliev.ShadcnBlazor.Components.Forms.ShadcnInputGroupAddon",
                "Maliev.ShadcnBlazor.Components.Forms.ShadcnInputGroupButton",
                "Maliev.ShadcnBlazor.Components.Forms.ShadcnInputGroupText"),
            ["input-otp"] = Types(
                "Maliev.ShadcnBlazor.Components.Forms.ShadcnInputOtp",
                "Maliev.ShadcnBlazor.Components.Forms.ShadcnInputOtpGroup",
                "Maliev.ShadcnBlazor.Components.Forms.ShadcnInputOtpSeparator",
                "Maliev.ShadcnBlazor.Components.Forms.ShadcnInputOtpSlot"),
            ["native-select"] = Types(
                "Maliev.ShadcnBlazor.Components.Forms.ShadcnNativeSelect`1",
                "Maliev.ShadcnBlazor.Components.Forms.ShadcnNativeSelectOptGroup",
                "Maliev.ShadcnBlazor.Components.Forms.ShadcnNativeSelectOption`1"),
            ["select"] = Types(
                "Maliev.ShadcnBlazor.Components.Forms.ShadcnSelect`1",
                "Maliev.ShadcnBlazor.Components.Forms.ShadcnSelectOption`1"),
            ["textarea"] = Types("Maliev.ShadcnBlazor.Components.Forms.ShadcnTextarea`1"),
            ["toast"] = Types(
                "Maliev.ShadcnBlazor.Components.Feedback.Toast.ShadcnToaster",
                "Maliev.ShadcnBlazor.Components.Feedback.Toast.IShadcnToastService",
                "Maliev.ShadcnBlazor.Components.Feedback.Toast.ShadcnToastService",
                "Maliev.ShadcnBlazor.Components.Feedback.Toast.ShadcnToastOptions",
                "Maliev.ShadcnBlazor.Components.Feedback.Toast.ShadcnToastItem")
        });

    public ComponentApiCatalog()
    {
        var descriptors = typeof(ShadcnComponentBase).Assembly.GetExportedTypes()
            .Where(type => !type.IsEnum && type.Namespace is not null && OwnedNamespaces.Contains(type.Namespace, StringComparer.Ordinal))
            .OrderBy(type => type.FullName, StringComparer.Ordinal)
            .Select(CreateDescriptor)
            .ToArray();
        All = new ReadOnlyCollection<ComponentApiDescriptor>(descriptors);
    }

    public IReadOnlyList<ComponentApiDescriptor> All { get; }

    public IReadOnlyList<ComponentApiDescriptor> GetByEntry(ComponentDocumentationEntry entry)
    {
        ArgumentNullException.ThrowIfNull(entry);
        if (entry.Namespace is null || entry.PrimaryType is null)
            return [];

        var commonBase = typeof(ShadcnComponentBase).FullName!;
        var matches = All.Where(descriptor =>
                descriptor.FullTypeName == commonBase ||
                IsEntryType(entry, descriptor.FullTypeName))
            .ToArray();
        return new ReadOnlyCollection<ComponentApiDescriptor>(matches);
    }

    private static bool IsEntryType(ComponentDocumentationEntry entry, string fullTypeName)
    {
        if (ExplicitComponentTypes.TryGetValue(entry.Slug, out var explicitTypes))
            return explicitTypes.Contains(fullTypeName);

        var prefix = $"{entry.Namespace}.{entry.PrimaryType}";
        if (fullTypeName.StartsWith(prefix, StringComparison.Ordinal))
            return true;

        return entry.Slug == "typography" &&
               fullTypeName == "Maliev.ShadcnBlazor.Components.Typography.ShadcnTypeset";
    }

    private static IReadOnlySet<string> Types(params string[] names) => names.ToHashSet(StringComparer.Ordinal);

    private static ComponentApiDescriptor CreateDescriptor(Type type)
    {
        var properties = type.GetProperties(BindingFlags.Public | BindingFlags.Instance | BindingFlags.DeclaredOnly)
            .Where(property => property.GetCustomAttribute<ParameterAttribute>() is not null || type.IsValueType || type == typeof(ShadcnSliderThumbAttributes))
            .OrderBy(property => property.Name, StringComparer.Ordinal)
            .ToArray();
        var propertyNames = properties.Select(property => property.Name).ToHashSet(StringComparer.Ordinal);
        var defaults = ReadDefaults(type, properties);
        var parameters = properties.Select(property => CreateParameter(type, property, propertyNames, defaults)).ToArray();

        return new ComponentApiDescriptor(
            type.Name.Split('`')[0],
            type.FullName ?? throw new InvalidOperationException("Exported API types require a full name."),
            new ReadOnlyCollection<ComponentParameterDescriptor>(parameters));
    }

    private static ComponentParameterDescriptor CreateParameter(
        Type componentType,
        PropertyInfo property,
        IReadOnlySet<string> propertyNames,
        IReadOnlyDictionary<string, object?> defaults)
    {
        var key = $"{componentType.FullName}.{property.Name}";
        var parameter = property.GetCustomAttribute<ParameterAttribute>();
        return new ComponentParameterDescriptor(
            property.Name,
            FriendlyName(property.PropertyType),
            FormatDefault(defaults.GetValueOrDefault(property.Name)),
            property.GetCustomAttribute<EditorRequiredAttribute>() is not null,
            FindBindingPair(property, propertyNames),
            CuratedDescriptions.GetValueOrDefault(key) ?? Describe(property.Name),
            CuratedConstraints.GetValueOrDefault(key) ?? DescribeConstraints(property.PropertyType),
            parameter?.CaptureUnmatchedValues == true);
    }

    private static IReadOnlyDictionary<string, object?> ReadDefaults(Type type, IReadOnlyList<PropertyInfo> properties)
    {
        try
        {
            var concreteType = type.IsGenericTypeDefinition
                ? type.MakeGenericType(type.GetGenericArguments().Select(_ => typeof(string)).ToArray())
                : type;
            var instance = concreteType.IsAbstract ? null : Activator.CreateInstance(concreteType);
            return properties.ToDictionary(
                property => property.Name,
                property => instance is null
                    ? null
                    : concreteType.GetProperty(property.Name, BindingFlags.Public | BindingFlags.Instance | BindingFlags.DeclaredOnly)?.GetValue(instance),
                StringComparer.Ordinal);
        }
        catch (Exception exception) when (exception is ArgumentException or MemberAccessException or TargetInvocationException)
        {
            return properties.ToDictionary(property => property.Name, _ => (object?)null, StringComparer.Ordinal);
        }
    }

    private static string? FindBindingPair(PropertyInfo property, IReadOnlySet<string> propertyNames)
    {
        if (property.Name.EndsWith("Changed", StringComparison.Ordinal))
        {
            var valueName = property.Name[..^"Changed".Length];
            return propertyNames.Contains(valueName) ? valueName : null;
        }

        var changedName = $"{property.Name}Changed";
        return propertyNames.Contains(changedName) ? changedName : null;
    }

    private static string Describe(string name) => name switch
    {
        "ChildContent" => "Content rendered inside the component.",
        "Disabled" => "Disables user interaction when true.",
        "ReadOnly" => "Prevents value changes while preserving focus and inspection.",
        "Invalid" => "Marks the component as invalid for styling and accessibility.",
        "Class" => "Additional CSS classes merged with the component classes.",
        "Style" => "Additional inline declarations merged with component-owned styles.",
        "AdditionalAttributes" => "Additional HTML attributes forwarded to the component root.",
        _ when name.EndsWith("Changed", StringComparison.Ordinal) => $"Callback raised when {name[..^"Changed".Length]} changes.",
        _ => $"Configures the {SplitWords(name).ToLowerInvariant()} value."
    };

    private static string DescribeConstraints(Type type)
    {
        var underlying = Nullable.GetUnderlyingType(type) ?? type;
        return underlying.IsEnum ? $"Allowed values: {string.Join(", ", Enum.GetNames(underlying))}." : "None.";
    }

    private static string SplitWords(string value) => string.Concat(value.Select((character, index) =>
        index > 0 && char.IsUpper(character) ? $" {character}" : character.ToString()));

    private static string FormatDefault(object? value) => value switch
    {
        null => "null",
        bool boolean => boolean ? "true" : "false",
        string text => $"\"{text}\"",
        char character => $"'{character}'",
        Enum enumeration => enumeration.ToString(),
        EventCallback => "unset",
        IEnumerable => "[]",
        IFormattable formattable => formattable.ToString(null, CultureInfo.InvariantCulture),
        _ => value.ToString() ?? "null"
    };

    private static string FriendlyName(Type type)
    {
        if (type.IsGenericType)
        {
            var name = type.GetGenericTypeDefinition().Name.Split('`')[0];
            return $"{name}<{string.Join(", ", type.GetGenericArguments().Select(FriendlyName))}>";
        }

        if (type.IsArray)
            return $"{FriendlyName(type.GetElementType()!)}[]";

        return type.Name;
    }
}
