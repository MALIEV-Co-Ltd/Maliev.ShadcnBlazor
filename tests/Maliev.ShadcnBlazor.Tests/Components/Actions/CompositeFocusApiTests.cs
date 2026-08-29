using Maliev.ShadcnBlazor.Components.Actions;
using Maliev.ShadcnBlazor.Components.Disclosure;
using Maliev.ShadcnBlazor.Components.Forms;
using Maliev.ShadcnBlazor.Components.Navigation;
using Maliev.ShadcnBlazor.Components.Overlays;
using Maliev.ShadcnBlazor.Components.Primitives;
using Maliev.ShadcnBlazor.Components.Selection;

namespace Maliev.ShadcnBlazor.Tests.Components.Actions;

public sealed class CompositeFocusApiTests
{
    public static TheoryData<Type> CompositeTypes => new()
    {
        typeof(ShadcnRadioGroup<string>),
        typeof(ShadcnSlider),
        typeof(ShadcnSelect<string>),
        typeof(ShadcnCombobox<string>),
        typeof(ShadcnDatePicker),
        typeof(ShadcnCalendar),
        typeof(ShadcnCommand),
        typeof(ShadcnTabs),
        typeof(ShadcnAccordion),
        typeof(ShadcnToggleGroup<string>),
        typeof(ShadcnNavigationMenu),
        typeof(ShadcnMenubar)
    };

    [Theory]
    [MemberData(nameof(CompositeTypes))]
    public void AuditedCompositesExposeTheSharedFocusContract(Type componentType)
    {
        Assert.True(typeof(IShadcnFocusable).IsAssignableFrom(componentType), componentType.FullName);
    }
}
