using System.Globalization;
using Bunit;
using Maliev.ShadcnBlazor.Components.Forms;
using Maliev.ShadcnBlazor.Theming;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Web;
using Microsoft.AspNetCore.Components.Forms;

namespace Maliev.ShadcnBlazor.Tests.Components.Forms;

public sealed class CalendarDatePickerTests : BunitContext
{
    public CalendarDatePickerTests()
    {
        var module = JSInterop.SetupModule("./_content/Maliev.ShadcnBlazor/js/shadcn-forms.js");
        module.SetupVoid("focusCalendarDay", _ => true);
        module.SetupVoid("observePopupDismissal", _ => true);
        module.SetupVoid("disconnectPopupDismissal", _ => true);
        module.SetupVoid("focusElement", _ => true);
        module.SetupVoid("focusCalendarInPopup", _ => true);
    }

    [Fact]
    public void CalendarRendersDeterministicSixWeekGridAndModifiers()
    {
        var cut = Render<ShadcnCalendar>(parameters => parameters
            .Add(component => component.VisibleMonth, new DateOnly(2026, 7, 1))
            .Add(component => component.Today, new DateOnly(2026, 7, 10))
            .Add(component => component.Value, new DateOnly(2026, 7, 15))
            .Add(component => component.Culture, CultureInfo.GetCultureInfo("en-US"))
            .Add(component => component.IsDateDisabled, date => date == new DateOnly(2026, 7, 20)));

        var days = cut.FindAll("button[data-slot='calendar-day']");
        Assert.Equal(42, days.Count);
        Assert.Equal("2026-06-28", days[0].GetAttribute("data-day"));
        Assert.Equal("true", cut.Find("[data-day='2026-07-10']").GetAttribute("data-today"));
        Assert.Equal("true", cut.Find("[data-day='2026-07-15']").GetAttribute("data-selected-single"));
        Assert.Equal("true", cut.Find("[data-day='2026-07-20']").GetAttribute("aria-disabled"));
        Assert.True(cut.Find("[data-day='2026-07-20']").HasAttribute("disabled"));
        Assert.Contains("July", cut.Find("[data-day='2026-07-20']").GetAttribute("aria-label"), StringComparison.Ordinal);
        Assert.Equal("true", days[0].GetAttribute("data-outside"));
    }

    [Fact]
    public void CalendarUsesShortestLocalizedWeekdayLabels()
    {
        var cut = Render<ShadcnCalendar>(parameters => parameters
            .Add(component => component.VisibleMonth, new DateOnly(2026, 8, 1))
            .Add(component => component.Culture, CultureInfo.InvariantCulture));

        Assert.Equal(
            ["Su", "Mo", "Tu", "We", "Th", "Fr", "Sa"],
            cut.FindAll("[data-slot='calendar-weekday']").Select(element => element.TextContent));
    }

    [Fact]
    public void CalendarAndDatePickerInheritProviderDirectionWhenNotExplicitlySet()
    {
        var cut = Render<CascadingValue<ShadcnContext>>(parameters => parameters
            .Add(component => component.Value, new ShadcnContext(false, ShadcnDirection.RightToLeft))
            .Add(component => component.ChildContent, builder =>
            {
                builder.OpenComponent<ShadcnCalendar>(0);
                builder.AddAttribute(1, nameof(ShadcnCalendar.VisibleMonth), new DateOnly(2026, 8, 1));
                builder.CloseComponent();
                builder.OpenComponent<ShadcnDatePicker>(10);
                builder.AddAttribute(11, nameof(ShadcnDatePicker.Open), true);
                builder.AddAttribute(12, nameof(ShadcnDatePicker.VisibleMonth), new DateOnly(2026, 8, 1));
                builder.CloseComponent();
            }));

        Assert.All(cut.FindAll("[data-slot='calendar']"), calendar => Assert.Equal("rtl", calendar.GetAttribute("dir")));
    }

    [Fact]
    public void CalendarHandlesLeapYearAndThaiLocale()
    {
        var cut = Render<ShadcnCalendar>(parameters => parameters
            .Add(component => component.VisibleMonth, new DateOnly(2024, 2, 1))
            .Add(component => component.Culture, CultureInfo.GetCultureInfo("th-TH")));

        Assert.Contains("กุมภาพันธ์", cut.Find("[data-slot='calendar-caption']").TextContent, StringComparison.Ordinal);
        Assert.NotEmpty(cut.FindAll("[data-day='2024-02-29']"));
        Assert.Contains(cut.FindAll("[data-slot='calendar-weekday']"), element => element.TextContent.Contains("อา", StringComparison.Ordinal));
    }

    [Fact]
    public void CalendarDropdownCaptionRequestsMonthAndYear()
    {
        DateOnly? visible = null;
        var cut = Render<ShadcnCalendar>(parameters => parameters
            .Add(component => component.VisibleMonth, new DateOnly(2026, 7, 1))
            .Add(component => component.CaptionLayout, ShadcnCalendarCaptionLayout.Dropdown)
            .Add(component => component.FromYear, 2024)
            .Add(component => component.ToYear, 2028)
            .Add(component => component.VisibleMonthChanged, value => visible = value));

        cut.Find("select[data-slot='calendar-month-select']").Change("8");
        Assert.Equal(new DateOnly(2026, 8, 1), visible);
        Assert.Equal("0", cut.FindAll("[data-slot='calendar-day']").Single(day => day.GetAttribute("tabindex") == "0").GetAttribute("tabindex"));
        cut.Find("select[data-slot='calendar-year-select']").Change("2027");
        Assert.Equal(new DateOnly(2027, 7, 1), visible);
    }

    [Fact]
    public void RangeCalendarRequestsStartThenCompletedRange()
    {
        ShadcnDateRange? requested = null;
        var cut = Render<ShadcnCalendar>(parameters => parameters
            .Add(component => component.Mode, ShadcnCalendarSelectionMode.Range)
            .Add(component => component.VisibleMonth, new DateOnly(2026, 7, 1))
            .Add(component => component.RangeChanged, range => requested = range));

        cut.Find("[data-day='2026-07-10']").Click();
        Assert.Equal(new ShadcnDateRange(new DateOnly(2026, 7, 10), null), requested);

        cut.Render(parameters => parameters
            .Add(component => component.Mode, ShadcnCalendarSelectionMode.Range)
            .Add(component => component.VisibleMonth, new DateOnly(2026, 7, 1))
            .Add(component => component.Range, requested)
            .Add(component => component.RangeChanged, range => requested = range));
        cut.Find("[data-day='2026-07-15']").Click();
        Assert.Equal(new ShadcnDateRange(new DateOnly(2026, 7, 10), new DateOnly(2026, 7, 15)), requested);
    }

    [Fact]
    public void CalendarKeyboardUsesRtlLogicalArrowsAndPageNavigation()
    {
        DateOnly? selected = null;
        DateOnly? visible = null;
        var cut = Render<ShadcnCalendar>(parameters => parameters
            .Add(component => component.Direction, ShadcnDirection.RightToLeft)
            .Add(component => component.VisibleMonth, new DateOnly(2026, 7, 1))
            .Add(component => component.Value, new DateOnly(2026, 7, 15))
            .Add(component => component.ValueChanged, value => selected = value)
            .Add(component => component.VisibleMonthChanged, value => visible = value));

        var day = cut.Find("[data-day='2026-07-15']");
        day.KeyDown(new KeyboardEventArgs { Key = "ArrowRight" });
        day.KeyDown(new KeyboardEventArgs { Key = "Enter" });
        Assert.Equal(new DateOnly(2026, 7, 14), selected);

        day.KeyDown(new KeyboardEventArgs { Key = "PageDown" });
        Assert.Equal(new DateOnly(2026, 8, 1), visible);
        day.KeyDown(new KeyboardEventArgs { Key = "PageDown", ShiftKey = true });
        Assert.Equal(new DateOnly(2027, 7, 1), visible);
    }

    [Fact]
    public void CalendarRespectsBoundsAndDisabledCallbacks()
    {
        var calls = 0;
        var cut = Render<ShadcnCalendar>(parameters => parameters
            .Add(component => component.VisibleMonth, new DateOnly(2026, 7, 1))
            .Add(component => component.Min, new DateOnly(2026, 7, 10))
            .Add(component => component.Max, new DateOnly(2026, 7, 20))
            .Add(component => component.ValueChanged, _ => calls++));
        cut.Find("[data-day='2026-07-09']").Click();
        Assert.Equal(0, calls);
        Assert.True(cut.Find("[data-slot='calendar-previous']").HasAttribute("disabled"));
        Assert.True(cut.Find("[data-slot='calendar-next']").HasAttribute("disabled"));
    }

    [Fact]
    public void DisabledCalendarAlsoDisablesCaptionDropdowns()
    {
        var cut = Render<ShadcnCalendar>(parameters => parameters
            .Add(component => component.VisibleMonth, new DateOnly(2026, 7, 1))
            .Add(component => component.CaptionLayout, ShadcnCalendarCaptionLayout.Dropdown)
            .Add(component => component.Disabled, true));

        Assert.All(cut.FindAll("[data-slot$='-select']"), select => Assert.True(select.HasAttribute("disabled")));
    }

    [Fact]
    public void CalendarKeyboardSkipsDisabledDatesAndRequestsCrossMonthVisibility()
    {
        DateOnly? visible = null;
        var cut = Render<ShadcnCalendar>(parameters => parameters
            .Add(component => component.VisibleMonth, new DateOnly(2026, 7, 1))
            .Add(component => component.Value, new DateOnly(2026, 7, 30))
            .Add(component => component.IsDateDisabled, date => date == new DateOnly(2026, 7, 31))
            .Add(component => component.VisibleMonthChanged, value => visible = value));

        cut.Find("[data-day='2026-07-30']").KeyDown(new KeyboardEventArgs { Key = "ArrowRight" });
        Assert.Equal(new DateOnly(2026, 8, 1), visible);
    }

    [Fact]
    public void CalendarBoundsCrossGridNavigationAndAlwaysKeepsOneEnabledRovingTarget()
    {
        DateOnly? visible = null;
        var cut = Render<ShadcnCalendar>(parameters => parameters
            .Add(component => component.VisibleMonth, new DateOnly(2026, 7, 1))
            .Add(component => component.Value, new DateOnly(2026, 7, 20))
            .Add(component => component.Min, new DateOnly(2026, 7, 10))
            .Add(component => component.Max, new DateOnly(2026, 7, 20))
            .Add(component => component.VisibleMonthChanged, value => visible = value));

        cut.Find("[data-day='2026-07-20']").KeyDown(new KeyboardEventArgs { Key = "PageDown" });
        Assert.Null(visible);
        var roving = cut.FindAll("[data-slot='calendar-day'][tabindex='0']");
        Assert.Single(roving);
        Assert.False(roving[0].HasAttribute("disabled"));
        Assert.Equal("2026-07-20", roving[0].GetAttribute("data-day"));
    }

    [Fact]
    public void CalendarDisabledDateSearchIsBoundedAndKeepsTheExistingEnabledTarget()
    {
        var predicateCalls = 0;
        var current = new DateOnly(2026, 7, 15);
        var cut = Render<ShadcnCalendar>(parameters => parameters
            .Add(component => component.VisibleMonth, new DateOnly(2026, 7, 1))
            .Add(component => component.Value, current)
            .Add(component => component.Min, new DateOnly(2026, 1, 1))
            .Add(component => component.Max, new DateOnly(2030, 1, 1))
            .Add(component => component.IsDateDisabled, date => { predicateCalls++; return date != current; }));

        predicateCalls = 0;
        cut.Find("[data-day='2026-07-15']").KeyDown(new KeyboardEventArgs { Key = "PageDown" });

        Assert.InRange(predicateCalls, 1, 800);
        Assert.Equal("2026-07-15", cut.Find("[data-slot='calendar-day'][tabindex='0']").GetAttribute("data-day"));
    }

    [Fact]
    public void CalendarReconcilesRovingFocusWhenDisabledPredicateChanges()
    {
        var disabledDate = new DateOnly(2026, 7, 31);
        var cut = Render<ShadcnCalendar>(parameters => parameters
            .Add(component => component.VisibleMonth, new DateOnly(2026, 7, 1))
            .Add(component => component.Value, disabledDate)
            .Add(component => component.IsDateDisabled, _ => false));
        Assert.Equal(disabledDate.ToString("yyyy-MM-dd"), cut.Find("[tabindex='0']").GetAttribute("data-day"));

        cut.Render(parameters => parameters
            .Add(component => component.VisibleMonth, new DateOnly(2026, 7, 1))
            .Add(component => component.Value, disabledDate)
            .Add(component => component.IsDateDisabled, date => date == disabledDate));

        var roving = cut.FindAll("[data-slot='calendar-day'][tabindex='0']");
        Assert.Single(roving);
        Assert.False(roving[0].HasAttribute("disabled"));
        Assert.Equal("2026-07-30", roving[0].GetAttribute("data-day"));
    }

    [Fact]
    public void CalendarReconcilesRovingFocusWhenBoundsTighten()
    {
        var focused = new DateOnly(2026, 7, 10);
        var cut = Render<ShadcnCalendar>(parameters => parameters
            .Add(component => component.VisibleMonth, new DateOnly(2026, 7, 1))
            .Add(component => component.Value, focused));
        Assert.Equal(focused.ToString("yyyy-MM-dd"), cut.Find("[tabindex='0']").GetAttribute("data-day"));

        cut.Render(parameters => parameters
            .Add(component => component.VisibleMonth, new DateOnly(2026, 7, 1))
            .Add(component => component.Value, focused)
            .Add(component => component.Min, new DateOnly(2026, 7, 20))
            .Add(component => component.Max, new DateOnly(2026, 7, 25)));

        var roving = cut.FindAll("[data-slot='calendar-day'][tabindex='0']");
        Assert.Single(roving);
        Assert.False(roving[0].HasAttribute("disabled"));
        Assert.Equal("2026-07-20", roving[0].GetAttribute("data-day"));
    }

    [Fact]
    public void CalendarAndPickerRejectExternalReversedRanges()
    {
        var reversed = new ShadcnDateRange(new DateOnly(2026, 8, 20), new DateOnly(2026, 8, 10));
        Assert.ThrowsAny<Exception>(() => Render<ShadcnCalendar>(parameters => parameters.Add(component => component.Range, reversed)));
        Assert.ThrowsAny<Exception>(() => Render<ShadcnDatePicker>(parameters => parameters.Add(component => component.Range, reversed)));
    }

    [Fact]
    public void DatePickerUsesDateOnlyIsoPayloadAndFormattedTrigger()
    {
        var date = new DateOnly(2026, 8, 13);
        var cut = Render<ShadcnDatePicker>(parameters => parameters
            .Add(component => component.Value, date)
            .Add(component => component.Name, "deliveryDate")
            .Add(component => component.Culture, CultureInfo.GetCultureInfo("th-TH"))
            .Add(component => component.Format, "d MMMM yyyy"));

        Assert.Contains("สิงหาคม", cut.Find("[data-slot='date-picker-trigger']").TextContent, StringComparison.Ordinal);
        var formControl = cut.Find("input[data-slot='date-picker-form-control']");
        Assert.Equal("deliveryDate", formControl.GetAttribute("name"));
        Assert.Equal("2026-08-13", formControl.GetAttribute("value"));
    }

    [Fact]
    public void DatePickerOpensSelectsAndClearsWithoutTimezoneConversion()
    {
        DateOnly? selected = null;
        var open = true;
        var cut = Render<ShadcnDatePicker>(parameters => parameters
            .Add(component => component.Value, selected)
            .Add(component => component.ValueChanged, value => selected = value)
            .Add(component => component.Open, open)
            .Add(component => component.OpenChanged, value => open = value)
            .Add(component => component.VisibleMonth, new DateOnly(2026, 8, 1))
            .Add(component => component.Clearable, true));

        cut.Find("[data-day='2026-08-13']").Click();
        Assert.Equal(new DateOnly(2026, 8, 13), selected);
        Assert.False(open);

        cut.Render(parameters => parameters
            .Add(component => component.Value, selected)
            .Add(component => component.ValueChanged, value => selected = value)
            .Add(component => component.Clearable, true));
        cut.Find("[data-slot='date-picker-clear']").Click();
        Assert.Null(selected);
    }

    [Fact]
    public void DatePickerRangeAndExactTextInputStayDateOnly()
    {
        ShadcnDateRange? range = null;
        var cut = Render<ShadcnDatePicker>(parameters => parameters
            .Add(component => component.Mode, ShadcnCalendarSelectionMode.Range)
            .Add(component => component.Range, new ShadcnDateRange(new DateOnly(2026, 8, 10), new DateOnly(2026, 8, 13)))
            .Add(component => component.RangeChanged, value => range = value)
            .Add(component => component.AllowTextInput, true)
            .Add(component => component.Format, "dd/MM/yyyy"));

        Assert.Contains("10/08/2026", cut.Find("[data-slot='date-picker-trigger']").TextContent, StringComparison.Ordinal);
        Assert.Contains("13/08/2026", cut.Find("[data-slot='date-picker-trigger']").TextContent, StringComparison.Ordinal);
        cut.Find("input[data-slot='date-picker-input']").Change("14/08/2026 – 18/08/2026");
        Assert.Equal(new ShadcnDateRange(new DateOnly(2026, 8, 14), new DateOnly(2026, 8, 18)), range);
        cut.Find("input[data-slot='date-picker-input']").Change("08/14/2026");
        Assert.Equal("true", cut.Find("input[data-slot='date-picker-input']").GetAttribute("aria-invalid"));
    }

    [Fact]
    public void DatePickerInvalidTextSuppressesStalePayloadAndRangeTextParsesBothDates()
    {
        ShadcnDateRange? range = null;
        var cut = Render<ShadcnDatePicker>(parameters => parameters
            .Add(component => component.Mode, ShadcnCalendarSelectionMode.Range)
            .Add(component => component.Range, new ShadcnDateRange(new DateOnly(2026, 8, 10), new DateOnly(2026, 8, 13)))
            .Add(component => component.RangeChanged, value => range = value)
            .Add(component => component.AllowTextInput, true)
            .Add(component => component.Format, "dd/MM/yyyy")
            .Add(component => component.Name, "window")
            .Add(component => component.Required, true));

        cut.Find("[data-slot='date-picker-input']").Change("14/08/2026 – 18/08/2026");
        Assert.Equal(new ShadcnDateRange(new DateOnly(2026, 8, 14), new DateOnly(2026, 8, 18)), range);
        cut.Find("[data-slot='date-picker-input']").Change("invalid");
        Assert.Empty(cut.FindAll("input[name='window.start']"));
        Assert.Equal("true", cut.Find("[data-slot='date-picker-input']").GetAttribute("aria-invalid"));
        Assert.Equal("true", cut.Find("[data-slot='date-picker-trigger']").GetAttribute("aria-required"));
        Assert.False(string.IsNullOrWhiteSpace(cut.Find("[data-slot='date-picker-trigger']").GetAttribute("aria-controls")));
    }

    [Fact]
    public void DatePickerRejectsReversedRangeAndParseMessageSurvivesValidateAndRerender()
    {
        var model = new DateModel { Window = new(new DateOnly(2026, 8, 10), new DateOnly(2026, 8, 13)) };
        var editContext = new EditContext(model);
        var cut = Render<CascadingValue<EditContext>>(parameters => parameters
            .Add(cascade => cascade.Value, editContext)
            .AddChildContent<ShadcnDatePicker>(picker => picker
                .Add(component => component.Mode, ShadcnCalendarSelectionMode.Range)
                .Add(component => component.Range, model.Window)
                .Add(component => component.RangeExpression, () => model.Window)
                .Add(component => component.RangeChanged, value => model.Window = value)
                .Add(component => component.AllowTextInput, true)
                .Add(component => component.Format, "dd/MM/yyyy")
                .Add(component => component.Name, "window")
                .Add(component => component.Required, true)));

        cut.Find("[data-slot='date-picker-input']").Change("18/08/2026 – 14/08/2026");
        editContext.Validate();
        cut.Render(parameters => parameters
            .Add(cascade => cascade.Value, editContext)
            .AddChildContent<ShadcnDatePicker>(picker => picker
                .Add(component => component.Mode, ShadcnCalendarSelectionMode.Range)
                .Add(component => component.Range, model.Window)
                .Add(component => component.RangeExpression, () => model.Window)
                .Add(component => component.RangeChanged, value => model.Window = value)
                .Add(component => component.AllowTextInput, true)
                .Add(component => component.Format, "dd/MM/yyyy")
                .Add(component => component.Name, "window")
                .Add(component => component.Required, true)));

        Assert.Equal(new ShadcnDateRange(new DateOnly(2026, 8, 10), new DateOnly(2026, 8, 13)), model.Window);
        Assert.Contains(editContext.GetValidationMessages(), message => message.Contains(nameof(DateModel.Window), StringComparison.Ordinal));
        Assert.False(cut.Find("[data-slot='date-picker-range-start-control']").HasAttribute("name"));
        Assert.Equal("18/08/2026 – 14/08/2026", cut.Find("[data-slot='date-picker-input']").GetAttribute("value"));
    }

    [Fact]
    public void ReadOnlyDatePickerStaysFocusableAndSuppressesMutation()
    {
        var calls = 0;
        var cut = Render<ShadcnDatePicker>(parameters => parameters
            .Add(component => component.Value, new DateOnly(2026, 8, 13))
            .Add(component => component.ReadOnly, true)
            .Add(component => component.ValueChanged, _ => calls++));

        var trigger = cut.Find("button");
        trigger.Click();
        Assert.False(trigger.HasAttribute("disabled"));
        Assert.Equal("true", trigger.GetAttribute("aria-readonly"));
        Assert.Equal(0, calls);
    }

    [Fact]
    public void ExternalValidationStateChangeAutomaticallyRerendersDatePickerInvalidState()
    {
        var model = new DateModel();
        var editContext = new EditContext(model);
        var messages = new ValidationMessageStore(editContext);
        var cut = Render<CascadingValue<EditContext>>(parameters => parameters
            .Add(cascade => cascade.Value, editContext)
            .AddChildContent<ShadcnDatePicker>(picker => picker
                .Add(component => component.Mode, ShadcnCalendarSelectionMode.Range)
                .Add(component => component.Range, model.Window)
                .Add(component => component.RangeExpression, () => model.Window)));

        messages.Add(new FieldIdentifier(model, nameof(DateModel.Window)), "External error");
        editContext.NotifyValidationStateChanged();

        cut.WaitForAssertion(() => Assert.Equal("true", cut.Find("[data-slot='date-picker-trigger']").GetAttribute("aria-invalid")));
    }

    [Fact]
    public async Task DatePickerDisposalClearsOwnedParseMessagesAndNotifiesTheForm()
    {
        var model = new DateModel { Date = new DateOnly(2026, 8, 13) };
        var editContext = new EditContext(model);
        var notifications = 0;
        editContext.OnValidationStateChanged += (_, _) => notifications++;
        var cut = Render<CascadingValue<EditContext>>(parameters => parameters
            .Add(cascade => cascade.Value, editContext)
            .AddChildContent<ShadcnDatePicker>(picker => picker
                .Add(component => component.Value, model.Date)
                .Add(component => component.ValueExpression, () => model.Date)
                .Add(component => component.AllowTextInput, true)
                .Add(component => component.Format, "dd/MM/yyyy")));

        cut.Find("[data-slot='date-picker-input']").Change("invalid");
        Assert.NotEmpty(editContext.GetValidationMessages());
        var notificationsBeforeDispose = notifications;

        await cut.FindComponent<ShadcnDatePicker>().Instance.DisposeAsync();

        Assert.Empty(editContext.GetValidationMessages());
        Assert.True(notifications > notificationsBeforeDispose);
    }

    [Fact]
    public void DatePickerEditContextReplacementClearsOldParseStateAndStartsClean()
    {
        var oldModel = new DateModel { Date = new DateOnly(2026, 8, 13) };
        var oldContext = new EditContext(oldModel);
        var oldNotifications = 0;
        oldContext.OnValidationStateChanged += (_, _) => oldNotifications++;
        var cut = Render<CascadingValue<EditContext>>(parameters => parameters
            .Add(cascade => cascade.Value, oldContext)
            .AddChildContent<ShadcnDatePicker>(picker => picker
                .Add(component => component.Value, oldModel.Date)
                .Add(component => component.ValueExpression, () => oldModel.Date)
                .Add(component => component.AllowTextInput, true)
                .Add(component => component.Format, "dd/MM/yyyy")));
        cut.Find("[data-slot='date-picker-input']").Change("invalid");
        Assert.NotEmpty(oldContext.GetValidationMessages());
        var notificationsBeforeReplacement = oldNotifications;

        var newModel = new DateModel { Date = new DateOnly(2026, 8, 20) };
        var newContext = new EditContext(newModel);
        cut.Render(parameters => parameters
            .Add(cascade => cascade.Value, newContext)
            .AddChildContent<ShadcnDatePicker>(picker => picker
                .Add(component => component.Value, newModel.Date)
                .Add(component => component.ValueExpression, () => newModel.Date)
                .Add(component => component.AllowTextInput, true)
                .Add(component => component.Format, "dd/MM/yyyy")));

        Assert.Empty(oldContext.GetValidationMessages());
        Assert.True(oldNotifications > notificationsBeforeReplacement);
        Assert.Empty(newContext.GetValidationMessages());
        Assert.Null(cut.Find("[data-slot='date-picker-input']").GetAttribute("aria-invalid"));
        Assert.Equal("20/08/2026", cut.Find("[data-slot='date-picker-input']").GetAttribute("value"));
    }

    private sealed class DateModel
    {
        public DateOnly? Date { get; set; }
        public ShadcnDateRange? Window { get; set; }
    }
}
