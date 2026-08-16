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
        const string source = """
<ShadcnAlert Variant="ShadcnAlertVariant.Default" AlertRole="ShadcnAlertRole.Status">
    <ShadcnAlertIcon>✓</ShadcnAlertIcon>
    <ShadcnAlertTitle>ชำระเงินสำเร็จ</ShadcnAlertTitle>
    <ShadcnAlertDescription>
        Payment processed — ใบเสนอราคาพร้อมดำเนินการต่อ
    </ShadcnAlertDescription>
    <ShadcnAlertAction>
        <button type="button">ดูรายละเอียด</button>
    </ShadcnAlertAction>
</ShadcnAlert>
""";
        return Example("alert", "Alert callout", "Compose an accessible shadcn alert with semantic icon, title, description, and an optional action.", source, preview, [EnumSelect("alert-variant", "Variant", variant, v => variant = v), EnumSelect("alert-role", "Role", role, v => role = v), Toggle("alert-action", "Action", v => action = v, true)], ["default", "destructive", "icon", "title", "description", "action", "alert", "status"]);
    }
    private static ComponentExampleDefinition Avatar()
    {
        var size = ShadcnAvatarSize.Default; var failed = false; var badge = false; var group = false;
        RenderFragment preview = b => { b.OpenComponent<AvatarDossierPreview>(0); b.AddAttribute(1, "Size", size); b.AddAttribute(2, "Failed", failed); b.AddAttribute(3, "Badge", badge); b.AddAttribute(4, "Group", group); b.CloseComponent(); };
        const string source = """
<div class="team-avatars" aria-label="Project team avatars">
    <div class="team-avatar-profile">
        <ShadcnAvatar>
            <ShadcnAvatarImage Source="images/avatars/operator-thai.png" Alt="Thai CNC operator" />
            <ShadcnAvatarFallback>
                <svg viewBox="0 0 24 24" aria-hidden="true"><circle cx="12" cy="8" r="3.25" fill="currentColor" /><path d="M5.5 20c.7-4 2.9-6 6.5-6s5.8 2 6.5 6" fill="currentColor" /></svg>
            </ShadcnAvatarFallback>
            <ShadcnAvatarBadge />
        </ShadcnAvatar>
        <span><strong>Natee</strong><span>Thai CNC operator</span></span>
    </div>
    <div class="team-avatar-profile">
        <ShadcnAvatar>
            <ShadcnAvatarImage Source="images/avatars/reviewer-thai.png" Alt="Thai quality reviewer" />
            <ShadcnAvatarFallback>QR</ShadcnAvatarFallback>
        </ShadcnAvatar>
        <span><strong>Pim</strong><span>Thai quality reviewer</span></span>
    </div>
    <div class="team-avatar-profile">
        <ShadcnAvatar>
            <ShadcnAvatarImage Source="images/avatars/coordinator-thai.png" Alt="Thai project coordinator" />
            <ShadcnAvatarFallback>PC</ShadcnAvatarFallback>
        </ShadcnAvatar>
        <span><strong>Malee</strong><span>Thai project coordinator</span></span>
    </div>
    <div class="team-avatar-profile">
        <ShadcnAvatar>
            <ShadcnAvatarFallback>
                <svg viewBox="0 0 24 24" aria-hidden="true"><circle cx="12" cy="8" r="3.25" fill="currentColor" /><path d="M5.5 20c.7-4 2.9-6 6.5-6s5.8 2 6.5 6" fill="currentColor" /></svg>
            </ShadcnAvatarFallback>
        </ShadcnAvatar>
        <span><strong>Team</strong><span>Fallback placeholder</span></span>
    </div>
</div>
<ShadcnAvatarGroup>
    <ShadcnAvatar><ShadcnAvatarImage Source="images/avatars/operator-thai.png" Alt="Thai CNC operator" /><ShadcnAvatarFallback>NT</ShadcnAvatarFallback></ShadcnAvatar>
    <ShadcnAvatar><ShadcnAvatarImage Source="images/avatars/reviewer-thai.png" Alt="Thai quality reviewer" /><ShadcnAvatarFallback>QR</ShadcnAvatarFallback></ShadcnAvatar>
    <ShadcnAvatar><ShadcnAvatarImage Source="images/avatars/assistant-thai.png" Alt="Thai support assistant" /><ShadcnAvatarFallback>SA</ShadcnAvatarFallback></ShadcnAvatar>
    <ShadcnAvatarGroupCount>+1</ShadcnAvatarGroupCount>
</ShadcnAvatarGroup>
""";
        return Example("avatar", "Avatar gallery", "Compare distinct Thai team portraits, a useful fallback placeholder, presence badge, and stacked group composition in one responsive example.", source, preview, [EnumSelect("avatar-size", "Size", size, v => size = v), Toggle("avatar-failed", "Failed image", v => failed = v), Toggle("avatar-badge", "Online badge", v => badge = v), Toggle("avatar-group", "Group", v => group = v)], ["gallery", "sm", "default", "lg", "fallback", "badge", "group", "distinct-images", "responsive"]);
    }
    private static ComponentExampleDefinition Badge()
    {
        var variant = ShadcnBadgeVariant.Default; var link = false; var invalid = false;
        RenderFragment preview = b =>
        {
            b.OpenElement(0, "div"); b.AddAttribute(1, "class", "showcase-badge-demo");
            b.OpenElement(2, "div"); b.AddAttribute(3, "class", "showcase-badge-demo__selected");
            b.OpenComponent<ShadcnBadge>(4); b.AddAttribute(5, "Variant", variant); b.AddAttribute(6, "Href", link ? "/docs/components/badge" : null); b.AddAttribute(7, "ChildContent", Text("Ready")); if (invalid) b.AddAttribute(8, "AdditionalAttributes", new Dictionary<string, object> { ["aria-invalid"] = "true" }); b.CloseComponent();
            b.CloseElement();
            b.OpenElement(10, "div"); b.AddAttribute(11, "class", "showcase-badge-demo__gallery");
            AddBadge(b, 20, ShadcnBadgeVariant.Default, "Default"); AddBadge(b, 30, ShadcnBadgeVariant.Secondary, "Secondary"); AddBadge(b, 40, ShadcnBadgeVariant.Destructive, "Destructive"); AddBadge(b, 50, ShadcnBadgeVariant.Outline, "Outline");
            b.CloseElement(); b.CloseElement();
        };
        const string source = """
<div class="showcase-badge-demo">
    <ShadcnBadge Variant="ShadcnBadgeVariant.Default">Default</ShadcnBadge>
    <ShadcnBadge Variant="ShadcnBadgeVariant.Secondary">Secondary</ShadcnBadge>
    <ShadcnBadge Variant="ShadcnBadgeVariant.Destructive">Destructive</ShadcnBadge>
    <ShadcnBadge Variant="ShadcnBadgeVariant.Outline" Href="/docs/components/badge">Outline link</ShadcnBadge>
</div>
""";
        return Example("badge", "Status badge", "Compare every semantic badge treatment at once, including a link and destructive state.", source, preview, [EnumSelect("badge-variant", "Variant", variant, v => variant = v), Toggle("badge-link", "Link", v => link = v), Toggle("badge-invalid", "Invalid", v => invalid = v)], ["variants", "inline", "link", "focus"]);
    }
    private static ComponentExampleDefinition Card()
    {
        var size = ShadcnCardSize.Default; var compactSpacing = false; var action = true;
        RenderFragment preview = b => { b.OpenComponent<CardDossierPreview>(0); b.AddAttribute(1, "Size", size); b.AddAttribute(2, "CompactSpacing", compactSpacing); b.AddAttribute(3, "Action", action); b.CloseComponent(); };
        const string source = """
<ShadcnCard Size="ShadcnCardSize.Default">
    <ShadcnCardHeader>
        <ShadcnCardTitle>Production order #4189</ShadcnCardTitle>
        <ShadcnCardDescription>Revision C · CNC enclosure</ShadcnCardDescription>
        <ShadcnCardAction><button type="button">Open</button></ShadcnCardAction>
    </ShadcnCardHeader>
    <ShadcnCardContent>Running · 3 files ready for inspection.</ShadcnCardContent>
    <ShadcnCardFooter>Updated 2 minutes ago</ShadcnCardFooter>
</ShadcnCard>
""";
        return Example("card", "Composed card", "Show a production-order card with title, description, action, content, and footer hierarchy.", source, preview, [EnumSelect("card-size", "Size", size, v => size = v), Toggle("card-spacing", "Compact spacing", v => compactSpacing = v), Toggle("card-action", "Action", v => action = v, true)], ["default", "sm", "header", "action", "content", "footer"]);
    }
    private static ComponentExampleDefinition Carousel()
    {
        var vertical = false; var loop = false; var rtl = false; var reduced = false;
        RenderFragment preview = b =>
        {
            b.OpenComponent<CarouselDossierPreview>(0);
            b.AddAttribute(1, "Vertical", vertical);
            b.AddAttribute(2, "Loop", loop);
            b.AddAttribute(3, "Rtl", rtl);
            b.AddAttribute(4, "ReducedMotion", reduced);
            b.CloseComponent();
        };
        const string source = """
@using Maliev.ShadcnBlazor.Components.Content

<ShadcnCarousel Label="Production queue"
                Orientation="@(vertical ? ShadcnCarouselOrientation.Vertical : ShadcnCarouselOrientation.Horizontal)"
                @bind-SelectedIndex="selectedIndex"
                Options="@Options">
    <ShadcnCarouselContent>
        <ShadcnCarouselItem Index="0" Label="Operations · Laser cell · 98%">
            <article class="queue-slide">Operations · Laser cell · 98% · On schedule</article>
        </ShadcnCarouselItem>
        <ShadcnCarouselItem Index="1" Label="Quality · First-pass yield · 96.2%">
            <article class="queue-slide">Quality · First-pass yield · 96.2% · Target met</article>
        </ShadcnCarouselItem>
        <ShadcnCarouselItem Index="2" Label="Delivery · Orders shipped · 1,284">
            <article class="queue-slide">Delivery · Orders shipped · 1,284 · This month</article>
        </ShadcnCarouselItem>
    </ShadcnCarouselContent>
    <ShadcnCarouselPrevious />
    <ShadcnCarouselNext />
</ShadcnCarousel>

<p aria-live="polite">Slide @(selectedIndex + 1) of 3</p>
<nav aria-label="Choose production queue slide">
    @for (var index = 0; index < 3; index++)
    {
        var slideIndex = index;
        <button type="button"
                aria-label="Show slide @(slideIndex + 1)"
                aria-pressed="@(selectedIndex == slideIndex)"
                @onclick="() => selectedIndex = slideIndex">
            @(slideIndex + 1)
        </button>
    }
</nav>

@code {
    private bool vertical;
    private bool loop;
    private bool rtl;
    private bool reducedMotion;
    private int selectedIndex;
    private ShadcnCarouselOptions Options => new()
    {
        Loop = loop,
        RightToLeft = rtl,
        ReducedMotion = reducedMotion
    };
}
""";
        return Example("carousel", "Carousel engine", "Move through a production queue with arrows, keyboard shortcuts, pointer gestures, slide status, and motion preferences.", source, preview, [Toggle("carousel-vertical", "Vertical", v => vertical = v), Toggle("carousel-loop", "Loop", v => loop = v), Toggle("carousel-rtl", "RTL", v => rtl = v), Toggle("carousel-reduced", "Reduced motion", v => reduced = v)], ["horizontal", "vertical", "loop", "keyboard", "pointer", "status", "dots", "rtl", "reduced-motion"]);
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
        RenderFragment preview = b => { b.OpenComponent<SkeletonDossierPreview>(0); b.AddAttribute(1, "Circle", circle); b.AddAttribute(2, "Motion", motion); b.CloseComponent(); };
        const string source = """
<section class="showcase-skeleton-layout" aria-label="Loading production overview">
    <div class="showcase-skeleton-layout__header">
        <ShadcnSkeleton Shape="ShadcnSkeletonShape.Circle" Animation="ShadcnSkeletonAnimation.Pulse" Style="width:2.75rem;height:2.75rem" />
        <div><ShadcnSkeleton Animation="ShadcnSkeletonAnimation.Pulse" Style="width:12rem;height:1rem" /><ShadcnSkeleton Animation="ShadcnSkeletonAnimation.Pulse" Style="width:8rem;height:.75rem" /></div>
    </div>
    <div class="showcase-skeleton-layout__body"><ShadcnSkeleton Animation="ShadcnSkeletonAnimation.Pulse" Style="width:68%;height:1rem" /><ShadcnSkeleton Animation="ShadcnSkeletonAnimation.Pulse" Style="width:92%;height:1rem" /><ShadcnSkeleton Animation="ShadcnSkeletonAnimation.Pulse" Style="width:48%;height:1rem" /></div>
    <div class="showcase-skeleton-layout__cards"><article><ShadcnSkeleton Animation="ShadcnSkeletonAnimation.Pulse" Style="width:100%;height:4.5rem" /><ShadcnSkeleton Animation="ShadcnSkeletonAnimation.Pulse" Style="width:70%;height:.75rem" /></article><article><ShadcnSkeleton Animation="ShadcnSkeletonAnimation.Pulse" Style="width:100%;height:4.5rem" /><ShadcnSkeleton Animation="ShadcnSkeletonAnimation.Pulse" Style="width:70%;height:.75rem" /></article><article><ShadcnSkeleton Animation="ShadcnSkeletonAnimation.Pulse" Style="width:100%;height:4.5rem" /><ShadcnSkeleton Animation="ShadcnSkeletonAnimation.Pulse" Style="width:70%;height:.75rem" /></article></div>
</section>
""";
        return Example("skeleton", "Skeleton", "Preview a realistic page-loading layout with shared geometry and respectful motion.", source, preview, [Toggle("skeleton-circle", "Avatar circle", v => circle = v), Toggle("skeleton-motion", "Pulse", v => motion = v, true)], ["layout", "circle", "pulse", "reduced-motion"]);
    }
    private static ComponentExampleDefinition Spinner()
    {
        var decorative = false; var large = false;
        RenderFragment preview = b =>
        {
            b.OpenElement(0, "div"); b.AddAttribute(1, "class", "showcase-spinner-task");
            b.OpenComponent<ShadcnSpinner>(2); b.AddAttribute(3, "Label", decorative ? null : "กำลังประมวลผลการชำระเงิน"); b.AddAttribute(4, "Size", large ? "1.5rem" : "1rem"); b.CloseComponent();
            b.OpenElement(5, "span"); b.AddContent(6, "Processing payment…"); b.CloseElement(); b.OpenElement(7, "strong"); b.AddContent(8, "฿100.00"); b.CloseElement(); b.CloseElement();
        };
        const string source = """
<div class="payment-status">
    <ShadcnSpinner Label="กำลังประมวลผลการชำระเงิน" />
    <span>Processing payment…</span>
    <strong>฿100.00</strong>
</div>
""";
        return Example("spinner", "Spinner", "Show loading context with an announced spinner, decorative option, and payment amount.", source, preview, [Toggle("spinner-decorative", "Decorative", v => decorative = v), Toggle("spinner-large", "Large", v => large = v)], ["status", "decorative", "size", "reduced-motion"]);
    }
    private static ComponentExampleDefinition Toast()
    {
        var limit = 3d; var start = false; var reduced = false; var type = ShadcnToastType.Success; var priority = ShadcnToastPriority.Normal;
        RenderFragment preview = b => { b.OpenComponent<ToastDossierPreview>(0); b.AddAttribute(1, "MaximumVisible", (int)limit); b.AddAttribute(2, "Placement", start ? ShadcnToastPlacement.BottomStart : ShadcnToastPlacement.BottomEnd); b.AddAttribute(3, "ReducedMotion", reduced); b.AddAttribute(4, "Type", type); b.AddAttribute(5, "Priority", priority); b.CloseComponent(); };
        return Example("toast", "Toast viewport", "Trigger the real page-level toast queue; notifications stack outside this preview and remain keyboard reachable.", "@inject IShadcnToastService Toasts\n<ShadcnButton OnClick=\"Show\">Show localized toast</ShadcnButton>\n<ShadcnToaster />", preview, [Number("toast-limit", "Visible limit", limit, v => limit = Math.Max(1, v)), Toggle("toast-start", "Logical start", v => start = v), Toggle("toast-reduced", "Reduced motion", v => reduced = v), EnumSelect("toast-type", "Type", type, v => type = v), EnumSelect("toast-priority", "Priority", priority, v => priority = v)], ["queue", "limit", "promise", "pause", "swipe", "F6", "rtl", "reduced-motion", "priority"]);
    }

    private static ComponentExampleDefinition Example(string slug, string title, string description, string source, RenderFragment preview, IReadOnlyList<ComponentParameterControl> controls, IReadOnlyList<string> tags) => new($"{slug}-primary", title, description, source, preview, controls, tags);
    private static ComponentParameterControl Toggle(string id, string label, Action<bool> apply, bool initial = false) => new(id, label, ComponentParameterControlKind.Toggle, initial.ToString(), [], value => apply(bool.Parse(value)));
    private static ComponentParameterControl Number(string id, string label, double value, Action<double> apply) => new(id, label, ComponentParameterControlKind.Number, value.ToString(System.Globalization.CultureInfo.InvariantCulture), [], text => apply(double.Parse(text, System.Globalization.CultureInfo.InvariantCulture)));
    private static ComponentParameterControl EnumSelect<T>(string id, string label, T value, Action<T> apply) where T : struct, Enum => new(id, label, ComponentParameterControlKind.Select, value.ToString(), Enum.GetNames<T>(), text => apply(Enum.Parse<T>(text)));
    private static RenderFragment Text(string value) => b => b.AddContent(0, value);
    private static void Add<T>(RenderTreeBuilder b, int sequence, string text) where T : IComponent { b.OpenComponent<T>(sequence); b.AddAttribute(sequence + 1, "ChildContent", Text(text)); b.CloseComponent(); }
    private static void AddBadge(RenderTreeBuilder b, int sequence, ShadcnBadgeVariant variant, string label) { b.OpenComponent<ShadcnBadge>(sequence); b.AddAttribute(sequence + 1, "Variant", variant); b.AddAttribute(sequence + 2, "ChildContent", Text(label)); b.CloseComponent(); }
}
