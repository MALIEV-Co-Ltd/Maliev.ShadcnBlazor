using Maliev.ShadcnBlazor.Components.Content;
using Maliev.ShadcnBlazor.Components.Feedback;
using Maliev.ShadcnBlazor.Components.Feedback.Toast;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Rendering;
using Maliev.ShadcnBlazor.Showcase.Components.Documentation;

namespace Maliev.ShadcnBlazor.Showcase.Documentation.Examples;

internal static class FeedbackContentExamples
{
    public static IReadOnlyList<ComponentExampleDefinition> Create(string slug) => slug switch
    {
        "alert" => [Alert()],
        "avatar" => [Avatar()],
        "badge" => [Badge()],
        "card" => [Card()],
        "carousel" => [Carousel()],
        "progress" => [Progress()],
        "skeleton" => [Skeleton()],
        "spinner" => [Spinner()],
        "toast" => [Toast()],
        _ => []
    };

    private static ComponentExampleDefinition Alert()
    {
        var variant = ShadcnAlertVariant.Default; var role = ShadcnAlertRole.Alert; var action = true;
        RenderFragment preview = b =>
        {
            b.OpenComponent<ShadcnAlert>(0);
            b.AddAttribute(1, nameof(ShadcnAlert.Variant), variant);
            b.AddAttribute(2, nameof(ShadcnAlert.AlertRole), role);
            b.AddAttribute(3, "class", "showcase-alert-card");
            b.AddAttribute(4, nameof(ShadcnAlert.ChildContent), (RenderFragment)(content =>
            {
                Add<ShadcnAlertIcon>(content, 0, variant == ShadcnAlertVariant.Destructive ? "!" : "✓");
                Add<ShadcnAlertTitle>(content, 3, variant == ShadcnAlertVariant.Destructive ? "ตรวจสอบข้อมูลก่อนส่ง" : "ชำระเงินสำเร็จ");
                Add<ShadcnAlertDescription>(content, 6, variant == ShadcnAlertVariant.Destructive ? "ยังมีรายการที่ต้องแก้ไขในใบเสนอราคา" : "Payment processed — ใบเสนอราคาพร้อมดำเนินการต่อ");
                if (action)
                {
                    content.OpenComponent<ShadcnAlertAction>(9);
                    content.AddAttribute(10, nameof(ShadcnAlertAction.ChildContent), (RenderFragment)(actionContent =>
                    {
                        actionContent.OpenElement(0, "button");
                        actionContent.AddAttribute(1, "type", "button");
                        actionContent.AddAttribute(2, "class", "showcase-alert-action");
                        actionContent.AddContent(3, variant == ShadcnAlertVariant.Destructive ? "ตรวจสอบ" : "ดูรายละเอียด");
                        actionContent.CloseElement();
                    }));
                    content.CloseComponent();
                }
            }));
            b.CloseComponent();
        };
        return Example("alert", "Alert callout", "Compose an accessible shadcn alert with semantic icon, title, description, and an optional action.", "<ShadcnAlert><ShadcnAlertIcon>...</ShadcnAlertIcon><ShadcnAlertTitle>...</ShadcnAlertTitle><ShadcnAlertDescription>...</ShadcnAlertDescription></ShadcnAlert>", preview, [EnumSelect("alert-variant", "Variant", variant, v => variant = v), EnumSelect("alert-role", "Role", role, v => role = v), Toggle("alert-action", "Action", v => action = v, true)], ["default", "destructive", "icon", "title", "description", "action", "alert", "status"]);
    }
    private static ComponentExampleDefinition Avatar()
    {
        var size = ShadcnAvatarSize.Default; var failed = false; var badge = false; var group = false;
        RenderFragment preview = b => { b.OpenComponent<AvatarDossierPreview>(0); b.AddAttribute(1, "Size", size); b.AddAttribute(2, "Failed", failed); b.AddAttribute(3, "Badge", badge); b.AddAttribute(4, "Group", group); b.CloseComponent(); };
        return Example("avatar", "Avatar fallback", "Review sizes and image failure fallback; badge and group states are available as live controls.", "<ShadcnAvatar><ShadcnAvatarImage Source=\"...\" Alt=\"Operator\" /><ShadcnAvatarFallback>NO</ShadcnAvatarFallback></ShadcnAvatar>", preview, [EnumSelect("avatar-size", "Size", size, v => size = v), Toggle("avatar-failed", "Failed image", v => failed = v), Toggle("avatar-badge", "Badge", v => badge = v), Toggle("avatar-group", "Group", v => group = v)], ["sm", "default", "lg", "fallback", "badge", "group"]);
    }
    private static ComponentExampleDefinition Badge()
    {
        var variant = ShadcnBadgeVariant.Default; var link = false; var invalid = false;
        RenderFragment preview = b => { b.OpenComponent<ShadcnBadge>(0); b.AddAttribute(1, "Variant", variant); b.AddAttribute(2, "Href", link ? "/docs/components/badge" : null); b.AddAttribute(3, "ChildContent", Text("Ready")); if (invalid) b.AddAttribute(4, "AdditionalAttributes", new Dictionary<string, object> { ["aria-invalid"] = "true" }); b.CloseComponent(); };
        return Example("badge", "Status badge", "Exercise all semantic variants, validation, and optional link rendering.", "<ShadcnBadge Variant=\"ShadcnBadgeVariant.Secondary\">Ready</ShadcnBadge>", preview, [EnumSelect("badge-variant", "Variant", variant, v => variant = v), Toggle("badge-link", "Link", v => link = v), Toggle("badge-invalid", "Invalid", v => invalid = v)], ["variants", "inline", "link", "focus"]);
    }
    private static ComponentExampleDefinition Card()
    {
        var size = ShadcnCardSize.Default; var compactSpacing = false; var action = true;
        RenderFragment preview = b => { b.OpenComponent<CardDossierPreview>(0); b.AddAttribute(1, "Size", size); b.AddAttribute(2, "CompactSpacing", compactSpacing); b.AddAttribute(3, "Action", action); b.CloseComponent(); };
        return Example("card", "Composed card", "Tune density, action composition, and card spacing.", "<ShadcnCard><ShadcnCardHeader>...</ShadcnCardHeader><ShadcnCardContent>...</ShadcnCardContent></ShadcnCard>", preview, [EnumSelect("card-size", "Size", size, v => size = v), Toggle("card-spacing", "Compact spacing", v => compactSpacing = v), Toggle("card-action", "Action", v => action = v, true)], ["default", "sm", "header", "action", "content", "footer"]);
    }
    private static ComponentExampleDefinition Carousel()
    {
        var vertical = false; var loop = false; var rtl = false; var reduced = true;
        RenderFragment preview = b => { b.OpenComponent<ShadcnCarousel>(0); b.AddAttribute(1, "Orientation", vertical ? ShadcnCarouselOrientation.Vertical : ShadcnCarouselOrientation.Horizontal); b.AddAttribute(2, "Options", new ShadcnCarouselOptions { Loop = loop, RightToLeft = rtl, ReducedMotion = reduced }); b.AddAttribute(3, "Label", "Production queue"); b.AddAttribute(4, "ChildContent", (RenderFragment)(c => { c.OpenComponent<ShadcnCarouselContent>(0); c.AddAttribute(1, "ChildContent", (RenderFragment)(items => { for (var i = 0; i < 3; i++) { items.OpenComponent<ShadcnCarouselItem>(i * 3); items.AddAttribute(i * 3 + 1, "Index", i); items.AddAttribute(i * 3 + 2, "ChildContent", Text($"Job {i + 1}")); items.CloseComponent(); } })); c.CloseComponent(); c.OpenComponent<ShadcnCarouselPrevious>(2); c.CloseComponent(); c.OpenComponent<ShadcnCarouselNext>(3); c.CloseComponent(); })); b.CloseComponent(); };
        return Example("carousel", "Carousel engine", "Review the Blazor-native typed engine across axis and loop modes.", "<ShadcnCarousel Options=\"Options\"><ShadcnCarouselContent>...</ShadcnCarouselContent></ShadcnCarousel>", preview, [Toggle("carousel-vertical", "Vertical", v => vertical = v), Toggle("carousel-loop", "Loop", v => loop = v), Toggle("carousel-rtl", "RTL", v => rtl = v), Toggle("carousel-reduced", "Reduced motion", v => reduced = v, true)], ["horizontal", "vertical", "loop", "keyboard", "pointer", "rtl", "reduced-motion"]);
    }
    private static ComponentExampleDefinition Progress()
    {
        var indeterminate = false; var value = 64d; var showValue = true;
        RenderFragment preview = b => { b.OpenComponent<ShadcnProgress>(0); b.AddAttribute(1, "Value", indeterminate ? null : value); b.AddAttribute(2, "Label", "Upload"); b.AddAttribute(3, "ShowValue", showValue); b.CloseComponent(); };
        return Example("progress", "Progress", "Compare determinate and indeterminate accessible progress.", "<ShadcnProgress Value=\"64\" Label=\"Upload\" ShowValue=\"true\" />", preview, [Toggle("progress-indeterminate", "Indeterminate", v => indeterminate = v), Number("progress-value", "Value", value, v => value = v), Toggle("progress-show-value", "Show value", v => showValue = v, true)], ["determinate", "indeterminate", "label", "value"]);
    }
    private static ComponentExampleDefinition Skeleton()
    {
        var circle = false; var motion = true;
        RenderFragment preview = b => { b.OpenComponent<ShadcnSkeleton>(0); b.AddAttribute(1, "Shape", circle ? ShadcnSkeletonShape.Circle : ShadcnSkeletonShape.Default); b.AddAttribute(2, "Animation", motion ? ShadcnSkeletonAnimation.Pulse : ShadcnSkeletonAnimation.None); b.AddAttribute(3, "Style", "width: 8rem; height: 2rem"); b.CloseComponent(); };
        return Example("skeleton", "Skeleton", "Adjust shape and animation while reduced motion remains respected.", "<ShadcnSkeleton Style=\"width:8rem;height:2rem\" />", preview, [Toggle("skeleton-circle", "Circle", v => circle = v), Toggle("skeleton-motion", "Pulse", v => motion = v, true)], ["rectangle", "circle", "pulse", "reduced-motion"]);
    }
    private static ComponentExampleDefinition Spinner()
    {
        var decorative = false; var large = false;
        RenderFragment preview = b => { b.OpenComponent<ShadcnSpinner>(0); b.AddAttribute(1, "Label", decorative ? null : "กำลังโหลด"); b.AddAttribute(2, "Size", large ? "1.5rem" : "1rem"); b.CloseComponent(); };
        return Example("spinner", "Spinner", "Choose announced/decorative semantics and token-safe size.", "<ShadcnSpinner Label=\"กำลังโหลด\" />", preview, [Toggle("spinner-decorative", "Decorative", v => decorative = v), Toggle("spinner-large", "Large", v => large = v)], ["status", "decorative", "size", "reduced-motion"]);
    }
    private static ComponentExampleDefinition Toast()
    {
        var limit = 3d; var start = false; var reduced = false; var type = ShadcnToastType.Success; var priority = ShadcnToastPriority.Normal;
        RenderFragment preview = b => { b.OpenComponent<ToastDossierPreview>(0); b.AddAttribute(1, "MaximumVisible", (int)limit); b.AddAttribute(2, "Placement", start ? ShadcnToastPlacement.BottomStart : ShadcnToastPlacement.BottomEnd); b.AddAttribute(3, "ReducedMotion", reduced); b.AddAttribute(4, "Type", type); b.AddAttribute(5, "Priority", priority); b.CloseComponent(); };
        return Example("toast", "Toast viewport", "Trigger and configure the deterministic queue viewport.", "@inject IShadcnToastService Toasts\n<ShadcnButton OnClick=\"Show\">Show</ShadcnButton>\n<ShadcnToaster />", preview, [Number("toast-limit", "Visible limit", limit, v => limit = Math.Max(1, v)), Toggle("toast-start", "Logical start", v => start = v), Toggle("toast-reduced", "Reduced motion", v => reduced = v), EnumSelect("toast-type", "Type", type, v => type = v), EnumSelect("toast-priority", "Priority", priority, v => priority = v)], ["queue", "limit", "promise", "pause", "swipe", "F6", "rtl", "reduced-motion", "priority"]);
    }

    private static ComponentExampleDefinition Example(string slug, string title, string description, string source, RenderFragment preview, IReadOnlyList<ComponentParameterControl> controls, IReadOnlyList<string> tags) => new($"{slug}-primary", title, description, source, preview, controls, tags);
    private static ComponentParameterControl Toggle(string id, string label, Action<bool> apply, bool initial = false) => new(id, label, ComponentParameterControlKind.Toggle, initial.ToString(), [], value => apply(bool.Parse(value)));
    private static ComponentParameterControl Number(string id, string label, double value, Action<double> apply) => new(id, label, ComponentParameterControlKind.Number, value.ToString(System.Globalization.CultureInfo.InvariantCulture), [], text => apply(double.Parse(text, System.Globalization.CultureInfo.InvariantCulture)));
    private static ComponentParameterControl EnumSelect<T>(string id, string label, T value, Action<T> apply) where T : struct, Enum => new(id, label, ComponentParameterControlKind.Select, value.ToString(), Enum.GetNames<T>(), text => apply(Enum.Parse<T>(text)));
    private static RenderFragment Text(string value) => b => b.AddContent(0, value);
    private static void Add<T>(RenderTreeBuilder b, int sequence, string text) where T : IComponent { b.OpenComponent<T>(sequence); b.AddAttribute(sequence + 1, "ChildContent", Text(text)); b.CloseComponent(); }
}
